export class ExperienceBar
{
    xoffset = 1;
    yoffset = 1;

    owner = null;

    constructor(owner)
    {
        this.owner = owner;
    }

    drawBg(ctx)
    {
        
    }
    drawFg(ctx)
    {
        
    }
    drawUI(ctx)
    {
        const x = 16;
        const y = 108;
        const h = 24;
        let w = 250;
        ctx.fillStyle = 'white';
        ctx.fillRect(x, y, w, h);
        const healthPercent = Math.round(100 * this.owner.experience / this.owner.experienceToNextLevel);
        w = w * Math.min(1, (healthPercent / 100));
        ctx.fillStyle = 'yellow';
        ctx.fillRect(x, y, w, h);
        ctx.fillStyle = 'black';
        ctx.fillText("Level: "+this.owner.level+" Exp: " + Math.round(this.owner.experience) + " / " + Math.round(this.owner.experienceToNextLevel), x + 8, y + 16);
    }
}