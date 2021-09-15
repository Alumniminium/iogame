import { Entity } from "./entity.js";
import { Vector } from "../vector.js";

export class BlueCircle extends Entity {

    sides = 0;
    step = 2 * Math.PI / this.sides;
    
    constructor(id, x, y, vX, vY) {
        super(id);
        this.position = new Vector(x, y);
        this.velocity = new Vector(vX, vY);
        this.size = 30;
        this.sizeHalf=this.size/2;
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
        ctx.arc(this.originX(), this.originY(), this.sizeHalf, 0, Math.PI * 2);
        ctx.fillStyle = this.fillColor;
        ctx.fill();
        ctx.strokeStyle = this.borderColor;
        ctx.stroke();

        // Draw health bar
        ctx.fillStyle = 'white';
        ctx.fillRect(this.originX() - this.size * 1.5, this.originY() - this.sizeHalf, this.size * 3, 4);
        ctx.fillStyle = 'red';
        ctx.fillRect(this.originX() - this.size * 1.5, this.originY() - this.sizeHalf, (this.size * 3) / 100 * (100 * this.health / this.maxHealth), 4);
        ctx.fillStyle = 'white';
        let nameTag = "Id: " + this.id + " - " + this.name;
        let textSize = ctx.measureText(nameTag);
        ctx.fillText(nameTag, this.originX() - textSize.width / 2, this.originY() - this.size * 1.5);
    }
}


