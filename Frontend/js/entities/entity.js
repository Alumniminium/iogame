import { Vector } from "../vector.js";

const random = (min, max) => Math.floor(Math.random() * (max - min)) + min;

export class Entity {
    speed = 5000;
    mass = 0;
    sides = 4;
    step = 2 * Math.PI / this.sides;
    id = 0;
    inCollision = false;
    isPlayer = false;
    drag = 0.99997;
    position = new Vector(0, 0);
    velocity = new Vector(0, 0);
    serverPosition = new Vector(0, 0);
    serverVelocity = new Vector(0, 0);
    size = 1;
    health = 10;
    maxHealth = 10;
    fillColor = 0;
    strokeColor=0;

    constructor(id) {
        this.id = id;
    }

    radius = function () { return this.size / 2; }
    origin = function () { return new Vector(this.position.x + this.radius(), this.position.y + this.radius()) }
    originServer = function () { return new Vector(this.serverPosition.x + this.radius(), this.serverPosition.y + this.radius()) }

    originX = function () { return this.origin().x; }
    originY = function () { return this.origin().y; }

    update(dt) {
        this.rotate(dt);

        if (!this.inCollision) {
            let d = 1 - (this.drag * dt);
            this.velocity.multiply(d);
        }
        this.velocity = Vector.clampMagnitude(this.velocity, this.speed);

        this.position.add(Vector.multiply(this.velocity, dt));

        var dx = Math.abs(this.serverPosition.x - this.position.x);
        var dy = Math.abs(this.serverPosition.y - this.position.y);
        if (dx > 5 || dy > 5)
            this.position = Vector.Lerp(this.position, this.serverPosition, dt * 4);
        // else
            // this.position = this.serverPosition;
    }

    draw(ctx) {

        ctx.strokeStyle = "magenta";
        ctx.beginPath();
        ctx.moveTo(this.originX(), this.originY());
        ctx.lineTo(this.originX() + this.velocity.x / 2, this.originY() + this.velocity.y / 2);
        ctx.stroke();

        // ctx.beginPath();
        // if (this.isPlayer) {
        //     ctx.arc(this.serverPosition.x + this.radius(), this.serverPosition.y + this.radius(), this.radius(), 0, Math.PI * 2);
        //     ctx.fillStyle = "black";
        //     ctx.fill();
        //     ctx.strokeStyle = this.borderColor;
        // }
        // else {
        //     ctx.fillStyle = "black";
        //     this.DrawServerPosition(ctx, this);
        // }
        // ctx.stroke();
        this.DrawShape(ctx, this);
    }

    checkCollision_Circle(entity) {
        let distance = Vector.distance(this.origin(), entity.origin());
        return distance <= this.radius() + entity.radius();
    }
    checkCollision_Point(vecor) {
        let distance = Vector.distance(this.origin(), vecor);
        return distance <= this.size;
    }

    DrawShape(ctx, entity) {
        ctx.fillStyle = this.inCollision ? "#990000" : this.fillColor;
        ctx.strokeStyle = entity.strokeColor;
        const shift = entity.direction;
        const origin = entity.origin();
        ctx.beginPath();
        for (let i = 0; i <= entity.sides; i++) {
            let curStep = i * entity.step + shift;
            ctx.lineTo(origin.x + entity.radius() * Math.cos(curStep), origin.y + entity.radius() * Math.sin(curStep));
        }
        ctx.lineWidth = 1;
        ctx.stroke();
        ctx.fill();
    }

    DrawServerPosition(ctx, entity) {
        ctx.strokeStyle = entity.borderColor;
        const shift = entity.direction;
        const origin = entity.originServer();
        ctx.beginPath();
        for (let i = 0; i <= entity.sides; i++) {
            let curStep = i * entity.step + shift;
            ctx.lineTo(origin.x + entity.radius() * Math.cos(curStep), origin.y + entity.radius() * Math.sin(curStep));
        }
        ctx.lineWidth = 1;
        ctx.stroke();
        ctx.fill();
    }

    rotate(dt) {
        this.direction += 0.003 * (this.velocity.y + this.velocity.x) * dt;

        if (this.direction > 360)
            this.direction = 0;
        if (this.direction < 0)
            this.direction = 360;
    }
}