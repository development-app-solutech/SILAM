"use strict";

var KTTarifcategorieassuranceCreate = function () {
    // Define shared variables
    var table;
    var datatable;
    var toolbarBase;
    var toolbarSelected;
    var selectedCount;
    var loader;
    var globalPrices = {}; // Stockage global de tous les prix saisis

    // Private functions
    var initTarifcategoriessuranceTable = function () {
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
                    targets: [0],
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

    var handlePrintTarifcategoriessurance = function () {
        const printLinks = document.querySelectorAll('[data-kt-list-tarifcategoriessurance-table-print="print_table"]');
        console.log(printLinks);

        printLinks.forEach(d => {
            //console.log(d);
            d.addEventListener('click', function (e) {
                e.preventDefault();
                console.log(e);

                $.ajax({
                    url: `/Tarifcategoriessurances/PrintList`,
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
                        Swal.fire({
                            text: "Erreur lors de l'impression.",
                            icon: "error",
                            buttonsStyling: false,
                            confirmButtonText: "Ok, got it!",
                            customClass: {
                                confirmButton: "btn fw-bold btn-primary",
                            }
                        });
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
        const filterSearch = document.querySelector('[data-kt-tarifcategorieassurance-table-filter="search"]');
        if (filterSearch) {
            filterSearch.addEventListener('keyup', function (e) {
                console.log("e.target.value", e.target.value);
                datatable.search(e.target.value).draw();
            });
        }
    }

    // Filter Datatable
    var handleFilterDatatable = () => {
        // Select filter options
        const filterForm = document.querySelector('[data-kt-tarifcategoriessurance-table-filter="form"]');
        const filterButton = filterForm.querySelector('[data-kt-tarifcategoriessurance-table-filter="filter"]');
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
        const resetButton = document.querySelector('[data-kt-tarifcategoriessurance-table-filter="reset"]');

        // Reset datatable
        resetButton.addEventListener('click', function () {
            // Select filter options
            const filterForm = document.querySelector('[data-kt-tarifcategoriessurance-table-filter="form"]');
            const selectOptions = filterForm.querySelectorAll('select');

            // Reset select2 values -- more info: https://select2.org/programmatic-control/add-select-clear-items
            selectOptions.forEach(select => {
                $(select).val('').trigger('change');
            });

            // Reset datatable --- official docs reference: https://datatables.net/reference/api/search()
            datatable.search('').draw();
        });
    }

    // Handle laboratoire filter
    var handleLaboratoireFilter = () => {
        console.log('🔧 Setting up laboratoire filter...');
        const laboratoireSelect = document.querySelector('#Idlaboratoire');
        console.log('📋 Laboratoire select element:', laboratoireSelect);
        if (laboratoireSelect) {
            console.log('✅ Laboratoire select found, adding event listeners');
            
            // Standard change event
            laboratoireSelect.addEventListener('change', function () {
                const idlaboratoire = this.value;
                console.log('🚀 Laboratory changed (standard event)! idlaboratoire:', idlaboratoire);
                loadCategoriesByLaboratoire(idlaboratoire);
            });
            
            // Select2 change event
            $(laboratoireSelect).on('select2:select', function (e) {
                const idlaboratoire = e.params.data.id;
                console.log('🚀 Laboratory changed (select2 event)! idlaboratoire:', idlaboratoire);
                loadCategoriesByLaboratoire(idlaboratoire);
            });
            
        } else {
            console.error('❌ Laboratoire select element not found!');
        }
    }

    // Load categories by laboratoire via AJAX
    var loadCategoriesByLaboratoire = (idlaboratoire) => {
        console.log('loadCategoriesByLaboratoire called with idlaboratoire:', idlaboratoire);
        
        const tableBody = document.querySelector('#kt_table_tarifcategorieassurance_create tbody');
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
        console.log('💰 Global prices storage:', globalPrices);

        $.ajax({
            url: '/Tarifcategorieassurance/GetCategoriesByLaboratoire',
            type: 'GET',
            data: { 
                idlaboratoire: idlaboratoire || null
            },
            success: function (categories) {
                console.log('Categories received:', categories);
                
                if (categories.error) {
                    console.error('Error from server:', categories.error);
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
                if (categories && categories.length > 0) {
                    categories.forEach((categorie, index) => {
                        // Récupérer le prix depuis le stockage global ou celui de la base de données
                        const savedPrice = globalPrices[categorie.categorieid] || categorie.prix || '';
                        
                        const row = document.createElement('tr');
                        row.innerHTML = `
                            <td class="font-normal text-foreground">
                                <input hidden type="text" name="Tarif[${index}].Categorieid" value="${categorie.categorieid}" />
                                <input hidden type="text" name="Tarif[${index}].Nom" value="${categorie.nom}" />
                                <span>${categorie.nom}</span>
                            </td>
                            <td class="font-normal text-foreground">
                                <input class="kt-input price-input" name="Tarif[${index}].Prix" placeholder="" type="number" step="0.01" value="${savedPrice}" data-category-id="${categorie.categorieid}">
                                <span class="text-destructive"></span>
                            </td>
                        `;
                        tableBody.appendChild(row);
                    });
                    
                    // Réinitialiser DataTable
                    initializeDataTable();
                    
                    // Ajouter les écouteurs d'événements pour sauvegarder automatiquement les prix
                    setupPriceAutoSave();
                    
                    console.log('✅ Table rebuilt with', categories.length, 'categories and DataTable initialized');
                } else {
                    // Afficher un message si aucune catégorie
                    const emptyRow = document.createElement('tr');
                    emptyRow.innerHTML = `
                        <td colspan="2" class="text-center text-muted py-4">
                            <i class="ki-duotone ki-information fs-1 text-muted mb-2"></i><br>
                            Aucune catégorie disponible pour ce laboratoire
                        </td>
                    `;
                    tableBody.appendChild(emptyRow);
                    
                    console.log('📝 Empty message displayed - DataTables not initialized');
                }
            },
            error: function (xhr, status, error) {
                console.error('AJAX error:', error);
                console.error('Status:', status);
                console.error('Response:', xhr.responseText);
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
            console.log('🔄 DataTable initialized');
        }
    }

    // Function to save current prices from table to global storage
    var saveCurrentPrices = () => {
        const tableBody = document.querySelector('#kt_table_tarifcategorieassurance_create tbody');
        if (!tableBody) return;
        
        const priceInputs = tableBody.querySelectorAll('input[name*=".Prix"]');
        priceInputs.forEach(input => {
            const categoryInput = input.closest('tr').querySelector('input[name*=".Categorieid"]');
            if (categoryInput) {
                const categoryId = categoryInput.value;
                if (input.value) {
                    globalPrices[categoryId] = input.value;
                    console.log(`💾 Saved price ${input.value} for category ${categoryId}`);
                }
            }
        });
    }

    // Function to setup automatic price saving on input change
    var setupPriceAutoSave = () => {
        const tableBody = document.querySelector('#kt_table_tarifcategorieassurance_create tbody');
        if (!tableBody) return;
        
        // Add event listeners to all price inputs
        const priceInputs = tableBody.querySelectorAll('.price-input');
        priceInputs.forEach(input => {
            // Save on blur (when user leaves the field)
            input.addEventListener('blur', function() {
                const categoryId = this.getAttribute('data-category-id');
                if (this.value) {
                    globalPrices[categoryId] = this.value;
                    console.log(`💾 Auto-saved price ${this.value} for category ${categoryId}`);
                } else {
                    // Remove from storage if empty
                    delete globalPrices[categoryId];
                    console.log(`🗑️ Removed price for category ${categoryId}`);
                }
            });
            
            // Also save on Enter key
            input.addEventListener('keydown', function(e) {
                if (e.key === 'Enter') {
                    this.blur(); // Trigger the blur event
                }
            });
        });
        
        console.log(`🔗 Auto-save setup for ${priceInputs.length} price inputs`);
    }


    // Delete subscirption
    var handleDeleteRows = () => {
        // Select all delete buttons
        const deleteButtons = table.querySelectorAll('[data-kt-tarifcategoriessurances-table-filter="delete_row"]');

        deleteButtons.forEach(d => {
            // Delete button on click
            d.addEventListener('click', function (e) {
                e.preventDefault();

                // Select parent row
                const parent = e.target.closest('tr');

                // Get user name
                //const userName = parent.querySelectorAll('td')[1].querySelectorAll('a')[1].innerText;
                //const userName = parent.querySelectorAll('td')[2].innerText;
                const userName = "The row";

                // SweetAlert2 pop up --- official docs reference: https://sweetalert2.github.io/
                Swal.fire({
                    text: "Are you sure you want to delete " + userName + "?",
                    icon: "warning",
                    showCancelButton: true,
                    buttonsStyling: false,
                    confirmButtonText: "Yes, delete!",
                    cancelButtonText: "No, cancel",
                    customClass: {
                        confirmButton: "kt-btn fw-bold kt-btn-destructive",
                        cancelButton: "kt-btn fw-bold kt-btn-active-light-primary"
                    }
                }).then(function (result) {
                    if (result.value) {

                        // ajax pour supprimer

                        let id = $(d).data('id');
                        console.log(id);

                        $.ajax({
                            url: `/Tarifcategoriessurance/DeleteTarifcategoriessurance?id=${id}`,
                            type: 'POST',
                            beforeSend: function () {

                                // show loader
                                $(loader).css('visibility', 'visible');
                            },
                            success: function (response) {

                                console.log(response);

                                if (response.success) {

                                    Swal.fire({
                                        text: "You have deleted " + userName + "!.",
                                        icon: "success",
                                        buttonsStyling: false,
                                        confirmButtonText: "Ok, got it!",
                                        customClass: {
                                            confirmButton: "kt-btn fw-bold kt-btn-primary",
                                        }
                                    }).then(function () {
                                        // Remove current row
                                        datatable.row($(parent)).remove().draw();
                                    }).then(function () {
                                        // Detect checked checkboxes
                                        toggleToolbars();
                                    });
                                }
                            },
                            error: function (request, error) {
                                Swal.fire({
                                    text: userName + " was not deleted.",
                                    icon: "error",
                                    buttonsStyling: false,
                                    confirmButtonText: "Ok, got it!",
                                    customClass: {
                                        confirmButton: "kt-btn fw-bold kt-btn-primary",
                                    }
                                });
                            },
                            error: function (request, error) {
                                Swal.fire({
                                    text: userName + " was not deleted.",
                                    icon: "error",
                                    buttonsStyling: false,
                                    confirmButtonText: "Ok, got it!",
                                    customClass: {
                                        confirmButton: "kt-btn fw-bold kt-btn-primary",
                                    }
                                });
                            }

                        }).always(function (dataOrjqXHR, textStatus, jqXHROrerrorThrown) {
                            // hide loader
                            $(loader).css('visibility', 'hidden');
                        });

                    } else if (result.dismiss === 'cancel') {
                        Swal.fire({
                            text: userName + " was not deleted.",
                            icon: "error",
                            buttonsStyling: false,
                            confirmButtonText: "Ok, got it!",
                            customClass: {
                                confirmButton: "kt-btn fw-bold kt-btn-primary",
                            }
                        });
                    }

                });
            })
        });
    }

    // Init toggle toolbar
    var initToggleToolbar = () => {
        // Toggle selected action toolbar
        // Select all checkboxes
        const checkboxes = table.querySelectorAll('[type="checkbox"]');

        // Select elements
        toolbarBase = document.querySelector('[data-kt-tarifcategoriessurance-table-toolbar="base"]');
        toolbarSelected = document.querySelector('[data-kt-tarifcategoriessurance-table-toolbar="selected"]');
        selectedCount = document.querySelector('[data-kt-tarifcategoriessurance-table-select="selected_count"]');
        const deleteSelected = document.querySelector('[data-kt-tarifcategoriessurance-table-select="delete_selected"]');

        // Toggle delete selected toolbar
        checkboxes.forEach(c => {
            // Checkbox on click event
            c.addEventListener('click', function () {
                setTimeout(function () {
                    toggleToolbars();
                }, 50);
            });
        });

        // Deleted selected rows
        deleteSelected.addEventListener('click', function () {
            // SweetAlert2 pop up --- official docs reference: https://sweetalert2.github.io/
            Swal.fire({
                text: "Are you sure you want to delete selected customers?",
                icon: "warning",
                showCancelButton: true,
                buttonsStyling: false,
                confirmButtonText: "Yes, delete!",
                cancelButtonText: "No, cancel",
                customClass: {
                    confirmButton: "btn fw-bold btn-danger",
                    cancelButton: "btn fw-bold btn-active-light-primary"
                }
            }).then(function (result) {
                if (result.value) {
                    Swal.fire({
                        text: "You have deleted all selected customers!.",
                        icon: "success",
                        buttonsStyling: false,
                        confirmButtonText: "Ok, got it!",
                        customClass: {
                            confirmButton: "btn fw-bold btn-primary",
                        }
                    }).then(function () {
                        // Remove all selected customers
                        checkboxes.forEach(c => {
                            if (c.checked) {
                                datatable.row($(c.closest('tbody tr'))).remove().draw();
                            }
                        });

                        // Remove header checked box
                        const headerCheckbox = table.querySelectorAll('[type="checkbox"]')[0];
                        headerCheckbox.checked = false;
                    }).then(function () {
                        toggleToolbars(); // Detect checked checkboxes
                        initToggleToolbar(); // Re-init toolbar to recalculate checkboxes
                    });
                } else if (result.dismiss === 'cancel') {
                    Swal.fire({
                        text: "Selected customers was not deleted.",
                        icon: "error",
                        buttonsStyling: false,
                        confirmButtonText: "Ok, got it!",
                        customClass: {
                            confirmButton: "btn fw-bold btn-primary",
                        }
                    });
                }
            });
        });
    }

    // Toggle toolbars
    const toggleToolbars = () => {
        // Select refreshed checkbox DOM elements 
        //const allCheckboxes = table.querySelectorAll('tbody [type="checkbox"]');
        const allCheckboxes = table.querySelectorAll('input[name = "list_tarifcategoriessurance_case_a_cocher"][type="checkbox"]');
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
            table = document.querySelector('#kt_table_tarifcategorieassurance_create');
            if (!table) {
                return;
            }

            loader = document.querySelector('#kt_spinner_tarifcategorieassurance_create');

            initTarifcategoriessuranceTable();
            //initToggleToolbar();
            handleSearchDatatable();
            handleLaboratoireFilter();
            //handlePrintTarifcategoriessurance();
            //handleResetForm();
            //handleDeleteRows();
            //handleFilterDatatable();

        }
    }
}();

// On document ready
//KTUtil.onDOMContentLoaded(function () {
//    KTTarifcategoriessuranceCreate.init();
//});