DataView.prototype.getString = function (offset, length) {
    let end = typeof length == 'number' ? offset + length : this.byteLength;
    let text = '';
    let val = -1;

    while (offset < this.byteLength && offset < end) {
        val = this.getUint8(offset++);
        if (val == 0) break;
        text += String.fromCharCode(val);
    }

    return text;
};
DataView.prototype.setString = function (offset, string, size) {
    this.setInt8(offset, string.length,true);
    string = ToSize(string,size);
    for (let i = 0; i < string.length; i++)
        this.setInt8(offset+1 + i, string[i].charCodeAt(0),true);
};

function ToSize(string, size)
{
    for(let i = string.length; i<size; i++)
    {
        string += "\0";
    }
    return string;
}

export class Packets {
    static LoginRequestPacket(user, pass) {
        let buffer = new ArrayBuffer(38);
        let v = new DataView(buffer);
        v.setInt16(0, 38, true);
        v.setInt16(2, 1, true);
        v.setString(4, user, 16);
        v.setString(5 + 16, pass, 16);
        return buffer;
    }

    static MovementPacket(player,up,down,left,right) {
        let buffer = new ArrayBuffer(12);
        let v = new DataView(buffer);
        v.setInt16(0, 12, true);
        v.setInt16(2, 1005, true);
        v.setUint32(4, player.id, true);
        v.setInt8(8, up ? 1:0, true);
        v.setInt8(9, down ? 1 : 0, true);
        v.setInt8(10, left ? 1 : 0, true);
        v.setInt8(11, right ? 1 : 0, true);
        return buffer;
    }

}