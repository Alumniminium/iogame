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
        this.size = 400;
        this.health = 100;
    }


    update(dt) {
        super.update(dt);
    }

    draw(ctx) {
        super.draw(ctx);
        super.DrawShape(ctx,this);
    }
}
