import { setEngine } from "./app/getEngine";
import { GameScreen } from "./app/screens/game/GameScreen";
import { userSettings } from "./app/utils/userSettings";
import { CreationEngine } from "./engine/engine";

/**
 * Importing these modules will automatically register there plugins with the engine.
 */
import "@pixi/sound";

const engine = new CreationEngine();
setEngine(engine);

(async () => {
  await engine.init({
    background: "#1E1E1E",
    resizeOptions: { minWidth: 768, minHeight: 1024, letterbox: false },
  });

  userSettings.init();

  await engine.navigation.showScreen(GameScreen);
})();
