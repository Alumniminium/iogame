import { Vector } from "../vector.js";

const random = (min, max) => Math.floor(Math.random() * (max - min)) + min;

export class Entity {
    sides = 4;
    fillColor = 0;
    strokeColor = 0;
    size = 1;

    id = 0;
    isPlayer = false;
    position = new Vector(0, 0);
    velocity = new Vector(0, 0);
    serverPosition = new Vector(0, 0);
    serverVelocity = new Vector(0, 0);
    health = 10;
    maxHealth = 10;

    drag = 0.99997;
    elasticity = 1;
    maxSpeed = 5000;

    constructor(id) {
        this.id = id;
    }
    
    get step() { return 2 * Math.PI / this.sides;}
    get radius(){return this.size / 2;}    
    get mass() { return Math.pow(this.size,3);}
    get inverseMass() { return 1 / this.mass;}
    get originX() { return this.origin.x;}
    get originY() { return this.origin.y;}
    get origin(){return new Vector(this.position.x + this.radius, this.position.y + this.radius)}
    get originServer() { return new Vector(this.serverPosition.x + this.radius, this.serverPosition.y + this.radius) }

    update(dt) {
        this.rotate(dt);
        
        let d = 1 - (this.drag * dt);
       
        this.velocity = this.velocity.multiply(d);
       
        const vel = this.velocity.multiply(dt);
       
        this.position = this.position.add(vel);

        var dx = Math.abs(this.serverPosition.x - this.position.x);
        var dy = Math.abs(this.serverPosition.y - this.position.y);
        var dvx = Math.abs(this.serverVelocity.x - this.velocity.x);
        var dvy = Math.abs(this.serverVelocity.y - this.velocity.y);

        if (dx > 1 || dy > 1)
            this.position = Vector.Lerp(this.position, this.serverPosition, dt * 4);
        else
            this.position = this.serverPosition;


        if (dvx > 10 || dvy > 10)
            this.velocity = Vector.Lerp(this.velocity, this.serverVelocity, dt * 4);
        else
            this.velocity = this.serverVelocity;
    }

    draw(ctx) {

        // ctx.strokeStyle = "magenta";
        // ctx.beginPath();
        // ctx.moveTo(this.position.x, this.position.y);
        // ctx.lineTo(this.position.x + this.velocity.x / 2, this.position.y + this.velocity.y / 2);
        // ctx.stroke();

        ctx.beginPath();
        if (this.isPlayer) {
            ctx.arc(this.serverPosition.x, this.serverPosition.y, this.radius, 0, Math.PI * 2);
            ctx.fillStyle = "black";
            ctx.fill();
            ctx.strokeStyle = this.borderColor;
        }
        else {
            ctx.fillStyle = "black";
            this.DrawServerPosition(ctx, this);
        }
        ctx.stroke();
        this.DrawShape(ctx, this);
    }

    checkCollision_Circle(entity) {
        if (this.radius + entity.radius >= entity.position.subtract(this.position).magnitude())
            return true;
        return false;
    }
    checkCollision_Point(vecor) {
        let distance = Vector.distance(this.origin, vecor);
        return distance <= this.size;
    }

    DrawShape(ctx, entity) {
        ctx.fillStyle = this.inCollision ? "#990000" : this.fillColor;
        ctx.strokeStyle = entity.strokeColor;
        const shift = entity.direction;
        const origin = entity.position;
        ctx.beginPath();
        for (let i = 0; i <= entity.sides; i++) {
            let curStep = i * entity.step + shift;
            ctx.lineTo(origin.x + entity.radius * Math.cos(curStep), origin.y + entity.radius * Math.sin(curStep));
        }
        ctx.lineWidth = 1;
        ctx.stroke();
        ctx.fill();
    }

    DrawServerPosition(ctx, entity) {
        ctx.arc(entity.serverPosition.x, entity.serverPosition.y, entity.radius, 0, Math.PI * 2);
        ctx.fillStyle = "black";
        ctx.fill();
        ctx.strokeStyle = entity.borderColor;
    }

    rotate(dt) {
        this.direction += 0.003 * (this.velocity.y + this.velocity.x) * dt;

        if (this.direction > 360)
            this.direction = 0;
        if (this.direction < 0)
            this.direction = 360;
    }
}