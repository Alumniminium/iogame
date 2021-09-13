import { Entity } from "./entity.js";
import { Vector } from "../vector.js";

export class YellowSquare extends Entity {

    sides = 4;
    constructor(id, x, y, vX, vY) {
        super(id);
        this.position = new Vector(x, y);
        this.velocity = new Vector(vX, vY);
        this.size = 30;
        this.health = 100;
        this.fillColor = "#ffe869";
        this.borderColor = "#bfae4e";
    }

    update(dt) {
        super.update(dt);
    }

    draw(ctx) {
        super.draw(ctx);
        const Xcenter = this.originX(),//this.position.x,
            Ycenter = this.originY(),//this.position.y,
            step = 2 * Math.PI / this.sides,//Precalculate step value
            shift = (Math.PI / 180.0) * this.direction;//Quick fix ;)

        ctx.beginPath();
        //ctx.moveTo (Xcenter +  size * Math.cos(0), Ycenter +  size *  Math.sin(0));          

        for (let i = 0; i <= this.sides; i++) {
            let curStep = i * step + shift;
            ctx.lineTo(Xcenter + this.size / 2 * Math.cos(curStep), Ycenter + this.size / 2 * Math.sin(curStep));
        }

        ctx.strokeStyle = "#9370DB";
        ctx.lineWidth = 1;
        ctx.stroke();
        ctx.fill();
    }
}


