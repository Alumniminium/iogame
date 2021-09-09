export class Entity {
    isPlayer = false;
    x = 100;
    y = 200;
    direction = 40;
    size = 1;
    vx = 0;
    vy = 0;
    health = 10;
    maxHealth = 10;
    fillColor = "#ffe869";
    borderColor = "#bfae4e";

    originX = function () { return this.x + this.size / 2; }
    originY = function () { return this.y + this.size / 2; }

    update(timeStamp)
    {
        let radians = Math.atan2(this.vy, this.vx);
        this.direction = 180 * radians / Math.PI;
    }

    
    checkCollision_Rec(entity) {
        //let dx = entity.x - this.x;
        //let dy = entity.y - this.y;
        //let d = Math.sqrt(Math.pow(dx, 2) + Math.pow(dy, 2));
        //return (d < this.size + entity.size);
        return (this.x < entity.x + entity.size && this.x + this.size > entity.x && this.y < entity.y + entity.size && this.y + this.size > entity.y);
    }
}