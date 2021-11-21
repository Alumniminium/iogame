export class uiRenderer
{
    canvas = document.getElementById('uiCanvas');
    context = this.canvas.getContext('2d');

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
    clear()
    {
        // this.context.globalAlpha = 0;
        this.context.clearRect(0, 0, this.canvas.width, this.canvas.height);
    }
    update(dt)
    {
    }
    draw()
    {
        this.clear();
        if (window.showServerPosToggle)
            this.DrawPerformanceMetrics();

        this.drawChat();
    }
    DrawPerformanceMetrics()
    {
        const fps = "FPS:  " + window.fps;
        const ping = "Ping: " + window.ping / 2 + "ms";
        const rtt = "RTT:  " + window.ping + "ms";
        const ppsr = "PPSrx:  " + window.packetsPerSecondReceived + " packets";
        const send = "Total TX: " + Math.round(window.totalBytesSent / 1024 * 100) / 100 + " Kb - TX/s: " + Math.round(window.bytesPerSecondSent / 1024 * 100) / 100 + " Kb/s";
        const recv = "Total RX: " + Math.round(window.totalBytesReceived / 1024 * 100) / 100 + " Kb - RX/s: " + Math.round(window.bytesPerSecondReceived / 1024 * 100) / 100 + " Kb/s";
        this.context.fillText(fps, 32, 32);
        this.context.fillText(ping, 32, 32 * 2);
        this.context.fillText(rtt, 32, 32 * 3);
        this.context.fillText(send, 32, 32 * 4);
        this.context.fillText(ppsr, 32, 32 * 5);
        this.context.fillText(recv, 32, 32 * 6);
    }

    drawChat()
    {
        const padding = 16;
        const lineHeight = 26;
        const height = 540;
        const width = 700;
        const x = 16;
        const y = this.canvas.height - height - 16;

        this.context.fillStyle = "#292d3ebf";
        this.context.lineWidth = 8;

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
}
