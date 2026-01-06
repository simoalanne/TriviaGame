using System.Data;
using System.Text.Json;
using Dapper;
using Microsoft.AspNetCore.SignalR;
using Shared;

namespace TriviaGame;

using TriviaItemDomain = TriviaItem<QuestionDomain>;
using TriviaItemDto = TriviaItem<QuestionDto>;
using TriviaItemForClient = TriviaItem<QuestionToClient>;

internal record CreateOrJoinGameRequest(string PlayerName, string? GameId = null);

internal record AdvanceGameRequest(string PlayerId, string Answer, int QuestionIndex);

public abstract record QuestionDomain(
    string QuestionText,
    string? Explanation
)
{
    public string? PlayerAnswer { get; set; }
    protected abstract string AnswerAsString();
    public abstract bool CheckAnswer(string answer);

    public QuestionToClient ToClient()
    {
        var playerAnswer = string.IsNullOrEmpty(PlayerAnswer) ? null : PlayerAnswer;
        return new QuestionToClient(
            QuestionText,
            Explanation: string.IsNullOrEmpty(Explanation) ? null : Explanation,
            CorrectAnswer: string.IsNullOrEmpty(playerAnswer) ? null : AnswerAsString(),
            PlayerAnswer: playerAnswer,
            PlayerAnswerCorrect: playerAnswer == null ? null : CheckAnswer(playerAnswer)
        );
    }
}

public record MultipleChoiceQuestion(
    string QuestionText,
    string? Explanation,
    string CorrectAnswer
) : QuestionDomain(QuestionText, Explanation)
{
    public override bool CheckAnswer(string answer) => answer == CorrectAnswer;
    protected override string AnswerAsString() => CorrectAnswer;
}

public record TrueOrFalseQuestion(
    string QuestionText,
    string? Explanation,
    bool CorrectAnswer
) : QuestionDomain(QuestionText, Explanation)
{
    public override bool CheckAnswer(string answer) => bool.TryParse(answer, out var b) && b == CorrectAnswer;
    protected override string AnswerAsString() => CorrectAnswer.ToString();
}

public record FillInTheBlankQuestion(
    string QuestionText,
    string? Explanation,
    List<string> CorrectAnswer
) : QuestionDomain(QuestionText, Explanation)
{
    private static string NormalizeAnswer(string answer) => answer.Trim().ToLowerInvariant();

    public override bool CheckAnswer(string answer) =>
        CorrectAnswer.Select(NormalizeAnswer).ToList().Contains(NormalizeAnswer(answer));

    protected override string AnswerAsString() => string.Join(", ", CorrectAnswer);
}

public record OrderingQuestion(
    string QuestionText,
    string? Explanation,
    int CorrectOrder
) : QuestionDomain(QuestionText, Explanation)
{
    public override bool CheckAnswer(string answer) => int.TryParse(answer, out var i) && i == CorrectOrder;
    protected override string AnswerAsString() => CorrectOrder.ToString();
}

public record PlayerData(string Name, int Score = 0, bool IsHost = false, bool HasReadiedUp = false)
{
    public string Id { get; init; } = Utils.NewGuid();
}

public record QuestionToClient(
    string QuestionText,
    string? Explanation,
    string? CorrectAnswer = null,
    string? PlayerAnswer = null,
    bool? PlayerAnswerCorrect = null
);

public class GameSession
{
    public TriviaItemDomain? CurrentTriviaItem;
    public readonly List<PlayerData> Players = [];
    public List<string> CompletedTriviaItemIds = [];
    public int PlayerTurnIndex;
    public bool GameStarted;

    public string AddPlayer(string playerName, bool isHost = false)
    {
        var newPlayer = new PlayerData(playerName, IsHost: isHost);
        Players.Add(newPlayer);
        return newPlayer.Id;
    }

    public void DrawNewTriviaItem(IDbConnection db)
    {
        var json = db.QuerySingleOrDefault<string>("""
                                                   SELECT Data
                                                   FROM TriviaItems
                                                   WHERE NOT Data->>'Id' = ANY(@CompletedIds)
                                                   ORDER BY RANDOM()
                                                   LIMIT 1
                                                   """,
            new { CompletedIds = CompletedTriviaItemIds }
        );

        var triviaItemDto = json is null
            ? null
            : JsonSerializer.Deserialize<TriviaItemDto>(json);

        if (triviaItemDto is null)
        {
            throw new InvalidOperationException("No trivia items available in the database.");
        }

        var triviaItemDomain = triviaItemDto.Map();
        CurrentTriviaItem = triviaItemDomain;
    }
}

public class GameStateResponse
{
    public Dictionary<string, int> PlayerScores { get; set; } = new();
    public string CurrentPlayerName { get; set; } = string.Empty;
    public TriviaItemForClient? CurrentTriviaItem { get; set; }

    public bool GameStarted => CurrentTriviaItem is not null;
}

public static class TriviaItemMapper
{
    public static TriviaItemDomain Map(this TriviaItemDto dto)
    {
        var questions = dto.Content.Questions.Select<QuestionDto, QuestionDomain>(qDto =>
        {
            return dto.QuestionType switch
            {
                QuestionType.MultipleChoice => new MultipleChoiceQuestion(
                    qDto.QuestionText,
                    qDto.Explanation,
                    qDto.MultipleChoiceAnswer!
                ),
                QuestionType.TrueOrFalse => new TrueOrFalseQuestion(
                    qDto.QuestionText,
                    qDto.Explanation,
                    qDto.TrueOrFalseAnswer!.Value
                ),
                QuestionType.FillInTheBlank => new FillInTheBlankQuestion(
                    qDto.QuestionText,
                    qDto.Explanation,
                    qDto.FillInTheBlankAnswer!
                ),
                QuestionType.Ordering => new OrderingQuestion(
                    qDto.QuestionText,
                    qDto.Explanation,
                    qDto.OrderingAnswer!.Value
                ),
                _ => throw new NotSupportedException($"Unsupported question type: {dto.QuestionType}")
            };
        }).Shuffle().ToList();

        var content = new ItemContent<QuestionDomain>(questions, dto.Content.CorrectAnswers);
        return new TriviaItemDomain(dto.Id, dto.Prompt, content, dto.QuestionType);
    }
}

