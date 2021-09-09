
export class Player {

    name = "player";

    health = 10;
    maxHealth = 10;

    x = 10;
    y = 20;
    direction = 40;

    size = 20;
    speed = 2;

    fillColor = "#00b2e1";
    borderColor = "#20bae9";


    originX = function () { return this.x + this.size / 2; }
    originY = function () { return this.y + this.size / 2; }

    draw(ctx) {
        ctx.beginPath();
        ctx.arc(this.x, this.y, this.size, 0, Math.PI * 2, false);
        ctx.fillStyle = this.fillColor;
        ctx.fill();
        ctx.strokeStyle = this.borderColor;
        ctx.stroke();
    }

    checkCollision_Sph(entity) {
        var distance = Math.sqrt((this.x - entity.x) ^ 2 + (this.y - entity.y) ^ 2)
        return (distance <= (entity.size + this.size));
    }
    checkCollision_Rec(entity) {
        //x1, x2           = Left
        //x1 + w1, x2 + w2 = Right
        //y1, y2           = Bottom
        //y1 - h1, y2 - h2 = Top


        if ((this.y < entity.y) ||
            ((this.y - this.size) > entity.y) ||
            (this.x > (entity.x + entity.size)) ||
            ((this.x + this.size) < entity.size)) {
            return false;
        }
        else {
            return true
        }
    }
}

export class YellowCube {
    size = 20;
    x = 100;
    y = 100;
    speed = 1;

    fillColor = "#ffe869";
    borderColor = "#bfae4e";

    originX = function () { return this.x + this.size / 2; }
    originY = function () { return this.y + this.size / 2; }

    draw(ctx) {
        ctx.fillStyle = this.borderColor;
        ctx.fillRect(this.x, this.y, this.size, this.size);
        ctx.fillStyle = this.fillColor;
        ctx.fillRect(4 + this.x, 4 + this.y, this.size - 8, this.size - 8);
    }
}