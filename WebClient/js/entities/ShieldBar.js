export class ShieldBar {
    constructor(owner) {
        this.owner = owner;
    }

    drawUI(ctx) {
        const x = 16;
        const y = 78;
        const h = 24;
        const maxWidth = 250;
        
        ctx.fillStyle = 'white';
        ctx.fillRect(x, y, maxWidth, h);
        
        const percent = Math.min(100, Math.round(100 * this.owner.shieldCharge / this.owner.shieldMaxCharge));
        const barWidth = maxWidth * (percent / 100);
        
        ctx.fillStyle = 'blue';
        ctx.fillRect(x, y, barWidth, h);
        
        ctx.fillStyle = 'black';
        ctx.fillText(`Shield: ${Math.round(this.owner.shieldCharge)} / ${Math.round(this.owner.shieldMaxCharge)}`, x + 8, y + 16);
    }
}