// Fichier JavaScript pour la page Edit d'Enteteresultat

$(document).ready(function () {

    // Gestion de la case à cocher "Sélectionner tout"
    $('#selectAllCheckbox').on('change', function() {
        var isChecked = $(this).is(':checked');
        $('.row-checkbox').prop('checked', isChecked);
    });

    // Gestion des cases à cocher individuelles
    $('.row-checkbox').on('change', function() {
        var totalCheckboxes = $('.row-checkbox').length;
        var checkedCheckboxes = $('.row-checkbox:checked').length;
        if (checkedCheckboxes === totalCheckboxes) {
            $('#selectAllCheckbox').prop('checked', true);
        } else {
            $('#selectAllCheckbox').prop('checked', false);
        }
    });

    $('.kt-select').select2({
        placeholder: "Sélectionner...",
        allowClear: true,
        width: '100%'
    });

    // AJAX pour charger les analyses selon la demande sélectionnée
    $('#Entetedemandeid').on('change', function() {
        var demandeId = $(this).val();
        var laboratoireId = $('#Laboratoireid').val();
        var $analyseSelect = $('#Idanalyse');
        var value = $('#Idanalyse').val();
        console.log('Idanalyse', value);
        $analyseSelect.empty();
        if (!demandeId) {
            $analyseSelect.append($('<option>', { value: '', text: '---' }));
            $analyseSelect.trigger('change');
            return;
        }
        $.ajax({
            url: '/Entetedemande/GetAnalyse',
            type: 'GET',
            data: { id: demandeId, laboratoireId: laboratoireId },
            success: function(response) {
                $analyseSelect.empty();
                $analyseSelect.append($('<option>', { value: '', text: '---' }));
                if (response.success && response.data) {
                    var analyses = [];
                    try {
                        analyses = JSON.parse(response.data);
                    } catch (e) {
                        analyses = [];
                    }
                    if (Array.isArray(analyses)) {
                        analyses.forEach(function(analyse) {
                            var text = analyse.Nom;
                            if (analyse.NomCategorie && analyse.NomCategorie !== "") {
                                text += ' (' + analyse.NomCategorie + ')';
                            }
                            //$analyseSelect.append($('<option>', { value: analyse.Id, text: text }));
                            $analyseSelect.append($('<option>', {
                                value: analyse.Id,
                                text: text,
                                selected: analyse.Id == value
                            }));
                        });
                    }
                }
                $analyseSelect.trigger('change');
            },
            error: function() {
                $analyseSelect.empty();
                $analyseSelect.append($('<option>', { value: '', text: 'Erreur lors du chargement' }));
                $analyseSelect.trigger('change');
            }
        });
    });

    //quand valeur champ Entetedemandeid change 
    //exécute requete ajax qui ne retourne detail sur patient 
    //a savoir age sexe et peau et mettre les input hidden avec
    $('#Entetedemandeid').on('change', function () {
        const selectedId = $(this).val();

        if (!selectedId) return;

        $.ajax({
            url: '/Entetedemande/GetPatientDetails',
            type: 'GET',
            data: { enteteDemandeId: selectedId },
            success: function (response) {
                // Supposons que le JSON ressemble à : { age: 45, sexe: "F", peau: "Mate" }
                if (response.success) {
                    let data = response.data;
                    console.log(data);
                    $('input[name="Age"]').val(data.age);
                    $('input[name="Sexe"]').val(data.sexe);
                    $('input[name="Peau"]').val(data.peau);
                }
            },
            error: function (xhr) {
                // Essaye de lire le message d'erreur du JSON retourné
                let message = "Une erreur est survenue.";

                if (xhr.responseJSON && xhr.responseJSON.message) {
                    message = xhr.responseJSON.message;
                }

                console.log(message);

                Swal.fire({
                    icon: 'error',
                    title: 'Erreur',
                    text: message,
                    confirmButtonText: 'OK'
                });
            }
        });
    });

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

    // lorsque champ est texte
    // faire evenement onchange
    // si sa valeur change, parcourir les champs de type formules, 
    // si le code champ est spécifié dans la formule, 
    // recalculer la formule avec la valeur du champ resultat 
    // la formule peut contenir des variables comme age, peau et sexe, 
    // elles sont sauvegardes dans des input hidden
    function calculerResultatDeTypeFormule() {
        console.log('Calcul des formules - début');

        // 1. Collecter les valeurs des variables globales
        let variables = {
            age: parseFloat($('input[name="Age"]').val()) || 0,
            sexe: $('input[name="Sexe"]').val() ? 1 : 0,
            peau: $('input[name="Peau"]').val() ? 1 : 0,
        };

        console.log('Variables globales:', variables);

        // 2. D'abord, initialiser TOUS les codes des paramètres à 0
        $('input[name$=".Code"]').each(function() {
            const code = $(this).val();
            if (code && code.trim() !== '') {
                variables[code] = 0;  // Initialiser à 0 par défaut
            }
        });
        
        // 3. Puis collecter les valeurs des champs texte (saisissables)
        $('input[name$=".Resultat"]').not('[data-formule]').each(function () {
            const $input = $(this);
            const name = $input.attr('name');
            const indexMatch = name.match(/Parametres\[(\d+)\]\.Resultat/);
            if (indexMatch) {
                const index = indexMatch[1];
                const code = $(`input[name="Parametres[${index}].Code"]`).val();
                const val = parseFloat($input.val());
                if (code) {
                    if (!isNaN(val)) {
                        variables[code] = val;
                        console.log(`Ajout variable ${code} = ${val}`);
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
        
        // 4. Collecter aussi les valeurs des champs formule déjà calculés
        $('input[name$=".Resultat"][data-formule]').each(function () {
            const $input = $(this);
            const name = $input.attr('name');
            const indexMatch = name.match(/Parametres\[(\d+)\]\.Resultat/);
            if (indexMatch) {
                const index = indexMatch[1];
                const code = $(`input[name="Parametres[${index}].Code"]`).val();
                const val = parseFloat($input.val());
                
                if (code && !isNaN(val)) {
                    variables[code] = val;
                    console.log(`🔄 Variable formule ${code} = ${val}`);
                }
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
        let formulaFieldsProcessed = 0;
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
                
                formulaFieldsProcessed++;
                console.log(`Traitement formule (itération ${iteration}):`, formula);

                try {
                    let expression = normalizeFormula(formula);
                    const caseInsensitiveVars = buildCaseInsensitiveVariables(variables);

                    // Remplacement robuste des variables, insensible à la casse.
                    expression = expression.replace(/\b[a-zA-Z_][a-zA-Z0-9_]*\b/g, function (token) {
                        if (codeLower && token.toLowerCase() === codeLower && typeof frozenRawValue !== 'undefined') {
                            return frozenRawValue.toString();
                        }

                        const value = caseInsensitiveVars[token.toLowerCase()];
                        return typeof value !== 'undefined' ? value.toString() : '0';
                    });

                    console.log('Expression avant nettoyage:', expression);

                    console.log('Expression à évaluer:', expression);

                    // Sécuriser l'évaluation - regex plus permissive pour inclure les espaces
                    if (!/^[0-9+\-*/.()\s]+$/.test(expression)) {
                        console.error('Expression non sécurisée:', expression);
                        $formulaInput.val('ERREUR');
                        return;
                    }

                    // Évaluer la formule
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
        
        console.log(`Nombre de champs de formule traités: ${formulaFieldsProcessed}`);

        console.log('Calcul des formules - fin');
    }

    // Fonction pour calculer automatiquement le commentaire
    function calculerCommentaireAutomatique($resultatInput) {
        if (!$resultatInput || $resultatInput.length === 0) return;
        
        const resultatValue = $resultatInput.val();
        if (!resultatValue || resultatValue.trim() === '') {
            // Si pas de résultat, vider le commentaire et supprimer les styles
            const name = $resultatInput.attr('name');
            const indexMatch = name.match(/Parametres\[(\d+)\]\.Resultat/);
            if (indexMatch) {
                const index = indexMatch[1];
                const $commentaireField = $(`input[name="Parametres[${index}].Commentaire"]`);
                if ($commentaireField.length > 0) {
                    $commentaireField.val('');
                    // Supprimer toutes les classes de style
                    $commentaireField.removeClass('result-comment-badge result-comment-bas result-comment-normal result-comment-haut');
                    $resultatInput.removeClass('result-input result-bas result-normal result-haut');
                }
            }
            return;
        }
        
        // Parser la valeur numérique
        const resultatNum = parseFloat(resultatValue);
        if (isNaN(resultatNum)) {
            // Si ce n'est pas un nombre, mettre "Normal" par défaut avec styling
            const name = $resultatInput.attr('name');
            const indexMatch = name.match(/Parametres\[(\d+)\]\.Resultat/);
            if (indexMatch) {
                const index = indexMatch[1];
                const $commentaireField = $(`input[name="Parametres[${index}].Commentaire"]`);
                if ($commentaireField.length > 0) {
                    $commentaireField.val('Normal');
                    // Appliquer le style Normal
                    $commentaireField.removeClass('result-comment-badge result-comment-bas result-comment-normal result-comment-haut');
                    $resultatInput.removeClass('result-input result-bas result-normal result-haut');
                    $commentaireField.addClass('result-comment-badge result-comment-normal');
                    $resultatInput.addClass('result-input result-normal');
                }
            }
            return;
        }
        
        // Appliquer la logique fixe :
        // - Si résultat < 0 : Bas
        // - Si résultat > 100 : Haut
        // - Si résultat entre 0 et 100 (inclus) : Normal
        let commentaire = 'Normal';
        if (resultatNum < 0) {
            commentaire = 'Bas';
        } else if (resultatNum > 100) {
            commentaire = 'Haut';
        }
        
        // Mettre à jour le champ commentaire correspondant avec styling visuel
        const name = $resultatInput.attr('name');
        const indexMatch = name.match(/Parametres\[(\d+)\]\.Resultat/);
        if (indexMatch) {
            const index = indexMatch[1];
            const $commentaireField = $(`input[name="Parametres[${index}].Commentaire"]`);
            if ($commentaireField.length > 0) {
                // Mettre à jour la valeur
                $commentaireField.val(commentaire);
                
                // Supprimer les anciennes classes
                $commentaireField.removeClass('result-comment-badge result-comment-bas result-comment-normal result-comment-haut');
                $resultatInput.removeClass('result-input result-bas result-normal result-haut');
                
                // Ajouter les nouvelles classes basées sur le commentaire
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

    // Événements pour les champs de résultat de type texte (non-formule)
    $(document).on('input change', 'input[name$=".Resultat"]:not([data-formule])', function() {
        console.log('Input changé pour champ texte:', $(this).attr('name'), $(this).val());
        
        // 1. Calculer le résultat SI
        calculerResultatSI($(this));
        
        // 2. Calculer les formules qui dépendent de ce champ
        calculerResultatDeTypeFormule();
        
        // 3. Calculer le commentaire automatique
        calculerCommentaireAutomatique($(this));
    });

    // Événements pour les champs de résultat de type formule
    $(document).on('blur', 'input[name$=".Resultat"][data-formule]:not([readonly])', function() {
        console.log('Champ formule quitté (blur):', $(this).attr('name'), $(this).val());
        
        // Le champ formule accepte une valeur brute ; le calcul final se fait au blur.
        calculerResultatDeTypeFormule();
    });

    function escapeRegexForFormula(value) {
        return (value || '').toString().replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    }

    function isSelfVariableInFormula(formula, code) {
        if (!formula || !code) return false;
        const pattern = new RegExp('\\b' + escapeRegexForFormula(code) + '\\b', 'i');
        return pattern.test(formula.toString());
    }

    // Fonction pour configurer les champs selon leur type (comme dans create.js)
    function configurerChampsSelonType() {
        console.log('Configuration des champs selon leur type...');
        
        $('input[name$=".Resultat"]').each(function() {
            const $resultatInput = $(this);
            const name = $resultatInput.attr('name');
            const indexMatch = name.match(/Parametres\[(\d+)\]\.Resultat/);
            
            if (indexMatch) {
                const index = indexMatch[1];
                
                // Chercher le type dans les champs cachés
                const $builderTypeField = $(`input[name="Parametres[${index}].Builder.Type"]`);
                const $builderValeurField = $(`input[name="Parametres[${index}].Builder.Valeur"]`);
                const $facteurField = $(`input[name="Parametres[${index}].FacteurConversion"]`);
                const $resultatSI = $(`input[name="Parametres[${index}].Resultatsi"]`);
                
                const builderType = $builderTypeField.val();
                const builderValeur = $builderValeurField.val();
                const facteurConversion = $facteurField.val();
                
                console.log(`Paramètre ${index}: Type="${builderType}", Valeur="${builderValeur}", Facteur="${facteurConversion}"`);
                
                if (builderType === 'Formule') {
                    const codeValue = ($(`input[name="Parametres[${index}].Code"]`).val() || '').toString();
                    const selfFormula = isSelfVariableInFormula((builderValeur || ''), codeValue);

                    // Editable uniquement si la variable du paramètre est présente dans sa propre formule.
                    $resultatInput.prop('readonly', !selfFormula);
                    $resultatInput.attr('data-formule', builderValeur || '');
                    $resultatInput.addClass('formule-field');
                    if (selfFormula) {
                        $resultatInput.attr('placeholder', 'Valeur brute automate');
                        $resultatInput.attr('title', 'Saisir la valeur brute puis quitter le champ pour calcul automatique');
                    } else {
                        $resultatInput.attr('placeholder', 'Auto-calculé');
                        $resultatInput.attr('title', 'Ce champ est auto-calculé');
                    }
                    
                    // Configurer le champ SI correspondant
                    if ($resultatSI.length > 0) {
                        $resultatSI.prop('readonly', true);
                        $resultatSI.attr('data-facteurconversion', facteurConversion || '');
                    }
                    
                    console.log(`⚙️ Paramètre ${index} configuré comme FORMULE (self=${selfFormula}): "${builderValeur}"`);
                } else if (builderType === 'Texte') {
                    // Configurer comme champ de saisie (editable)
                    $resultatInput.prop('readonly', false);
                    $resultatInput.removeAttr('data-formule');
                    $resultatInput.removeClass('formule-field');
                    
                    // Configurer le champ SI correspondant
                    if ($resultatSI.length > 0) {
                        $resultatSI.attr('data-facteurconversion', facteurConversion || '');
                    }
                    
                    console.log(`✏️ Paramètre ${index} configuré comme TEXTE`);
                } else if (builderType === 'Liste') {
                    // Pour les listes, laisser comme c'est (probablement des select)
                    console.log(`📄 Paramètre ${index} configuré comme LISTE`);
                } else {
                    console.log(`❓ Paramètre ${index} type inconnu: "${builderType}"`);
                }
            }
        });
        
        console.log('Configuration des champs terminée');
    }
    
    // Initialisation complète au chargement de la page
    function initialiserCalculsAutomatiques() {
        console.log('Initialisation des calculs automatiques...');
        
        // 1. D'abord configurer les champs selon leur type
        configurerChampsSelonType();
        
        // 2. Puis calculer toutes les formules
        calculerResultatDeTypeFormule();
        
        // 3. Enfin calculer les résultats SI et commentaires pour tous les champs
        $('input[name$=".Resultat"]').each(function() {
            const $resultatInput = $(this);
            if ($resultatInput.val() && $resultatInput.val().trim() !== '') {
                // Calculer SI
                calculerResultatSI($resultatInput);
                // Calculer commentaire
                calculerCommentaireAutomatique($resultatInput);
            }
        });
        
        console.log('Initialisation terminée');
    }

    // Appeler l'initialisation au chargement
    initialiserCalculsAutomatiques();
    
    // Ajouter un déclencheur pour recalculer si les variables globales changent
    $('input[name="Age"], input[name="Sexe"], input[name="Peau"]').on('change', function() {
        console.log('Variable globale changée, recalcul des formules...');
        calculerResultatDeTypeFormule();
    });

    // Rendre les champs commentaire éditables (supprimer readonly)
    $('.commentaire-field').prop('readonly', false).css({
        'background-color': '', 
        'cursor': ''
    });

    // Note: Le tableau des paramètres est maintenant géré statiquement par Razor
    // Plus besoin de reconstruction dynamique qui causait des conflits

});
