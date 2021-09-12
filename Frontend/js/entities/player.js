import { Entity } from "./entity.js";
import { Input } from "../input.js";
import { Vector } from "../vector.js";

export class Player extends Entity {

    name = "player";

    input = new Input();
    constructor(x,y) {
        super();
        this.position = new Vector(x,y);
        this.isPlayer = true;
        this.size = 30;
        this.input.setup();
        this.fillColor = "#00b2e1";
        this.borderColor = "#20bae9";
        this.speed = 10;
        this.health = 10;
        this.maxHealth = 10;
    }
    draw(ctx) {
        ctx.beginPath();
        ctx.arc(this.position.x + this.size / 2, this.position.y + this.size / 2, this.size / 2, 0, Math.PI * 2);
        ctx.fillRect(this.position.x, this.position.y, this.size, this.size);
        ctx.fillStyle = this.fillColor;
        ctx.fill();
        ctx.strokeStyle = this.borderColor;
        ctx.stroke();

        // Draw health bar
        ctx.fillStyle = 'white';
        ctx.fillRect(this.position.x - this.size, this.position.y - this.size, this.size * 3, 4);
        ctx.fillStyle = 'red';
        ctx.fillRect(this.position.x - this.size, this.position.y - this.size, (this.size * 3) / 100 * (100 * this.health / this.maxHealth), 4);

    }
    update(dt) 
    {
        super.update(dt);

        const inputVector = new Vector(0, 0);
        if (this.input.left)
            inputVector.x--;
        else if (this.input.right)
            inputVector.x++;

        if (this.input.up)
            inputVector.y--;
        else if (this.input.down)
            inputVector.y++;

        let inputVector = Vector.clampMagnitude(inputVector, 1);
        inputVector.multiply(this.speed * dt);

        this.velocity.add(inputVector);

        if (this.health < this.maxHealth)
            this.health += 10 * dt;

        this.velocity.x *= 0.95;
        this.velocity.y *= 0.95;

        this.position.add(this.velocity);
    }
}
