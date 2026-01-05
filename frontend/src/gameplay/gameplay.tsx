import { useState } from "react";
import * as signalR from "@microsoft/signalr";
import axios from "axios";

// --- Types ---
export interface QuestionToClient {
  question: string;
  options: string[];
  playerAnswer?: string;
}

export interface ItemContent<T> {
  Questions: T[];
}

export interface TriviaItemForClient {
  Id: string;
  Prompt: string;
  QuestionType: string;
  Content: ItemContent<QuestionToClient>;
}

export interface GameStateResponse {
  PlayerScores: Record<string, number>;
  CurrentTriviaItem: TriviaItemForClient | null;
}

interface CreateJoinRequest {
  playerName: string;
  gameId?: string;
}

// --- Component ---
export default function TriviaTest() {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
  const [playerName, setPlayerName] = useState<string>("");
  const [gameId, setGameId] = useState<string | null>(null);
  const [playerId, setPlayerId] = useState<string | null>(null);
  const [gameState, setGameState] = useState<GameStateResponse | null>(null);

  // New state for submitting answers
  const [questionIndex, setQuestionIndex] = useState<number>(0);
  const [answer, setAnswer] = useState<string>("");

  const setupConnection = async (gid: string, pid: string): Promise<void> => {
    const conn = new signalR.HubConnectionBuilder()
      .withUrl("/gamehub")
      .withAutomaticReconnect()
      .build();

    conn.on("GameStateUpdated", (state: GameStateResponse) => {
      setGameState(state);
    });

    await conn.start();
    console.log("Connected to SignalR");

    await conn.invoke("JoinGame", gid, pid);
    setConnection(conn);
  };

  const handleCreateGame = async (): Promise<void> => {
    const res = await axios.post<{ playerId: string; gameId: string }>("/create-game", { playerName } as CreateJoinRequest);
    setPlayerId(res.data.playerId);
    setGameId(res.data.gameId);
    await setupConnection(res.data.gameId, res.data.playerId);
  };

  const handleStartGame = async (): Promise<void> => {
    if (!connection || !gameId || !playerId) return;
    await connection.invoke("StartGame", gameId, playerId);
  };

  const handleSubmitAnswer = async (): Promise<void> => {
    if (!connection || !gameId || !playerId) return;
    await connection.invoke("SubmitAnswer", gameId, playerId, questionIndex, answer);
    setAnswer(""); // optional: clear input after submit
  };

  return (
    <div>
      <h1>Trivia Test</h1>

      <div>
        <input
          placeholder="Player name"
          value={playerName}
          onChange={(e) => setPlayerName(e.target.value)}
        />
        <button onClick={handleCreateGame}>Create Game</button>
      </div>

      <div>
        <p>Game ID: {gameId || "No game created"}</p>
        <button onClick={handleStartGame}>Start Game</button>
      </div>

      <div>
        <h3>Submit Answer</h3>
        <input
          type="number"
          placeholder="Question index"
          value={questionIndex}
          onChange={(e) => setQuestionIndex(Number(e.target.value))}
        />
        <input
          placeholder="Answer"
          value={answer}
          onChange={(e) => setAnswer(e.target.value)}
        />
        <button onClick={handleSubmitAnswer}>Submit Answer</button>
      </div>

      <div>
        <h3>Game State:</h3>
        <pre>{gameState ? JSON.stringify(gameState, null, 2) : "No state yet"}</pre>
      </div>
    </div>
  );
}
