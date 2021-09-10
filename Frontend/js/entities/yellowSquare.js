import { Entity } from "./entity.js";
import { Vector } from "../vector.js";

export class YellowSquare extends Entity {
    inCollision = false;

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

    update(secondsPassed) {
        super.update(secondsPassed);
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
        //ctx.fillStyle = "#ffffff";
        //ctx.fillText(`vX: ${this.velocity.x.toFixed(2)} vY: ${this.velocity.y.toFixed(2)}`, this.position.x, this.position.y - this.size);
        ctx.fillStyle = this.inCollision ? "#990000" : this.borderColor;
        ctx.fillRect(this.position.x, this.position.y, this.size, this.size);
        ctx.fillStyle = this.inCollision ? "#ff4d4d" : this.borderColor;
        ctx.fillRect(4 + this.position.x, 4 + this.position.y, this.size - 8, this.size - 8);
    }
}


export class PurplePentagon extends YellowSquare {

    constructor(x, y, vX, vY) {
        super();
        this.position = new Vector(x, y);
        this.velocity = new Vector(vX, vY);
        this.speed = 8;
        this.size = 50;
        this.health = 100;
        this.fillColor = "#ffe869";
        this.borderColor = "#bfae4e";
    }

    update(dt) {
        super.update(dt);
        if (this.direction < 360)
            this.direction += 10*dt;
        else
            this.direction = 0;
    }

    draw(ctx) {
        var numberOfSides = 5,
            Xcenter = this.originX(),//this.position.x,
            Ycenter = this.originY(),//this.position.y,
            step = 2 * Math.PI / numberOfSides,//Precalculate step value
            shift = (Math.PI / 180.0) * this.direction;//Quick fix ;)

        ctx.beginPath();
        //ctx.moveTo (Xcenter +  size * Math.cos(0), Ycenter +  size *  Math.sin(0));          

        for (var i = 0; i <= numberOfSides; i++) {
            var curStep = i * step + shift;
            ctx.lineTo(Xcenter + this.size/2 * Math.cos(curStep), Ycenter + this.size/2 * Math.sin(curStep));
        }

        ctx.strokeStyle = "#9370DB";
        ctx.fillStyle = "#4B0082";
        ctx.lineWidth = 1;
        ctx.stroke();
        ctx.fill();
    }
}