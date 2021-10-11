import { Packets } from "./packets.js";
import { Vector } from "../vector.js";
import { Entity } from "../entities/entity.js";
import { Bullet } from "../entities/bullet.js";

export class Net
{
    socket = null;
    connected = false;
    game = null;
    player = null;
    camera = null;

    requestQueue = new Map();

    constructor(game)
    {
        this.game = game;
        this.camera = this.game.renderer.camera;
        this.player = this.game.player;
    }

    connect()
    {
        this.socket = new WebSocket("ws://localhost:5000/chat");
        this.socket.binaryType = 'arraybuffer';
        this.socket.onmessage = this.OnPacket.bind(this);
        this.socket.onopen = this.Connected.bind(this);
    }

    Connected()
    {
        console.log("connected");
        this.connected = true;
        this.send(Packets.LoginRequestPacket(this.player.name, "pass"));
    }

    sendMessage(text)
    {
        this.send(Packets.ChatPacket(this.player.id, this.player.name, text));
    }

    OnPacket(packet)
    {
        let data = packet.data;
        let dv = new DataView(data);
        let len = dv.getInt16(0, true);
        let id = dv.getInt16(2, true);
        // console.log("got packet " + id);

        // window.totalBytesReceived += len;
        window.bytesReceived += len;

        switch (id)
        {
            //login response
            case 2:
                {
                    this.LoginResponseHandler(dv);
                    break;
                }
            case 1004:
                {
                    this.ChatHandler(dv);
                    break;
                }
            case 1005:
                {
                    this.MovementHandler(dv);
                    break;
                }
            case 1010:
                {
                    this.StatusHandler(dv);
                    break;
                }
            // Spawn Entity
            case 1015:
                {
                    this.SpawnPacketHandler(dv);
                    break;
                }
            // Spawn Resource
            case 1116:
                {
                    this.ResourceSpawnPacket(dv);
                    break;
                }
            case 9000:
                this.PingPacketHandler(dv, data);
                break;
        }
    }

    ChatHandler(rdr)
    {
        let uniqueId = rdr.getUint32(4, true);
        let fromLen = rdr.getUint8(8, true);
        let from = rdr.getString(9, fromLen);
        let textlene = rdr.getUint8(25, true);
        let text = rdr.getString(26, textlene);

        window.game.addChatLogLine(from +": "+text);
    }
    ResourceSpawnPacket(rdr)
    {
        let uniqueId = rdr.getUint32(4, true);
        let resourceId = rdr.getUint16(8, true);
        let direction = rdr.getFloat32(10, true);
        let x = rdr.getFloat32(14, true);
        let y = rdr.getFloat32(18, true);
        let vx = rdr.getFloat32(22, true);
        let vy = rdr.getFloat32(26, true);

        if (this.requestQueue.has(uniqueId))
            this.requestQueue.delete(uniqueId);

        let entity = new Entity(uniqueId);
        entity.sides = resourceId == 0 ? 4 : resourceId == 1 ? 3 : resourceId == 2 ? 5 : 8;
        entity.direction = direction;
        entity.size = resourceId == 0 ? 100 : resourceId == 1 ? 200 : resourceId == 2 ? 300 : 800;
        entity.maxHealth = 100;
        entity.health = 100;
        entity.fillColor = "black";
        entity.strokeColor = "red";
        entity.drag = 0.99997;
        entity.position = new Vector(x, y);
        entity.serverPosition = new Vector(x, y);
        entity.velocity = new Vector(vx, vy);
        entity.serverVelocity = new Vector(vx, vy);
        entity.maxSpeed = 5000;

        this.game.addEntity(entity);
    }

    PingPacketHandler(rdr, data)
    {
        let ping = rdr.getInt16(4, true);
        if (ping != 0)
        {
            window.ping = ping;
            window.bytesPerSecondSent = window.bytesSent;
            window.totalBytesSent += window.bytesSent;
            window.bytesSent = 0;

            window.bytesPerSecondReceived = window.bytesReceived;
            window.totalBytesReceived += window.bytesReceived;
            window.bytesReceived = 0;
        }
        else
            this.send(data);
    }

