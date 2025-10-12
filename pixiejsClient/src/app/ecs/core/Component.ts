import { World } from "./World";
import { EvPacketReader } from "../../network/EvPacketReader";
import { EvPacketWriter } from "../../network/EvPacketWriter";
import { ComponentTypeId } from "../../enums/ComponentIds";
import { NTT } from "./NTT";

export type FieldType = "i8" | "u8" | "i16" | "u16" | "i32" | "u32" | "i64" | "u64" | "f32" | "f64" | "bool" | "guid" | "vector2" | "string64";

interface FieldMetadata {
  propertyKey: string;
  index: number;
  type: FieldType;
  skip?: boolean;
}

// Component registry - auto-populated by @component decorator
export const ComponentRegistry = new Map<ComponentTypeId, typeof Component>();

// Field metadata storage
const fieldMetadata = new Map<any, FieldMetadata[]>();

// Decorator for component registration
export function component(componentType: ComponentTypeId) {
  return function (target: any) {
    ComponentRegistry.set(componentType, target);
    target.prototype.componentType = componentType;
  };
}

// Decorator for field serialization
export function serverField(index: number, type: FieldType, options?: { skip?: boolean }) {
  return function (target: any, propertyKey: string | symbol, _descriptor?: PropertyDescriptor) {
    const constructor = target.constructor;
    if (!fieldMetadata.has(constructor)) {
      fieldMetadata.set(constructor, []);
    }
    fieldMetadata.get(constructor)!.push({
      propertyKey: propertyKey as string,
      index,
      type,
      skip: options?.skip,
    });
  };
}

/**
 * Base class for all ECS components.
 * Components are data containers attached to entities that define their properties and behavior.
 */
export abstract class Component {
  readonly ntt: NTT;
  componentType!: ComponentTypeId;
  @serverField(0, "i64") public changedTick: bigint = 0n;
  public created: number;

  constructor(ntt: NTT | string) {
    this.ntt = typeof ntt === "string" ? NTT.from(ntt) : ntt;
    this.created = Date.now();
    this.changedTick = World.currentTick;
  }

  /**
   * Returns the class name of this component type
   */
  getTypeName(): string {
    return this.constructor.name;
  }

  /**
   * Serialize this component to binary (like backend's Serialize method)
   */
  toBuffer(): ArrayBuffer {
    // Create a temporary buffer to collect serialized data
    const tempBuffer = new ArrayBuffer(4096);
    const tempView = new DataView(tempBuffer);
    let offset = 0;

    // Helper to write bytes without packet header
    const rawWriter = {
      i8: (value: number) => {
        tempView.setInt8(offset, value);
        offset += 1;
      },
      u8: (value: number) => {
        tempView.setUint8(offset, value);
        offset += 1;
      },
      i16: (value: number) => {
        tempView.setInt16(offset, value, true);
        offset += 2;
      },
      u16: (value: number) => {
        tempView.setUint16(offset, value, true);
        offset += 2;
      },
      i32: (value: number) => {
        tempView.setInt32(offset, value, true);
        offset += 4;
      },
      u32: (value: number) => {
        tempView.setUint32(offset, value, true);
        offset += 4;
      },
      i64: (value: bigint) => {
        tempView.setBigInt64(offset, value, true);
        offset += 8;
      },
      f32: (value: number) => {
        tempView.setFloat32(offset, value, true);
        offset += 4;
      },
      f64: (value: number) => {
        tempView.setFloat64(offset, value, true);
        offset += 8;
      },
      Guid: (uid: string) => {
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
          rawWriter.i8(byte);
        }
      },
    };

    // Collect fields from all classes in the inheritance chain
    const allFields: FieldMetadata[] = [];
    let currentClass = this.constructor;
    while (currentClass) {
      const metadata = fieldMetadata.get(currentClass);
      if (metadata) {
        allFields.push(...metadata);
      }
      currentClass = Object.getPrototypeOf(currentClass);
      if (currentClass === Function.prototype) break;
    }

    if (allFields.length === 0) {
      throw new Error(`No field metadata for ${this.constructor.name}`);
    }

    // Sort by index and serialize fields
    const sorted = allFields.sort((a, b) => a.index - b.index);
    for (const field of sorted) {
      if (!field.skip) {
        this.writeFieldRaw(rawWriter, field.type, (this as any)[field.propertyKey]);
      }
    }

