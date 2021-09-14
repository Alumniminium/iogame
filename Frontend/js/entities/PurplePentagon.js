import { Entity } from "./entity.js";
import { Vector } from "../vector.js";



export class PurplePentagon extends Entity {

    sides = 5;
    step = 2 * Math.PI / this.sides;

    constructor(id, x, y, vX, vY) {
        super(id);
        this.position = new Vector(x, y);
        this.velocity = new Vector(vX, vY);
        this.speed = 8;
        this.size = 50;
        this.health = 100;
        this.fillColor = "#4B0082";
        this.borderColor = "#9370DB";
    }


    update(dt) {
        super.update(dt);
        this.position.add(this.velocity);
    }

    draw(ctx) {
        super.draw(ctx);
        super.DrawShape(ctx,this);
    }
}
