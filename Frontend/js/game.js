import { Net } from "./network/network.js";
import { Player } from "./entities/player.js";
import { renderer } from "./renderer.js";
import { Camera } from "./camera.js";
import { Bullet } from "./entities/bullet.js";

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
  redTriangles = [];
  yellowSquares = [];
  purplePentagons = [];
  players = [];

  player = null;
  net = null;
  renderer = null;
  camera = null;

  constructor(name)
  {
    window.totalBytesReceived = 0;
    window.bytesReceived = 0;
    window.totalBytesSent = 0;
    window.bytesSent = 0;
    window.chatLog = ["", "", "", "", "", "", "", "", "", ""];

    let canvas = document.getElementById('gameCanvas');
    let context = canvas.getContext('2d');

    this.player = new Player(0, name, 211, 211);
    this.camera = new Camera(context, this.player);
    this.renderer = new renderer(this.camera);
    this.net = new Net();
    window.requestAnimationFrame(dt => this.gameLoop(dt));
  }


  async gameLoop(dt)
  {
    const fixedUpdateRate = 1 / 60;
    this.secondsPassed = (dt - this.oldTimeStamp) / 1000;
    this.oldTimeStamp = dt;
    this.fixedUpdateAcc += this.secondsPassed;

    while (this.fixedUpdateAcc >= fixedUpdateRate)
    {
      this.fixedUpdate(fixedUpdateRate);
      this.fixedUpdateAcc -= fixedUpdateRate;
    }
    this.update(this.secondsPassed);
    this.renderer.draw(this.secondsPassed);

    window.requestAnimationFrame(dt => this.gameLoop(dt));
  }

  update(dt)
  {
    this.renderer.update(dt);
    this.camera.moveTo(this.player.position);
  }

  fixedUpdate(dt)
  {
    for (let i = 0; i < this.entitiesArray.length; i++)
    {
      const entity = this.entitiesArray[i];
      entity.update(dt);

      if (!this.camera.canSee(entity))
        this.removeEntity(entity);
    }
    this.detectCollisions();
  }

  addEntity(entity)
  {
    if (!this.entities.has(entity.id))
    {
      this.entities.set(entity.id, entity);
      this.entitiesArray.push(entity);

      if (entity.sides == 3)
        this.redTriangles.push(entity);
      else if (entity.sides == 4)
        this.yellowSquares.push(entity);
      else if (entity.sides == 5)
        this.purplePentagons.push(entity);
      else if(entity.id > 999999)
        this.players.push(entity);
    }
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
      if (id > 999999)
      {
        for (let i = 0; i < this.players.length; i++)
        {
          if (this.players[i].id == id)
          {
            this.players.splice(i, 1);
            break;
          }
        }
      }
      if (entity.sides == 3)
      {
        for (let i = 0; i < this.redTriangles.length; i++)
        {
          if (this.redTriangles[i].id == id)
          {
            this.redTriangles.splice(i, 1);
            break;
          }
        }
      }
      if (entity.sides == 4)
      {
        for (let i = 0; i < this.yellowSquares.length; i++)
        {
          if (this.yellowSquares[i].id == id)
          {
            this.yellowSquares.splice(i, 1);
            break;
          }
        }
      }
      if (entity.sides == 5)
      {
        for (let i = 0; i < this.purplePentagons.length; i++)
        {
          if (this.purplePentagons[i].id == id)
          {
            this.purplePentagons.splice(i, 1);
            break;
          }
        }
      }
    }
  }
  sendMessage(text)
  {
    this.net.sendMessage(text);
    this.addChatLogLine(this.player.name + ": " + text);
  }
  addChatLogLine(text)
  {
    if (window.chatLog.length == 10)
      window.chatLog.shift();

    window.chatLog.push(text);
  }
  detectCollisions()
  {
    for (let i = 0; i < this.entitiesArray.length; i++)
    {
      const a = this.entitiesArray[i];

      for (let j = i + 1; j < this.entitiesArray.length; j++)
      {
        const b = this.entitiesArray[j];

        if (a.owner == b || b.owner == a)
          continue;

        if (a.checkCollision_Circle(b))
        {
          let dist = a.position.subtract(b.position);
          let pen_depth = a.radius + b.radius - dist.magnitude();
          let pen_res = dist.unit().multiply(pen_depth / (a.inverseMass + b.inverseMass));
          a.position = a.position.add(pen_res.multiply(a.inverseMass));
          b.position = b.position.add(pen_res.multiply(-b.inverseMass));

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
            impulseVec = impulseVec.multiply(10);
            fa = impulseVec.multiply(-b.inverseMass);
            b.velocity = b.velocity.add(fb);
          }
          else if (b instanceof Bullet)
          {
            impulseVec = impulseVec.multiply(10);
            fb = impulseVec.multiply(a.inverseMass);
            a.velocity = a.velocity.add(fa);
          }
          else
          {
            a.velocity = a.velocity.add(fa);
            b.velocity = b.velocity.add(fb);
          }
        }
      }
    }
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
    const name = node.value;
    window.game = new Game(name);
    const div = document.getElementById("textInputContainer");
    div.style.display = "none";
    window.game.net.connect();
  }
});