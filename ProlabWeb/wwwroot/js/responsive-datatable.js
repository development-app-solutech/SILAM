/**
 * Simplified Responsive Datatable with Mobile Collapse
 */

document.addEventListener('DOMContentLoaded', function() {
    console.log('ResponsiveDatatable initializing...');
    
    const table = document.querySelector('.responsive-table');
    if (!table) {
        console.log('No responsive table found');
        return;
    }
    
    console.log('Table found, setting up collapse functionality');
    initializeCollapseButtons();
    
    function initializeCollapseButtons() {
        const tbody = table.querySelector('tbody');
        const thead = table.querySelector('thead');
        
        if (!tbody || !thead) {
            console.error('Table structure incomplete');
            return;
        }
        
        const rows = tbody.querySelectorAll('tr');
        console.log(`Found ${rows.length} data rows`);
        
        // Add header for toggle column
        const headerRow = thead.querySelector('tr');
        if (headerRow) {
            const toggleHeader = document.createElement('th');
            toggleHeader.className = 'collapse-toggle-header';
            toggleHeader.innerHTML = '<i class="fas fa-ellipsis-v" style="opacity: 0.5;"></i>';
            headerRow.appendChild(toggleHeader);
        }
        
        rows.forEach((row, index) => {
            // Get original cell data
            const cells = row.querySelectorAll('td');
            const rowData = extractRowData(cells);
            
            // Add collapse button cell
            const toggleCell = document.createElement('td');
            toggleCell.className = 'collapse-toggle-cell';
            toggleCell.innerHTML = `
                <button type="button" class="collapse-toggle" data-row="${index}" aria-label="Show details">
                    <i class="fas fa-chevron-down"></i>
                </button>
            `;
            row.appendChild(toggleCell);
            
            // Create expanded content row
            const expandedRow = document.createElement('tr');
            expandedRow.className = 'collapse-row';
            expandedRow.id = `collapse-${index}`;
            expandedRow.style.display = 'none';
            
            const expandedCell = document.createElement('td');
            expandedCell.colSpan = cells.length + 1;
            expandedCell.innerHTML = createExpandedContent(rowData);
            
            expandedRow.appendChild(expandedCell);
            
            // Insert the expanded row after current row
            row.parentNode.insertBefore(expandedRow, row.nextSibling);
        });
        
        // Add click handlers
        table.addEventListener('click', function(e) {
            const button = e.target.closest('.collapse-toggle');
            if (!button) return;
            
            e.preventDefault();
            
            const rowIndex = button.getAttribute('data-row');
            const expandedRow = document.getElementById(`collapse-${rowIndex}`);
            const icon = button.querySelector('i');
            
            if (expandedRow.style.display === 'none') {
                // Expand
                expandedRow.style.display = 'table-row';
                icon.className = 'fas fa-chevron-up';
                button.setAttribute('aria-label', 'Hide details');
                button.closest('tr').classList.add('row-expanded');
            } else {
                // Collapse
                expandedRow.style.display = 'none';
                icon.className = 'fas fa-chevron-down';
                button.setAttribute('aria-label', 'Show details');
                button.closest('tr').classList.remove('row-expanded');
            }
        });
        
        console.log('Collapse functionality initialized');
    }
    
    function extractRowData(cells) {
        const data = {};
        
        if (cells.length >= 1) data.numero = cells[0].textContent.trim();
        if (cells.length >= 2) data.date = cells[1].textContent.trim();
        if (cells.length >= 3) {
            const patientCell = cells[2];
            const patientName = patientCell.querySelector('.patient-name');
            const patientId = patientCell.querySelector('.patient-id');
            data.patient = patientName ? patientName.textContent.trim() : patientCell.textContent.trim();
            data.patientId = patientId ? patientId.textContent.trim() : '';
        }
        if (cells.length >= 4) data.prescripteur = cells[3].textContent.trim();
        
        return data;
    }
    
    function createExpandedContent(data) {
        let html = '<div class="collapse-content">';
        
        if (data.numero) {
            html += `
                <div class="detail-row">
                    <span class="detail-label">Numéro:</span>
                    <span class="detail-value">${data.numero}</span>
                </div>
            `;
        }
        
        if (data.date) {
            html += `
                <div class="detail-row">
                    <span class="detail-label">Date:</span>
                    <span class="detail-value">${data.date}</span>
                </div>
            `;
        }
        
        if (data.patient) {
            html += `
                <div class="detail-row">
                    <span class="detail-label">Patient:</span>
                    <span class="detail-value">${data.patient}${data.patientId ? '<br><small>' + data.patientId + '</small>' : ''}</span>
                </div>
            `;
        }
        
        if (data.prescripteur) {
            html += `
                <div class="detail-row">
                    <span class="detail-label">Prescripteur:</span>
                    <span class="detail-value">${data.prescripteur}</span>
                </div>
            `;
        }
        
        html += '</div>';
        return html;
    }
    
    // Statistics cards interaction
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
                
                // Scroll to table
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
    
    console.log('ResponsiveDatatable initialized successfully');
});
