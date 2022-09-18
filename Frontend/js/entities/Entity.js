import { HealthBar } from "../HealthBar.js";
import { Vector } from "../Vector.js";

export class Entity
{
    owner = null;
    sides = 6;
    fillColor = "magenta";
    strokeColor = 0;
    size = 1;

    id = 0;
    position = new Vector(0, 0);
    // nextPosition = new Vector(0, 0);
    velocity = new Vector(0, 0);
    serverPosition = new Vector(0, 0);
    serverVelocity = new Vector(0, 0);
    health = 100;
    maxHealth = 100;

    drag = 0.9999;
    elasticity = 1;
    maxSpeed = 1500;

    healthBar = null;

    constructor(id)
    {
        this.id = id;
        this.healthBar = new HealthBar(this);
    }

    get step() { return 2 * Math.PI / this.sides; }
    get radius() { return this.size / 2; }
    get mass() { return Math.pow(this.size, 3); }
    get inverseMass() { return 1 / this.mass; }

    update(dt)
    {
        this.velocity = Vector.clampMagnitude(this.velocity, this.maxSpeed);

        let d = 1 - (this.drag * dt);
        this.velocity = this.velocity.multiply(d);

        if (this.velocity.magnitude() < 5)
            this.velocity = new Vector(0, 0);

        this.position = this.position.add(this.velocity.multiply(dt));
            if(this.serverPosition.x != 0 || this.serverPosition.y != 0)
                this.position = this.serverPosition;
        this.velocity = this.serverVelocity;
    }

    draw(ctx)
    {
        this.DrawShape(ctx);
    }

    DrawShape(ctx)
    {
        if (this.sides == 1)
        {
            ctx.beginPath();
            ctx.arc(this.position.x, this.position.y, this.radius, 0, Math.PI * 2);
            ctx.fill();
        }
        else
        {
            const shift = this.direction;
            const origin = this.position;
            ctx.beginPath();
            for (let i = 0; i <= this.sides; i++)
            {
                let curStep = i * this.step + shift;
                ctx.lineTo(origin.x + this.radius * Math.cos(curStep), origin.y + this.radius * Math.sin(curStep));
            }
            //ctx.stroke();
            ctx.fill();
        }
    }

    DrawShield(ctx)
    {
        ctx.strokeStyle = "blue";
        ctx.beginPath();
        ctx.arc(this.position.x, this.position.y, window.shieldRadius, 0, Math.PI * 2);
        ctx.stroke();
    }
}