/**
 * Standalone Status Filter for Entetedemande Table
 * This script works independently of DataTables to avoid initialization conflicts
 */

(function() {
    'use strict';

    console.log('Loading standalone status filter...');

    // Wait for DOM to be ready
    function waitForElement(selector, callback, maxAttempts = 50) {
        let attempts = 0;
        
        function check() {
            attempts++;
            const element = document.querySelector(selector);
            
            if (element) {
                console.log(`Found element ${selector} after ${attempts} attempts`);
                callback(element);
            } else if (attempts < maxAttempts) {
                console.log(`Attempt ${attempts}: Element ${selector} not found, retrying...`);
                setTimeout(check, 100);
            } else {
                console.error(`Element ${selector} not found after ${maxAttempts} attempts`);
            }
        }
        
        check();
    }

    // Initialize the status filter
    function initializeStatusFilter() {
        console.log('Initializing standalone status filter...');

        // Wait for both the filter dropdown and the table to be ready
        waitForElement('[data-kt-entetedemande-table-filter="status"]', function(statusFilter) {
            waitForElement('#kt_table_entetedemande_index tbody', function(tableBody) {
                setupStatusFilter(statusFilter, tableBody);
            });
        });
    }

    function setupStatusFilter(statusFilter, tableBody) {
        console.log('Setting up status filter with elements:', { statusFilter, tableBody });
        
        // Add change event listener
        statusFilter.addEventListener('change', function(e) {
            const selectedStatus = e.target.value;
            console.log('🔍 Status filter changed to:', selectedStatus);
            
            // Get all table rows
            const allRows = tableBody.querySelectorAll('tr');
            console.log('📊 Total rows found:', allRows.length);
            
            if (selectedStatus === '') {
                console.log('✅ Showing all rows');
                // Show all rows
                allRows.forEach((row, index) => {
                    row.style.display = '';
                    console.log(`Row ${index}: Shown`);
                });
            } else {
                console.log('🎯 Filtering by status:', selectedStatus);
                let visibleCount = 0;
                
                allRows.forEach((row, index) => {
                    // Status column is index 9 (0-based)
                    const statusCell = row.cells[9];
                    
                    if (statusCell) {
                        const statusText = statusCell.textContent.trim();
                        const matches = statusText === selectedStatus;
                        
                        console.log(`Row ${index}: "${statusText}" === "${selectedStatus}"? ${matches}`);
                        
                        if (matches) {
                            row.style.display = '';
                            visibleCount++;
                        } else {
                            row.style.display = 'none';
                        }
                    } else {
                        console.log(`Row ${index}: No status cell found, hiding`);
                        row.style.display = 'none';
                    }
                });
                
                console.log(`✨ Filter complete: ${visibleCount} rows visible out of ${allRows.length} total`);
                
                // Update any pagination info
                updatePaginationInfo(visibleCount, allRows.length);
            }
        });
        
        // Test initial state
        console.log('🧪 Testing initial filter state...');
        console.log('Current filter value:', statusFilter.value);
        console.log('Available options:');
        const options = statusFilter.querySelectorAll('option');
        options.forEach((option, index) => {
            console.log(`  ${index}: "${option.value}" - "${option.textContent}"`);
        });
        
        // Show sample of status column data
        console.log('📋 Sample status column data:');
        const sampleRows = tableBody.querySelectorAll('tr');
        for (let i = 0; i < Math.min(5, sampleRows.length); i++) {
            const statusCell = sampleRows[i].cells[9];
            if (statusCell) {
                console.log(`  Row ${i}: "${statusCell.textContent.trim()}"`);
            }
        }
        
        console.log('🎉 Status filter setup complete!');
    }

    function updatePaginationInfo(visibleCount, totalCount) {
        // Try to find and update pagination info
        const infoSelectors = [
            '.dataTables_info',
            '[class*="info"]',
            '.kt-pagination-info',
            '.pagination-info'
        ];
        
        for (const selector of infoSelectors) {
            const infoElement = document.querySelector(selector);
            if (infoElement && infoElement.textContent.includes('entries')) {
                infoElement.textContent = `Showing ${visibleCount} entries (filtered from ${totalCount} total entries)`;
                console.log('📄 Updated pagination info:', infoElement.textContent);
                break;
            }
        }
    }

    // Start initialization when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeStatusFilter);
    } else {
        // DOM is already ready
        initializeStatusFilter();
    }

    // Also try after a delay to handle dynamic content
    setTimeout(initializeStatusFilter, 1000);
    setTimeout(initializeStatusFilter, 3000);

    console.log('✅ Standalone status filter script loaded');

})();
