// <meter-panel> Web Component
// Pure CSS grid container, no child-moving logic

class MeterPanel extends HTMLElement {
    static get observedAttributes() {
        return ['layout'];
    }
    connectedCallback() {
        this.updateLayout();
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
        this.style.display = 'grid';
        this.style.gap = '1rem';
        this.style.width = '100%';
        this.style.height = '100%';
        this.style.gridTemplateRows = `repeat(${rows}, 1fr)`;
        this.style.gridTemplateColumns = `repeat(${cols}, 1fr)`;
    }
}

customElements.define('meter-panel', MeterPanel);
