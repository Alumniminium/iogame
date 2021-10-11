
export class renderer
{
    canvas = document.getElementById('gameCanvas');
    context = this.canvas.getContext('2d');
    camera = null;
    fps = 0;

    constructor(camera)
    {
        this.camera = camera;
        window.addEventListener('resize', this.setCanvasDimensions.bind(this));
        this.setCanvasDimensions();
    }


    setCanvasDimensions()
    {
        this.canvas.width = window.innerWidth;
        this.canvas.height = window.innerHeight;
    }


    clear()
    {
        this.context.fillStyle = "#292d3e";
        this.context.fillRect(0, 0, this.camera.viewport.width, this.camera.viewport.height);
    }
    update(dt)
    {
        this.fps = Math.round(1 / dt);
    }
    draw()
    {
        this.clear();
        this.camera.begin();
        this.drawGridLines();

        for (let i = 0; i < window.game.entitiesArray.length; i++)
        {
            const entity = window.game.entitiesArray[i];
            entity.draw(this.context);
        }

        if (window.showServerPosToggle)
            this.drawCollisionGrid();

        this.camera.end();

        if (window.showServerPosToggle)
            this.DrawPerformanceMetrics();

        this.drawChat();
    }

    drawGridLines()
    {
        let s = 125;
        this.context.strokeStyle = '#041f2d';
        this.context.lineWidth = 4;
        this.context.beginPath();
        for (let x = 0; x <= window.game.MAP_WIDTH; x += s)
        {
            if (x < this.camera.viewport.left || x > this.camera.viewport.right)
                continue;
            this.context.moveTo(x, 0);
            this.context.lineTo(x, window.game.MAP_HEIGHT);
        }
        for (let y = 0; y <= window.game.MAP_HEIGHT; y += s)
        {
            if (y < this.camera.viewport.top || y > this.camera.viewport.bottom)
                continue;
            this.context.moveTo(0, y);
            this.context.lineTo(window.game.MAP_WIDTH, y);
        }
        this.context.stroke();
    }

    drawCollisionGrid()
    {
        let s = 1000;
        this.context.strokeStyle = 'magenta';
        this.context.lineWidth = 8;
        this.context.beginPath();

        for (let x = 0; x <= window.game.MAP_WIDTH; x += s)
        {
            if (x < this.camera.viewport.left || x > this.camera.viewport.right)
                continue;
            this.context.moveTo(x, 0);
            this.context.lineTo(x, window.game.MAP_HEIGHT);
        }
        for (let y = 0; y <= window.game.MAP_HEIGHT; y += s)
        {
            if (y < this.camera.viewport.top || y > this.camera.viewport.bottom)
                continue;
            this.context.moveTo(0, y);
            this.context.lineTo(window.game.MAP_WIDTH, y);
        }
        this.context.stroke();

        this.context.fillStyle = 'magenta';
        this.context.font = '80px Arial';
        for (let x2 = 0; x2 <= window.game.MAP_WIDTH - s; x2 += s)
        {
            for (let y2 = 0; y2 <= window.game.MAP_HEIGHT - s; y2 += s)
                this.context.fillText(`${x2 / s},${y2 / s}`, x2 + s / 2, y2 + s / 2, s);
        }
    }

    DrawPerformanceMetrics()
    {
        this.context.font = '22px monospace';
        this.context.fillStyle = 'white';
        const fps = "FPS:  " + this.fps;
        const ping = "Ping: " + window.ping / 2 + "ms";
        const rtt = "RTT:  " + window.ping + "ms";
        const send = "Total TX: " + Math.round(window.totalBytesSent / 1024 * 100) / 100 + " Kb - TX/s: " + Math.round(window.bytesPerSecondSent / 1024 * 100) / 100 + " Kb/s";
        const recv = "Total RX: " + Math.round(window.totalBytesReceived / 1024 * 100) / 100 + " Kb - RX/s: " + Math.round(window.bytesPerSecondReceived / 1024 * 100) / 100 + " Kb/s";
        this.context.fillText(fps, 32, 32);
        this.context.fillText(ping, 32, 32 * 2);
        this.context.fillText(rtt, 32, 32 * 3);
        this.context.fillText(send, 32, 32 * 4);
        this.context.fillText(recv, 32, 32 * 5);
    }

    drawChat()
    {
        const padding = 16;
        const lineHeight = 32;
        const height = 333;
        const width = 600;
        const x = 16;
        const y = this.canvas.height - height - 16;

        this.context.fillStyle = "#292d3ebf";
        this.context.font = '20px monospace';
        this.context.lineWidth = 8;

        this.context.fillRect(x,y,width,height);

        this.context.strokeStyle = "#25293ae6"
        this.context.beginPath();
        this.context.moveTo(x,y);
        this.context.lineTo(x+width,y);
        this.context.lineTo(x+width,y+height);
        this.context.lineTo(x,height+y);
        this.context.lineTo(x,y);
        this.context.stroke();

        this.context.fillStyle = 'white';
        for (let i = 0; i < 10; i++)
        {
            const yOffset = y + (lineHeight * i);
            const entry = window.chatLog[i];
            this.context.fillText(entry, x+padding, yOffset+padding*2);
        }

    }
}
