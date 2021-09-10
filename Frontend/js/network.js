
export class Net {
    socket = null;
    connected = false;

    connect() {
        this.socket = new WebSocket("ws://localhost:5000/chat");
        this.socket.binaryType = 'arraybuffer';
        this.socket.onmessage = this.OnPacket;
        this.socket.onopen = this.Connected;
    }

    Connected() {
        console.log("connected")
        this.connected = true;
    }

    OnPacket(packet) {
        var data = packet.data;
        var dv = new DataView(data);
        var len = dv.getInt16(0, true);
        var id = dv.getInt16(2, true);

        switch (id) {
            case 1:
                {
                    var x = dv.getInt32(4, true);
                    var y = dv.getInt32(8, true);
                    console.log(`Length: ${len} bytes. Id: ${id}, X=${x} Y=${y}`);
                    break;
                }
        }
    }

    Send(packet) {
        this.socket.send(packet);
    }
}