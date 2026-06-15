// Clean rewrite of Enteteresultat Create JavaScript
// Enhanced initialization with better library detection and loading
$(document).ready(function() {
console.log('Enteteresultat Create.js loading...');

// Setup AJAX to automatically include anti-forgery token
$(document).ready(function() {
    $.ajaxSetup({
        beforeSend: function(xhr, settings) {
            // Add anti-forgery token to all AJAX requests
            if (settings.type !== 'GET') {
                var token = $('input[name="__RequestVerificationToken"]').val();
                if (token && settings.data) {
                    if (typeof settings.data === 'string') {
                        settings.data += '&__RequestVerificationToken=' + encodeURIComponent(token);
                    } else if (typeof settings.data === 'object') {
                        settings.data.__RequestVerificationToken = token;
                    }
                }
            }
        }
    });
});

    
    // Check if required libraries are available
    if (typeof $ === 'undefined') {
        console.error('jQuery is not loaded!');
        return;
    }
    
    // Enhanced library loading check with retry mechanism
    let initAttempts = 0;
    const maxAttempts = 10;
    const attemptDelay = 200;
    
    function attemptInitialization() {
        initAttempts++;
        console.log('Initialization attempt ' + initAttempts + '/' + maxAttempts);
        
        // Check for Select2
        const select2Available = typeof $.fn.select2 !== 'undefined';
        // Check for SweetAlert2
        const sweetAlertAvailable = typeof Swal !== 'undefined';
        
        console.log('Library status:', {
            select2: select2Available,
            sweetAlert: sweetAlertAvailable,
            jquery: typeof $ !== 'undefined'
        });
        
        if (!select2Available && initAttempts < maxAttempts) {
            console.log('Select2 not ready, retrying in ' + attemptDelay + 'ms...');
            setTimeout(attemptInitialization, attemptDelay);
            return;
        }
        
        if (!select2Available) {
            console.error('Select2 is not loaded after ' + maxAttempts + ' attempts! Continuing without Select2...');
        }
        
        if (!sweetAlertAvailable) {
            console.warn('SweetAlert2 is not loaded!');
        }
        
        console.log('Initializing Enteteresultat Create page...');
        // Initialize the page
        initializePage();
    }
    
    // Start initialization attempts
    attemptInitialization();
});

function initializePage() {
    try {
        console.log('Initializing page...');
        
        // Initialize Select2 dropdowns
        initializeSelect2();
        
        // Setup event handlers
        setupEventHandlers();
        
        // Initialize checkbox functionality
        initializeCheckboxes();
        
        console.log('Page initialization complete');
    } catch (error) {
        console.error('Error initializing page:', error);
    }
}

function initializeCheckboxes() {
    // Gestion de la case à cocher "Sélectionner tout"
    $('#selectAllCheckbox').on('change', function() {
        var isChecked = $(this).is(':checked');
        $('.row-checkbox').prop('checked', isChecked);
    });

    // Gestion des cases à cocher individuelles
    $('.row-checkbox').on('change', function() {
        var totalCheckboxes = $('.row-checkbox').length;
        var checkedCheckboxes = $('.row-checkbox:checked').length;
        
        // Si toutes les cases sont cochées, cocher la case "Sélectionner tout"
        if (checkedCheckboxes === totalCheckboxes) {
            $('#selectAllCheckbox').prop('checked', true);
        }
        // Si au moins une case n'est pas cochée, décocher la case "Sélectionner tout"
        else {
            $('#selectAllCheckbox').prop('checked', false);
        }
    });
}

