import { Packets } from "./packets.js";
import { Vector } from "../vector.js";
import { YellowSquare } from "../entities/yellowSquare.js";
import { RedTriangle } from "../entities/RedTriangle.js";
import { PurplePentagon } from "../entities/PurplePentagon.js";
import { PurpleOctagon } from "../entities/PurpleOctagon.js";
import { BlueCircle } from "../entities/blueCircle.js";
import { Entity } from "../entities/entity.js";

export class Net {
    socket = null;
    connected = false;
    game = null;
    constructor(game) {
        this.game = game;
    }

    connect() {
        this.socket = new WebSocket("ws://62.178.176.71:5000/chat");
        this.socket.binaryType = 'arraybuffer';
        this.socket.onmessage = this.OnPacket.bind(this);
        this.socket.onopen = this.Connected;
    }

    Connected() {
        console.log("connected")
        this.connected = true;
        this.send(Packets.LoginRequestPacket("user", "pass"));
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
                    this.game.player.id = uid;
                    this.game.player.position = new Vector(x, y);
                    this.game.entities.set(uid, this.game.player);
                    break;
                }
            case 1005:
                {
                    let uid = dv.getInt32(4, true);
                    let lookId = dv.getInt32(8, true);
                    let x = dv.getFloat32(12, true);
                    let y = dv.getFloat32(16, true);
                    let vx = dv.getFloat32(20, true);
                    let vy = dv.getFloat32(24, true);

                    let entity = this.game.entities.get(uid);
                    if (entity == undefined) {
                        switch (lookId) {
                            case 0:
                                {
                                    entity = new BlueCircle(uid, x, y, vx, vy);
                                    entity.sides = 0;
                                    break;
                                }
                            case 3:
                                {
                                    entity = new RedTriangle(uid, x, y, vx, vy);
                                    break;
                                }
                            case 4:
                                {
                                    entity = new YellowSquare(uid, x, y, vx, vy);
                                    break;
                                }
                            case 5:
                                {
                                    entity = new PurplePentagon(uid, x, y, vx, vy);
                                    break;
                                }
                            case 8:
                                {
                                    entity = new PurpleOctagon(uid, x, y, vx, vy);
                                    break;
                                }
                        }


                        if (entity.originX() > this.game.camera.viewport.left && entity.originX() < this.game.camera.viewport.right && entity.originY() > this.game.camera.viewport.top && entity.originY() < this.game.camera.viewport.bottom)
                            this.game.entities.set(uid, entity);
                    }

                    entity.serverPosition = new Vector(x, y);
                    entity.velocity = new Vector(vx, vy);
                    if (entity.originX() < this.game.camera.viewport.left || entity.originX() > this.game.camera.viewport.right || entity.originY() < this.game.camera.viewport.top || entity.originY() > this.game.camera.viewport.bottom)
                        this.game.entities.delete(uid);
                    break;
                }
        }
    }

    send(packet) {
        this.socket.send(packet);
    }
}