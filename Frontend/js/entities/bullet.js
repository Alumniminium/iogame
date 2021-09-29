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
    }

    update(dt) {
        if(new Date().getTime() > this.spawnTime + 3000)
            window.game.removeEntity(this);
        this.rotate(dt);
        let vel = this.velocity.multiply(dt);
        this.position = this.position.add(vel);
    }

    draw(ctx) {
        ctx.strokeStyle = this.owner.borderColor;
        ctx.fillStyle = this.owner.fillColor;
        ctx.lineWidth = 25;
        ctx.beginPath();
        ctx.arc(this.position.x, this.position.y, this.radius, 0, Math.PI * 2);
        ctx.stroke();
        ctx.fill();
    }
}
