/**
 * Medical Results Responsive Datatable
 * Handles mobile collapse functionality and responsive behavior
 */

class MedicalResultsDatatable {
    constructor(tableSelector = '.responsive-table') {
        this.table = document.querySelector(tableSelector);
        this.isMobile = window.innerWidth <= 768;
        console.log('Initializing MedicalResultsDatatable', {
            table: this.table,
            isMobile: this.isMobile,
            windowWidth: window.innerWidth
        });
        
        if (this.table) {
            this.init();
        } else {
            console.error('Table not found with selector:', tableSelector);
        }
    }

    init() {
        console.log('Init called');
        this.addCollapseToggleButtons();
        this.bindEvents();
        this.handleResize();
    }

    addCollapseToggleButtons() {
        const tbody = this.table.querySelector('tbody');
        const rows = tbody.querySelectorAll('tr');

        rows.forEach((row, index) => {
            // Create toggle button
            const toggleCell = document.createElement('td');
            toggleCell.className = 'collapse-toggle-cell';
            
            const toggleButton = document.createElement('button');
            toggleButton.className = 'collapse-toggle';
            toggleButton.setAttribute('aria-label', 'Afficher plus d\'informations');
            toggleButton.setAttribute('data-row-index', index);
            toggleButton.innerHTML = '<i class="fas fa-chevron-down"></i>';
            
            toggleCell.appendChild(toggleButton);
            row.appendChild(toggleCell);

            // Create collapse content
            this.createCollapseContent(row, index);
        });

        // Add header for toggle column
        const thead = this.table.querySelector('thead tr');
        const headerToggleCell = document.createElement('th');
        headerToggleCell.className = 'collapse-toggle-header';
        headerToggleCell.innerHTML = '<span class="sr-only">Actions</span>';
        thead.appendChild(headerToggleCell);
    }

    createCollapseContent(row, index) {
        const cells = row.querySelectorAll('td');
        const data = this.extractRowData(cells);
        
        // Create collapse content row
        const collapseRow = document.createElement('tr');
        collapseRow.className = 'collapse-content-row';
        collapseRow.style.display = 'none';
        
        const collapseCell = document.createElement('td');
        collapseCell.setAttribute('colspan', cells.length + 1);
        
        const collapseContent = document.createElement('div');
        collapseContent.className = 'collapse-content slide-down';
        
        // Build content HTML
        let contentHTML = '';
        if (data.patient && data.patientId) {
            contentHTML += `
                <div class="detail-row">
                    <span class="detail-label">Patient Complet:</span>
                    <span class="detail-value">${data.patient}<br><small>${data.patientId}</small></span>
                </div>
            `;
        }
        if (data.prescripteur) {
            contentHTML += `
                <div class="detail-row">
                    <span class="detail-label">Prescripteur:</span>
                    <span class="detail-value">${data.prescripteur}</span>
                </div>
            `;
        }
        if (data.date) {
            contentHTML += `
                <div class="detail-row">
                    <span class="detail-label">Date Complète:</span>
                    <span class="detail-value">${data.date}</span>
                </div>
            `;
        }
        if (data.numeroCommande) {
            contentHTML += `
                <div class="detail-row">
                    <span class="detail-label">N° Commande:</span>
                    <span class="detail-value">${data.numeroCommande}</span>
                </div>
            `;
        }
        
        collapseContent.innerHTML = contentHTML;
        collapseCell.appendChild(collapseContent);
        collapseRow.appendChild(collapseCell);
        
        // Insert after current row
        row.parentNode.insertBefore(collapseRow, row.nextSibling);
        
        // Store reference
        row.setAttribute('data-collapse-target', `collapse-${index}`);
        collapseRow.setAttribute('id', `collapse-${index}`);
    }

    extractRowData(cells) {
        const data = {};
        
        // Extract data based on column positions
        if (cells[0]) data.numeroCommande = cells[0].textContent.trim();
        if (cells[1]) data.date = cells[1].textContent.trim();
        if (cells[2]) {
            const patientCell = cells[2];
            const patientName = patientCell.querySelector('.patient-name');
            const patientId = patientCell.querySelector('.patient-id');
            data.patient = patientName ? patientName.textContent.trim() : patientCell.textContent.trim();
            data.patientId = patientId ? patientId.textContent.trim() : '';
        }
        if (cells[3]) data.prescripteur = cells[3].textContent.trim();
        
        return data;
    }

