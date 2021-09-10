import { Vector } from "./vector.js";

export class Input {
    left = false;
    right = false;
    down = false;
    up=false;

    keyDownHandler(e) {
        e.preventDefault();
        let val = e.key.replace('Arrow', '');
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
                this.up=true;
                break;
            case "s":
            case 'Down':
                this.down=true;
                break;
        }
    }
    keyUpHandler(e) {
        e.preventDefault();
        let val = e.key.replace('Arrow', '');

        switch (val) {
            case "a":
            case 'Left':
                this.left=false;
                break;
            case "d":
            case 'Right':
                this.right = false;
                break;
            case 'Up':
            case "w":
                this.up=false;
                break;
            case "s":
            case 'Down':
                this.down = false;
                break;
        }
    }
    setup() {
        document.addEventListener("keydown", this.keyDownHandler.bind(this));
        document.addEventListener("keyup", this.keyUpHandler.bind(this));
    }
}