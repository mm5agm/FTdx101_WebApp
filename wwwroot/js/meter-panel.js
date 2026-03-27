// <meter-panel> Web Component
// Uses CSS grid to lay out child <meter-gauge> elements according to layout attribute (e.g., "2x2")

class MeterPanel extends HTMLElement {
    static get observedAttributes() {
        return ['layout'];
    }
    constructor() {
        super();
        this.attachShadow({ mode: 'open' });
        this.grid = document.createElement('div');
        this.grid.className = 'meter-panel-grid';
        const style = document.createElement('style');
        style.textContent = `
            .meter-panel-grid {
                display: grid;
                gap: 1rem;
                width: 100%;
                height: 100%;
            }
        `;
        this.shadowRoot.append(style, this.grid);
    }
    connectedCallback() {
        this.updateLayout();
        this.moveChildren();
    }
    attributeChangedCallback(name, oldValue, newValue) {
        if (name === 'layout' && oldValue !== newValue) {
            this.updateLayout();
        }
    }
    updateLayout() {
        const layout = (this.getAttribute('layout') || '2x2').split('x');
        const rows = parseInt(layout[0], 10) || 2;
        const cols = parseInt(layout[1], 10) || 2;
        this.grid.style.gridTemplateRows = `repeat(${rows}, 1fr)`;
        this.grid.style.gridTemplateColumns = `repeat(${cols}, 1fr)`;
    }
    moveChildren() {
        // Move all <meter-gauge> children into the grid
        Array.from(this.children).forEach(child => {
            if (child.tagName.toLowerCase() === 'meter-gauge') {
                this.grid.appendChild(child);
            }
        });
    }
}

customElements.define('meter-panel', MeterPanel);
