import { PlayerInformation } from "./PlayerInformation";

type PlayerOverlayItem = {
  id: string;
  name: string;
  score: number;
  isThisClient: boolean;
  isThisPlayersTurn: boolean;
};

type PlayersOverlayProps = {
  players: PlayerOverlayItem[];
};

const cornerStyles = [
  { top: 16, left: 16 },     // top-left
  { top: 16, right: 16 },   // top-right
  { bottom: 16, left: 16 }, // bottom-left
];

export function PlayersOverlay({ players }: PlayersOverlayProps) {
  const clientPlayer = players.find((p) => p.isThisClient);
  const otherPlayers = players.filter((p) => !p.isThisClient);

  return (
    <>
      {clientPlayer && (
        <div
          style={{
            position: "absolute",
            bottom: 16,
            right: 16,
          }}
        >
          <PlayerInformation {...clientPlayer} />
        </div>
      )}

      {otherPlayers.map((player, index) => {
        const corner = cornerStyles[index];
        if (!corner) return null;

        return (
          <div
            key={player.id}
            style={{
              position: "absolute",
              ...corner,
            }}
          >
            <PlayerInformation {...player} />
          </div>
        );
      })}
    </>
  );
}
