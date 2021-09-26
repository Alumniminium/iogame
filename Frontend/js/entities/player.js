import { Entity } from "./entity.js";
import { Input } from "../input.js";
import { Vector } from "../vector.js";
import { Packets } from "../network/packets.js";
import { Bullet } from "./bullet.js";

export class Player extends Entity {

    game = null;
    name = "player";

    input = new Input();
    constructor(game, id, name, x, y) {
        super(id);
        this.ping =0;
        this.game = game;
        this.name = name;
        this.position = new Vector(x, y);
        this.isPlayer = true;
        this.size = 300;
        this.mass = Math.pow(this.size,3);
        this.fillColor = "#00b2e1";
        this.borderColor = "#20bae9";
        this.speed = 1500;
        this.health = 10;
        this.maxHealth = 10;
    }
    draw(ctx) {
        super.draw(ctx);
        ctx.fillStyle = this.fillColor;
        ctx.strokeStyle = this.borderColor;

        ctx.beginPath();
        ctx.arc(this.position.x + this.radius(), this.position.y + this.radius(), this.radius(), 0, Math.PI * 2);
        ctx.fillStyle = this.fillColor;
        ctx.fill();
        ctx.strokeStyle = this.borderColor;
        ctx.stroke();

        // Draw health bar
        ctx.fillStyle = 'white';
        ctx.fillRect(this.position.x - this.size, this.position.y - this.radius(), this.size * 3, 4);
        ctx.fillStyle = 'red';
        ctx.fillRect(this.position.x - this.size, this.position.y - this.radius(), (this.size * 3) / 100 * (100 * this.health / this.maxHealth), 4);
        ctx.fillStyle = 'white';
        let nameTag = "Id: " + this.id + ", Ping: " + this.ping +"ms";
        let textSize = ctx.measureText(nameTag);
        ctx.fillText(nameTag, this.originX() - textSize.width / 2, this.originY() - this.size * 1.1);
    }
    update(dt) {
        let inputVector = new Vector(0, 0);
        if (this.input.left)
            inputVector.x--;
        else if (this.input.right)
            inputVector.x++;

        if (this.input.up)
            inputVector.y--;
        else if (this.input.down)
            inputVector.y++;

        inputVector = Vector.clampMagnitude(inputVector, 1);
        inputVector.multiply(this.speed);

        this.velocity.add(inputVector);
        this.velocity = Vector.clampMagnitude(this.velocity, this.speed);
        // if(this.input.lmb)
        // {
        //     var pos = this.game.renderer.camera.screenToWorld(this.input.mpos.x,this.input.mpos.y);
        //     let speed = 200;
        //     var dir = Math.atan2(pos.y - this.originY(), pos.x - this.originX());
        //     var dx = Math.cos(dir) * speed;
        //     var dy = Math.sin(dir) * speed;

        //     let bullet = new Bullet(this.game.random(10000000,20000000));
        //     bullet.position = this.origin();
        //     bullet.velocity = new Vector(dx,dy);
        //     bullet.owner = this;
        //     this.game.addEntity(bullet);
        // }

        this.renerateHealth(dt);

        super.update(dt);

        if (this.input.changed) {
            this.input.changed = false;
            this.game.net.send(Packets.MovementPacket(this, this.input.up, this.input.down, this.input.left, this.input.right));
        }
    }

    renerateHealth(dt) {
        if (this.health < this.maxHealth) {
            const healthAdd = 10 * dt;

            if (this.health + healthAdd > this.maxHealth)
                this.health = this.maxHealth;

            else
                this.health += healthAdd;
        }
    }
}
