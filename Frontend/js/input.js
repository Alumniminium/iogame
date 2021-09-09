export class Input {

    dx = 0;
    dy = 0;

    keyDownHandler(e) {
        e.preventDefault();
        let val = e.key.replace('Arrow', '');
        // console.log(val);
        switch (val) {
            case "a":
            case 'Left':
                this.dx = -1;
                break;
            case "d":
            case 'Right':
                this.dx = 1;
                break;
            case "w":
            case 'Up':
                this.dy = -1;
                break;
            case "s":
            case 'Down':
                this.dy = 1;
                break;
        }
    }
    keyUpHandler(e) {
        e.preventDefault();
        let val = e.key.replace('Arrow', '');

        switch (val) {
            case "a":
            case 'Left':
            case "d":
            case 'Right':
                this.dx = 0;
                break;
            case 'Up':
            case "w":
            case "s":
            case 'Down':
                this.dy = 0;
                break;
        }
    }
    setup() {
        document.addEventListener("keydown", this.keyDownHandler.bind(this), false);
        document.addEventListener("keyup", this.keyUpHandler.bind(this), false);
    }
}