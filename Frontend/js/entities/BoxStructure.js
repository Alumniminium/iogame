import { Entity } from "./entity.js";
import { Vector } from "../vector.js";


export class BoxStructure extends Entity {

    constructor(id,x,y,w,h,r) {
        super(id);
        this.position2 = new Vector(x,y);
        this.rotation = r;
        this.width = w;
        this.height = h;
    }

    draw(ctx)
    {
        console.log("Drawing BoxStructure");
        ctx.fillStyle = "black";
        ctx.lineWidth = 2;
                
        ctx.beginPath();
        //draw rectangle with rotation
        ctx.translate(this.position2.x, this.position2.y);
        ctx.rotate(this.rotation);
        ctx.rect(-this.width/2, -this.height/2, this.width, this.height);
        ctx.stroke();
        ctx.fill();
        ctx.rotate(-this.rotation);
        ctx.translate(-this.position2.x, -this.position2.y);
        
    }
}
