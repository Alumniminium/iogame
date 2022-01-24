import { Vector } from "../vector.js";
import { Entity } from "./entity.js";

export class Line extends Entity {

    from = new Vector(0,0);
    to = new Vector(0,0);
    created = new Date().getTime();
    constructor(id,from,to) {
        super(id);
        this.position = from;
        this.from = from;
        this.to=to;
    }

    draw(ctx)
    {
        ctx.fillStyle = this.fillColor;
        ctx.strokeStyle = this.fillColor;
        ctx.lineWidth = 1;
                
        ctx.beginPath();
        ctx.moveTo(this.from.x,this.from.y);
        ctx.lineTo(this.to.x,this.to.y);
        
        ctx.stroke();
        ctx.fill();

        if(this.created + 1000 < new Date().getTime())
            window.game.removeEntity(this);
    }
}
