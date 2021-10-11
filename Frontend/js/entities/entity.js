import { Vector } from "../vector.js";

export class Entity
{
    owner = null;
    sides = 6;
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

    constructor(id)
    {
        this.id = id;
    }

    get step() { return 2 * Math.PI / this.sides; }
    get radius() { return this.size / 2; }
    get mass() { return Math.pow(this.size, 3); }
    get inverseMass() { return 1 / this.mass; }
    get originX() { return this.origin.x; }
    get originY() { return this.origin.y; }
    get origin() { return new Vector(this.position.x + this.radius, this.position.y + this.radius); }
    get originServer() { return new Vector(this.serverPosition.x + this.radius, this.serverPosition.y + this.radius); }

    update(dt)
    {
        this.velocity = Vector.clampMagnitude(this.velocity, this.maxSpeed);

        let d = 1 - (this.drag * dt);
        this.velocity = this.velocity.multiply(d);

        if (this.velocity.magnitude() < 5)
            this.velocity = new Vector(0, 0);

        if (this.serverPosition.x != -1 && this.serverPosition.y != -1)
        {
            const dp = this.serverPosition.subtract(this.position);

            if (dp.x > 0.1 || dp.y > 0.1)
                this.position = Vector.lerp(this.position, this.serverPosition, dt * 4);
            else if (dp.x < 0.1 && dp.y < 0.1)
            {
                this.position = this.serverPosition;
                this.serverPosition = new Vector(-1, -1);
            }
        }
        if (this.serverVelocity.x != -1 && this.serverVelocity.y != -1)
        {
            const dv = this.serverVelocity.subtract(this.velocity);

            if (dv.x > 0.1 || dv.y > 0.1)
                this.velocity = Vector.lerp(this.velocity, this.serverVelocity, dt * 4);
            else if (dv.x < 0.1 && dv.y < 0.1)
            {
                this.velocity = this.serverVelocity;
                this.serverVelocity = new Vector(-1, -1);
            }
        }


        this.position = this.position.add(this.velocity.multiply(dt));

        this.rotate(dt);
    }

    draw(ctx)
    {
        if (window.showServerPosToggle)
            this.DrawServerPosition(ctx);
        this.DrawShape(ctx);
    }

    checkCollision_Circle(entity)
    {
        return (this.radius + entity.radius >= entity.position.subtract(this.position).magnitude());
    }
    checkCollision_Point(vecor)
    {
        return Vector.distance(this.origin, vecor) <= this.size;
    }

    DrawShape(ctx)
    {
        ctx.fillStyle = this.fillColor;
        ctx.strokeStyle = this.strokeColor;
        const shift = this.direction;
        const origin = this.position;
        ctx.beginPath();
        for (let i = 0; i <= this.sides; i++)
        {
            let curStep = i * this.step + shift;
            ctx.lineTo(origin.x + this.radius * Math.cos(curStep), origin.y + this.radius * Math.sin(curStep));
        }
        ctx.lineWidth = 1;
        ctx.stroke();
        ctx.fill();
    }

    DrawServerPosition(ctx)
    {
        ctx.fillStyle = "#ff9933";
        ctx.strokeStyle = "#663300";

        if (this.id >= 1000000)
        {
            ctx.lineWidth = 20;
            ctx.beginPath();
            ctx.arc(this.serverPosition.x, this.serverPosition.y, this.radius, 0, Math.PI * 2);
        }
        else
        {
            ctx.lineWidth = 1;
            const shift = this.direction;
            const origin = this.serverPosition;
            ctx.beginPath();
            for (let i = 0; i <= this.sides; i++)
            {
                let curStep = i * this.step + shift;
                ctx.lineTo(origin.x + this.radius * Math.cos(curStep), origin.y + this.radius * Math.sin(curStep));
            }
        }
        ctx.stroke();
        ctx.fill();
    }

    rotate(dt)
    {
        this.direction += 0.003 * (this.velocity.y + this.velocity.x) * dt;

        if (this.direction > 360)
            this.direction = 0;
        if (this.direction < 0)
            this.direction = 360;
    }
}