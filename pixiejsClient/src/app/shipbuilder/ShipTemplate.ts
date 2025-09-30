export interface ShipTemplatePart {
  gridX: number;
  gridY: number;
  type: "hull" | "shield" | "engine";
  shape: "triangle" | "square";
}

export interface ShipTemplate {
  name: string;
  parts: ShipTemplatePart[];
}
