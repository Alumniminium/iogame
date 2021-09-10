import { Net } from "./network.js";
import { Input } from "./input.js";
import { Player } from "./entities/player.js";
import { PurplePentagon, YellowSquare } from "./entities/yellowSquare.js";
import { Camera } from "./camera.js"
import { Entity } from "./entities/entity.js";
import { Vector } from "./vector.js";

const random = (min, max) => Math.floor(Math.random() * (max - min)) + min;

const MAP_WIDTH = 1000;
const MAP_HEIGHT = 1000;
window.addEventListener('resize', setCanvasDimensions);
const canvas = document.getElementById('gameCanvas');
const context = canvas.getContext('2d');
const camera = new Camera(context);
let secondsPassed;
let oldTimeStamp;
let fps;
const restitution = 0.5

var player = new Player();

let gameObjects =
[
  player
];


var net = new Net();
net.connect();
net.Connected = () => 
{

}
net.OnPacket = (p) =>
{
    console.log("packet");
};


for (let i = 0; i < 50; i++)
{
  gameObjects.push(new PurplePentagon(random(1, MAP_WIDTH), random(1, MAP_HEIGHT), random(-3, 4), random(-3, 4)));
  gameObjects.push(new YellowSquare(random(1, MAP_WIDTH), random(1, MAP_HEIGHT), random(-3, 4), random(-3, 4)));
}
setCanvasDimensions();
window.requestAnimationFrame((timeStamp) => { gameLoop(timeStamp) });

function setCanvasDimensions() {
  const scaleRatio = Math.max(1, 1000 / window.innerWidth);
  canvas.width = scaleRatio * window.innerWidth;
  canvas.height = scaleRatio * window.innerHeight;
}


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

  context.lineWidth = 8;
  context.fillStyle = "#292d3e";
  context.fillRect(0, 0, canvas.width, canvas.height);

  camera.begin();
  drawGridLines();
  context.strokeStyle = "#fffff";
  context.strokeRect(8, 8, MAP_WIDTH - 8, MAP_HEIGHT - 8);

  for (let i = 0; i < gameObjects.length; i++) {
    var entity = gameObjects[i];
    if (entity.position.x > camera.viewport.left && entity.position.x < camera.viewport.right) {
      if (entity.position.y > camera.viewport.top && entity.position.y < camera.viewport.bottom) {
        gameObjects[i].draw(context);
      }
    }
  }

  camera.moveTo(player.position.x, player.position.y);
  camera.end();
  drawFpsCounter();
}

function drawGridLines() {
  let s = 28;
  let pL = 20;
  let pT = 20;
  let pR = 20;
  let pB = 20;
  context.lineWidth = 1;
  context.strokeStyle = '#232735';
  context.beginPath();
  for (var x = pL; x <= MAP_WIDTH - pR; x += s) {
    context.moveTo(x, pT);
    context.lineTo(x, MAP_HEIGHT - pB);
  }
  for (var y = pT; y <= MAP_HEIGHT - pB; y += s) {
    context.moveTo(pL, y);
    context.lineTo(MAP_WIDTH - pR, y);
  }
  context.stroke();
}

function drawFpsCounter() {
  context.font = '25px Arial';
  context.fillStyle = 'white';
  context.fillText("FPS: " + fps, 10, 30);
}
function detectEdgeCollisions() {
  let entity;
  for (let i = 0; i < gameObjects.length; i++) {
    gameObjects[i].inCollision = false;
    entity = gameObjects[i];

    // Check for left and right
    if (entity.position.x < entity.size) {
      entity.velocity.x = Math.abs(entity.velocity.x) * restitution;
      entity.position.x = entity.size;
    } else if (entity.position.x > MAP_WIDTH - entity.size) {
      entity.velocity.x = -Math.abs(entity.velocity.x) * restitution;
      entity.position.x = MAP_WIDTH - entity.size;
    }

    // Check for bottom and top
    if (entity.position.y < entity.size) {
      entity.velocity.y = Math.abs(entity.velocity.y) * restitution;
      entity.position.y = entity.size;
    } else if (entity.position.y > MAP_HEIGHT - entity.size) {
      entity.velocity.y = -Math.abs(entity.velocity.y) * restitution;
      entity.position.y = MAP_HEIGHT - entity.size;
    }
  }
}
function detectCollisions() {

  let a;
  let b;

  for (let i = 0; i < gameObjects.length; i++) {
    a = gameObjects[i];
    for (let j = i + 1; j < gameObjects.length; j++) {
      {
        b = gameObjects[j];

        if (a.checkCollision_Rec(b)) {
          a.inCollision = true;
          b.inCollision = true;

          let collision = Vector.subtract(b.position, a.position);
          let distance = Vector.distance(b.position, a.position);
          let collisionNormalized = collision.divide(distance);
          let relativeVelocity = Vector.subtract(a.velocity, b.velocity);
          let speed = Vector.dot(relativeVelocity, collisionNormalized);

          //speed *= 0.5;
          if (speed < 0) {
            break;
          }

          let impulse = 2 * speed / (a.size + b.size);
          var fa = new Vector(impulse * b.size * collisionNormalized.x, impulse * b.size * collisionNormalized.y);
          var fb = new Vector(impulse * a.size * collisionNormalized.x, impulse * a.size * collisionNormalized.y);

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
