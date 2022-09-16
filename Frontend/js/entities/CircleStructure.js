import { Entity } from "./entity.js";
import { Vector } from "../vector.js";

export class CircleStructure extends Entity
{
    constructor(id, x, y, rot, r, color)
    {
        super(id);
        this.fillColor = color;
        this.position = new Vector(x, y);
        this.size = r*2;
        this.rotation = rot;
    }

    draw(ctx)
    {
        ctx.fillStyle = this.fillColor;
        ctx.lineWidth = 2;

        ctx.beginPath();
        ctx.arc(this.position.x, this.position.y, this.radius, 0, 2 * Math.PI);

        ctx.fill();
    }
}
