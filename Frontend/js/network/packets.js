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

    static ChatPacket(uid, user, message)
    {
        let buffer = new ArrayBuffer(282);
        let v = new DataView(buffer);
        v.setInt16(0, buffer.byteLength, true);
        v.setInt16(2, 1004, true);
        v.setUint32(4, uid, true);
        v.setString(8, user, 16);
        v.setString(9 + 16, message, 256);
        return buffer;
    }

    static MovementPacket(player, up, down, left, right, fire, x, y)
    {
        let buffer = new ArrayBuffer(21);
        let v = new DataView(buffer);
        v.setInt16(0, buffer.byteLength, true);
        v.setInt16(2, 1005, true);
        v.setUint32(4, player.id, true);
        v.setUint32(8, player.id, true);

        let inputs = 0;
        if (up)
            inputs = setBit(inputs, 0);
        if (down)
            inputs = setBit(inputs, 1);
        if (left)
            inputs = setBit(inputs, 2);
        if (right)
            inputs = setBit(inputs, 3);
        if (fire)
            inputs = setBit(inputs, 4);
        // if(altFire)
        //     setBit(inputs, 5);
        // if(boost)
        //     setBit(inputs, 6);
        // if(undefined)
        //     setBit(inputs, 7);

        v.setInt8(12, inputs, true);
        v.setFloat32(13, x, true);
        v.setFloat32(17, y, true);

        return buffer;
    }

    static RequestEntity(playerId, uniqueId)
    {
        let buffer = new ArrayBuffer(12);
        let v = new DataView(buffer);
        v.setInt16(0, buffer.byteLength, true);
        v.setInt16(2, 1016, true);
        v.setUint32(4, playerId, true);
        v.setUint32(8, uniqueId, true);
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
    this.setInt8(offset, string.length, true);
    string = ToSize(string, size);
    for (let i = 0; i < string.length; i++)
        this.setInt8(offset + 1 + i, string[i].charCodeAt(0), true);
};

function ToSize(string, size)
{
    for (let i = string.length; i < size; i++)
    {
        string += "\0";
    }
    return string;
}

// function getBit(number, bitPosition)
// {
//     return (number & (1 << bitPosition)) === 0 ? 0 : 1;
// }

function setBit(number, bitPosition)
{
    return number | (1 << bitPosition);
}