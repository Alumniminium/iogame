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

  camera = new Camera(this.context);
  secondsPassed;
  oldTimeStamp = 0;
  fps;

  entities = new Map();
  entitiesArray = [];

  player = new Player(this, 0, "Player Name", 211, 211);
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
    this.bgCanvas.width = window.innerWidth;
    this.bgCanvas.height = window.innerHeight;
    this.drawGridLines();
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
    this.context.clearRect(0, 0, this.canvas.width, this.canvas.height);

    this.camera.begin();
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
    this.bgContext.lineWidth = 8;
    this.bgContext.fillStyle = "#292d3e";
    this.bgContext.fillRect(0, 0, this.bgCanvas.width, this.bgCanvas.height);

    const s = 28;
    this.bgContext.lineWidth = 1;
    this.bgContext.strokeStyle = '#041f2d';
    this.bgContext.beginPath();
    for (let x = s; x <= this.MAP_WIDTH - s; x += s) {
      this.bgContext.moveTo(x, s);
      this.bgContext.lineTo(x, this.MAP_HEIGHT - s);
    }
    for (let y = s; y <= this.MAP_HEIGHT - s; y += s) {
      this.bgContext.moveTo(s, y);
      this.bgContext.lineTo(this.MAP_WIDTH - s, y);
    }
    this.bgContext.stroke();
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
      if (this.camera.canSee(entity)) {
        if (entity.position.x < entity.sizeHalf) {
          entity.velocity.x = Math.abs(entity.velocity.x) * entity.restitution;
          entity.position.x = entity.sizeHalf;
        } else if (entity.position.x > this.MAP_WIDTH - entity.size) {
          entity.velocity.x = -Math.abs(entity.velocity.x) * entity.restitution;
          entity.position.x = this.MAP_WIDTH - entity.size;
        }

        if (entity.position.y < entity.sizeHalf) {
          entity.velocity.y = Math.abs(entity.velocity.y) * entity.restitution;
          entity.position.y = entity.sizeHalf;
        } else if (entity.position.y > this.MAP_HEIGHT - entity.size) {
          entity.velocity.y = -Math.abs(entity.velocity.y) * entity.restitution;
          entity.position.y = this.MAP_HEIGHT - entity.size;
        }
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

      if (this.camera.canSee(a)) {
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

          if (this.camera.canSee(b)) {

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
  }
}

var game = new Game();