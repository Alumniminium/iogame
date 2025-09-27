using System;
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using server.ECS;

namespace server.Simulation.Net;

public class PacketReader
{
    private readonly ReadOnlyMemory<byte> _buffer;
    private int _offset;

    private static readonly string[] HexLookup = CreateHexLookup();

    private static string[] CreateHexLookup()
    {
        var lookup = new string[256];
        for (int i = 0; i < 256; i++)
            lookup[i] = i.ToString("x2");
        return lookup;
    }

    public PacketReader(ReadOnlySpan<byte> buffer)
    {
        _buffer = new ReadOnlyMemory<byte>(buffer.ToArray());
        _offset = 0;
        ReadHeader();
    }

    public PacketReader(ReadOnlyMemory<byte> buffer) : this(buffer.Span) { }

    public PacketReader(byte[] buffer) : this(buffer.AsSpan()) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Header ReadHeader()
    {
        var length = ReadUInt16();
        var id = ReadUInt16();
        return new Header(length, (server.Enums.PacketId)id);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PacketReader Goto(int position)
    {
        _offset = position;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Guid ReadGuid()
    {
        EnsureBytes(16);
        var guidBytes = _buffer.Span.Slice(_offset, 16);
        _offset += 16;

        var hex = HexLookup;
        var guidString = string.Concat(
            hex[guidBytes[3]], hex[guidBytes[2]], hex[guidBytes[1]], hex[guidBytes[0]], "-",
            hex[guidBytes[5]], hex[guidBytes[4]], "-",
            hex[guidBytes[7]], hex[guidBytes[6]], "-",
            hex[guidBytes[8]], hex[guidBytes[9]], "-",
            hex[guidBytes[10]], hex[guidBytes[11]], hex[guidBytes[12]], hex[guidBytes[13]], hex[guidBytes[14]], hex[guidBytes[15]]
        );

        return Guid.Parse(guidString);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NTT ReadNtt()
    {
        return new NTT(ReadGuid());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long ReadInt64()
    {
        EnsureBytes(8);
        var value = BinaryPrimitives.ReadInt64LittleEndian(_buffer.Span.Slice(_offset));
        _offset += 8;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong ReadUInt64()
    {
        EnsureBytes(8);
        var value = BinaryPrimitives.ReadUInt64LittleEndian(_buffer.Span.Slice(_offset));
        _offset += 8;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadInt32()
    {
        EnsureBytes(4);
        var value = BinaryPrimitives.ReadInt32LittleEndian(_buffer.Span.Slice(_offset));
        _offset += 4;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint ReadUInt32()
    {
        EnsureBytes(4);
        var value = BinaryPrimitives.ReadUInt32LittleEndian(_buffer.Span.Slice(_offset));
        _offset += 4;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short ReadInt16()
    {
        EnsureBytes(2);
        var value = BinaryPrimitives.ReadInt16LittleEndian(_buffer.Span.Slice(_offset));
        _offset += 2;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort ReadUInt16()
    {
        EnsureBytes(2);
        var value = BinaryPrimitives.ReadUInt16LittleEndian(_buffer.Span.Slice(_offset));
        _offset += 2;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte ReadByte()
    {
        EnsureBytes(1);
        return _buffer.Span[_offset++];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public sbyte ReadSByte()
    {
        return (sbyte)ReadByte();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ReadFloat()
    {
        EnsureBytes(4);
        var value = BinaryPrimitives.ReadSingleLittleEndian(_buffer.Span.Slice(_offset));
        _offset += 4;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double ReadDouble()
    {
        EnsureBytes(8);
        var value = BinaryPrimitives.ReadDoubleLittleEndian(_buffer.Span.Slice(_offset));
        _offset += 8;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 ReadVector2()
    {
        return new Vector2(ReadFloat(), ReadFloat());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 ReadVector3()
    {
        return new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ReadBool()
    {
        return ReadByte() != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T ReadEnum<T>() where T : Enum
    {
        var underlyingType = Enum.GetUnderlyingType(typeof(T));
        if (underlyingType == typeof(byte))
            return (T)(object)ReadByte();
        if (underlyingType == typeof(short))
            return (T)(object)ReadInt16();
        if (underlyingType == typeof(ushort))
            return (T)(object)ReadUInt16();
        if (underlyingType == typeof(int))
            return (T)(object)ReadInt32();
        throw new NotSupportedException($"Enum type {typeof(T)} not supported");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadString8()
    {
        var length = ReadByte();
        EnsureBytes(length);
        var stringBytes = _buffer.Span.Slice(_offset, length);
        _offset += length;
        return Encoding.UTF8.GetString(stringBytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadString16()
    {
        var length = ReadUInt16();
        EnsureBytes(length);
        var stringBytes = _buffer.Span.Slice(_offset, length);
        _offset += length;
        return Encoding.UTF8.GetString(stringBytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadString32()
    {
        var length = ReadInt32();
        EnsureBytes(length);
        var stringBytes = _buffer.Span.Slice(_offset, length);
        _offset += length;
        return Encoding.UTF8.GetString(stringBytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadStringFixed(int fixedLength)
    {
        EnsureBytes(fixedLength);
        var stringBytes = _buffer.Span.Slice(_offset, fixedLength);
        _offset += fixedLength;

        var nullIndex = stringBytes.IndexOf((byte)0);
        if (nullIndex >= 0)
            stringBytes = stringBytes.Slice(0, nullIndex);

        return Encoding.UTF8.GetString(stringBytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> ReadBytes(int count)
    {
        EnsureBytes(count);
        var bytes = _buffer.Span.Slice(_offset, count);
        _offset += count;
        return bytes;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PacketReader Skip(int count)
    {
        EnsureBytes(count);
        _offset += count;
        return this;
    }

    public int Position => _offset;

    public int Remaining => _buffer.Length - _offset;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureBytes(int count)
    {
        if (_offset + count > _buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(count), "Not enough bytes remaining in buffer");
    }
}