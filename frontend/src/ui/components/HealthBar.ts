export class HealthBar {
  private element: HTMLElement;

  constructor(entityId: number) {
    this.element = document.createElement('div');
    this.element.id = `health-ui-${entityId}`;
    this.element.className = 'health-ui';
    this.element.innerHTML = `
      <div class="health-container">
        <div class="health-label">Health</div>
        <div class="health-bar">
          <div id="health-${entityId}" class="health-fill"></div>
        </div>
        <div id="health-text-${entityId}" class="health-text">100/100</div>
      </div>
    `;
  }

  getElement(): HTMLElement {
    return this.element;
  }
}