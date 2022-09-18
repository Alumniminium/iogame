import { Packets } from "./Packets.js";
import { Vector } from "../Vector.js";
import { BoxEntity } from "../entities/BoxEntity.js";
import { CircleEntity } from "../entities/CircleEntity.js";
import { LineEntity } from "../entities/LineEntity.js";

export class Net
{
    host = "localhost";
    socket = null;
    connected = false;
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

        this.socket = new WebSocket("ws://" + this.host + ":5000/chat");

        fetch("http://" + this.host + "/BaseResources.json").then(r => r.json()).then(json =>
        {
            // const BodyDamage, BorderColor, Color, Drag, Elasticity, Health, Mass, MaxAliveNum, MaxSpeed, Sides, Size = json;
            this.baseResources = json;
        });

        this.socket.binaryType = 'arraybuffer';
        this.socket.onmessage = this.OnPacket.bind(this);
        this.socket.onopen = this.Connected.bind(this);

        for (let i = 0; i < 5; i++)
        {
            if (this.connected)
                return true;

            await this.sleep(1000);
        }
        return false;
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

            console.log("received packet " + id);

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
                case 1116:
                    {
                        this.CircleEntitySpawn(rdr);
                        break;
                    }
                case 1117:
                    {
                        this.BoxEntitySpawn(rdr);
                        break;
                    }
                case 1118:
                    {
                        this.LineEntitySpawnHandler(rdr);
                        break;
                    }
                case 9000:
                    this.PingHandler(rdr, packet);
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
    BoxEntitySpawn(rdr)
    {
        const uniqueId = rdr.getInt32(4, true);
        const w = rdr.getInt16(8, true);
        const h = rdr.getInt16(10, true);
        const r = rdr.getFloat32(12, true);
        const x = rdr.getFloat32(16, true);
        const y = rdr.getFloat32(20, true);
        const c = rdr.getUint32(24, true);

        if (this.requestQueue.has(uniqueId))
            this.requestQueue.delete(uniqueId);

        let entity = new BoxEntity(uniqueId, x, y, w, h, r, this.toColor(c));
        window.game.addEntity(entity);
    }
    CircleEntitySpawn(rdr)
    {
        const uniqueId = rdr.getInt32(4, true);
        const r = rdr.getUint16(8, true);
        const rot = rdr.getFloat32(10, true);
        const x = rdr.getFloat32(14, true);
        const y = rdr.getFloat32(18, true);
        const c = rdr.getUint32(22, true);

        if (this.requestQueue.has(uniqueId))
            this.requestQueue.delete(uniqueId);

        let entity = new CircleEntity(uniqueId, x, y, rot, r, this.toColor(c));
        window.game.addEntity(entity);
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

            // if (window.avgFps < 140)
            // {
            //     window.resolutionMultiplier -= 0.05;
            //     console.log("changing resolution to " + 100 * window.resolutionMultiplier + "%");
            //     window.game.renderer.setCanvasDimensions();
            // }
            // else if (window.avgFps > 140 && window.resolutionMultiplier < 1)
            // {
            //     window.resolutionMultiplier += 0.01;
            //     console.log("changing resolution to " + 100 * window.resolutionMultiplier + "%");
            //     window.game.renderer.setCanvasDimensions();
            // }
            window.avgFps += window.fps;
            window.avgFps /= 1000;
        }
        else
            this.send(data);
    }

    StatusHandler(rdr)
    {
        const uid = rdr.getInt32(4, true);
        const val = rdr.getUint32(8, true);
        const type = rdr.getInt32(12, true);
        console.log(`Status: Id=${uid}, Val=${val}, Type=${type}`);

        if (window.game.entities.has(uid))
        {
            // console.log(`Status: Id=${uid}, Val=${val}, Type=${type}`);
            const entity = window.game.entities.get(uid);

            switch (type)
            {
                // Alive
                case 0:
                    console.log(`setting alive of ${uid} to ${val}}`);
                    if (val == 0)
                        window.game.removeEntity(entity);
                    break;
                // Health
                case 1:
                    // console.log(`setting health of ${uid} to ${val}/${entity.maxHealth}`);
                    entity.health = val;
                    if (entity.health <= 0)
                        window.game.removeEntity(entity);
                    break;
                case 2: //?
                    break;
                case 3: // Size
                    break;
                case 4: // Direction
                    break;
                case 5:
                    window.playerThrottle = val;
                    break;
                case 10:
                    window.batteryCapacity = val;
                    break;
                case 11:
                    window.batteryCharge = val;
                    break;
                case 12:
                    window.batteryChargeRate = val;
                    break;
                case 13:
                    window.batteryDischargeRate = val;
                    break;
                case 14:
                    window.enginePowerDraw = val;
                    break;
                case 15:
                    window.shieldPowerDraw = val;
                    break;
                case 16:
                    window.weaponPowerDraw = val;
                    break;
                case 20:
                    window.shieldCharge = val;
                    break;
                case 21:
                    window.shieldMaxCharge = val;
                    break;
                case 22:
                    window.shieldRechargeRate = val;
                    break;
                case 23:
                    window.shieldPowerUse = val;
                    break;
                case 24:
                    window.shieldPowerUseRecharge = val;
                    break;
                case 25:
                    window.shieldRadius = val;
                    break;
                case 100: //Inv capacity
                    window.playerStorageCapacity = val;
                    break;
                case 101: //Inv Triangles
                    window.playerTriangles = val;
                    break;
                case 102: //Inv Squares
                    window.playerSquares = val;
                    break;
                case 103: //Inv Pentagons
                    window.playerPentagons = val;
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
        this.camera.distance = viewDistance;

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