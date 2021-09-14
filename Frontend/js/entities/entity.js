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
    direction = random(0, 361);
    size = 1;
    health = 10;
    maxHealth = 10;
    fillColor = "#ffe869";
    borderColor = "#bfae4e";

    constructor(id) {
        this.id = id;
        this.direction = random(0, 361);
    }
    origin = function () { return Vector.add(this.position, this.size / 2) }
    originX = function () { return this.position.x + this.size / 2; }
    originY = function () { return this.position.y + this.size / 2; }

    update(dt) {
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

        if (this.velocity.y > 0)
            this.direction += 0.5 * dt;
        else
            this.direction -= 0.5 * dt;
        if (this.direction > 360)
            this.direction = 0;
        if (this.direction < 0)
            this.direction = 360;
        if (this.serverPosition != this.position)
            this.position = Vector.Lerp(this.position, this.serverPosition, dt * 2);
    }

    draw(ctx) {
        ctx.strokeStyle = "magenta";
        ctx.moveTo(this.originX(), this.originY());
        ctx.lineTo(this.originX() + this.velocity.x, this.originY() + this.velocity.y);
        ctx.stroke();
    }

    checkCollision_Circle(entity) {
        let distance = Vector.distance(this.origin(), entity.origin());
        return distance < this.size / 2 + entity.size / 2;
    }

    DrawShape(ctx, entity) {
        ctx.fillStyle = this.inCollision ? "#990000" : this.fillColor;
        ctx.strokeStyle = entity.borderColor;
        const shift = entity.direction;
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