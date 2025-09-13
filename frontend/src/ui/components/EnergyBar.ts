export class EnergyBar {
  private element: HTMLElement;

  constructor(entityId: number) {
    this.element = document.createElement('div');
    this.element.id = `energy-ui-${entityId}`;
    this.element.className = 'energy-ui';
    this.element.innerHTML = `
      <div class="energy-container">
        <div class="energy-label">Energy</div>
        <div class="energy-bar">
          <div id="energy-${entityId}" class="energy-fill"></div>
        </div>
        <div id="energy-text-${entityId}" class="energy-text">1000/1000</div>
      </div>
    `;
  }

  getElement(): HTMLElement {
    return this.element;
  }
}