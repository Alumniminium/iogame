import { Entity } from "./entity.js";
import { Vector } from "../vector.js";


export class Asteroid extends Entity {

    points = [];
    constructor(id,x,y,points) {
        super(id);
        this.position.x = x;
        this.position.y = y;
        this.points = points;
    }

    draw(ctx)
    {
        ctx.fillStyle = this.fillColor;
        ctx.lineWidth = 2;
                
        ctx.beginPath();
        ctx.moveTo(this.points[0].x,this.points[0].y);

        for(const [x,y] of this.points)
            ctx.lineTo(x,y);
        
        ctx.stroke();
        ctx.fill();
    }
}
