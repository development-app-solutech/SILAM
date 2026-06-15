"use strict";

var KTAnalyseAvecOngletTabs = function () {

    // Variables globales
    var parametreIndex = 0;
    var valeurReferenceIndex = 0;
    var tarifAssuranceIndex = 0;

    // Fonction pour gérer la navigation par onglets
    var handleTabNavigation = function () {
        const tabLinks = document.querySelectorAll('.tab-link');
        const tabContents = document.querySelectorAll('.tab-content');

        tabLinks.forEach(function (link) {
            link.addEventListener('click', function (e) {
                e.preventDefault();
                const targetTab = this.getAttribute('data-tab');

                // Enlever la classe active de tous les liens et contenus
                tabLinks.forEach(function (l) { l.classList.remove('active'); });
                tabContents.forEach(function (c) { c.classList.remove('active'); });

                // Ajouter la classe active au lien et au contenu cibles
                this.classList.add('active');
                document.getElementById(targetTab).classList.add('active');
            });
        });
    };

    // Fonctions pour l'onglet Paramètres
    var initParametresTab = function () {
        // Ajouter un nouveau paramètre
        const addParametreBtn = document.querySelector('#addParametreBtn');
        if (addParametreBtn) {
            addParametreBtn.addEventListener('click', function () {
                addParametreRow();
            });
        }

        // Gérer la suppression des paramètres existants
        handleParametreDelete();
    };

    var addParametreRow = function () {
        const tableBody = document.querySelector('#parametresTableBody');
        const newRow = document.createElement('tr');

        newRow.innerHTML = `
            <td>
                <input type="text" name="Parametres[${parametreIndex}].Nom" class="form-control" placeholder="Nom du paramètre" />
            </td>
            <td>
                <select name="Parametres[${parametreIndex}].Codeunite" class="form-control">
                    <option value="">---</option>
                    ${getUniteOptions()}
                </select>
            </td>
            <td>
                <select name="Parametres[${parametreIndex}].Codeunitesi" class="form-control">
                    <option value="">---</option>
                    ${getUniteOptions()}
                </select>
            </td>
            <td>
                <input type="number" name="Parametres[${parametreIndex}].Facteurconversion" class="form-control" step="0.01" />
            </td>
            <td>
                <input type="number" name="Parametres[${parametreIndex}].Decimale" class="form-control" />
            </td>
            <td>
                <input type="number" name="Parametres[${parametreIndex}].Decimalesi" class="form-control" />
            </td>
            <td>
                <input type="checkbox" name="Parametres[${parametreIndex}].Masquer" value="true" class="form-check-input" />
                <input type="hidden" name="Parametres[${parametreIndex}].Masquer" value="false" />
            </td>
            <td>
                <input type="number" name="Parametres[${parametreIndex}].Ordre" class="form-control" />
            </td>
            <td>
                <button type="button" class="btn btn-sm btn-danger remove-parametre-btn">
                    <i class="fas fa-trash"></i>
                </button>
            </td>
        `;

        tableBody.appendChild(newRow);
        parametreIndex++;

        // Ajouter l'événement de suppression au nouveau bouton
        newRow.querySelector('.remove-parametre-btn').addEventListener('click', function () {
            newRow.remove();
        });
    };

    var handleParametreDelete = function () {
        document.addEventListener('click', function (e) {
            if (e.target.classList.contains('remove-parametre-btn') || e.target.closest('.remove-parametre-btn')) {
                const row = e.target.closest('tr');
                row.remove();
            }
        });
    };

    var getUniteOptions = function () {
        const uniteSelect = document.querySelector('[name="Codeunite"]');
        if (uniteSelect) {
            let options = '';
            for (let i = 0; i < uniteSelect.options.length; i++) {
                const option = uniteSelect.options[i];
                options += `<option value="${option.value}">${option.text}</option>`;
            }
            return options;
        }
        return '';
    };

    // Fonctions pour l'onglet Valeurs de référence
    var initValeursReferenceTab = function () {
        const addValeurBtn = document.querySelector('#addValeurReferenceBtn');
        if (addValeurBtn) {
            addValeurBtn.addEventListener('click', function () {
                addValeurReferenceRow();
            });
        }

        handleValeurReferenceDelete();
    };

    var addValeurReferenceRow = function () {
        const tableBody = document.querySelector('#valeursReferenceTableBody');
        const newRow = document.createElement('tr');

        newRow.innerHTML = `
            <td>
                <select name="ValeursReference[${valeurReferenceIndex}].Sexe" class="form-control">
                    <option value="">---</option>
                    <option value="M">Masculin</option>
                    <option value="F">Féminin</option>
                    <option value="T">Tout</option>
                </select>
            </td>
            <td>
                <input type="number" name="ValeursReference[${valeurReferenceIndex}].Agemin" class="form-control" min="0" />
            </td>
            <td>
                <input type="number" name="ValeursReference[${valeurReferenceIndex}].Agemax" class="form-control" min="0" />
            </td>
            <td>
                <input type="number" name="ValeursReference[${valeurReferenceIndex}].Valeurmin" class="form-control" step="0.01" />
            </td>
            <td>
                <input type="number" name="ValeursReference[${valeurReferenceIndex}].Valeurmax" class="form-control" step="0.01" />
            </td>
            <td>
                <input type="number" name="ValeursReference[${valeurReferenceIndex}].Valeurcritique" class="form-control" step="0.01" />
            </td>
            <td>
                <textarea name="ValeursReference[${valeurReferenceIndex}].Commentaire" class="form-control" rows="2"></textarea>
            </td>
            <td>
                <button type="button" class="btn btn-sm btn-danger remove-valeur-reference-btn">
                    <i class="fas fa-trash"></i>
                </button>
            </td>
        `;

        tableBody.appendChild(newRow);
        valeurReferenceIndex++;

        newRow.querySelector('.remove-valeur-reference-btn').addEventListener('click', function () {
            newRow.remove();
        });
    };

    var handleValeurReferenceDelete = function () {
        document.addEventListener('click', function (e) {
            if (e.target.classList.contains('remove-valeur-reference-btn') || e.target.closest('.remove-valeur-reference-btn')) {
                const row = e.target.closest('tr');
                row.remove();
            }
        });
    };

    // Fonctions pour l'onglet Tarifs d'assurance
    var initTarifsAssuranceTab = function () {
        const addTarifBtn = document.querySelector('#addTarifAssuranceBtn');
        if (addTarifBtn) {
            addTarifBtn.addEventListener('click', function () {
                addTarifAssuranceRow();
            });
        }

        handleTarifAssuranceDelete();
    };

    var addTarifAssuranceRow = function () {
        const tableBody = document.querySelector('#tarifsAssuranceTableBody');
        const newRow = document.createElement('tr');

        newRow.innerHTML = `
            <td>
                <select name="TarifsAssurance[${tarifAssuranceIndex}].Idassurance" class="form-control">
                    <option value="">---</option>
                    ${getAssuranceOptions()}
                </select>
            </td>
            <td>
                <input type="number" name="TarifsAssurance[${tarifAssuranceIndex}].Tarif" class="form-control" step="0.01" min="0" />
            </td>
            <td>
                <input type="date" name="TarifsAssurance[${tarifAssuranceIndex}].Dateapplication" class="form-control" />
            </td>
            <td>
                <textarea name="TarifsAssurance[${tarifAssuranceIndex}].Commentaire" class="form-control" rows="2"></textarea>
            </td>
            <td>
                <button type="button" class="btn btn-sm btn-danger remove-tarif-assurance-btn">
                    <i class="fas fa-trash"></i>
                </button>
            </td>
        `;

        tableBody.appendChild(newRow);
        tarifAssuranceIndex++;

        newRow.querySelector('.remove-tarif-assurance-btn').addEventListener('click', function () {
            newRow.remove();
        });
    };

    var handleTarifAssuranceDelete = function () {
        document.addEventListener('click', function (e) {
            if (e.target.classList.contains('remove-tarif-assurance-btn') || e.target.closest('.remove-tarif-assurance-btn')) {
                const row = e.target.closest('tr');
                row.remove();
            }
        });
    };

    var getAssuranceOptions = function () {
        // Récupérer les options depuis le ViewBag ou une source de données
        const assuranceSelects = document.querySelectorAll('select[name*="Idassurance"]');
        if (assuranceSelects.length > 0) {
            // Essayer de trouver un select existant pour copier les options
            for (let select of assuranceSelects) {
                if (select.options.length > 0) {
                    let options = '';
                    for (let i = 0; i < select.options.length; i++) {
                        const option = select.options[i];
                        options += `<option value="${option.value}">${option.text}</option>`;
                    }
                    return options;
                }
            }
        }

        // Fallback: utiliser les données du ViewBag si disponibles
        if (typeof assuranceOptions !== 'undefined') {
            let options = '<option value="">---</option>';
            assuranceOptions.forEach(function (assurance) {
                options += `<option value="${assurance.value}">${assurance.text}</option>`;
            });
            return options;
        }

        return '<option value="">---</option>';
    };

    // Initialisation des indices pour l'édition
    var initExistingRows = function () {
        const parametreRows = document.querySelectorAll('#parametresTableBody tr');
        parametreIndex = parametreRows.length;

        const valeurReferenceRows = document.querySelectorAll('#valeursReferenceTableBody tr');
        valeurReferenceIndex = valeurReferenceRows.length;

        const tarifAssuranceRows = document.querySelectorAll('#tarifsAssuranceTableBody tr');
        tarifAssuranceIndex = tarifAssuranceRows.length;
    };

    // Fonction d'initialisation publique
    return {
        init: function () {
            handleTabNavigation();
            initParametresTab();
            initValeursReferenceTab();
            initTarifsAssuranceTab();
            initExistingRows();
        }
    };
}();

// Initialisation quand le DOM est prêt
//document.addEventListener('DOMContentLoaded', function () {
//    KTAnalyseAvecOngletTabs.init();
//});
