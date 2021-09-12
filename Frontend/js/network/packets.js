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
        v.setInt16(0, 32, true);
        v.setInt16(2, 1, true);
        v.setString(4, user, 16);
        v.setString(5+16, pass, 16);
        return buffer;
    }

}