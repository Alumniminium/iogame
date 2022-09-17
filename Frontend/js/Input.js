import { Packets } from "./network/Packets.js";
import { Vector } from "./Vector.js";

export class Input
{
    left = false;
    right = false;
    down = false;
    up = false;
    lmb = false;
    rmb = false;
    rcs = true;
    drop = false;
    mpos = new Vector(0, 0);
    changed = false;
    posChanged = false;
    renderer = null;

    setup(game)
    {
        this.game = game;
        this.chatNode = document.getElementById("chatInputContainer");
        this.input = document.getElementById("chatInput");

        this.renderer = game.uiRenderer;

        document.addEventListener("keydown", this.keyDownHandler.bind(this));
        document.addEventListener("keyup", this.keyUpHandler.bind(this));

        this.renderer.canvas.addEventListener("mousedown", this.mouseDownHandler.bind(this));
        this.renderer.canvas.addEventListener("mouseup", this.mouseUpHandler.bind(this));
        this.renderer.canvas.addEventListener("mousemove", this.mouseMoveHandler.bind(this));
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
        this.sendPacket();
    }
    mouseMoveHandler(e)
    {
        e.preventDefault();
        this.mpos = new Vector(e.offsetX * window.resolutionMultiplier, e.offsetY * window.resolutionMultiplier);
        this.posChanged = true;
        this.sendPacket();
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
        this.sendPacket();
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
            case "A":
            case 'Left':
                this.left = true;
                break;
            case "d":
            case "D":
            case 'Right':
                this.right = true;
                break;
            case "w":
            case "W":
            case 'Up':
                this.up = true;
                break;
            case "s":
            case "S":
            case 'Down':
                this.down = true;
                break;
            case 'u':
            case 'U':
                this.drop = true;
                break;
            case " ":
                this.lmb = true;
                break;
            case "Control":
                this.rcs = false;
                break;
            case "Shift":
                this.boost = true;
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
                console.log(val);
                this.changed = false;
                break;
        }
        this.sendPacket();
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
                case "a": ;
                case "A":
                case 'Left':
                    this.left = false;
                    break;
                case "d":
                case "D":
                case 'Right':
                    this.right = false;
                    break;
                case 'Up':
                case "W":
                case "w":
                    this.up = false;
                    break;
                case "s":
                case "S":
                case 'Down':
                    this.down = false;
                    break;
                case 'u':
                case "U":
                    this.drop = false;
                    break;
                case " ":
                    this.lmb = false;
                    break;
                case "Control":
                    this.rcs = true;
                    break;
                case "Shift":
                    this.boost = false;
                    break;
                case "r":
                case "R":
                    this.rcs = !this.rcs;
                    break;
                default:
                    console.log(val);
                    this.changed = false;
                    break;
            }
            this.sendPacket();
        }
    }

    sendPacket()
    {
        if (window.input.posChanged && new Date().getTime() > window.game.player.lastShot + 25)
        {
            window.input.changed = true;
            window.game.player.lastShot = new Date().getTime();
        }

        if (window.input.changed)
        {
            window.input.changed = false;
            window.input.posChanged = false;
            let pos = window.game.camera.screenToWorld(window.input.mpos.x, window.input.mpos.y);
            var d = window.game.player.position.subtract(pos).unit();

            window.game.net.send(Packets.MovementPacket(window.game.player, window.input.up, window.input.down, window.input.left, window.input.right, window.input.lmb, window.input.boost, window.input.rcs, window.input.drop, -d.x, -d.y));
        }
    }
}