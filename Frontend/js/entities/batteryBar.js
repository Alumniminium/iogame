export class BatteryBar
{
    xoffset =1;
    yoffset = 0.9;

    owner = null;

    constructor(owner)
    {
        this.owner = owner;
    }

    drawBg(ctx)
    {
        // const x = this.owner.position.x - this.owner.size * this.xoffset;
        // const y = this.owner.position.y - this.owner.size * this.yoffset;
        // const w = this.owner.size * 2;
        // const h = 1;
        
        // ctx.fillRect(x, y, w, h);
    }
    drawFg(ctx)
    {        
        // const x = this.owner.position.x - this.owner.size * this.xoffset;
        // const y = this.owner.position.y - this.owner.size * this.yoffset;
        // const healthPercent = Math.round(100 * this.owner.batteryCharge / this.owner.batteryCapacity); 
        // const w = (this.owner.size * 2) * Math.min(1, (healthPercent / 100));
        // const h = 1;

        // ctx.fillRect(x, y, w, h);
    }
    drawUI(ctx)
    {
        const x = 16;
        const y = 48;
        const h = 24;
        let w = 250;
        ctx.fillStyle = 'white';
        ctx.fillRect(x, y, w, h);
        const healthPercent = Math.round(100 * this.owner.batteryCharge / this.owner.batteryCapacity); 
        w = w * Math.min(1, (healthPercent / 100));
        ctx.fillStyle = 'green';
        ctx.fillRect(x, y, w, h);
        ctx.fillStyle = 'black';
        ctx.fillText("Battery: " + Math.round(this.owner.batteryCharge) + " / " + Math.round(this.owner.batteryCapacity), x + 8, y + 16);
    }
}