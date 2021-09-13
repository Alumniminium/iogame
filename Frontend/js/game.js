import { Net } from "./network/network.js";
import { Player } from "./entities/player.js";
import { YellowSquare } from "./entities/yellowSquare.js";
import { PurpleOctagon } from "./entities/PurpleOctagon.js";
import { PurplePentagon } from "./entities/PurplePentagon.js";
import { RedTriangle } from "./entities/RedTriangle.js";
import { Camera } from "./camera.js"
import { Vector } from "./vector.js";
import { Packets } from "./network/packets.js";

export class Game {
  random = (min, max) => Math.floor(Math.random() * (max - min)) + min;

  MAP_WIDTH = 5000;
  MAP_HEIGHT = 5000;
  canvas = document.getElementById('gameCanvas');
  context = this.canvas.getContext('2d');
  camera = new Camera(this.context);
  secondsPassed;
  oldTimeStamp = 0;
  fps;
  restitution = 0.9

  gameObjects = [];
  player = new Player(0, "Player Name", 211, 211);
  net = new Net(this);

  constructor() {
    for (let i = 0; i < 400; i++) {
      this.gameObjects.push(new YellowSquare(i, this.random(0, this.MAP_WIDTH), this.random(0, this.MAP_HEIGHT), this.random(-3, 4), this.random(-3, 4)));
    }
    this.setCanvasDimensions();
    this.gameObjects.push(this.player);
    this.camera.distance = 1700
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

    for (let i = 0; i < this.gameObjects.length; i++) {
      this.gameObjects[i].update(secondsPassed);
      //if (this.gameObjects[i].health <= 0) {
      //  this.gameObjects.splice(i, 1);
      //}
    }
    this.camera.moveTo(this.player.originX(), this.player.originY());
    this.detectEdgeCollisions();
    this.detectCollisions(secondsPassed);
  }
  draw() {

    this.context.lineWidth = 8;
    this.context.fillStyle = "#292d3e";
    this.context.fillRect(0, 0, this.canvas.width, this.canvas.height);

    this.camera.begin();
    this.drawGridLines();
    this.context.strokeStyle = "#fffff";
    this.context.strokeRect(8, 8, this.MAP_WIDTH - 8, this.MAP_HEIGHT - 8);

    for (let i = 0; i < this.gameObjects.length; i++) {
      let entity = this.gameObjects[i];
      if (entity.originX() > this.camera.viewport.left && entity.originX() < this.camera.viewport.right) {
        if (entity.originY() > this.camera.viewport.top && entity.originY() < this.camera.viewport.bottom) {
          this.gameObjects[i].draw(this.context);
        }
      }
    }

    this.camera.end();
    this.drawFpsCounter();
  }

  drawGridLines() {
    let s = 28;
    let pL = 20;
    let pT = 20;
    let pR = 20;
    let pB = 20;
    this.context.lineWidth = 1;
    this.context.strokeStyle = '#232735';
    this.context.beginPath();
    for (let x = pL; x <= this.MAP_WIDTH - pR; x += s) {
      this.context.moveTo(x, pT);
      this.context.lineTo(x, this.MAP_HEIGHT - pB);
    }
    for (let y = pT; y <= this.MAP_HEIGHT - pB; y += s) {
      this.context.moveTo(pL, y);
      this.context.lineTo(this.MAP_WIDTH - pR, y);
    }
    this.context.stroke();
  }

  drawFpsCounter() {
    this.context.font = '25px Arial';
    this.context.fillStyle = 'white';
    this.context.fillText("FPS: " + this.fps, 10, 30);
  }

  detectEdgeCollisions() {
    for (let i = 0; i < this.gameObjects.length; i++) {
      let entity = this.gameObjects[i];
      if (entity.originX() > this.camera.viewport.left && entity.originX() < this.camera.viewport.right) {
        if (entity.originY() > this.camera.viewport.top && entity.originY() < this.camera.viewport.bottom) {
          {
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
  }

  detectCollisions(dt) {

    let a;
    let b;


    for (let i = 0; i < this.gameObjects.length; i++) {
      this.gameObjects[i].inCollision = false;


      for (let i = 0; i < this.gameObjects.length; i++) {
        a = this.gameObjects[i];

        if (a.originX() > this.camera.viewport.left && a.originX() < this.camera.viewport.right) {
          if (a.originY() > this.camera.viewport.top && a.originY() < this.camera.viewport.bottom) {
            for (let j = i + 1; j < this.gameObjects.length; j++) {
              {
                b = this.gameObjects[j];
                if (b.originX() > this.camera.viewport.left && b.originX() < this.camera.viewport.right) {
                  if (b.originY() > this.camera.viewport.top && b.originY() < this.camera.viewport.bottom) {
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
        }
      }
    }
  }
}
var game = new Game();