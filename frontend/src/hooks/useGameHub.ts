import { useState, useRef, useCallback } from "react";
import * as signalR from "@microsoft/signalr";

export interface QuestionToClient {
  questionText: string;
  explanation?: string;
  correctAnswer?: string;
  playerAnswer?: string;
  playerAnswerCorrect?: boolean;
}

export interface ItemContent<T> {
  questions: T[];
  correctAnswers?: string[];
}

export type QuestionType =
  | "MultipleChoice"
  | "TrueOrFalse"
  | "FillInTheBlank"
  | "Ordering";

export interface TriviaItemForClient {
  id?: string;
  prompt: string;
  content: ItemContent<QuestionToClient>;
  questionType: QuestionType;
}

export interface GameStateResponse {
  playerScores: Record<string, number>;
  currentPlayerName: string;
  currentTriviaItem: TriviaItemForClient | null;
  gameStarted: boolean;
}


interface UseGameHubOptions {
  onConnected?: () => void;
  onDisconnected?: () => void;
}

export function useGameHub({ onConnected, onDisconnected }: UseGameHubOptions = {}) {
  const [gameState, setGameState] = useState<GameStateResponse | null>(null);
  const [isConnected, setIsConnected] = useState(false);
  const [hasReadiedUp, setHasReadiedUp] = useState(false);

  const connectionRef = useRef<signalR.HubConnection | null>(null);

  // Establish SignalR connection
  const connect = useCallback(async (gameId: string, playerId: string) => {
    if (connectionRef.current) return;

    const conn = new signalR.HubConnectionBuilder()
      .withUrl("/gamehub")
      .withAutomaticReconnect()
      .build();

    conn.on("GameStateUpdated", (state: GameStateResponse) => {
      setGameState(state);
    });

    conn.onclose(() => {
      setIsConnected(false);
      onDisconnected?.();
    });

    await conn.start();
    setIsConnected(true);
    connectionRef.current = conn;
    onConnected?.();

    // Join the game on the hub
    await conn.invoke("JoinGame", gameId, playerId);
  }, [onConnected, onDisconnected]);

  // Disconnect manually if needed
  const disconnect = useCallback(async () => {
    if (!connectionRef.current) return;
    await connectionRef.current.stop();
    setIsConnected(false);
    connectionRef.current = null;
  }, []);

  // Domain-friendly actions
  const startGame = useCallback(async (gameId: string, playerId: string) => {
    if (!connectionRef.current) return;
    await connectionRef.current.invoke("StartGame", gameId, playerId);
    setHasReadiedUp(true);
  }, []);

  const submitAnswer = useCallback(
    async (gameId: string, playerId: string, questionIndex: number, answer: string) => {
      if (!connectionRef.current) return;
      await connectionRef.current.invoke("SubmitAnswer", gameId, playerId, questionIndex, answer);
    },
    []
  );

  return {
    isConnected,
    gameState,
    hasReadiedUp,
    connect,
    disconnect,
    startGame,
    submitAnswer,
  };
}
