import { PacketHeader } from "./PacketHeader";

export class EvPacketReader {
  private view: DataView;
  private offset = 0;
  private buffer: ArrayBuffer;
  private uint8View: Uint8Array;

  // Shared TextDecoder instance for better performance
  private static textDecoder = new TextDecoder();

  // Pre-calculated hex lookup table for GUID formatting
  private static hexLookup = Array.from({ length: 256 }, (_, i) =>
    i.toString(16).padStart(2, "0"),
  );

  constructor(buffer: ArrayBuffer) {
    this.buffer = buffer;
    this.view = new DataView(buffer);
    this.uint8View = new Uint8Array(buffer);
  }

  Header(): PacketHeader {
    const length = this.i16();
    const id = this.i16();
    return new PacketHeader(length, id);
  }

  Goto(position: number): EvPacketReader {
    this.offset = position;
    return this;
  }

  Guid(offset = -1): string {
    if (-1 !== offset) {
      this.offset = offset;
    }

    // Read all 16 bytes at once using slice - much faster than individual reads
    const guidBytes = this.uint8View.slice(this.offset, this.offset + 16);
    this.offset += 16;

    // Use pre-calculated hex lookup table for fast conversion
    const hex = EvPacketReader.hexLookup;

    // Format GUID using fast string concatenation
    return (
      `${hex[guidBytes[3]]}${hex[guidBytes[2]]}${hex[guidBytes[1]]}${hex[guidBytes[0]]}-` +
      `${hex[guidBytes[5]]}${hex[guidBytes[4]]}-` +
      `${hex[guidBytes[7]]}${hex[guidBytes[6]]}-` +
      `${hex[guidBytes[8]]}${hex[guidBytes[9]]}-` +
      `${hex[guidBytes[10]]}${hex[guidBytes[11]]}${hex[guidBytes[12]]}${hex[guidBytes[13]]}${hex[guidBytes[14]]}${hex[guidBytes[15]]}`
    );
  }

  StringWithByteLength(offset = -1): string {
    if (-1 !== offset) {
      this.offset = offset;
    }

    // Create a view directly on the buffer slice for 16 bytes
    const stringView = new Uint8Array(this.buffer, this.offset, 16);
    this.offset += 16;

    // Use shared TextDecoder instance
    return EvPacketReader.textDecoder.decode(stringView);
  }

  i64(offset = -1): bigint {
    if (-1 !== offset) {
      this.offset = offset;
    }
    const value = this.view.getBigUint64(this.offset, true);
    this.offset += 8;
    return value;
  }

  i32(offset = -1): number {
    if (-1 !== offset) {
      this.offset = offset;
    }
    const value = this.view.getInt32(this.offset, true);
    this.offset += 4;
    return value;
  }

  u16(offset = -1): number {
    if (-1 !== offset) {
      this.offset = offset;
    }
    const value = this.view.getUint16(this.offset, true);
    this.offset += 2;
    return value;
  }

  u32(offset = -1): number {
    if (-1 !== offset) {
      this.offset = offset;
    }
    const value = this.view.getUint32(this.offset, true);
    this.offset += 4;
    return value;
  }

  i16(offset = -1): number {
    if (-1 !== offset) {
      this.offset = offset;
    }
    const value = this.view.getInt16(this.offset, true);
    this.offset += 2;
    return value;
  }

  i8(offset = -1): number {
    if (-1 !== offset) {
      this.offset = offset;
    }
    const value = this.view.getInt8(this.offset);
    this.offset += 1;
    return value;
  }

  f64(offset = -1): number {
    if (-1 !== offset) {
      this.offset = offset;
    }
    const value = this.view.getFloat64(this.offset, true);
    this.offset += 8;
    return value;
  }

  f32(offset = -1): number {
    if (-1 !== offset) {
      this.offset = offset;
    }
    const value = this.view.getFloat32(this.offset, true);
    this.offset += 4;
    return value;
  }

  Skip(count: number): EvPacketReader {
    this.offset += count;
    return this;
  }

  StringWith8bitLengthPrefix(offset = -1): string {
    if (-1 !== offset) {
      this.offset = offset;
    }
    const length = this.view.getUint8(this.offset);
    this.offset += 1;

    // Create a view directly on the buffer slice - no copying needed
    const stringView = new Uint8Array(this.buffer, this.offset, length);
    this.offset += length;

    // Use shared TextDecoder instance
    return EvPacketReader.textDecoder.decode(stringView);
  }
  StringWith16bitLengthPrefix(offset = -1): string {
    if (-1 !== offset) {
      this.offset = offset;
    }
    const length = this.view.getUint16(this.offset, true);
    this.offset += 2;

    // Create a view directly on the buffer slice - no copying needed
    const stringView = new Uint8Array(this.buffer, this.offset, length);
    this.offset += length;

    // Use shared TextDecoder instance
    return EvPacketReader.textDecoder.decode(stringView);
  }
  StringWith32bitLengthPrefix(offset = -1): string {
    if (-1 !== offset) {
      this.offset = offset;
    }
    const length = this.view.getInt32(this.offset, true);
    this.offset += 4;

    // Create a view directly on the buffer slice - no copying needed
    const stringView = new Uint8Array(this.buffer, this.offset, length);
    this.offset += length;

    // Use shared TextDecoder instance
    return EvPacketReader.textDecoder.decode(stringView);
  }

  StringWithoutLength(length: number, offset = -1): string {
    if (-1 !== offset) {
      this.offset = offset;
    }

    // Create a view directly on the buffer slice - no copying needed
    const stringView = new Uint8Array(this.buffer, this.offset, length);
    this.offset += length;

    // Use shared TextDecoder instance
    return EvPacketReader.textDecoder.decode(stringView);
  }
}
