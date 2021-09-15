import { Vector } from "./vector.js";

export class Input {

    game = null;

    left = false;
    right = false;
    down = false;
    up = false;
    lmb = false;
    rmb = false;
    mpos = new Vector(0,0);
    changed = false;

    mouseDownHandler(e) {
        e.preventDefault();
        this.changed = true;
        const worldPos = this.game.camera.screenToWorld(e.offsetX, e.offsetY);

        switch (e.button) {
            case 0:
                this.lmb = true;
                console.log(`LMBv: x:${e.offsetX}, y:${e.offsetY} |-> ${worldPos.x},${worldPos.y}`);
                break;
            case 1:
                this.rmb = true;
                console.log(`RMBv: x:${e.offsetX}, y:${e.offsetY} |-> ${worldPos.x},${worldPos.y}`);
                break;
            default:
                this.changed = false;
                break;
        }
    }
    mouseMoveHandler(e) {
        e.preventDefault();
        const worldPos = this.game.camera.screenToWorld(e.offsetX, e.offsetY);
        this.mpos = worldPos;
        console.log(`Mouse: x:${e.offsetX}, y:${e.offsetY} |-> ${worldPos.x},${worldPos.y}`);
    }
    mouseUpHandler(e) {
        e.preventDefault();
        this.changed = true;
        const worldPos = this.game.camera.screenToWorld(e.offsetX, e.offsetY);
        
        for(let i = 0; i<this.game.entitiesArray.length; i++)
        {
            const entity = this.game.entitiesArray[i];
            if (entity.checkCollision_Point(worldPos))
                entity.fillColor = "white";
        }

        switch (e.button) {
            case 0:
                this.lmb = false;
                console.log(`LMB^: x:${e.offsetX}, y:${e.offsetY} |-> ${worldPos.x},${worldPos.y}`);
                break;
            case 1:
                this.rmb = false;
                console.log(`RMB^: x:${e.offsetX}, y:${e.offsetY} |-> ${worldPos.x},${worldPos.y}`);
                break;
            default:
                this.changed = false;
                break;
        }
    }
    keyDownHandler(e) {
        e.preventDefault();
        if (e.repeat) { return }
        let val = e.key.replace('Arrow', '');
        console.log('input');
        this.changed = true;
        // console.log(val);
        switch (val) {
            case "a":
            case 'Left':
                this.left = true;
                break;
            case "d":
            case 'Right':
                this.right = true;
                break;
            case "w":
            case 'Up':
                this.up = true;
                break;
            case "s":
            case 'Down':
                this.down = true;
                break;
            case " ":
                console.log("space");
                break;
            default:
                console.log(val);
                this.changed = false;
                break;
        }
    }
    keyUpHandler(e) {
        e.preventDefault();
        if (e.repeat) { return }
        let val = e.key.replace('Arrow', '');
        console.log('input');
        this.changed = true;

        switch (val) {
            case "a":
            case 'Left':
                this.left = false;
                break;
            case "d":
            case 'Right':
                this.right = false;
                break;
            case 'Up':
            case "w":
                this.up = false;
                break;
            case "s":
            case 'Down':
                this.down = false;
                break;
            case " ":
                console.log("space");
                break;
            default:
                console.log(val);
                this.changed = false;
                break;
        }
    }
    setup(game) {
        this.game = game;
        document.addEventListener("keydown", this.keyDownHandler.bind(this));
        document.addEventListener("keyup", this.keyUpHandler.bind(this));
        game.canvas.addEventListener("mousedown", this.mouseDownHandler.bind(this));
        game.canvas.addEventListener("mouseup", this.mouseUpHandler.bind(this));
        game.canvas.addEventListener("mousemove", this.mouseMoveHandler.bind(this));
    }
}