public static class GameEndpoints
{
    public static void MapGameEndpoints(this WebApplication app)
    {
        app.MapPost("/create-game", (CreateOrJoinGameRequest request, GameStore gameStore) =>
        {
            var info = gameStore.Create(request.PlayerName);
            return Results.Ok(new { info.PlayerId, info.GameId });
        });

        app.MapGet("/active-games", (GameStore gs) => Results.Ok(gs.GetIds()));

        app.MapPost("/join-game", (CreateOrJoinGameRequest request, GameStore gameStore) =>
        {
            var info = gameStore.TryJoin(request.GameId ?? string.Empty, request.PlayerName);
            return info is null
                ? Results.NotFound("Game not found.")
                : Results.Ok(new { info.PlayerId, info.GameId });
        });
    }
}

public sealed class GameHub(GameStore store) : Hub
{
    // First message from the client should be to join the game
    public async Task JoinGame(string gameId, string playerId)
    {
        if (!store.TryGet(gameId, out var game))
            throw new HubException("Game not found");

        if (game.Players.All(p => p.Id != playerId))
            throw new HubException("Player not found in this game");

        await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
        store.MapConnectionToGame(Context.ConnectionId, gameId);

        // Once player has joined, others should instantly be notified of the new player joining
        await Clients.Caller.SendAsync(
            "GameStateUpdated",
            BuildGameState(game)
        );
    }

    public async Task StartGame(string gameId, string playerId, IDbConnection db)
    {
        if (!store.TryGet(gameId, out var game))
            throw new HubException("Game not found");

        if (game.GameStarted)
            throw new HubException("Game has already started");

        // mark the player as readied up
        var index = game.Players.FindIndex(p => p.Id == playerId);
        if (index is -1)
            throw new HubException("Player not found in this game");

        game.Players[index] = game.Players[index] with { HasReadiedUp = true };

        // if all have readied up, start the game
        if (game.Players.All(p => p.HasReadiedUp))
        {
            game.GameStarted = true;
            game.DrawNewTriviaItem(db);
        }

        await Clients.Group(gameId).SendAsync(
            "GameStateUpdated",
            BuildGameState(game)
        );
    }

    public async Task SubmitAnswer(string gameId, string playerId, int questionIndex, string answer,
        IDbConnection db)
    {
        if (!store.TryGet(gameId, out var game))
            throw new HubException("Game not found");

        if (!game.GameStarted)
            throw new HubException("Game has not started yet");

        var currentPlayer = game.Players[game.PlayerTurnIndex];
        if (currentPlayer.Id != playerId)
            throw new HubException("It's not this player's turn");

        var questions = game.CurrentTriviaItem!.Content.Questions;
        if (questionIndex < 0 || questionIndex >= questions.Count)
            throw new HubException("Invalid question index");

        if (string.IsNullOrEmpty(answer))
            throw new HubException("Answer cannot be empty");


        var currentQuestion = questions[questionIndex];
        if (currentQuestion.PlayerAnswer is not null)
            throw new HubException("Question has already been answered");

        currentQuestion.PlayerAnswer = answer;

        var isCorrect = currentQuestion.CheckAnswer(answer);
        if (isCorrect)
        {
            currentPlayer = currentPlayer with { Score = currentPlayer.Score + 1 };
            game.Players[game.PlayerTurnIndex] = currentPlayer;
        }

        game.PlayerTurnIndex = (game.PlayerTurnIndex + 1) % game.Players.Count;

        var allAnswered = game.CurrentTriviaItem.Content.Questions.All(q => q.PlayerAnswer is not null);
        if (allAnswered)
        {
            game.CompletedTriviaItemIds.Add(game.CurrentTriviaItem.Id!);
            game.DrawNewTriviaItem(db);
        }

        await Clients.Group(gameId).SendAsync(
            "GameStateUpdated",
            BuildGameState(game)
        );
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var disconnectedId = Context.ConnectionId;
        // For now use a radical approach: if someone disconnects, end the game
        // In the future the focus should be on connectionId to playerId mapping so features like reconnection,
        // not allowing player to create multiple connections, etc. can be implemented.
        // stable playerId should be the single source of truth for identifying what the player's state is on the server.
        store.RemoveGameByConnectionId(disconnectedId);
        await base.OnDisconnectedAsync(exception);
    }

    private static GameStateResponse BuildGameState(GameSession game)
    {
        return new GameStateResponse
        {
            PlayerScores = game.Players.ToDictionary(p => p.Name, p => p.Score),
            CurrentPlayerName = game.Players[game.PlayerTurnIndex].Name,
            CurrentTriviaItem = game.CurrentTriviaItem is null
                ? null
                : new TriviaItemForClient(
                    Id: game.CurrentTriviaItem.Id,
                    Prompt: game.CurrentTriviaItem.Prompt,
                    QuestionType: game.CurrentTriviaItem.QuestionType,
                    Content: new ItemContent<QuestionToClient>(
                        Questions: game.CurrentTriviaItem.Content.Questions.Select(q => q.ToClient()).ToList(),
                        CorrectAnswers: game.CurrentTriviaItem.Content.CorrectAnswers
                    ))
        };
    }
}
