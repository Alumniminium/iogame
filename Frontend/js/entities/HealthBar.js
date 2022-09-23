export class HealthBar
{
    xoffset =1;
    yoffset = 1.1;

    owner = null;

    constructor(owner)
    {
        this.owner = owner;
    }

    drawBg(ctx)
    {
        const x = this.owner.position.x - this.owner.size * this.xoffset;
        const y = this.owner.position.y - this.owner.size * this.yoffset;
        const w = this.owner.size * 2;
        const h = 1;
        
        ctx.fillRect(x, y, w, h);
    }
    drawFg(ctx)
    {        
        const x = this.owner.position.x - this.owner.size * this.xoffset;
        const y = this.owner.position.y - this.owner.size * this.yoffset;
        const healthPercent = 100 * this.owner.health / this.owner.maxHealth; 
        const w = (this.owner.size * 2) * Math.min(1, (healthPercent / 100));
        const h = 1;

        ctx.fillRect(x, y, w, h);
    }

    drawUI(ctx)
    {
        const x = 16;
        const y = 16;
        const h = 24;
        let w = 250;
        ctx.fillStyle = 'white';
        ctx.fillRect(x, y, w, h);
        const healthPercent = 100 * this.owner.health / this.owner.maxHealth; 
        w = w * Math.min(1, (healthPercent / 100));
        ctx.fillStyle = 'red';
        ctx.fillRect(x, y, w, h);
        ctx.fillStyle = 'black';
        ctx.fillText("Health: " + Math.round(this.owner.health) + " / " + Math.round(this.owner.maxHealth), x + 8, y + 16);
    }
}
