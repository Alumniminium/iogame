import { Camera } from "./camera.js";

export class renderer {
    canvas = document.getElementById('gameCanvas');
    context = this.canvas.getContext('2d');
    camera = null;
    game = null;
    fps = 0;

    constructor(game, camera) {
        this.game = game;
        this.camera = camera;

        window.addEventListener('resize', this.setCanvasDimensions.bind(this));

        this.setCanvasDimensions();
    }


    setCanvasDimensions() {
        this.canvas.width = window.innerWidth;
        this.canvas.height = window.innerHeight;
    }


    clear() {
        this.context.fillStyle = "#292d3e";
        this.context.fillRect(0, 0, this.canvas.width, this.canvas.height);
        this.context.fillRect(0, 0, this.camera.viewport.width, this.camera.viewport.height);
    }
    update(dt)
    {
        this.fps = Math.round(1 / dt);
    }
    draw() {
        this.clear();
        this.camera.begin();
        this.drawGridLines();

        this.context.strokeStyle = "#fffff";
        this.context.strokeRect(8, 8, this.game.MAP_WIDTH - 8, this.game.MAP_HEIGHT - 8);

        for (let i = 0; i < this.game.entitiesArray.length; i++) {
            const entity =  this.game.entitiesArray[i];
            entity.draw(this.context);
        }
        this.camera.end();
        this.drawFpsCounter();
    }

    drawGridLines() {
        let s = 125;
        this.context.strokeStyle = '#041f2d';
        this.context.lineWidth = 4;
        this.context.beginPath();
        for (let x = s; x <= this.game.MAP_WIDTH - s; x += s) {
            this.context.moveTo(x, s);
            this.context.lineTo(x, this.game.MAP_HEIGHT - s);
        }
        for (let y = s; y <= this.game.MAP_HEIGHT - s; y += s) {
            this.context.moveTo(s, y);
            this.context.lineTo(this.game.MAP_WIDTH - s, y);
        }
        this.context.stroke();

        s = 3000;
        this.context.strokeStyle = 'magenta';
        this.context.lineWidth = 8;
        this.context.beginPath();
        for (let x = s; x <= this.game.MAP_WIDTH - s; x += s) {
            this.context.moveTo(x, s);
            this.context.lineTo(x, this.game.MAP_HEIGHT - s);
        }
        for (let y = s; y <= this.game.MAP_HEIGHT - s; y += s) {
            this.context.moveTo(s, y);
            this.context.lineTo(this.game.MAP_WIDTH - s, y);
        }
        this.context.stroke();

        this.context.fillStyle = "magenta";
        this.context.font = '80px Arial';
        for (let x2 = 0; x2 <= this.game.MAP_WIDTH - s; x2 += s) {
            for (let y2 = 0; y2 <= this.game.MAP_HEIGHT - s; y2 += s)
                this.context.fillText(`${x2 / s},${y2 / s}`, x2 + s / 2, y2 + s / 2, s);
        }
    }

    drawFpsCounter() {
        this.context.font = '20px Arial';
        this.context.fillStyle = 'white';
        const fpsString = "FPS: " + this.fps;
        const stringSize = this.context.measureText(this.fpsString);
        this.context.fillText(fpsString, stringSize.width * 0.25, stringSize.fontBoundingBoxAscent * 2);
    }

}
