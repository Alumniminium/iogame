import { Packets } from "./Packets.js";
import { Vector } from "../Vector.js";
import { BoxEntity } from "../entities/BoxEntity.js";
import { CircleEntity } from "../entities/CircleEntity.js";
import { TriangleEntity } from "../entities/TriangleEntity.js";
import { LineEntity } from "../entities/LineEntity.js";

export class Net
{
    host = "localhost";
    // host = "io.her.st";
    socket = null;
    connected = false;
    failed = false;
    player = null;
    camera = null;

    requestQueue = new Map();
    baseResources = [];
    sleep = ms => new Promise(resolve => setTimeout(resolve, ms));

    async connect()
    {
        window.packetsPerSecondReceived = 0;
        this.player = window.game.player;
        this.camera = window.game.camera;

        this.socket = new WebSocket("ws://" + this.host + "/chat");
        // this.socket = new WebSocket("wss://" + this.host + "/chat");

        fetch("http://" + this.host + "/BaseResources.json").then(r => r.json()).then(json =>
        // fetch("https://" + this.host + "/BaseResources.json").then(r => r.json()).then(json =>
        {
            // const BodyDamage, BorderColor, Color, Drag, Elasticity, Health, Mass, MaxAliveNum, MaxSpeed, Sides, Size = json;
            this.baseResources = json;
        });

        this.socket.binaryType = 'arraybuffer';
        this.socket.onmessage = this.OnPacket.bind(this);
        this.socket.onopen = this.Connected.bind(this);
        this.socket.onerror = this.Error.bind(this);

        for (let i = 0; i < 5; i++)
        {
            if (this.connected)
            {
                this.send(Packets.LoginRequestPacket(this.player.name, ""));
                return true;
            }
            if(this.failed)
            {
                return false;
            }

            await this.sleep(1000);
        }
        return false;
    }
    Error = () => this.failed = true;
    Connected = () => this.connected = true;
    sendMessage = (text) => this.send(Packets.ChatPacket(this.player.id, text));

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
                case 3:
                    {
                        this.AssociateIdHandler(rdr);
                        break;
                    }
                case 4:
                    {
                        this.StatusHandler(rdr);
                        break;
                    }
                case 10:
                    {
                        this.ChatHandler(rdr);
                        break;
                    }
                case 20:
                    {
                        this.MovementHandler(rdr);
                        break;
                    }
                case 31:
                    {
                        this.CustomEntitySpawn(rdr);
                        break;
                    }
                case 33:
                    {
                        this.LineEntitySpawnHandler(rdr);
                        break;
                    }
                case 90:
                    this.PingHandler(rdr, packet);
                    break;
            }
            bytesProcessed += len;
        }
    }
    AssociateIdHandler(rdr)
    {
        const id = rdr.getInt32(4, true);
        const nameLen = rdr.getUint8(8, true);
        const name = rdr.getString(9, nameLen);
        console.log("associated id " + id + " with name " + name);
        window.game.addEntityName(id, name);
    }

    ChatHandler(rdr)
    {
        const fromId = rdr.getUint32(4, true);
        const channel = rdr.getUint8(8, true);
        const textLen = rdr.getUint8(9, true);
        const text = rdr.getString(10, textLen);
        let name = window.game.entityNames.get(fromId);
        if(channel == 0)
            window.game.addChatLogLine("["+channel+"] "+name + ": " + text);
        if (channel == 10)
            window.game.addLeaderboardLine(text);
    }
    CustomEntitySpawn(rdr)
    {
        const uniqueId = rdr.getInt32(4, true);
        const type = rdr.getInt32(8,true);
        const w = rdr.getFloat32(12, true);
        const h = rdr.getFloat32(16, true);
        const r = rdr.getFloat32(20, true);
        const x = rdr.getFloat32(24, true);
        const y = rdr.getFloat32(28, true);
        const c = rdr.getUint32(32, true);

        if (this.requestQueue.has(uniqueId))
            this.requestQueue.delete(uniqueId);

        let entity = null;
        if(type == 2)
            entity = new BoxEntity(uniqueId, x, y, w, h, r, this.toColor(c));
        if(type == 1)
            entity = new TriangleEntity(uniqueId, x, y, r, w,h, this.toColor(c));
        if(type == 0)
            entity = new CircleEntity(uniqueId, x, y, r, w, this.toColor(c));

        window.game.addEntity(entity);
        console.log("spawned entity " + uniqueId + " at " + x + ", " + y + " with color " + this.toColor(c) + " and size " + w + ", " + h);
    }

    PingHandler(rdr, data)
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

            window.avgFps += window.fps;
            window.avgFps /= 1000;
        }
        else
            this.send(data);
    }

    StatusHandler(rdr)
    {
        const uid = rdr.getInt32(4, true);
        const val = rdr.getFloat64(8, true);
        const type = rdr.getInt32(16, true);
        // console.log(`Status: Id=${uid}, Val=${val}, Type=${type}`);

        if (window.game.entities.has(uid))
        {
            // console.log(`Status: Id=${uid}, Val=${val}, Type=${type}`);
            const entity = window.game.entities.get(uid);

            switch (type)
            {
                // Alive
                case 0:
                    if (val == 0)
                        window.game.removeEntity(entity);
                    break;
                // Health
                case 1:
                    entity.health = val;
                    break;
                case 2:
                    entity.maxHealth = val; 
                    break;
                case 3:
                    entity.size = val;
                    break;
                case 4: // Direction
                    entity.direction = val;
                    break;
                case 5:
                    entity.throttle = val;
                    break;
                case 10:
                    entity.batteryCapacity = val;
                    break;
                case 11:
                    entity.batteryCharge = val;
                    break;
                case 12:
                    entity.batteryChargeRate = val;
                    break;
                case 13:
                    entity.batteryDischargeRate = val;
                    break;
                case 14:
                    entity.enginePowerDraw = val;
                    break;
                case 15:
                    entity.shieldPowerDraw = val;
                    break;
                case 16:
                    entity.weaponPowerDraw = val;
                    break;
                case 20:
                    entity.shieldCharge = val;
                    break;
                case 21:
                    entity.shieldMaxCharge = val;
                    break;
                case 22:
                    entity.shieldRechargeRate = val;
                    break;
                case 23:
                    entity.shieldPowerUse = val;
                    break;
                case 24:
                    entity.shieldPowerUseRecharge = val;
                    break;
                case 25:
                    entity.shieldRadius = val;
                    break;
                case 100: //Inv capacity
                    entity.playerStorageCapacity = val;
                    break;
                case 101: //Inv Triangles
                    entity.playerTriangles = val;
                    break;
                case 102: //Inv Squares
                    entity.playerSquares = val;
                    break;
                case 103: //Inv Pentagons
                    entity.playerPentagons = val;
                    break;
                case 200:
                    if(entity.level == val)
                        break;
                    if(uid == this.player.id)
                        this.camera.distance *= 1.03;
                    entity.level = val;
                    break;
                case 201:
                    entity.experience = val;
                    break;
                case 202:
                    entity.experienceToNextLevel = val;
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
        const r = rdr.getFloat32(20, true);
        // const vx = rdr.getFloat32(20, true);
        // const vy = rdr.getFloat32(24, true);
        console.log(`Movement: Id=${uid}, Ticks=${ticks}, X=${x}, Y=${y}, R=${r}`);
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
            entity.direction = r;
        }
    }
    LineEntitySpawnHandler(rdr)
    {
        const uid = rdr.getInt32(4, true) * 100;
        const targetUid = rdr.getInt32(8, true);
        const startx = Math.round(rdr.getFloat32(12, true) * 100) / 100;
        const starty = Math.round(rdr.getFloat32(16, true) * 100) / 100;
        const endx = Math.round(rdr.getFloat32(20, true) * 100) / 100;
        const endy = Math.round(rdr.getFloat32(24, true) * 100) / 100;
        // const vx = rdr.getFloat32(20, true);
        // const vy = rdr.getFloat32(24, true);

        let line = window.game.entities.get(uid);
        if (line == undefined)
        {
            line = new LineEntity(uid, new Vector(startx, starty), new Vector(endx, endy));
            window.game.addEntity(line);
        }
        line.from = new Vector(startx, starty);
        line.to = new Vector(endx, endy);
    }

    LoginResponseHandler(rdr)
    {
        const uid = rdr.getInt32(4, true);
        const ticks = rdr.getInt32(8, true);
        const x = rdr.getFloat32(12, true);
        const y = rdr.getFloat32(16, true);
        const map_width = rdr.getInt32(20, true);
        const map_height = rdr.getInt32(24, true);
        const viewDistance = rdr.getUint16(28, true);
        const playerSize = rdr.getFloat32(30, true);
        const playerDrag = rdr.getFloat32(34, true);
        const playerElasticity = rdr.getFloat32(38, true);
        const playerSpeed = rdr.getUint16(42, true);

        window.game.MAP_WIDTH = map_width;
        window.game.MAP_HEIGHT = map_height;
        this.camera.distance = viewDistance * 0.8;

        this.player.id = uid;
        this.player.position = new Vector(x, y);
        this.player.serverPosition = new Vector(x, y);
        this.player.elasticity = playerElasticity;
        this.player.drag = playerDrag;
        this.player.size = playerSize;
        this.player.maxSpeed = playerSpeed;

        window.input.setup(window.game);
        window.game.addEntity(this.player);
    }

    send(packet)
    {
        try
        {
            window.bytesSent += packet.byteLength;
            this.socket.send(packet);
        }
        catch (e)
        {
            window.NewGame();
        }
    }

    toColor(num)
    {
        return "#" + num.toString(16).padStart(6, '0');
    }
}