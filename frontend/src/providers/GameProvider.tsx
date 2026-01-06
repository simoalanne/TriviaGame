type GameContextState = {
  playerId: string | null;
  gameId: string | null;
  username: string | null;
  setPlayerId: (id: string) => void;
  setGameId: (id: string) => void;
  setUsername: (name: string) => void;
};

import { createContext, useContext, useState, type ReactNode } from 'react';

const GameContext = createContext<GameContextState | undefined>(undefined);

export function GameProvider({ children }: { children: ReactNode }) {
  const [playerId, setPlayerId] = useState<string | null>(null);
  const [gameId, setGameId] = useState<string | null>(null);
  const [username, setUsername] = useState<string | null>(null);

  return (
    <GameContext.Provider value={{ playerId, setPlayerId, gameId, setGameId, username, setUsername }}>
      {children}
    </GameContext.Provider>
  );
}

export function useGameContext() {
  const context = useContext(GameContext);
  if (!context) throw new Error("useGameContext must be used within GameProvider");
  return context;
}
