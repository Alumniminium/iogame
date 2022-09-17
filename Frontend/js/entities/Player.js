import { Entity } from "./Entity.js";
import { Vector } from "../Vector.js";

export class Player extends Entity
{
    name = "player";
    constructor(id, name, x, y)
    {
        super(id);
        this.name = name;
        this.position = new Vector(x, y);
        this.size = 200;
        this.maxSpeed = 1500;
        this.health = 100;
        this.maxHealth = 100;
        this.fillColor = "#008dba";
        this.borderColor = "#005e85";
        this.lastShot = new Date().getTime();
    }
    draw(ctx)
    {
        super.draw(ctx);

        this.drawWeapon(ctx);

        // ctx.fillStyle = this.fillColor;
// 
        // ctx.lineWidth = 2;
        // ctx.beginPath();
        // ctx.arc(this.position.x, this.position.y, this.radius, 0, Math.PI * 2);
        // ctx.stroke();
        // ctx.fill();
    }
    drawWeapon(ctx)
    {
        // console.log(this.direction);
        var dx = Math.cos(this.direction);
        var dy = Math.sin(this.direction);
        var pos = new Vector(dx,dy);
        var d = pos.multiply(this.size);
        // console.log(d);

        // ctx.strokeStyle = "#393939";
        ctx.lineWidth = this.radius / 2;
        ctx.beginPath();
        ctx.moveTo(this.position.x, this.position.y);
        ctx.lineTo(this.position.x + d.x, this.position.y + d.y);
        ctx.stroke();

        d.multiply(0.95);
        // ctx.strokeStyle = "#616161";
        ctx.lineWidth = this.radius / 2.5;
        ctx.beginPath();
        ctx.moveTo(this.position.x, this.position.y);
        ctx.lineTo(this.position.x + d.x, this.position.y + d.y);
        ctx.stroke();
    }

    update(dt)
    {
        let inputVector = new Vector(0, 0);
        if (window.input.left)
            inputVector.x = -1;
        else if (window.input.right)
            inputVector.x = 1;

        if (window.input.up)
            inputVector.y = -1;
        else if (window.input.down)
            inputVector.y = 1;

        // if (inputVector.magnitude() == 0)
        //     return;

        inputVector = inputVector.multiply(500);
        // inputVector = inputVector.mult(dt);


        this.velocity = this.velocity.add(inputVector);
        this.regenerateHealth(dt);
        super.update(dt);
    }

    regenerateHealth(dt)
    {
        if (this.health < this.maxHealth) 
        {
            const healthAdd = 1 * dt;

            if (this.health + healthAdd > this.maxHealth)
                this.health = this.maxHealth;

            else
                this.health += healthAdd;
        }
    }
}
