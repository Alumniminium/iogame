import { Vector } from "./vector.js";

export class Input
{

    game = null;
    left = false;
    right = false;
    down = false;
    up = false;
    lmb = false;
    rmb = false;
    mpos = new Vector(0, 0);
    changed = false;
    posChanged = false;

    mouseDownHandler(e)
    {
        e.preventDefault();
        this.changed = true;

        switch (e.button)
        {
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
    mouseMoveHandler(e)
    {
        e.preventDefault();
        this.mpos = new Vector(e.offsetX, e.offsetY);
        this.posChanged = true;
    }
    mouseUpHandler(e)
    {
        e.preventDefault();
        this.changed = true;
        switch (e.button)
        {
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
    keyDownHandler(e)
    {
        if (e.repeat) { return; }
        let val = e.key.replace('Arrow', '');
        this.changed = true;
        const chatNode = document.getElementById("chatInputContainer");
        if (chatNode.style.display != "block")
        {
            switch (val)
            {
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
                case "p":
                    window.showServerPosToggle = !window.showServerPosToggle;
                    break;
                default:
                    console.log(val);
                    this.changed = false;
                    break;
            }
        }
    }
    keyUpHandler(e)
    {
        if (e.repeat) { return; }
        let val = e.key.replace('Arrow', '');
        this.changed = true;
        const chatNode = document.getElementById("chatInputContainer");
        const input = document.getElementById("chatInput");
        if (chatNode.style.display == "block")
        {
            if (val == "Enter")
            {
                const message = input.value;
                input.value = "";
                chatNode.style.display = "none";

                this.game.sendMessage(message);
            }
        }
        else
        {
            switch (val)
            {
                case "Enter":
                    const chatNode = document.getElementById("chatInputContainer");
                    const input = document.getElementById("chatInput");
                    chatNode.style.display = chatNode.style.display == "none" ? "block" : "none";
                    input.focus();
                    break;
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
    }
    setup(game)
    {
        this.game = game;
        document.addEventListener("keydown", this.keyDownHandler.bind(this));
        document.addEventListener("keyup", this.keyUpHandler.bind(this));
        game.renderer.canvas.addEventListener("mousedown", this.mouseDownHandler.bind(this));
        game.renderer.canvas.addEventListener("mouseup", this.mouseUpHandler.bind(this));
        game.renderer.canvas.addEventListener("mousemove", this.mouseMoveHandler.bind(this));
    }
}