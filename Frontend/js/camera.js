import { Entity } from "./entities/entity.js";
import { Vector } from "./vector.js";

export class Camera {
    constructor(context, settings) {
        settings = settings || {};
        this.distance = 1000.0;
        this.lookAt = new Vector(0, 0);
        this.context = context;
        this.fieldOfView = settings.fieldOfView || Math.PI / 4.0;
        this.viewport = {
            left: 0,
            right: 0,
            top: 0,
            bottom: 0,
            width: 0,
            height: 0,
            scale: new Vector(1.0, 1.0)
        };

        this.updateViewport();
    }

    begin() {
        this.context.save();
        this.applyScale();
        this.applyTranslation();
    }

    end() {
        this.context.restore();
    }

    canSee(entity)
    {
        if (entity.originX() > this.viewport.left && entity.originX() < this.viewport.right) 
            if (entity.originY() > this.viewport.top && entity.originY() < this.viewport.bottom)
                return true;
        return false;
    }

    applyScale() {
        this.context.scale(this.viewport.scale.x, this.viewport.scale.y);
    }

    applyTranslation() {
        this.context.translate(-this.viewport.left, -this.viewport.top);
    }

    updateViewport() {
        this.aspectRatio = this.context.canvas.width / this.context.canvas.height;
        this.viewport.width = this.distance * Math.tan(this.fieldOfView);
        this.viewport.height = this.viewport.width / this.aspectRatio;
        this.viewport.left = this.lookAt.x - (this.viewport.width / 2.0);
        this.viewport.top = this.lookAt.y - (this.viewport.height / 2.0);
        this.viewport.right = this.viewport.left + this.viewport.width;
        this.viewport.bottom = this.viewport.top + this.viewport.height;
        this.viewport.scale.x = this.context.canvas.width / this.viewport.width;
        this.viewport.scale.y = this.context.canvas.height / this.viewport.height;
    }

    zoomTo(z) {
        this.distance = z;
        this.updateViewport();
    }

    moveTo(vector) {
        this.lookAt = vector;
        this.updateViewport();
    }

    screenToWorld(x, y) {
        return new Vector((x / this.viewport.scale.x) + this.viewport.left, (y / this.viewport.scale.y) + this.viewport.top); 
    }

    worldToScreen(x, y, obj) {
        obj = obj || {};
        obj.x = (x - this.viewport.left) * (this.viewport.scale.x);
        obj.y = (y - this.viewport.top) * (this.viewport.scale.y);
        return obj;
    }
};