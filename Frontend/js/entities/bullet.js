import { Entity } from "./entity.js";

export class Bullet extends Entity {

    constructor(id, owner) {
        super(id);
        this.owner = owner;
    }
}
