import { Component } from "../core/Component";

export class ParentChildComponent extends Component {
  constructor(
    entityId: string,
    public parentId: string,
  ) {
    super(entityId);
  }
}
