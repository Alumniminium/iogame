export class UiRenderer
{
    canvas = document.getElementById('gameCanvas');
    context = this.canvas.getContext('2d');
    totalTime = 0;
    dt = 0;

    constructor()
    {
        window.addEventListener('resize', this.setCanvasDimensions.bind(this));
        this.setCanvasDimensions();
        this.context.font = '16px monospace';
        this.context.fillStyle = 'white';
    }

    setCanvasDimensions()
    {
        this.canvas.width = window.innerWidth;
        this.canvas.height = window.innerHeight;
    }

    update(dt)
    {
        this.dt = dt;
        this.totalTime += dt;
    }

    draw()
    {
        if (window.showServerPosToggle)
            this.DrawPerformanceMetrics();

        this.drawChat();
        this.drawLeaderboard();
        this.DrawInventory();
    }
    
    DrawPerformanceMetrics()
    {
        const fps = "FPS:  " + window.fps;
        const ping = "Ping: " + window.ping / 2 + "ms";
        const rtt = "RTT:  " + window.ping + "ms";
        const ppsr = "PPSrx:  " + window.packetsPerSecondReceived + " packets";
        const send = "Total TX: " + Math.round(window.totalBytesSent     / 1024/ 1024 * 100) / 100 + " MB - TX/s: " + Math.round(window.bytesPerSecondSent     / 1024 * 100) / 100 + " Kb/s";
        const recv = "Total RX: " + Math.round(window.totalBytesReceived / 1024/ 1024 * 100) / 100 + " MB - RX/s: " + Math.round(window.bytesPerSecondReceived / 1024 * 100) / 100 + " Kb/s";
        this.context.fillText(fps, 32, 32);
        this.context.fillText(ping, 32, 32 * 2);
        this.context.fillText(rtt, 32, 32 * 3);
        this.context.fillText(ppsr, 32, 32 * 4);
        this.context.fillText(send, 32, 32 * 5);
        this.context.fillText(recv, 32, 32 * 6);
    }

    DrawInventory()
    {
        const capacity =  "Storage:     " +  window.game.player.playerStorageCapacity + "kg";
        const usage =     "Full:        " + (window.game.player.playerTriangles + window.playerSquares + window.playerPentagons) + "kg";
        const triangles = "△:           " +  window.game.player.playerTriangles + "kg";
        const squares =   "◻:           " +  window.game.player.playerSquares + "kg";
        const pentagons = "⬠:           " + window.game.player.playerPentagons + "kg";
        
        const batteryCapacity   = "Battery Capacity:  " + window.game.player.batteryCapacity + "kWh";
        const batteryCharge     = "Battery Charge:    " + window.game.player.batteryCharge + "kWh";
        const chargeRate        = "Charge Rate:       " + window.game.player.batteryChargeRate + "kW";
        const dischargeRate     = "Discharge Rate:    " + window.game.player.batteryDischargeRate + "kW";

        const enginePowerDraw   = "Engine Power Draw: " + window.game.player.enginePowerDraw + "kW";
        const throttle          = "Throttle:          " + window.game.player.playerThrottle + "%";

        const weaponPowerDraw   = "Weapon Power Draw: " + window.game.player.weaponPowerDraw + "kW";

        const shieldPowerDraw   = "Shield Power Draw: " + window.game.player.shieldPowerUse + "kW";
        const shieldCharge      = "Shield Charge:     " + window.game.player.shieldCharge + "kW";
        const shieldMaxCharge   = "Shield Max Charge: " + window.game.player.shieldMaxCharge + "kW";
        const shieldRadius      = "Shield Radius:     " + window.game.player.shieldRadius + "m";

        
        this.context.fillText(capacity,     this.canvas.width - 32 * 11, this.canvas.height - 32 * 2);
        this.context.fillText(usage,        this.canvas.width - 32 * 11, this.canvas.height - 32 * 3);
        this.context.fillText(triangles,    this.canvas.width - 32 * 11, this.canvas.height - 32 * 4);
        this.context.fillText(squares,      this.canvas.width - 32 * 11, this.canvas.height - 32 * 5);
        this.context.fillText(pentagons,    this.canvas.width - 32 * 11, this.canvas.height - 32 * 6);

        this.context.fillText(throttle,         this.canvas.width - 32 * 11, this.canvas.height - 32 * 7);
        this.context.fillText(chargeRate,       this.canvas.width - 32 * 11, this.canvas.height - 32 * 10);
        this.context.fillText(dischargeRate,    this.canvas.width - 32 * 11, this.canvas.height - 32 * 11);
        this.context.fillText(enginePowerDraw,  this.canvas.width - 32 * 11, this.canvas.height - 32 * 12);
        this.context.fillText(shieldPowerDraw,  this.canvas.width - 32 * 11, this.canvas.height - 32 * 13);
        
        window.game.player.healthBar.drawUI(this.context);
        window.game.player.shieldBar.drawUI(this.context);
        window.game.player.batteryBar.drawUI(this.context);
        window.game.player.expBar.drawUI(this.context);
    }

    drawChat()
    {
        const padding = 8;
        const lineHeight = 26;
        const height = 500;
        const width = 700;
        const x = 8;
        const y = this.canvas.height - height - 16;

        this.context.fillStyle = "#292d3ebf";
        this.context.lineWidth = 6;

        this.context.fillRect(x, y, width, height);

        this.context.strokeStyle = "#25293ae6";
        this.context.beginPath();
        this.context.moveTo(x, y);
        this.context.lineTo(x + width, y);
        this.context.lineTo(x + width, y + height);
        this.context.lineTo(x, height + y);
        this.context.lineTo(x, y);
        this.context.stroke();

        this.context.fillStyle = 'white';
        for (let i = 0; i < 18; i++)
        {
            const yOffset = y + (lineHeight * i);
            const entry = window.chatLog[i];
            this.context.fillText(entry, x + padding, yOffset + padding * 2);
        }
    }
    drawLeaderboard()
    {
        const padding = 8;
        const lineHeight = 26;
        const height = 130;
        const width = 260;
        const x = this.canvas.width - width - padding;
        const y = padding;

        this.context.fillStyle = "#092d3ebf";
        this.context.lineWidth = 6;

        this.context.fillRect(x, y, width, height);

        this.context.strokeStyle = "#25293ae6";
        this.context.beginPath();
        this.context.moveTo(x, y);
        this.context.lineTo(x + width, y);
        this.context.lineTo(x + width, y + height);
        this.context.lineTo(x, height + y);
        this.context.lineTo(x, y);
        this.context.stroke();

        this.context.fillStyle = 'white';
        for (let i = 0; i < 5; i++)
        {
            const yOffset = y + (lineHeight * i);
            const entry = window.leaderboardLog[i];
            this.context.fillText(entry, x + padding, yOffset + padding * 2);
        }
    }
}
