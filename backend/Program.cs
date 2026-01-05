using Microsoft.AspNetCore.Http.Json;
using FluentValidation;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using TriviaGame;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>(optional: true);
var connectionString = builder.Configuration.GetConnectionString("Postgres");

// Configure JSON options: camelCase, enums as strings, don't serialize nulls
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});


// Register services
builder.Services.AddSignalR();
builder.Services.AddScoped<IDbConnection>(_ => new Npgsql.NpgsqlConnection(connectionString));
builder.Services.AddSingleton<IValidator<TriviaItem<QuestionDto>>, TriviaQuestionValidator>();
builder.Services.AddSingleton<GameStore>();

// Create tables if they don't exist
await DbSetup.EnsureTablesExistAsync(connectionString!);

var app = builder.Build();
app.UseWebSockets(); 

// Create endpoints
app.MapCrudEndpoints();
app.MapGameEndpoints();
app.MapHub<GameHub>("/gamehub");

app.UseStaticFiles(); // serves wwwroot by default

app.Run();