    bindEvents() {
        // Toggle collapse on button click
        this.table.addEventListener('click', (e) => {
            if (e.target.closest('.collapse-toggle')) {
                e.preventDefault();
                const button = e.target.closest('.collapse-toggle');
                const rowIndex = button.getAttribute('data-row-index');
                this.toggleRow(rowIndex, button);
            }
        });

        // Handle window resize
        window.addEventListener('resize', this.debounce(() => {
            this.handleResize();
        }, 250));

        // Handle keyboard navigation
        this.table.addEventListener('keydown', (e) => {
            if (e.target.closest('.collapse-toggle') && (e.key === 'Enter' || e.key === ' ')) {
                e.preventDefault();
                e.target.click();
            }
        });
    }

    toggleRow(rowIndex, button) {
        const collapseTarget = document.getElementById(`collapse-${rowIndex}`);
        const row = document.querySelector(`[data-collapse-target="collapse-${rowIndex}"]`);
        const icon = button.querySelector('i');
        
        if (collapseTarget.style.display === 'none' || !collapseTarget.style.display) {
            // Show
            collapseTarget.style.display = 'table-row';
            row.classList.add('row-expanded');
            icon.className = 'fas fa-chevron-up';
            button.setAttribute('aria-label', 'Masquer les informations');
            
            // Add animation class
            const content = collapseTarget.querySelector('.collapse-content');
            content.classList.add('slide-down');
            
        } else {
            // Hide
            collapseTarget.style.display = 'none';
            row.classList.remove('row-expanded');
            icon.className = 'fas fa-chevron-down';
            button.setAttribute('aria-label', 'Afficher plus d\'informations');
        }
    }

    handleResize() {
        const wasMobile = this.isMobile;
        this.isMobile = window.innerWidth <= 768;
        
        if (wasMobile !== this.isMobile) {
            // Reset all expanded rows when switching between mobile/desktop
            this.collapseAllRows();
        }
    }

    collapseAllRows() {
        const expandedRows = this.table.querySelectorAll('.row-expanded');
        expandedRows.forEach(row => {
            const rowIndex = row.getAttribute('data-collapse-target').replace('collapse-', '');
            const button = this.table.querySelector(`[data-row-index="${rowIndex}"]`);
            if (button) {
                this.toggleRow(rowIndex, button);
            }
        });
    }

    // Utility function for debouncing resize events
    debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

    // Public method to refresh the datatable
    refresh() {
        this.collapseAllRows();
        this.init();
    }

    // Public method to expand all rows
    expandAll() {
        const buttons = this.table.querySelectorAll('.collapse-toggle');
        buttons.forEach((button, index) => {
            const collapseTarget = document.getElementById(`collapse-${index}`);
            if (collapseTarget.style.display === 'none' || !collapseTarget.style.display) {
                this.toggleRow(index, button);
            }
        });
    }

    // Public method to collapse all rows
    collapseAll() {
        const buttons = this.table.querySelectorAll('.collapse-toggle');
        buttons.forEach((button, index) => {
            const collapseTarget = document.getElementById(`collapse-${index}`);
            if (collapseTarget.style.display !== 'none') {
                this.toggleRow(index, button);
            }
        });
    }
}

// Auto-initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    // Check if the medical results table exists
    const medicalTable = document.querySelector('.responsive-table');
    if (medicalTable) {
        window.medicalResultsDatatable = new MedicalResultsDatatable();
    }
});

// Statistics Cards Interaction
document.addEventListener('DOMContentLoaded', function() {
    const statCards = document.querySelectorAll('.stat-card');
    
    statCards.forEach(card => {
        const button = card.querySelector('.stat-card-button');
        if (button) {
            button.addEventListener('click', function(e) {
                e.preventDefault();
                
                // Add click animation
                card.style.transform = 'scale(0.98)';
                setTimeout(() => {
                    card.style.transform = '';
                }, 150);
                
                // You can add custom logic here for each stat card
                const cardType = card.classList.contains('warning') ? 'warning' : 
                               card.classList.contains('success') ? 'success' : 'info';
                
                console.log(`Clicked on ${cardType} stat card`);
                
                // Example: scroll to results table
                const resultsContainer = document.querySelector('.results-container');
                if (resultsContainer) {
                    resultsContainer.scrollIntoView({ 
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            });
        }
    });
});

// Export for module usage
if (typeof module !== 'undefined' && module.exports) {
    module.exports = MedicalResultsDatatable;
}
