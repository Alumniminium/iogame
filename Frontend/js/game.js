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
    this.camera.moveTo(this.player.origin());
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

    for (let i = 0; i < this.entitiesArray.length; i++)
      this.entitiesArray[i].inCollision = false;

    for (let i = 0; i < this.entitiesArray.length; i++) {
      const a = this.entitiesArray[i];

      if (a.InCollision)
        continue;

      for (let j = i; j < this.entitiesArray.length; j++) {
        const b = this.entitiesArray[j];

        if (a == b || b.InCollisio)
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

          let impulse = 2 * speed / (a.mass + b.mass);
          let fa = new Vector(impulse * b.mass * collisionNormalized.x, impulse * b.mass * collisionNormalized.y);
          let fb = new Vector(impulse * a.mass * collisionNormalized.x, impulse * a.mass * collisionNormalized.y);

          a.velocity.subtract(fa);
          b.velocity.add(fb);

          // console.log(`a=${a.id}: ${fa.x},${fa.y}`);
          // console.log(`b=${b.id}: ${fb.x},${fb.y}`);

          // if (a.isPlayer || b.isPlayer) {
          // if (a.health > 0)
          //   a.health--;
          // else
          //   this.removeEntity(a);
          // if (b.health > 0)
          //   b.health--;
          // else
          //   this.removeEntity(b);
          // }
        }
      }
      if (a.origin().x < a.radius()) {
        a.velocity.x = Math.abs(a.velocity.x) * a.drag;
        a.position.x = a.radius();
      } else if (a.origin().x > this.MAP_WIDTH - a.size) {
        a.velocity.x = -Math.abs(a.velocity.x) * a.drag;
        a.position.x = this.MAP_WIDTH - a.size;
      }

      if (a.origin().y < a.radius()) {
        a.velocity.y = Math.abs(a.velocity.y) * a.drag;
        a.position.y = a.radius();
      } else if (a.origin().y > this.MAP_HEIGHT - a.size) {
        a.velocity.y = -Math.abs(a.velocity.y) * a.drag;
        a.position.y = this.MAP_HEIGHT - a.size;
      }
      // a.inCollision = false;
    }
  }
}
var game = new Game();