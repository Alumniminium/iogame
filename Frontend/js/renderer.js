
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
        this.context.clearRect(0, 0, this.canvas.width, this.canvas.height);
    }
    update(dt)
    {
        this.fps = Math.round(1 / dt);
        window.game.entitiesArray = window.game.entitiesArray.sort((a, b) => a.sides - b.sides);
    }
    draw()
    {
        this.clear();
        this.camera.begin();
        this.drawGridLines();

        this.context.lineWidth = 16;

        let x = 0;
        for (let i = 0; i < window.game.entitiesArray.length; i++)
        {
            const entity = window.game.entitiesArray[i];

            if (x != entity.sides)
            {
                this.context.fillStyle = entity.fillColor;
                x = entity.sides;
            }
            entity.draw(this.context);
        }
        this.context.fillStyle = 'white';
        for (let i = 0; i < window.game.entitiesArray.length; i++)
        {
            const entity = window.game.entitiesArray[i];
            if (entity.health == entity.maxHealth)
                continue;
            entity.healthBar.drawBg(this.context);
        }
        this.context.fillStyle = 'red';
        for (let i = 0; i < window.game.entitiesArray.length; i++)
        {
            const entity = window.game.entitiesArray[i];
            if (entity.health == entity.maxHealth)
                continue;
            entity.healthBar.drawFg(this.context);
        }
        if (window.showCollisionGrid)
            this.drawCollisionGrid();

        if (window.showServerPosToggle)
        {
            this.context.fillStyle = "#ff9933";
            // this.context.strokeStyle = "#663300";
            for (let i = 0; i < window.game.entitiesArray.length; i++)
                window.game.entitiesArray[i].DrawServerPosition(this.context);
        }
        this.camera.end();
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
        let s = 500;
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

        // this.context.fillStyle = 'magenta';
        // this.context.font = '40px Arial';
        // for (let x2 = 0; x2 <= window.game.MAP_WIDTH - s; x2 += s)
        // {
        //     for (let y2 = 0; y2 <= window.game.MAP_HEIGHT - s; y2 += s)
        //         this.context.fillText(`${x2 / s},${y2 / s}`, x2 + s / 2, y2 + s / 2, s);
        // }
    }
}
