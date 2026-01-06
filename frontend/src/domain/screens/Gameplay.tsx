import { useEffect, useMemo } from "react";
import { useNavigate } from "react-router-dom";
import { useGameContext } from "../../providers/GameProvider";
import { useGameHub } from "../../hooks/useGameHub";
import { PlayersOverlay } from "../components/PlayersOverlay";
import { Button } from "../../ui/Button";
import { QuestionsGrid } from "../components/QuestionsGrid";

export function GameplayScreen() {
  const navigate = useNavigate();
  const { playerId, gameId, username } = useGameContext();

  const {
    connect,
    gameState,
    isConnected,
    hasReadiedUp,
    startGame,
    submitAnswer,
  } = useGameHub();

  useEffect(() => {
    if (!playerId || !gameId) {
      navigate("/");
      return;
    }

    connect(gameId, playerId);
  }, [playerId, gameId, connect, navigate]);

  const players = useMemo(() => {
    if (!gameState || !username) return [];

    return Object.entries(gameState.playerScores).map(([name, score]) => ({
      id: name,
      name,
      score,
      isThisClient: name === username,
      isThisPlayersTurn: name === gameState.currentPlayerName,
    }));
  }, [gameState, username]);

  const allowedToAnswer = useMemo(() => {
    return gameState?.currentPlayerName === username;
  }, [gameState, username]);

  const handleAnswerQuestion = (questionIndex: number, answer: string) => {
    if (!gameId || !playerId) return;
    submitAnswer(gameId, playerId, questionIndex, answer);
  };

  return (
    <div
      style={{
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        justifyContent: "center",
        width: "100vw",
        height: "100vh",
        padding: 16,
        boxSizing: "border-box",
        gap: 16,
      }}
    >
      <PlayersOverlay players={players} />
      {!isConnected && (
        <div style={{ fontSize: 18, opacity: 0.7 }}>Connecting…</div>
      )}

      {isConnected && !gameState?.gameStarted && (
        <Button
          label={hasReadiedUp ? "Waiting for others…" : "Ready up"}
          onClick={() => startGame(gameId!, playerId!)}
          disabled={hasReadiedUp}
        />
      )}

      {isConnected && gameState?.gameStarted && gameState.currentTriviaItem && (
        <div
          style={{
            display: "flex",
            flexDirection: "column",
            alignItems: "flex-start",
            width: "100%",
            maxWidth: 900, // optional max width for grid readability
            gap: 16,
          }}
        >
          <div style={{ fontWeight: 600, fontSize: 18 }}>
            {gameState.currentTriviaItem.prompt}
          </div>
          <QuestionsGrid
            triviaItem={gameState.currentTriviaItem}
            allowedToAnswer={allowedToAnswer}
            onAnswerQuestion={handleAnswerQuestion}
          />
        </div>
      )}
    </div>
  );
}
