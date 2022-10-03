import { Entity } from "./Entity.js";
import { Vector } from "../Vector.js";

export class TriangleEntity extends Entity
{
    constructor(id, x, y, rot, w,h, color)
    {
        super(id);
        this.fillColor = color;
        this.position = new Vector(x, y);
        this.serverPosition = new Vector(x, y);
        this.width = w;
        this.height = h;
        this.rotation = rot + Math.PI;
    }

    draw(ctx)
    {
        ctx.fillStyle = this.fillColor;
        ctx.lineWidth = 2;

        ctx.beginPath();
        // rotation
        ctx.translate(this.position.x, this.position.y);
        ctx.rotate(this.rotation);
        
        ctx.moveTo(-this.width/2, -this.height/2);
        ctx.lineTo(this.width/2, -this.height/2);
        ctx.lineTo(0, this.height/2);
        ctx.lineTo(-this.width/2, -this.height/2);
        ctx.fill();
        ctx.rotate(-this.rotation);
        ctx.translate(-this.position.x, -this.position.y);
        super.draw(ctx);
    }
}
