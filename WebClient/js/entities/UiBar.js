export class UiBar
{
    x = 0;
    y = 0;
    w = 0;
    h = 0;

    text = "";

    xoffset =1;
    yoffset = 0.9;

    minValue = 0;
    maxValue = 100;
    value = 50;

    owner = null;

    percent = () => Math.round(100 * this.value / this.maxValue); 

    constructor(owner, x, y, xoffset, yoffset, w, h, text, minValue, maxValue, value, color)
    {
        this.owner = owner;
        this.x = x;
        this.y = y;
        this.xoffset = xoffset;
        this.yoffset = yoffset;
        this.w = w;
        this.h = h;
        this.text = text;
        this.minValue = minValue;
        this.maxValue = maxValue;
        this.value = value;
        this.color = color;
    }

    drawBg(ctx)
    {
        const x = this.owner.position.x - this.owner.size * this.xoffset;
        const y = this.owner.position.y - this.owner.size * this.yoffset;
        const w = this.owner.size * 2;
        const h = 0.5;
        
        ctx.fillRect(x, y, w, h);
    }
    drawFg(ctx)
    {        
        const x = this.owner.position.x - this.owner.size * this.xoffset;
        const y = this.owner.position.y - this.owner.size * this.yoffset;
        const w2 = this.w * Math.min(1, (this.percent() / 100));
        const w = (this.owner.size * 2) * Math.min(1, (w2 / 100));
        const h = 0.5;

        ctx.fillRect(x, y, w, h);
    }

    drawUI(ctx)
    {
        ctx.fillStyle = 'white';
        ctx.fillRect(this.x, this.y, this.w, this.h);
        const w2 = this.w * Math.min(1, (this.percent() / 100));
        ctx.fillStyle = this.color;
        ctx.fillRect(this.x, this.y, w2, this.h);
        ctx.fillStyle = 'black';
        ctx.fillText(this.text + ": " + Math.round(this.value) + " / " + Math.round(this.maxValue), this.x + 8, this.y + this.h / 1.3);
    }
}