    // Return only the used portion of the buffer
    return tempBuffer.slice(0, offset);
  }

  /**
   * Deserialize from binary and update this component
   */
  fromBuffer(reader: EvPacketReader): void {
    // Collect fields from all classes in the inheritance chain
    const allFields: FieldMetadata[] = [];
    let currentClass = this.constructor;
    while (currentClass) {
      const metadata = fieldMetadata.get(currentClass);
      if (metadata) {
        allFields.push(...metadata);
      }
      currentClass = Object.getPrototypeOf(currentClass);
      if (currentClass === Function.prototype) break;
    }

    if (allFields.length === 0) {
      throw new Error(`No field metadata for ${this.constructor.name}`);
    }

    // Sort by index and deserialize fields
    const sorted = allFields.sort((a, b) => a.index - b.index);
    for (const field of sorted) {
      const value = this.readField(reader, field.type);
      if (!field.skip) {
        (this as any)[field.propertyKey] = value;
      }
    }
  }

  /**
   * Static factory to create component from buffer
   */
  static fromBuffer<T extends Component>(this: new (ntt: NTT, ...args: any[]) => T, ntt: NTT, reader: EvPacketReader): T {
    const instance = new this(ntt);
    instance.fromBuffer(reader);
    return instance;
  }

  protected writeFieldRaw(writer: any, type: FieldType, value: any): void {
    if (value === undefined) throw new Error("Missing value");
    switch (type) {
      case "i8":
        writer.i8(value ?? 0);
        break;
      case "u8":
        writer.u8(value ?? 0);
        break;
      case "i16":
        writer.i16(value ?? 0);
        break;
      case "u16":
        writer.u16(value ?? 0);
        break;
      case "i32":
        writer.i32(value ?? 0);
        break;
      case "u32":
        writer.u32(value ?? 0);
        break;
      case "i64":
        writer.i64(value ?? 0n);
        break;
      case "u64":
        writer.i64(value ?? 0n);
        break;
      case "f32":
        writer.f32(value ?? 0);
        break;
      case "f64":
        writer.f64(value ?? 0);
        break;
      case "bool":
        writer.i8(value ? 1 : 0);
        break;
      case "guid":
        writer.Guid(value ?? "00000000-0000-0000-0000-000000000000");
        break;
      case "vector2":
        writer.f32(value?.x ?? 0);
        writer.f32(value?.y ?? 0);
        break;
      case "string64": {
        const bytes = new TextEncoder().encode(value ?? "");
        const truncated = bytes.slice(0, 64);
        // Write bytes one by one
        for (let i = 0; i < truncated.length; i++) {
          writer.i8(truncated[i]);
        }
        // Pad with zeros to reach 64 bytes
        for (let i = truncated.length; i < 64; i++) {
          writer.i8(0);
        }
        break;
      }
      default:
        throw new Error(`Unknown field type ${type}`);
    }
  }

  protected writeField(writer: EvPacketWriter, type: FieldType, value: any): void {
    if (value === undefined) throw new Error("Missing value");
    switch (type) {
      case "i8":
        writer.i8(value ?? 0);
        break;
      case "u8":
        writer.i8(value ?? 0);
        break; // u8 written as i8 (byte is byte)
      case "i16":
        writer.i16(value ?? 0);
        break;
      case "u16":
        writer.u16(value ?? 0);
        break;
      case "i32":
        writer.i32(value ?? 0);
        break;
      case "u32":
        writer.u32(value ?? 0);
        break;
      case "i64":
        writer.i64(value ?? 0n);
        break;
      case "u64":
        writer.i64(value ?? 0n);
        break; // u64 written as i64
      case "f32":
        writer.f32(value ?? 0);
        break;
      case "f64":
        writer.f64(value);
        break;
      case "bool":
        writer.i8(value ? 1 : 0);
        break;
      case "guid":
        writer.Guid(value ?? "00000000-0000-0000-0000-000000000000");
        break;
      case "vector2":
        writer.f32(value?.x);
        writer.f32(value?.y);
        break;
      case "string64": {
        const bytes = new TextEncoder().encode(value ?? "");
        const truncated = bytes.slice(0, 64);
        // Write bytes one by one
        for (let i = 0; i < truncated.length; i++) {
          writer.i8(truncated[i]);
        }
        // Pad with zeros to reach 64 bytes
        for (let i = truncated.length; i < 64; i++) {
          writer.i8(0);
        }
        break;
      }
      default:
        throw new Error(`Unknown field type ${type}`);
    }
  }

  protected readField(reader: EvPacketReader, type: FieldType): any {
    switch (type) {
      case "i8":
        return reader.i8();
      case "u8":
        return reader.i8() & 0xff; // Read as i8, mask to unsigned
      case "i16":
        return reader.i16();
      case "u16":
        return reader.u16();
      case "i32":
        return reader.i32();
      case "u32":
        return reader.u32();
      case "i64":
        return reader.i64();
      case "u64":
        return reader.i64(); // Read as i64 (JS doesn't distinguish)
      case "f32":
        return reader.f32();
      case "f64":
        return reader.f64();
      case "bool":
        return reader.i8() !== 0;
      case "guid":
        return reader.Guid();
      case "vector2":
        return { x: reader.f32(), y: reader.f32() };
      case "string64": {
        const bytes = new Uint8Array(64);
        for (let i = 0; i < 64; i++) {
          bytes[i] = reader.i8();
        }
        const nullIndex = bytes.indexOf(0);
        const length = nullIndex >= 0 ? nullIndex : 64;
        return new TextDecoder().decode(bytes.subarray(0, length));
      }
    }
  }
}
