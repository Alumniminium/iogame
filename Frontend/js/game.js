import { Net } from "./network.js";
import { Input } from "./input.js";
import { Player,YellowCube } from "./player.js";
import { Camera} from "./camera.js"

window.addEventListener('resize', setCanvasDimensions);
const canvas = document.getElementById('gameCanvas');
const context = canvas.getContext('2d');
const camera = new Camera(context);

var net = new Net();
net.connect();

var input = new Input();
input.setup();

var player = new Player();
var yellowCube = new YellowCube();

const MAP_SIZE = 1000;
let renderInterval = setInterval(render, 1000 / 60);

setCanvasDimensions();

function setCanvasDimensions() {
  const scaleRatio = Math.max(1, 200 / window.innerWidth);
  canvas.width = scaleRatio * window.innerWidth;
  canvas.height = scaleRatio * window.innerHeight;
}


function getCurrentState() {
  player.x = Math.max(0, player.x + input.dx * player.speed);
  player.y = Math.max(0, player.y + input.dy * player.speed);
  player.x = Math.min(MAP_SIZE, player.x + input.dx * player.speed);
  player.y = Math.min(MAP_SIZE, player.y + input.dy * player.speed);
  return player;
}

function render() {
  renderBackground();
  camera.begin();
  const me = getCurrentState();
  camera.moveTo(me.x, me.y);

  if (player.checkCollision_Rec(yellowCube))
  {
    yellowCube.x += input.dx * player.speed;
    yellowCube.y += input.dy * player.speed;
  }

  renderPlayer(me);
  camera.end();
}

function renderBackground() {
  context.lineWidth = 8;
  context.fillStyle = "#292d3e"
  context.fillRect(0, 0, canvas.width, canvas.height);
}

// Renders a ship at the given coordinates
function renderPlayer(player) {
  context.fillStyle = "#bfae4e"
  context.strokeRect(0, 0, MAP_SIZE - 8, MAP_SIZE + 8);
  yellowCube.draw(context);
  player.draw(context);

  // Draw health bar
  //context.fillStyle = 'white';
  //context.fillRect(canvasX - player.size / 2, canvasY - player.size/2, player.size * 2, 12);
  //context.fillStyle = 'red';
  //context.fillRect(canvasX - player.size / 2,canvasY - player.size/2, player.size*2 / 100 * (100 * player.health / player.maxHealth),12);
}