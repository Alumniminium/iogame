import { Entity } from "./Entity.js";
import { Vector } from "../Vector.js";

export class BoxEntity extends Entity {

    constructor(id,x,y,w,h,r, color) {
        super(id);
        this.fillColor = color;
        this.position = new Vector(x,y);
        this.serverPosition = new Vector(x,y);
        this.rotation = r;
        this.width = w;
        this.height = h;
        this.size = w;
    }

    draw(ctx)
    {
        ctx.fillStyle = this.fillColor;
        ctx.lineWidth = 2;
        
        ctx.beginPath();
        ctx.translate(this.position.x, this.position.y);
        ctx.rotate(this.rotation);
        ctx.rect(-this.width/2, -this.height/2, this.width, this.height);
        
        ctx.fill();
        ctx.rotate(-this.rotation);
        ctx.translate(-this.position.x, -this.position.y);
        super.draw(ctx);
    }
}
