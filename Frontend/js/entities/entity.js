import { Vector } from "../vector.js";

const random = (min, max) => Math.floor(Math.random() * (max - min)) + min;

export class Entity {
    inCollision = false;
    isPlayer = false;
    position = new Vector(0, 0);
    velocity = new Vector(0, 0);
    direction = random(0,361);
    size = 1;
    health = 10;
    maxHealth = 10;
    fillColor = "#ffe869";
    borderColor = "#bfae4e";

    constructor()
    {
        this.direction = random(0, 361);
        console.log(this.direction);
    }

    originX = function () { return this.position.x + this.size / 2; }
    originY = function () { return this.position.y + this.size / 2; }

    update(timeStamp)
    {
        let radians = Math.atan2(this.velocity.x, this.velocity.x);
        this.direction = 180 * radians / Math.PI;
    }

    draw(ctx)
    {
        ctx.strokeStyle = "magenta";
        ctx.moveTo(this.originX(), this.originY());
        ctx.lineTo(this.originX() + this.velocity.x * 50, this.originY() + this.velocity.y * 50);
        ctx.stroke();
    }

    
    checkCollision_Rec(entity) {
        return (this.position.x < entity.position.x + entity.size && this.position.x + this.size > entity.position.x && this.position.y < entity.position.y + entity.size && this.position.y + this.size > entity.position.y);
    } 
    checkCollision_Circle(entity) {
        var distX = this.originX() - entity.originX();
        var distY = this.originY() - entity.originY();
        var distance = Math.sqrt((distX * distX) + (distY * distY));
        return distance < this.size/2 + entity.size/2;
    }
}