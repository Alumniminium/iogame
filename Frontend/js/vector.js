export class Vector {
    constructor(x, y) {
        this.x = x || 0;
        this.y = y || 0;
    }
    /* INSTANCE METHODS */
    negative() {
        this.x = -this.x;
        this.y = -this.y;
        return this;
    }
    /* INSTANCE METHODS */
    add(v) {
        if (v instanceof Vector) {
            this.x += v.x;
            this.y += v.y;
        } else {
            this.x += v;
            this.y += v;
        }
        return this;
    }
    /* INSTANCE METHODS */
    subtract(v) {
        if (v instanceof Vector) {
            this.x -= v.x;
            this.y -= v.y;
        } else {
            this.x -= v;
            this.y -= v;
        }
        return this;
    }
    /* INSTANCE METHODS */
    multiply(v) {
        if (v instanceof Vector) {
            this.x *= v.x;
            this.y *= v.y;
        } else {
            this.x *= v;
            this.y *= v;
        }
        return this;
    }
    /* INSTANCE METHODS */
    divide(v) {
        if (v instanceof Vector) {
            if (v.x != 0)
                this.x /= v.x;
            if (v.y != 0)
                this.y /= v.y;
        } else {
            if (v != 0) {
                this.x /= v;
                this.y /= v;
            }
        }
        return this;
    }
    /* INSTANCE METHODS */
    equals(v) {
        return this.x == v.x && this.y == v.y;
    }
    /* INSTANCE METHODS */
    dot(v) {
        return this.x * v.x + this.y * v.y;
    }
    /* INSTANCE METHODS */
    cross(v) {
        return this.x * v.y - this.y * v.x;
    }
    /* INSTANCE METHODS */
    length() {
        return Math.sqrt(this.dot(this));
    }
    /* INSTANCE METHODS */
    normalize() {
        return this.divide(this.length());
    }
    /* INSTANCE METHODS */
    min() {
        return Math.min(this.x, this.y);
    }
    /* INSTANCE METHODS */
    max() {
        return Math.max(this.x, this.y);
    }
    /* INSTANCE METHODS */
    toAngles() {
        return -Math.atan2(-this.y, this.x);
    }
    /* INSTANCE METHODS */
    angleTo(a) {
        return Math.acos(this.dot(a) / (this.length() * a.length()));
    }
    /* INSTANCE METHODS */
    toArray(n) {
        return [this.x, this.y].slice(0, n || 2);
    }
    /* INSTANCE METHODS */
    clone() {
        return new Vector(this.x, this.y);
    }
    /* INSTANCE METHODS */
    set(x, y) {
        this.x = x; this.y = y;
        return this;
    }
    /* STATIC METHODS */
    static normalize(a)
    {
        return Vector.divide(Vector.sqrMagnitude(a));
    }
    static distance(a, b) {
        return Math.sqrt((b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y));
    }
    static negative(v) {
        return new Vector(-v.x, -v.y);
    }
    static add(a, b) {
        if (b instanceof Vector)
            return new Vector(a.x + b.x, a.y + b.y);
        else
            return new Vector(a.x + b, a.y + b);
    }
    static subtract(a, b) {
        if (b instanceof Vector)
            return new Vector(a.x - b.x, a.y - b.y);
        else
            return new Vector(a.x - b, a.y - b);
    }
    static multiply(a, b) {
        if (b instanceof Vector)
            return new Vector(a.x * b.x, a.y * b.y);
        else
            return new Vector(a.x * b, a.y * b);
    }
    static divide(a, b) {
        if (b instanceof Vector)
            return new Vector(a.x / b.x, a.y / b.y);
        else
            return new Vector(a.x / b, a.y / b);
    }
    static equals(a, b) {
        return a.x == b.x && a.y == b.y;
    }
    static dot(a, b) {
        return a.x * b.x + a.y * b.y;
    }
    static cross(a, b) {
        return a.x * b.y - a.y * b.x;
    }
    static sqrMagnitude(vector) { return vector.x * vector.x + vector.y * vector.y; }
    static clampMagnitude(a, maxLength) {
        let sqrmag = Vector.sqrMagnitude(a);
        if (sqrmag > maxLength * maxLength) {
            let mag = Math.sqrt(sqrmag);
            let normalized_x = a.x / mag;
            let normalized_y = a.y / mag;
            return new Vector(normalized_x * maxLength,
                normalized_y * maxLength);
        }
        return a;
    }
    static Lerp(a, b, t) {
        t = Math.max(0,t);
        t = Math.min(1,t);
        return new Vector(
            a.x + (b.x - a.x) * t,
            a.y + (b.y - a.y) * t
        );
    }
}


