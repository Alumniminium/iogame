export class HealthBar
{
    bgColor = "";
    fgColor = "";

    owner = null;

    constructor(owner)
    {
        this.owner = owner;
    }

    drawBg(ctx)
    {
        const x = this.owner.position.x - this.owner.size;
        const y = this.owner.position.y - this.owner.size * 0.9;
        const w = this.owner.size * 2;
        const h = 16
        
        ctx.fillRect(x, y, w, h);
    }
    drawFg(ctx)
    {        
        const x = this.owner.position.x - this.owner.size;
        const y = this.owner.position.y - this.owner.size * 0.9;
        const healthPercent = 100 * this.owner.health / this.owner.maxHealth; 
        const w = (this.owner.size * 2) * (healthPercent / 100);
        const h = 16;

        ctx.fillRect(x, y, w, h);
    }
}