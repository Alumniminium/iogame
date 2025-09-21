export class PacketHeader {
  length: number;
  id: number;

  constructor(length: number, id: number) {
    this.length = length;
    this.id = id;
  }
}