function initializeSelect2() {
    console.log('Initializing Select2...');
    
    try {
        // Vérifier si Select2 est disponible
        if (typeof $.fn.select2 === 'undefined') {
            console.warn('Select2 not available, falling back to basic dropdowns');
            return;
        }

        // Configuration Select2 pour le champ Entetedemande
        $('.select2-demande').select2({
            placeholder: "Sélectionner une demande...",
            allowClear: true,
            width: '100%',
            templateResult: function(data) {
                if (data.loading) return data.text;
                if (data.id === '') return data.text; // Option "---"
                
                // Afficher seulement le contenu des données, sans header
                var $option = $('<div class="select2-option-content" style="padding: 5px; font-size: 12px;"></div>');
                $option.html(data.text);
                
                return $option;
            },
            templateSelection: function(data) {
                if (data.id === '') return data.text;
                // Utiliser le texte de sélection stocké dans data.group
                if (data.group) {
                    return data.group;
                }
                // Fallback : extraire les données du HTML si group n'est pas disponible
                if (data.text && data.text.includes('<table>')) {
                    try {
                        var tempDiv = $('<div>').html(data.text);
                        var cells = tempDiv.find('td');
                        if (cells.length >= 5) {
                            var numero = cells.eq(0).text().trim();
                            var date = cells.eq(1).text().trim();
                            var patient = cells.eq(2).text().trim();
                            
                            // Retourner un format lisible : "Numéro - Patient - Date"
                            return numero + ' - ' + patient + ' - ' + date;
                        }
                    } catch (e) {
                        console.log('Erreur lors de l\'extraction des données:', e);
                    }
                }
                // Si ce n'est pas du HTML ou en cas d'erreur, retourner le texte tel quel
                return data.text || '';
            }
        });

        // Ajouter le header une seule fois en haut de la liste déroulante
        $('.select2-demande').on('select2:open', function() {
            setTimeout(function() {
                var $dropdown = $('.select2-results__options');
                if ($dropdown.length > 0 && !$dropdown.find('.select2-header-fixed').length) {
                    var headerHtml = '<table style="width:100%; font-family:monospace; font-size:12px;"><tr>' +
                                    '<td style="width:12%;">Numéro</td>' +
                                    '<td style="width:12%;">Date</td>' +
                                    '<td style="width:35%;">Patient</td>' +
                                    '<td style="width:25%;">Prescripteur</td>' +
                                    '<td style="width:16%;">Police Assur.</td>' +
                                    '</tr></table>';
                    var $header = $('<div class="select2-header-fixed" style="font-weight: bold; padding: 5px; background-color: #f8f9fa; border-bottom: 1px solid #dee2e6; font-size: 12px; font-family: monospace; position: sticky; top: 0; z-index: 10;"></div>');
                    $header.html(headerHtml);
                    $dropdown.prepend($header);
                }
            }, 10);
        });

        // Activer select2 sur tous les <select> avec la classe .kt-select
        $('.kt-select').select2({
            placeholder: "Sélectionner...",
            allowClear: true,
            width: '100%'
        });
        
        console.log('Select2 initialization complete');
    } catch (error) {
        console.error('Error initializing Select2:', error);
        console.warn('Falling back to basic dropdowns');
    }
}

