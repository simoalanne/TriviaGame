import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Button } from "../../ui/Button";
import { TextInput } from "../../ui/TextInput";
import { Select } from "../../ui/Select";
import { useGameContext } from "../../providers/GameProvider";
import { getActiveGames, joinGame } from "../../api";

export function JoinGameScreen() {
  const navigate = useNavigate();
  const { setPlayerId, setGameId, setUsername } = useGameContext();

  const [games, setGames] = useState<string[]>([]);
  const [selectedGame, setSelectedGame] = useState<string | null>(null);
  const [username, setName] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadGames = async () => {
    try {
      const res = await getActiveGames();
      setGames(res);
    } catch {
      setError("Failed to load active games");
    }
  };

  useEffect(() => {
    loadGames();
  }, []);

  const handleJoin = async () => {
    if (!selectedGame || !username.trim()) return;

    setLoading(true);
    setError(null);

    try {
      const res = await joinGame(username.trim(), selectedGame);
      setPlayerId(res.playerId);
      setGameId(res.gameId);
      setUsername(username.trim());
      navigate("/gameplay");
    } catch {
      setError("Failed to join game");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div
      style={{
        height: "100vh",
        display: "flex",
        flexDirection: "column",
        justifyContent: "center",
        alignItems: "center",
        gap: 24,
        padding: 32,
      }}
    >
      <h2 style={{ fontSize: 32, margin: 0 }}>Join Game</h2>

      {/* Game selection */}
      <div style={{ display: "flex", gap: 12, alignItems: "center" }}>
        <Select
          value={selectedGame}
          onChange={setSelectedGame}
          options={games.map((g) => ({ label: g, value: g }))}
          disabled={games.length === 0}
        />
        <Button label="↻" onClick={loadGames} />
      </div>

      {/* Username */}
      <TextInput
        value={username}
        onChange={setName}
        placeholder="Enter your name"
      />

      {error && <div style={{ color: "red" }}>{error}</div>}

      <Button
        label={loading ? "Joining..." : "Join Game"}
        onClick={handleJoin}
        disabled={loading || !selectedGame || !username.trim()}
      />
    </div>
  );
}
