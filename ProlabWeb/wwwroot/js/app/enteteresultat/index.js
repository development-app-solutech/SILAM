"use strict";

var KTEnteteresultatList = function () {
    var table;
    var datatable;
    var originalRowCount = 0;

    var initEnteteresultatTable = function () {
        try {
            if (!table) {
                console.error('Table element not found for #enteteresultat-table');
                return;
            }
            datatable = $(table).DataTable({
                ordering: true,
                searching: true,
                scrollX: true,
                order: [[1, 'desc']], // Order by date descending
                pageLength: 10,
                responsive: true,
                language: {
                    "emptyTable": "Aucun résultat disponible",
                    "info": "Affichage de _START_ à _END_ sur _TOTAL_ résultats",
                    "infoEmpty": "Affichage de 0 à 0 sur 0 résultat",
                    "infoFiltered": "(filtré à partir de _MAX_ résultats totaux)",
                    "lengthMenu": "Afficher _MENU_ résultats par page",
                    "loadingRecords": "Chargement...",
                    "processing": "Traitement...",
                    "search": "Rechercher:",
                    "zeroRecords": "Aucun résultat trouvé",
                    "paginate": {
                        "first": "Premier",
                        "last": "Dernier",
                        "next": "Suivant",
                        "previous": "Précédent"
                    }
                },
                columnDefs: [
                    {
                        targets: [0], // checkbox column
                        orderable: false,
                        className: 'text-center'
                    },
                    {
                        targets: [8], // actions column
                        orderable: false,
                        className: 'text-center'
                    }
                ],
                drawCallback: function() {
                    try { updateVisibleCount(); } catch (e) { console.warn('updateVisibleCount failed', e); }
                    try { updatePaginationInfo(); } catch (e) { console.warn('updatePaginationInfo failed', e); }
                }
            });
            
            originalRowCount = datatable ? datatable.rows().count() : 0;
    
            // Cacher la barre de recherche native
            $('.dt-search').css('display', 'none');
    
            // Re-init functions on every table re-draw -- more info: https://datatables.net/reference/event/draw
            if (datatable) {
                datatable.on('draw', function () {
                    handleCheckboxes();
                    handlePrintMenu();
                    if (typeof KTMenu !== 'undefined' && KTMenu.init) { KTMenu.init(); }
                });
            }
        } catch (err) {
            console.error('Failed to initialize DataTable:', err);
        }
    };

    var handleSearchDatatable = function () {
        const filterSearch = document.querySelector('input[data-kt-enteteresultat-table-filter="search"]');
        if (filterSearch) {
            let searchTimeout;
            filterSearch.addEventListener('input', function (e) {
                clearTimeout(searchTimeout);
                const searchValue = e.target.value;
                
                // Show loading state
                if (searchValue.length > 0) {
                    showTableLoading();
                }
                
                searchTimeout = setTimeout(() => {
                    if (datatable) {
                        datatable.search(searchValue).draw();
                    }
                    hideTableLoading();
                }, 300); // Debounce search
            });
        }
    };
    
    var handleAdvancedFilters = function() {
        // Date range filtering
        const dateFrom = document.getElementById('dateFrom');
        const dateTo = document.getElementById('dateTo');
        const statusFilter = document.getElementById('statusFilter');
        
        if (dateFrom && dateTo) {
            dateFrom.addEventListener('change', applyFilters);
            dateTo.addEventListener('change', applyFilters);
        }
        
        if (statusFilter) {
            statusFilter.addEventListener('change', applyFilters);
        }
        
        // Clear filters button
        const clearFiltersBtn = document.getElementById('clearFilters');
        if (clearFiltersBtn) {
            clearFiltersBtn.addEventListener('click', function() {
                // Reset all filters
                if (dateFrom) dateFrom.value = '';
                if (dateTo) dateTo.value = '';
                if (statusFilter) statusFilter.value = '';
                const searchInput = document.querySelector('input[data-kt-enteteresultat-table-filter="search"]');
                if (searchInput) searchInput.value = '';
                
                // Clear DataTable search and redraw
                if (datatable) {
                    datatable.search('').draw();
                }
                applyFilters();
            });
        }
    };
    
    var applyFilters = function() {
        showTableLoading();
        
        setTimeout(() => {
            const dateFrom = document.getElementById('dateFrom')?.value;
            const dateTo = document.getElementById('dateTo')?.value;
            const status = document.getElementById('statusFilter')?.value;
            
            // Apply custom filtering
            $.fn.dataTable.ext.search.push(
                function(settings, data, dataIndex) {
                    // Date filtering (assuming date is in column 1)
                    if (dateFrom || dateTo) {
                        const rowDate = new Date(data[1].split('\n')[0]); // Get date part from formatted date
                        
                        if (dateFrom && rowDate < new Date(dateFrom)) return false;
                        if (dateTo && rowDate > new Date(dateTo)) return false;
                    }
                    
                    // Status filtering (assuming status is in column 7)
                    if (status) {
                        const statusText = data[7];
                        if (status === 'validated' && !statusText.includes('Validé')) return false;
                        if (status === 'pending' && !statusText.includes('En attente')) return false;
                    }
                    
                    return true;
                }
            );
            
            if (datatable) {
                datatable.draw();
            }
            
            // Clear the custom filter
            $.fn.dataTable.ext.search.pop();
            
            hideTableLoading();
        }, 200);
    };

    var handleCheckboxes = function () {
        // Enhanced select all functionality
        $('#selectAllCheckbox').on('change', function () {
            var checked = $(this).is(':checked');
            $('.row-checkbox:visible').prop('checked', checked);
            updateSelectedCount();
            toggleBulkActions();
        });
        
        // Enhanced individual checkbox handling
        $(document).on('change', '.row-checkbox', function () {
            updateSelectedCount();
            toggleBulkActions();
            
            const totalVisible = $('.row-checkbox:visible').length;
            const checkedVisible = $('.row-checkbox:visible:checked').length;
            
            if (checkedVisible === 0) {
                $('#selectAllCheckbox').prop('checked', false).prop('indeterminate', false);
            } else if (checkedVisible === totalVisible) {
                $('#selectAllCheckbox').prop('checked', true).prop('indeterminate', false);
            } else {
                $('#selectAllCheckbox').prop('checked', false).prop('indeterminate', true);
            }
        });
    };
    
    var updateSelectedCount = function() {
        const selectedCount = $('.row-checkbox:checked').length;
        document.getElementById('selectedCount').textContent = selectedCount;
    };
    
    var toggleBulkActions = function() {
        const selectedCount = $('.row-checkbox:checked').length;
        const bulkActions = document.getElementById('bulkActions');
        
        if (bulkActions) {
            if (selectedCount > 0) {
                bulkActions.classList.remove('hidden');
                bulkActions.classList.add('flex');
            } else {
                bulkActions.classList.add('hidden');
                bulkActions.classList.remove('flex');
            }
        }
    };
    
    var updateVisibleCount = function() {
        if (!datatable) return;
        
        const visibleCount = datatable.rows({search: 'applied'}).count();
        const visibleCountElement = document.getElementById('visibleCount');
        if (visibleCountElement) {
            visibleCountElement.textContent = visibleCount;
        }
        
        // Show/hide empty state
        const emptyState = document.getElementById('emptyState');
        const tableContainer = document.querySelector('.kt-scrollable-x-auto');
        
        if (visibleCount === 0 && emptyState && tableContainer) {
            emptyState.classList.remove('hidden');
            tableContainer.style.display = 'none';
        } else if (emptyState && tableContainer) {
            emptyState.classList.add('hidden');
            tableContainer.style.display = 'block';
        }
    };
    
    var updatePaginationInfo = function() {
        if (!datatable) return;
        
        const info = datatable.page.info();
        const startRecord = document.getElementById('startRecord');
        const endRecord = document.getElementById('endRecord');
        const totalRecords = document.getElementById('totalRecords');
        
        if (startRecord && info) startRecord.textContent = info.start + 1;
        if (endRecord && info) endRecord.textContent = info.end;
        if (totalRecords && info) totalRecords.textContent = info.recordsTotal;
    };
    
    var showTableLoading = function() {
        const loading = document.getElementById('table-loading');
        if (loading) {
            loading.classList.remove('hidden');
            loading.style.display = 'block';
            loading.style.pointerEvents = 'auto';
        }
    };
    
    var hideTableLoading = function() {
        const loading = document.getElementById('table-loading');
        if (loading) {
            loading.classList.add('hidden');
            loading.style.display = 'none';
            loading.style.pointerEvents = 'none';
        }
    };
    
    var handleEditNavigation = function() {
        // Prevent edit navigation from being interfered by loading states
        $(document).on('click', 'a[href*="/Edit/"]', function(e) {
            // Ensure no loading states are active that could block navigation
            hideTableLoading();
            hidePrintSpinner();
            
            // Allow normal navigation to proceed
            return true;
        });
        
        // Also handle Edit links in dropdown menus
        $(document).on('click', 'a:contains("Modifier")', function(e) {
            // Ensure no loading states are active that could block navigation
            hideTableLoading();
            hidePrintSpinner();
            
            // Allow normal navigation to proceed
            return true;
        });
    };
    
    var handlePageSizeChange = function() {
        const pageSizeSelect = document.getElementById('pageSize');
        if (pageSizeSelect) {
            pageSizeSelect.addEventListener('change', function() {
                const pageSize = parseInt(this.value);
                if (datatable) {
                    datatable.page.len(pageSize).draw();
                }
            });
        }
    };

    var handleDeleteRows = function () {
        const deleteButtons = document.querySelectorAll('[data-kt-enteteresultat-table-filter="delete_row"]');
        deleteButtons.forEach(d => {
            d.addEventListener('click', function (e) {
                e.preventDefault();
                const parent = e.target.closest('tr');
                const label = parent.querySelectorAll('td')[2]?.innerText || '';
                Swal.fire({
                    text: `Voulez-vous vraiment supprimer le résultat n° ${label} ?`,
                    icon: "warning",
                    showCancelButton: true,
                    buttonsStyling: false,
                    confirmButtonText: "Oui, supprimer !",
                    cancelButtonText: "Non, annuler",
                    customClass: {
                        confirmButton: "kt-btn fw-bold kt-btn-destructive",
                        cancelButton: "kt-btn fw-bold kt-btn-active-light-primary"
                    }
                }).then(function (result) {
                    if (result.value) {
                        let id = $(d).data('id');
                        $.ajax({
                            url: `/Enteteresultat/DeleteEnteteresultat?id=${id}`,
                            type: 'GET',
                            success: function (response) {
                                if (response.success) {
                                    Swal.fire({
                                        text: "Suppression réussie !",
                                        icon: "success",
                                        buttonsStyling: false,
                                        confirmButtonText: "Ok, compris !",
                                        customClass: {
                                            confirmButton: "kt-btn fw-bold kt-btn-primary",
                                        }
                                    }).then(function () {
                                        if (datatable) {
                                            datatable.row($(parent)).remove().draw();
                                        }
                                    });
                                } else {
                                    Swal.fire({
                                        text: response.message || "Erreur lors de la suppression.",
                                        icon: "error",
                                        buttonsStyling: false,
                                        confirmButtonText: "Ok, compris !",
                                        customClass: {
                                            confirmButton: "kt-btn fw-bold kt-btn-primary",
                                        }
                                    });
                                }
                            },
                            error: function () {
                                Swal.fire({
                                    text: "Erreur lors de la suppression.",
                                    icon: "error",
                                    buttonsStyling: false,
                                    confirmButtonText: "Ok, compris !",
                                    customClass: {
                                        confirmButton: "kt-btn fw-bold kt-btn-primary",
                                    }
                                });
                            }
                        });
                    }
                });
            });
        });
    };

    var base64ToUint8Array = (base64) => {
        const binaryString = atob(base64);
        const len = binaryString.length;
        const bytes = new Uint8Array(len);
        for (let i = 0; i < len; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }
        return bytes;
    }

    var pdfBase64ToPngArray = async (base64Pdf) => {
        const pdfData = base64ToUint8Array(base64Pdf);
        const pdf = await pdfjsLib.getDocument({ data: pdfData }).promise;

        const result = [];

        for (let i = 1; i <= pdf.numPages; i++) {
            const page = await pdf.getPage(i);

            const scale = 2; // Résolution
            const viewport = page.getViewport({ scale });

            const canvas = document.createElement('canvas');
            const context = canvas.getContext('2d');
            canvas.width = viewport.width;
            canvas.height = viewport.height;

            await page.render({ canvasContext: context, viewport }).promise;

            const imgData = canvas.toDataURL('image/png'); // image/png base64
            result.push(imgData);
        }

        return result; // tableau de "data:image/png;base64,..."
    }

    var handlePrintMenu = function () {

        //const printButtons = document.querySelectorAll('[data-kt-enteteresultat-table-filter="print_row"]');
        //console.log('handle print menu');
        //console.log(printButtons);

        //printButtons.forEach(d => {
        //    console.log(d);
        //    $(d).on('click', (e) => {
        //        console.log('click print menu');
        //        console.log("Impression démarrée", e.target);
        //        e.preventDefault();
        //        e.stopPropagation();

        //        var id = $(d).data('id');
        //        console.log(id);

        console.log('handlePrintMenu called!');

        $(document).off('click.print').on('click.print', '[data-kt-enteteresultat-table-filter="print_row"]', function (e) {
            e.preventDefault();
            e.stopPropagation();

            const $btn = $(e.currentTarget);
            const id = $btn.data('id');

            console.log("Impression demandée pour ID :", id);
            if (!id) { showErrorMessage('ID du résultat introuvable.'); return; }

            $("iframe[data-rapport='rapportanalyse']").remove();

            // Show enhanced loading spinner
            showPrintSpinner();

            $.ajax({
                url: '/Enteteresultat/GenererRapportAnalyse',
                type: 'GET',
                data: { id: id },
                success: function (data) {
                    console.log(data);

                    if (data.success) {
                        var printFrame = document.createElement('iframe');
                        printFrame.setAttribute('data-rapport', 'rapportanalyse');
                        printFrame.style.position = 'fixed';
                        printFrame.style.right = '0';
                        printFrame.style.bottom = '0';
                        printFrame.style.width = '0';
                        printFrame.style.height = '0';
                        printFrame.style.border = '0';
                        document.body.appendChild(printFrame);

                        printFrame.onload = function () {
                            var doc = printFrame.contentWindow || printFrame.contentDocument;
                            if (doc.document) doc = doc.document;

                            pdfBase64ToPngArray(data.pdfBase64)
                                .then(images => {
                                    images.forEach(img => {
                                        const imageElement = doc.createElement('img');
                                        imageElement.src = img;
                                        imageElement.style.maxWidth = '100%';
                                        doc.body.appendChild(imageElement);
                                    });
                                    setTimeout(function () {
                                        printFrame.contentWindow.focus();
                                        printFrame.contentWindow.print();
                                        setTimeout(function () {
                                            document.body.removeChild(printFrame);
                                            
                                            // Message de succès supprimé - impression silencieuse
                                        }, 1000);
                                    }, 500);
                                })
                                .catch(err => {
                                    console.error('Error converting PDF to images:', err);
                                    showErrorMessage('Erreur lors de la conversion du rapport en image.');
                                });
                        };
                        printFrame.src = 'about:blank';

                    } else {
                        showErrorMessage('Erreur lors de la génération du rapport.');
                    }
                },
                error: function (xhr, status, error) {
                    console.error('Ajax error:', error);
                    showErrorMessage('Erreur lors de la génération du rapport.');
                },
                complete: function () {
                    hidePrintSpinner();
                }
            });
        });
    };
    
    var showPrintSpinner = function() {
        const spinner = document.getElementById('print-spinner');
        if (spinner) {
            spinner.classList.remove('hidden');
            spinner.style.display = 'block';
            spinner.style.pointerEvents = 'auto';
        }
    };
    
    var hidePrintSpinner = function() {
        const spinner = document.getElementById('print-spinner');
        if (spinner) {
            spinner.classList.add('hidden');
            spinner.style.display = 'none';
            spinner.style.pointerEvents = 'none';
        }
    };
    
    var showSuccessMessage = function(message) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                text: message,
                icon: 'success',
                timer: 3000,
                timerProgressBar: true,
                showConfirmButton: false,
                toast: true,
                position: 'top-end'
            });
        } else {
            // Fallback notification
            console.log('Success:', message);
        }
    };
    
    var showErrorMessage = function(message) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                text: message,
                icon: 'error',
                confirmButtonText: 'Ok, compris !',
                customClass: {
                    confirmButton: 'bg-red-500 hover:bg-red-600 text-white px-4 py-2 rounded-lg',
                }
            });
        } else {
            alert(message);
        }
    };
    
    var handleModalClose = function() {
        const closeModalBtn = document.getElementById('closeRapportModal');
        if (closeModalBtn) {
            closeModalBtn.addEventListener('click', function() {
                const modal = document.getElementById('rapportModal');
                if (modal) {
                    modal.classList.add('hidden');
                }
            });
        }
    };
    
    var updateLastUpdateTime = function() {
        const lastUpdateElement = document.getElementById('lastUpdate');
        if (lastUpdateElement) {
            const now = new Date();
            lastUpdateElement.textContent = now.toLocaleTimeString('fr-FR', {
                hour: '2-digit',
                minute: '2-digit'
            });
        }
    };
    
    var ensureCleanState = function() {
        // Force hide all loading states on initialization
        hideTableLoading();
        hidePrintSpinner();
        
        // Ensure no modals are showing
        const modal = document.getElementById('rapportModal');
        if (modal) {
            modal.classList.add('hidden');
        }
        
        console.log('Clean state ensured - all spinners and modals hidden');
    };
    
    return {
        init: function () {
            table = document.querySelector('#enteteresultat-table');
            if (!table) return;
            
            // Ensure clean state first
            ensureCleanState();
            
            // Initialize all components
            initEnteteresultatTable();
            handleSearchDatatable();
            handleAdvancedFilters();
            handleCheckboxes();
            handlePrintMenu();
            handleModalClose();
            handlePageSizeChange();
            handleEditNavigation();
            
            // Update counts on load
            updateSelectedCount();
            updateVisibleCount();
            updatePaginationInfo();
            updateLastUpdateTime();
            
            // Auto-refresh last update time every minute
            setInterval(updateLastUpdateTime, 60000);
        }
    };
}();

//KTUtil.onDOMContentLoaded(function () {
//    KTEnteteresultatList.init();
//});