function setupEventHandlers() {
    console.log('Setting up event handlers...');
    
    // AJAX pour charger les analyses ET les détails du patient selon la demande sélectionnée
    // CONSOLIDÉ: Un seul gestionnaire d'événement pour éviter les conflits
    $('[name="Entetedemandeid"]').on('change', function() {
        console.log('Entetedemandeid changed:', $(this).val());
        var demandeId = $(this).val();
        var laboratoireId = $('[name="Laboratoireid"]').val();
        var $analyseSelect = $('[name="Idanalyse"]');
        
        // Reset analyze selection
        $analyseSelect.empty();
        $analyseSelect.append($('<option>', { value: '', text: '---' }));
        
        if (!demandeId) {
            console.log('No demande selected, clearing fields');
            if (typeof $.fn.select2 !== 'undefined' && $analyseSelect.hasClass('kt-select')) {
                $analyseSelect.trigger('change.select2');
            }
            $analyseSelect.trigger('change');
            
            // Clear patient details
            $('input[name="Age"]').val('');
            $('input[name="Sexe"]').val('');
            $('input[name="Peau"]').val('');
            return;
        }
        
        console.log('Loading analyses for demande:', demandeId, 'laboratoire:', laboratoireId);
        $.ajax({
            url: '/Entetedemande/GetAnalyse',
            type: 'GET',
            data: { id: demandeId, laboratoireId: laboratoireId },
            success: function(response) {
                console.log('GetAnalyse response:', response);
                $analyseSelect.empty();
                $analyseSelect.append($('<option>', { value: '', text: '---' }));
                if (response.success && response.data) {
                    var analyses = [];
                    try {
                        analyses = JSON.parse(response.data);
                        console.log('Parsed analyses:', analyses);
                    } catch (e) {
                        console.error('Error parsing analyses JSON:', e);
                        analyses = [];
                    }
                    if (Array.isArray(analyses)) {
                        analyses.forEach(function(analyse) {
                            var text = analyse.Nom;
                            if (analyse.NomCategorie && analyse.NomCategorie !== "") {
                                text += ' (' + analyse.NomCategorie + ')';
                            }
                            $analyseSelect.append($('<option>', { value: analyse.Id, text: text }));
                        });
                        console.log('Added', analyses.length, 'analyses to select');
                    } else {
                        console.warn('Analyses is not an array:', analyses);
                    }
                } else {
                    console.warn('GetAnalyse failed or no data:', response);
                }
                // Réinitialiser Select2 si nécessaire
                if (typeof $.fn.select2 !== 'undefined' && $analyseSelect.hasClass('kt-select')) {
                    $analyseSelect.trigger('change.select2');
                }
                $analyseSelect.trigger('change');
            },
            error: function(xhr, status, error) {
                console.error('GetAnalyse AJAX error:', status, error, xhr.responseText);
                $analyseSelect.empty();
                $analyseSelect.append($('<option>', { value: '', text: 'Erreur lors du chargement' }));
                // Réinitialiser Select2 si nécessaire
                if (typeof $.fn.select2 !== 'undefined' && $analyseSelect.hasClass('kt-select')) {
                    $analyseSelect.trigger('change.select2');
                }
                $analyseSelect.trigger('change');
            }
        });
        
        // Load patient details
        console.log('Loading patient details for demande:', demandeId);
        $.ajax({
            url: '/Entetedemande/GetPatientDetails', 
            type: 'GET',
            data: { enteteDemandeId: demandeId },
            success: function (response) {
                console.log('GetPatientDetails response:', response);
                if (response.success) {
                    let data = response.data;
                    console.log('Patient data:', data);
                    $('input[name="Age"]').val(data.age);
                    $('input[name="Sexe"]').val(data.sexe);
                    $('input[name="Peau"]').val(data.peau);
                } else {
                    console.warn('GetPatientDetails failed:', response);
                }
            },
            error: function (xhr, status, error) {
                console.error('GetPatientDetails AJAX error:', status, error, xhr.responseText);
                let message = "Erreur lors du chargement des détails du patient.";

                if (xhr.responseJSON && xhr.responseJSON.message) {
                    message = xhr.responseJSON.message;
                }

                console.error(message);

                if (typeof Swal !== 'undefined') {
                    Swal.fire({
                        icon: 'error',
                        title: 'Erreur',
                        text: message,
                        confirmButtonText: 'OK'
                    });
                } else {
                    alert(message);
                }
            }
        });
    });

    // Remplir dynamiquement le tableau des paramètres selon l'analyse sélectionnée
    $('[name="Idanalyse"]').on('change', function() {
        console.log('Idanalyse changed:', $(this).val());
        var analyseId = $(this).val();
        var $tbody = $('.kt-table tbody');
        $tbody.empty();
        if (!analyseId) {
            console.log('No analyse selected, clearing parameters table');
            return;
        }
        console.log('Loading parameters for analyse:', analyseId);
        $.ajax({
            url: '/Parametre/GetParametreByAnalyse',
            type: 'GET',
            data: { id: analyseId },
            success: function(response) {
                console.log('GetParametreByAnalyse response:', response);
                if (response.success && response.data) {
                    var parametres = [];
                    try {
                        parametres = JSON.parse(response.data);
                        console.log('Parsed parameters:', parametres);
                    } catch (e) {
                        console.error('Error parsing parameters JSON:', e);
                        parametres = [];
                    }
                    if (Array.isArray(parametres)) {
                        parametres.forEach(function(param, i) {
                            createParameterRow(param, i, $tbody);
                        });
                        
                        console.log('Added', parametres.length, 'parameter rows to table');

                        // Setup event handlers for result inputs
                        setupResultEventHandlers();

                        // Initialiser Select2 sur les nouveaux selects dynamiques
                        if (typeof $.fn.select2 !== 'undefined') {
                            $('.kt-select').not('.select2-hidden-accessible').select2({
                                placeholder: "Sélectionner...",
                                allowClear: true,
                                width: '100%'
                            });
                            console.log('Select2 initialized for dynamic parameter selects');
                        }
                    } else {
                        console.warn('Parameters is not an array:', parametres);
                    }
                } else {
                    console.warn('GetParametreByAnalyse failed or no data:', response);
                }
            },
            error: function(xhr, status, error) {
                console.error('GetParametreByAnalyse AJAX error:', status, error);
                $tbody.empty();
                
                var errorMessage = 'Impossible de charger les paramètres pour cette analyse.';
                
                // Handle specific error types
                if (xhr.status === 401) {
                    errorMessage = 'Session expirée. Veuillez vous reconnecter.';
                } else if (xhr.status === 403) {
                    errorMessage = 'Accès non autorisé à cette ressource.';
                } else if (xhr.status >= 500) {
                    errorMessage = 'Erreur serveur. Veuillez réessayer plus tard.';
                }
                
                // Show user-friendly error message
                if (typeof Swal !== 'undefined') {
                    Swal.fire({
                        icon: 'error',
                        title: 'Erreur',
                        text: errorMessage,
                        confirmButtonText: 'OK'
                    });
                }
            }
        });
    });

    // Handle laboratory selection change to load analyses
    $('[name="Laboratoireid"]').on('change', function() {
        console.log('Laboratoireid changed:', $(this).val());
        var laboratoireId = $(this).val();
        var demandeId = $('[name="Entetedemandeid"]').val();
        var $analyseSelect = $('[name="Idanalyse"]');

        // If a demande is selected, always load analyses from demande endpoint
        // so we keep only analyses actually requested for that demande.
        if (demandeId) {
            $('[name="Entetedemandeid"]').trigger('change');
            return;
        }
        
        // Reset analyze selection
        $analyseSelect.empty();
        $analyseSelect.append($('<option>', { value: '', text: '---' }));
        
        if (!laboratoireId || laboratoireId === '') {
            console.log('No laboratory selected, clearing analyses');
            if (typeof $.fn.select2 !== 'undefined' && $analyseSelect.hasClass('kt-select')) {
                $analyseSelect.trigger('change.select2');
            }
            $analyseSelect.trigger('change');
            return;
        }
        
        console.log('Loading analyses for laboratory:', laboratoireId);
        $.ajax({
            url: '/Enteteresultat/GetAnalysesByLaboratory',
            type: 'GET',
            data: { laboratoireId: laboratoireId },
            success: function(response) {
                console.log('GetAnalysesByLaboratory response:', response);
                $analyseSelect.empty();
                $analyseSelect.append($('<option>', { value: '', text: '---' }));
                if (response.success && response.data) {
                    var analyses = [];
                    try {
                        analyses = JSON.parse(response.data);
                        console.log('Parsed laboratory analyses:', analyses);
                    } catch (e) {
                        console.error('Error parsing laboratory analyses JSON:', e);
                        analyses = [];
                    }
                    if (Array.isArray(analyses)) {
                        analyses.forEach(function(analyse) {
                            var text = analyse.Nom;
                            $analyseSelect.append($('<option>', { value: analyse.Id, text: text }));
                        });
                        console.log('Added', analyses.length, 'analyses from laboratory to select');
                    } else {
                        console.warn('Laboratory analyses is not an array:', analyses);
                    }
                } else {
                    console.warn('GetAnalysesByLaboratory failed or no data:', response);
                }
                // Réinitialiser Select2 si nécessaire
                if (typeof $.fn.select2 !== 'undefined' && $analyseSelect.hasClass('kt-select')) {
                    $analyseSelect.trigger('change.select2');
                }
                $analyseSelect.trigger('change');
            },
            error: function(xhr, status, error) {
                console.error('GetAnalysesByLaboratory AJAX error:', status, error, xhr.responseText);
                $analyseSelect.empty();
                $analyseSelect.append($('<option>', { value: '', text: 'Erreur lors du chargement' }));
                // Réinitialiser Select2 si nécessaire
                if (typeof $.fn.select2 !== 'undefined' && $analyseSelect.hasClass('kt-select')) {
                    $analyseSelect.trigger('change.select2');
                }
                $analyseSelect.trigger('change');
            }
        });
    });
    
    // On page load, prioritize demande-based loading when a demande is preselected.
    $(document).ready(function() {
        var demandeId = $('[name="Entetedemandeid"]').val();
        var laboratoireId = $('[name="Laboratoireid"]').val();
        if (demandeId && demandeId !== '') {
            console.log('Demande pre-selected on page load, triggering demande change:', demandeId);
            $('[name="Entetedemandeid"]').trigger('change');
        } else if (laboratoireId && laboratoireId !== '') {
            console.log('Laboratory pre-selected on page load, triggering change:', laboratoireId);
            $('[name="Laboratoireid"]').trigger('change');
        }
    });
    
    console.log('Event handlers setup complete');
}

