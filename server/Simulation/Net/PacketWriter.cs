using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using server.ECS;
using server.Enums;

namespace server.Simulation.Net;

public class PacketWriter : IDisposable
{
    private byte[] _buffer;
    private int _offset;
    private readonly bool _fromPool;
    private readonly int _initialSize;

    public PacketWriter(PacketId packetId, int initialSize = 4096)
    {
        _initialSize = initialSize;
        _buffer = ArrayPool<byte>.Shared.Rent(initialSize);
        _fromPool = true;
        _offset = 0;

        WriteInt16(0);
        WriteInt16((short)packetId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PacketWriter Goto(int position)
    {
        _offset = position;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PacketWriter WriteGuid(Guid guid)
    {
        EnsureCapacity(16);
        if (!guid.TryWriteBytes(_buffer.AsSpan(_offset)))
            throw new InvalidOperationException("Failed to write GUID");
        _offset += 16;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PacketWriter WriteNtt(NTT ntt)
    {
        return WriteGuid(ntt.Id);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PacketWriter WriteInt64(long value)
    {
        EnsureCapacity(8);
        BinaryPrimitives.WriteInt64LittleEndian(_buffer.AsSpan(_offset), value);
        _offset += 8;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PacketWriter WriteUInt64(ulong value)
    {
        EnsureCapacity(8);
        BinaryPrimitives.WriteUInt64LittleEndian(_buffer.AsSpan(_offset), value);
        _offset += 8;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PacketWriter WriteInt32(int value)
    {
        EnsureCapacity(4);
        BinaryPrimitives.WriteInt32LittleEndian(_buffer.AsSpan(_offset), value);
        _offset += 4;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PacketWriter WriteUInt32(uint value)
    {
        EnsureCapacity(4);
        BinaryPrimitives.WriteUInt32LittleEndian(_buffer.AsSpan(_offset), value);
        _offset += 4;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PacketWriter WriteInt16(short value)
    {
        EnsureCapacity(2);
        BinaryPrimitives.WriteInt16LittleEndian(_buffer.AsSpan(_offset), value);
        _offset += 2;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PacketWriter WriteUInt16(ushort value)
    {
        EnsureCapacity(2);
        BinaryPrimitives.WriteUInt16LittleEndian(_buffer.AsSpan(_offset), value);
        _offset += 2;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PacketWriter WriteByte(byte value)
    {
        EnsureCapacity(1);
        _buffer[_offset++] = value;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PacketWriter WriteSByte(sbyte value)
    {
        EnsureCapacity(1);
        _buffer[_offset++] = (byte)value;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PacketWriter WriteFloat(float value)
    {
        EnsureCapacity(4);
        BinaryPrimitives.WriteSingleLittleEndian(_buffer.AsSpan(_offset), value);
        _offset += 4;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PacketWriter WriteDouble(double value)
    {
        EnsureCapacity(8);
        BinaryPrimitives.WriteDoubleLittleEndian(_buffer.AsSpan(_offset), value);
        _offset += 8;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PacketWriter WriteVector2(Vector2 value)
    {
        WriteFloat(value.X);
        WriteFloat(value.Y);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PacketWriter WriteVector3(Vector3 value)
    {
        WriteFloat(value.X);
        WriteFloat(value.Y);
        WriteFloat(value.Z);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PacketWriter WriteBool(bool value)
    {
        WriteByte(value ? (byte)1 : (byte)0);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PacketWriter WriteEnum<T>(T value) where T : Enum
    {
        var underlyingType = Enum.GetUnderlyingType(typeof(T));
        if (underlyingType == typeof(byte))
            WriteByte((byte)(object)value);
        else if (underlyingType == typeof(short))
            WriteInt16((short)(object)value);
        else if (underlyingType == typeof(ushort))
            WriteUInt16((ushort)(object)value);
        else if (underlyingType == typeof(int))
            WriteInt32((int)(object)value);
        else
            throw new NotSupportedException($"Enum type {typeof(T)} not supported");
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PacketWriter WriteString8(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var length = Math.Min(255, bytes.Length);
        WriteByte((byte)length);
        WriteBytes(bytes.AsSpan(0, length));
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PacketWriter WriteString16(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var length = Math.Min(ushort.MaxValue, bytes.Length);
        WriteUInt16((ushort)length);
        WriteBytes(bytes.AsSpan(0, length));
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PacketWriter WriteString32(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        WriteInt32(bytes.Length);
        WriteBytes(bytes);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PacketWriter WriteStringFixed(string value, int fixedLength)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var copyLength = Math.Min(fixedLength, bytes.Length);
        EnsureCapacity(fixedLength);
        bytes.AsSpan(0, copyLength).CopyTo(_buffer.AsSpan(_offset));
        if (copyLength < fixedLength)
            _buffer.AsSpan(_offset + copyLength, fixedLength - copyLength).Clear();
        _offset += fixedLength;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PacketWriter WriteBytes(ReadOnlySpan<byte> bytes)
    {
        EnsureCapacity(bytes.Length);
        bytes.CopyTo(_buffer.AsSpan(_offset));
        _offset += bytes.Length;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PacketWriter Skip(int count)
    {
        EnsureCapacity(count);
        _offset += count;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureCapacity(int additionalBytes)
    {
        var required = _offset + additionalBytes;
        if (required <= _buffer.Length)
            return;

        var newSize = Math.Max(required, _buffer.Length * 2);
        var newBuffer = ArrayPool<byte>.Shared.Rent(newSize);
        _buffer.AsSpan(0, _offset).CopyTo(newBuffer);

        if (_fromPool)
            ArrayPool<byte>.Shared.Return(_buffer);

        _buffer = newBuffer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Memory<byte> Finalize()
    {
        BinaryPrimitives.WriteInt16LittleEndian(_buffer.AsSpan(0), (short)_offset);

        var result = new byte[_offset];
        _buffer.AsSpan(0, _offset).CopyTo(result);
        return result;
    }

    public void Dispose()
    {
        if (_fromPool && _buffer != null)
        {
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = null;
        }
    }
}