import { HealthBar } from "../healthBar.js";
import { Vector } from "../vector.js";

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
    // get origin() { return new Vector(this.position.x + this.radius, this.position.y + this.radius); }
    //get originServer() { return new Vector(this.serverPosition.x + this.radius, this.serverPosition.y + this.radius); }

    update(dt)
    {
        this.velocity = Vector.clampMagnitude(this.velocity, this.maxSpeed);

        let d = 1 - (this.drag * dt);
        this.velocity = this.velocity.multiply(d);

        if (this.velocity.magnitude() < 5)
            this.velocity = new Vector(0, 0);

        this.position = this.position.add(this.velocity.multiply(dt));

        // if (this.serverPosition.x != -1 && this.serverPosition.y != -1)
        // {
        //     let dp = this.serverPosition.subtract(this.position);
        //     dp.x = Math.abs(dp.x);
        //     dp.y = Math.abs(dp.y);

        //     if (dp.x < this.size && dp.y < this.radius)
        //     {
        //         this.position = Vector.lerp(this.position, this.serverPosition, dt * 5);
        //     }
        //     else
        //     {
        this.position = this.serverPosition;
        this.velocity = this.serverVelocity;
        //         this.serverPosition = new Vector(-1, -1);
        //         this.serverVelocity = new Vector(-1, -1);
        //     }

        //     if (dp.x < 0.1 && dp.y < 0.1)
        //     {
        //         this.position = this.serverPosition;
        //         this.velocity = this.serverVelocity;
        //         this.serverPosition = new Vector(-1, -1);
        //         this.serverVelocity = new Vector(-1, -1);
        //     }
        // }
        // if (this.serverVelocity.x != -1 && this.serverVelocity.y != -1)
        // {
        //     const dv = this.serverVelocity.subtract(this.velocity);
        //     dv.x = Math.abs(dv.x);
        //     dv.y = Math.abs(dv.y);

        //     if (dv.x < 5 && dv.y < 5)
        //         this.velocity = Vector.lerp(this.velocity, this.serverVelocity, dt * 2);

        //     if (dv.x < 0.1 && dv.y < 0.1)
        //     {
        //         this.velocity = this.serverVelocity;
        //         this.serverVelocity = new Vector(-1, -1);
        //     }
        // }
        this.rotate(dt);
    }

    draw(ctx)
    {
        this.DrawShape(ctx);
        // draw id
        // ctx.fillStyle = "white";
        // ctx.font = "10px Arial";
        // ctx.fillText(this.id, this.position.x - 5, this.position.y - 5);
    }

    intersectsWithCircle(entity)
    {
        return (this.radius + entity.radius >= entity.position.subtract(this.position).magnitude());
    }
    intersectsWithPoint(vecor)
    {
        return Vector.dist(this.position, vecor) <= this.size;
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

    DrawServerPosition(ctx)
    {
        // if (this.id >= 1000000)
        // {
        // ctx.lineWidth = 2;
        // ctx.beginPath();
        // ctx.arc(this.serverPosition.x, this.serverPosition.y, 50, 0, Math.PI * 2);
        //}
        // else
        // {
        //     ctx.lineWidth = 1;
        //     const shift = this.direction;
        //     const origin = this.serverPosition;
        //     ctx.beginPath();
        //     for (let i = 0; i <= this.sides; i++)
        //     {
        //         let curStep = i * this.step + shift;
        //         ctx.lineTo(origin.x + this.radius * Math.cos(curStep), origin.y + this.radius * Math.sin(curStep));
        //     }
        // }
        // ctx.stroke();
        // ctx.fill();
    }

    rotate(dt)
    {
        // var p1 = this.position;
        // var p2 = this.position.add(this.velocity);
        // this.direction = Math.atan2(p2.y - p1.y, p2.x - p1.x);//* 180 / Math.PI;
    }
}