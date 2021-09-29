import { Vector } from "./vector.js";

export class Input {

    game = null;

    left = false;
    right = false;
    down = false;
    up = false;
    lmb = false;
    rmb = false;
    mpos = new Vector(0, 0);
    changed = false;

    mouseDownHandler(e) {
        e.preventDefault();
        this.changed = true;

        switch (e.button) {
            case 0:
                this.lmb = true;
                break;
            case 1:
                this.rmb = true;
                break;
            default:
                this.changed = false;
                break;
        }
    }
    mouseMoveHandler(e) {
        e.preventDefault();
        this.mpos = new Vector(e.offsetX, e.offsetY);
    }
    mouseUpHandler(e) {
        e.preventDefault();
        this.changed = true;
        switch (e.button) {
            case 0:
                this.lmb = false;
                break;
            case 1:
                this.rmb = false;
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
                this.lmb = true;
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
                this.lmb = false;
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
        game.renderer.canvas.addEventListener("mousedown", this.mouseDownHandler.bind(this));
        game.renderer.canvas.addEventListener("mouseup", this.mouseUpHandler.bind(this));
        game.renderer.canvas.addEventListener("mousemove", this.mouseMoveHandler.bind(this));
    }
}