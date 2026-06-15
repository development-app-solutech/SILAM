"use strict";

var KTParametreCreate = function () {
    // Handle laboratory change and filter analyses
    var handleLaboratoireChange = function () {
        console.log('handleLaboratoireChange done');
        const laboratoireSelect = document.getElementById('laboratoireSelect');
        const analyseSelect = document.getElementById('analyseSelect');
        
        if (laboratoireSelect && analyseSelect) {
            laboratoireSelect.addEventListener('change', function () {
                const laboratoireId = this.value;
                
                // Clear analyses dropdown
                analyseSelect.innerHTML = '<option value="">Sélectionner une analyse</option>';
                
                if (laboratoireId && laboratoireId !== '') {
                    // Make AJAX call to get analyses for selected laboratory
                    fetch(`/Parametre/GetAnalysesByLaboratoire?laboratoireId=${laboratoireId}`)
                        .then(response => response.json())
                        .then(data => {
                            if (data.success) {
                                console.log(data);
                                const analyses = JSON.parse(data.data);
                                analyses.forEach(analyse => {
                                    const option = document.createElement('option');
                                    option.value = analyse.Idanalyse;
                                    option.textContent = analyse.Nom;
                                    analyseSelect.appendChild(option);
                                });
                            } else {
                                console.error('Erreur lors de la récupération des analyses:', data.message);
                            }
                        })
                        .catch(error => {
                            console.error('Erreur AJAX:', error);
                        });
                }

                //$(analyseSelect).trigger('change');
            });
        }
    };

    return {
        init: function () {
            console.log('KTParametreCreate.init appelé');
            handleLaboratoireChange();
        }
    };
}();

document.addEventListener('DOMContentLoaded', function () {
    if (typeof KTParametreCreate !== 'undefined') {
        KTParametreCreate.init();
    }
});
