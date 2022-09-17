import { Net } from "./network/Net.js";
import { Player } from "./entities/Player.js";
import { Renderer } from "./Renderer.js";
import { UiRenderer } from "./UiRenderer.js";
import { Camera } from "./Camera.js";
import { Input } from "./Input.js";

export class Game
{
  entities = new Map();
  entitiesArray = [];

  secondsPassed;
  oldTimeStamp = 0;
  fixedUpdateAcc = 0;


  player = null;
  net = null;
  renderer = null;
  uiRenderer = null;
  camera = null;

  constructor(name)
  {
    window.totalBytesReceived = 0;
    window.bytesReceived = 0;
    window.totalBytesSent = 0;
    window.bytesSent = 0;
    window.chatLog = ["", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""];
    window.input = new Input();

    let canvas = document.getElementById('gameCanvas');
    let context = canvas.getContext('2d');
    this.player = new Player(0, name.substring(0, 15), 211, 211);
    this.camera = new Camera(context, this.player);
    this.renderer = new Renderer(this.camera);
    this.uiRenderer = new UiRenderer();
    this.net = new Net();
    window.requestAnimationFrame(dt => this.gameLoop(dt));
  }
  random(min, max) { return Math.floor(Math.random() * (max - min)) + min; }


  async gameLoop(dt)
  {
    const fixedUpdateRate = 1 / 144;
    this.secondsPassed = (dt - this.oldTimeStamp) / 1000;
    this.oldTimeStamp = dt;
    this.fixedUpdateAcc += this.secondsPassed;

    if (this.fixedUpdateAcc >= fixedUpdateRate)
    {
      this.fixedUpdate(fixedUpdateRate);
      this.fixedUpdateAcc -= fixedUpdateRate;
    }

    this.update();
    this.draw();
  }

  update()
  {
    this.player.update();

    for (let i = 0; i < this.entitiesArray.length; i++)
    {
      const entity = this.entitiesArray[i];
      entity.update(this.secondsPassed);
    }

    this.camera.moveTo(this.player.position);
    this.renderer.update(this.secondsPassed);
    this.uiRenderer.update(this.secondsPassed);
  }
  draw()
  {
    this.renderer.draw(this.secondsPassed);
    this.uiRenderer.draw();
    window.requestAnimationFrame(dt => this.gameLoop(dt));
  }

  fixedUpdate(dt) { }

  addEntity(entity)
  {
    if (this.entities.has(entity.id))
      return;

    this.entities.set(entity.id, entity);
    this.entitiesArray.push(entity);
  }

  removeEntity(entity)
  {
    const id = entity.id;
    if (this.entities.has(id))
    {
      this.entities.delete(id);
      for (let i = 0; i < this.entitiesArray.length; i++)
      {
        if (this.entitiesArray[i].id == id)
        {
          this.entitiesArray.splice(i, 1);
          break;
        }
      }
    }
  }
  sendMessage = text => this.net.sendMessage(text);
  addChatLogLine(text)
  {
    if (window.chatLog.length == 18)
      window.chatLog.shift();

    window.chatLog.push(text);
  }
}


function NewGame()
{
  const chatNode = document.getElementById("chatInputContainer");
  chatNode.style.display = "none";

  const node = document.getElementById("textInput");
  node.focus();
  node.addEventListener("keyup", async function (event)
  {
    if (event.key === "Enter")
    {
      const div = document.getElementById("textInputContainer");
      const name = node.value;

      if (name == "")
      {
        alert("i asked for your fucking name");
        return;
      }

      window.game = new Game(name);
      if (await window.game.net.connect() == false)
      {
        alert("failed to connect to server");
        return;
      }
      else
      {
        const gameCanvas = document.getElementById("gameCanvas");
        gameCanvas.style.display = "block";
        div.style.display = "none";
      }
    }
  });
}
window.NewGame = NewGame;


NewGame();