function escapeRegexForFormula(value) {
    return (value || '').toString().replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}

function isSelfVariableInFormula(formula, code) {
    if (!formula || !code) return false;
    const pattern = new RegExp('\\b' + escapeRegexForFormula(code) + '\\b', 'i');
    return pattern.test(formula.toString());
}

function createParameterRow(param, i, $tbody) {
    var uniteSpan = '<span>' + (param.Unite || '') + '</span>';
    var uniteHidden = '<input type="hidden" name="Parametres['+i+'].Unite" value="'+(param.Unite || '')+'" />';
    var uniteSIspan = '<span>' + (param.UniteSI || '') + '</span>';
    var uniteSIHidden = '<input type="hidden" name="Parametres['+i+'].UniteSI" value="'+(param.UniteSI || '')+'" />';
    var nomHidden = '<input type="hidden" name="Parametres['+i+'].Nom" value="'+(param.Nom || '')+'" />';
    var codeHidden = '<input type="hidden" name="Parametres['+i+'].Code" value="'+(param.Code || '')+'" />';
    var paramidHidden = '<input type="hidden" name="Parametres[' + i + '].Parametreid" value="' + (param.Parametreid || '') + '" />';
    
    var builderTypeValue = (param.Builder && param.Builder.Type) ? param.Builder.Type : '';
    var builderValeurValue = (param.Builder && param.Builder.Valeur) ? param.Builder.Valeur : '';
    var builderHidden = '<input type="hidden" name="Parametres[' + i + '].Builder.Type" value="' + builderTypeValue + '" />' +
                       '<input type="hidden" name="Parametres[' + i + '].Builder.Valeur" value="' + builderValeurValue + '" />';
    
    var facteurconversionHidden = '<input type="hidden" name="Parametres[' + i + '].FacteurConversion" value="' + (param.FacteurConversion || '') + '" />';

    var resultatSpan = '';
    var resultatSIspan = '';

    // Handle different parameter types
    if (param.Builder && param.Builder.Type === 'Texte') {
        resultatSpan = '<input class="kt-input" name="Parametres[' + i + '].Resultat" type="number" placeholder="Résultat" />';
        resultatSIspan = '<input class="kt-input" name="Parametres[' + i + '].Resultatsi" type="number" data-facteurconversion="' + (param.FacteurConversion || '') + '" placeholder="Résultat SI" />';
    } else if (param.Builder && param.Builder.Type === 'Liste') {
        var options = '';
        if (param.Builder.Valeur) {
            var optionList = param.Builder.Valeur.split(',');
            for (var j = 0; j < optionList.length; j++) {
                var optValue = optionList[j].trim();
                if (optValue !== '') {
                    options += '<option value="' + optValue + '">' + optValue + '</option>';
                }
            }
        }
        resultatSpan = '<select name="Parametres[' + i + '].Resultat" class="kt-select"><option value="">---</option>' + options + '</select>';
        resultatSIspan = '<select name="Parametres[' + i + '].Resultatsi" class="kt-select"><option value="">---</option>' + options + '</select>';
    } else if (param.Builder && param.Builder.Type === 'Formule') {
        var isSelfFormula = isSelfVariableInFormula((param.Builder.Valeur || ''), (param.Code || ''));
        if (isSelfFormula) {
            resultatSpan = '<input class="kt-input" name="Parametres[' + i + '].Resultat" type="number" data-formule="' + (param.Builder.Valeur || '') + '" placeholder="Valeur brute automate" title="Saisir la valeur brute puis quitter le champ pour calcul automatique" />';
        } else {
            resultatSpan = '<input class="kt-input" name="Parametres[' + i + '].Resultat" type="number" data-formule="' + (param.Builder.Valeur || '') + '" placeholder="Auto-calculé" title="Ce champ est auto-calculé" readonly />';
        }
        resultatSIspan = '<input class="kt-input" name="Parametres[' + i + '].Resultatsi" type="number" data-facteurconversion="' + (param.FacteurConversion || '') + '" readonly />';
    }

    var row = '<tr>' +
                '<td class="font-normal text-foreground">' +
                    paramidHidden +
                    '<div class="flex flex-col">' +
                        '<span class="font-bold">' + (param.Nom || '') + '</span>' + nomHidden +
                        '<span class="text-sm text-gray-500">' + (param.Code || '') + '</span>' + codeHidden +
                    '</div>' +
                '</td>' +
                '<td class="font-normal text-foreground">' +
                    uniteSpan + uniteHidden +
                '</td>' +
                '<td class="font-normal text-foreground">' +
                    resultatSpan + builderHidden +
                '</td>' +
                '<td class="font-normal text-foreground">' +
                    uniteSIspan + uniteSIHidden +
                '</td>' +
                '<td class="font-normal text-foreground">' +
                    resultatSIspan + facteurconversionHidden +
                '</td>' +
                '<td class="font-normal text-foreground">' +
                '<input type="text" class="kt-input commentaire-field" name="Parametres[' + i + '].Commentaire" placeholder="Auto-calculé" />' +
                '</td>' +
            '</tr>';

    $tbody.append(row);
}

