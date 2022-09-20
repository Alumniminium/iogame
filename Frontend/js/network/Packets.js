export class Packets
{
    static LoginRequestPacket(user, pass)
    {
        let buffer = new ArrayBuffer(38);
        let v = new DataView(buffer);
        v.setInt16(0, buffer.byteLength, true);
        v.setInt16(2, 1, true);
        v.setString(4, user, 16);
        v.setString(5 + 16, pass, 16);
        return buffer;
    }

    static ChatPacket(uid, message)
    {
        let buffer = new ArrayBuffer(9 + message.length);
        let v = new DataView(buffer);
        v.setUint16(0, buffer.byteLength, true);
        v.setUint16(2, 10, true);
        v.setUint32(4, uid, true);
        v.setString(8, message, true);
        return buffer;
    }

    static MovementPacket(player, thrust,reverseThrust, left, right, fire,boost,rcs,drop, x, y)
    {
        let buffer = new ArrayBuffer(22);
        let v = new DataView(buffer);
        v.setInt16(0, buffer.byteLength, true);
        v.setInt16(2, 21, true);
        v.setInt32(4, player.id, true);
        v.setInt32(8, player.id, true);

        let inputs = 0;
        if (thrust)
            inputs = setBit(inputs, 0);
        if (reverseThrust)
            inputs = setBit(inputs, 1);
        if (left)
            inputs = setBit(inputs, 2);
        if (right)
            inputs = setBit(inputs, 3);
        if (boost)
            inputs = setBit(inputs, 4);
        if(rcs)
            inputs = setBit(inputs, 5);
        if(fire)
            inputs = setBit(inputs, 6);
        if(drop)
            inputs = setBit(inputs, 7);

        v.setInt16(12, inputs, true);
        v.setFloat32(14, x, true);
        v.setFloat32(18, y, true);

        return buffer;
    }

    static RequestEntity(playerId, uniqueId)
    {
        let buffer = new ArrayBuffer(12);
        let v = new DataView(buffer);
        v.setInt16(0, buffer.byteLength, true);
        v.setInt16(2, 39, true);
        v.setInt32(4, playerId, true);
        v.setInt32(8, uniqueId, true);
        return buffer;
    }
}

DataView.prototype.getString = function (offset, length)
{
    let end = typeof length == 'number' ? offset + length : this.byteLength;
    let text = '';
    let val = -1;

    while (offset < this.byteLength && offset < end)
    {
        val = this.getUint8(offset++);
        text += String.fromCharCode(val);
    }

    return text;
};
DataView.prototype.setString = function (offset, string, size)
{
    this.setUint8(offset, string.length, true);
    string = ToSize(string, size);
    for (let i = 0; i < string.length; i++)
        this.setUint8(offset + 1 + i, string[i].charCodeAt(0), true);
};

function ToSize(string, size)
{
    for (let i = string.length; i < size; i++)
    {
        string += "\0";
    }
    return string;
}

function getBit(number, bitPosition)
{
    return (number & (1 << bitPosition)) === 0 ? 0 : 1;
}

function setBit(number, bitPosition)
{
    return number | (1 << bitPosition);
}