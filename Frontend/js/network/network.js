import { Packets } from "./packets.js";
import { Vector } from "../vector.js";
import { Entity } from "../entities/entity.js";
import { Bullet } from "../entities/bullet.js";

export class Net
{
    socket = null;
    connected = false;
    player = null;
    camera = null;

    requestQueue = new Map();

    connect()
    {
        window.packetsPerSecondReceived = 0;
        this.player = window.game.player;
        this.camera = window.game.camera;

        this.socket = new WebSocket("ws://localhost:5000/chat");
        this.socket.binaryType = 'arraybuffer';
        this.socket.onmessage = this.OnPacket.bind(this);
        this.socket.onopen = this.Connected.bind(this);
    }

    Connected()
    {
        console.log("connected");
        this.connected = true;
        this.send(Packets.LoginRequestPacket(this.player.name, ""));
    }

    sendMessage(text)
    {
        this.send(Packets.ChatPacket(this.player.id, this.player.name, text));
    }

    OnPacket(buffer)
    {
        window.bytesReceived += buffer.data.byteLength;
        window.packetsPerSecondReceived++;
        const data = buffer.data;
        let bytesProcessed = 0;
        while (bytesProcessed < data.byteLength)
        {
            let packet = data.slice(bytesProcessed);
            const rdr = new DataView(packet);
            const len = rdr.getInt16(0, true);
            packet = packet.slice(0, len);
            const id = rdr.getInt16(2, true);

            switch (id)
            {
                case 2:
                    {
                        this.LoginResponseHandler(rdr);
                        break;
                    }
                case 1004:
                    {
                        this.ChatHandler(rdr);
                        break;
                    }
                case 1005:
                    {
                        this.MovementHandler(rdr);
                        break;
                    }
                case 1010:
                    {
                        this.StatusHandler(rdr);
                        break;
                    }
                case 1015:
                    {
                        this.SpawnPacketHandler(rdr);
                        break;
                    }
                case 1116:
                    {
                        this.ResourceSpawnPacket(rdr);
                        break;
                    }
                case 9000:
                    this.PingPacketHandler(rdr, packet);
                    break;
            }
            bytesProcessed += len;
        }


    }

    ChatHandler(rdr)
    {
        const fromLen = rdr.getUint8(4, true);
        const from = rdr.getString(5, fromLen);
        const textlene = rdr.getUint8(21, true);
        const text = rdr.getString(22, textlene);
        window.game.addChatLogLine(from + ": " + text);
    }
    ResourceSpawnPacket(rdr)
    {
        const uniqueId = rdr.getInt32(4, true);
        const resourceId = rdr.getUint16(8, true);
        const direction = rdr.getFloat32(10, true);
        const x = rdr.getFloat32(14, true);
        const y = rdr.getFloat32(18, true);
        const vx = rdr.getFloat32(22, true);
        const vy = rdr.getFloat32(26, true);

        if (this.requestQueue.has(uniqueId))
            this.requestQueue.delete(uniqueId);

        let entity = new Entity(uniqueId);
        entity.sides = resourceId;
        entity.direction = direction;
        entity.size = resourceId == 4 ? 100 : resourceId == 3 ? 150 : resourceId == 5 ? 200 : resourceId == 6 ? 300 : 500;
        entity.fillColor = resourceId == 4 ? "#ffe869" : resourceId == 3 ? "#ff5050" : resourceId >4 ? "#4B0082" : "white";
        entity.maxHealth = resourceId == 3 ? 200 : resourceId == 4 ? 100 : resourceId == 5 ? 400 : resourceId == 6 ? 800 : 1000;
        entity.health = resourceId == 3 ? 200 : resourceId == 4 ? 100 : resourceId == 5 ? 400 : resourceId == 6 ? 800 : 1000;
        // entity.elasticity = resourceId == 3 ? 1 : resourceId == 4 ? 0 : resourceId == 5 ? -1 : resourceId == 6 ? 0.5 : -0.5;
        entity.elasticity = 1;
        entity.drag = 0.9999;
        entity.position = new Vector(x, y);
        entity.serverPosition = new Vector(x, y);
        entity.velocity = new Vector(vx, vy);
        entity.serverVelocity = new Vector(vx, vy);
        entity.maxSpeed = 1500;

        window.game.addEntity(entity);
    }