function setupResultEventHandlers() {
    // Événements pour les champs de type texte
    $('input[name$=".Resultat"]').not('[data-formule]').off('input').on('input', function () {
        console.log('Champ texte modifié:', $(this).attr('name'), $(this).val());
        
        // 1. Calculer les formules qui dépendent de ce champ
        calculerFormules();
        
        // 2. Calculer les commentaires
        calculerCommentaireAutomatique($(this));
        
        // 3. Calculer les résultats SI
        calculerResultatSI($(this));
    });

    // Événements pour les champs de type formule
    $('input[name$=".Resultat"][data-formule]:not([readonly])').off('blur').on('blur', function () {
        console.log('Champ formule quitté (blur):', $(this).attr('name'), $(this).val());
        
        // Le champ formule accepte une valeur brute ; le calcul final se fait au blur.
        calculerFormules();
    });
}

// Fonction pour calculer le résultat SI à partir du facteur de conversion
function calculerResultatSI($resultatInput) {
    const name = $resultatInput.attr('name');
    const indexMatch = name.match(/Parametres\[(\d+)\]\.Resultat/);
    if (!indexMatch) return;

    const index = indexMatch[1];
    const $resultatSI = $(`input[name="Parametres[${index}].Resultatsi"]`);
    if ($resultatSI.length === 0) return;

    const facteur = parseFloat($resultatSI.attr('data-facteurconversion'));
    const val = parseFloat($resultatInput.val());

    if (!isNaN(val) && !isNaN(facteur) && facteur !== 0) {
        const converted = val * facteur;
        $resultatSI.val(converted.toFixed(2));
        console.log(`Conversion SI: ${val} * ${facteur} = ${converted.toFixed(2)}`);
    } else if ($resultatInput.val() === '') {
        $resultatSI.val('');
    }
}

