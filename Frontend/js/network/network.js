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
    player = null;
    camera = null;

    requestQueue = new Map();

    constructor(game) {
        this.game = game;
        this.camera = this.game.renderer.camera;
        this.player = this.game.player;
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
        this.send(Packets.LoginRequestPacket("user", "pass"));
    }

    OnPacket(packet) {
        let data = packet.data;
        let dv = new DataView(data);
        let len = dv.getInt16(0, true);
        let id = dv.getInt16(2, true);
        // console.log("got packet " + id);

        switch (id) {
            //login response
            case 2:
                {
                    let uid = dv.getInt32(4, true);
                    let ticks = dv.getInt32(8, true);
                    let x = dv.getFloat32(12, true);
                    let y = dv.getFloat32(16, true);
                    let map_width = dv.getInt32(20, true);
                    let map_height = dv.getInt32(24, true);
                    let viewDistance = dv.getInt16(28, true);

                    this.game.MAP_WIDTH = map_width;
                    this.game.MAP_HEIGHT = map_height;
                    this.camera.distance = viewDistance;

                    this.player.id = uid;
                    this.player.position = new Vector(x, y);
                    this.player.serverPosition = new Vector(x, y);
                    this.player.input.setup(this.game);
                    this.game.addEntity(this.player);
                    break;
                }
            case 1005:
                {
                    let uid = dv.getInt32(4, true);
                    let ticks = dv.getInt32(8, true);
                    let lookId = dv.getInt32(12, true);
                    let x = dv.getFloat32(16, true);
                    let y = dv.getFloat32(20, true);
                    let vx = dv.getFloat32(24, true);
                    let vy = dv.getFloat32(28, true);

                    let entity = this.game.entities.get(uid);
                    if (entity == undefined) 
                    {
                        if(this.requestQueue.has(uid) == false)
                        {
                            if(this.camera.canSeeXY(x,y))
                            {
                                console.log(`Requesting SpawnPacket for ${uid}`);
                                this.send(Packets.RequestEntity(this.player.id, uid));
                                this.requestQueue.set(uid,false);
                            }
                        }
                        return;
                    }
                    entity.serverPosition = new Vector(x, y);
                    entity.velocity = new Vector(vx, vy);
                    break;
                }
            // Spawn Entity
            case 1015:
                {
                    let uniqueId = dv.getUint32(4,true);
                    let direction = dv.getUint16(8,true);
                    let size = dv.getUint16(10,true);
                    let mass =dv.getUint16(12,true);
                    let maxHealh = dv.getUint32(14,true);
                    let curHealth = dv.getUint32(18,true);
                    let color = dv.getUint32(22, true);
                    let borderColor = dv.getUint32(26, true);
                    let drag = dv.getFloat32(30,true);
                    let x = dv.getFloat32(34,true);
                    let y = dv.getFloat32(38,true);
                    let vx = dv.getFloat32(42,true);
                    let vy = dv.getFloat32(46,true);

                    if(this.requestQueue.has(uniqueId))
                        this.requestQueue.delete(uniqueId);

                    let entity = new Entity(uniqueId);
                    console.log(direction);
                    entity.direction = direction;
                    entity.size = size;
                    entity.mass = mass;
                    entity.maxHealth = maxHealh;
                    entity.health=curHealth;
                    // entity.color = color;
                    // entity.borderColor = borderColor;
                    entity.drag = drag;
                    entity.position = new Vector(x,y);
                    entity.velocity = new Vector(vx,vy);

                    if(uniqueId >= 1000000)
                        entity.isPlayer = true;

                    console.log(`Spawn: Id=${uniqueId}, Dir=${direction}, Size=${size}, Mass=${mass}, Health=${curHealth}, MaxHealth=${maxHealh}, Drag=${drag}`);
                    this.game.addEntity(entity);
                    break;
                }
            case 9000:
                let ping = dv.getInt16(4, true);
                if (ping != 0)
                    this.player.ping = ping;
                else
                    this.send(data);
                break;
        }
    }

    send(packet) {
        this.socket.send(packet);
    }
}