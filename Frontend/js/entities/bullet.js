import { Entity } from "./entity.js";
import { Vector } from "../vector.js";

export class Bullet extends Entity {
    owner = null;
    sides = 16;
    step = 2 * Math.PI / this.sides;

    constructor(id) {
        super(id);
        this.size = 50;
        this.health = 100;
        this.fillColor = "#ffe869";
        this.borderColor = "#bfae4e";
    }

    update(dt) {
        super.update(dt);
    }

    draw(ctx) {
        super.draw(ctx);
        ctx.fillStyle = this.fillColor;
        ctx.strokeStyle = this.borderColor;

        ctx.beginPath();
        ctx.arc(this.originX(), this.originY(), this.radius, 0, Math.PI * 2);
        ctx.fillStyle = this.fillColor;
        ctx.fill();
        ctx.strokeStyle = this.borderColor;
        ctx.stroke();
    }
}
