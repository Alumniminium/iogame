using System;

namespace server.Utilities;

public class FastRandom
{
    private const double REAL_INT_DOUBLE = 1 / (int.MaxValue + 1d), REAL_UINT_DOUBLE = 1 / (uint.MaxValue + 1d);
    private const float REAL_INT_FLOAT = 1 / (int.MaxValue + 1f);
    private const uint Y = 842502087, Z = 3579807591, W = 273326509;

    public int Seed
    {
        get => _seed;
        set
        {
            _seed = value;
            Reset();
        }
    }

    private int _seed;
    private uint _x, _y, _z, _w, _bitBuffer, _bitMask;

    public FastRandom() { Seed = Environment.TickCount; }
    public FastRandom(int seed) { Seed = seed; }

    public void Reset()
    {
        _x = (uint)Seed;
        _y = Y;
        _z = Z;
        _w = W;
        _bitBuffer = 0;
        _bitMask = 1;
    }
    public void Reset(int seed) => Seed = seed;

    /// <summary>Returns a random <see cref="int"/> in the range of 0 (inclusive) to <see cref="int.MaxValue"/> (exlusive)</summary>
    public int Next()
    {
        var t = _x ^ _x << 11;
        _x = _y;
        _y = _z;
        _z = _w;
        _w = _w ^ _w >> 19 ^ t ^ t >> 8;
        var rtn = _w & 0x7FFFFFFF;
        return rtn == 0x7FFFFFFF ? Next() : (int)rtn;
    }

    /// <summary>Returns a random <see cref="int"/> in the range of 0 (inclusive) to <paramref name="max"/> (exlusive)</summary>
    public int Next(int max)
    {
        var t = _x ^ _x << 11;
        _x = _y;
        _y = _z;
        _z = _w;
        return (int)(REAL_INT_DOUBLE * (int)(0x7FFFFFFF & (_w = _w ^ _w >> 19 ^ t ^ t >> 8)) * max);
    }

    /// <summary>Returns a random <see cref="int"/> in the range of <paramref name="min"/> (inclusive) to <paramref name="max"/> (exclusive)</summary>
    public int Next(int min, int max)
    {
        var t = _x ^ _x << 11;
        _x = _y;
        _y = _z;
        _z = _w;
        var range = max - min;
        return range < 0
            ? min + (int)(REAL_UINT_DOUBLE * (_w = _w ^ _w >> 19 ^ t ^ t >> 8) * (max - (long)min))
            : min + (int)(REAL_INT_DOUBLE * (int)(0x7FFFFFFF & (_w = _w ^ _w >> 19 ^ t ^ t >> 8)) * range);
    }

    /// <summary>Returns a random <see cref="double"/> in the range of 0 (inclusive) to 1 (exclusive)</summary>
    public double NextDouble()
    {
        var t = _x ^ _x << 11;
        _x = _y;
        _y = _z;
        _z = _w;
        return REAL_INT_DOUBLE * (int)(0x7FFFFFFF & (_w = _w ^ _w >> 19 ^ t ^ t >> 8));
    }

    /// <summary>Fills the provided byte array with random bytes</summary>
    public void NextBytes(byte[] buffer)
    {
        uint x = _x,
            y = _y,
            z = _z,
            w = _w;
        var i = 0;
        uint t;
        for (var bound = buffer.Length - 3; i < bound;)
        {
            t = x ^ x << 11;
            x = y;
            y = z;
            z = w;
            w = w ^ w >> 19 ^ t ^ t >> 8;
            buffer[i++] = (byte)w;
            buffer[i++] = (byte)(w >> 8);
            buffer[i++] = (byte)(w >> 16);
            buffer[i++] = (byte)(w >> 24);
        }
        if (i < buffer.Length)
        {
            t = x ^ x << 11;
            x = y;
            y = z;
            z = w;
            w = w ^ w >> 19 ^ t ^ t >> 8;
            buffer[i++] = (byte)w;
            if (i < buffer.Length)
            {
                buffer[i++] = (byte)(w >> 8);
                if (i < buffer.Length)
                {
                    buffer[i++] = (byte)(w >> 16);
                    if (i < buffer.Length)
                        buffer[i] = (byte)(w >> 24);
                }
            }
        }
        _x = x;
        _y = y;
        _z = z;
        _w = w;
    }

