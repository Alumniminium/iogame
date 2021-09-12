import { Packets } from "./packets.js";
import { Vector } from "../vector.js";

export class Net {
    socket = null;
    connected = false;
    game = null;
    constructor(game)
    {
        this.game = game;
    }

    connect() {
        this.socket = new WebSocket("ws://localhost:5000/chat");
        this.socket.binaryType = 'arraybuffer';
        this.socket.onmessage = this.OnPacket.bind(this);
        this.socket.onopen = this.Connected;
    }

    Connected() {
        console.log("connected")
        this.connected = true;
        this.send(Packets.LoginRequestPacket("user","pass"));
    }

    OnPacket(packet) {
        let data = packet.data;
        let dv = new DataView(data);
        let len = dv.getInt16(0, true);
        let id = dv.getInt16(2, true);

        switch (id) {
            case 2:
                {
                    let uid = dv.getInt32(4, true);
                    let x = dv.getFloat32(8, true);
                    let y = dv.getFloat32(12, true);
                    this.game.player.position = new Vector(x, y);
                    break;
                }
        }
    }

    Send(packet) {
        this.socket.send(packet);
    }
}