import { Net } from "./network.js";
import { Input } from "./input.js";
import { Player } from "./entities/player.js";
import { YellowSquare } from "./entities/yellowSquare.js";
import { Camera } from "./camera.js"
import { Entity } from "./entities/entity.js";

const random = (min, max) => Math.floor(Math.random() * (max - min)) + min;

const MAP_WIDTH = 50000;
const MAP_HEIGHT = 3000;
window.addEventListener('resize', setCanvasDimensions);
const canvas = document.getElementById('gameCanvas');
const context = canvas.getContext('2d');
const camera = new Camera(context);

var player = new Player();

let gameObjects =
  [
    player
  ];

for (let i = 0; i < 1000; i++)
  gameObjects.push(new YellowSquare(random(1, MAP_WIDTH), random(1, MAP_HEIGHT), random(-3, 4), random(-3, 4)));

var net = new Net();
net.connect();

let secondsPassed;
let oldTimeStamp;
let fps;
const restitution = 0.5

setCanvasDimensions();

function setCanvasDimensions() {
  const scaleRatio = Math.max(1, 1000 / window.innerWidth);
  canvas.width = scaleRatio * window.innerWidth;
  canvas.height = scaleRatio * window.innerHeight;
}
window.requestAnimationFrame((timeStamp) => { gameLoop(timeStamp) });

function gameLoop(timeStamp) {

  secondsPassed = (timeStamp - oldTimeStamp) / 1000;
  oldTimeStamp = timeStamp;

  update(secondsPassed)
  draw(secondsPassed);

  window.requestAnimationFrame((timeStamp) => gameLoop(timeStamp));
}

function update(secondsPassed) {
  fps = Math.round(1 / secondsPassed);

  for (let i = 0; i < gameObjects.length; i++) {
    gameObjects[i].update(secondsPassed);
    if (gameObjects[i].health <= 0)
      gameObjects.splice(i, 1);
  }
  detectEdgeCollisions();
  detectCollisions();
}
function draw() {

  drawFpsCounter();

  camera.begin();
  let s = 28
  let pL = 20
  let pT = 20
  let pR = 20
  let pB = 20
  context.lineWidth = 1;
  context.strokeStyle = '#232735'
  context.beginPath()
  for (var x = pL; x <= MAP_WIDTH - pR; x += s) {
    context.moveTo(x, pT)
    context.lineTo(x, MAP_HEIGHT - pB)
  }
  for (var y = pT; y <= MAP_HEIGHT - pB; y += s) {
    context.moveTo(pL, y)
    context.lineTo(MAP_WIDTH - pR, y)
  }
  context.stroke()
  context.strokeStyle = "#fffff";
  context.strokeRect(8, 8, MAP_WIDTH - 8, MAP_HEIGHT - 8);
  for (let i = 0; i < gameObjects.length; i++) {
    gameObjects[i].draw(context);
  }
  camera.moveTo(player.x, player.y);
  camera.end();
}

function drawFpsCounter() {
  context.lineWidth = 8;
  context.fillStyle = "#292d3e";
  context.fillRect(0, 0, canvas.width, canvas.height);

  context.font = '25px Arial';
  context.fillStyle = 'white';
  context.fillText("FPS: " + fps, 10, 30);
}
function detectEdgeCollisions() {
  let entity;
  for (let i = 0; i < gameObjects.length; i++) {
    entity = gameObjects[i];

    // Check for left and right
    if (entity.x < entity.size) {
      entity.vx = Math.abs(entity.vx) * restitution;
      entity.x = entity.size;
    } else if (entity.x > MAP_WIDTH - entity.size) {
      entity.vx = -Math.abs(entity.vx) * restitution;
      entity.x = MAP_WIDTH - entity.size;
    }

    // Check for bottom and top
    if (entity.y < entity.size) {
      entity.vy = Math.abs(entity.vy) * restitution;
      entity.y = entity.size;
    } else if (entity.y > MAP_HEIGHT - entity.size) {
      entity.vy = -Math.abs(entity.vy) * restitution;
      entity.y = MAP_HEIGHT - entity.size;
    }
  }
}
function detectCollisions() {

  let a;
  let b;

  for (let i = 0; i < gameObjects.length; i++) {
    gameObjects[i].inCollision = false;
  }

  for (let i = 0; i < gameObjects.length; i++) {
    a = gameObjects[i];
    for (let j = i + 1; j < gameObjects.length; j++) {
      {
        b = gameObjects[j];

        if (a.checkCollision_Rec(b)) {
          a.inCollision = true;
          b.inCollision = true;

          let vCollision = { x: b.x - a.x, y: b.y - a.y };
          let distance = Math.sqrt((b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y));
          let vCollisionNorm = { x: vCollision.x / distance, y: vCollision.y / distance };
          let vRelativeVelocity = { x: a.vx - b.vx, y: a.vy - b.vy };
          let speed = vRelativeVelocity.x * vCollisionNorm.x + vRelativeVelocity.y * vCollisionNorm.y;
          speed *= 0.5;//Math.min(obj1.restitution, obj2.restitution);
          if (speed < 0) {
            break;
          }
          let impulse = 2 * speed / (a.size + b.size);
          a.vx -= (impulse * b.size * vCollisionNorm.x);
          a.vy -= (impulse * b.size * vCollisionNorm.y);
          b.vx += (impulse * a.size * vCollisionNorm.x);
          b.vy += (impulse * a.size * vCollisionNorm.y);

          if (a.isPlayer || b.isPlayer) {
            a.health--;
            b.health--;
          }
        }
      }
    }
  }
}
