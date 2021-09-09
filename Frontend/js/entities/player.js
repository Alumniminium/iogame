import { Entity } from "./entity.js";
import { Input } from "../input.js";

export class Player extends Entity {

    name = "player";

    input = new Input();
    constructor() {
        super();
        this.isPlayer = true;
        this.size = 20;
        this.input.setup();
        this.fillColor = "#00b2e1";
        this.borderColor = "#20bae9";
        this.speed = 100;
        this.health = 100;
        this.maxHealth = 100;
    }
    draw(ctx) {
        ctx.beginPath();
        ctx.fillRect(this.x, this.y, this.size, this.size);
        ctx.fillStyle = this.fillColor;
        ctx.fill();
        ctx.strokeStyle = this.borderColor;
        ctx.stroke();

        // Draw health bar
        ctx.fillStyle = 'white';
        ctx.fillRect(this.x - this.size, this.y - this.size, this.size * 3, 12);
        ctx.fillStyle = 'red';
        ctx.fillRect(this.x - this.size, this.y - this.size, (this.size*3) /100 * (100 * this.health / this.maxHealth), 12);

    }
    update(secondsPassed) {
        super.update(secondsPassed);
        if (this.input.dx != 0)
            this.vx = this.input.dx * this.speed * secondsPassed;
        if (this.input.dy != 0)
            this.vy = this.input.dy * this.speed * secondsPassed;

        if (isNaN(this.vx) || isNaN(this.vy)) {
            this.vx = 0;
            this.vy = 0;
            return;
        }

        this.vx *= 0.9;
        this.vy *= 0.9;

        this.x += this.vx;
        this.y += this.vy;
    }
}
