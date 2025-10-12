import { NTT } from "../ecs/core/NTT";

/**
 * Binary packet writer for serializing network data.
 * Provides methods for writing various data types with little-endian byte order.
 */
export class EvPacketWriter {
  private buffer: ArrayBuffer;
  private view: DataView;
  private offset = 0;

  constructor(packetId: number) {
    this.buffer = new ArrayBuffer(4096);
    this.view = new DataView(this.buffer);
    this.i16(0);
    this.i16(packetId);
  }

  /**
   * Jump to specific position in buffer
   */
  Goto(position: number): EvPacketWriter {
    this.offset = position;
    return this;
  }

  Guid(ntt: string | NTT) {
    let uid: any

    if (uid instanceof NTT) {
      uid = uid.id
    }
    else {
      uid = ntt.toString()
    }

    uid = uid as string

    const parts = uid.split("-");
    if (5 !== parts.length) {
      throw new Error("Invalid GUID format.");
    }

    const data1 = parts[0]
      .match(/.{2}/g)!
      .reverse()
      .map((hex: string) => parseInt(hex, 16));
    const data2 = parts[1]
      .match(/.{2}/g)!
      .reverse()
      .map((hex: string) => parseInt(hex, 16));
    const data3 = parts[2]
      .match(/.{2}/g)!
      .reverse()
      .map((hex: string) => parseInt(hex, 16));
    const data4 = parts[3].match(/.{2}/g)!.map((hex: string) => parseInt(hex, 16));
    const data5 = parts[4].match(/.{2}/g)!.map((hex: string) => parseInt(hex, 16));

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
  u32(value: number, offset = -1): EvPacketWriter {
    if (-1 !== offset) {
      this.offset = offset;
    }
    this.view.setUint32(this.offset, value, true);
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

  u16(value: number, offset = -1): EvPacketWriter {
    if (-1 !== offset) {
      this.offset = offset;
    }
    this.view.setUint16(this.offset, value, true);
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
    const length = Math.min(255, bytes.length);
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
    const length = Math.min(65_535, bytes.length);
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
    const length = bytes.length;
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

  /**
   * Finalize packet by setting length header and trimming buffer
   */
  FinishPacket(): EvPacketWriter {
    const len = this.offset;
    this.view.setInt16(0, len, true);

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

  /**
   * Get the final packet buffer (only the written portion)
   */
  ToArray(): ArrayBuffer {
    return this.buffer.slice(0, this.offset);
  }
}