    PingPacketHandler(rdr, data)
    {
        const ping = rdr.getInt16(4, true);
        if (ping != 0)
        {
            window.ping = ping;
            window.bytesPerSecondSent = window.bytesSent;
            window.totalBytesSent += window.bytesSent;
            window.bytesSent = 0;
            window.packetsPerSecondReceived = 0;

            window.bytesPerSecondReceived = window.bytesReceived;
            window.totalBytesReceived += window.bytesReceived;
            window.bytesReceived = 0;

            window.game.addChatLogLine("FPS: "+window.fps);
        }
        else
            this.send(data);
    }

    SpawnPacketHandler(rdr)
    {
        const uniqueId = rdr.getInt32(4, true);
        const ownerId = rdr.getInt32(8, true);
        const direction = rdr.getFloat32(12, true);
        const size = rdr.getUint16(16, true);
        const maxHealh = rdr.getUint32(18, true);
        const curHealth = rdr.getUint32(22, true);
        const color = rdr.getUint32(26, true);
        const borderColor = rdr.getUint32(30, true);
        const drag = rdr.getFloat32(34, true);
        const sides = rdr.getUint8(38, true);
        const x = rdr.getFloat32(39, true);
        const y = rdr.getFloat32(43, true);
        const vx = rdr.getFloat32(47, true);
        const vy = rdr.getFloat32(51, true);
        const maxSpeed = rdr.getUint32(55, true);

        if (this.requestQueue.has(uniqueId))
            this.requestQueue.delete(uniqueId);

        let entity = new Entity(uniqueId);
        if (window.game.entities.has(ownerId))
        {
            entity = new Bullet(uniqueId, window.game.entities.get(ownerId));
        }
        entity.drag = drag;
        entity.sides = sides;
        entity.direction = direction;
        entity.size = size;
        entity.maxHealth = maxHealh;
        entity.health = curHealth;
        entity.fillColor = this.toColor(color);
        entity.strokeColor = this.toColor(borderColor);
        entity.position = new Vector(x, y);
        entity.serverPosition = new Vector(x, y);
        entity.velocity = new Vector(vx, vy);
        entity.serverVelocity = new Vector(vx, vy);
        entity.maxSpeed = maxSpeed;

        // console.log(`Spawn: Id=${uniqueId}, Dir=${direction}, Size=${size}, Health=${curHealth}, MaxHealth=${maxHealh}, Drag=${drag}`);
        window.game.addEntity(entity);
    }

    StatusHandler(rdr)
    {
        const uid = rdr.getInt32(4, true);
        const val = rdr.getUint32(8, true);
        const type = rdr.getInt32(12, true);

        if (window.game.entities.has(uid))
        {
            const entity = window.game.entities.get(uid);

            switch (type)
            {
                // Alive
                case 0:
                    // console.log(`setting alive of ${uid} to ${val}}`);
                    if (val == 0)
                        window.game.removeEntity(entity);
                    break;
                // Health
                case 1:
                    console.log(`setting health of ${uid} to ${val}/${entity.maxHealth}`);
                    entity.health = val;
                    if (entity.health <= 0)
                        window.game.removeEntity(entity);
                    break;
            }
        }
    }
    MovementHandler(rdr)
    {
        const uid = rdr.getInt32(4, true);
        const ticks = rdr.getInt32(8, true);
        const x = Math.round(rdr.getFloat32(12, true) * 100) / 100;
        const y = Math.round(rdr.getFloat32(16, true) * 100) / 100;
        const vx = rdr.getFloat32(20, true);
        const vy = rdr.getFloat32(24, true);

        let entity = window.game.entities.get(uid);
        if (entity == undefined)
        {
            if (this.requestQueue.has(uid) == false)
            {
                console.log(`Requesting SpawnPacket for ${uid}`);
                this.send(Packets.RequestEntity(this.player.id, uid));
                this.requestQueue.set(uid, false);
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
        const uid = rdr.getInt32(4, true);
        const ticks = rdr.getInt32(8, true);
        const x = rdr.getFloat32(12, true);
        const y = rdr.getFloat32(16, true);
        const map_width = rdr.getInt32(20, true);
        const map_height = rdr.getInt32(24, true);
        const viewDistance = rdr.getInt16(28, true);

        window.game.MAP_WIDTH = map_width;
        window.game.MAP_HEIGHT = map_height;
        this.camera.distance = viewDistance;

        this.player.id = uid;
        this.player.position = new Vector(x, y);
        this.player.serverPosition = new Vector(x, y);
        this.player.input.setup(window.game);
        window.game.addEntity(this.player);
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