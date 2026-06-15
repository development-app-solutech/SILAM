"use strict";

var KTPrescripteurList = function () {
    var table;
    var datatable;
    var loader;

    // Initialisation du DataTable
    var initTable = function () {
        if (!$.fn.DataTable.isDataTable(table)) {
            datatable = $(table).DataTable({
                ordering: false,
                searching: true,
                scrollX: true,
                'order': [],
                "pageLength": 10
            });
            $('.dt-search').css('display', 'none');
        } else {
            datatable = $(table).DataTable();
        }
        console.log('DataTable initialisÃ© ?', $.fn.DataTable.isDataTable(table), datatable); // LOG
    };

    // Recherche dynamique
    var handleSearchDatatable = function () {
        const filterSearch = document.querySelector('[data-kt-prescripteur-table-filter="search"]');
        if (filterSearch) {
            console.log("ðŸ” Champ de recherche dÃ©tectÃ© !");
            filterSearch.addEventListener('keyup', function (e) {
                const searchTerm = e.target.value;
                console.log('ðŸ” Terme recherchÃ© :', searchTerm);
                if ($.fn.DataTable.isDataTable(table)) {
                    $(table).DataTable().search(searchTerm).draw();
                }
            });
        } else {
            console.warn("âš ï¸ Champ de recherche non trouvÃ© !");
        }
    };

    // Impression PDF
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
    };

    var handlePrint = function () {
        const printLinks = document.querySelectorAll('[data-kt-list-activity-table-print="print_table"]');
        printLinks.forEach(d => {
            d.addEventListener('click', function (e) {
                e.preventDefault();
                $.ajax({
                    url: '/Prescripteur/PrintList',
                    type: 'POST',
                    beforeSend: function () {
                        if (loader) $(loader).css('visibility', 'visible');
                    },
                    success: function (data) {
                        var blob = b64toBlob(data, 'application/pdf');
                        var objectUrl = URL.createObjectURL(blob);
                        var iframe = $('<iframe style="display: none;"></iframe>');
                        iframe.attr('src', objectUrl);
                        $('body').append(iframe);
                        iframe.on('load', function () {
                            iframe[0].contentWindow.focus();
                            iframe[0].contentWindow.print();
                        });
                    },
                    error: function () {
                        Swal.fire({
                            text: "Erreur lors de l'impression.",
                            icon: "error",
                            buttonsStyling: false,
                            confirmButtonText: "Ok, got it!",
                            customClass: { confirmButton: "btn fw-bold btn-primary" }
                        });
                    },
                    complete: function () {
                        if (loader) $(loader).css('visibility', 'hidden');
                    }
                });
            });
        });
    };

    // Suppression AJAX
    var handleDeleteRows = function () {
        const deleteButtons = document.querySelectorAll('[data-kt-prescripteur-table-filter="delete_row"]');
        deleteButtons.forEach(d => {
            d.addEventListener('click', function (e) {
                e.preventDefault();
                const parent = e.target.closest('tr');
                const itemName = parent.querySelectorAll('td')[1].innerText;
                Swal.fire({
                    text: "Voulez-vous vraiment supprimer " + itemName + " ?",
                    icon: "warning",
                    showCancelButton: true,
                    buttonsStyling: false,
                    confirmButtonText: "Oui, supprimer!",
                    cancelButtonText: "Non, annuler",
                    customClass: {
                        confirmButton: "kt-btn fw-bold kt-btn-destructive",
                        cancelButton: "kt-btn fw-bold kt-btn-active-light-primary"
                    }
                }).then(function (result) {
                    if (result.value) {
                        let id = $(d).data('id');
                        console.log('Suppression prescripteur id :', id); // LOG
                        // RÃ©cupÃ©rer le token anti-forgery
                        var token = $('input[name="__RequestVerificationToken"]').val();
                        $.ajax({
                            url: '/Prescripteur/DeleteAjax',
                            type: 'POST',
                            data: { id: id, __RequestVerificationToken: token },
                            success: function (response) {
                                console.log('RÃ©ponse suppression :', response); // LOG
                                if (response.success) {
                                    datatable.row(parent).remove().draw();
                                    Swal.fire({
                                        text: "Prescripteur supprimÃ©(e) avec succÃ¨s!",
                                        icon: "success",
                                        buttonsStyling: false,
                                        confirmButtonText: "OK",
                                        customClass: { confirmButton: "btn fw-bold btn-primary" }
                                    });
                                } else {
                                    Swal.fire({
                                        text: response.message || "Erreur lors de la suppression.",
                                        icon: "error",
                                        buttonsStyling: false,
                                        confirmButtonText: "OK",
                                        customClass: { confirmButton: "btn fw-bold btn-primary" }
                                    });
                                }
                            },
                            error: function (xhr, status, error) {
                                console.log('Erreur AJAX suppression :', status, error, xhr.responseText); // LOG
                                Swal.fire({
                                    text: "Erreur lors de la suppression.",
                                    icon: "error",
                                    buttonsStyling: false,
                                    confirmButtonText: "OK",
                                    customClass: { confirmButton: "btn fw-bold btn-primary" }
                                });
                            }
                        });
                    }
                });
            });
        });
    };

    return {
        init: function () {
            console.log('KTPrescripteurList.init appelÃ©'); // LOG
            table = document.querySelector('#kt_table_prescripteur_index');
            if (!table) return;
            loader = document.querySelector('#kt_spinner_prescripteur_index');
            console.log('initTable appelÃ©'); // LOG
            initTable();
            handleSearchDatatable();
            handlePrint();
            handleDeleteRows();
        }
    };
}();

document.addEventListener('DOMContentLoaded', function () {
    if (typeof KTPrescripteurList !== 'undefined') {
        KTPrescripteurList.init();
    }
});
