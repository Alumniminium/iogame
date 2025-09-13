export class ExperienceBar {
    constructor(owner) {
        this.owner = owner;
    }

    drawUI(ctx) {
        const x = 16;
        const y = 108;
        const h = 24;
        const maxWidth = 250;
        
        ctx.fillStyle = 'white';
        ctx.fillRect(x, y, maxWidth, h);
        
        const percent = Math.min(100, Math.round(100 * this.owner.experience / this.owner.experienceToNextLevel));
        const barWidth = maxWidth * (percent / 100);
        
        ctx.fillStyle = 'yellow';
        ctx.fillRect(x, y, barWidth, h);
        
        ctx.fillStyle = 'black';
        ctx.fillText(`Level: ${this.owner.level} Exp: ${Math.round(this.owner.experience)} / ${Math.round(this.owner.experienceToNextLevel)}`, x + 8, y + 16);
    }
}