// Fonction pour calculer les formules
function calculerFormules() {
    console.log('Calcul des formules - début');

    // 1. Collecter les variables globales
    let variables = {
        age: parseFloat($('input[name="Age"]').val()) || 0,
        sexe: $('input[name="Sexe"]').val() ? 1 : 0,
        peau: $('input[name="Peau"]').val() ? 1 : 0,
    };

    console.log('Variables globales:', variables);

    // 2. Collecter les résultats des champs texte (par code)
    $('input[name$=".Resultat"]').not('[data-formule]').each(function () {
        const $input = $(this);
        const name = $input.attr('name');
        const indexMatch = name.match(/Parametres\[(\d+)\]\.Resultat/);
        if (indexMatch) {
            const index = indexMatch[1];
            const code = $(`input[name="Parametres[${index}].Code"]`).val();
            const val = parseFloat($input.val());
            console.log(`Debug param ${index}: code='${code}', val='${$input.val()}', isNaN(val)=${isNaN(val)}`);
            
            if (code) {
                if (!isNaN(val)) {
                    variables[code] = val;
                    console.log(`✅ Ajout variable ${code} = ${val}`);
                } else if ($input.val() !== '') {
                    // Si la valeur n'est pas un nombre mais n'est pas vide, la traiter comme 0
                    variables[code] = 0;
                    console.log(`⚠️ Variable ${code} non numérique, mise à 0`);
                } else {
                    // Champ vide = 0
                    variables[code] = 0;
                    console.log(`📝 Variable ${code} vide, mise à 0`);
                }
            }
        }
    });
    
    // Debug: afficher tous les codes disponibles même ceux sans valeur
    $('input[name$=".Code"]').each(function() {
        const code = $(this).val();
        const name = $(this).attr('name');
        const indexMatch = name.match(/Parametres\[(\d+)\]\.Code/);
        if (indexMatch) {
            const index = indexMatch[1];
            console.log(`📋 Paramètre ${index}: code='${code}' (dans variables: ${code in variables})`);
        }
    });

    console.log('Variables complètes:', variables);

    function buildCaseInsensitiveVariables(source) {
        const map = {};
        Object.keys(source).forEach(function (key) {
            map[key.toLowerCase()] = source[key];
        });
        return map;
    }

    function normalizeFormula(rawFormula) {
        if (!rawFormula) return '';
        const text = rawFormula.toString().trim();
        const eqIndex = text.indexOf('=');
        // Supporte les formules du type "L=(H*L)/100" en évaluant seulement la partie droite.
        return eqIndex >= 0 ? text.substring(eqIndex + 1).trim() : text;
    }

    // Figer la valeur brute saisie des champs formule pendant le cycle de calcul.
    // Cela évite les auto-réapplications successives (ex: 5 -> 0.40 -> 0.03 -> 0.00).
    const rawFormulaValuesByIndex = {};
    $('input[name$=".Resultat"][data-formule]').each(function () {
        const $input = $(this);
        const name = $input.attr('name') || '';
        const indexMatch = name.match(/Parametres\[(\d+)\]\.Resultat/);
        if (!indexMatch) return;

        const index = indexMatch[1];
        const rawValue = parseFloat($input.val());
        if (!isNaN(rawValue)) {
            rawFormulaValuesByIndex[index] = rawValue;
        }
    });

    // 3. Calculer les champs de type formule avec gestion des dépendances circulaires
    let maxIterations = 10; // Limite pour éviter les boucles infinies
    let iteration = 0;
    let hasChanges = true;
    
    // Boucle jusqu'à ce qu'il n'y ait plus de changements ou limite atteinte
    while (hasChanges && iteration < maxIterations) {
        hasChanges = false;
        iteration++;
        console.log(`🔄 Itération ${iteration} du calcul des formules`);
        
        $('input[name$=".Resultat"][data-formule]').each(function () {
            const $formulaInput = $(this);
            const formula = $formulaInput.data('formule');
            const oldValue = $formulaInput.val();
            const name = $formulaInput.attr('name') || '';
            const indexMatch = name.match(/Parametres\[(\d+)\]\.Resultat/);

            if (!indexMatch) {
                return;
            }

            const index = indexMatch[1];
            const code = ($(`input[name="Parametres[${index}].Code"]`).val() || '').toString();
            const codeLower = code.toLowerCase();
            const frozenRawValue = rawFormulaValuesByIndex[index];

            if (!formula) {
                console.warn('Formule vide pour:', $formulaInput.attr('name'));
                return;
            }

            console.log(`Traitement formule (itération ${iteration}):`, formula);

            try {
                let expression = normalizeFormula(formula);
                const caseInsensitiveVars = buildCaseInsensitiveVariables(variables);
                const unresolvedTokens = [];

                // Remplacement robuste des variables, insensible à la casse.
                // Si une variable manque, on n'écrase pas la saisie brute avec 0.
                expression = expression.replace(/\b[a-zA-Z_][a-zA-Z0-9_]*\b/g, function (token) {
                    if (codeLower && token.toLowerCase() === codeLower && typeof frozenRawValue !== 'undefined') {
                        return frozenRawValue.toString();
                    }

                    const value = caseInsensitiveVars[token.toLowerCase()];
                    if (typeof value === 'undefined') {
                        unresolvedTokens.push(token);
                        return token;
                    }
                    return value.toString();
                });

                if (unresolvedTokens.length > 0) {
                    console.warn('Formule non résolue, variables manquantes:', unresolvedTokens, 'formule:', formula);
                    return;
                }

                console.log('Expression avant nettoyage:', expression);

                console.log('Expression à évaluer:', expression);

                // Sécuriser l'évaluation - regex plus permissive pour inclure les espaces
                if (!/^[0-9+\-*/.()\s]+$/.test(expression)) {
                    console.error('Expression non sécurisée:', expression);
                    $formulaInput.val('ERREUR');
                    return;
                }

                const result = eval(expression);
                console.log('Résultat calculé:', result);

                if (!isNaN(result) && isFinite(result)) {
                    const formattedResult = result.toFixed(2);
                    
                    // Vérifier si la valeur a changé
                    if (oldValue !== formattedResult) {
                        hasChanges = true;
                        $formulaInput.val(formattedResult);
                        console.log(`✅ Formule évaluée (${oldValue} → ${formattedResult}): ${formula}`);
                        
                        // Mettre à jour la variable pour les prochains calculs
                        if (code) {
                            variables[code] = parseFloat(formattedResult);
                        }
                        
                        // Mettre à jour immédiatement SI et commentaire pour la valeur calculée finale
                        calculerResultatSI($formulaInput);
                        calculerCommentaireAutomatique($formulaInput);
                    } else {
                        console.log(`⚪ Pas de changement pour: ${formula} = ${formattedResult}`);
                    }
                } else {
                    console.warn('Résultat invalide:', result);
                    if (oldValue !== '') {
                        hasChanges = true;
                        $formulaInput.val('');
                    }
                }
            } catch (e) {
                console.error('Erreur dans le calcul de la formule:', formula, e);
                if (oldValue !== 'ERREUR') {
                    hasChanges = true;
                    $formulaInput.val('ERREUR');
                }
            }
        });
    }

    console.log('Calcul des formules - fin');
}

