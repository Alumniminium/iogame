import { Entity } from "./entity.js";
import { Vector } from "../vector.js";

export class PurpleOctagon extends Entity {

    constructor(x, y, vX, vY) {
        super();
        this.position = new Vector(x, y);
        this.velocity = new Vector(vX, vY);
        this.speed = 8;
        this.size = 200;
        this.health = 100;
        this.fillColor = "#9370DB";
        this.borderColor = "#4B0082";
    }


    update(dt) {
        super.update(dt);
        this.position.add(this.velocity);
    }

    draw(ctx) {
        super.draw(ctx);
        const numberOfSides = 8, Xcenter = this.originX(), //this.position.x,
            Ycenter = this.originY(), //this.position.y,
            step = 2 * Math.PI / numberOfSides, //Precalculate step value
            shift = (Math.PI / 180.0) * this.direction; //Quick fix ;)

        ctx.beginPath();
        //ctx.moveTo (Xcenter +  size * Math.cos(0), Ycenter +  size *  Math.sin(0));          
        for (let i = 0; i <= numberOfSides; i++) {
            let curStep = i * step + shift;
            ctx.lineTo(Xcenter + this.size / 2 * Math.cos(curStep), Ycenter + this.size / 2 * Math.sin(curStep));
        }

        ctx.strokeStyle = this.borderColor;
        ctx.lineWidth = 1;
        ctx.stroke();
        ctx.fill();

        ctx.fillStyle = "green";
        ctx.fillRect(this.originX(), this.originY(), 4, 4);
        ctx.strokeStyle = "black";
        ctx.arc(this.originX(), this.originY(), this.size/2, 0, Math.PI * 2, false);
    }
}
