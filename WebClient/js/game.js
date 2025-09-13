import { Net } from "./network/Net.js";
import { Player } from "./entities/Player.js";
import { Renderer } from "./rendering/Renderer.js";
import { UiRenderer } from "./rendering/UiRenderer.js";
import { Camera } from "./rendering/Camera.js";
import { Input } from "./Input.js";

export class Game {
  entities = new Map();
  entityNames = new Map();
  entitiesArray = [];

  secondsPassed = 0;
  oldTimeStamp = 0;
  fixedUpdateAcc = 0;

  player = null;
  net = null;
  renderer = null;
  uiRenderer = null;
  camera = null;

  constructor(name) {
    this.entityNames.set(0, "Server");
    
    // Initialize global tracking variables
    window.totalBytesReceived = 0;
    window.bytesReceived = 0;
    window.totalBytesSent = 0;
    window.bytesSent = 0;
    window.chatLog = Array(18).fill("");
    window.leaderboardLog = Array(5).fill("");
    window.input = new Input();

    const canvas = document.getElementById('gameCanvas');
    const context = canvas.getContext('2d');
    
    this.player = new Player(0, name.substring(0, 15), 211, 211);
    this.camera = new Camera(context, this.player);
    this.renderer = new Renderer(this.camera);
    this.uiRenderer = new UiRenderer();
    this.net = new Net();
    
    window.requestAnimationFrame(dt => this.gameLoop(dt));
  }

  random(min, max) { 
    return Math.floor(Math.random() * (max - min)) + min; 
  }


  async gameLoop(dt) {
    const fixedUpdateRate = 1 / 144;
    this.secondsPassed = (dt - this.oldTimeStamp) / 1000;
    this.oldTimeStamp = dt;
    this.fixedUpdateAcc += this.secondsPassed;

    if (this.fixedUpdateAcc >= fixedUpdateRate) {
      this.fixedUpdate(fixedUpdateRate);
      this.fixedUpdateAcc -= fixedUpdateRate;
    }

    this.update();
    this.draw();
  }

  update() {
    this.player.update();

    for (const entity of this.entitiesArray) {
      entity.update(this.secondsPassed);
    }

    this.camera.moveTo(this.player.position);
    this.renderer.update(this.secondsPassed);
    this.uiRenderer.update(this.secondsPassed);
  }

  draw() {
    this.renderer.draw(this.secondsPassed);
    this.uiRenderer.draw();
    window.requestAnimationFrame(dt => this.gameLoop(dt));
  }

  fixedUpdate(dt) {
    // Fixed update logic would go here
  }

  addEntity(entity) {
    if (this.entities.has(entity.id)) return;
    
    this.entities.set(entity.id, entity);
    this.entitiesArray.push(entity);
  }

  addEntityName = (entityId, name) => this.entityNames.set(entityId, name);

  removeEntity(entity) {
    const id = entity.id;
    if (!this.entities.has(id)) return;
    
    this.entities.delete(id);
    const index = this.entitiesArray.findIndex(e => e.id === id);
    if (index !== -1) {
      this.entitiesArray.splice(index, 1);
    }
  }

  sendMessage = text => this.net.sendMessage(text);

  addChatLogLine(text) {
    if (window.chatLog.length >= 18) {
      window.chatLog.shift();
    }
    window.chatLog.push(text);
  }

  addLeaderboardLine(text) {
    if (window.leaderboardLog.length >= 5) {
      window.leaderboardLog.shift();
    }
    window.leaderboardLog.push(text);
  }
}


function initGame() {
  const chatNode = document.getElementById("chatInputContainer");
  const textInput = document.getElementById("textInput");
  const textInputContainer = document.getElementById("textInputContainer");
  
  chatNode.style.display = "none";
  textInput.focus();
  
  // Keep input focused
  textInput.addEventListener("blur", () => textInput.focus());
  
  textInput.addEventListener("keyup", async (event) => {
    if (event.key === "Enter") {
      const name = textInput.value.trim() || "unnamed";
      
      window.game = new Game(name);
      
      try {
        const connected = await window.game.net.connect();
        if (!connected) {
          alert("Failed to connect to server");
          return;
        }
        
        const gameCanvas = document.getElementById("gameCanvas");
        gameCanvas.style.display = "block";
        textInputContainer.style.display = "none";
      } catch (error) {
        console.error("Connection error:", error);
        alert("Failed to connect to server");
      }
    }
  });
}

window.initGame = initGame;
initGame();