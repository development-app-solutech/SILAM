"use strict";

var KTTarifanalyseassuranceEdit = function () {
    // Define shared variables
    var table;
    var datatable;
    var toolbarBase;
    var toolbarSelected;
    var selectedCount;
    var loader;
    var globalPrices = {}; // Stockage global de tous les prix saisis

    // Private functions
    var initTarifanalyseassuranceTable = function () {
        //// Set date data order
        //const tableRows = table.querySelectorAll('tbody tr');

        //tableRows.forEach(row => {
        //    const dateRow = row.querySelectorAll('td');
        //    const lastLogin = dateRow[3].innerText.toLowerCase(); // Get last login time
        //    let timeCount = 0;
        //    let timeFormat = 'minutes';

        //    // Determine date & time format -- add more formats when necessary
        //    if (lastLogin.includes('yesterday')) {
        //        timeCount = 1;
        //        timeFormat = 'days';
        //    } else if (lastLogin.includes('mins')) {
        //        timeCount = parseInt(lastLogin.replace(/\D/g, ''));
        //        timeFormat = 'minutes';
        //    } else if (lastLogin.includes('hours')) {
        //        timeCount = parseInt(lastLogin.replace(/\D/g, ''));
        //        timeFormat = 'hours';
        //    } else if (lastLogin.includes('days')) {
        //        timeCount = parseInt(lastLogin.replace(/\D/g, ''));
        //        timeFormat = 'days';
        //    } else if (lastLogin.includes('weeks')) {
        //        timeCount = parseInt(lastLogin.replace(/\D/g, ''));
        //        timeFormat = 'weeks';
        //    }

        //    // Subtract date/time from today -- more info on moment datetime subtraction: https://momentjs.com/docs/#/durations/subtract/
        //    const realDate = moment().subtract(timeCount, timeFormat).format();

        //    // Insert real date to last login attribute
        //    dateRow[3].setAttribute('data-order', realDate);

        //    // Set real date for joined column
        //    const joinedDate = moment(dateRow[5].innerHTML, "DD MMM YYYY, LT").format(); // select date from 5th column in table
        //    dateRow[5].setAttribute('data-order', joinedDate);
        //});

        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatable = $(table).DataTable({
            ordering: false,
            searching: true,
            scrollX: true,
            //"info": false,
            'order': [],
            "pageLength": 10,
            //"lengthChange": false,
            'columnDefs': [
                {
                    targets: [0],
                    width: '60%'
                },
                {
                    targets: [1],
                    width: '40%'
                }
            ]
        });

        $('.dt-search').css('display', 'none');

        // Re-init functions on every table re-draw -- more info: https://datatables.net/reference/event/draw
        //datatable.on('draw', function () {
        //    initToggleToolbar();
        //    handleDeleteRows();
        //    toggleToolbars();
        //    KTMenu.init();
        //});
    }

    // Function to convert a Base64 string to a Blob
    var b64toBlob = function (b64Data, contentType, sliceSize) {
        contentType = contentType || '';
        sliceSize = sliceSize || 512;

        var byteCharacters = atob(b64Data);
        var byteArrays = [];

        for (var offset = 0; offset < byteCharacters.length; offset += sliceSize) {
            var slice = byteCharacters.slice(offset, offset + sliceSize);

            var byteNumbers = new Array(slice.length);
            for (var i = 0; i < slice.length; i++) {
                byteNumbers[i] = slice.charCodeAt(i);
            }

            var byteArray = new Uint8Array(byteNumbers);
            byteArrays.push(byteArray);
        }

        var blob = new Blob(byteArrays, { type: contentType });
        return blob;
    }

    var handlePrintTarifanalyseassurance = function () {
        const printLinks = document.querySelectorAll('[data-kt-list-tarifanalyseassurance-table-print="print_table"]');
        console.log(printLinks);

        printLinks.forEach(d => {
            //console.log(d);
            d.addEventListener('click', function (e) {
                e.preventDefault();
                console.log(e);

                $.ajax({
                    url: `/Tarifanalyseassurances/PrintList`,
                    type: 'POST',
                    beforeSend: function () {

                        // show loader
                        $(loader).css('visibility', 'visible');
                    },
                    success: function (data) {
                        // Create a Blob from the Base64 string
                        var blob = b64toBlob(data, 'application/pdf');
                        // Create an object URL from the Blob
                        var objectUrl = URL.createObjectURL(blob);
                        // Create an iframe with the object URL as the src
                        var iframe = $('<iframe style="display: none;"></iframe>');
                        iframe.attr('src', objectUrl);
                        $('body').append(iframe);
                        // Wait for the iframe to load and then print its content
                        iframe.on('load', function () {
                            iframe[0].contentWindow.focus();
                            iframe[0].contentWindow.print();
                        });
                    },
                    error: function (request, error) {
                        // Erreur silencieuse pour l'impression - pas de popup
                        console.error('🖨️ Erreur lors de l\'impression (EDIT):', error);
                    }
                }).always(function (dataOrjqXHR, textStatus, jqXHROrerrorThrown) {
                    // hide loader
                    $(loader).css('visibility', 'hidden');
                });

            });
        });
    }

    // Search Datatable --- official docs reference: https://datatables.net/reference/api/search()
    var handleSearchDatatable = () => {
        const filterSearch = document.querySelector('[data-kt-tarifanalyseassurance-table-filter="search"]');
        if (filterSearch && datatable) {
            filterSearch.addEventListener('keyup', function (e) {
                console.log("e.target.value", e.target.value);
                datatable.search(e.target.value).draw();
            });
            console.log('🔍 Search handler attached (EDIT)');
        } else {
            console.warn('⚠️ Search input or datatable not found (EDIT)');
        }
    }

    // Handle laboratory filter
    var handleLaboratoireFilter = () => {
        console.log('🔬 handleLaboratoireFilter called (EDIT)');
        
        // Attendre un peu pour que Select2 soit initialisé si nécessaire
        setTimeout(() => {
            const laboratoireSelect = document.querySelector('select[name="Idlaboratoire"]');
            console.log('laboratoireSelect found:', laboratoireSelect);
            
            if (laboratoireSelect) {
                console.log('Adding change event listener to laboratoire select (EDIT)');
                
                // Support pour Select2 et sélecteur normal
                $(laboratoireSelect).on('change.laboratoireFilter', function() {
                    console.log('🔄 Laboratoire changed (EDIT):', this.value);
                    const idlaboratoire = this.value;
                    const codeassurance = getCodeAssurance();
                    if (idlaboratoire && idlaboratoire !== '') {
                        loadAnalysesByLaboratoire(idlaboratoire, codeassurance);
                    } else {
                        console.log('⚠️ Empty laboratoire selected, clearing table (EDIT)');
                        clearAnalysesTable();
                    }
                });
                
                console.log('✅ Event listener attached successfully (EDIT)');
            } else {
                console.warn('⚠️ laboratoireSelect not found (EDIT)');
            }
        }, 500);
    }

    // Get code assurance from hidden input or form
    var getCodeAssurance = () => {
        const codeassuranceInput = document.querySelector('input[name="Codeassurance"]');
        return codeassuranceInput ? codeassuranceInput.value : null;
    }

    // Load analyses by laboratoire with advanced logic (EDIT mode)
    var loadAnalysesByLaboratoire = (idlaboratoire, codeassurance) => {
        console.log('loadAnalysesByLaboratoire called with idlaboratoire:', idlaboratoire, 'codeassurance:', codeassurance);
        
        const tableBody = document.querySelector('#kt_table_tarifanalyseassurance_edit tbody');
        if (!tableBody) {
            console.error('Table body not found');
            return;
        }

        // Show loading indicator
        if (loader) {
            $(loader).css('visibility', 'visible');
        }

        // Sauvegarder les prix saisis par l'utilisateur dans le stockage global
        saveCurrentPrices();
        console.log('💰 Global prices storage (EDIT):', globalPrices);

        $.ajax({
            url: '/Tarifanalyseassurance/GetAnalysesByLaboratoire',
            type: 'POST',
            data: { 
                idlaboratoire: idlaboratoire || null,
                codeassurance: codeassurance || null
            },
            success: function (analyses) {
                console.log('Analyses received:', analyses);
                
                if (analyses.error) {
                    console.error('Error from server:', analyses.error);
                    return;
                }

                // Détruire le DataTable existant s'il existe
                if (datatable && $.fn.DataTable.isDataTable(table)) {
                    datatable.clear().destroy();
                    datatable = null;
                }
                
                // Clear table body
                tableBody.innerHTML = '';
                
                // Rebuild table with new data
                if (analyses && analyses.length > 0) {
                    analyses.forEach((analyse, index) => {
                        // En mode EDIT: utiliser les prix de la base de données ou les prix saisis par l'utilisateur
                        const savedPrice = globalPrices[analyse.idanalyse] || analyse.prix || '';
                        
                        const row = document.createElement('tr');
                        row.innerHTML = `
                            <td class="font-normal text-foreground">
                                <input hidden type="text" name="Tarif[${index}].Idanalyse" value="${analyse.idanalyse}" />
                                <input hidden type="text" name="Tarif[${index}].Nom" value="${analyse.nom}" />
                                <span>${analyse.nom}</span>
                            </td>
                            <td class="font-normal text-foreground">
                                <input class="kt-input price-input" name="Tarif[${index}].Prix" placeholder="" type="number" step="0.01" value="${savedPrice}" data-analyse-id="${analyse.idanalyse}">
                                <span class="text-destructive"></span>
                            </td>
                        `;
                        tableBody.appendChild(row);
                    });
                    
                    // Réinitialiser DataTable
                    initializeDataTable();
                    
                    // Ajouter les écouteurs d'événements pour sauvegarder automatiquement les prix
                    setupPriceAutoSave();
                    
                    console.log('✅ Table rebuilt with', analyses.length, 'analyses and DataTable initialized (EDIT)');
                } else {
                    // Afficher un message si aucune analyse
                    const emptyRow = document.createElement('tr');
                    emptyRow.innerHTML = `
                        <td colspan="2" class="text-center text-muted py-4">
                            <i class="ki-duotone ki-information fs-1 text-muted mb-2"></i><br>
                            Aucune analyse disponible pour ce laboratoire
                        </td>
                    `;
                    tableBody.appendChild(emptyRow);
                    
                    console.log('📝 Empty message displayed (EDIT) - DataTables not initialized');
                }
            },
            error: function (xhr, status, error) {
                console.error('AJAX error:', error);
                console.error('Status:', status);
                console.error('Response:', xhr.responseText);
                // Erreur silencieuse - pas de popup pour éviter les conflits
                console.warn('⚠️ Erreur lors du chargement des analyses (EDIT)');
            },
            complete: function () {
                // Hide loading indicator
                if (loader) {
                    $(loader).css('visibility', 'hidden');
                }
            }
        });
    }

    // Function to initialize DataTable
    var initializeDataTable = () => {
        if (table && !$.fn.DataTable.isDataTable(table)) {
            datatable = $(table).DataTable({
                ordering: false,
                searching: true,
                scrollX: true,
                'order': [],
                "pageLength": 10,
                'columnDefs': [
                    {
                        targets: [0],
                        width: '60%'
                    },
                    {
                        targets: [1],
                        width: '40%'
                    }
                ]
            });
            $('.dt-search').css('display', 'none');
            console.log('🔄 DataTable initialized (Analyses EDIT)');
        }
    }

    // Function to save current prices from table to global storage
    var saveCurrentPrices = () => {
        const tableBody = document.querySelector('#kt_table_tarifanalyseassurance_edit tbody');
        if (!tableBody) return;
        
        const priceInputs = tableBody.querySelectorAll('input[name*=".Prix"]');
        priceInputs.forEach(input => {
            const analyseInput = input.closest('tr').querySelector('input[name*=".Idanalyse"]');
            if (analyseInput) {
                const analyseId = analyseInput.value;
                if (input.value) {
                    globalPrices[analyseId] = input.value;
                    console.log(`💾 Saved price ${input.value} for analyse ${analyseId} (EDIT)`);
                }
            }
        });
    }

    // Function to setup automatic price saving on input change
    var setupPriceAutoSave = () => {
        const tableBody = document.querySelector('#kt_table_tarifanalyseassurance_edit tbody');
        if (!tableBody) return;
        
        // Add event listeners to all price inputs
        const priceInputs = tableBody.querySelectorAll('.price-input');
        priceInputs.forEach(input => {
            // Save on blur (when user leaves the field)
            input.addEventListener('blur', function() {
                const analyseId = this.getAttribute('data-analyse-id');
                if (this.value) {
                    globalPrices[analyseId] = this.value;
                    console.log(`💾 Auto-saved price ${this.value} for analyse ${analyseId} (EDIT)`);
                } else {
                    // Remove from storage if empty
                    delete globalPrices[analyseId];
                    console.log(`🗑️ Removed price for analyse ${analyseId} (EDIT)`);
                }
            });
            
            // Also save on Enter key
            input.addEventListener('keydown', function(e) {
                if (e.key === 'Enter') {
                    this.blur(); // Trigger the blur event
                }
            });
        });
        
        console.log(`🔗 Auto-save setup for ${priceInputs.length} price inputs (Analyses EDIT)`);
    }

    // Function to clear analyses table
    var clearAnalysesTable = () => {
        console.log('🧹 Clearing analyses table (EDIT)');
        const tableBody = document.querySelector('#kt_table_tarifanalyseassurance_edit tbody');
        if (!tableBody) return;
        
        // Destroy DataTable if it exists
        if (datatable && $.fn.DataTable.isDataTable(table)) {
            datatable.clear().destroy();
            datatable = null;
        }
        
        // Clear table body
        tableBody.innerHTML = '';
        
        // Show empty message
        const emptyRow = document.createElement('tr');
        emptyRow.innerHTML = `
            <td colspan="2" class="text-center text-muted py-4">
                <i class="ki-duotone ki-information fs-1 text-muted mb-2"></i><br>
                Veuillez sélectionner un laboratoire
            </td>
        `;
        tableBody.appendChild(emptyRow);
        
        console.log('📝 Table cleared - Please select laboratory message displayed (EDIT)');
    }

    // Filter Datatable
    var handleFilterDatatable = () => {
        // Select filter options
        const filterForm = document.querySelector('[data-kt-tarifanalyseassurance-table-filter="form"]');
        const filterButton = filterForm.querySelector('[data-kt-tarifanalyseassurance-table-filter="filter"]');
        const selectOptions = filterForm.querySelectorAll('select');

        // Filter datatable on submit
        filterButton.addEventListener('click', function () {
            var filterString = '';

            // Get filter values
            selectOptions.forEach((item, index) => {
                if (item.value && item.value !== '') {
                    if (index !== 0) {
                        filterString += ' ';
                    }

                    // Build filter value options
                    filterString += item.value;
                }
            });

            // Filter datatable --- official docs reference: https://datatables.net/reference/api/search()
            datatable.search(filterString).draw();
        });
    }

    // Reset Filter
    var handleResetForm = () => {
        // Select reset button
        const resetButton = document.querySelector('[data-kt-tarifanalyseassurance-table-filter="reset"]');

        // Reset datatable
        resetButton.addEventListener('click', function () {
            // Select filter options
            const filterForm = document.querySelector('[data-kt-tarifanalyseassurance-table-filter="form"]');
            const selectOptions = filterForm.querySelectorAll('select');

            // Reset select2 values -- more info: https://select2.org/programmatic-control/add-select-clear-items
            selectOptions.forEach(select => {
                $(select).val('').trigger('change');
            });

            // Reset datatable --- official docs reference: https://datatables.net/reference/api/search()
            datatable.search('').draw();
        });
    }


    // Delete subscription - Fonction désactivée (SweetAlert supprimé)
    var handleDeleteRows = () => {
        // Fonction désactivée pour éviter les conflits avec SweetAlert
        console.log('⚠️ handleDeleteRows désactivé (EDIT) - SweetAlert retiré');
        return;
    }

    // Init toggle toolbar
    var initToggleToolbar = () => {
        // Toggle selected action toolbar
        // Select all checkboxes
        const checkboxes = table.querySelectorAll('[type="checkbox"]');

        // Select elements
        toolbarBase = document.querySelector('[data-kt-tarifanalyseassurance-table-toolbar="base"]');
        toolbarSelected = document.querySelector('[data-kt-tarifanalyseassurance-table-toolbar="selected"]');
        selectedCount = document.querySelector('[data-kt-tarifanalyseassurance-table-select="selected_count"]');
        const deleteSelected = document.querySelector('[data-kt-tarifanalyseassurance-table-select="delete_selected"]');

        // Toggle delete selected toolbar
        checkboxes.forEach(c => {
            // Checkbox on click event
            c.addEventListener('click', function () {
                setTimeout(function () {
                    toggleToolbars();
                }, 50);
            });
        });

        // Deleted selected rows - Fonction désactivée (SweetAlert supprimé)
        if (deleteSelected) {
            deleteSelected.addEventListener('click', function () {
                console.log('⚠️ Delete selected désactivé (EDIT) - SweetAlert retiré');
                // Fonction désactivée pour éviter les conflits avec SweetAlert
                return;
            });
        }
    }

    // Toggle toolbars
    const toggleToolbars = () => {
        // Select refreshed checkbox DOM elements 
        //const allCheckboxes = table.querySelectorAll('tbody [type="checkbox"]');
        const allCheckboxes = table.querySelectorAll('input[name = "list_tarifanalyseassurance_case_a_cocher"][type="checkbox"]');
        console.log(allCheckboxes);

        // Detect checkboxes state & count
        let checkedState = false;
        let count = 0;

        // Count checked boxes
        allCheckboxes.forEach(c => {
            if (c.checked) {
                checkedState = true;
                count++;
            }
        });

        // Toggle toolbars
        if (checkedState) {
            selectedCount.innerHTML = count;
            toolbarBase.classList.add('d-none');
            toolbarSelected.classList.remove('d-none');
        } else {
            toolbarBase.classList.remove('d-none');
            toolbarSelected.classList.add('d-none');
        }
    }

    return {
        // Public functions  
        init: function () {
            table = document.querySelector('#kt_table_tarifanalyseassurance_edit');
            if (!table) {
                return;
            }

            loader = document.querySelector('#kt_spinner_tarifanalyseassurance_edit');

            initTarifanalyseassuranceTable();
            //initToggleToolbar();
            handleSearchDatatable();
            handleLaboratoireFilter();
            //handlePrintTarifanalyseassurance();
            //handleResetForm();
            //handleDeleteRows();
            //handleFilterDatatable();

        }
    }
}();

//// On document ready
//$(document).ready(function() {
//    console.log('Document ready, initializing KTTarifanalyseassuranceEdit');
//    KTTarifanalyseassuranceEdit.init();
//});
