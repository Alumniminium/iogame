import { Entity } from "./entity.js";

const random = (min, max) => Math.floor(Math.random() * (max - min)) + min;

export class YellowSquare extends Entity {
    inCollision = false;

    constructor(x, y, vX, vY) {
        super();
        this.x = x;
        this.y = y;
        this.vx = vX;
        this.vy = vY;
        this.speed = 8;
        this.size = 10;
        this.health = 10;
        this.fillColor = "#ffe869";
        this.borderColor = "#bfae4e";
    }

    update(secondsPassed) {
        super.update(secondsPassed);
        if (!this.inCollision) {
            if (Math.abs(this.vx) > 0.05)
                this.vx *= 0.98;
            if (Math.abs(this.vy) > 0.05)
                this.vy *= 0.98;
        }
        var dx = this.vx;
        var dy = this.vy;

        if (isNaN(dx) || isNaN(dy))
            return;

        this.x += dx;
        this.y += dy;
    }

    draw(ctx) {
        // ctx.translate(this.x, this.y);
        // ctx.rotate(Math.PI / 180 * (this.direction + 90));
        // ctx.translate(-this.x, -this.y);
        ctx.fillText(`vX: ${this.vx.toFixed(2)} vY: ${this.vy.toFixed(2)}`, this.x, this.y - this.size);
        ctx.fillStyle = this.inCollision ? "#990000" : this.borderColor;
        ctx.fillRect(this.x, this.y, this.size, this.size);
        ctx.fillStyle = this.inCollision ? "#ff4d4d" : this.borderColor;
        ctx.fillRect(4 + this.x, 4 + this.y, this.size - 8, this.size - 8);
        // ctx.setTransform(1, 0, 0, 1, 0, 0);
    }
}