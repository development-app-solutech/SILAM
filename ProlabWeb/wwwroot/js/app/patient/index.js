"use strict";

var KTPatientList = function () {
    // Define shared variables
    var table;
    var datatable;
    var toolbarBase;
    var toolbarSelected;
    var selectedCount;
    var loader;

    // Private functions
    var initPatientTable = function () {
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
                    targets: [1,9],
                    className: 'dt-head-center dt-body-center'
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

    var handlePrintPatient = function () {
        const printLinks = document.querySelectorAll('[data-kt-list-patient-table-print="print_table"]');
        console.log(printLinks);

        printLinks.forEach(d => {
            //console.log(d);
            d.addEventListener('click', function (e) {
                e.preventDefault();
                console.log(e);

                $.ajax({
                    url: `/Patients/PrintList`,
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
        const filterSearch = document.querySelector('[data-kt-patient-table-filter="search"]');
        filterSearch.addEventListener('keyup', function (e) {
            console.log("e.target.value", e.target.value);
            datatable.search(e.target.value).draw();
        });
    }

    // Filter Datatable
    var handleFilterDatatable = () => {
        // Select filter options
        const filterForm = document.querySelector('[data-kt-patient-table-filter="form"]');
        const filterButton = filterForm.querySelector('[data-kt-patient-table-filter="filter"]');
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
        const resetButton = document.querySelector('[data-kt-patient-table-filter="reset"]');

        // Reset datatable
        resetButton.addEventListener('click', function () {
            // Select filter options
            const filterForm = document.querySelector('[data-kt-patient-table-filter="form"]');
            const selectOptions = filterForm.querySelectorAll('select');

            // Reset select2 values -- more info: https://select2.org/programmatic-control/add-select-clear-items
            selectOptions.forEach(select => {
                $(select).val('').trigger('change');
            });

            // Reset datatable --- official docs reference: https://datatables.net/reference/api/search()
            datatable.search('').draw();
        });
    }


    // Delete subscirption
    var handleDeleteRows = () => {
        // Select all delete buttons
        const deleteButtons = table.querySelectorAll('[data-kt-patients-table-filter="delete_row"]');

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
                            url: `/Patient/DeletePatient?id=${id}`,
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
        toolbarBase = document.querySelector('[data-kt-patient-table-toolbar="base"]');
        toolbarSelected = document.querySelector('[data-kt-patient-table-toolbar="selected"]');
        selectedCount = document.querySelector('[data-kt-patient-table-select="selected_count"]');
        const deleteSelected = document.querySelector('[data-kt-patient-table-select="delete_selected"]');

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
        const allCheckboxes = table.querySelectorAll('input[name = "list_patient_case_a_cocher"][type="checkbox"]');
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
            table = document.querySelector('#kt_table_patient_index');
            if (!table) {
                return;
            }

            loader = document.querySelector('#kt_spinner_patient_index');

            initPatientTable();
            //initToggleToolbar();
            handleSearchDatatable();
            //handlePrintPatient();
            //handleResetForm();
            handleDeleteRows();
            //handleFilterDatatable();

        }
    }
}();

// On document ready
//KTUtil.onDOMContentLoaded(function () {
//    KTPatientList.init();
//});