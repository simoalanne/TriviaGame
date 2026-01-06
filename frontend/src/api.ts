import axios from "axios";

// Response types
export type CreateOrJoinGameResponse = {
  playerId: string;
  gameId: string;
};

export async function createGame(playerName: string): Promise<CreateOrJoinGameResponse> {
  const response = await axios.post<CreateOrJoinGameResponse>("/create-game", {
    playerName,
  });
  return response.data;
}

export async function getActiveGames(): Promise<string[]> {
  const response = await axios.get<string[]>("/active-games");
  return response.data;
}

export async function joinGame(playerName: string, gameId: string): Promise<CreateOrJoinGameResponse> {
  const response = await axios.post<CreateOrJoinGameResponse>("/join-game", {
    playerName,
    gameId,
  });
  return response.data;
}
