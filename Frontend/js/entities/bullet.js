import { Vector } from "../vector.js";
import { Entity } from "./entity.js";

export class Bullet extends Entity {
    owner = null;
    sides = 4;
    step = 2 * Math.PI / this.sides;

    constructor(id) {
        super(id);
        this.size = 50;
        this.health = 100;
        this.fillColor = "#ffe869";
        this.borderColor = "#bfae4e";
    }

    update(dt) {
        if(new Date().getTime() > this.spawnTime + 1000)
            window.game.removeEntity(this);
        this.rotate(dt);
        let vel = Vector.multiply(this.velocity,dt);
        this.position.add(vel);
    }

    draw(ctx) {
        super.draw(ctx);
    }
}
