import { HealthBar } from "./HealthBar.js";
import { ShieldBar } from "./shieldBar.js";
import { BatteryBar } from "./batteryBar.js";
import { Vector } from "../Vector.js";

export class Entity
{
    name = "";
    id = 0;
    owner = null;
    sides = 6;
    fillColor = "magenta";
    strokeColor = 0;
    size = 1;
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
    shieldBar = null;
    batteryBar = null;

    constructor(id)
    {
        this.id = id;
        this.healthBar = new HealthBar(this);
        this.shieldBar = new ShieldBar(this);
        this.batteryBar = new BatteryBar(this);
    }

    get step() { return 2 * Math.PI / this.sides; }
    get radius() { return this.size / 2; }
    get mass() { return Math.pow(this.size, 3); }
    get inverseMass() { return 1 / this.mass; }

    update(dt)
    {
        if (this.name == "" && window.game.entityNames.has(this.id))
            this.name = window.game.entityNames.get(this.id);
        this.velocity = Vector.clampMagnitude(this.velocity, this.maxSpeed);

        let d = 1 - (this.drag * dt);
        this.velocity = this.velocity.multiply(d);

        if (this.velocity.magnitude() < 5)
            this.velocity = new Vector(0, 0);

        this.position = this.position.add(this.velocity.multiply(dt));
        if (this.serverPosition.x != 0 || this.serverPosition.y != 0)
            this.position = this.serverPosition;
        this.velocity = this.serverVelocity;
    }

    draw(ctx)
    {
        if (this.name != "")
        {
            this.drawName(ctx);
            this.drawWeapon(ctx);
            this.drawShape(ctx);
        }
        if(this.shieldCharge > 0)
            this.drawShield(ctx);
    }

    drawName(ctx)
    {
        ctx.fillStyle = "white";
        ctx.font = "10px Arial";
        ctx.textAlign = "center";
        ctx.fillText(this.name, this.position.x, this.position.y - this.size);
    }

    drawShape(ctx)
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

    drawShield(ctx)
    {
        ctx.strokeStyle = "blue";
        ctx.beginPath();
        ctx.arc(this.position.x, this.position.y, this.shieldRadius, 0, Math.PI * 2);
        ctx.stroke();
    }
    drawWeapon(ctx)
    {
        var dx = Math.cos(this.direction);
        var dy = Math.sin(this.direction);
        var pos = new Vector(dx, dy);
        var d = pos.multiply(this.size);

        ctx.strokeStyle = "#616161";
        ctx.lineWidth = this.radius / 2;
        ctx.beginPath();
        ctx.moveTo(this.position.x, this.position.y);
        ctx.lineTo(this.position.x + d.x, this.position.y + d.y);
        ctx.stroke();

        d.multiply(0.95);
        ctx.strokeStyle = "#393939";
        ctx.lineWidth = this.radius / 2.5;
        ctx.beginPath();
        ctx.moveTo(this.position.x, this.position.y);
        ctx.lineTo(this.position.x + d.x, this.position.y + d.y);
        ctx.stroke();
    }
}