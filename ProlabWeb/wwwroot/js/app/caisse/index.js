"use strict";

var KTCaisseList = function () {
    var table;
    var datatable;
    var loader;

    // Initialisation du DataTable
    var initCaisseTable = function () {
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
        console.log('DataTable initialisé ?', $.fn.DataTable.isDataTable(table), datatable); // LOG
    };

    // Recherche dynamique
    var handleSearchDatatable = function () {
        const filterSearch = document.querySelector('[data-kt-caisse-table-filter="search"]');
        if (filterSearch) {
            console.log("🔍 Champ de recherche détecté !");
            filterSearch.addEventListener('keyup', function (e) {
                const searchTerm = e.target.value;
                console.log('🔍 Terme recherché :', searchTerm);
                if ($.fn.DataTable.isDataTable(table)) {
                    $(table).DataTable().search(searchTerm).draw();
                }
            });
        } else {
            console.warn("⚠️ Champ de recherche non trouvé !");
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

    var handlePrintCaisse = function () {
        const printLinks = document.querySelectorAll('[data-kt-list-activity-table-print="print_table"]');
        printLinks.forEach(d => {
            d.addEventListener('click', function (e) {
                e.preventDefault();
                $.ajax({
                    url: '/Caisse/PrintList',
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
        const deleteButtons = document.querySelectorAll('[data-kt-caisse-table-filter="delete_row"]');
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
                        console.log('Suppression caisse id :', id); // LOG
                        // Récupérer le token anti-forgery
                        var token = $('input[name="__RequestVerificationToken"]').val();
                        $.ajax({
                            url: '/Caisse/DeleteAjax',
                            type: 'POST',
                            data: { id: id, __RequestVerificationToken: token },
                            success: function (response) {
                                console.log('Réponse suppression :', response); // LOG
                                if (response.success) {
                                    datatable.row(parent).remove().draw();
                                    Swal.fire({
                                        text: "Caisse supprimée avec succès!",
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
            console.log('KTCaisseList.init appelé'); // LOG
            table = document.querySelector('#kt_table_caisse_index');
            if (!table) return;
            loader = document.querySelector('#kt_spinner_caisse_index');
            console.log('initCaisseTable appelé'); // LOG
            initCaisseTable();
            handleSearchDatatable();
            handlePrintCaisse();
            handleDeleteRows();
        }
    };
}();

document.addEventListener('DOMContentLoaded', function () {
    if (typeof KTCaisseList !== 'undefined') {
        KTCaisseList.init();
    }
});
