
export class Renderer
{
    canvas = document.getElementById('gameCanvas');
    context = this.canvas.getContext('2d');
    camera = null;
    frames = 0;

    constructor(camera)
    {
        window.resolutionMultiplier = 1;
        this.camera = camera;
        window.addEventListener('resize', this.setCanvasDimensions.bind(this));
        this.setCanvasDimensions();
    }


    setCanvasDimensions()
    {
        let dpi = window.devicePixelRatio * window.resolutionMultiplier;
        let style_height = +getComputedStyle(this.canvas).getPropertyValue("height").slice(0, -2);
        let style_width = +getComputedStyle(this.canvas).getPropertyValue("width").slice(0, -2);
        
        this.canvas.setAttribute('height', style_height * dpi);
        this.canvas.setAttribute('width', style_width * dpi);
    }


    clear()
    {
        this.context.clearRect(0, 0, this.canvas.width, this.canvas.height);
    }

    update(dt)
    {
        this.frames++;
        window.fps = Math.round(1 / dt);
        // window.game.entitiesArray = window.game.entitiesArray.sort((a, b) => a.sides - b.sides);
    }
    
    draw()
    {
        this.clear();
        this.camera.begin();
        this.drawGridLines();

        this.context.lineWidth = 1;
        this.context.fillStyle = 'magenta';

        let x = 0;
        for (let i = 0; i < window.game.entitiesArray.length; i++)
        {
            const entity = window.game.entitiesArray[i];
            entity.draw(this.context);
        }
        this.context.fillStyle = 'white';
        for (let i = 0; i < window.game.entitiesArray.length; i++)
        {
            const entity = window.game.entitiesArray[i];
            if (entity.health != entity.maxHealth)
                entity.healthBar.drawBg(this.context);
            if(entity.shieldCharge != entity.shieldMaxCharge)
                entity.shieldBar.drawBg(this.context);
            if(entity.batteryCharge != entity.batteryCapacity)
                entity.batteryBar.drawBg(this.context);
        }
        this.context.fillStyle = 'red';
        for (let i = 0; i < window.game.entitiesArray.length; i++)
        {
            const entity = window.game.entitiesArray[i];
            if (entity.health != entity.maxHealth)
                entity.healthBar.drawFg(this.context);
        }
        this.context.fillStyle = 'blue';
        for (let i = 0; i < window.game.entitiesArray.length; i++)
        {
            const entity = window.game.entitiesArray[i];
            if(entity.shieldCharge != entity.shieldMaxCharge)
                entity.shieldBar.drawFg(this.context);
        }
        this.context.fillStyle = 'green';
        for (let i = 0; i < window.game.entitiesArray.length; i++)
        {
            const entity = window.game.entitiesArray[i];
            if(entity.batteryCharge != entity.batteryCapacity)
                entity.batteryBar.drawFg(this.context);
        }
        this.camera.end();
    }

    drawGridLines()
    {
        let s = 20;
        this.context.strokeStyle = '#041f2d';
        this.context.lineWidth = 1;
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
}
