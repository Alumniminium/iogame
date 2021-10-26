import { Vector } from "./vector.js";

export class Input
{
    left = false;
    right = false;
    down = false;
    up = false;
    lmb = false;
    rmb = false;
    mpos = new Vector(0, 0);
    changed = false;
    posChanged = false;

    setup(game)
    {
        this.game = game;
        this.chatNode = document.getElementById("chatInputContainer");
        this.input = document.getElementById("chatInput");

        document.addEventListener("keydown", this.keyDownHandler.bind(this));
        document.addEventListener("keyup", this.keyUpHandler.bind(this));

        window.game.renderer.canvas.addEventListener("mousedown", this.mouseDownHandler.bind(this));
        window.game.renderer.canvas.addEventListener("mouseup", this.mouseUpHandler.bind(this));
        window.game.renderer.canvas.addEventListener("mousemove", this.mouseMoveHandler.bind(this));

        window.game.renderer.canvas.addEventListener("touchstart", function (e)
        {
            e.preventDefault();
            var touch = e.touches[0];
            var mouseEvent = new MouseEvent("mousedown", 
            {
                clientX: touch.clientX,
                clientY: touch.clientY
            });
            window.game.renderer.canvas.dispatchEvent(mouseEvent);
        });

        window.game.renderer.canvas.addEventListener("touchend", (e) =>
        {
            e.preventDefault();
            var mouseEvent = new MouseEvent("mouseup", {});
            window.game.renderer.canvas.dispatchEvent(mouseEvent);
        });

        window.game.renderer.canvas.addEventListener("touchmove", (e) =>
        {
            e.preventDefault();
            var touch = e.touches[0];
            var mouseEvent = new MouseEvent("mousemove", {
                clientX: touch.clientX,
                clientY: touch.clientY
            });
            window.game.renderer.canvas.dispatchEvent(mouseEvent);
        });
    }

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

        if (this.chatNode.style.display == "block") // if the chat input is visible
            return;                                 // don't.

        this.changed = true;
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
                this.changed = false; // server doesn't need to know
                break;
            case "c":
                window.showCollisionGrid = !window.showCollisionGrid;
                this.changed = false; // server doesn't need to know
                break;

            default:
                // console.log(val);
                this.changed = false;
                break;
        }
    }
    keyUpHandler(e)
    {
        if (e.repeat) { return; }
        let val = e.key.replace('Arrow', '');
        if (this.chatNode.style.display == "block")
        {
            if (val == "Enter")
            {
                const message = this.input.value;
                this.input.value = "";
                this.chatNode.style.display = "none";

                this.game.sendMessage(message);
            }
        }
        else
        {
            this.changed = true;
            switch (val)
            {
                case "Enter":
                    this.chatNode.style.display = this.chatNode.style.display == "none" ? "block" : "none";
                    this.input.focus();
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
                    // console.log(val);
                    this.changed = false;
                    break;
            }
        }
    }
}