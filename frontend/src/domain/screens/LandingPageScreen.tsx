import { useNavigate } from "react-router-dom";
import { Button } from "../../ui/Button";

export function LandingPageScreen() {
  const navigate = useNavigate();

  return (
    <div
      style={{
        height: "100vh",
        display: "flex",
        flexDirection: "column",
        justifyContent: "center",
        alignItems: "center",
        gap: 24,
        background: "linear-gradient(135deg, #89f7fe 0%, #66a6ff 100%)",
      }}
    >
      <h1 style={{ fontSize: 48, fontWeight: 700, color: "#fff", margin: 0 }}>
        Trivia Game
      </h1>

      <div style={{ display: "flex", flexDirection: "column", gap: 16, width: 200 }}>
        <Button label="Create Game" onClick={() => navigate("/create")} />
        <Button label="Join Game" onClick={() => navigate("/join")} />
      </div>
    </div>
  );
}
