import { Net } from "./network/network.js";
import { Player } from "./entities/player.js";
import { renderer } from "./renderer.js";
import { uiRenderer } from "./uiRenderer.js";
import { Camera } from "./camera.js";
import { Bullet } from "./entities/bullet.js";
import { Input } from "./input.js";

export class Game
{
  random(min, max) { return Math.floor(Math.random() * (max - min)) + min; }

  MAP_WIDTH = -1;
  MAP_HEIGHT = -1;

  secondsPassed;
  oldTimeStamp = 0;
  totalTime = 0;
  fixedUpdateAcc = 0;

  entities = new Map();
  entitiesArray = [];
  players = [];

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
    this.renderer = new renderer(this.camera);
    this.uiRenderer = new uiRenderer();
    this.net = new Net();
    window.requestAnimationFrame(dt => this.gameLoop(dt));
  }


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
    this.player.update();
    for (let i = 0; i < this.entitiesArray.length; i++)
    {
      const entity = this.entitiesArray[i];
      entity.update(this.secondsPassed);
    }
    this.camera.moveTo(this.player.position);
    //this.detectCollisions(this.secondsPassed);
    this.renderer.update(this.secondsPassed);
    this.uiRenderer.update(this.secondsPassed);

    this.renderer.draw(this.secondsPassed);
    this.uiRenderer.draw();

    window.requestAnimationFrame(dt => this.gameLoop(dt));
  }

  fixedUpdate(dt) { }

  addEntity(entity)
  {
    if (!this.entities.has(entity.id))
    {
      this.entities.set(entity.id, entity);
      this.entitiesArray.push(entity);
    }
  }

  removeEntity(entity)
  {
    const id = entity.id;
    if (this.entities.has(id))
    {
      // if (id == this.player.id)
      // window.location.reload();

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
    if (text.startsWith("Server:"))
    {
      console.log(text);
    }
    else
    {
      if (window.chatLog.length == 18)
        window.chatLog.shift();

      window.chatLog.push(text);
    }
  }
  detectCollisions(dt)
  {
    for (let i = 0; i < this.entitiesArray.length; i++)
    {
      const a = this.entitiesArray[i];

      for (let j = i + 1; j < this.entitiesArray.length; j++)
      {
        const b = this.entitiesArray[j];

        if (a.owner == b || b.owner == a)
          continue;

        if (a.intersectsWithCircle(b))
        {
          if (!(a instanceof Bullet) && !(b instanceof Bullet))
            this.resolvePenetration(a, b);

          let normal = a.position.subtract(b.position).unit();
          let relVel = a.velocity.subtract(b.velocity);
          let sepVel = relVel.dot(normal);
          let new_sepVel = -sepVel * Math.min(a.elasticity, b.elasticity);
          let vsep_diff = new_sepVel - sepVel;

          let impulse = vsep_diff / (a.inverseMass + b.inverseMass);
          let impulseVec = normal.multiply(impulse);

          let fa = impulseVec.multiply(a.inverseMass);
          let fb = impulseVec.multiply(-b.inverseMass);

          if (a instanceof Bullet)
          {
            b.velocity = b.velocity.add(fb);
            a.velocity = a.velocity.multiply(1 - 0.99 * dt);
          }
          else
            a.velocity = a.velocity.add(fa);

          if (b instanceof Bullet)
          {
            a.velocity = a.velocity.add(fa);
            a.velocity = b.velocity.multiply(1 - 0.99 * dt);
          }
          else
            b.velocity = b.velocity.add(fb);

        }
      }
    }
  }
  resolvePenetration(a, b)
  {
    let dist = a.position.subtract(b.position);
    let pen_depth = a.radius + b.radius - dist.magnitude();
    let pen_res = dist.unit().multiply(pen_depth / (a.inverseMass + b.inverseMass));

    a.position = a.position.add(pen_res.multiply(a.inverseMass));
    b.position = b.position.add(pen_res.multiply(-b.inverseMass));
  }
}

const chatNode = document.getElementById("chatInputContainer");
chatNode.style.display = "none";

const node = document.getElementById("textInput");
node.focus();
node.addEventListener("keyup", function (event)
{
  if (event.key === "Enter")
  {
    const div = document.getElementById("textInputContainer");
    div.style.display = "none";
    const gameCanvas = document.getElementById("gameCanvas");
    gameCanvas.style.display = "block";

    const name = node.value;
    window.game = new Game(name);
    window.game.net.connect();
  }
});