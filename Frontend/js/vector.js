export class Vector
{
    constructor(x, y)
    {
        this.x = x || 0;
        this.y = y || 0;
    }

    negative() { return Vector.negative(this); }
    add(v) { return Vector.add(this, v); }
    subtract(v) { return Vector.subtract(this, v); }
    multiply(v) { return Vector.multiply(this, v); }
    divide(v) { return Vector.divide(this, v); }
    equals(v) { return Vector.equals(this, v); }
    dot(v) { return Vector.dot(this, v); }
    cross(v) { return Vector.cross(this, v); }
    length() { return Math.sqrt(this.dot(this)); }
    normalize() { return Vector.normalize(this); }
    min() { return Math.min(this.x, this.y); }
    max() { return Math.max(this.x, this.y); }
    toAngles() { return -Math.atan2(-this.y, this.x); }
    angleTo(a) { return Math.acos(this.dot(a) / (this.length() * a.length())); }
    magnitude() { return Vector.magnitude(this); }
    unit() 
    {
        if (this.magnitude == 0)
            return new Vector(0, 0);
        return new Vector(this.x / this.magnitude(), this.y / this.magnitude());
    }

    static normalize(a) { return Vector.divide(Vector.sqrMagnitude(a)); }
    static distance(a, b) { return Math.sqrt((b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y)); }
    static negative(v) { return new Vector(-v.x, -v.y); }
    static equals(a, b) { return a.x == b.x && a.y == b.y; }
    static dot(a, b) { return a.x * b.x + a.y * b.y; }
    static cross(a, b) { return a.x * b.y - a.y * b.x; }
    static magnitude(vector) { return Math.sqrt(Vector.sqrMagnitude(vector)); }
    static sqrMagnitude(vector) { return vector.x * vector.x + vector.y * vector.y; }
    static add(a, b) { return new Vector(a.x + b.x, a.y + b.y); }
    static subtract(a, b) { return new Vector(a.x - b.x, a.y - b.y); }
    
    static multiply(a, b) 
    {
        if (b instanceof Vector)
            return new Vector(a.x * b.x, a.y * b.y);
        else
            return new Vector(a.x * b, a.y * b);
    }
    static divide(a, b)
    {
        if (b instanceof Vector)
            return new Vector(a.x / b.x, a.y / b.y);
        else
            return new Vector(a.x / b, a.y / b);
    }
    static clampMagnitude(a, maxLength)
    {
        var mag = a.magnitude();
        if(mag < maxLength)
            return a;

        var normalized_x = a.x / mag;
        var normalized_y = a.y / mag;
        return new Vector(normalized_x * maxLength, normalized_y * maxLength);
    }
    static lerp(a, b, t)
    {
        t = Math.max(0, t);
        t = Math.min(1, t);
        return new Vector(
            a.x + (b.x - a.x) * t,
            a.y + (b.y - a.y) * t
        );
    }
}