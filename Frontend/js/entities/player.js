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
        this.ping = 0;
        this.game = game;
        this.name = name;
        this.position = new Vector(x, y);
        this.isPlayer = true;
        this.size = 200;
        this.mass = Math.pow(this.size, 3);
        this.maxSpeed = 1500;
        this.health = 10;
        this.maxHealth = 10;
        this.fillColor = "#008dba";
        this.borderColor = "#005e85";
        this.lastShot = new Date().getTime();
    }
    draw(ctx) {
        super.draw(ctx);

        var pos = this.game.renderer.camera.screenToWorld(this.input.mpos.x,this.input.mpos.y);
        var d = Vector.subtract(this.position,pos).unit();
        d.multiply(this.radius()*2);

        ctx.strokeStyle= "#393939";
        ctx.lineWidth = 95;
        ctx.beginPath();
        ctx.moveTo(this.position.x, this.position.y);
        ctx.lineTo(this.position.x + -d.x, this.position.y+ -d.y);
        ctx.stroke();

        d.multiply(0.95);
        ctx.strokeStyle= "#616161";
        ctx.lineWidth = 70;
        ctx.beginPath();
        ctx.moveTo(this.position.x, this.position.y);
        ctx.lineTo(this.position.x + -d.x, this.position.y+ -d.y);
        ctx.stroke();

        ctx.fillStyle = this.fillColor;
        ctx.strokeStyle = this.borderColor;

        ctx.lineWidth = 25;
        ctx.beginPath();
        ctx.arc(this.position.x, this.position.y, this.radius(), 0, Math.PI * 2);
        ctx.stroke();
        ctx.fill();
       

        // Draw health bar
        ctx.fillStyle = 'white';
        ctx.fillRect(this.position.x - this.size, this.position.y - this.radius(), this.size * 3, 4);
        ctx.fillStyle = 'red';
        ctx.fillRect(this.position.x - this.size, this.position.y - this.radius(), (this.size * 3) / 100 * (100 * this.health / this.maxHealth), 4);
        ctx.fillStyle = 'white';
        let nameTag = "Id: " + this.id + ", Ping: " + this.ping + "ms";
        let textSize = ctx.measureText(nameTag);
        ctx.fillText(nameTag, this.originX() - textSize.width / 2, this.originY() - this.size * 1.1);
    }
    update(dt) {
        let inputVector = new Vector(0, 0);
        if (this.input.left)
            inputVector.x-= 1000;
        else if (this.input.right)
            inputVector.x+= 1000;

        if (this.input.up)
            inputVector.y-= 1000;
        else if (this.input.down)
            inputVector.y+= 1000;

        if (this.input.changed) {
            this.input.changed = false;
            this.game.net.send(Packets.MovementPacket(this, this.input.up, this.input.down, this.input.left, this.input.right));
        }
        inputVector = Vector.clampMagnitude(inputVector, 1000);
        inputVector.multiply(dt);
        this.velocity.add(inputVector);
       
        if(this.input.lmb && new Date().getTime() > this.lastShot + 200)
        {
            this.lastShot =  new Date().getTime();
            var pos = this.game.renderer.camera.screenToWorld(this.input.mpos.x,this.input.mpos.y);
            let speed = 1000;
            var dir = Math.atan2(pos.y - this.position.y, pos.x - this.position.x);
            var dx = Math.cos(dir);
            var dy = Math.sin(dir);
            let bullet = new Bullet(this.game.random(10000000,20000000));
            bullet.position = new Vector(-dx + this.position.x,-dy+this.position.y);
            bullet.velocity = new Vector(dx,dy).multiply(speed);
            bullet.owner = this;
            bullet.direction = 0;
            bullet.mass=3000000;
            bullet.spawnTime = new Date().getTime();
            

            let dist = Vector.subtract(this.position, bullet.position);
            let pen_depth = this.radius() + bullet.radius() - dist.magnitude();
            let pen_res = Vector.multiply(dist.unit(),pen_depth).multiply(1.5);

            bullet.position.add(pen_res);
            

            
            this.game.addEntity(bullet);
        }

        this.renerateHealth(dt);

        super.update(dt);
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
