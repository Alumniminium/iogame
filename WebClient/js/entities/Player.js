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
        this.size = 200;
        this.maxSpeed = 1500;
        this.health = 100;
        this.maxHealth = 100;
        this.fillColor = "#008dba";
        this.borderColor = "#005e85";
        this.lastShot = new Date().getTime();
    }
    draw(ctx)
    {
        super.draw(ctx);
    }

    update(dt)
    {
        super.update(dt);
    }
}
