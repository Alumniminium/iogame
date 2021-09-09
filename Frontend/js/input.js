export class Input {

    dx = 0;
    dy = 0;

    keyDownHandler(e) {
        let val = e.key.replace('Arrow', '');

        switch (val) {
            case 'Left':
                this.dx = -1;
                break;
            case 'Right':
                this.dx = 1;
                break;
            case 'Up':
                this.dy = -1;
                break;
            case 'Down':
                this.dy = 1;
                break;
        }
    }
    keyUpHandler(e) {
        let val = e.key.replace('Arrow', '');

        switch (val) {
            case 'Left':
            case 'Right':
                this.dx = 0;
                break;
            case 'Up':
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