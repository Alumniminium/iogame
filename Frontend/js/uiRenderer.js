export class uiRenderer
{
    canvas = document.getElementById('uiCanvas');
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
    clear()
    {
        // this.context.globalAlpha = 0;
        this.context.clearRect(0, 0, this.canvas.width, this.canvas.height);
    }
    update(dt)
    {
        this.dt = dt;
        this.totalTime += dt;
    }
    draw()
    {
        this.clear();
        if (window.showServerPosToggle)
            this.DrawPerformanceMetrics();

        this.drawChat();
        this.DrawInventory();
    }
    DrawPerformanceMetrics()
    {
        const fps = "FPS:  " + window.fps;
        const ping = "Ping: " + window.ping / 2 + "ms";
        const rtt = "RTT:  " + window.ping + "ms";
        const ppsr = "PPSrx:  " + window.packetsPerSecondReceived + " packets";
        const send = "Total TX: " + Math.round(window.totalBytesSent / 1024/ 1024 * 100) / 100 + " MB - TX/s: " + Math.round(window.bytesPerSecondSent / 1024 * 100) / 100 + " Kb/s";
        const recv = "Total RX: " + Math.round(window.totalBytesReceived / 1024/ 1024 * 100) / 100 + " MB - RX/s: " + Math.round(window.bytesPerSecondReceived / 1024 * 100) / 100 + " Kb/s";
        this.context.fillText(fps, 32, 32);
        this.context.fillText(ping, 32, 32 * 2);
        this.context.fillText(rtt, 32, 32 * 3);
        this.context.fillText(send, 32, 32 * 4);
        this.context.fillText(ppsr, 32, 32 * 5);
        this.context.fillText(recv, 32, 32 * 6);

        // for(let entity of window.game.entitiesArray)
        // {
        //     console.log(entity);
        // }
        // const cx = 400;
        // const cy = 400;
        // const mouseY = window.input.mpos.y;
        // const mouseX = window.input.mpos.x;
        // const radius = 100;
        // const quadWidth = Math.PI;     // area of effect PI/2 is 90 degree
        // const steps = radius / quadWidth; // number steps around the circle matches 1 pixel per step, 
        // const noiseAmpMax = 15;         // in pixels
        // const noiseWaveMoveSpeed = 3;  // speed of waves on circle in radians per second
        // const noiseWaveFreq = 32;

        // var amp = 0; // amplitude of noise 
        // var wavePos = ((this.totalTime) * Math.PI) * noiseWaveMoveSpeed;
        // var mouseDir = Math.atan2(mouseY - cy, mouseX - cx);

        // this.context.beginPath();
        // this.context.strokeStyle = "#fff";
        // this.context.fillStyle = "red";
        // // draw arc for parts that have no noise as it is a log quicker
        // this.context.arc(cx, cy, radius, mouseDir + quadWidth / 2, mouseDir + Math.PI * 2 - quadWidth / 2);
        // for (var a = 0; a < 1; a += 1 / steps) {
        //     var angle = (mouseDir - quadWidth / 2) + a * quadWidth;
        //     var angDist = Math.abs(angle - mouseDir); // find angular distance from mouse
        //                                             // as a positive value, it does not mater 
        //                                             // what the sign is
        //     if (angDist < quadWidth / 2) { // is angle distance within the range of effect
        //                                 // normalise the distance (make it 0 to 1)
        //     amp = 1 - angDist / (quadWidth / 2);
        //     } else {
        //     amp = 0; // no noise
        //     }
        //     // amp will be zero if away from mouse direction and 0 to 1 the closer to 
        //     // mouse angle it gets.
        //     // add a sin wave to the radius and scale it by amp
        //     var dist = radius + Math.sin(wavePos + noiseWaveFreq * angle) * noiseAmpMax * amp;
        //     var x = cx + dist * Math.cos(angle);
        //     var y = cy + dist * Math.sin(angle);
        //     this.context.lineTo(x, y);
        // }
        // this.context.closePath(); // use close path to close the gap (only needed if you need to draw a line from the end to the start. It is not needed to match beginPath
        // this.context.fill();
        // this.context.stroke();
    }

    DrawInventory()
    {
        const capacity =  "Storage Capacity: " + window.playerStorageCapacity + "kg";
        const usage =     "Utilization:      " + (window.playerTriangles + window.playerSquares + window.playerPentagons) + "kg";
        const triangles = "△:            " + window.playerTriangles + "kg";
        const squares =   "◻:            " + window.playerSquares + "kg";
        const pentagons = "⬠:            " + window.playerPentagons + "kg";
        
        const throttle    = "Throttle:      " + window.playerThrottle + "%";
        const totalPower  = "Total Power:   " + window.playerTotalPower + "kW";
        const enginePower = "Engine Power:  " + window.playerEnginePower + "kW";
        const shieldPower = "Shield Power:  " + window.playerShieldPower + "kW";
        const weaponPower = "Weapon Power:  " + window.playerWeaponPower + "kW";

        
        this.context.fillText(capacity, this.canvas.width / 2, 32 * 2);
        this.context.fillText(usage, this.canvas.width / 2, 32 * 3);
        this.context.fillText(triangles, this.canvas.width / 2, 32 * 4);
        this.context.fillText(squares, this.canvas.width / 2, 32 * 5);
        this.context.fillText(pentagons, this.canvas.width / 2, 32 * 6);


        this.context.fillText(throttle,     this.canvas.width / 2, 32 * 7);
        this.context.fillText(totalPower,   this.canvas.width / 2, 32 * 8);
        this.context.fillText(enginePower,  this.canvas.width / 2, 32 * 9);
        this.context.fillText(shieldPower,  this.canvas.width / 2, 32 * 10);
        this.context.fillText(weaponPower,  this.canvas.width / 2, 32 * 11);
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
}
