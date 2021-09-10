import { Entity } from "./entity.js";
import { Vector } from "../vector.js";

export class YellowSquare extends Entity {

    sides = 4;
    constructor(x, y, vX, vY) {
        super();
        this.position = new Vector(x, y);
        this.velocity = new Vector(vX, vY);
        this.speed = 8;
        this.size = 30;
        this.health = 100;
        this.fillColor = "#ffe869";
        this.borderColor = "#bfae4e";
    }

    update(dt) {
        super.update(dt);

        if (!this.inCollision) {
            if (Math.abs(this.velocity.x) > 0.05)
                this.velocity.x *= 0.999;
            if (Math.abs(this.velocity.y) > 0.05)
                this.velocity.y *= 0.999;
        }
        var dx = this.velocity.x;
        var dy = this.velocity.y;

        if (isNaN(dx) || isNaN(dy))
            return;

        this.position.add(this.velocity);
    }

    draw(ctx) {
        super.draw(ctx);
        var Xcenter = this.originX(),//this.position.x,
            Ycenter = this.originY(),//this.position.y,
            step = 2 * Math.PI / this.sides,//Precalculate step value
            shift = (Math.PI / 180.0) * this.direction;//Quick fix ;)

        ctx.beginPath();
        //ctx.moveTo (Xcenter +  size * Math.cos(0), Ycenter +  size *  Math.sin(0));          

        for (var i = 0; i <= this.sides; i++) {
            var curStep = i * step + shift;
            ctx.lineTo(Xcenter + this.size / 2 * Math.cos(curStep), Ycenter + this.size / 2 * Math.sin(curStep));
        }

        ctx.strokeStyle = "#9370DB";
        ctx.fillStyle = this.inCollision ? "#990000" : this.borderColor;
        ctx.lineWidth = 1;
        ctx.stroke();
        ctx.fill();
    }
}


