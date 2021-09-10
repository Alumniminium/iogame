import { Vector } from "../vector.js";

export class Entity {
    isPlayer = false;
    position = new Vector(0, 0);
    velocity = new Vector(0, 0);
    direction = 40;
    size = 1;
    health = 10;
    maxHealth = 10;
    fillColor = "#ffe869";
    borderColor = "#bfae4e";

    originX = function () { return this.position.x + this.size / 2; }
    originY = function () { return this.position.y + this.size / 2; }

    update(timeStamp)
    {
        //let radians = Math.atan2(this.velocity.x, this.velocity.x);
        //this.direction = 180 * radians / Math.PI;
    }

    
    checkCollision_Rec(entity) {
        return (this.position.x < entity.position.x + entity.size && this.position.x + this.size > entity.position.x && this.position.y < entity.position.y + entity.size && this.position.y + this.size > entity.position.y);
    }
}