// Global helper functions
function calculerCommentaireAutomatique($resultatInput) {
    if (!$resultatInput || $resultatInput.length === 0) return;
        
    const resultatValue = $resultatInput.val();
    if (!resultatValue || resultatValue.trim() === '') {
        const name = $resultatInput.attr('name');
        const indexMatch = name.match(/Parametres\[(\d+)\]\.Resultat/);
        if (indexMatch) {
            const index = indexMatch[1];
            const $commentaireField = $('input[name="Parametres[' + index + '].Commentaire"]');
            if ($commentaireField.length > 0) {
                $commentaireField.val('');
                $commentaireField.removeClass('result-comment-badge result-comment-bas result-comment-normal result-comment-haut');
                $resultatInput.removeClass('result-input result-bas result-normal result-haut');
            }
        }
        return;
    }
        
    const resultatNum = parseFloat(resultatValue);
    let commentaire = 'Normal';
    
    if (isNaN(resultatNum)) {
        // Non-numeric values get "Normal"
        commentaire = 'Normal';
    } else {
        if (resultatNum < 0) {
            commentaire = 'Bas';
        } else if (resultatNum > 100) {
            commentaire = 'Haut';
        }
    }
        
    const name = $resultatInput.attr('name');
    const indexMatch = name.match(/Parametres\[(\d+)\]\.Resultat/);
    if (indexMatch) {
        const index = indexMatch[1];
        const $commentaireField = $('input[name="Parametres[' + index + '].Commentaire"]');
        if ($commentaireField.length > 0) {
            $commentaireField.val(commentaire);
                
            $commentaireField.removeClass('result-comment-badge result-comment-bas result-comment-normal result-comment-haut');
            $resultatInput.removeClass('result-input result-bas result-normal result-haut');
                
            $commentaireField.addClass('result-comment-badge');
            $resultatInput.addClass('result-input');
                
            if (commentaire === 'Bas') {
                $commentaireField.addClass('result-comment-bas');
                $resultatInput.addClass('result-bas');
            } else if (commentaire === 'Normal') {
                $commentaireField.addClass('result-comment-normal');
                $resultatInput.addClass('result-normal');
            } else if (commentaire === 'Haut') {
                $commentaireField.addClass('result-comment-haut');
                $resultatInput.addClass('result-haut');
            }
        }
    }
}