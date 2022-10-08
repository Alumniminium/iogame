import { Entity } from "./Entity.js";
import { Vector } from "../Vector.js";

export class Player extends Entity
{
    name = "player";
    constructor(id, name, x, y)
    {
        super(id);
        this.name = name;
        this.position = new Vector(x, y);
        this.health = 100;
        this.maxHealth = 100;
        this.fillColor = "#008dba";
        this.borderColor = "#005e85";
        this.lastShot = new Date().getTime();
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

    update(dt)
    {
        super.update(dt);
    }
}
