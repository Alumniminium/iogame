import { Net } from "./network/network.js";
import { Player } from "./entities/player.js";
import { YellowSquare } from "./entities/yellowSquare.js";
import { PurpleOctagon } from "./entities/PurpleOctagon.js";
import { PurplePentagon } from "./entities/PurplePentagon.js";
import { RedTriangle } from "./entities/RedTriangle.js";
import { Camera } from "./camera.js"
import { Vector } from "./vector.js";
import { Packets } from "./network/packets.js";

'use strict'

export class Game {
  random = (min, max) => Math.floor(Math.random() * (max - min)) + min;

  MAP_WIDTH = 1000;
  MAP_HEIGHT = 1000;
  canvas = document.getElementById('gameCanvas');
  context = this.canvas.getContext('2d');
  camera = new Camera(this.context);
  secondsPassed;
  oldTimeStamp = 0;
  fps;
  restitution = 0.9

  entities = new Map();
  // gameObjects = [];
  player = new Player(0, "Player Name", 211, 211);
  net = new Net(this);

  constructor() {
    this.setCanvasDimensions();
    // this.gameObjects.push(this.player);
    this.camera.distance = 1000
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

    for (const [key, value] of this.entities) {
      value.update(secondsPassed);

      this.camera.moveTo(this.player.originX(), this.player.originY());
      this.detectEdgeCollisions();
      this.detectCollisions(secondsPassed);
    }
  }
  draw() {

    this.context.lineWidth = 8;
    this.context.fillStyle = "#292d3e";
    this.context.fillRect(0, 0, this.canvas.width, this.canvas.height);

    this.camera.begin();
    this.drawGridLines();
    this.context.strokeStyle = "#fffff";
    this.context.strokeRect(8, 8, this.MAP_WIDTH - 8, this.MAP_HEIGHT - 8);


    for (const [id, entity] of this.entities) {
      if (entity.originX() > this.camera.viewport.left && entity.originX() < this.camera.viewport.right) {
        if (entity.originY() > this.camera.viewport.top && entity.originY() < this.camera.viewport.bottom) {
          entity.draw(this.context);
        }
      }
    }
    this.camera.end();
    this.drawFpsCounter();
  }

  drawGridLines() {
    const s = 28;
    this.context.lineWidth = 1;
    this.context.strokeStyle = '#041f2d';
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
  }

  drawFpsCounter() {
    this.context.font = '20px Arial';
    this.context.fillStyle = 'white';
    const fpsString = "FPS: " + this.fps;
    const stringSize = this.context.measureText(this.fpsString);
    this.context.fillText(fpsString, this.canvas.width / 2 - stringSize.width / 2, this.canvas.height - stringSize.fontBoundingBoxAscent);
  }

  detectEdgeCollisions() {
    for (const [id, entity] of this.entities) {
      if (entity.originX() > this.camera.viewport.left && entity.originX() < this.camera.viewport.right) {
        if (entity.originY() > this.camera.viewport.top && entity.originY() < this.camera.viewport.bottom) {
          if (entity.position.x < entity.size / 2) {
            entity.velocity.x = Math.abs(entity.velocity.x) * this.restitution;
            entity.position.x = entity.size / 2;
          } else if (entity.position.x > this.MAP_WIDTH - entity.size) {
            entity.velocity.x = -Math.abs(entity.velocity.x) * this.restitution;
            entity.position.x = this.MAP_WIDTH - entity.size;
          }

          if (entity.position.y < entity.size / 2) {
            entity.velocity.y = Math.abs(entity.velocity.y) * this.restitution;
            entity.position.y = entity.size / 2;
          } else if (entity.position.y > this.MAP_HEIGHT - entity.size) {
            entity.velocity.y = -Math.abs(entity.velocity.y) * this.restitution;
            entity.position.y = this.MAP_HEIGHT - entity.size;
          }
        }
      }
    }
  }

  detectCollisions(dt) {
    for (const entity of this.entities.values()) {
      entity.inCollision = false;
    }

    for (const a of this.entities.values()) {
      if (a.originX() > this.camera.viewport.left && a.originX() < this.camera.viewport.right && a.originY() > this.camera.viewport.top && a.originY() < this.camera.viewport.bottom) {
        for (const b of this.entities.values()) {
          if (b.originX() > this.camera.viewport.left && b.originX() < this.camera.viewport.right && b.originY() > this.camera.viewport.top && b.originY() < this.camera.viewport.bottom) {
            if (a.checkCollision_Circle(b)) {
              a.inCollision = true;
              b.inCollision = true;
              let collision = Vector.subtract(b.position, a.position);
              let distance = Vector.distance(b.position, a.position);
              let collisionNormalized = collision.divide(distance);
              let relativeVelocity = Vector.subtract(a.velocity, b.velocity);
              let speed = Vector.dot(relativeVelocity, collisionNormalized);

              //speed *= 0.5;
              if (speed < 0) {
                continue;
              }

              let impulse = 2 * speed / (a.size + b.size);
              let fa = new Vector(impulse * b.size * collisionNormalized.x, impulse * b.size * collisionNormalized.y);
              let fb = new Vector(impulse * a.size * collisionNormalized.x, impulse * a.size * collisionNormalized.y);

              // fa.multiply(dt);
              // fb.multiply(dt);

              a.velocity.subtract(fa);
              b.velocity.add(fb);

              if (a.isPlayer || b.isPlayer) {
                a.health--;
                b.health--;
              }
            }
          }
        }
      }
    }
  }
  // let a;
  // let b;
  // for (let i = 0; i < this.gameObjects.length; i++) {
  //   this.gameObjects[i].inCollision = false;


  //   for (let i = 0; i < this.gameObjects.length; i++) {
  //     a = this.gameObjects[i];

  //     if (a.originX() > this.camera.viewport.left && a.originX() < this.camera.viewport.right) {
  //       if (a.originY() > this.camera.viewport.top && a.originY() < this.camera.viewport.bottom) {
  //         for (let j = i + 1; j < this.gameObjects.length; j++) {
  //           {
  //             b = this.gameObjects[j];
  //             if (b.originX() > this.camera.viewport.left && b.originX() < this.camera.viewport.right) {
  //               if (b.originY() > this.camera.viewport.top && b.originY() < this.camera.viewport.bottom) {
  //                 if (a.checkCollision_Circle(b)) {
  //                   a.inCollision = true;
  //                   b.inCollision = true;
  //                   let collision = Vector.subtract(b.position, a.position);
  //                   let distance = Vector.distance(b.position, a.position);
  //                   let collisionNormalized = collision.divide(distance);
  //                   let relativeVelocity = Vector.subtract(a.velocity, b.velocity);
  //                   let speed = Vector.dot(relativeVelocity, collisionNormalized);

  //                   //speed *= 0.5;
  //                   if (speed < 0) {
  //                     continue;
  //                   }

  //                   let impulse = 2 * speed / (a.size + b.size);
  //                   let fa = new Vector(impulse * b.size * collisionNormalized.x, impulse * b.size * collisionNormalized.y);
  //                   let fb = new Vector(impulse * a.size * collisionNormalized.x, impulse * a.size * collisionNormalized.y);

  //                   // fa.multiply(dt);
  //                   // fb.multiply(dt);

  //                   a.velocity.subtract(fa);
  //                   b.velocity.add(fb);

  //                   if (a.isPlayer || b.isPlayer) {
  //                     a.health--;
  //                     b.health--;
  //                   }
  //                 }
  //               }
  //             }
  //           }
  //         }
  //       }
  //     }
  //   }
}
var game = new Game();