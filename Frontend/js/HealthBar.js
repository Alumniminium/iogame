export class HealthBar
{
    xoffset =1;
    yoffset = 0.8;

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
        const h = 2;
        
        ctx.fillRect(x, y, w, h);
    }
    drawFg(ctx)
    {        
        const x = this.owner.position.x - this.owner.size * this.xoffset;
        const y = this.owner.position.y - this.owner.size * this.yoffset;
        const healthPercent = 100 * this.owner.health / this.owner.maxHealth; 
        const w = (this.owner.size * 2) * Math.min(1, (healthPercent / 100));
        const h = 2;

        ctx.fillRect(x, y, w, h);
    }
}