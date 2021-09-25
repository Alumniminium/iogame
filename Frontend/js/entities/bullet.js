import { Entity } from "./entity.js";
import { Vector } from "../vector.js";

export class Bullet extends Entity {
    owner = null;
    sides = 16;
    step = 2 * Math.PI / this.sides;

    constructor(id) {
        super(id);
        this.size = 50;
        this.health = 100;
        this.fillColor = "#ffe869";
        this.borderColor = "#bfae4e";
    }

    update(dt) {
        super.update(dt);
    }

    draw(ctx) {
        super.draw(ctx);
        this.DrawShape(ctx,this);   
        console.log('bullet');
    }
}
