import { Entity } from "./entity.js";

export class Bullet extends Entity {

    constructor(id, owner) {
        super(id);
        this.owner = owner;
        this.size = 25;
        this.health = 100;
    }

    update(dt) {
        super.update(dt);
    }

    draw(ctx) {
        ctx.strokeStyle = this.owner.borderColor;
        // ctx.fillStyle = this.fillColor;
        ctx.beginPath();
        ctx.arc(this.position.x, this.position.y, this.radius, 0, Math.PI * 2);
        // ctx.stroke();
        ctx.fill();
    }
}
