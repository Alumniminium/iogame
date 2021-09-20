import { Net } from "./network/network.js";
import { Player } from "./entities/player.js";
import { Camera } from "./camera.js"
import { Vector } from "./vector.js";

'use strict'

export class Game {
  random = (min, max) => Math.floor(Math.random() * (max - min)) + min;
  MAP_WIDTH = 100;
  MAP_HEIGHT = 100;

  canvas = document.getElementById('gameCanvas');
  context = this.canvas.getContext('2d');

  secondsPassed;
  oldTimeStamp = 0;
  fps;

  entities = new Map();
  entitiesArray = [];

  player = new Player(this, 0, "Player Name", 211, 211);
  camera = new Camera(this.context, this.player);
  net = new Net(this);

  constructor() {
    this.setCanvasDimensions();
    this.net.connect();
    window.addEventListener('resize', this.setCanvasDimensions.bind(this));
    window.requestAnimationFrame((timeStamp) => { this.gameLoop(timeStamp) });
  }

  setCanvasDimensions() {
    this.canvas.width = window.innerWidth;
    this.canvas.height = window.innerHeight;
  }

  gameLoop(timeStamp) {
    this.secondsPassed = (timeStamp - this.oldTimeStamp) / 1000;
    this.oldTimeStamp = timeStamp;

    this.update(this.secondsPassed)
    this.draw(this.secondsPassed);

    window.requestAnimationFrame((timeStamp) => this.gameLoop(timeStamp));
  }

  update(secondsPassed) {
    this.fps = Math.round(1 / secondsPassed);

    for (let i = 0; i < this.entitiesArray.length; i++) {
      const entity = this.entitiesArray[i];
      entity.update(secondsPassed);
    }

    this.camera.moveTo(this.player.origin());
    this.detectCollisions(secondsPassed);
    this.detectEdgeCollisions();
  }
  draw() {
    this.context.fillStyle = "#292d3e";
    this.context.fillRect(0, 0, this.canvas.width, this.canvas.height);
    this.camera.begin();
    this.drawGridLines();

    this.context.strokeStyle = "#fffff";
    this.context.strokeRect(8, 8, this.MAP_WIDTH - 8, this.MAP_HEIGHT - 8);

    for (let i = 0; i < this.entitiesArray.length; i++) {
      const entity = this.entitiesArray[i];
      if (this.camera.canSee(entity))
        entity.draw(this.context);
      else
        this.removeEntity(entity.id);
    }
    this.camera.end();
    this.drawFpsCounter();
  }

  drawGridLines() {
    let s = 50;
    this.context.strokeStyle = '#041f2d';
    this.context.lineWidth = 1;
    this.context.beginPath();
    for (let x = s; x <= this.MAP_WIDTH - s; x += s) {
      this.context.moveTo(x, s);
      this.context.lineTo(x, this.MAP_HEIGHT - s);
    }
    for (let y = s; y <= this.MAP_HEIGHT - s; y += s) {
      this.context.moveTo(s, y);
      this.context.lineTo(this.MAP_WIDTH - s, y);
    }
    this.context.stroke();

    s = 300;
    this.context.strokeStyle = 'magenta';
    this.context.lineWidth = 1;
    this.context.beginPath();
    for (let x = s; x <= this.MAP_WIDTH - s; x += s) {
      this.context.moveTo(x, s);
      this.context.lineTo(x, this.MAP_HEIGHT - s);
    }
    for (let y = s; y <= this.MAP_HEIGHT - s; y += s) {
      this.context.moveTo(s, y);
      this.context.lineTo(this.MAP_WIDTH - s, y);
    }
    this.context.stroke();

    this.context.fillStyle = "magenta";
    for(let x2 =0; x2 <= this.MAP_WIDTH-s;x2+=s)
    {
      for(let y2 =0; y2 <= this.MAP_HEIGHT-s; y2+=s)
        this.context.fillText(`${x2/s},${y2/s}`,x2 + s/2,y2+s/2,s);
    }
  }

