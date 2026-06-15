$(document).ready(function () {

    $('#btnRecherche').on('click', function (e) {
        e.preventDefault();
        var debut = $('#Debut').val();
        var fin = $('#Fin').val();
        var partenaireid = $('#Partenaireid').val();
        
        if (!debut) {
            Swal.fire({
                text: 'Veuillez sélectionner une date de début.',
                icon: 'warning',
                confirmButtonText: 'Ok',
                customClass: {
                    confirmButton: 'kt-btn fw-bold kt-btn-primary',
                }
            });
            return;
        }
        
        if (!fin) {
            Swal.fire({
                text: 'Veuillez sélectionner une date de fin.',
                icon: 'warning',
                confirmButtonText: 'Ok',
                customClass: {
                    confirmButton: 'kt-btn fw-bold kt-btn-primary',
                }
            });
            return;
        }
        
        if (!partenaireid) {
            Swal.fire({
                text: 'Veuillez sélectionner un partenaire.',
                icon: 'warning',
                confirmButtonText: 'Ok',
                customClass: {
                    confirmButton: 'kt-btn fw-bold kt-btn-primary',
                }
            });
            return;
        }
        
        $.ajax({
            url: '/Rapport/GenererFacturePartenaire',
            type: 'GET',
            data: { debut: debut, fin: fin, partenaireid: partenaireid },
            beforeSend: function() {
                $('#loader-spinner').show();
            },
            success: function (response) {
                if (response.success && response.pdfBase64) {
                    // Convertir le PDF base64 en blob
                    var binaryString = atob(response.pdfBase64);
                    var bytes = new Uint8Array(binaryString.length);
                    for (var i = 0; i < binaryString.length; i++) {
                        bytes[i] = binaryString.charCodeAt(i);
                    }
                    var pdfBlob = new Blob([bytes], { type: 'application/pdf' });

                    // Convertir le PDF en images et les imprimer
                    convertPdfToImagesAndPrint(pdfBlob);
                } else {
                    Swal.fire({
                        text: 'Erreur dans la réponse du serveur.',
                        icon: 'error',
                        confirmButtonText: 'Ok, compris !',
                        customClass: {
                            confirmButton: 'kt-btn fw-bold kt-btn-primary',
                        }
                    });
                }
            },
            error: function (xhr) {
                var errorMessage = 'Erreur lors de la génération du rapport.';
                if (xhr.responseText) {
                    errorMessage = xhr.responseText;
                }
                Swal.fire({
                    text: errorMessage,
                    icon: 'error',
                    confirmButtonText: 'Ok, compris !',
                    customClass: {
                        confirmButton: 'kt-btn fw-bold kt-btn-primary',
                    }
                });
            },
            complete: function() {
                $('#loader-spinner').hide();
            }
        });
    });
});

// Fonction pour convertir PDF en images et imprimer
function convertPdfToImagesAndPrint(pdfBlob) {
    var fileReader = new FileReader();
    fileReader.onload = function () {
        var typedArray = new Uint8Array(this.result);

        pdfjsLib.getDocument(typedArray).promise.then(function (pdf) {
            var numPages = pdf.numPages;
            var promises = [];

            // Convertir chaque page en image
            for (var i = 1; i <= numPages; i++) {
                promises.push(renderPageToImage(pdf, i));
            }

            Promise.all(promises).then(function (images) {
                printImages(images);
            });
        }).catch(function (error) {
            console.error('Erreur lors du traitement du PDF:', error);
            Swal.fire({
                text: 'Erreur lors du traitement du PDF.',
                icon: 'error',
                confirmButtonText: 'Ok, compris !',
                customClass: {
                    confirmButton: 'kt-btn fw-bold kt-btn-primary',
                }
            });
        });
    };
    fileReader.readAsArrayBuffer(pdfBlob);
}

// Fonction pour rendre une page PDF en image
function renderPageToImage(pdf, pageNumber) {
    return pdf.getPage(pageNumber).then(function (page) {
        var scale = 2.0; // Facteur d'échelle pour la qualité
        var viewport = page.getViewport({ scale: scale });

        var canvas = document.createElement('canvas');
        var context = canvas.getContext('2d');
        canvas.height = viewport.height;
        canvas.width = viewport.width;

        var renderContext = {
            canvasContext: context,
            viewport: viewport
        };

        return page.render(renderContext).promise.then(function () {
            return canvas.toDataURL('image/png');
        });
    });
}

// Fonction pour imprimer les images
function printImages(images) {
    var iframe = document.createElement('iframe');
    iframe.style.position = 'absolute';
    iframe.style.top = '-10000px';
    iframe.style.left = '-10000px';
    iframe.style.width = '1px';
    iframe.style.height = '1px';
    document.body.appendChild(iframe);

    var doc = iframe.contentDocument || iframe.contentWindow.document;
    doc.open();

    var content = '<html><head><title>Impression</title></head><body style="margin: 0; padding: 0;">';
    images.forEach(function (imageSrc, index) {
        if (index > 0) {
            content += '<div style="page-break-before: always;"></div>';
        }
        content += '<img src="' + imageSrc + '" style="width: 100%; height: auto; display: block;" />';
    });
    content += '</body></html>';

    doc.write(content);
    doc.close();

    iframe.onload = function () {
        iframe.contentWindow.print();
        setTimeout(function () {
            document.body.removeChild(iframe);
        }, 1000);
    };
}
