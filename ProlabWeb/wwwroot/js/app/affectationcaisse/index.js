"use strict";

var KTAffectationcaisseList = function () {
	var table;
	var datatable;
	var loader;

	// Initialisation du DataTable
	var initAffectationcaisseTable = function () {
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
		const filterSearch = document.querySelector('[data-kt-affectationcaisse-table-filter="search"]');
		if (filterSearch) {
			filterSearch.addEventListener('keyup', function (e) {
				const searchTerm = e.target.value;
				if ($.fn.DataTable.isDataTable(table)) {
					$(table).DataTable().search(searchTerm).draw();
				}
			});
		}
	};

	// Suppression AJAX (écoute en phase de capture pour éviter le blocage par le menu)
    var handleDeleteRows = function () {
        console.log('handleDeleteRows: écoute en capture activée'); // DEBUG

        document.addEventListener('click', function (e) {
            const trigger = e.target.closest('[data-kt-affectationcaisses-table-filter="delete_row"]');
            if (!trigger) return;

            console.log('CLIC détecté (capture) sur Dissocier'); // DEBUG
            e.preventDefault();
            e.stopPropagation();

            const parent = trigger.closest('tr');
            console.log('Parent tr:', parent); // DEBUG
            const caisseName = parent?.querySelectorAll('td')[0]?.innerText || '';
            console.log('Nom caisse:', caisseName); // DEBUG

            Swal.fire({
                text: "Voulez-vous vraiment dissocier cette affectation de la caisse " + caisseName + " ?",
                icon: "warning",
                showCancelButton: true,
                buttonsStyling: false,
                confirmButtonText: "Oui",
                cancelButtonText: "Non",
                customClass: {
					confirmButton: "btn btn-danger fw-bold px-4 py-2",
					cancelButton: "btn btn-secondary fw-bold px-4 py-2"
                }
            }).then(function (result) {
                if (result.isConfirmed) {
                    const id = $(trigger).data('id');
                    console.log('ID récupéré:', id); // DEBUG

                    $.ajax({
                        url: '/Affectationcaisse/DeleteAjax',
                        type: 'POST',
                        data: { id: id },
                        beforeSend: function () {
                            if (loader) $(loader).css('visibility', 'visible');
                        },
                        success: function (response) {
                            if (response.success) {
                                if ($.fn.DataTable.isDataTable(table)) {
                                    $(table).DataTable().row(parent).remove().draw();
                                } else {
                                    parent?.remove();
                                }
                                Swal.fire({
                                    text: "Dissociation  réussie!",
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
                            console.error('Erreur AJAX:', status, error, xhr.responseText);
                            Swal.fire({
                                text: xhr.responseJSON?.message || xhr.responseText || "Erreur lors de la dissociation.",
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
        }, true); // <-- phase de capture
    };

	return {
		init: function () {
			table = document.querySelector('#kt_table_affectationcaisse_index');
			if (!table) return;
			loader = document.querySelector('#kt_spinner_affectationcaisse_index');
			initAffectationcaisseTable();
			handleSearchDatatable();
			handleDeleteRows();
		},
		// Appel direct depuis l'attribut onclick du lien "Dissocier"
		deleteRow: function (trigger, e) {
			if (e) { e.preventDefault(); e.stopPropagation(); }
			try {
				const parent = trigger.closest('tr');
				const caisseName = parent?.querySelectorAll('td')[0]?.innerText || '';
				const id = $(trigger).data('id');

				Swal.fire({
					text: "Voulez-vous vraiment dissocier cette affectation de caisse " + caisseName + " ?",
					icon: "warning",
					showCancelButton: true,
					buttonsStyling: false,
					confirmButtonText: "Oui",
					cancelButtonText: "Non",
				customClass: {
						confirmButton: "btn btn-danger fw-bold px-4 py-2",
						cancelButton: "btn btn-secondary fw-bold px-4 py-2"
					}
				}).then(function (result) {
					if (!result.isConfirmed) return;

					$.ajax({
						url: '/Affectationcaisse/DeleteAjax',
						type: 'POST',
						data: { id: id },
						beforeSend: function () { if (loader) $(loader).css('visibility', 'visible'); },
						success: function (response) {
							if (response.success) {
								if ($.fn.DataTable.isDataTable(table)) {
									$(table).DataTable().row(parent).remove().draw();
								} else { parent?.remove(); }
								Swal.fire({ text: "ADissociation réussie!", icon: "success", buttonsStyling: false, confirmButtonText: "OK", customClass: { confirmButton: "btn fw-bold btn-primary" } });
							} else {
								Swal.fire({ text: response.message || "Erreur lors de la dissociation.", icon: "error", buttonsStyling: false, confirmButtonText: "OK", customClass: { confirmButton: "btn fw-bold btn-primary" } });
							}
						},
						error: function (xhr, status, error) {
							console.error('Erreur AJAX:', status, error, xhr.responseText);
							Swal.fire({ text: xhr.responseJSON?.message || xhr.responseText || "Erreur lors de la dissociation.", icon: "error", buttonsStyling: false, confirmButtonText: "OK", customClass: { confirmButton: "btn fw-bold btn-primary" } });
						},
						complete: function () { if (loader) $(loader).css('visibility', 'hidden'); }
					});
				});
			} catch (err) {
				console.error('deleteRow error:', err);
			}
		}
	};
}();


document.addEventListener('DOMContentLoaded', function () {
	if (typeof KTAffectationcaisseList !== 'undefined') {
		KTAffectationcaisseList.init();
	}
});