    SpawnPacketHandler(rdr)
    {
        let uniqueId = rdr.getUint32(4, true);
        let ownerId = rdr.getUint32(8, true);
        let direction = rdr.getFloat32(12, true);
        let size = rdr.getUint16(16, true);
        let maxHealh = rdr.getUint32(18, true);
        let curHealth = rdr.getUint32(22, true);
        let color = rdr.getUint32(26, true);
        let borderColor = rdr.getUint32(30, true);
        let drag = rdr.getFloat32(34, true);
        let sides = rdr.getUint8(38, true);
        let x = rdr.getFloat32(39, true);
        let y = rdr.getFloat32(43, true);
        let vx = rdr.getFloat32(47, true);
        let vy = rdr.getFloat32(51, true);
        let maxSpeed = rdr.getUint32(55, true);

        if (this.requestQueue.has(uniqueId))
            this.requestQueue.delete(uniqueId);

        let entity = new Entity(uniqueId);
        if (this.game.entities.has(ownerId))
        {
            entity = new Bullet(uniqueId, this.game.entities.get(ownerId));
        }
        entity.sides = sides;
        entity.direction = direction;
        entity.size = size;
        entity.maxHealth = maxHealh;
        entity.health = curHealth;
        entity.fillColor = this.toColor(color);
        entity.strokeColor = this.toColor(borderColor);
        entity.drag = drag;
        entity.position = new Vector(x, y);
        entity.serverPosition = new Vector(x, y);
        entity.velocity = new Vector(vx, vy);
        entity.serverVelocity = new Vector(vx, vy);
        entity.maxSpeed = maxSpeed;

        console.log(`Spawn: Id=${uniqueId}, Dir=${direction}, Size=${size}, Health=${curHealth}, MaxHealth=${maxHealh}, Drag=${drag}`);
        this.game.addEntity(entity);
    }

    StatusHandler(rdr)
    {
        let uid = rdr.getInt32(4, true);
        let val = rdr.getBigUint64(8, true);
        let type = rdr.getInt32(20, true);

        if (this.game.entities.has(uid))
        {
            const entity = this.game.entities.get(uid);

            switch (type)
            {
                // Alive
                case 0:
                    if (val == 0)
                        this.game.removeEntity(entity);
                    break;
                // Health
                case 1:
                    entity.health = val;
                    if (entity.health <= 0)
                        this.game.removeEntity(entity);
                    break;
            }
        }
    }
    MovementHandler(rdr)
    {
        let uid = rdr.getInt32(4, true);
        let ticks = rdr.getInt32(8, true);
        let x = rdr.getFloat32(12, true);
        let y = rdr.getFloat32(16, true);
        let vx = rdr.getFloat32(20, true);
        let vy = rdr.getFloat32(24, true);

        let entity = this.game.entities.get(uid);
        if (entity == undefined)
        {
            if (this.requestQueue.has(uid) == false)
            {
                if (this.camera.canSeeXY(x, y))
                {
                    console.log(`Requesting SpawnPacket for ${uid}`);
                    this.send(Packets.RequestEntity(this.player.id, uid));
                    this.requestQueue.set(uid, false);
                }
            }
        }

        else
        {
            entity.serverPosition = new Vector(x, y);
            entity.serverVelocity = new Vector(vx, vy);
        }
    }

    LoginResponseHandler(rdr)
    {
        let uid = rdr.getInt32(4, true);
        let ticks = rdr.getInt32(8, true);
        let x = rdr.getFloat32(12, true);
        let y = rdr.getFloat32(16, true);
        let map_width = rdr.getInt32(20, true);
        let map_height = rdr.getInt32(24, true);
        let viewDistance = rdr.getInt16(28, true);

        this.game.MAP_WIDTH = map_width;
        this.game.MAP_HEIGHT = map_height;
        this.camera.distance = viewDistance;

        this.player.id = uid;
        this.player.position = new Vector(x, y);
        this.player.serverPosition = new Vector(x, y);
        this.player.input.setup(this.game);
        this.game.addEntity(this.player);
    }

    send(packet)
    {
        window.bytesSent += packet.byteLength;
        this.socket.send(packet);
    }

    toColor(num)
    {
        return "#" + num.toString(16).padStart(6, '0');
    }
}