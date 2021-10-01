import { Vector } from "../vector.js";

const random = (min, max) => Math.floor(Math.random() * (max - min)) + min;

export class Entity {    
    owner = null;
    sides = 4;
    fillColor = 0;
    strokeColor = 0;
    size = 1;

    id = 0;
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
        
        let d = 1 - (this.drag * dt);
       
        this.velocity = this.velocity.multiply(d);

        var dv = this.serverVelocity.subtract(this.velocity);
        var dp = this.serverPosition.subtract(this.position);

        if (dp.x < 10 || dp.y < 10)
            this.position = Vector.Lerp(this.position, this.serverPosition, dt * 4);
        else
            this.position = this.serverPosition;


        if (dv.x < 10 || dv.y < 10)
            this.velocity = Vector.Lerp(this.velocity, this.serverVelocity, dt * 4);
        else
            this.velocity = this.serverVelocity;
        
        const vel = this.velocity.multiply(dt);

        this.rotate(dt);
        this.position = this.position.add(vel);
    }

    draw(ctx) {
        if(window.showServerPosToggle)
            this.DrawServerPosition(ctx);
        this.DrawShape(ctx);
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

    DrawShape(ctx) {
        ctx.fillStyle = this.fillColor;
        ctx.strokeStyle = this.strokeColor;
        const shift = this.direction;
        const origin = this.position;
        ctx.beginPath();
        for (let i = 0; i <= this.sides; i++) {
            let curStep = i * this.step + shift;
            ctx.lineTo(origin.x + this.radius * Math.cos(curStep), origin.y + this.radius * Math.sin(curStep));
        }
        ctx.lineWidth = 1;
        ctx.stroke();
        ctx.fill();
    }

    DrawServerPosition(ctx) {
        // ctx.fillStyle = "white";
        ctx.strokeStyle = this.borderColor;
        ctx.beginPath();
        ctx.arc(this.serverPosition.x, this.serverPosition.y, this.radius, 0, Math.PI * 2);
        ctx.stroke();
    }

    rotate(dt) {
        this.direction += 0.003 * (this.velocity.y + this.velocity.x) * dt;

        if (this.direction > 360)
            this.direction = 0;
        if (this.direction < 0)
            this.direction = 360;
    }
}