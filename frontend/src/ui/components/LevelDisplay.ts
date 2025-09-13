export class LevelDisplay {
  private element: HTMLElement;

  constructor(entityId: number) {
    this.element = document.createElement('div');
    this.element.id = `level-ui-${entityId}`;
    this.element.className = 'level-ui';
    this.element.innerHTML = `
      <div class="level-container">
        <div id="level-${entityId}" class="level-text">Level 1</div>
        <div class="exp-container">
          <div class="exp-label">EXP</div>
          <div class="exp-bar">
            <div id="exp-${entityId}" class="exp-fill"></div>
          </div>
          <div id="exp-text-${entityId}" class="exp-text">0/100</div>
        </div>
      </div>
    `;
  }

  getElement(): HTMLElement {
    return this.element;
  }
}