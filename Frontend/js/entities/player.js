import { Entity } from "./entity.js";
import { Input } from "../input.js";
import { Vector } from "../vector.js";

export class Player extends Entity {

    name = "player";

    input = new Input();
    constructor() {
        super();
        this.isPlayer = true;
        this.size = 30;
        this.input.setup();
        this.fillColor = "#00b2e1";
        this.borderColor = "#20bae9";
        this.speed = 10;
        this.health = 100;
        this.maxHealth = 100;
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
    update(secondsPassed) {
        super.update(secondsPassed);

        var inputVector = new Vector(0,0);
        if (this.input.left)
            inputVector.x--;
        else if (this.input.right)
            inputVector.x++;

        if (this.input.up)
            inputVector.y--;
        else if (this.input.down)
            inputVector.y++;

    

        if (isNaN(this.velocity.x) || isNaN(this.velocity.y)) {
            this.velocity.x = 0;
            this.velocity.y = 0;
            return;
        }

        if (this.health < this.maxHealth)
            this.health += 10 * secondsPassed;

        var inputVector = Vector.clampMagnitude(inputVector,1);
        inputVector.multiply(this.speed * secondsPassed);

        this.velocity.add(inputVector);
        

        this.velocity.multiply(0.95);

        // console.log(`PLoc: x=${this.position.x}, y=${this.position.y} Vel: x=${this.velocity.x}, y=${this.velocity.y}`)
        this.position = Vector.add(this.velocity, this.position);
    }
}
