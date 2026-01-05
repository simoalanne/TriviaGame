using System.Data;
using System.Text.Json;
using Dapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace TriviaGame;

using TriviaItemDto = TriviaItem<QuestionDto>;
using Validator = IValidator<TriviaItem<QuestionDto>>;

public static class CrudEndpoints
{
    private record QuestionTypeFilter(
        bool? IncludeMultipleChoice,
        bool? IncludeTrueOrFalse,
        bool? IncludeFillInTheBlank,
        bool? IncludeOrdering);

    private static string GetWhereClause(QuestionTypeFilter filter)
    {
        var allTypes = new List<(bool? Include, int EnumValue)>
        {
            (filter.IncludeMultipleChoice, (int)QuestionType.MultipleChoice),
            (filter.IncludeTrueOrFalse, (int)QuestionType.TrueOrFalse),
            (filter.IncludeFillInTheBlank, (int)QuestionType.FillInTheBlank),
            (filter.IncludeOrdering, (int)QuestionType.Ordering)
        };

        var includeTypes = allTypes.Where(t => t.Include == true).Select(t => t.EnumValue.ToString()).ToList();
        var excludeTypes = allTypes.Where(t => t.Include == false).Select(t => t.EnumValue.ToString()).ToList();

        var conditions = new List<string>();

        if (includeTypes.Count > 0)
            conditions.Add($"(Data->>'QuestionType')::int IN ({string.Join(", ", includeTypes)})");

        if (excludeTypes.Count > 0)
            conditions.Add($"(Data->>'QuestionType')::int NOT IN ({string.Join(", ", excludeTypes)})");

        return conditions.Count > 0
            ? "WHERE " + string.Join(" AND ", conditions)
            : string.Empty;
    }


    public static void MapCrudEndpoints(this WebApplication app)
    {
        // GET all trivia items
        app.MapGet("/trivia",
            async (
                // This should maybe be changes to be [from query] but that seemed too complex because it requires
                // a bunch of boilerplate code in minimal APIs to work.
                bool? includeMultipleChoice,
                bool? includeTrueOrFalse,
                bool? includeFillInTheBlank,
                bool? includeOrdering,
                int? page,
                int? pageSize,
                IDbConnection db
            ) =>
            {
                var filter = new QuestionTypeFilter(
                    includeMultipleChoice,
                    includeTrueOrFalse,
                    includeFillInTheBlank,
                    includeOrdering
                );
                
                page ??= 1;
                pageSize ??= 20;

                var whereClause = GetWhereClause(filter);
                var sql = $"""
                               SELECT COALESCE(jsonb_agg(Data)::text, '[]')
                               FROM (
                                   SELECT Data
                                   FROM TriviaItems
                                   {whereClause}
                                   ORDER BY CreatedAt DESC
                                   LIMIT @PageSize OFFSET @Offset
                               )
                           """;
                var json = await db.QuerySingleAsync<string>(sql, new { PageSize = pageSize, Offset = (page - 1) * pageSize });
                var items = JsonSerializer.Deserialize<List<TriviaItemDto>>(json);
                return Results.Ok(items);
            });



        // GET single trivia item by id
        app.MapGet("/trivia/{id}", async (string id, IDbConnection db) =>
        {
            var json = await db.QuerySingleOrDefaultAsync<string>(
                "SELECT Data::text FROM TriviaItems WHERE Data->>'Id' = @Id",
                new { Id = id }
            );

            var item = json is null
                ? null
                : JsonSerializer.Deserialize<TriviaItemDto>(json);

            return json is null
                ? Results.NotFound()
                : Results.Ok(item);
        });

        // CREATE a new trivia item
        app.MapPost("/trivia", async (
            TriviaItemDto item,
            IDbConnection db,
            Validator validator) =>
        {
            var insertResult = await InsertTriviaItemsToDb([item], db, validator);
            if (insertResult.Errors is not null) return Results.BadRequest(insertResult.Errors);
            var createdId = insertResult.InsertedIds!.First();
            return Results.Created($"/trivia/{createdId}", createdId);
        });

        // CREATE many trivia items
        app.MapPost("/trivia/batch", async (
            List<TriviaItemDto> items,
            IDbConnection db,
            Validator validator) =>
        {
            if (items.Count == 0) return Results.BadRequest("No trivia items provided.");
            var insertResult = await InsertTriviaItemsToDb(items, db, validator);
            return insertResult.Errors is not null
                ? Results.BadRequest(insertResult.Errors)
                : Results.Created("/trivia/batch", insertResult.InsertedIds);
        });

        // UPDATE an existing trivia item
        app.MapPut("/trivia/", async (
            TriviaItemDto item,
            IDbConnection db,
            Validator validator) =>
        {
            var validation = await validator.ValidateAsync(item);
            if (!validation.IsValid)
                return Results.BadRequest(validation.Errors);

            var affected = await db.ExecuteAsync(
                "UPDATE TriviaItems SET Data = @Data WHERE Data->>'Id' = @Id",
                new { Data = Utils.ToJson(item), item.Id }
            );

            return affected == 0 ? Results.NotFound() : Results.NoContent();
        });


        // DELETE a single trivia item
        app.MapDelete("/trivia/{id}", async (string id, IDbConnection db) =>
            await DeleteTriviaItemsFromDb(db, [id]));

        // DELETE many trivia items
        app.MapDelete("/trivia/batch", async ([FromBody] List<string> ids, IDbConnection db) =>
        await DeleteTriviaItemsFromDb(db, ids));

        // DELETE all trivia items
        app.MapDelete("/trivia/all", async (IDbConnection db) =>
            await DeleteTriviaItemsFromDb(db));
    }

    private static async Task<IResult> DeleteTriviaItemsFromDb(
        IDbConnection db,
        IEnumerable<string>? ids = null)
    {
        var sql = ids is null
            ? "DELETE FROM TriviaItems"
            : "DELETE FROM TriviaItems WHERE Data->>'Id' = ANY(@Ids)";

        var affected = ids is null
            ? await db.ExecuteAsync(sql)
            : await db.ExecuteAsync(sql, new { Ids = ids });

        return affected > 0 ? Results.NoContent() : Results.NotFound();
    }

    private record InsertResults(List<string>? InsertedIds = null, List<ValidationFailure>? Errors = null);

    private record ValidationFailure(List<FluentValidation.Results.ValidationFailure> Errors, int? Index = null);

    private record ProcessResult(string? UsedId = null, string? Json = null, ValidationFailure? Error = null);

    private static async Task<InsertResults> InsertTriviaItemsToDb(
        IEnumerable<TriviaItemDto> items,
        IDbConnection db,
        Validator validator)
    {
        var processed = await Task.WhenAll(items.Select(async (item, idx) =>
        {
            var validation = await validator.ValidateAsync(item);
            var id = Utils.NewGuid();
            return validation.IsValid
                ? new ProcessResult(UsedId: id, Json: Utils.ToJson(item with { Id = id }))
                : new ProcessResult(Error: new ValidationFailure(validation.Errors, idx));
        }).Where(json => json is not null));

        var errors = processed
            .Where(r => r.Error is not null)
            .Select(r => r.Error!)
            .ToList();

        if (errors.Count > 0) return new InsertResults(Errors: errors);

        const string sql = "INSERT INTO TriviaItems (Data) VALUES (@Data::jsonb);";
        await db.ExecuteAsync(sql, processed.Select(r => new { Data = r.Json }));

        return new InsertResults(InsertedIds: processed.Select(r => r.UsedId!).ToList());
    }
}
