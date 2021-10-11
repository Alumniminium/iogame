import { Entity } from "./entity.js";
import { Input } from "../input.js";
import { Vector } from "../vector.js";
import { Packets } from "../network/packets.js";

export class Player extends Entity
{

    game = null;
    name = "player";

    input = null;
    constructor(game, id, name, x, y)
    {
        super(id);
        this.game = game;
        this.input = new Input(this.game);
        this.name = name;
        this.position = new Vector(x, y);
        this.size = 200;
        this.maxSpeed = 1500;
        this.health = 10;
        this.maxHealth = 10;
        this.fillColor = "#008dba";
        this.borderColor = "#005e85";
        this.lastShot = new Date().getTime();
    }
    draw(ctx)
    {
        super.draw(ctx);

        this.drawWeapon(ctx);

        ctx.fillStyle = this.fillColor;
        ctx.strokeStyle = this.borderColor;

        ctx.lineWidth = 20;
        ctx.beginPath();
        ctx.arc(this.position.x, this.position.y, this.radius, 0, Math.PI * 2);
        ctx.stroke();
        ctx.fill();

        this.drawHealthbar(ctx);
    }
    drawWeapon(ctx)
    {
        var pos = this.game.renderer.camera.screenToWorld(this.input.mpos.x, this.input.mpos.y);
        var d = this.position.subtract(pos).unit();
        d = d.multiply(this.radius * 2);

        ctx.strokeStyle = "#393939";
        ctx.lineWidth = 95;
        ctx.beginPath();
        ctx.moveTo(this.position.x, this.position.y);
        ctx.lineTo(this.position.x + -d.x, this.position.y + -d.y);
        ctx.stroke();

        d.multiply(0.95);
        ctx.strokeStyle = "#616161";
        ctx.lineWidth = 70;
        ctx.beginPath();
        ctx.moveTo(this.position.x, this.position.y);
        ctx.lineTo(this.position.x + -d.x, this.position.y + -d.y);
        ctx.stroke();
    }

    drawHealthbar(ctx)
    {
        ctx.fillStyle = 'white';
        ctx.fillRect(this.position.x - this.size, this.position.y - this.size * 0.9, this.size * 2, 16);
        ctx.fillStyle = 'red';
        ctx.fillRect(this.position.x - this.size, this.position.y - this.size * 0.9, (this.size * 2) / 100 * (100 * this.health / this.maxHealth), 16);

        ctx.fillStyle = 'white';
        ctx.font = 'bolder 40px Arial';
        let nameTag = this.name + " (" + this.id + ")";
        let textSize = ctx.measureText(nameTag);
        ctx.fillText(nameTag, this.position.x - textSize.width / 2, this.position.y - this.size * 1.20);
    }

    update(dt)
    {
        let inputVector = new Vector(0, 0);
        if (this.input.left)
            inputVector.x -= 1000;
        else if (this.input.right)
            inputVector.x += 1000;

        if (this.input.up)
            inputVector.y -= 1000;
        else if (this.input.down)
            inputVector.y += 1000;

        inputVector = Vector.clampMagnitude(inputVector, 1000);
        inputVector = inputVector.multiply(dt);

        this.velocity = this.velocity.add(inputVector);

        if (this.input.lmb && this.input.posChanged && new Date().getTime() > this.lastShot + 200)
        {
            this.input.changed = true;
            this.lastShot = new Date().getTime();
        }

        if (this.input.changed)
        {
            this.input.changed = false;
            this.input.posChanged = false;
            let pos = this.game.renderer.camera.screenToWorld(this.input.mpos.x, this.input.mpos.y);
            this.game.net.send(Packets.MovementPacket(this, this.input.up, this.input.down, this.input.left, this.input.right, this.input.lmb, pos.x, pos.y));
        }
        this.renerateHealth(dt);
        super.update(dt);

    }

    renerateHealth(dt)
    {
        if (this.health < this.maxHealth) 
        {
            const healthAdd = 10 * dt;

            if (this.health + healthAdd > this.maxHealth)
                this.health = this.maxHealth;

            else
                this.health += healthAdd;
        }
    }
}
