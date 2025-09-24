export class EvPacketWriter {
  private buffer: ArrayBuffer;
  private view: DataView;
  private offset = 0;

  constructor(packetId: number) {
    this.buffer = new ArrayBuffer(4096);
    this.view = new DataView(this.buffer);
    this.i16(0); // Length placeholder
    this.i16(packetId); // Packet ID
  }

  Goto(position: number): EvPacketWriter {
    this.offset = position;
    return this;
  }

  Guid(uid: string) {
    const parts = uid.split("-");
    if (5 !== parts.length) {
      throw new Error("Invalid GUID format.");
    }

    const data1 = parts[0]
      .match(/.{2}/g)!
      .reverse()
      .map((hex) => parseInt(hex, 16));
    const data2 = parts[1]
      .match(/.{2}/g)!
      .reverse()
      .map((hex) => parseInt(hex, 16));
    const data3 = parts[2]
      .match(/.{2}/g)!
      .reverse()
      .map((hex) => parseInt(hex, 16));
    const data4 = parts[3].match(/.{2}/g)!.map((hex) => parseInt(hex, 16));
    const data5 = parts[4].match(/.{2}/g)!.map((hex) => parseInt(hex, 16));

    const allBytes = [...data1, ...data2, ...data3, ...data4, ...data5];
    for (const byte of allBytes) {
      this.i8(byte);
    }

    return this;
  }

  StringWithByteLength(value: string) {
    const bytes = new TextEncoder().encode(value);
    for (const byte of bytes) {
      this.i8(byte);
    }
    return this;
  }

  i64(value: bigint, offset = -1): EvPacketWriter {
    if (-1 !== offset) {
      this.offset = offset;
    }
    this.view.setBigUint64(this.offset, value, true);
    this.offset += 8;
    return this;
  }

  i32(value: number, offset = -1): EvPacketWriter {
    if (-1 !== offset) {
      this.offset = offset;
    }
    this.view.setInt32(this.offset, value, true);
    this.offset += 4;
    return this;
  }

  i16(value: number, offset = -1): EvPacketWriter {
    if (-1 !== offset) {
      this.offset = offset;
    }
    this.view.setInt16(this.offset, value, true);
    this.offset += 2;
    return this;
  }

  i8(value: number, offset = -1): EvPacketWriter {
    if (-1 !== offset) {
      this.offset = offset;
    }
    this.view.setInt8(this.offset, value);
    this.offset += 1;
    return this;
  }

  f64(value: number, offset = -1): EvPacketWriter {
    if (-1 !== offset) {
      this.offset = offset;
    }
    this.view.setFloat64(this.offset, value, true);
    this.offset += 8;
    return this;
  }

  f32(value: number, offset = -1): EvPacketWriter {
    if (-1 !== offset) {
      this.offset = offset;
    }
    this.view.setFloat32(this.offset, value, true);
    this.offset += 4;
    return this;
  }

  Skip(count: number): EvPacketWriter {
    this.offset += count;
    return this;
  }

  StringWith8bitLengthPrefix(value: string, offset = -1): EvPacketWriter {
    if (-1 !== offset) {
      this.offset = offset;
    }
    const bytes = new TextEncoder().encode(value);
    const length = Math.min(255, bytes.length); // UTF-8 length
    this.i8(length);
    for (let i = 0; i < length; i++) {
      this.i8(bytes[i]);
    }
    return this;
  }
  StringWith16bitLengthPrefix(value: string, offset = -1): EvPacketWriter {
    if (-1 !== offset) {
      this.offset = offset;
    }
    const bytes = new TextEncoder().encode(value);
    const length = Math.min(65_535, bytes.length); // UTF-8 length
    this.i16(length);
    for (let i = 0; i < length; i++) {
      this.i8(bytes[i]);
    }
    return this;
  }
  StringWith32bitLengthPrefix(value: string, offset = -1): EvPacketWriter {
    if (-1 !== offset) {
      this.offset = offset;
    }
    const bytes = new TextEncoder().encode(value);
    const length = bytes.length; // UTF-8 length
    this.i32(length);
    for (let i = 0; i < length; i++) {
      this.i8(bytes[i]);
    }
    return this;
  }

  StringWithoutLength(value: string, offset = -1): EvPacketWriter {
    if (-1 !== offset) {
      this.offset = offset;
    }
    const bytes = new TextEncoder().encode(value);
    for (const byte of bytes) {
      this.i8(byte);
    }
    return this;
  }

  FinishPacket(): EvPacketWriter {
    const len = this.offset;
    this.view.setInt16(0, len, true); // Set length at offset 0

    const newBuffer = new ArrayBuffer(len);
    const newView = new DataView(newBuffer);
    const oldView = new DataView(this.buffer);

    for (let i = 0; i < len; i++) {
      newView.setInt8(i, oldView.getInt8(i));
    }

    this.buffer = newBuffer;
    this.view = newView;
    this.offset = len;

    return this;
  }

  ToArray(): ArrayBuffer {
    return this.buffer;
  }
}
