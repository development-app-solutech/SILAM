/**
 * Simple Responsive Datatable
 * Works without complex frameworks - just vanilla JS
 */

document.addEventListener('DOMContentLoaded', function() {
    console.log('Simple Responsive Table: Initializing...');
    
    // Find the table
    const table = document.querySelector('.responsive-table');
    if (!table) {
        console.log('No responsive table found');
        return;
    }
    
    console.log('Table found, setting up mobile functionality');
    
    // Add toggle buttons to each row
    setupMobileToggles();
    
    function setupMobileToggles() {
        const tbody = table.querySelector('tbody');
        const thead = table.querySelector('thead tr');
        
        if (!tbody || !thead) {
            console.error('Table structure is incomplete');
            return;
        }
        
        // Add header for toggle column
        const toggleHeader = document.createElement('th');
        toggleHeader.innerHTML = '<span style="opacity: 0.5;">⋮</span>';
        toggleHeader.style.width = '40px';
        toggleHeader.style.textAlign = 'center';
        thead.appendChild(toggleHeader);
        
        // Process each data row
        const rows = tbody.querySelectorAll('tr');
        console.log(`Processing ${rows.length} table rows`);
        
        rows.forEach((row, index) => {
            // Get the row data
            const cells = row.querySelectorAll('td');
            const rowData = extractRowData(cells);
            
            // Add toggle button cell
            const toggleCell = document.createElement('td');
            toggleCell.style.textAlign = 'center';
            toggleCell.style.verticalAlign = 'middle';
            toggleCell.innerHTML = `
                <button class="mobile-toggle" data-row="${index}" aria-label="Toggle details">
                    <span class="toggle-icon">▼</span>
                </button>
            `;
            row.appendChild(toggleCell);
            
            // Create mobile details div
            const detailsDiv = document.createElement('div');
            detailsDiv.className = 'mobile-details';
            detailsDiv.id = `details-${index}`;
            detailsDiv.innerHTML = createMobileDetails(rowData);
            
            // Insert details div after the table row
            row.parentNode.insertBefore(createDetailsRow(detailsDiv, cells.length + 1), row.nextSibling);
        });
        
        // Add click event listeners
        table.addEventListener('click', handleToggleClick);
        
        console.log('Mobile toggle setup complete');
    }
    
    function createDetailsRow(detailsDiv, colspan) {
        const detailsRow = document.createElement('tr');
        detailsRow.className = 'mobile-details-row';
        detailsRow.style.display = 'none';
        
        const detailsCell = document.createElement('td');
        detailsCell.colSpan = colspan;
        detailsCell.style.padding = '0';
        detailsCell.appendChild(detailsDiv);
        
        detailsRow.appendChild(detailsCell);
        return detailsRow;
    }
    
    function extractRowData(cells) {
        const data = {};
        
        if (cells[0]) data.numero = cells[0].textContent.trim();
        if (cells[1]) data.date = cells[1].textContent.trim();
        if (cells[2]) {
            const patientName = cells[2].querySelector('.patient-name');
            const patientId = cells[2].querySelector('.patient-id');
            data.patient = patientName ? patientName.textContent.trim() : cells[2].textContent.trim();
            data.patientId = patientId ? patientId.textContent.trim() : '';
        }
        if (cells[3]) data.prescripteur = cells[3].textContent.trim();
        
        return data;
    }
    
    function createMobileDetails(data) {
        let html = '';
        
        if (data.numero) {
            html += `
                <div class="detail-item">
                    <span class="detail-label">Numéro:</span>
                    <span class="detail-value">${data.numero}</span>
                </div>
            `;
        }
        
        if (data.date) {
            html += `
                <div class="detail-item">
                    <span class="detail-label">Date:</span>
                    <span class="detail-value">${data.date}</span>
                </div>
            `;
        }
        
        if (data.patient) {
            html += `
                <div class="detail-item">
                    <span class="detail-label">Patient:</span>
                    <span class="detail-value">${data.patient}${data.patientId ? '<br><small>' + data.patientId + '</small>' : ''}</span>
                </div>
            `;
        }
        
        if (data.prescripteur) {
            html += `
                <div class="detail-item">
                    <span class="detail-label">Prescripteur:</span>
                    <span class="detail-value">${data.prescripteur}</span>
                </div>
            `;
        }
        
        return html;
    }
    
    function handleToggleClick(event) {
        const button = event.target.closest('.mobile-toggle');
        if (!button) return;
        
        event.preventDefault();
        
        const rowIndex = button.getAttribute('data-row');
        const detailsRow = button.closest('tr').nextElementSibling;
        const icon = button.querySelector('.toggle-icon');
        
        if (detailsRow && detailsRow.classList.contains('mobile-details-row')) {
            if (detailsRow.style.display === 'none') {
                // Show details
                detailsRow.style.display = 'table-row';
                icon.textContent = '▲';
                button.setAttribute('aria-label', 'Hide details');
            } else {
                // Hide details
                detailsRow.style.display = 'none';
                icon.textContent = '▼';
                button.setAttribute('aria-label', 'Toggle details');
            }
        }
    }
    
    // Handle statistics cards
    const statCards = document.querySelectorAll('.stat-card');
    statCards.forEach(card => {
        const button = card.querySelector('.stat-button');
        if (button) {
            button.addEventListener('click', function(e) {
                e.preventDefault();
                
                // Simple scroll to table
                const tableContainer = document.querySelector('.table-container');
                if (tableContainer) {
                    tableContainer.scrollIntoView({ 
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            });
        }
    });
    
    console.log('Simple Responsive Table: Initialization complete');
});
