"use strict";

var KTValidationresultatList = function () {
    var table;
    var datatable;

    var initValidationresultatTable = function () {
        datatable = $(table).DataTable({
            ordering: false,
            searching: true,
            scrollX: true,
            order: [],
            pageLength: 10,
            columnDefs: [
                {
                    targets: [8], // colonne actions
                    className: 'dt-head-center dt-body-center'
                }
            ]
        });

        // Cacher la barre de recherche native
        $('.dt-search').css('display', 'none');
    };

    var handleSearchDatatable = function () {
        const filterSearch = document.querySelector('input[data-kt-validationresultat-table-filter="search"]');
        if (filterSearch) {
            filterSearch.addEventListener('keyup', function (e) {
                datatable.search(e.target.value).draw();
            });
        }
    };

    var handleCheckboxes = function () {
        $('#selectAllCheckbox').on('change', function () {
            var checked = $(this).is(':checked');
            $('.row-checkbox').prop('checked', checked);
        });
        $('.row-checkbox').on('change', function () {
            if (!$(this).is(':checked')) {
                $('#selectAllCheckbox').prop('checked', false);
            } else if ($('.row-checkbox:checked').length === $('.row-checkbox').length) {
                $('#selectAllCheckbox').prop('checked', true);
            }
        });
    };

    return {
        init: function () {
            table = document.querySelector('#validationresultat-table');
            if (!table) return;
            initValidationresultatTable();
            handleSearchDatatable();
            handleCheckboxes();
        }
    };
}();

$(document).ready(function() {
    // Activer/désactiver le bouton selon la sélection
    function updateValiderEnLotBtn() {
        const checked = $("#validationresultat-table .row-checkbox:checked").length > 0;
        $("#valider-en-lot-btn").prop("disabled", !checked);
    }
    $(document).on("change", ".row-checkbox, #selectAllCheckbox", updateValiderEnLotBtn);

    // Gestion du select all
    $(document).on('change', '#selectAllCheckbox', function() {
        var checked = $(this).is(':checked');
        $("#validationresultat-table .row-checkbox").prop('checked', checked).trigger('change');
    });

    // Clic sur le bouton Valider en lot
    $("#valider-en-lot-btn").on("click", function(e) {
        e.preventDefault();
        const ids = $("#validationresultat-table .row-checkbox:checked").map(function() { return $(this).val(); }).get();
        if (ids.length === 0) return;
        Swal.fire({
            title: 'Valider en lot',
            text: 'Êtes-vous sûr de vouloir valider en lot les résultats sélectionnés ? Cette action est irréversible.',
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Oui, valider',
            cancelButtonText: 'Annuler'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/Validationresultat/ValiderEnLot',
                    type: 'GET',
                    data: { ids: ids },
                    traditional: true,
                    success: function(response) {
                        if (response.success) {
                            Swal.fire({
                                icon: 'success',
                                title: 'Succès',
                                text: response.message
                            }).then(() => location.reload());
                        } else {
                            Swal.fire({
                                icon: 'error',
                                title: 'Erreur',
                                text: response.message || 'Une erreur est survenue.'
                            });
                        }
                    },
                    error: function() {
                        Swal.fire({
                            icon: 'error',
                            title: 'Erreur',
                            text: 'Erreur lors de la validation en lot.'
                        });
                    }
                });
            }
        });
    });
}); 