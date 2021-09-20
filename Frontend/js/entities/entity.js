import { Vector } from "../vector.js";

const random = (min, max) => Math.floor(Math.random() * (max - min)) + min;

export class Entity {
    id = 0;
    inCollision = false;
    isPlayer = false;
    restitution = 0.9;
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
        this.mass = Math.pow(this.size, 3);
        // this.restitution = /
    }

    radius = function() { return this.size / 2; }
    origin = function () { return new Vector(this.position.x + this.radius, this.position.y + this.radius) }
    originServer = function () { return new Vector(this.serverPosition.x + this.radius, this.serverPosition.y + this.radius) }

    originX = function () { return this.origin().x; }
    originY = function () { return this.origin().y; }

    update(dt) {

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

        this.rotate(dt);

        if (this.serverPosition.x != 0 && this.serverPosition.y != 0)
        {
            var delta = Vector.subtract(this.serverPosition,this.position);
            
            var dx = Math.abs(delta.x);
            var dy = Math.abs(delta.y);
            
            if (dx > 0.01 || dy > 0.01)
                this.position = Vector.Lerp(this.position, this.serverPosition, dt);
            else
                this.serverPosition = this.position;
        }
    }

    draw(ctx) {
        // ctx.strokeStyle = "magenta";
        // ctx.moveTo(this.originX(), this.originY());
        // ctx.lineTo(this.originX() + this.velocity.x, this.originY() + this.velocity.y);
        ctx.stroke();

        ctx.beginPath();
        if (this.isPlayer) {
            ctx.arc(this.serverPosition.x + this.radius, this.serverPosition.y + this.radius, this.radius, 0, Math.PI * 2);
            ctx.fillStyle = "black";
            ctx.fill();
            ctx.strokeStyle = this.borderColor;
        }
        else {
            ctx.fillStyle = "black";
            this.DrawShape2(ctx, this);
        }
        ctx.stroke();
    }

    checkCollision_Circle(entity) {
        let distance = Vector.distance(this.origin(), entity.origin());
        return Math.abs(distance) <= this.radius + entity.radius;
    }
    checkCollision_Point(vecor) {
        let distance = Vector.distance(this.origin(), vecor);
        return distance <= this.size;
    }

    DrawShape(ctx, entity) {
        ctx.fillStyle = this.inCollision ? "#990000" : this.fillColor;
        ctx.strokeStyle = entity.borderColor;
        const shift = entity.direction;
        const origin = entity.origin();
        ctx.beginPath();
        for (let i = 0; i <= entity.sides; i++) {
            let curStep = i * entity.step + shift;
            ctx.lineTo(origin.x + entity.radius * Math.cos(curStep), origin.y + entity.radius * Math.sin(curStep));
        }
        ctx.lineWidth = 1;
        ctx.stroke();
        ctx.fill();
    }

    DrawShape2(ctx, entity) {
        //ctx.fillStyle = this.inCollision ? "#990000" : this.fillColor;
        ctx.strokeStyle = entity.borderColor;
        const shift = entity.direction;
        const origin = entity.originServer();
        ctx.beginPath();
        for (let i = 0; i <= entity.sides; i++) {
            let curStep = i * entity.step + shift;
            ctx.lineTo(origin.x + entity.radius * Math.cos(curStep), origin.y + entity.radius * Math.sin(curStep));
        }
        ctx.lineWidth = 1;
        ctx.stroke();
        ctx.fill();
    }

    rotate(dt) {
        if (this.velocity.y > 0)
            this.direction += 0.03 * Math.abs(this.velocity.y) * dt;
        else
            this.direction -= 0.03 * Math.abs(this.velocity.y) * dt;

        if (this.direction > 360)
            this.direction = 0;
        if (this.direction < 0)
            this.direction = 360;
    }
}