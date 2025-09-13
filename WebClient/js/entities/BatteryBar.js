export class BatteryBar {
    constructor(owner) {
        this.owner = owner;
    }

    drawUI(ctx) {
        const x = 16;
        const y = 48;
        const h = 24;
        const maxWidth = 250;
        
        ctx.fillStyle = 'white';
        ctx.fillRect(x, y, maxWidth, h);
        
        const percent = Math.min(100, Math.round(100 * this.owner.batteryCharge / this.owner.batteryCapacity));
        const barWidth = maxWidth * (percent / 100);
        
        ctx.fillStyle = 'green';
        ctx.fillRect(x, y, barWidth, h);
        
        ctx.fillStyle = 'black';
        ctx.fillText(`Battery: ${Math.round(this.owner.batteryCharge)} / ${Math.round(this.owner.batteryCapacity)}`, x + 8, y + 16);
    }
}