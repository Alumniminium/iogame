import { Entity } from "./entity.js";
import { Vector } from "../vector.js";

export class YellowSquare extends Entity {

    sides = 4;
    step = 2 * Math.PI / this.sides;

    constructor(id, x, y, vX, vY) {
        super(id);
        this.position = new Vector(x, y);
        this.velocity = new Vector(vX, vY);
        this.size = 200;
        this.health = 100;
    }

    update(dt) {
        super.update(dt);
    }

    draw(ctx) {
        super.draw(ctx);
        super.DrawShape(ctx, this);
    }
}