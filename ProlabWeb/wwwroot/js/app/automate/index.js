"use strict";

var KTAutomateList = function () {
    var table;
    var datatable;
    var loader;

    // Initialisation du DataTable
    var initAutomateTable = function () {
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
        const filterSearch = document.querySelector('[data-kt-automate-table-filter="search"]');
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

    // Suppression AJAX
    var handleDeleteRows = function () {
        const deleteButtons = document.querySelectorAll('[data-kt-automates-table-filter="delete_row"]');
        deleteButtons.forEach(d => {
            d.addEventListener('click', function (e) {
                e.preventDefault();
                const parent = e.target.closest('tr');
                const automateName = parent.querySelectorAll('td')[1].innerText;
                Swal.fire({
                    text: "Voulez-vous vraiment supprimer " + automateName + " ?",
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
                        console.log('Suppression automate id :', id); // LOG
                        // Récupérer le token anti-forgery
                        var token = $('input[name="__RequestVerificationToken"]').val();
                        $.ajax({
                            url: '/Automate/DeleteAjax',
                            type: 'POST',
                            data: { id: id, __RequestVerificationToken: token },
                            beforeSend: function () {
                                if (loader) $(loader).css('visibility', 'visible');
                            },
                            success: function (response) {
                                console.log('Réponse suppression :', response); // LOG
                                if (response.success) {
                                    datatable.row(parent).remove().draw();
                                    Swal.fire({
                                        text: "Automate supprimé avec succès!",
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
                            },
                            complete: function () {
                                if (loader) $(loader).css('visibility', 'hidden');
                            }
                        });
                    }
                });
            });
        });
    };

    return {
        init: function () {
            console.log('KTAutomateList.init appelé'); // LOG
            table = document.querySelector('#kt_table_automate_index');
            if (!table) return;
            loader = document.querySelector('#kt_spinner_automate_index');
            console.log('initAutomateTable appelé'); // LOG
            initAutomateTable();
            handleSearchDatatable();
            handleDeleteRows();
        }
    };
}();

document.addEventListener('DOMContentLoaded', function () {
    if (typeof KTAutomateList !== 'undefined') {
        KTAutomateList.init();
    }
}); 