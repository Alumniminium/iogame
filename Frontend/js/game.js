import { Net } from "./network/network.js";
import { Player } from "./entities/player.js";
import { Vector } from "./vector.js";
import { renderer } from "./renderer.js";
import { Camera } from "./camera.js";

export class Game {
  random = (min, max) => Math.floor(Math.random() * (max - min)) + min;

  MAP_WIDTH = -1;
  MAP_HEIGHT = -1;

  secondsPassed;
  oldTimeStamp = 0;
  totalTime = 0;

  entities = new Map();
  entitiesArray = [];

  player = null;
  net = null;
  renderer = null;
  camera = null;

  constructor() {

    let canvas = document.getElementById('gameCanvas');
    let context = canvas.getContext('2d');

    this.player = new Player(this, 0, "Player Name", 211, 211);
    this.camera = new Camera(context, this.player);
    this.renderer = new renderer(this, this.camera);
    this.net = new Net(this);
    this.net.connect();
    window.requestAnimationFrame(dt => this.gameLoop(dt));
  }

  gameLoop(dt) {
    this.secondsPassed = (dt - this.oldTimeStamp) / 1000;
    this.oldTimeStamp = dt;

    this.update(this.secondsPassed)
    this.renderer.draw(this.secondsPassed);

    window.requestAnimationFrame(dt => this.gameLoop(dt));
  }

  update(dt) {
    this.renderer.update(dt);
    for (let i = 0; i < this.entitiesArray.length; i++) {
      const entity = this.entitiesArray[i];
      entity.update(dt);

      if (!this.camera.canSee(entity))
        this.removeEntity(entity);
    }
    this.camera.moveTo(this.player.position);
    this.detectCollisions(dt);

    // this.renderer.camera.moveTo(this.player.origin());
  }


  addEntity(entity) {
    // console.log(`adding entity ${entity.id}`);
    if (!this.entities.has(entity.id)) {
      this.entities.set(entity.id, entity);
      this.entitiesArray.push(entity);
    }
  }

  removeEntity(entity) {
    const id = entity.id;
    // console.log(`removing entity ${id}`);
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

  detectCollisions() {
    this.entitiesArray.forEach((a, index) => {
      for (let i = index + 1; i < this.entitiesArray.length; i++) {
        const b = this.entitiesArray[i];
        if (a.checkCollision_Circle(b)) {

          let dist = Vector.subtract(a.position, b.position);
          let pen_depth = a.radius() + b.radius() - dist.magnitude();
          let pen_res = Vector.multiply(dist.unit(),pen_depth / (a.inverseMass + b.inverseMass));
          a.position.add(Vector.multiply(pen_res, a.inverseMass));
          b.position.add(Vector.multiply(pen_res, -b.inverseMass));

          let normal = Vector.subtract(a.position, b.position).unit();
          let relVel = Vector.subtract(a.velocity,b.velocity);
          let sepVel = Vector.dot(relVel, normal);
          let new_sepVel = -sepVel * Math.min(a.elasticity, b.elasticity);
          let vsep_diff = new_sepVel - sepVel;
          let impulse = vsep_diff / (a.inverseMass + b.inverseMass);
          let impulseVec = Vector.multiply(normal,impulse);

          a.velocity.add(Vector.multiply(impulseVec, a.inverseMass));
          b.velocity.add(Vector.multiply(impulseVec, -b.inverseMass));
        }
      }
    });
  }
}
window.game = new Game();