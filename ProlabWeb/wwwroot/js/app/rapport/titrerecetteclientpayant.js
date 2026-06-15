$(document).ready(function () {

    function base64ToUint8Array(base64) {
        const binaryString = atob(base64);
        const len = binaryString.length;
        const bytes = new Uint8Array(len);
        for (let i = 0; i < len; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }
        return bytes;
    }

    async function pdfBase64ToPngArray(base64Pdf) {
        const pdfData = base64ToUint8Array(base64Pdf);
        const pdf = await pdfjsLib.getDocument({ data: pdfData }).promise;

        const result = [];

        for (let i = 1; i <= pdf.numPages; i++) {
            const page = await pdf.getPage(i);

            const scale = 2;
            const viewport = page.getViewport({ scale });

            const canvas = document.createElement('canvas');
            const context = canvas.getContext('2d');
            canvas.width = viewport.width;
            canvas.height = viewport.height;

            await page.render({ canvasContext: context, viewport }).promise;

            const imgData = canvas.toDataURL('image/png');
            result.push(imgData);
        }

        return result;
    }

    $('#btnRecherche').on('click', function (e) {
        e.preventDefault();

        var debut = $('#Debut').val();
        var fin = $('#Fin').val();

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

        $("iframe[data-rapport='titrerecetteclientpayant']").remove();

        $.ajax({
            url: '/Rapport/GenererTitreRecetteClientPayant',
            type: 'GET',
            data: { debut: debut, fin: fin },
            beforeSend: function () {
                $('#loader-spinner').show();
            },
            success: function (data) {
                if (data.success) {
                    var printFrame = document.createElement('iframe');
                    printFrame.setAttribute('data-rapport', 'titrerecetteclientpayant');
                    printFrame.style.position = 'fixed';
                    printFrame.style.right = '0';
                    printFrame.style.bottom = '0';
                    printFrame.style.width = '0';
                    printFrame.style.height = '0';
                    printFrame.style.border = '0';
                    document.body.appendChild(printFrame);

                    printFrame.onload = function () {
                        var doc = printFrame.contentWindow || printFrame.contentDocument;
                        if (doc.document) doc = doc.document;

                        pdfBase64ToPngArray(data.pdfBase64)
                            .then(images => {
                                images.forEach(img => {
                                    const imageElement = doc.createElement('img');
                                    imageElement.src = img;
                                    imageElement.style.maxWidth = '100%';
                                    doc.body.appendChild(imageElement);
                                });
                                setTimeout(function () {
                                    printFrame.contentWindow.focus();
                                    printFrame.contentWindow.print();
                                    setTimeout(function () {
                                        document.body.removeChild(printFrame);
                                    }, 1000);
                                }, 500);
                            })
                            .catch(() => {
                                Swal.fire({
                                    text: 'Erreur lors de la conversion du rapport en image.',
                                    icon: 'error',
                                    confirmButtonText: 'Ok, compris !',
                                    customClass: {
                                        confirmButton: 'kt-btn fw-bold kt-btn-primary',
                                    }
                                });
                            });
                    };
                    printFrame.src = 'about:blank';
                } else {
                    Swal.fire({
                        text: 'Erreur lors de la génération du rapport.',
                        icon: 'error',
                        confirmButtonText: 'Ok, compris !',
                        customClass: {
                            confirmButton: 'kt-btn fw-bold kt-btn-primary',
                        }
                    });
                }
            },
            error: function () {
                Swal.fire({
                    text: 'Erreur lors de la génération du rapport.',
                    icon: 'error',
                    confirmButtonText: 'Ok, compris !',
                    customClass: {
                        confirmButton: 'kt-btn fw-bold kt-btn-primary',
                    }
                });
            },
            complete: function () {
                $('#loader-spinner').hide();
            }
        });
    });
});
