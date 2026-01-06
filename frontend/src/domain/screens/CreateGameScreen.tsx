import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { TextInput } from "../../ui/TextInput";
import { Button } from "../../ui/Button";
import { useGameContext } from "../../providers/GameProvider";
import { createGame } from "../../api";

export function CreateGameScreen() {
  const navigate = useNavigate();
  const { setPlayerId, setGameId, setUsername } = useGameContext();

  const [name, setName] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleCreateGame = async () => {
    if (!name.trim()) {
      setError("Please enter a name");
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const res = await createGame(name.trim());
      setPlayerId(res.playerId);
      setGameId(res.gameId);
      setUsername(name.trim());

      navigate("/gameplay");
    } catch (err) {
      console.error(err);
      setError("Failed to create game. Please try again.");
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
      <h2 style={{ fontSize: 32, margin: 0 }}>Create Game</h2>

      <TextInput
        value={name}
        onChange={setName}
        placeholder="Enter your name"
      />

      {error && <div style={{ color: "red" }}>{error}</div>}

      <Button
        label={loading ? "Creating..." : "Create Game"}
        onClick={handleCreateGame}
        disabled={loading}
      />
    </div>
  );
}
