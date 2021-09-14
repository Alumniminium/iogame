import { Vector } from "../vector.js";

const random = (min, max) => Math.floor(Math.random() * (max - min)) + min;

export class Entity {
    id = 0;
    inCollision = false;
    isPlayer = false;
    position = new Vector(0, 0);
    velocity = new Vector(0, 0);
    serverPosition = new Vector(0, 0);
    serverVelocity = new Vector(0, 0);
    direction = random(0,361);
    size = 1;
    health = 10;
    maxHealth = 10;
    fillColor = "#ffe869";
    borderColor = "#bfae4e";

    constructor(id)
    {
        this.id = id;
        this.direction = random(0, 361);
    }

    originX = function () { return this.position.x + this.size / 2; }
    originY = function () { return this.position.y + this.size / 2; }

    update(dt)
    {
        // if (this.serverVelocity != this.velocity)
        //     Vector.Lerp(this.velocity, this.serverVelocity, dt*2);

        if (isNaN(this.velocity.x) || isNaN(this.velocity.y)) {
            this.velocity.x = 0;
            this.velocity.y = 0;
            return;
        }

        if (!this.inCollision) {
            if (Math.abs(this.velocity.x) > 0.05)
                this.velocity.x *= 0.9999;
            if (Math.abs(this.velocity.y) > 0.05)
                this.velocity.y *= 0.9999;
        }

        let velocity = Vector.multiply(this.velocity, dt);
        this.position.add(velocity);

        let radians = Math.atan2(this.velocity.x, this.velocity.x);
        this.direction = 180 * radians / Math.PI;
        if (this.serverPosition != this.position)
            this.position = Vector.Lerp(this.position, this.serverPosition, dt * 2);
    }

    draw(ctx)
    {
        ctx.strokeStyle = "magenta";
        ctx.moveTo(this.originX(), this.originY());
        ctx.lineTo(this.originX() + this.velocity.x, this.originY() + this.velocity.y);
        ctx.stroke();
    }

    
    checkCollision_Rec(entity) {
        return (this.position.x < entity.position.x + entity.size && this.position.x + this.size > entity.position.x && this.position.y < entity.position.y + entity.size && this.position.y + this.size > entity.position.y);
    } 
    checkCollision_Circle(entity) {
        let distX = this.originX() - entity.originX();
        let distY = this.originY() - entity.originY();
        let distance = Math.sqrt(distX * distX + distY * distY);
        return distance < this.size/2 + entity.size/2;
    }

    DrawShape(ctx, entity) {
        ctx.fillStyle = this.inCollision ? "#990000" : this.fillColor;
        ctx.strokeStyle = entity.borderColor;
        const shift = (Math.PI / 180.0) * entity.direction;
        ctx.beginPath();
        for (let i = 0; i <= entity.sides; i++) {
            let curStep = i * entity.step + shift;
            ctx.lineTo(entity.originX() + entity.size / 2 * Math.cos(curStep), entity.originY() + entity.size / 2 * Math.sin(curStep));
        }
        ctx.lineWidth = 1;
        ctx.stroke();
        ctx.fill();
    }
}