    /// <summary>Returns a random <see cref="double"/> in the range of 0 (inclusive) to <paramref name="max"/> (exclusive)</summary>
    public double NextDouble(double max)
    {
        var t = _x ^ _x << 11;
        _x = _y;
        _y = _z;
        _z = _w;
        return REAL_INT_DOUBLE * ((int)(0x7FFFFFFF & (_w = _w ^ _w >> 19 ^ t ^ t >> 8)) * max);
    }

    /// <summary>Returns a random <see cref="double"/> in the range of <paramref name="min"/> (inclusive) to <paramref name="max"/> (exclusive)</summary>
    public double NextDouble(double min, double max)
    {
        var t = _x ^ _x << 11;
        _x = _y;
        _y = _z;
        _z = _w;
        var range = max - min;
        return range < 0
            ? min + REAL_UINT_DOUBLE * ((int)(_w = _w ^ _w >> 19 ^ t ^ t >> 8) * (max - (long)min))
            : min + REAL_INT_DOUBLE * ((int)(0x7FFFFFFF & (_w = _w ^ _w >> 19 ^ t ^ t >> 8)) * range);
    }

    /// <summary>Returns a random <see cref="float"/> in the range of 0 (inclusive) to 1 (exclusive)</summary>
    public float NextFloat()
    {
        var t = _x ^ _x << 11;
        _x = _y;
        _y = _z;
        _z = _w;
        return REAL_INT_FLOAT * (int)(0x7FFFFFFF & (_w = _w ^ _w >> 19 ^ t ^ t >> 8));
    }

    /// <summary>Returns a random <see cref="float"/> in the range of 0 (inclusive) to <paramref name="max"/> (exclusive)</summary>
    public float NextFloat(float max)
    {
        var t = _x ^ _x << 11;
        _x = _y;
        _y = _z;
        _z = _w;
        return REAL_INT_FLOAT * ((int)(0x7FFFFFFF & (_w = _w ^ _w >> 19 ^ t ^ t >> 8)) * max);
    }

    /// <summary>Returns a random <see cref="float"/> in the range of <paramref name="min"/> (inclusive) to <paramref name="max"/> (exclusive)</summary>
    public float NextFloat(float min, float max)
    {
        var t = _x ^ _x << 11;
        _x = _y;
        _y = _z;
        _z = _w;
        var range = max - min;
        return range < 0
            ? min + REAL_INT_FLOAT * (int)(_w = _w ^ _w >> 19 ^ t ^ t >> 8) * (max - (long)min)
            : min + REAL_INT_FLOAT * (int)(0x7FFFFFFF & (_w = _w ^ _w >> 19 ^ t ^ t >> 8)) * range;
    }

    /// <summary>Returns a random <see cref="uint"/> in the range of -<see cref="uint.MinValue"/> (inclusive) to <see cref="uint.MaxValue"/> (inclusive)</summary>
    public uint NextUInt()
    {
        var t = _x ^ _x << 11;
        _x = _y;
        _y = _z;
        _z = _w;
        return _w = _w ^ _w >> 19 ^ t ^ t >> 8;
    }

    /// <summary>Returns a random <see cref="int"/> in the range of 0 (inclusive) to <see cref="int.MaxValue"/> (inclusive)</summary>
    public int NextInt()
    {
        var t = _x ^ _x << 11;
        _x = _y;
        _y = _z;
        _z = _w;
        return (int)(0x7FFFFFFF & (_w = _w ^ _w >> 19 ^ t ^ t >> 8));
    }

    /// <summary>Returns a random <see cref="bool"/></summary>
    public bool NextBool()
    {
        if (_bitMask == 1)
        {
            var t = _x ^ _x << 11;
            _x = _y;
            _y = _z;
            _z = _w;
            _bitBuffer = _w = _w ^ _w >> 19 ^ t ^ t >> 8;
            _bitMask = 0x80000000;
            return (_bitBuffer & _bitMask) == 0;
        }
        return (_bitBuffer & (_bitMask >>= 1)) == 0;
    }

    /// <summary>Returns a random value from <paramref name="values"/></summary>
    public T NextValue<T>(params T[] values)
    {
        var t = _x ^ _x << 11;
        _x = _y;
        _y = _z;
        _z = _w;
        return values[(int)(REAL_INT_DOUBLE * (int)(0x7FFFFFFF & (_w = _w ^ _w >> 19 ^ t ^ t >> 8)) * values.Length)];
    }
}