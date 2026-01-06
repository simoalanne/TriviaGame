type PlayerCardProps = {
  name: string;
  score: number;
  leadingZeros?: number;
  isThisClient: boolean;
  isThisPlayersTurn: boolean;
};

export function PlayerInformation({
  name,
  score,
  leadingZeros = 3,
  isThisClient,
  isThisPlayersTurn,
}: PlayerCardProps) {
  const paddedScore = String(score).padStart(leadingZeros, "0");

  return (
    <div
      style={{
        display: "flex",
        flexDirection: "column",
        alignItems: "flex-start",
        gap: 4,
        padding: 12,
        borderRadius: 8,
        transform: isThisPlayersTurn ? "scale(1.03)" : "scale(1)",
        animation: isThisPlayersTurn ? "pulse 1.6s ease-in-out infinite" : undefined,
      }}
    >
      <span
        style={{
          fontSize: 16,
          fontWeight: 600,
          color: isThisClient ? "#1c7ed6" : "#222",
        }}
      >
        {name}
      </span>

      <span
        style={{
          fontSize: 14,
          fontVariantNumeric: "tabular-nums",
          opacity: 0.75,
        }}
      >
        {paddedScore}
      </span>

      <style>
        {`
          @keyframes pulse {
            0% { transform: scale(1); }
            50% { transform: scale(1.05); }
            100% { transform: scale(1); }
          }
        `}
      </style>
    </div>
  );
}
