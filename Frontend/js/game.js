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
    const fixedUpdateRate = 1 / 144;
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
    // this.position = Vector.lerp(this.player.position, this.player.nextPosition, dt);
    this.renderer.update(dt);
    this.camera.moveTo(this.player.position);
  }

  fixedUpdate(dt)
  {
    for (let i = 0; i < this.entitiesArray.length; i++)
    {
      const entity = this.entitiesArray[i];
      entity.update(dt);
    }
    this.detectCollisions(dt);
  }

  addEntity(entity)
  {
    if (!this.entities.has(entity.id))
    {
      // console.log(`added ${entity.id}`);
      this.entities.set(entity.id, entity);
      this.entitiesArray.push(entity);
    }
  }

  removeEntity(entity)
  {
    const id = entity.id;
    if (this.entities.has(id))
    {
      // console.log(`removed ${entity.id}`);
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
            impulseVec = impulseVec.multiply(0.5);
            fa = impulseVec.multiply(-b.inverseMass);
            b.velocity = b.velocity.add(fb);
            a.velocity = a.velocity.multiply(1 - 0.99 * dt);
          }
          else
            a.velocity = a.velocity.add(fa);

          if (b instanceof Bullet)
          {
            impulseVec = impulseVec.multiply(0.5);
            fb = impulseVec.multiply(a.inverseMass);
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
    const name = node.value;
    window.game = new Game(name);
    const div = document.getElementById("textInputContainer");
    div.style.display = "none";
    window.game.net.connect();
  }
});