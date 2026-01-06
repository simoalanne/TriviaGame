import { BrowserRouter, Routes, Route } from "react-router-dom";
import { MantineProvider } from "@mantine/core";
import { LandingPageScreen } from "./domain/screens/LandingPageScreen";
import { JoinGameScreen } from "./domain/screens/JoinGameScreen";
import { CreateGameScreen } from "./domain/screens/CreateGameScreen";
import { GameplayScreen } from "./domain/screens/Gameplay";
import { GameProvider } from "./providers/GameProvider";
import '@mantine/core/styles.css';

export default function App() {
  return (
    <MantineProvider>
      <GameProvider>
        <BrowserRouter>
          <Routes>
            <Route path="/" element={<LandingPageScreen />} />
            <Route path="/join" element={<JoinGameScreen />} />
            <Route path="/create" element={<CreateGameScreen />} />
            <Route path="/gameplay" element={<GameplayScreen />} />
          </Routes>
        </BrowserRouter>
      </GameProvider>
    </MantineProvider>
  );
}
