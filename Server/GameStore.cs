using System.Collections.Concurrent;

namespace TriviaGame;

// Client should make sure to store these IDs to connect to the game later and to identify themselves
public record PlayerConnectionInformation(string GameId, string PlayerId);

public sealed class GameStore
{
    private readonly ConcurrentDictionary<string, GameSession> _games = new();

    public PlayerConnectionInformation Create(string playerName)
    {
        var gameId = Utils.NewGuid();
        var game = new GameSession();
        var playerId = game.AddPlayer(playerName, isHost: true);
        _games[gameId] = game;
        return new PlayerConnectionInformation(gameId, playerId);
    }

    public PlayerConnectionInformation? TryJoin(string gameId, string playerName)
    {
        if (!_games.TryGetValue(gameId, out var game)) return null;
        var playerId = game.AddPlayer(playerName);
        return new PlayerConnectionInformation(gameId, playerId);
    }

    public bool TryGet(string gameId, out GameSession game) =>
        _games.TryGetValue(gameId, out game!);

    public IReadOnlyCollection<string> GetIds() =>
        _games.Keys.ToList().AsReadOnly();
}