  drawFpsCounter() {
    this.context.font = '20px Arial';
    this.context.fillStyle = 'white';
    const fpsString = "FPS: " + this.fps;
    const stringSize = this.context.measureText(this.fpsString);
    this.context.fillText(fpsString, this.canvas.width / 2 - stringSize.width / 2, this.canvas.height - stringSize.fontBoundingBoxAscent);
  }


  addEntity(entity) {
    if (!this.entities.has(entity.id)) {
      this.entities.set(entity.id, entity);
      this.entitiesArray.push(entity);
    }
  }
  removeEntity(id) {
    if (this.entities.has(id)) {
      this.entities.delete(id);
      for (let i = 0; i < this.entitiesArray.length; i++) {
        if (this.entitiesArray[i].id == id) {
          this.entitiesArray.splice(i, 1);
          break;
        }
      }
    }
  }

  detectEdgeCollisions() {
    for (let i = 0; i < this.entitiesArray.length; i++) {
      const entity = this.entitiesArray[i];
      if (entity.position.x < entity.radius) {
        entity.velocity.x = Math.abs(entity.velocity.x) * entity.restitution;
        entity.position.x = entity.radius;
      } else if (entity.position.x > this.MAP_WIDTH - entity.size) {
        entity.velocity.x = -Math.abs(entity.velocity.x) * entity.restitution;
        entity.position.x = this.MAP_WIDTH - entity.size;
      }

      if (entity.position.y < entity.radius) {
        entity.velocity.y = Math.abs(entity.velocity.y) * entity.restitution;
        entity.position.y = entity.radius;
      } else if (entity.position.y > this.MAP_HEIGHT - entity.size) {
        entity.velocity.y = -Math.abs(entity.velocity.y) * entity.restitution;
        entity.position.y = this.MAP_HEIGHT - entity.size;
      }
    }
  }

  detectCollisions() {

    const possibleCollisions = [];

    for (let i = 0; i < this.entitiesArray.length; i++) {
      const entity = this.entitiesArray[i];
      entity.inCollision = false;
    }

    for (let i = 0; i < this.entitiesArray.length; i++) {
      const a = this.entitiesArray[i];

      if (a.InCollision)
        continue;

      for (let j = i; j < this.entitiesArray.length; j++) {
        const b = this.entitiesArray[j];

        if (a == b || a.InCollision || b.InCollisio)
          continue;

        if (b.owner != undefined) {
          if (b.owner == a)
            continue;
        }
        if (a.owner != undefined) {
          if (a.owner == b)
            continue;
        }

        if (a.checkCollision_Circle(b)) {
          a.inCollision = true;
          b.inCollision = true;
          let collision = Vector.subtract(b.position, a.position);
          let distance = Vector.distance(b.position, a.position);
          let collisionNormalized = collision.divide(distance);
          let relativeVelocity = Vector.subtract(a.velocity, b.velocity);
          let speed = Vector.dot(relativeVelocity, collisionNormalized);

          if (speed < 0)
            continue;

          var overlap = Vector.subtract(a.origin(), b.origin());
          var off = overlap.length() - (a.radius + b.radius);
          var direction = Vector.normalize(b.origin() - a.origin());
          var mul = Vector.multiply(direction,off);
          a.position.add(mul);
          b.position.subtract(mul);

          let impulse = 2 * speed / (Math.pow(a.size, 3) + Math.pow(b.size, 3));
          let fa = new Vector(impulse * Math.pow(b.size, 3) * collisionNormalized.x, impulse * Math.pow(b.size, 3) * collisionNormalized.y);
          let fb = new Vector(impulse * Math.pow(a.size, 3) * collisionNormalized.x, impulse * Math.pow(a.size, 3) * collisionNormalized.y);

          a.velocity.subtract(fa);
          b.velocity.add(fb);

          if (a.isPlayer || b.isPlayer) {
            if (a.health > 0)
              a.health--;
            if (b.health > 0)
              b.health--;
          }
        }
      }
    }
  }
}
var game = new Game();