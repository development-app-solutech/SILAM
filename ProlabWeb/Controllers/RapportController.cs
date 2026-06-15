using FastReport;
using FastReport.AdvMatrix;
using FastReport.Data;
using Humanizer;
using iText.IO.Font;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProlabWeb.Data;
using ProlabWeb.Db;
using ProlabWeb.Helpers;
using ProlabWeb.ViewModels;
using System.Buffers.Text;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using static iText.StyledXmlParser.Css.Font.CssFontFace;
using static ProlabWeb.Helpers.Utilities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProlabWeb.Controllers
{
    [AllowAnonymous]
    public class RapportController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ProlabwebContext _context;
        private readonly ILogger<RapportController> _logger;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ProlabIdentityUser> _usermanager;

        public RapportController(IWebHostEnvironment webHostEnvironment, ProlabwebContext context, ILogger<RapportController> logger, IConfiguration configuration, UserManager<ProlabIdentityUser> userManager)
        {
            _webHostEnvironment = webHostEnvironment;
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _usermanager = userManager;
        }

        private string GetFontPath(bool normal, bool bold)
        {
            string os = "";
            string type = "";

            if (OperatingSystem.IsWindows())
                os = "Windows";
            else if (OperatingSystem.IsLinux())
                os = "Linux";

            if (normal)
                type = "Normal";
            else if (bold)
                type = "Bold";

            string fontPath = _configuration[$"{os}:Fonts:TimesNewRoman:{type}"];

            return fontPath;
        }

        public static float CmToPt(float cm)
        {
            return cm * 28.3465f;
        }

        [HttpGet]
        public IActionResult RecetteParJour()
        {
            return View(new RecetteParJourVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RecetteParJour(RecetteParJourVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            ViewBag.Message = $"Date saisie : {vm.Date} - Type de client : {vm.TypeClient}";
            return View(vm);
        }

        private Document drawNumeroDePages(PdfDocument pdf, Document document, PageSize pageSize, PdfFont fontNormal)
        {
            float height = CmToPt(0.6f); // Hauteur du footer
            float width = CmToPt(2f);    // Largeur réservée pour le numéro de page

            // Nombre total de pages
            int numberOfPages = pdf.GetNumberOfPages();

            // Parcours des pages
            for (int i = 1; i <= numberOfPages; i++)
            {
                var page = pdf.GetPage(i);

                float x = pageSize.GetWidth() - pageSize.GetRight() - width; // x depuis la droite
                float y = pageSize.GetBottom() / 2 - height / 2; // centré verticalement dans la marge

                // Format : 1/3, 2/3, etc.
                var pageNumberText = $"{i}/{numberOfPages}";

                // Créer un paragraphe avec la police et taille
                var paragraph = new Paragraph(pageNumberText)
                    .SetFont(fontNormal)
                    .SetFontSize(11)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFixedPosition(i, x, y, width);

                document.Add(paragraph);
            }

            return document;
        }

        [HttpGet]
        public async Task<IActionResult> GenererRecetteParJour(string date, string? typeClient)
        {
            try
            {
                // Vérifier que la date est valide
                if (string.IsNullOrWhiteSpace(date) || !DateTime.TryParse(date, out var parsedDate))
                {
                    _logger.LogWarning("Paramètre date non valide");

                    return BadRequest("Le paramètre date est invalide.");
                }

                var normalizedTypeClient = string.IsNullOrWhiteSpace(typeClient)
                    ? null
                    : typeClient.Trim().ToLowerInvariant();
                if (normalizedTypeClient != null && normalizedTypeClient != "payant" && normalizedTypeClient != "assurance")
                {
                    _logger.LogWarning("Paramètre typeClient inconnu: {TypeClient}", typeClient);

                    return BadRequest("Le paramètre typeClient est invalide.");
                }

                // utilisateur connecté
                var user = await _usermanager.GetUserAsync(User);

                Utilisateur utilisateur = null;

                if (user != null)
                {
                    utilisateur = await _context.Utilisateurs.AsNoTracking()
                        .Include(x => x.CodesiteNavigation)
                        .Where(x => x.Userid == user.Id)
                        .FirstOrDefaultAsync();

                    if (utilisateur == null)
                    {
                        _logger.LogWarning("Utilisateur non trouvé");

                        return BadRequest("Utilisateur non trouvé");
                    }
                    else
                    {
                        var roles = await _usermanager.GetRolesAsync(user);
                        bool isCaissier = roles.Contains(EnumRoles.Caisse.ToString());

                        if (!isCaissier)
                        {
                            _logger.LogWarning("L'utilisateur ne dispose pas du rôle caissier");

                            return BadRequest("L'utilisateur ne dispose pas du rôle caissier");
                        }
                    }
                }

                var cultureFr = new CultureInfo("fr-FR");

                using var stream = new MemoryStream();
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf, PageSize.A4);

                float pageHeight = PageSize.A4.GetHeight();

                float marginTop = document.GetTopMargin();
                float marginBottom = document.GetBottomMargin();
                float marginLeft = document.GetLeftMargin();
                float marginRight = document.GetRightMargin();

                Console.WriteLine($"Top: {marginTop}, Bottom: {marginBottom}, Left: {marginLeft}, Right: {marginRight}");

                // S’assurer que les encodages étendus sont disponibles
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                PdfFont fontNormal = PdfFontFactory.CreateFont(GetFontPath(true, false), PdfEncodings.IDENTITY_H);
                PdfFont fontBold = PdfFontFactory.CreateFont(GetFontPath(false, true), PdfEncodings.IDENTITY_H);

                Console.WriteLine($"Nomral: {GetFontPath(true, false)}, Bold: {GetFontPath(false, true)}");

                Console.WriteLine($"Top: {marginTop}, Bottom: {marginBottom}, Left: {marginLeft}, Right: {marginRight}");

                // S’assurer que les encodages étendus sont disponibles
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                // === Table d'en-tête (1 ligne, 3 colonnes) ===
                float totalWidth = PageSize.A4.GetWidth() - 36 - 36; // largeur utile = largeur page - marges
                float col1Width = CmToPt(2.75f);
                float col3Width = CmToPt(5.75f);
                float col2Width = totalWidth - col1Width - col3Width;

                Table enteteTable = new Table(new float[] { col1Width, col2Width, col3Width });
                enteteTable.SetWidth(UnitValue.CreatePointValue(totalWidth));
                enteteTable.SetFixedLayout(); // pour que les colonnes respectent les tailles fixées

                // === Colonne 1 : Image ===
                string imagePath = System.IO.Path.Combine(_webHostEnvironment.WebRootPath, "img", "inh_togo_logo.png");
                Image img = new Image(ImageDataFactory.Create(imagePath))
                    .SetWidth(CmToPt(2.75f))
                    .SetHeight(CmToPt(1.75f));

                enteteTable.AddCell(new Cell().Add(img).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                // === Colonne 2 : espace vide ===
                enteteTable.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                // === Colonne 3 : sous-table 3 lignes ===
                Table subTable = new Table(1).SetWidth(UnitValue.CreatePointValue(col3Width));

                subTable.AddCell(new Cell()
                    .Add(new Paragraph("REPUBLIQUE TOGOLAISE")
                    .SetFont(fontNormal).SetFontSize(10).SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                subTable.AddCell(new Cell()
                    .Add(new Paragraph("Travail - Liberté - Patrie")
                    .SetFont(fontNormal).SetFontSize(10).SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                subTable.AddCell(new Cell()
                    .SetHeight(CmToPt(0.25f)) // espace vide
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                enteteTable.AddCell(new Cell().Add(subTable).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                // === Ajouter au document
                document.Add(enteteTable);

                // Valeur dynamique de l'opérateur
                string operateur = "";

                // Récupérer l'utilisateur connecté
                //string login = User.Identity?.Name ?? string.Empty;
                //operateur = login;
                //var utilisateur = _context.Utilisateurs.FirstOrDefault(u => u.Login == login);
                if (utilisateur != null)
                {
                    operateur = $"{utilisateur.Nom} {utilisateur.Prenom}".Trim();
                }

                // Texte à insérer
                var typeClientLabel = normalizedTypeClient == null
                    ? "Tous clients"
                    : normalizedTypeClient == "assurance" ? "Assurances" : "Clients payants";
                string texte = $"Recettes du jour - Opérateur : {operateur} - {typeClientLabel}";

                // Création du paragraphe
                var paragraph = new Paragraph(texte)
                    .SetFont(fontBold)
                    .SetFontSize(12)
                    .SetTextAlignment(TextAlignment.CENTER); // centré horizontalement

                // Ajouter au document
                document.Add(paragraph);

                var q1 = from P in _context.Entetefactures
                         join p1 in _context.Entetedemandes on P.Entetedemandeid equals p1.Entetedemandeid
                         join p2 in _context.Detailfactures on P.Entetefactureid equals p2.Entetefactureid
                         join p6 in _context.Detaildemandes on p2.Detaildemandeid equals p6.Detaildemandeid
                         join p3 in _context.Categories on p6.Categorieid equals p3.Categorieid
                         join p4 in _context.Patients on p1.Patientid equals p4.Patientid
                         join p5 in _context.Sexes on p4.Codesexe equals p5.Codesexe
                         where P.Date.Date == DateTime.Parse(date).Date &&
                            P.Utilisateurid == utilisateur.Utilisateurid
                         select new
                         {
                             P.Date,
                             P.Entetefactureid,
                             Traitement = p3.Nom,
                             p6.Net,
                             EstAssure = p1.Policeassuranceid != null,
                             p1.Policeassuranceid,
                             p4.Patientid,
                             p4.Nom,
                             p4.Prenom,
                             Sexe = p5.Value,
                             p4.Datenaissance
                         };

                var q2 = from P in _context.Entetefactures
                         join p1 in _context.Detailfactures on P.Entetefactureid equals p1.Entetefactureid
                         join p2 in _context.Entetedemandes on P.Entetedemandeid equals p2.Entetedemandeid
                         join p3 in _context.Patients on p2.Patientid equals p3.Patientid
                         join p4 in _context.Sexes on p3.Codesexe equals p4.Codesexe
                         join p5 in _context.Detaildemandes on p1.Detaildemandeid equals p5.Detaildemandeid
                         join p6 in _context.Analyses on p5.Idanalyse equals p6.Idanalyse
                         where P.Date.Date == DateTime.Parse(date).Date &&
                            P.Utilisateurid == utilisateur.Utilisateurid
                         select new
                         {
                             P.Date,
                             P.Entetefactureid,
                             Traitement = p6.Nom,
                             p5.Net,
                             EstAssure = p2.Policeassuranceid != null,
                             p2.Policeassuranceid,
                             p3.Patientid,
                             p3.Nom,
                             p3.Prenom,
                             Sexe = p4.Value,
                             p3.Datenaissance
                         };

                var combined = q1.Concat(q2).ToList();

                if (normalizedTypeClient == "assurance")
                {
                    combined = combined.Where(x => x.EstAssure).ToList();
                }
                else if (normalizedTypeClient == "payant")
                {
                    combined = combined.Where(x => !x.EstAssure).ToList();
                }

                var rows = combined
                    .GroupBy(x => new
                    {
                        x.Policeassuranceid,
                        x.Date,
                        x.Entetefactureid,
                        x.Patientid,
                        x.Nom,
                        x.Prenom,
                        x.Sexe,
                        x.Datenaissance
                    })
                    .Select(g => new
                    {
                        Policeassuranceid = g.Key.Policeassuranceid,
                        Date = g.Key.Date.ToString("dd/MM/yyyy"),
                        Patient = $"{g.Key.Nom} {g.Key.Prenom}",
                        Sexe = g.Key.Sexe,
                        Datenaissance = g.Key.Datenaissance.ToString("dd/MM/yyyy"),
                        Analyse = string.Join(", ", g.Select(e => e.Traitement).Distinct()),
                        Montant = g.Sum(e => e.Net),
                    })
                    .ToList();

                // === Ligne 3 : Table 6 colonnes ===
                float[] colWidths = {
                    CmToPt(2.5f),
                    CmToPt(4.25f),
                    CmToPt(2.5f),
                    CmToPt(2.5f),
                    CmToPt(4.25f),
                    CmToPt(2.5f)
                };

                Table table6 = new Table(colWidths);
                table6.SetWidth(UnitValue.CreatePointValue(colWidths.Sum()));

                // === Header ===
                string[] headers = { "DATE", "PATIENT", "SEXE", "DATE DE NAISSANCE", "ANALYSES", "MONTANT" };
                decimal totaux = 0;

                if (normalizedTypeClient == "assurance")
                {
                    var policeIds = rows
                        .Where(r => r.Policeassuranceid.HasValue)
                        .Select(r => r.Policeassuranceid.Value)
                        .Distinct()
                        .ToList();

                    var policeInfos = (from police in _context.Policeassurances
                                       join assurance in _context.Assurances on police.Codeassurance equals assurance.Codeassurance
                                       where policeIds.Contains(police.Policeassuranceid)
                                       select new
                                       {
                                           police.Policeassuranceid,
                                           police.Libelle,
                                           police.Taux,
                                           AssuranceNom = assurance.Nom
                                       })
                                       .ToDictionary(x => x.Policeassuranceid, x => x);

                    var rowsByPolice = rows
                        .GroupBy(r => r.Policeassuranceid)
                        .OrderBy(g =>
                        {
                            if (g.Key.HasValue && policeInfos.TryGetValue(g.Key.Value, out var info))
                            {
                                return info.AssuranceNom + " " + info.Libelle;
                            }

                            return string.Empty;
                        })
                        .ToList();

                    foreach (var group in rowsByPolice)
                    {
                        string policeTitre = "ASSURANCE INCONNUE";
                        if (group.Key.HasValue && policeInfos.TryGetValue(group.Key.Value, out var info))
                        {
                            var tauxLabel = info.Taux.ToString("0.##", cultureFr);
                            policeTitre = $"{info.AssuranceNom}: {tauxLabel}%";
                        }

                        document.Add(new Paragraph(policeTitre)
                            .SetFont(fontBold)
                            .SetFontSize(11)
                            .SetTextAlignment(TextAlignment.LEFT)
                            .SetMarginTop(CmToPt(0.35f))
                            .SetMarginBottom(CmToPt(0.15f)));

                        var tableAssurance = new Table(colWidths);
                        tableAssurance.SetWidth(UnitValue.CreatePointValue(colWidths.Sum()));

                        foreach (var header in headers)
                        {
                            tableAssurance.AddHeaderCell(new Cell()
                                .Add(new Paragraph(header).SetFont(fontNormal).SetFontSize(10))
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                .SetHeight(CmToPt(1f))
                                .SetBorder(new SolidBorder(ColorConstants.BLACK, 1))
                            );
                        }

                        foreach (var row in group)
                        {
                            tableAssurance.AddCell(new Cell().Add(new Paragraph(row.Date)).SetFont(fontNormal).SetFontSize(11)
                                .SetHeight(CmToPt(0.75f))
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                            tableAssurance.AddCell(new Cell().Add(new Paragraph(row.Patient)).SetFont(fontNormal).SetFontSize(11)
                                .SetTextAlignment(TextAlignment.LEFT)
                                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                            tableAssurance.AddCell(new Cell().Add(new Paragraph(row.Sexe)).SetFont(fontNormal).SetFontSize(11)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                            tableAssurance.AddCell(new Cell().Add(new Paragraph(row.Datenaissance)).SetFont(fontNormal).SetFontSize(11)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                            tableAssurance.AddCell(new Cell().Add(new Paragraph(row.Analyse)).SetFont(fontNormal).SetFontSize(11)
                                .SetTextAlignment(TextAlignment.LEFT)
                                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                            tableAssurance.AddCell(new Cell().Add(new Paragraph(row.Montant.ToString("N0", cultureFr))).SetFont(fontNormal).SetFontSize(11)
                                .SetTextAlignment(TextAlignment.RIGHT)
                                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));
                        }

                        document.Add(tableAssurance);

                        var totalPolice = group.Sum(r => r.Montant);
                        totaux += totalPolice;

                        var totalPoliceTable = new Table(UnitValue.CreatePointArray(new float[] { CmToPt(15), CmToPt(4) }));
                        totalPoliceTable.AddCell(CreateCell("Total police :", fontBold, 11, TextAlignment.RIGHT));
                        totalPoliceTable.AddCell(CreateCell(totalPolice.ToString("N0", cultureFr), fontNormal, 11, TextAlignment.RIGHT));
                        document.Add(totalPoliceTable);

                        document.Add(new Paragraph().SetMarginTop(CmToPt(0.1f)));
                    }
                }
                else
                {
                    foreach (var header in headers)
                    {
                        table6.AddHeaderCell(new Cell()
                            .Add(new Paragraph(header).SetFont(fontNormal).SetFontSize(10))
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .SetHeight(CmToPt(1f))
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1))
                        );
                    }

                    foreach (var row in rows)
                    {
                        totaux += row.Montant;

                        table6.AddCell(new Cell().Add(new Paragraph(row.Date)).SetFont(fontNormal).SetFontSize(11)
                            .SetHeight(CmToPt(0.75f))
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                        table6.AddCell(new Cell().Add(new Paragraph(row.Patient)).SetFont(fontNormal).SetFontSize(11)
                            .SetTextAlignment(TextAlignment.LEFT)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                        table6.AddCell(new Cell().Add(new Paragraph(row.Sexe)).SetFont(fontNormal).SetFontSize(11)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                        table6.AddCell(new Cell().Add(new Paragraph(row.Datenaissance)).SetFont(fontNormal).SetFontSize(11)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                        table6.AddCell(new Cell().Add(new Paragraph(row.Analyse)).SetFont(fontNormal).SetFontSize(11)
                            .SetTextAlignment(TextAlignment.LEFT)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                        table6.AddCell(new Cell().Add(new Paragraph(row.Montant.ToString("N0", cultureFr))).SetFont(fontNormal).SetFontSize(11)
                            .SetTextAlignment(TextAlignment.RIGHT)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));
                    }

                    document.Add(table6);
                }

                // Trait noir largeur 19 cm
                {
                    document.Add(new Paragraph().SetMarginTop(CmToPt(0.25f)));
                    document.Add(new LineSeparator(new SolidLine()).SetWidth(UnitValue.CreatePercentValue(100)));
                    document.Add(new Paragraph().SetMarginTop(CmToPt(0.25f)));
                }

                {
                    var table = new Table(UnitValue.CreatePointArray(new float[] { CmToPt(15), CmToPt(4) }));
                    table.AddCell(CreateCell("Total :", fontBold, 11, TextAlignment.RIGHT));
                    table.AddCell(CreateCell(totaux.ToString("N0", cultureFr), fontNormal, 11, TextAlignment.RIGHT));
                    document.Add(table);
                }

                document.Close();

                byte[] pdfBytes = stream.ToArray();

                // Exemple de réouverture et modification
                using (var input = new MemoryStream(pdfBytes))
                using (var output = new MemoryStream())
                {
                    var reader = new PdfReader(input);
                    var writer1 = new PdfWriter(output);
                    var pdfDoc = new PdfDocument(reader, writer1);

                    // Ajout des numéros de page (code selon besoin)
                    float height = CmToPt(0.6f); // Hauteur du footer
                    float width = CmToPt(2f);    // Largeur réservée pour le numéro de page
                    var fontFooter = PdfFontFactory.CreateFont(GetFontPath(true, false), PdfEncodings.IDENTITY_H);

                    // Nombre total de pages
                    int numberOfPages = pdfDoc.GetNumberOfPages();

                    //Parcours des pages
                    for (int i = 1; i <= numberOfPages; i++)
                    {
                        PdfPage page = pdfDoc.GetPage(i);

                        float x = PageSize.A4.GetWidth() - marginRight - width / 2; // x depuis la droite
                        float y = marginBottom / 2 - height; // centré verticalement dans la marge

                        // Format : 1/3, 2/3, etc.
                        var pageNumberText = $"{i}/{numberOfPages}";

                        PdfCanvas pdfCanvas = new PdfCanvas(page);

                        //// Bordure noire
                        //pdfCanvas.SetLineWidth(1)
                        //  .SetStrokeColor(ColorConstants.BLACK)
                        //  .Rectangle(x, y, width, height)
                        //  .Stroke();

                        // Commence à écrire du texte
                        pdfCanvas.BeginText()
                            .SetFontAndSize(fontFooter, 11)
                            .SetFillColor(ColorConstants.BLACK)
                            .MoveText(x, y)
                            .ShowText(pageNumberText)
                            .EndText()
                            .Release();
                    }

                    pdfDoc.Close();

                    string base64 = Convert.ToBase64String(output.ToArray());

                    return Json(new
                    {
                        success = true,
                        pdfBase64 = base64
                    });
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message, "erreur lors de la génération de rapport");
                throw;
            }
        }

        //[HttpGet, ActionName("GenererRecetteParJour")]
        //public IActionResult GenererRecetteParJour(string date)
        //{
        //    try
        //    {
        //        // Vérifier que la date est valide
        //        if (string.IsNullOrWhiteSpace(date) || !DateTime.TryParse(date, out var parsedDate))
        //        {
        //            _logger.LogWarning("Paramètre date non valide");

        //            return BadRequest("Le paramètre date est invalide.");
        //        }

        //        AppContext.SetSwitch("System.Drawing.EnableUnixSupport", true);
        //        FastReport.Utils.Config.WebMode = true; // si Web

        //        var reportPath = Path.Combine(_webHostEnvironment.WebRootPath, "reports", "recetteparjour.frx");
        //        if (!System.IO.File.Exists(reportPath))
        //        {
        //            _logger.LogWarning("Chemin non valide");
        //            return NotFound("Le rapport n'existe pas.");
        //        }

        //        // Récupérer l'utilisateur connecté
        //        string login = User.Identity?.Name ?? string.Empty;
        //        string operateur = login;
        //        var utilisateur = _context.Utilisateurs.FirstOrDefault(u => u.Login == login);
        //        if (utilisateur != null)
        //        {
        //            operateur = $"{utilisateur.Nom} {utilisateur.Prenom}".Trim();
        //        }
        //        _logger.LogWarning("Utilisateur en cours");

        //        using (var report = new FastReport.Report())
        //        {
        //            _logger.LogWarning($"Report initialized");

        //            report.Load(reportPath);
        //            _logger.LogWarning($"Report loaded");

        //            // Repasser la valeur brute de date (string) au report
        //            report.SetParameterValue("datefacture", DateTime.Parse(date));
        //            report.SetParameterValue("operateur", operateur);

        //            _logger.LogWarning($"Paramètre envoyé");

        //            report.Prepare();

        //            _logger.LogWarning($"Report prepared");

        //            using (var ms = new MemoryStream())
        //            {
        //                var imageExport = new FastReport.Export.Image.ImageExport
        //                {
        //                    ImageFormat = FastReport.Export.Image.ImageExportFormat.Png,
        //                    SeparateFiles = false,
        //                    ResolutionX = 300,
        //                    ResolutionY = 300
        //                };
        //                report.Export(imageExport, ms);
        //                ms.Position = 0;
        //                var base64 = Convert.ToBase64String(ms.ToArray());

        //                _logger.LogWarning($"Report exported to image");

        //                return Json(new { success = true, imageBase64 = "data:image/png;base64," + base64 });
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex.Message, "erreur d'impression");
        //        throw;
        //    }
        //}

        [HttpGet]
        public IActionResult RecetteParJourParPartenaire()
        {
            return View(new RecetteParJourVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RecetteParJourParPartenaire(RecetteParJourVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            ViewBag.Message = $"Date saisie : {vm.Date}";
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GenererRecetteParJourParPartenaire(string date)
        {
            // Vérifier que la date est valide
            if (string.IsNullOrWhiteSpace(date) || !DateTime.TryParse(date, out var parsedDate))
            {
                _logger.LogWarning("Paramètre date non valide");

                return BadRequest("Le paramètre date est invalide.");
            }

            // utilisateur connecté
            var user = await _usermanager.GetUserAsync(User);

            Utilisateur utilisateur = null;

            if (user != null)
            {
                utilisateur = await _context.Utilisateurs.AsNoTracking()
                    .Include(x => x.CodesiteNavigation)
                    .Where(x => x.Userid == user.Id)
                    .FirstOrDefaultAsync();

                if (utilisateur == null)
                {
                    _logger.LogWarning("Utilisateur non trouvé");

                    return BadRequest("Utilisateur non trouvé");
                }
                else
                {
                    var roles = await _usermanager.GetRolesAsync(user);
                    bool isCaissier = roles.Contains(EnumRoles.Caisse.ToString());

                    if (!isCaissier)
                    {
                        _logger.LogWarning("L'utilisateur ne dispose pas du rôle caissier");

                        return BadRequest("L'utilisateur ne dispose pas du rôle caissier");
                    }
                }
            }

            try
            {
                var cultureFr = new CultureInfo("fr-FR");

                using var stream = new MemoryStream();
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf, PageSize.A4);
                document.SetMargins(36, 36, 36, 36);

                float pageHeight = PageSize.A4.GetHeight();

                float marginTop = document.GetTopMargin();
                float marginBottom = document.GetBottomMargin();
                float marginLeft = document.GetLeftMargin();
                float marginRight = document.GetRightMargin();

                Console.WriteLine($"Top: {marginTop}, Bottom: {marginBottom}, Left: {marginLeft}, Right: {marginRight}");

                // S’assurer que les encodages étendus sont disponibles
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                PdfFont fontNormal = PdfFontFactory.CreateFont(GetFontPath(true, false), PdfEncodings.IDENTITY_H);
                PdfFont fontBold = PdfFontFactory.CreateFont(GetFontPath(false, true), PdfEncodings.IDENTITY_H);

                Console.WriteLine($"Normal: {GetFontPath(true, false)}, Bold: {GetFontPath(false, true)}");

                // === Table d'en-tête (1 ligne, 3 colonnes) ===
                float totalWidth = PageSize.A4.GetWidth() - 36 - 36; // largeur utile = largeur page - marges
                float col1Width = CmToPt(2.75f);
                float col3Width = CmToPt(5.75f);
                float col2Width = totalWidth - col1Width - col3Width;

                Table enteteTable = new Table(new float[] { col1Width, col2Width, col3Width });
                enteteTable.SetWidth(UnitValue.CreatePointValue(totalWidth));
                enteteTable.SetFixedLayout(); // pour que les colonnes respectent les tailles fixées

                // === Colonne 1 : Image ===
                string imagePath = System.IO.Path.Combine(_webHostEnvironment.WebRootPath, "img", "inh_togo_logo.png");
                Image img = new Image(ImageDataFactory.Create(imagePath))
                    .SetWidth(CmToPt(2.75f))
                    .SetHeight(CmToPt(1.75f));

                enteteTable.AddCell(new Cell().Add(img).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                // === Colonne 2 : espace vide ===
                enteteTable.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                // === Colonne 3 : sous-table 3 lignes ===
                Table subTable = new Table(1).SetWidth(UnitValue.CreatePointValue(col3Width));

                subTable.AddCell(new Cell()
                    .Add(new Paragraph("REPUBLIQUE TOGOLAISE")
                    .SetFont(fontNormal).SetFontSize(10).SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                subTable.AddCell(new Cell()
                    .Add(new Paragraph("Travail - Liberté - Patrie")
                    .SetFont(fontNormal).SetFontSize(10).SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                subTable.AddCell(new Cell()
                    .SetHeight(CmToPt(0.25f)) // espace vide
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                enteteTable.AddCell(new Cell().Add(subTable).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                // === Ajouter au document
                document.Add(enteteTable);

                // Valeur dynamique de l'opérateur
                string operateur = "";

                // Récupérer l'utilisateur connecté
                //string login = User.Identity?.Name ?? string.Empty;
                //operateur = login;
                //var utilisateur = _context.Utilisateurs.FirstOrDefault(u => u.Login == login);
                if (utilisateur != null)
                {
                    operateur = $"{utilisateur.Nom} {utilisateur.Prenom}".Trim();
                }

                // Texte à insérer
                string texte = $"Recettes du jour - Opérateur : {operateur}";

                // Création du paragraphe
                var paragraph = new Paragraph(texte)
                    .SetFont(fontBold)
                    .SetFontSize(12)
                    .SetTextAlignment(TextAlignment.CENTER); // centré horizontalement

                // Ajouter au document
                document.Add(paragraph);

                var partenaires = (
                    from p in _context.Entetefactures
                    join d in _context.Entetedemandes on p.Entetedemandeid equals d.Entetedemandeid
                    join partenaire in _context.Partenaires on d.Partenaireid equals partenaire.Partenaireid
                    where p.Date.Date == DateTime.Parse(date).Date 
                        && d.Utilisateurid == utilisateur.Utilisateurid
                    select new
                    {
                        partenaire.Partenaireid,
                        partenaire.Nom,
                        p.Entetefactureid,
                        p.Date,
                        d.Entetedemandeid
                    })
                    .AsEnumerable() // force LINQ to Objects
                    .GroupBy(x => x.Partenaireid)
                    .Select(g => g.First())
                    .Select(x => new
                    {
                        x.Partenaireid,
                        x.Nom
                    })
                    .ToList();

                decimal totaux = 0;

                foreach (var partenaire in partenaires)
                {
                    var q1 = from P in _context.Entetefactures
                             join p1 in _context.Entetedemandes on P.Entetedemandeid equals p1.Entetedemandeid
                             join p2 in _context.Detailfactures on P.Entetefactureid equals p2.Entetefactureid
                             join p6 in _context.Detaildemandes on p2.Detaildemandeid equals p6.Detaildemandeid
                             join p3 in _context.Categories on p6.Categorieid equals p3.Categorieid
                             join p4 in _context.Patients on p1.Patientid equals p4.Patientid
                             join p5 in _context.Sexes on p4.Codesexe equals p5.Codesexe
                             join p7 in _context.Partenaires on p1.Partenaireid equals p7.Partenaireid
                             where P.Date.Date == DateTime.Parse(date).Date
                                   && p7.Partenaireid == partenaire.Partenaireid
                                   && p1.Utilisateurid == utilisateur.Utilisateurid
                             select new
                             {
                                 P.Date,
                                 P.Entetefactureid,
                                 Traitement = p3.Nom,
                                 Prix = p6.Prix,
                                 p4.Patientid,
                                 p4.Nom,
                                 p4.Prenom,
                                 Sexe = p5.Value,
                                 p4.Datenaissance
                             };

                    var q2 = from P in _context.Entetefactures
                             join p1 in _context.Detailfactures on P.Entetefactureid equals p1.Entetefactureid
                             join p2 in _context.Entetedemandes on P.Entetedemandeid equals p2.Entetedemandeid
                             join p3 in _context.Patients on p2.Patientid equals p3.Patientid
                             join p4 in _context.Sexes on p3.Codesexe equals p4.Codesexe
                             join p5 in _context.Detaildemandes on p1.Detaildemandeid equals p5.Detaildemandeid
                             join p6 in _context.Analyses on p5.Idanalyse equals p6.Idanalyse
                             join p7 in _context.Partenaires on p2.Partenaireid equals p7.Partenaireid
                             where P.Date.Date == DateTime.Parse(date).Date
                                   && p7.Partenaireid == partenaire.Partenaireid
                                   && p2.Utilisateurid == utilisateur.Utilisateurid
                             select new
                             {
                                 P.Date,
                                 P.Entetefactureid,
                                 Traitement = p6.Nom,
                                 Prix = p5.Prix,
                                 p3.Patientid,
                                 p3.Nom,
                                 p3.Prenom,
                                 Sexe = p4.Value,
                                 p3.Datenaissance
                             };

                    var union = q1.Concat(q2).ToList(); // switch to LINQ to Objects for aggregation

                    var rows = union
                        .GroupBy(x => new
                        {
                            x.Date,
                            x.Entetefactureid,
                            x.Patientid,
                            x.Nom,
                            x.Prenom,
                            x.Sexe,
                            x.Datenaissance
                        })
                        .Select(g => new
                        {
                            Date = g.Key.Date.ToString("dd/MM/yyyy"),
                            Patient = $"{g.Key.Nom} {g.Key.Prenom}",
                            Sexe = g.Key.Sexe,
                            Datenaissance = g.Key.Datenaissance.ToString("dd/MM/yyyy"),
                            Analyse = string.Join(", ", g.Select(x => x.Traitement).Distinct()).ToString(),
                            Montant = g.Sum(x => x.Prix)
                        })
                        .ToList();

                    // === TABLE PRINCIPALE ===
                    Table mainTable = new Table(1).UseAllAvailableWidth();

                    // === Ligne 1 : Table 2 colonnes ===
                    Table subTable1 = new Table(UnitValue.CreatePercentArray(new float[] { 1, 2 }))
                        .UseAllAvailableWidth();

                    subTable1.AddCell(new Cell()
                        .Add(new Paragraph("Partenaire :").SetFont(fontBold).SetFontSize(11))
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    );

                    subTable1.AddCell(new Cell()
                        .Add(new Paragraph(partenaire.Nom).SetFont(fontNormal).SetFontSize(11))
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    );

                    mainTable.AddCell(new Cell().Add(subTable1).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                    // === Ligne 2 : Espace vide 0.25 cm ===
                    mainTable.AddCell(new Cell()
                        .SetHeight(CmToPt(0.25f))
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    );

                    // === Ligne 3 : Table 6 colonnes ===
                    float[] colWidths = {
                        CmToPt(2.5f),
                        CmToPt(4.25f),
                        CmToPt(2.5f),
                        CmToPt(2.5f),
                        CmToPt(4.25f),
                        CmToPt(2.5f)
                    };

                    Table table6 = new Table(colWidths);
                    table6.SetWidth(UnitValue.CreatePointValue(colWidths.Sum()));

                    // === Header ===
                    string[] headers = { "DATE", "PATIENT", "SEXE", "DATE DE NAISSANCE", "ANALYSES", "MONTANT" };

                    foreach (var header in headers)
                    {
                        table6.AddHeaderCell(new Cell()
                            .Add(new Paragraph(header).SetFont(fontNormal).SetFontSize(10))
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .SetHeight(CmToPt(1f))
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1))
                        );
                    }

                    // === Body (exemple avec 20 lignes) ===
                    foreach (var row in rows)
                    {
                        totaux += row.Montant;

                        table6.AddCell(new Cell().Add(new Paragraph(row.Date)).SetFont(fontNormal).SetFontSize(11)
                            .SetHeight(CmToPt(0.75f))
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                        table6.AddCell(new Cell().Add(new Paragraph(row.Patient)).SetFont(fontNormal).SetFontSize(11)
                            .SetTextAlignment(TextAlignment.LEFT)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                        table6.AddCell(new Cell().Add(new Paragraph(row.Sexe)).SetFont(fontNormal).SetFontSize(11)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                        table6.AddCell(new Cell().Add(new Paragraph(row.Datenaissance)).SetFont(fontNormal).SetFontSize(11)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                        table6.AddCell(new Cell().Add(new Paragraph(row.Analyse)).SetFont(fontNormal).SetFontSize(11)
                            .SetTextAlignment(TextAlignment.LEFT)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                        table6.AddCell(new Cell().Add(new Paragraph(row.Montant.ToString("N0", cultureFr))).SetFont(fontNormal).SetFontSize(11)
                            .SetTextAlignment(TextAlignment.RIGHT)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));
                    }

                    // Ajouter la sous-table au tableau principal
                    mainTable.AddCell(new Cell().Add(table6).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                    // === Ligne 4 : Espace vide 0.5 cm ===
                    mainTable.AddCell(new Cell()
                        .SetHeight(CmToPt(0.5f))
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                    // === Ajouter au document ===
                    document.Add(mainTable);
                }

                // Trait noir largeur 19 cm
                {
                    document.Add(new Paragraph().SetMarginTop(CmToPt(0.25f)));
                    document.Add(new LineSeparator(new SolidLine()).SetWidth(UnitValue.CreatePercentValue(100)));
                    document.Add(new Paragraph().SetMarginTop(CmToPt(0.25f)));
                }

                // Totaux finaux 
                // Ligne 1 : Total : | [valeur]
                {
                    var table = new Table(UnitValue.CreatePointArray(new float[] { CmToPt(15), CmToPt(4) }));
                    table.AddCell(CreateCell("Total :", fontBold, 11, TextAlignment.RIGHT));
                    table.AddCell(CreateCell(totaux.ToString("N0", cultureFr), fontNormal, 11, TextAlignment.RIGHT));
                    document.Add(table);
                }

                document.Close();

                byte[] pdfBytes = stream.ToArray();

                // Exemple de réouverture et modification
                using (var input = new MemoryStream(pdfBytes))
                using (var output = new MemoryStream())
                {
                    var reader = new PdfReader(input);
                    var writer1 = new PdfWriter(output);
                    var pdfDoc = new PdfDocument(reader, writer1);

                    // Ajout des numéros de page (code selon besoin)
                    float height = CmToPt(0.6f); // Hauteur du footer
                    float width = CmToPt(2f);    // Largeur réservée pour le numéro de page
                    var fontFooter = PdfFontFactory.CreateFont(GetFontPath(true, false), PdfEncodings.IDENTITY_H);

                    // Nombre total de pages
                    int numberOfPages = pdfDoc.GetNumberOfPages();

                    //Parcours des pages
                    for (int i = 1; i <= numberOfPages; i++)
                    {
                        PdfPage page = pdfDoc.GetPage(i);

                        float x = PageSize.A4.GetWidth() - marginRight - width / 2; // x depuis la droite
                        float y = marginBottom / 2 - height; // centré verticalement dans la marge

                        // Format : 1/3, 2/3, etc.
                        var pageNumberText = $"{i}/{numberOfPages}";

                        PdfCanvas pdfCanvas = new PdfCanvas(page);

                        //// Bordure noire
                        //pdfCanvas.SetLineWidth(1)
                        //  .SetStrokeColor(ColorConstants.BLACK)
                        //  .Rectangle(x, y, width, height)
                        //  .Stroke();

                        // Commence à écrire du texte
                        pdfCanvas.BeginText()
                            .SetFontAndSize(fontFooter, 11)
                            .SetFillColor(ColorConstants.BLACK)
                            .MoveText(x, y)
                            .ShowText(pageNumberText)
                            .EndText()
                            .Release();
                    }

                    pdfDoc.Close();

                    string base64 = Convert.ToBase64String(output.ToArray());

                    return Json(new
                    {
                        success = true,
                        pdfBase64 = base64
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "erreur lors de la génération de rapport");
                throw;
            }
        }

        ////[HttpGet]
        ////public IActionResult GenererRecetteParJourParPartenaire(string date)
        ////{
        ////    if (string.IsNullOrWhiteSpace(date) || !DateTime.TryParse(date, out var parsedDate))
        ////    {
        ////        return BadRequest("Le paramètre date est invalide.");
        ////    }
        ////    var reportPath = System.IO.Path.Combine(_webHostEnvironment.WebRootPath, "reports", "recetteparjourparpartenaire.frx");
        ////    if (!System.IO.File.Exists(reportPath))
        ////        return NotFound("Le rapport n'existe pas.");

        ////    string login = User.Identity?.Name ?? string.Empty;
        ////    string operateur = login;
        ////    var utilisateur = _context.Utilisateurs.FirstOrDefault(u => u.Login == login);
        ////    if (utilisateur != null)
        ////    {
        ////        operateur = $"{utilisateur.Nom} {utilisateur.Prenom}".Trim();
        ////    }

        ////    using (var report = new FastReport.Report())
        ////    {
        ////        report.Load(reportPath);
        ////        report.SetParameterValue("datefacture", DateTime.Parse(date));
        ////        report.SetParameterValue("operateur", operateur);
        ////        report.Prepare();
        ////        using (var ms = new MemoryStream())
        ////        {
        ////            var imageExport = new FastReport.Export.Image.ImageExport
        ////            {
        ////                ImageFormat = FastReport.Export.Image.ImageExportFormat.Png,
        ////                SeparateFiles = false,
        ////                ResolutionX = 300,
        ////                ResolutionY = 300
        ////            };
        ////            report.Export(imageExport, ms);
        ////            ms.Position = 0;
        ////            var base64 = Convert.ToBase64String(ms.ToArray());
        ////            return Json(new { success = true, imageBase64 = "data:image/png;base64," + base64 });
        ////        }
        ////    }
        ////}

        [HttpGet]
        public IActionResult RecetteParPeriode()
        {
            return View(new RecetteParPeriodeVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RecetteParPeriode(RecetteParPeriodeVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            // Pour l'instant, afficher les dates saisies dans la vue
            ViewBag.Message = $"Période saisie : du {vm.Debut} au {vm.Fin}";
            return View(vm);
        }

        public class RequeteLigne
        {
            public DateTime DateFacture { get; set; }
            public Guid EnteteFactureId { get; set; }
            public string Traitement { get; set; }
            public decimal Prix { get; set; }
            public bool EstAssure { get; set; }
            public Guid? Policeassuranceid { get; set; }
            public Guid PatientId { get; set; }
            public string Nom { get; set; }
            public string Prenom { get; set; }
            public string Sexe { get; set; }
            public DateTime DateNaissance { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> GenererRecetteParPeriode(string debut, string fin, string? typeClient)
        {
            // Vérifier que les dates sont valides
            if (string.IsNullOrWhiteSpace(debut) || !DateTime.TryParse(debut, out var parsedDebut) ||
                string.IsNullOrWhiteSpace(fin) || !DateTime.TryParse(fin, out var parsedFin))
            {
                return BadRequest("Les paramètres de dates sont invalides.");
            }

            var normalizedTypeClient = string.IsNullOrWhiteSpace(typeClient)
                ? null
                : typeClient.Trim().ToLowerInvariant();
            if (normalizedTypeClient != null && normalizedTypeClient != "payant" && normalizedTypeClient != "assurance")
            {
                _logger.LogWarning("Paramètre typeClient inconnu: {TypeClient}", typeClient);

                return BadRequest("Le paramètre typeClient est invalide.");
            }

            // Vérifier que la date de début est antérieure à la date de fin
            if (parsedDebut > parsedFin)
            {
                return BadRequest("La date de début doit être antérieure à la date de fin.");
            }

            // utilisateur connecté
            var user = await _usermanager.GetUserAsync(User);

            Utilisateur utilisateur = null;

            if (user != null)
            {
                utilisateur = await _context.Utilisateurs.AsNoTracking()
                    .Include(x => x.CodesiteNavigation)
                    .Where(x => x.Userid == user.Id)
                    .FirstOrDefaultAsync();

                if (utilisateur == null)
                {
                    _logger.LogWarning("Utilisateur non trouvé");

                    return BadRequest("Utilisateur non trouvé");
                }
                else
                {
                    var roles = await _usermanager.GetRolesAsync(user);
                    bool isCaissier = roles.Contains(EnumRoles.Caisse.ToString());

                    if (!isCaissier)
                    {
                        _logger.LogWarning("L'utilisateur ne dispose pas du rôle caissier");

                        return BadRequest("L'utilisateur ne dispose pas du rôle caissier");
                    }
                }
            }

            try
            {
                var cultureFr = new CultureInfo("fr-FR");

                using var stream = new MemoryStream();
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf, PageSize.A4);

                float pageHeight = PageSize.A4.GetHeight();

                float marginTop = document.GetTopMargin();
                float marginBottom = document.GetBottomMargin();
                float marginLeft = document.GetLeftMargin();
                float marginRight = document.GetRightMargin();

                Console.WriteLine($"Top: {marginTop}, Bottom: {marginBottom}, Left: {marginLeft}, Right: {marginRight}");

                // S’assurer que les encodages étendus sont disponibles
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                PdfFont fontNormal = PdfFontFactory.CreateFont(GetFontPath(true, false), PdfEncodings.IDENTITY_H);
                PdfFont fontBold = PdfFontFactory.CreateFont(GetFontPath(false, true), PdfEncodings.IDENTITY_H);

                Console.WriteLine($"Nomral: {GetFontPath(true, false)}, Bold: {GetFontPath(false, true)}");

                Console.WriteLine($"Top: {marginTop}, Bottom: {marginBottom}, Left: {marginLeft}, Right: {marginRight}");

                // S’assurer que les encodages étendus sont disponibles
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                // === Table d'en-tête (1 ligne, 3 colonnes) ===
                float totalWidth = PageSize.A4.GetWidth() - 36 - 36; // largeur utile = largeur page - marges
                float col1Width = CmToPt(2.75f);
                float col3Width = CmToPt(5.75f);
                float col2Width = totalWidth - col1Width - col3Width;

                Table enteteTable = new Table(new float[] { col1Width, col2Width, col3Width });
                enteteTable.SetWidth(UnitValue.CreatePointValue(totalWidth));
                enteteTable.SetFixedLayout(); // pour que les colonnes respectent les tailles fixées

                // === Colonne 1 : Image ===
                string imagePath = System.IO.Path.Combine(_webHostEnvironment.WebRootPath, "img", "inh_togo_logo.png");
                Image img = new Image(ImageDataFactory.Create(imagePath))
                    .SetWidth(CmToPt(2.75f))
                    .SetHeight(CmToPt(1.75f));

                enteteTable.AddCell(new Cell().Add(img).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                // === Colonne 2 : espace vide ===
                enteteTable.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                // === Colonne 3 : sous-table 3 lignes ===
                Table subTable = new Table(1).SetWidth(UnitValue.CreatePointValue(col3Width));

                subTable.AddCell(new Cell()
                    .Add(new Paragraph("REPUBLIQUE TOGOLAISE")
                    .SetFont(fontNormal).SetFontSize(10).SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                subTable.AddCell(new Cell()
                    .Add(new Paragraph("Travail - Liberté - Patrie")
                    .SetFont(fontNormal).SetFontSize(10).SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                subTable.AddCell(new Cell()
                    .SetHeight(CmToPt(0.25f)) // espace vide
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                enteteTable.AddCell(new Cell().Add(subTable).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                // === Ajouter au document
                document.Add(enteteTable);

                // Valeur dynamique de l'opérateur
                string operateur = "";

                // Récupérer l'utilisateur connecté
                //string login = User.Identity?.Name ?? string.Empty;
                //operateur = login;
                //var utilisateur = _context.Utilisateurs.FirstOrDefault(u => u.Login == login);
                if (utilisateur != null)
                {
                    operateur = $"{utilisateur.Nom} {utilisateur.Prenom}".Trim();
                }

                // Texte à insérer
                var typeClientLabel = normalizedTypeClient == null
                    ? "Tous clients"
                    : normalizedTypeClient == "assurance" ? "Assurances" : "Clients payants";
                string texte = $"Recettes de période : de {parsedDebut.ToShortDateString()} à {parsedFin.ToShortDateString()} - {typeClientLabel}";

                // Création du paragraphe
                var paragraph = new Paragraph(texte)
                    .SetFont(fontBold)
                    .SetFontSize(12)
                    .SetTextAlignment(TextAlignment.CENTER); // centré horizontalement

                // Ajouter au document
                document.Add(paragraph);

                // Sous-requête 1 : Catégories
                var q1 = from P in _context.Entetefactures
                         join p1 in _context.Entetedemandes on P.Entetedemandeid equals p1.Entetedemandeid
                         join p2 in _context.Detailfactures on P.Entetefactureid equals p2.Entetefactureid
                         join p6 in _context.Detaildemandes on p2.Detaildemandeid equals p6.Detaildemandeid
                         join p3 in _context.Categories on p6.Categorieid equals p3.Categorieid
                         join p4 in _context.Patients on p1.Patientid equals p4.Patientid
                         join p5 in _context.Sexes on p4.Codesexe equals p5.Codesexe
                         where P.Date.Date >= parsedDebut.Date
                            && P.Date.Date <= parsedFin.Date
                            && P.Utilisateurid == utilisateur.Utilisateurid
                         select new RequeteLigne
                         {
                             DateFacture = P.Date,
                             EnteteFactureId = P.Entetefactureid,
                             Traitement = p3.Nom,
                             Prix = p6.Prix,
                             EstAssure = p1.Policeassuranceid != null,
                             Policeassuranceid = p1.Policeassuranceid,
                             PatientId = p4.Patientid,
                             Nom = p4.Nom,
                             Prenom = p4.Prenom,
                             Sexe = p5.Value,
                             DateNaissance = p4.Datenaissance
                         };

                // Sous-requête 2 : Analyses
                var q2 = from P in _context.Entetefactures
                         join p1 in _context.Detailfactures on P.Entetefactureid equals p1.Entetefactureid
                         join p2 in _context.Entetedemandes on P.Entetedemandeid equals p2.Entetedemandeid
                         join p3 in _context.Patients on p2.Patientid equals p3.Patientid
                         join p4 in _context.Sexes on p3.Codesexe equals p4.Codesexe
                         join p5 in _context.Detaildemandes on p1.Detaildemandeid equals p5.Detaildemandeid
                         join p6 in _context.Analyses on p5.Idanalyse equals p6.Idanalyse
                         where P.Date.Date >= parsedDebut.Date
                            && P.Date.Date <= parsedFin.Date
                            && P.Utilisateurid == utilisateur.Utilisateurid
                         select new RequeteLigne
                         {
                             DateFacture = P.Date,
                             EnteteFactureId = P.Entetefactureid,
                             Traitement = p6.Nom,
                             Prix = p5.Prix,
                             EstAssure = p2.Policeassuranceid != null,
                             Policeassuranceid = p2.Policeassuranceid,
                             PatientId = p3.Patientid,
                             Nom = p3.Nom,
                             Prenom = p3.Prenom,
                             Sexe = p4.Value,
                             DateNaissance = p3.Datenaissance
                         };

                // UNION ALL des deux requêtes et passage en mémoire
                var union = q1.Concat(q2).ToList();

                if (normalizedTypeClient == "assurance")
                {
                    union = union.Where(x => x.EstAssure).ToList();
                }
                else if (normalizedTypeClient == "payant")
                {
                    union = union.Where(x => !x.EstAssure).ToList();
                }

                var rows = union
                    .GroupBy(x => new
                    {
                        x.Policeassuranceid,
                        x.DateFacture,
                        x.EnteteFactureId,
                        x.PatientId,
                        x.Nom,
                        x.Prenom,
                        x.Sexe,
                        x.DateNaissance
                    })
                    .Select(g => new
                    {
                        Policeassuranceid = g.Key.Policeassuranceid,
                        Date = g.Key.DateFacture.ToString("dd/MM/yyyy"),
                        Patient = $"{g.Key.Nom} {g.Key.Prenom}",
                        Sexe = g.Key.Sexe,
                        Datenaissance = g.Key.DateNaissance.ToString("dd/MM/yyyy"),
                        Analyse = string.Join(", ", g.Select(e => e.Traitement).Distinct()),
                        Montant = g.Sum(e => e.Prix),
                    })
                    .ToList();

                // === Ligne 3 : Table 6 colonnes ===
                float[] colWidths = {
                    CmToPt(2.5f),
                    CmToPt(4.25f),
                    CmToPt(2.5f),
                    CmToPt(2.5f),
                    CmToPt(4.25f),
                    CmToPt(2.5f)
                };

                Table table6 = new Table(colWidths);
                table6.SetWidth(UnitValue.CreatePointValue(colWidths.Sum()));

                // === Header ===
                string[] headers = { "DATE", "PATIENT", "SEXE", "DATE DE NAISSANCE", "ANALYSES", "MONTANT" };
                decimal totaux = 0;

                if (normalizedTypeClient == "assurance")
                {
                    var policeIds = rows
                        .Where(r => r.Policeassuranceid.HasValue)
                        .Select(r => r.Policeassuranceid.Value)
                        .Distinct()
                        .ToList();

                    var policeInfos = (from police in _context.Policeassurances
                                       join assurance in _context.Assurances on police.Codeassurance equals assurance.Codeassurance
                                       where policeIds.Contains(police.Policeassuranceid)
                                       select new
                                       {
                                           police.Policeassuranceid,
                                           police.Libelle,
                                           police.Taux,
                                           AssuranceNom = assurance.Nom
                                       })
                                       .ToDictionary(x => x.Policeassuranceid, x => x);

                    var rowsByPolice = rows
                        .GroupBy(r => r.Policeassuranceid)
                        .OrderBy(g =>
                        {
                            if (g.Key.HasValue && policeInfos.TryGetValue(g.Key.Value, out var info))
                            {
                                return info.AssuranceNom + " " + info.Libelle;
                            }

                            return string.Empty;
                        })
                        .ToList();

                    foreach (var group in rowsByPolice)
                    {
                        string policeTitre = "ASSURANCE INCONNUE";
                        if (group.Key.HasValue && policeInfos.TryGetValue(group.Key.Value, out var info))
                        {
                            var tauxLabel = info.Taux.ToString("0.##", cultureFr);
                            policeTitre = $"{info.AssuranceNom}: {tauxLabel}%";
                        }

                        document.Add(new Paragraph(policeTitre)
                            .SetFont(fontBold)
                            .SetFontSize(11)
                            .SetTextAlignment(TextAlignment.LEFT)
                            .SetMarginTop(CmToPt(0.35f))
                            .SetMarginBottom(CmToPt(0.15f)));

                        var tableAssurance = new Table(colWidths);
                        tableAssurance.SetWidth(UnitValue.CreatePointValue(colWidths.Sum()));

                        foreach (var header in headers)
                        {
                            tableAssurance.AddHeaderCell(new Cell()
                                .Add(new Paragraph(header).SetFont(fontNormal).SetFontSize(10))
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                .SetHeight(CmToPt(1f))
                                .SetBorder(new SolidBorder(ColorConstants.BLACK, 1))
                            );
                        }

                        foreach (var row in group)
                        {
                            tableAssurance.AddCell(new Cell().Add(new Paragraph(row.Date)).SetFont(fontNormal).SetFontSize(11)
                                .SetHeight(CmToPt(0.75f))
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                            tableAssurance.AddCell(new Cell().Add(new Paragraph(row.Patient)).SetFont(fontNormal).SetFontSize(11)
                                .SetTextAlignment(TextAlignment.LEFT)
                                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                            tableAssurance.AddCell(new Cell().Add(new Paragraph(row.Sexe)).SetFont(fontNormal).SetFontSize(11)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                            tableAssurance.AddCell(new Cell().Add(new Paragraph(row.Datenaissance)).SetFont(fontNormal).SetFontSize(11)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                            tableAssurance.AddCell(new Cell().Add(new Paragraph(row.Analyse)).SetFont(fontNormal).SetFontSize(11)
                                .SetTextAlignment(TextAlignment.LEFT)
                                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                            tableAssurance.AddCell(new Cell().Add(new Paragraph(row.Montant.ToString("N0", cultureFr))).SetFont(fontNormal).SetFontSize(11)
                                .SetTextAlignment(TextAlignment.RIGHT)
                                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));
                        }

                        document.Add(tableAssurance);

                        var totalPolice = group.Sum(r => r.Montant);
                        totaux += totalPolice;

                        var totalPoliceTable = new Table(UnitValue.CreatePointArray(new float[] { CmToPt(15), CmToPt(4) }));
                        totalPoliceTable.AddCell(CreateCell("Total police :", fontBold, 11, TextAlignment.RIGHT));
                        totalPoliceTable.AddCell(CreateCell(totalPolice.ToString("N0", cultureFr), fontNormal, 11, TextAlignment.RIGHT));
                        document.Add(totalPoliceTable);

                        document.Add(new Paragraph().SetMarginTop(CmToPt(0.1f)));
                    }
                }
                else
                {
                    foreach (var header in headers)
                    {
                        table6.AddHeaderCell(new Cell()
                            .Add(new Paragraph(header).SetFont(fontNormal).SetFontSize(10))
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .SetHeight(CmToPt(1f))
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1))
                        );
                    }

                    foreach (var row in rows)
                    {
                        totaux += row.Montant;

                        table6.AddCell(new Cell().Add(new Paragraph(row.Date)).SetFont(fontNormal).SetFontSize(11)
                            .SetHeight(CmToPt(0.75f))
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                        table6.AddCell(new Cell().Add(new Paragraph(row.Patient)).SetFont(fontNormal).SetFontSize(11)
                            .SetTextAlignment(TextAlignment.LEFT)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                        table6.AddCell(new Cell().Add(new Paragraph(row.Sexe)).SetFont(fontNormal).SetFontSize(11)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                        table6.AddCell(new Cell().Add(new Paragraph(row.Datenaissance)).SetFont(fontNormal).SetFontSize(11)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                        table6.AddCell(new Cell().Add(new Paragraph(row.Analyse)).SetFont(fontNormal).SetFontSize(11)
                            .SetTextAlignment(TextAlignment.LEFT)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                        table6.AddCell(new Cell().Add(new Paragraph(row.Montant.ToString("N0", cultureFr))).SetFont(fontNormal).SetFontSize(11)
                            .SetTextAlignment(TextAlignment.RIGHT)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));
                    }

                    document.Add(table6);
                }

                // Trait noir largeur 19 cm
                {
                    document.Add(new Paragraph().SetMarginTop(CmToPt(0.25f)));
                    document.Add(new LineSeparator(new SolidLine()).SetWidth(UnitValue.CreatePercentValue(100)));
                    document.Add(new Paragraph().SetMarginTop(CmToPt(0.25f)));
                }

                // Totaux finaux 
                // Ligne 1 : Total : | [valeur]
                {
                    var table = new Table(UnitValue.CreatePointArray(new float[] { CmToPt(15), CmToPt(4) }));
                    table.AddCell(CreateCell("Total :", fontBold, 11, TextAlignment.RIGHT));
                    table.AddCell(CreateCell(totaux.ToString("N0", cultureFr), fontNormal, 11, TextAlignment.RIGHT));
                    document.Add(table);
                }

                document.Close();

                byte[] pdfBytes = stream.ToArray();

                // Exemple de réouverture et modification
                using (var input = new MemoryStream(pdfBytes))
                using (var output = new MemoryStream())
                {
                    var reader = new PdfReader(input);
                    var writer1 = new PdfWriter(output);
                    var pdfDoc = new PdfDocument(reader, writer1);

                    // Ajout des numéros de page (code selon besoin)
                    float height = CmToPt(0.6f); // Hauteur du footer
                    float width = CmToPt(2f);    // Largeur réservée pour le numéro de page
                    var fontFooter = PdfFontFactory.CreateFont(GetFontPath(true, false), PdfEncodings.IDENTITY_H);

                    // Nombre total de pages
                    int numberOfPages = pdfDoc.GetNumberOfPages();

                    //Parcours des pages
                    for (int i = 1; i <= numberOfPages; i++)
                    {
                        PdfPage page = pdfDoc.GetPage(i);

                        float x = PageSize.A4.GetWidth() - marginRight - width / 2; // x depuis la droite
                        float y = marginBottom / 2 - height; // centré verticalement dans la marge

                        // Format : 1/3, 2/3, etc.
                        var pageNumberText = $"{i}/{numberOfPages}";

                        PdfCanvas pdfCanvas = new PdfCanvas(page);

                        //// Bordure noire
                        //pdfCanvas.SetLineWidth(1)
                        //  .SetStrokeColor(ColorConstants.BLACK)
                        //  .Rectangle(x, y, width, height)
                        //  .Stroke();

                        // Commence à écrire du texte
                        pdfCanvas.BeginText()
                            .SetFontAndSize(fontFooter, 11)
                            .SetFillColor(ColorConstants.BLACK)
                            .MoveText(x, y)
                            .ShowText(pageNumberText)
                            .EndText()
                            .Release();
                    }

                    pdfDoc.Close();

                    string base64 = Convert.ToBase64String(output.ToArray());

                    return Json(new
                    {
                        success = true,
                        pdfBase64 = base64
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "erreur lors de la génération de rapport");
                throw;
            }
}

        //[HttpGet]
        //public IActionResult GenererRecetteParPeriode(string debut, string fin)
        //{
        //    // Vérifier que les dates sont valides
        //    if (string.IsNullOrWhiteSpace(debut) || !DateTime.TryParse(debut, out var parsedDebut) ||
        //        string.IsNullOrWhiteSpace(fin) || !DateTime.TryParse(fin, out var parsedFin))
        //    {
        //        return BadRequest("Les paramètres de dates sont invalides.");
        //    }

        //    // Vérifier que la date de début est antérieure à la date de fin
        //    if (parsedDebut > parsedFin)
        //    {
        //        return BadRequest("La date de début doit être antérieure à la date de fin.");
        //    }

        //    var reportPath = System.IO.Path.Combine(_webHostEnvironment.WebRootPath, "reports", "recetteparperiode.frx");
        //    if (!System.IO.File.Exists(reportPath))
        //        return NotFound("Le rapport n'existe pas.");

        //    // Récupérer l'utilisateur connecté
        //    string login = User.Identity?.Name ?? string.Empty;
        //    string operateur = login;
        //    var utilisateur = _context.Utilisateurs.FirstOrDefault(u => u.Login == login);
        //    if (utilisateur != null)
        //    {
        //        operateur = $"{utilisateur.Nom} {utilisateur.Prenom}".Trim();
        //    }

        //    using (var report = new FastReport.Report())
        //    {
        //        report.Load(reportPath);
        //        // Passer les deux dates au rapport
        //        report.SetParameterValue("datedebut", parsedDebut.Date);
        //        report.SetParameterValue("datefin", parsedFin.Date);
        //        report.SetParameterValue("datedebutstr", parsedDebut.ToString("dd/MM/yyyy"));
        //        report.SetParameterValue("datefinstr", parsedFin.ToString("dd/MM/yyyy"));
        //        report.SetParameterValue("operateur", operateur);
        //        report.Prepare();
        //        using (var ms = new MemoryStream())
        //        {
        //            var imageExport = new FastReport.Export.Image.ImageExport
        //            {
        //                ImageFormat = FastReport.Export.Image.ImageExportFormat.Png,
        //                SeparateFiles = false,
        //                ResolutionX = 300,
        //                ResolutionY = 300
        //            };
        //            report.Export(imageExport, ms);
        //            ms.Position = 0;
        //            var base64 = Convert.ToBase64String(ms.ToArray());
        //            return Json(new { success = true, imageBase64 = "data:image/png;base64," + base64 });
        //        }
        //    }
        //}

        [HttpGet]
        public IActionResult RecetteParPeriodeParPartenaire()
        {
            return View(new RecetteParPeriodeVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RecetteParPeriodeParPartenaire(RecetteParPeriodeVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            ViewBag.Message = $"Période saisie : du {vm.Debut} au {vm.Fin}";
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GenererRecetteParPeriodeParPartenaire(string debut, string fin)
        {
            if (string.IsNullOrWhiteSpace(debut) || !DateTime.TryParse(debut, out var parsedDebut) ||
                string.IsNullOrWhiteSpace(fin) || !DateTime.TryParse(fin, out var parsedFin))
            {
                return BadRequest("Les paramètres de dates sont invalides.");
            }

            if (parsedDebut > parsedFin)
            {
                return BadRequest("La date de début doit être antérieure à la date de fin.");
            }

            // utilisateur connecté
            var user = await _usermanager.GetUserAsync(User);

            Utilisateur utilisateur = null;

            if (user != null)
            {
                utilisateur = await _context.Utilisateurs.AsNoTracking()
                    .Include(x => x.CodesiteNavigation)
                    .Where(x => x.Userid == user.Id)
                    .FirstOrDefaultAsync();

                if (utilisateur == null)
                {
                    _logger.LogWarning("Utilisateur non trouvé");

                    return BadRequest("Utilisateur non trouvé");
                }
                else
                {
                    var roles = await _usermanager.GetRolesAsync(user);
                    bool isCaissier = roles.Contains(EnumRoles.Caisse.ToString());

                    if (!isCaissier)
                    {
                        _logger.LogWarning("L'utilisateur ne dispose pas du rôle caissier");

                        return BadRequest("L'utilisateur ne dispose pas du rôle caissier");
                    }
                }
            }

            try
            {
                var cultureFr = new CultureInfo("fr-FR");

                using var stream = new MemoryStream();
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf, PageSize.A4);
                document.SetMargins(36, 36, 36, 36);

                float pageHeight = PageSize.A4.GetHeight();

                float marginTop = document.GetTopMargin();
                float marginBottom = document.GetBottomMargin();
                float marginLeft = document.GetLeftMargin();
                float marginRight = document.GetRightMargin();

                Console.WriteLine($"Top: {marginTop}, Bottom: {marginBottom}, Left: {marginLeft}, Right: {marginRight}");

                // S’assurer que les encodages étendus sont disponibles
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                PdfFont fontNormal = PdfFontFactory.CreateFont(GetFontPath(true, false), PdfEncodings.IDENTITY_H);
                PdfFont fontBold = PdfFontFactory.CreateFont(GetFontPath(false, true), PdfEncodings.IDENTITY_H);

                Console.WriteLine($"Normal: {GetFontPath(true, false)}, Bold: {GetFontPath(false, true)}");

                // === Table d'en-tête (1 ligne, 3 colonnes) ===
                float totalWidth = PageSize.A4.GetWidth() - 36 - 36; // largeur utile = largeur page - marges
                float col1Width = CmToPt(2.75f);
                float col3Width = CmToPt(5.75f);
                float col2Width = totalWidth - col1Width - col3Width;

                Table enteteTable = new Table(new float[] { col1Width, col2Width, col3Width });
                enteteTable.SetWidth(UnitValue.CreatePointValue(totalWidth));
                enteteTable.SetFixedLayout(); // pour que les colonnes respectent les tailles fixées

                // === Colonne 1 : Image ===
                string imagePath = System.IO.Path.Combine(_webHostEnvironment.WebRootPath, "img", "inh_togo_logo.png");
                Image img = new Image(ImageDataFactory.Create(imagePath))
                    .SetWidth(CmToPt(2.75f))
                    .SetHeight(CmToPt(1.75f));

                enteteTable.AddCell(new Cell().Add(img).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                // === Colonne 2 : espace vide ===
                enteteTable.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                // === Colonne 3 : sous-table 3 lignes ===
                Table subTable = new Table(1).SetWidth(UnitValue.CreatePointValue(col3Width));

                subTable.AddCell(new Cell()
                    .Add(new Paragraph("REPUBLIQUE TOGOLAISE")
                    .SetFont(fontNormal).SetFontSize(10).SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                subTable.AddCell(new Cell()
                    .Add(new Paragraph("Travail - Liberté - Patrie")
                    .SetFont(fontNormal).SetFontSize(10).SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                subTable.AddCell(new Cell()
                    .SetHeight(CmToPt(0.25f)) // espace vide
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                enteteTable.AddCell(new Cell().Add(subTable).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                // === Ajouter au document
                document.Add(enteteTable);

                // Valeur dynamique de l'opérateur
                string operateur = "";

                // Récupérer l'utilisateur connecté
                //string login = User.Identity?.Name ?? string.Empty;
                //operateur = login;
                //var utilisateur = _context.Utilisateurs.FirstOrDefault(u => u.Login == login);
                if (utilisateur != null)
                {
                    operateur = $"{utilisateur.Nom} {utilisateur.Prenom}".Trim();
                }

                // Texte à insérer
                string texte = $"Recettes de période : de {parsedDebut.ToShortDateString()} à {parsedFin.ToShortDateString()}";

                // Création du paragraphe
                var paragraph = new Paragraph(texte)
                    .SetFont(fontBold)
                    .SetFontSize(12)
                    .SetTextAlignment(TextAlignment.CENTER); // centré horizontalement

                // Ajouter au document
                document.Add(paragraph);

                var partenaires = (
                    from p in _context.Entetefactures
                    join d in _context.Entetedemandes on p.Entetedemandeid equals d.Entetedemandeid
                    join partenaire in _context.Partenaires on d.Partenaireid equals partenaire.Partenaireid
                    where p.Date.Date >= parsedDebut.Date &&
                          p.Date.Date <= parsedFin.Date &&
                          d.Utilisateurid == utilisateur.Utilisateurid
                    select new
                    {
                        partenaire.Partenaireid,
                        partenaire.Nom,
                        EnteteFactureId = p.Entetefactureid,
                        Date = p.Date,
                        EnteteDemandeId = d.Entetedemandeid
                    })
                    .AsEnumerable()
                    .GroupBy(x => x.Partenaireid)
                    .Select(g => g.OrderBy(x => x.Date).FirstOrDefault()) // Simule DISTINCT ON
                    .ToList()
                    .Select(x => new
                    {
                        x.Partenaireid,
                        x.Nom,
                    })
                .ToList();

                decimal totaux = 0;

                foreach (var partenaire in partenaires)
                {
                    var q1 = from P in _context.Entetefactures
                             join p1 in _context.Entetedemandes on P.Entetedemandeid equals p1.Entetedemandeid
                             join p2 in _context.Detailfactures on P.Entetefactureid equals p2.Entetefactureid
                             join p6 in _context.Detaildemandes on p2.Detaildemandeid equals p6.Detaildemandeid
                             join p3 in _context.Categories on p6.Categorieid equals p3.Categorieid
                             join p4 in _context.Patients on p1.Patientid equals p4.Patientid
                             join p5 in _context.Sexes on p4.Codesexe equals p5.Codesexe
                             join p7 in _context.Partenaires on p1.Partenaireid equals p7.Partenaireid
                             where P.Date.Date >= parsedDebut.Date 
                                && P.Date.Date <= parsedFin.Date
                                && p7.Partenaireid == partenaire.Partenaireid
                                && p1.Utilisateurid == utilisateur.Utilisateurid
                             select new 
                             {
                                 DateFacture = P.Date,
                                 EnteteFactureId = P.Entetefactureid,
                                 Traitement = p3.Nom,
                                 Prix = p6.Prix,
                                 PatientId = p4.Patientid,
                                 Nom = p4.Nom,
                                 Prenom = p4.Prenom,
                                 Sexe = p5.Value,
                                 DateNaissance = p4.Datenaissance
                             };

                    var q2 = from P in _context.Entetefactures
                             join p1 in _context.Detailfactures on P.Entetefactureid equals p1.Entetefactureid
                             join p2 in _context.Entetedemandes on P.Entetedemandeid equals p2.Entetedemandeid
                             join p3 in _context.Patients on p2.Patientid equals p3.Patientid
                             join p4 in _context.Sexes on p3.Codesexe equals p4.Codesexe
                             join p5 in _context.Detaildemandes on p1.Detaildemandeid equals p5.Detaildemandeid
                             join p6 in _context.Analyses on p5.Idanalyse equals p6.Idanalyse
                             join p7 in _context.Partenaires on p2.Partenaireid equals p7.Partenaireid
                             where P.Date.Date >= parsedDebut.Date 
                                && P.Date.Date <= parsedFin.Date
                                && p7.Partenaireid == partenaire.Partenaireid
                                && p2.Utilisateurid == utilisateur.Utilisateurid
                             select new 
                             {
                                 DateFacture = P.Date,
                                 EnteteFactureId = P.Entetefactureid,
                                 Traitement = p6.Nom,
                                 Prix = p5.Prix,
                                 PatientId = p3.Patientid,
                                 Nom = p3.Nom,
                                 Prenom = p3.Prenom,
                                 Sexe = p4.Value,
                                 DateNaissance = p3.Datenaissance
                             };

                    var union = q1.Concat(q2).ToList();

                    var rows = union
                        .GroupBy(x => new
                        {
                            x.DateFacture,
                            x.EnteteFactureId,
                            x.PatientId,
                            x.Nom,
                            x.Prenom,
                            x.Sexe,
                            x.DateNaissance
                        })
                        .Select(g => new
                        {
                            Date = g.Key.DateFacture.ToString("dd/MM/yyyy"),
                            Patient = $"{g.Key.Nom} {g.Key.Prenom}",
                            Sexe = g.Key.Sexe,
                            Datenaissance = g.Key.DateNaissance.ToString("dd/MM/yyyy"),
                            Analyse = string.Join(", ", g.Select(e => e.Traitement).Distinct()),
                            Montant = g.Sum(e => e.Prix),
                        })
                        .ToList();

                    // === TABLE PRINCIPALE ===
                    Table mainTable = new Table(1).UseAllAvailableWidth();

                    // === Ligne 1 : Table 2 colonnes ===
                    Table subTable1 = new Table(UnitValue.CreatePercentArray(new float[] { 1, 2 }))
                        .UseAllAvailableWidth();

                    subTable1.AddCell(new Cell()
                        .Add(new Paragraph("Partenaire :").SetFont(fontBold).SetFontSize(11))
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    );

                    subTable1.AddCell(new Cell()
                        .Add(new Paragraph(partenaire.Nom).SetFont(fontNormal).SetFontSize(11))
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    );

                    mainTable.AddCell(new Cell().Add(subTable1).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                    // === Ligne 2 : Espace vide 0.25 cm ===
                    mainTable.AddCell(new Cell()
                        .SetHeight(CmToPt(0.25f))
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    );

                    // === Ligne 3 : Table 6 colonnes ===
                    float[] colWidths = {
                        CmToPt(2.5f),
                        CmToPt(4.25f),
                        CmToPt(2.5f),
                        CmToPt(2.5f),
                        CmToPt(4.25f),
                        CmToPt(2.5f)
                    };

                    Table table6 = new Table(colWidths);
                    table6.SetWidth(UnitValue.CreatePointValue(colWidths.Sum()));

                    // === Header ===
                    string[] headers = { "DATE", "PATIENT", "SEXE", "DATE DE NAISSANCE", "ANALYSES", "MONTANT" };

                    foreach (var header in headers)
                    {
                        table6.AddHeaderCell(new Cell()
                            .Add(new Paragraph(header).SetFont(fontNormal).SetFontSize(10))
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .SetHeight(CmToPt(1f))
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1))
                        );
                    }

                    // === Body (exemple avec 20 lignes) ===
                    foreach (var row in rows)
                    {
                        totaux += row.Montant;

                        table6.AddCell(new Cell().Add(new Paragraph(row.Date)).SetFont(fontNormal).SetFontSize(11)
                            .SetHeight(CmToPt(0.75f))
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                        table6.AddCell(new Cell().Add(new Paragraph(row.Patient)).SetFont(fontNormal).SetFontSize(11)
                            .SetTextAlignment(TextAlignment.LEFT)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                        table6.AddCell(new Cell().Add(new Paragraph(row.Sexe)).SetFont(fontNormal).SetFontSize(11)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                        table6.AddCell(new Cell().Add(new Paragraph(row.Datenaissance)).SetFont(fontNormal).SetFontSize(11)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                        table6.AddCell(new Cell().Add(new Paragraph(row.Analyse)).SetFont(fontNormal).SetFontSize(11)
                            .SetTextAlignment(TextAlignment.LEFT)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                        table6.AddCell(new Cell().Add(new Paragraph(row.Montant.ToString("N0", cultureFr))).SetFont(fontNormal).SetFontSize(11)
                            .SetTextAlignment(TextAlignment.RIGHT)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));
                    }

                    // Ajouter la sous-table au tableau principal
                    mainTable.AddCell(new Cell().Add(table6).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                    // === Ligne 4 : Espace vide 0.5 cm ===
                    mainTable.AddCell(new Cell()
                        .SetHeight(CmToPt(0.5f))
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                    // === Ajouter au document ===
                    document.Add(mainTable);
                }

                // Trait noir largeur 19 cm
                {
                    document.Add(new Paragraph().SetMarginTop(CmToPt(0.25f)));
                    document.Add(new LineSeparator(new SolidLine()).SetWidth(UnitValue.CreatePercentValue(100)));
                    document.Add(new Paragraph().SetMarginTop(CmToPt(0.25f)));
                }

                // Totaux finaux 
                // Ligne 1 : Total : | [valeur]
                {
                    var table = new Table(UnitValue.CreatePointArray(new float[] { CmToPt(15), CmToPt(4) }));
                    table.AddCell(CreateCell("Total :", fontBold, 11, TextAlignment.RIGHT));
                    table.AddCell(CreateCell(totaux.ToString("N0", cultureFr), fontNormal, 11, TextAlignment.RIGHT));
                    document.Add(table);
                }

                document.Close();

                byte[] pdfBytes = stream.ToArray();

                // Exemple de réouverture et modification
                using (var input = new MemoryStream(pdfBytes))
                using (var output = new MemoryStream())
                {
                    var reader = new PdfReader(input);
                    var writer1 = new PdfWriter(output);
                    var pdfDoc = new PdfDocument(reader, writer1);

                    // Ajout des numéros de page (code selon besoin)
                    float height = CmToPt(0.6f); // Hauteur du footer
                    float width = CmToPt(2f);    // Largeur réservée pour le numéro de page
                    var fontFooter = PdfFontFactory.CreateFont(GetFontPath(true, false), PdfEncodings.IDENTITY_H);

                    // Nombre total de pages
                    int numberOfPages = pdfDoc.GetNumberOfPages();

                    //Parcours des pages
                    for (int i = 1; i <= numberOfPages; i++)
                    {
                        PdfPage page = pdfDoc.GetPage(i);

                        float x = PageSize.A4.GetWidth() - marginRight - width / 2; // x depuis la droite
                        float y = marginBottom / 2 - height; // centré verticalement dans la marge

                        // Format : 1/3, 2/3, etc.
                        var pageNumberText = $"{i}/{numberOfPages}";

                        PdfCanvas pdfCanvas = new PdfCanvas(page);

                        //// Bordure noire
                        //pdfCanvas.SetLineWidth(1)
                        //  .SetStrokeColor(ColorConstants.BLACK)
                        //  .Rectangle(x, y, width, height)
                        //  .Stroke();

                        // Commence à écrire du texte
                        pdfCanvas.BeginText()
                            .SetFontAndSize(fontFooter, 11)
                            .SetFillColor(ColorConstants.BLACK)
                            .MoveText(x, y)
                            .ShowText(pageNumberText)
                            .EndText()
                            .Release();
                    }

                    pdfDoc.Close();

                    string base64 = Convert.ToBase64String(output.ToArray());

                    return Json(new
                    {
                        success = true,
                        pdfBase64 = base64
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "erreur lors de la génération de rapport");
                throw;
            }
        }

        //[HttpGet]
        //public IActionResult GenererRecetteParPeriodeParPartenaire(string debut, string fin)
        //{
        //    if (string.IsNullOrWhiteSpace(debut) || !DateTime.TryParse(debut, out var parsedDebut) ||
        //        string.IsNullOrWhiteSpace(fin) || !DateTime.TryParse(fin, out var parsedFin))
        //    {
        //        return BadRequest("Les paramètres de dates sont invalides.");
        //    }

        //    if (parsedDebut > parsedFin)
        //    {
        //        return BadRequest("La date de début doit être antérieure à la date de fin.");
        //    }

        //    var reportPath = System.IO.Path.Combine(_webHostEnvironment.WebRootPath, "reports", "recetteparperiodeparpartenaire.frx");
        //    if (!System.IO.File.Exists(reportPath))
        //        return NotFound("Le rapport n'existe pas.");

        //    string login = User.Identity?.Name ?? string.Empty;
        //    string operateur = login;
        //    var utilisateur = _context.Utilisateurs.FirstOrDefault(u => u.Login == login);
        //    if (utilisateur != null)
        //    {
        //        operateur = $"{utilisateur.Nom} {utilisateur.Prenom}".Trim();
        //    }

        //    using (var report = new FastReport.Report())
        //    {
        //        report.Load(reportPath);
        //        report.SetParameterValue("datedebut", parsedDebut.Date);
        //        report.SetParameterValue("datefin", parsedFin.Date);
        //        report.SetParameterValue("datedebutstr", parsedDebut.ToString("dd/MM/yyyy"));
        //        report.SetParameterValue("datefinstr", parsedFin.ToString("dd/MM/yyyy"));
        //        report.SetParameterValue("operateur", operateur);
        //        report.Prepare();
        //        using (var ms = new MemoryStream())
        //        {
        //            var imageExport = new FastReport.Export.Image.ImageExport
        //            {
        //                ImageFormat = FastReport.Export.Image.ImageExportFormat.Png,
        //                SeparateFiles = false,
        //                ResolutionX = 300,
        //                ResolutionY = 300
        //            };
        //            report.Export(imageExport, ms);
        //            ms.Position = 0;
        //            var base64 = Convert.ToBase64String(ms.ToArray());
        //            return Json(new { success = true, imageBase64 = "data:image/png;base64," + base64 });
        //        }
        //    }
        //}

        [HttpGet]
        public IActionResult FactureAssurance()
        {
            var assurances = new List<SelectListItem> { new SelectListItem { Value = "", Text = "---" } };
            assurances.AddRange(_context.Assurances.Select(a => new SelectListItem { 
                Value = a.Codeassurance, 
                Text = a.Nom 
            }));
            ViewBag.Codeassurance = assurances;
            return View(new FactureAssuranceVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult FactureAssurance(FactureAssuranceVM vm)
        {
            var assurances = new List<SelectListItem> { new SelectListItem { Value = "", Text = "---" } };
            assurances.AddRange(_context.Assurances.Select(a => new SelectListItem { 
                Value = a.Codeassurance, 
                Text = a.Nom,
                Selected = a.Codeassurance == vm.Codeassurance
            }));
            ViewBag.Codeassurance = assurances;
            
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            ViewBag.Message = $"Période saisie : du {vm.Debut} au {vm.Fin} pour l'assurance {vm.Codeassurance}";
            return View(vm);
        }

        private Cell CreateCell(string text, PdfFont font, float fontSize, TextAlignment alignment, float? fixedWidth = null, VerticalAlignment verticalAlignment = VerticalAlignment.MIDDLE)
        {
            text ??= "";

            Paragraph paragraph = new Paragraph(text)
                .SetFont(font)
                .SetFontSize(fontSize)
                .SetTextAlignment(alignment);

            Cell cell = new Cell()
                .Add(paragraph)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetVerticalAlignment(verticalAlignment);

            if (fixedWidth.HasValue)
            {
                cell.SetWidth(fixedWidth.Value);
            }

            return cell;
        }

        // Surcharge simplifiée pour les appels avec seulement texte et police
        private Cell CreateCell(string text, PdfFont font)
        {
            return CreateCell(text, font, 9, TextAlignment.CENTER);
        }

        [HttpGet]
        public IActionResult GenererFactureAssurance(string debut, string fin, string codeassurance)
        {
            var cultureFr = new CultureInfo("fr-FR");
            
            if (string.IsNullOrWhiteSpace(debut) || !DateTime.TryParse(debut, cultureFr, DateTimeStyles.None, out var parsedDebut) ||
                string.IsNullOrWhiteSpace(fin) || !DateTime.TryParse(fin, cultureFr, DateTimeStyles.None, out var parsedFin))
            {
                return BadRequest("Les paramètres de dates sont invalides.");
            }

            if (parsedDebut > parsedFin)
            {
                return BadRequest("La date de début doit être antérieure à la date de fin.");
            }
            
            // Validation pour éviter des dates futures
            var currentYear = DateTime.Now.Year;
            if (parsedDebut.Year > currentYear || parsedFin.Year > currentYear)
            {
                return BadRequest("Les dates ne peuvent pas être supérieures à l'année actuelle (" + currentYear.ToString() + ").");
            }

            if (string.IsNullOrWhiteSpace(codeassurance))
            {
                return BadRequest("Le code assurance est obligatoire.");
            }

            // Récupérer l'assurance
            var assurance = _context.Assurances.FirstOrDefault(a => a.Codeassurance == codeassurance);
            if (assurance == null)
            {
                return BadRequest("Assurance introuvable.");
            }

            // Récupérer les détails de demandes avec les objets dépendants
            var detaildemandes = _context.Detaildemandes
                .Include(dd => dd.Entetedemande)
                    .ThenInclude(ed => ed.Policeassurance)
                .Include(dd => dd.Detailfactures)
                    .ThenInclude(df => df.Entetefacture)
                .Where(dd => dd.Entetedemande.Policeassurance.Codeassurance == codeassurance &&
                            dd.Entetedemande.Date.Date >= parsedDebut.Date &&
                            dd.Entetedemande.Date.Date <= parsedFin.Date)
                .ToList();

            // Calculer les montants
            var montantTotal = detaildemandes.Sum(dd => dd.Prix);
            var remiseTotal = montantTotal * 0.20m; // 20% du montant total
            var montantNet = montantTotal - remiseTotal;
            var sommeTotal = NumberToWordsConverter.Convert(montantTotal).ToUpper();
            var sommeRemise = NumberToWordsConverter.Convert(remiseTotal).ToUpper();
            var sommeNet = NumberToWordsConverter.Convert(montantNet).ToUpper();

            var query1 = (from p in _context.Entetefactures
                          join p1 in _context.Entetedemandes on p.Entetedemandeid equals p1.Entetedemandeid
                          join p2 in _context.Detaildemandes on p1.Entetedemandeid equals p2.Entetedemandeid
                          join p3 in _context.Categories on p2.Categorieid equals p3.Categorieid
                          join p5 in _context.Policeassurances on p1.Policeassuranceid equals p5.Policeassuranceid
                          join p4 in _context.Assurances on p5.Codeassurance equals p4.Codeassurance
                          where p.Date.Date >= parsedDebut.Date && p.Date.Date <= parsedFin.Date
                                && p4.Codeassurance == assurance.Codeassurance
                          group new { p, p2, p3, p4 } by new { 
                              p.Date, 
                              p3.Categorieid, 
                              p3.Nom, 
                              p2.Prix, 
                              p2.Partassurance, 
                              p2.Partpatient, 
                              p2.Complement, 
                              p2.Net, 
                              p4.Codeassurance
                          } into g
                          select new
                          {
                              Date = g.Key.Date,
                              Id = g.Key.Categorieid,
                              Nom = g.Key.Nom,
                              Nbre = g.Count(),
                              Prix = g.Key.Prix,
                              Montant = g.Count() * g.Key.Prix,
                              PartAssurance = g.Key.Partassurance,
                              PartPatient = g.Key.Partpatient,
                              Complement = g.Key.Complement,
                              Net = g.Key.Net,
                              CodeAssurance = g.Key.Codeassurance,
                          });

            var query2 = (from p in _context.Entetefactures
                          join p1 in _context.Entetedemandes on p.Entetedemandeid equals p1.Entetedemandeid
                          join p2 in _context.Detaildemandes on p1.Entetedemandeid equals p2.Entetedemandeid
                          join p3 in _context.Analyses on p2.Idanalyse equals p3.Idanalyse
                          join p4 in _context.Policeassurances on p1.Policeassuranceid equals p4.Policeassuranceid
                          join p5 in _context.Assurances on p4.Codeassurance equals p5.Codeassurance
                          where p.Date.Date >= parsedDebut.Date && p.Date.Date <= parsedFin.Date
                                && p5.Codeassurance == assurance.Codeassurance
                          group new { p, p2, p3, p5 } by new { 
                              p.Date, 
                              p3.Idanalyse, 
                              p3.Nom, 
                              p2.Prix, 
                              p2.Partassurance, 
                              p2.Partpatient, 
                              p2.Complement, 
                              p2.Net, 
                              p5.Codeassurance
                          } into g
                          select new
                          {
                              Date = g.Key.Date,
                              Id = g.Key.Idanalyse,
                              Nom = g.Key.Nom,
                              Nbre = g.Count(),
                              Prix = g.Key.Prix,
                              Montant = g.Count() * g.Key.Prix,
                              PartAssurance = g.Key.Partassurance,
                              PartPatient = g.Key.Partpatient,
                              Complement = g.Key.Complement,
                              Net = g.Key.Net,
                              CodeAssurance = g.Key.Codeassurance,
                          });

            var toutesLesCategoriesEtAnalyses = query1.Concat(query2)
                                   .OrderByDescending(x => x.Date)
                                   .Select(x => new
                                   {
                                       Date = x.Date.ToString("dd/MM/yyyy"),
                                       x.Id,
                                       x.Nom,
                                       x.Nbre,
                                       x.Prix,
                                       x.Montant,
                                       Remise = Math.Round(x.Montant * 0.20m, 2)
                                   });

            try
            {

                using var stream = new MemoryStream();
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf, PageSize.A4);
                document.SetMargins(36, 36, 36, 36);

                float pageHeight = PageSize.A4.GetHeight();

                float marginTop = document.GetTopMargin();
                float marginBottom = document.GetBottomMargin();
                float marginLeft = document.GetLeftMargin();
                float marginRight = document.GetRightMargin();

                Console.WriteLine($"Top: {marginTop}, Bottom: {marginBottom}, Left: {marginLeft}, Right: {marginRight}");

                // S’assurer que les encodages étendus sont disponibles
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                PdfFont fontNormal = PdfFontFactory.CreateFont(GetFontPath(true, false), PdfEncodings.IDENTITY_H);
                PdfFont fontBold = PdfFontFactory.CreateFont(GetFontPath(false, true), PdfEncodings.IDENTITY_H);

                Console.WriteLine($"Normal: {GetFontPath(true, false)}, Bold: {GetFontPath(false, true)}");

                var dateDebut = parsedDebut.ToShortDateString();
                var dateFin = parsedFin.ToShortDateString();
                var date = DateTime.Now;
                var dateImpression = $"{date.ToShortDateString()} {date.ToShortTimeString()}";
                var doit = assurance.Nom;

                string login = User.Identity?.Name ?? string.Empty;
                string operateur = login;
                var utilisateur = _context.Utilisateurs.FirstOrDefault(u => u.Login == login);
                if (utilisateur != null)
                {
                    operateur = $"{utilisateur.Nom} {utilisateur.Prenom}".Trim();
                }

                // Générer le numéro de facture
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var idinh = utilisateur?.Idinh ?? "";
                var annee = DateTime.Now.Year;
                var factureNum = $"{timestamp}/{idinh}/INH/{annee}";

                // === EN-TETE ===
                float[] headerWidths = { CmToPt(9), CmToPt(2), CmToPt(6) };
                Table headerTable = new Table(UnitValue.CreatePointArray(headerWidths));
                headerTable.SetWidth(UnitValue.CreatePercentValue(100));

                // Colonne 1 : Infos Ministère
                Cell col1 = new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetTextAlignment(TextAlignment.CENTER);
                string[] col1Texts = {
                    "MINISTERE DE LA SANTE, DE L'HYGIENE PUBLIQUE ET DE L'ACCES UNIVERSEL AUX SOINS",
                    "------",
                    "CABINET",
                    "------",
                    "SECRETARIAT GENERAL",
                    "------",
                    "INSTITUT NATIONAL",
                    "BP : 1396 Tél : 22 21 06 33 LOME-TOGO"
                };
                foreach (var txt in col1Texts)
                {
                    col1.Add(new Paragraph(txt).SetFont(fontNormal).SetFontSize(10).SetTextAlignment(TextAlignment.CENTER));
                }

                col1.Add(new Paragraph("\n").SetHeight(CmToPt(0.25f))); // espace 0.25 cm
                col1.Add(new Paragraph($"Période du {dateDebut} au {dateFin}")
                    .SetFont(fontBold)
                    .SetFontSize(11)
                    .SetTextAlignment(TextAlignment.CENTER));

                headerTable.AddCell(col1);

                // Colonne 2 : Vide
                headerTable.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                // Colonne 3 : Infos République
                Cell col3 = new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetTextAlignment(TextAlignment.CENTER);
                col3.Add(new Paragraph("REPUBLIQUE TOGOLAISE").SetFont(fontNormal).SetFontSize(10));
                col3.Add(new Paragraph("Travail - Liberté - Patrie").SetFont(fontNormal).SetFontSize(10));
                col3.Add(new Paragraph("\n").SetHeight(CmToPt(0.5f))); // espace 0.25 cm
                col3.Add(new Paragraph($"\tLomé le, {dateImpression}").SetFont(fontNormal).SetFontSize(10));
                col3.Add(new Paragraph("\n").SetHeight(CmToPt(0.5f))); // espace 0.25 cm
                col3.Add(new Paragraph($"\tDoit :").SetFont(fontBold).SetFontSize(10));
                col3.Add(new Paragraph($"\t{doit}").SetFont(fontNormal).SetFontSize(10));
                col3.Add(new Paragraph("\n").SetHeight(CmToPt(0.25f))); // espace 0.25 cm
                col3.Add(new Paragraph($"\tFacture N° :").SetFont(fontBold).SetFontSize(10));
                col3.Add(new Paragraph($"\t{factureNum}").SetFont(fontNormal).SetFontSize(10));

                headerTable.AddCell(col3);

                document.Add(headerTable);

                // === ESPACE ===
                document.Add(new Paragraph("\n").SetHeight(CmToPt(0.25f)));

                // === TABLEAU DES DONNEES ===
                float[] columnWidths = { CmToPt(3.25f), CmToPt(5.25f), CmToPt(2), CmToPt(2.75f), CmToPt(3.25f), CmToPt(2.5f) };
                Table table = new Table(UnitValue.CreatePointArray(columnWidths)).UseAllAvailableWidth();

                // En-tête
                string[] headers = { "Date de référence", "Désignation", "Nbre", "Prix unitaire", "Montant total", "Remise" };
                foreach (var header in headers)
                {
                    table.AddHeaderCell(CreateCell(header, fontBold, 11, TextAlignment.CENTER).SetBorder(new SolidBorder(1)));
                }

                // Exemple de ligne de données (à remplacer par boucle sur les vraies données)
                foreach (var item in toutesLesCategoriesEtAnalyses)
                {
                    table.AddCell(CreateCell(item.Date, fontNormal, 11, TextAlignment.CENTER).SetBorder(new SolidBorder(1)));
                    table.AddCell(CreateCell(item.Nom, fontNormal, 11, TextAlignment.CENTER).SetBorder(new SolidBorder(1)));
                    table.AddCell(CreateCell(item.Nbre.ToString(), fontNormal, 11, TextAlignment.CENTER).SetBorder(new SolidBorder(1)));
                    table.AddCell(CreateCell(item.Prix.ToString("N0", cultureFr), fontNormal, 11, TextAlignment.CENTER).SetBorder(new SolidBorder(1)));
                    table.AddCell(CreateCell(item.Montant.ToString("N0", cultureFr), fontNormal, 11, TextAlignment.CENTER).SetBorder(new SolidBorder(1)));
                    table.AddCell(CreateCell(item.Remise.ToString("N0", cultureFr), fontNormal, 11, TextAlignment.CENTER).SetBorder(new SolidBorder(1)));
                }

                // === FOOTER DU TABLEAU PRINCIPAL (TOTAL) ===
                Cell totalLabelCell = new Cell(1, 4)
                    .Add(new Paragraph("TOTAL").SetFont(fontBold).SetFontSize(11).SetTextAlignment(TextAlignment.LEFT))
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetBorder(new SolidBorder(1));
                table.AddCell(totalLabelCell);

                // Exemple de valeurs fictives, à remplacer dynamiquement
                table.AddCell(new Cell().Add(new Paragraph(montantTotal.ToString("N0", cultureFr)).SetFont(fontBold).SetFontSize(11).SetTextAlignment(TextAlignment.CENTER))
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetBorder(new SolidBorder(1)));
                table.AddCell(new Cell().Add(new Paragraph(remiseTotal.ToString("N0", cultureFr)).SetFont(fontBold).SetFontSize(11).SetTextAlignment(TextAlignment.CENTER))
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetBorder(new SolidBorder(1)));

                document.Add(table);

                document.Add(new Paragraph("\n").SetHeight(CmToPt(0.25f)));

                float[] arreteWidths = { CmToPt(7), CmToPt(12) };
                Table arreteTable = new Table(UnitValue.CreatePointArray(arreteWidths)).UseAllAvailableWidth();

                arreteTable.AddCell(new Cell().Add(new Paragraph("Arrêté la présente facture à la somme de :")
                    .SetFont(fontNormal).SetFontSize(11).SetTextAlignment(TextAlignment.LEFT))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                arreteTable.AddCell(new Cell().Add(new Paragraph(sommeTotal)
                    .SetFont(fontBold).SetFontSize(11).SetTextAlignment(TextAlignment.LEFT))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                document.Add(arreteTable);

                float[] ristourneWidths = { CmToPt(4), CmToPt(15) };
                Table ristourneTable = new Table(UnitValue.CreatePointArray(ristourneWidths)).UseAllAvailableWidth();

                ristourneTable.AddCell(new Cell().Add(new Paragraph("N.B Ristourne à payer")
                    .SetFont(fontNormal).SetFontSize(11).SetTextAlignment(TextAlignment.LEFT))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                ristourneTable.AddCell(new Cell().Add(new Paragraph($"{remiseTotal.ToString("N0", cultureFr)} : {sommeRemise}")
                    .SetFont(fontBold).SetFontSize(11).SetTextAlignment(TextAlignment.LEFT))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                document.Add(ristourneTable);

                float[] netWidths = { CmToPt(2.5f), CmToPt(16.5f) };
                Table netTable = new Table(UnitValue.CreatePointArray(netWidths)).UseAllAvailableWidth();

                netTable.AddCell(new Cell().Add(new Paragraph("Net à payer :")
                    .SetFont(fontBold).SetFontSize(11).SetTextAlignment(TextAlignment.LEFT))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                netTable.AddCell(new Cell().Add(new Paragraph(montantNet.ToString("N0", cultureFr))
                    .SetFont(fontNormal).SetFontSize(11).SetTextAlignment(TextAlignment.LEFT))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                document.Add(netTable);

                Table lettreTable = new Table(1).UseAllAvailableWidth();
                lettreTable.AddCell(new Cell().Add(new Paragraph(sommeNet)
                    .SetFont(fontNormal).SetFontSize(11).SetTextAlignment(TextAlignment.LEFT))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                document.Add(lettreTable);

                float[] signatureWidths = { CmToPt(11.25f), CmToPt(7.75f) };
                Table signatureTable = new Table(UnitValue.CreatePointArray(signatureWidths)).UseAllAvailableWidth();

                signatureTable.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                signatureTable.AddCell(new Cell().Add(new Paragraph("Le Responsable de la collecte de l'échantillon")
                    .SetFont(fontNormal).SetFontSize(11).SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                document.Add(signatureTable);

                float[] responsableWidths = { CmToPt(3.5f), CmToPt(15.5f) };
                Table responsableTable = new Table(UnitValue.CreatePointArray(responsableWidths)).UseAllAvailableWidth();

                responsableTable.AddCell(new Cell().Add(new Paragraph("Le Responsable")
                    .SetFont(fontNormal).SetFontSize(11).SetTextAlignment(TextAlignment.LEFT))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                responsableTable.AddCell(new Cell().Add(new Paragraph("")
                    .SetFont(fontNormal).SetFontSize(11).SetTextAlignment(TextAlignment.LEFT))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                document.Add(responsableTable);

                document.Add(new Paragraph("\n").SetHeight(CmToPt(0.25f)));

                Table validationTable = new Table(UnitValue.CreatePointArray(signatureWidths)).UseAllAvailableWidth();
                validationTable.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                validationTable.AddCell(new Cell().Add(new Paragraph("Validation du règlement de la facture")
                    .SetFont(fontNormal).SetFontSize(11).SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                document.Add(validationTable);

                float[] banqueWidths = { CmToPt(6), CmToPt(5.25f), CmToPt(7.75f) };
                Table banqueTable = new Table(UnitValue.CreatePointArray(banqueWidths)).UseAllAvailableWidth();

                Table leftBankTable = new Table(1).UseAllAvailableWidth();
                leftBankTable.AddCell(new Cell().Add(new Paragraph("AGENCE CPTABLE INST. NA. H")
                    .SetFont(fontNormal).SetFontSize(11).SetTextAlignment(TextAlignment.LEFT))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                leftBankTable.AddCell(new Cell().Add(new Paragraph("CODE BANQUE : TG116")
                    .SetFont(fontNormal).SetFontSize(11).SetTextAlignment(TextAlignment.LEFT))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                leftBankTable.AddCell(new Cell().Add(new Paragraph("COMPTE N° : 040210024408")
                    .SetFont(fontNormal).SetFontSize(11).SetTextAlignment(TextAlignment.LEFT))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                banqueTable.AddCell(new Cell().Add(leftBankTable).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                banqueTable.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                Table rightBankTable = new Table(1).UseAllAvailableWidth();
                rightBankTable.AddCell(new Cell().Add(new Paragraph("CODE GUICHET : 01101")
                    .SetFont(fontNormal).SetFontSize(11).SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                rightBankTable.AddCell(new Cell().Add(new Paragraph("Clé RIB : 24")
                    .SetFont(fontNormal).SetFontSize(11).SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                rightBankTable.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER)); // ligne vide

                banqueTable.AddCell(new Cell().Add(rightBankTable).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                document.Add(banqueTable);

                document.Add(new Paragraph("\n").SetHeight(CmToPt(0.25f)));

                Table directeur1 = new Table(UnitValue.CreatePointArray(signatureWidths)).UseAllAvailableWidth();
                directeur1.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                directeur1.AddCell(new Cell().Add(new Paragraph("Le Directeur de l'INH")
                    .SetFont(fontNormal).SetFontSize(11).SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                document.Add(directeur1);

                // Espacement
                document.Add(new Paragraph("\n").SetHeight(CmToPt(0.75f)));

                float[] imprimesWidths = { CmToPt(8), CmToPt(3.25f), CmToPt(7.75f) };
                Table imprimeTable = new Table(UnitValue.CreatePointArray(imprimesWidths)).UseAllAvailableWidth();

                imprimeTable.AddCell(new Cell().Add(new Paragraph($"Imprimé le {dateImpression}")
                    .SetFont(fontNormal).SetFontSize(11).SetTextAlignment(TextAlignment.LEFT))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                imprimeTable.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                imprimeTable.AddCell(new Cell().Add(new Paragraph("Dr. HALATOKO Wemboo Afiwa")
                    .SetFont(fontBold).SetUnderline().SetFontSize(11).SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                document.Add(imprimeTable);

                document.Close();

                byte[] pdfBytes = stream.ToArray();

                // Exemple de réouverture et modification
                using (var input = new MemoryStream(pdfBytes))
                using (var output = new MemoryStream())
                {
                    var reader = new PdfReader(input);
                    var writer1 = new PdfWriter(output);
                    var pdfDoc = new PdfDocument(reader, writer1);

                    // Ajout des numéros de page (code selon besoin)
                    float height = CmToPt(0.6f); // Hauteur du footer
                    float width = CmToPt(2f);    // Largeur réservée pour le numéro de page
                    var fontFooter = PdfFontFactory.CreateFont(GetFontPath(true, false), PdfEncodings.IDENTITY_H);

                    // Nombre total de pages
                    int numberOfPages = pdfDoc.GetNumberOfPages();

                    //Parcours des pages
                    for (int i = 1; i <= numberOfPages; i++) 
                    {
                        PdfPage page = pdfDoc.GetPage(i);

                        float x = PageSize.A4.GetWidth() - marginRight - width / 2; // x depuis la droite
                        float y = marginBottom / 2 - height; // centré verticalement dans la marge

                        // Format : 1/3, 2/3, etc.
                        var pageNumberText = $"{i}/{numberOfPages}";

                        PdfCanvas pdfCanvas = new PdfCanvas(page);

                        //// Bordure noire
                        //pdfCanvas.SetLineWidth(1)
                        //  .SetStrokeColor(ColorConstants.BLACK)
                        //  .Rectangle(x, y, width, height)
                        //  .Stroke();

                        // Commence à écrire du texte
                        pdfCanvas.BeginText()
                            .SetFontAndSize(fontFooter, 11)
                            .SetFillColor(ColorConstants.BLACK)
                            .MoveText(x, y)
                            .ShowText(pageNumberText)
                            .EndText()
                            .Release();
                    }

                    pdfDoc.Close();

                    string base64 = Convert.ToBase64String(output.ToArray());

                    return Json(new
                    {
                        success = true,
                        pdfBase64 = base64
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "erreur lors de la génération de rapport");
                throw;
            }
        }

        //[HttpGet]
        //public IActionResult GenererFactureAssurance(string debut, string fin, string codeassurance)
        //{
        //    if (string.IsNullOrWhiteSpace(debut) || !DateTime.TryParse(debut, out var parsedDebut) ||
        //        string.IsNullOrWhiteSpace(fin) || !DateTime.TryParse(fin, out var parsedFin))
        //    {
        //        return BadRequest("Les paramètres de dates sont invalides.");
        //    }

        //    if (parsedDebut > parsedFin)
        //    {
        //        return BadRequest("La date de début doit être antérieure à la date de fin.");
        //    }

        //    if (string.IsNullOrWhiteSpace(codeassurance))
        //    {
        //        return BadRequest("Le code assurance est obligatoire.");
        //    }

        //    // Récupérer l'assurance
        //    var assurance = _context.Assurances.FirstOrDefault(a => a.Codeassurance == codeassurance);
        //    if (assurance == null)
        //    {
        //        return BadRequest("Assurance introuvable.");
        //    }

        //    // Récupérer les détails de demandes avec les objets dépendants
        //    var detaildemandes = _context.Detaildemandes
        //        .Include(dd => dd.Entetedemande)
        //            .ThenInclude(ed => ed.Policeassurance)
        //        .Include(dd => dd.Detailfactures)
        //            .ThenInclude(df => df.Entetefacture)
        //        .Where(dd => dd.Entetedemande.Policeassurance.Codeassurance == codeassurance &&
        //                    dd.Entetedemande.Date >= parsedDebut.Date &&
        //                    dd.Entetedemande.Date <= parsedFin.Date)
        //        .ToList();

        //    // Calculer les montants
        //    var montantTotal = detaildemandes.Sum(dd => dd.Prix);
        //    var remiseTotal = montantTotal * 0.20m; // 20% du montant total
        //    var montantNet = montantTotal - remiseTotal;

        //    var reportPath = System.IO.Path.Combine(_webHostEnvironment.WebRootPath, "reports", "factureassurance.frx");
        //    if (!System.IO.File.Exists(reportPath))
        //        return NotFound("Le rapport n'existe pas.");

        //    string login = User.Identity?.Name ?? string.Empty;
        //    string operateur = login;
        //    var utilisateur = _context.Utilisateurs.FirstOrDefault(u => u.Login == login);
        //    if (utilisateur != null)
        //    {
        //        operateur = $"{utilisateur.Nom} {utilisateur.Prenom}".Trim();
        //    }

        //    // Générer le numéro de facture
        //    var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        //    var idinh = utilisateur?.Idinh ?? "";
        //    var annee = DateTime.Now.Year;
        //    var numero = $"{timestamp}/{idinh}/INH/{annee}";

        //    using (var report = new FastReport.Report())
        //    {
        //        report.Load(reportPath);
        //        report.SetParameterValue("datedebut", parsedDebut.Date);
        //        report.SetParameterValue("datefin", parsedFin.Date);
        //        report.SetParameterValue("datedebutstr", parsedDebut.ToString("dd/MM/yyyy"));
        //        report.SetParameterValue("datefinstr", parsedFin.ToString("dd/MM/yyyy"));
        //        report.SetParameterValue("codeassurance", codeassurance);
        //        report.SetParameterValue("operateur", operateur);
        //        report.SetParameterValue("numero", numero);
        //        report.SetParameterValue("montanttotal", montantTotal.ToString("F2"));
        //        report.SetParameterValue("remisetotal", remiseTotal.ToString("F2"));
        //        report.SetParameterValue("montantnet", montantNet.ToString("F2"));
        //        report.SetParameterValue("sommetotal", ((int)montantTotal).ToWords(new CultureInfo("fr")).ToUpper());
        //        report.SetParameterValue("sommeremise", ((int)remiseTotal).ToWords(new CultureInfo("fr")).ToUpper());
        //        report.SetParameterValue("sommenet", ((int)montantNet).ToWords(new CultureInfo("fr")).ToUpper());
        //        report.SetParameterValue("assurance", assurance.Nom);
        //        report.SetParameterValue("dateedition", DateTime.Now.ToString("dd MMMM yyyy", new CultureInfo("fr-FR")));
        //        report.Prepare();
        //        using (var ms = new MemoryStream())
        //        {
        //            var imageExport = new FastReport.Export.Image.ImageExport
        //            {
        //                ImageFormat = FastReport.Export.Image.ImageExportFormat.Png,
        //                SeparateFiles = false,
        //                ResolutionX = 300,
        //                ResolutionY = 300
        //            };
        //            report.Export(imageExport, ms);
        //            ms.Position = 0;
        //            var base64 = Convert.ToBase64String(ms.ToArray());
        //            return Json(new { success = true, imageBase64 = "data:image/png;base64," + base64 });
        //        }
        //    }
        //}

        [HttpGet]
        public IActionResult FacturePartenaire()
        {
            var partenaires = new List<SelectListItem> { new SelectListItem { Value = "", Text = "---" } };
            partenaires.AddRange(_context.Partenaires.Select(p => new SelectListItem { 
                Value = p.Partenaireid.ToString(), 
                Text = p.Nom 
            }));
            ViewBag.Partenaireid = partenaires;
            return View(new FacturePartenaireVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult FacturePartenaire(FacturePartenaireVM vm)
        {
            var partenaires = new List<SelectListItem> { new SelectListItem { Value = "", Text = "---" } };
            partenaires.AddRange(_context.Partenaires.Select(p => new SelectListItem { 
                Value = p.Partenaireid.ToString(), 
                Text = p.Nom,
                Selected = p.Partenaireid == vm.Partenaireid
            }));
            ViewBag.Partenaireid = partenaires;

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            return View(vm);
        }

        [HttpGet]
        public IActionResult GenererFacturePartenaire(string debut, string fin, Guid partenaireid)
        {
            var cultureFr = new CultureInfo("fr-FR");
            
            if (string.IsNullOrWhiteSpace(debut) || !DateTime.TryParse(debut, cultureFr, DateTimeStyles.None, out var parsedDebut) ||
                string.IsNullOrWhiteSpace(fin) || !DateTime.TryParse(fin, cultureFr, DateTimeStyles.None, out var parsedFin))
            {
                return BadRequest("Les paramètres de dates sont invalides.");
            }

            if (parsedDebut > parsedFin)
            {
                return BadRequest("La date de début doit être antérieure à la date de fin.");
            }
            
            // Validation pour éviter des dates futures
            var currentYear = DateTime.Now.Year;
            if (parsedDebut.Year > currentYear || parsedFin.Year > currentYear)
            {
                return BadRequest("Les dates ne peuvent pas être supérieures à l'année actuelle (" + currentYear.ToString() + ").");
            }

            // Récupérer le partenaire
            var partenaire = _context.Partenaires.FirstOrDefault(p => p.Partenaireid == partenaireid);
            if (partenaire == null)
            {
                return BadRequest("Partenaire introuvable.");
            }

            // Récupérer les détails de demandes avec les objets dépendants
            var detaildemandes = _context.Detaildemandes
                .Include(dd => dd.Entetedemande)
                    .ThenInclude(ed => ed.Partenaire)
                .Include(dd => dd.Detailfactures)
                    .ThenInclude(df => df.Entetefacture)
                .Where(dd => dd.Entetedemande.Partenaireid == partenaireid &&
                            dd.Entetedemande.Date.Date >= parsedDebut.Date &&
                            dd.Entetedemande.Date.Date <= parsedFin.Date)
                .ToList();

            if (detaildemandes == null || !detaildemandes.Any())
            {
                return BadRequest("Aucune donnée trouvée pour cette période.");
            }

            // Calculer les totaux
            decimal montantTotal = detaildemandes.Sum(dd => dd.Prix);
            decimal remiseTotal = montantTotal * 0.20m; // 20% du montant total
            decimal montantNet = montantTotal - remiseTotal;

            var sommeTotal = NumberToWordsConverter.Convert(montantTotal).ToUpper();
            var sommeRemise = NumberToWordsConverter.Convert(remiseTotal).ToUpper();
            var sommeNet = NumberToWordsConverter.Convert(montantNet).ToUpper();

            var query1 = (from p in _context.Entetefactures
                          join p1 in _context.Entetedemandes on p.Entetedemandeid equals p1.Entetedemandeid
                          join p2 in _context.Detaildemandes on p1.Entetedemandeid equals p2.Entetedemandeid
                          join p3 in _context.Categories on p2.Categorieid equals p3.Categorieid
                          join p4 in _context.Partenaires on p1.Partenaireid equals p4.Partenaireid
                          where p.Date.Date >= parsedDebut.Date && p.Date.Date <= parsedFin.Date
                                && p4.Partenaireid == partenaire.Partenaireid
                          group new { p, p2, p3, p4 } by new { 
                              Type = p3.Nom,
                              Prix = p2.Prix
                          } into g
                          select new
                          {
                              Type = g.Key.Type,
                              Nbre = g.Count(),
                              Prix = g.Key.Prix,
                              Montant = g.Count() * g.Key.Prix,
                              PartAssurance = g.Sum(x => x.p2.Prix) * 0.8m,
                              PartPatient = g.Sum(x => x.p2.Prix) * 0.2m,
                              Complement = 0m,
                              Net = g.Sum(x => x.p2.Prix) * 0.8m,
                              CodePartenaire = g.Key
                          });

            var query2 = (from p in _context.Entetefactures
                          join p1 in _context.Entetedemandes on p.Entetedemandeid equals p1.Entetedemandeid
                          join p2 in _context.Detaildemandes on p1.Entetedemandeid equals p2.Entetedemandeid
                          join p3 in _context.Analyses on p2.Idanalyse equals p3.Idanalyse
                          join p4 in _context.Partenaires on p1.Partenaireid equals p4.Partenaireid
                          where p.Date.Date >= parsedDebut.Date && p.Date.Date <= parsedFin.Date
                                && p4.Partenaireid == partenaire.Partenaireid
                          group new { p, p2, p3, p4 } by new { 
                              Type = p3.Nom,
                              Prix = p2.Prix
                          } into g
                          select new
                          {
                              Type = g.Key.Type,
                              Nbre = g.Count(),
                              Prix = g.Key.Prix,
                              Montant = g.Count() * g.Key.Prix,
                              PartAssurance = g.Sum(x => x.p2.Prix) * 0.8m,
                              PartPatient = g.Sum(x => x.p2.Prix) * 0.2m,
                              Complement = 0m,
                              Net = g.Sum(x => x.p2.Prix) * 0.8m,
                              CodePartenaire = g.Key
                          });

            var toutesLesCategoriesEtAnalyses = query1.Concat(query2)
                    .GroupBy(x => x.Type)
                    .Select(g => new
                    {
                        Type = g.Key,
                        Nbre = g.Sum(x => x.Nbre),
                        Prix = g.First().Prix,
                        Montant = g.Sum(x => x.Montant),
                        PartAssurance = g.Sum(x => x.PartAssurance),
                        PartPatient = g.Sum(x => x.PartPatient),
                        Complement = g.Sum(x => x.Complement),
                        Net = g.Sum(x => x.Net)
                    })
                    .ToList();

            try
            {
                using var stream = new MemoryStream();
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf, PageSize.A4);
                document.SetMargins(36, 36, 36, 36);

                float pageHeight = PageSize.A4.GetHeight();

                float marginTop = document.GetTopMargin();
                float marginBottom = document.GetBottomMargin();
                float marginLeft = document.GetLeftMargin();
                float marginRight = document.GetRightMargin();

                Console.WriteLine($"Top: {marginTop}, Bottom: {marginBottom}, Left: {marginLeft}, Right: {marginRight}");

                // S'assurer que les encodages étendus sont disponibles
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                PdfFont fontNormal = PdfFontFactory.CreateFont(GetFontPath(true, false), PdfEncodings.IDENTITY_H);
                PdfFont fontBold = PdfFontFactory.CreateFont(GetFontPath(false, true), PdfEncodings.IDENTITY_H);

                Console.WriteLine($"Normal: {GetFontPath(true, false)}, Bold: {GetFontPath(false, true)}");

                var dateDebut = parsedDebut.ToShortDateString();
                var dateFin = parsedFin.ToShortDateString();
                var date = DateTime.Now;
                var dateImpression = $"{date.ToShortDateString()} {date.ToShortTimeString()}";
                var doit = partenaire.Nom;

                string login = User.Identity?.Name ?? string.Empty;
                string operateur = login;
                var utilisateur = _context.Utilisateurs.FirstOrDefault(u => u.Login == login);
                if (utilisateur != null)
                {
                    operateur = $"{utilisateur.Nom} {utilisateur.Prenom}".Trim();
                }

                // Générer le numéro de facture
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var idinh = utilisateur?.Idinh ?? "";
                var annee = DateTime.Now.Year;
                var factureNum = $"{timestamp}/{idinh}/INH/{annee}";

                // === EN-TETE ===
                float[] headerWidths = { CmToPt(9), CmToPt(2), CmToPt(6) };
                Table headerTable = new Table(UnitValue.CreatePointArray(headerWidths));
                headerTable.SetWidth(UnitValue.CreatePercentValue(100));

                // Colonne 1 : Infos Ministère
                Cell col1 = new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetTextAlignment(TextAlignment.CENTER);
                string[] col1Texts = {
                    "MINISTERE DE LA SANTE, DE L'HYGIENE PUBLIQUE ET DE L'ACCES UNIVERSEL AUX SOINS",
                    "------",
                    "CABINET",
                    "------",
                    "SECRETARIAT GENERAL",
                    "------",
                    "INSTITUT NATIONAL",
                    "BP : 1396 Tél : 22 21 06 33 LOME-TOGO"
                };
                foreach (var txt in col1Texts)
                {
                    col1.Add(new Paragraph(txt).SetFont(fontNormal).SetFontSize(10).SetTextAlignment(TextAlignment.CENTER));
                }

                col1.Add(new Paragraph("\n").SetHeight(CmToPt(0.25f))); // espace 0.25 cm
                col1.Add(new Paragraph($"Période du {dateDebut} au {dateFin}")
                    .SetFont(fontBold)
                    .SetFontSize(11)
                    .SetTextAlignment(TextAlignment.CENTER));

                headerTable.AddCell(col1);

                // Colonne 2 : Vide
                headerTable.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                // Colonne 3 : Infos République
                Cell col3 = new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetTextAlignment(TextAlignment.CENTER);
                col3.Add(new Paragraph("REPUBLIQUE TOGOLAISE").SetFont(fontNormal).SetFontSize(10));
                col3.Add(new Paragraph("Travail - Liberté - Patrie").SetFont(fontNormal).SetFontSize(10));
                col3.Add(new Paragraph("\n").SetHeight(CmToPt(0.5f))); // espace 0.25 cm
                col3.Add(new Paragraph($"\tLomé le, {dateImpression}").SetFont(fontNormal).SetFontSize(10));
                col3.Add(new Paragraph("\n").SetHeight(CmToPt(0.5f))); // espace 0.25 cm
                col3.Add(new Paragraph($"\tDoit :").SetFont(fontBold).SetFontSize(10));
                col3.Add(new Paragraph($"\t{doit}").SetFont(fontNormal).SetFontSize(10));
                col3.Add(new Paragraph("\n").SetHeight(CmToPt(0.25f))); // espace 0.25 cm
                col3.Add(new Paragraph($"\tFacture N° :").SetFont(fontBold).SetFontSize(10));
                col3.Add(new Paragraph($"\t{factureNum}").SetFont(fontNormal).SetFontSize(10));

                headerTable.AddCell(col3);

                document.Add(headerTable);

                // === ESPACE ===
                document.Add(new Paragraph("\n").SetHeight(CmToPt(0.25f)));

                // === TABLEAU DES DONNEES ===
                float[] columnWidths = { CmToPt(3.25f), CmToPt(5.25f), CmToPt(2), CmToPt(2.75f), CmToPt(3.25f), CmToPt(2.5f) };
                Table table = new Table(UnitValue.CreatePointArray(columnWidths)).UseAllAvailableWidth();

                // En-tête
                string[] headers = { "Date de référence", "Désignation", "Nbre", "Prix unitaire", "Montant total", "Remise" };
                foreach (var header in headers)
                {
                    table.AddHeaderCell(CreateCell(header, fontBold, 11, TextAlignment.CENTER).SetBorder(new SolidBorder(1)));
                }

                // Lignes de données (adaptées aux données partenaire)
                foreach (var item in toutesLesCategoriesEtAnalyses)
                {
                    table.AddCell(CreateCell(dateDebut, fontNormal, 11, TextAlignment.CENTER).SetBorder(new SolidBorder(1)));
                    table.AddCell(CreateCell(item.Type, fontNormal, 11, TextAlignment.CENTER).SetBorder(new SolidBorder(1)));
                    table.AddCell(CreateCell(item.Nbre.ToString(), fontNormal, 11, TextAlignment.CENTER).SetBorder(new SolidBorder(1)));
                    table.AddCell(CreateCell(item.Prix.ToString("N0", cultureFr), fontNormal, 11, TextAlignment.CENTER).SetBorder(new SolidBorder(1)));
                    table.AddCell(CreateCell(item.Montant.ToString("N0", cultureFr), fontNormal, 11, TextAlignment.CENTER).SetBorder(new SolidBorder(1)));
                    table.AddCell(CreateCell(item.PartPatient.ToString("N0", cultureFr), fontNormal, 11, TextAlignment.CENTER).SetBorder(new SolidBorder(1)));
                }

                // === FOOTER DU TABLEAU PRINCIPAL (TOTAL) ===
                Cell totalLabelCell = new Cell(1, 4)
                    .Add(new Paragraph("TOTAL").SetFont(fontBold).SetFontSize(11).SetTextAlignment(TextAlignment.LEFT))
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetBorder(new SolidBorder(1));
                table.AddCell(totalLabelCell);

                table.AddCell(new Cell().Add(new Paragraph(montantTotal.ToString("N0", cultureFr)).SetFont(fontBold).SetFontSize(11).SetTextAlignment(TextAlignment.CENTER))
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetBorder(new SolidBorder(1)));
                table.AddCell(new Cell().Add(new Paragraph(remiseTotal.ToString("N0", cultureFr)).SetFont(fontBold).SetFontSize(11).SetTextAlignment(TextAlignment.CENTER))
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetBorder(new SolidBorder(1)));

                document.Add(table);

                document.Add(new Paragraph("\n").SetHeight(CmToPt(0.25f)));

                float[] arreteWidths = { CmToPt(7), CmToPt(12) };
                Table arreteTable = new Table(UnitValue.CreatePointArray(arreteWidths)).UseAllAvailableWidth();

                arreteTable.AddCell(new Cell().Add(new Paragraph("Arrêté la présente facture à la somme de :")
                    .SetFont(fontNormal).SetFontSize(11).SetTextAlignment(TextAlignment.LEFT))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                arreteTable.AddCell(new Cell().Add(new Paragraph(sommeTotal)
                    .SetFont(fontBold).SetFontSize(11).SetTextAlignment(TextAlignment.LEFT))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                document.Add(arreteTable);

                float[] ristourneWidths = { CmToPt(4), CmToPt(15) };
                Table ristourneTable = new Table(UnitValue.CreatePointArray(ristourneWidths)).UseAllAvailableWidth();

                ristourneTable.AddCell(new Cell().Add(new Paragraph("N.B Ristourne à payer")
                    .SetFont(fontNormal).SetFontSize(11).SetTextAlignment(TextAlignment.LEFT))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                ristourneTable.AddCell(new Cell().Add(new Paragraph($"{remiseTotal.ToString("N0", cultureFr)} : {sommeRemise}")
                    .SetFont(fontBold).SetFontSize(11).SetTextAlignment(TextAlignment.LEFT))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                document.Add(ristourneTable);

                float[] netWidths = { CmToPt(2.5f), CmToPt(16.5f) };
                Table netTable = new Table(UnitValue.CreatePointArray(netWidths)).UseAllAvailableWidth();

                netTable.AddCell(new Cell().Add(new Paragraph("Net à payer :")
                    .SetFont(fontBold).SetFontSize(11).SetTextAlignment(TextAlignment.LEFT))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                netTable.AddCell(new Cell().Add(new Paragraph(montantNet.ToString("N0", cultureFr))
                    .SetFont(fontNormal).SetFontSize(11).SetTextAlignment(TextAlignment.LEFT))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                document.Add(netTable);

                Table lettreTable = new Table(1).UseAllAvailableWidth();
                lettreTable.AddCell(new Cell().Add(new Paragraph(sommeNet)
                    .SetFont(fontNormal).SetFontSize(11).SetTextAlignment(TextAlignment.LEFT))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                document.Add(lettreTable);

                float[] signatureWidths = { CmToPt(11.25f), CmToPt(7.75f) };
                Table signatureTable = new Table(UnitValue.CreatePointArray(signatureWidths)).UseAllAvailableWidth();

                signatureTable.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                signatureTable.AddCell(new Cell().Add(new Paragraph("Le Responsable de la collecte de l'échantillon")
                    .SetFont(fontNormal).SetFontSize(11).SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                document.Add(signatureTable);

                float[] responsableWidths = { CmToPt(3.5f), CmToPt(15.5f) };
                Table responsableTable = new Table(UnitValue.CreatePointArray(responsableWidths)).UseAllAvailableWidth();

                responsableTable.AddCell(new Cell().Add(new Paragraph("Le Responsable")
                    .SetFont(fontNormal).SetFontSize(11).SetTextAlignment(TextAlignment.LEFT))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                responsableTable.AddCell(new Cell().Add(new Paragraph("")
                    .SetFont(fontNormal).SetFontSize(11).SetTextAlignment(TextAlignment.LEFT))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                document.Add(responsableTable);

                document.Add(new Paragraph("\n").SetHeight(CmToPt(0.25f)));

                Table validationTable = new Table(UnitValue.CreatePointArray(signatureWidths)).UseAllAvailableWidth();
                validationTable.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                validationTable.AddCell(new Cell().Add(new Paragraph("Validation du règlement de la facture")
                    .SetFont(fontNormal).SetFontSize(11).SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                document.Add(validationTable);

                float[] banqueWidths = { CmToPt(6), CmToPt(5.25f), CmToPt(7.75f) };
                Table banqueTable = new Table(UnitValue.CreatePointArray(banqueWidths)).UseAllAvailableWidth();

                Table leftBankTable = new Table(1).UseAllAvailableWidth();
                leftBankTable.AddCell(new Cell().Add(new Paragraph("AGENCE CPTABLE INST. NA. H")
                    .SetFont(fontNormal).SetFontSize(11).SetTextAlignment(TextAlignment.LEFT))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                leftBankTable.AddCell(new Cell().Add(new Paragraph("CODE BANQUE : TG116")
                    .SetFont(fontNormal).SetFontSize(11).SetTextAlignment(TextAlignment.LEFT))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                leftBankTable.AddCell(new Cell().Add(new Paragraph("COMPTE N° : 040210024408")
                    .SetFont(fontNormal).SetFontSize(11).SetTextAlignment(TextAlignment.LEFT))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                banqueTable.AddCell(new Cell().Add(leftBankTable).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                banqueTable.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                Table rightBankTable = new Table(1).UseAllAvailableWidth();
                rightBankTable.AddCell(new Cell().Add(new Paragraph("CODE GUICHET : 01101")
                    .SetFont(fontNormal).SetFontSize(11).SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                rightBankTable.AddCell(new Cell().Add(new Paragraph("Clé RIB : 24")
                    .SetFont(fontNormal).SetFontSize(11).SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                rightBankTable.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER)); // ligne vide

                banqueTable.AddCell(new Cell().Add(rightBankTable).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                document.Add(banqueTable);

                document.Add(new Paragraph("\n").SetHeight(CmToPt(0.25f)));

                Table directeur1 = new Table(UnitValue.CreatePointArray(signatureWidths)).UseAllAvailableWidth();
                directeur1.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                directeur1.AddCell(new Cell().Add(new Paragraph("Le Directeur de l'INH")
                    .SetFont(fontNormal).SetFontSize(11).SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                document.Add(directeur1);

                // Espacement
                document.Add(new Paragraph("\n").SetHeight(CmToPt(0.75f)));

                float[] imprimesWidths = { CmToPt(8), CmToPt(3.25f), CmToPt(7.75f) };
                Table imprimeTable = new Table(UnitValue.CreatePointArray(imprimesWidths)).UseAllAvailableWidth();

                imprimeTable.AddCell(new Cell().Add(new Paragraph($"Imprimé le {dateImpression}")
                    .SetFont(fontNormal).SetFontSize(11).SetTextAlignment(TextAlignment.LEFT))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                imprimeTable.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                imprimeTable.AddCell(new Cell().Add(new Paragraph("Dr. HALATOKO Wemboo Afiwa")
                    .SetFont(fontBold).SetUnderline().SetFontSize(11).SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                document.Add(imprimeTable);

                document.Close();

                byte[] pdfBytes = stream.ToArray();

                // Exemple de réouverture et modification
                using (var input = new MemoryStream(pdfBytes))
                using (var output = new MemoryStream())
                {
                    var reader = new PdfReader(input);
                    var writer1 = new PdfWriter(output);
                    var pdfDoc = new PdfDocument(reader, writer1);

                    // Ajout des numéros de page (code selon besoin)
                    float height = CmToPt(0.6f); // Hauteur du footer
                    float width = CmToPt(2f);    // Largeur réservée pour le numéro de page
                    var fontFooter = PdfFontFactory.CreateFont(GetFontPath(true, false), PdfEncodings.IDENTITY_H);

                    // Nombre total de pages
                    int numberOfPages = pdfDoc.GetNumberOfPages();

                    //Parcours des pages
                    for (int i = 1; i <= numberOfPages; i++) 
                    {
                        PdfPage page = pdfDoc.GetPage(i);

                        float x = PageSize.A4.GetWidth() - marginRight - width / 2; // x depuis la droite
                        float y = marginBottom / 2 - height; // centré verticalement dans la marge

                        // Format : 1/3, 2/3, etc.
                        var pageNumberText = $"{i}/{numberOfPages}";

                        PdfCanvas pdfCanvas = new PdfCanvas(page);

                        //// Bordure noire
                        //pdfCanvas.SetLineWidth(1)
                        //  .SetStrokeColor(ColorConstants.BLACK)
                        //  .Rectangle(x, y, width, height)
                        //  .Stroke();

                        // Commence à écrire du texte
                        pdfCanvas.BeginText()
                            .SetFontAndSize(fontFooter, 11)
                            .SetFillColor(ColorConstants.BLACK)
                            .MoveText(x, y)
                            .ShowText(pageNumberText)
                            .EndText()
                            .Release();
                    }

                    pdfDoc.Close();

                    string base64 = Convert.ToBase64String(output.ToArray());

                    return Json(new
                    {
                        success = true,
                        pdfBase64 = base64
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération de la facture partenaire");
                return StatusCode(500, "Une erreur s'est produite lors de la génération de la facture.");
            }
        }

        [HttpGet]
        public IActionResult TitreRecetteParAssurance()
        {
            var assurances = new List<SelectListItem> { new SelectListItem { Value = "", Text = "---" } };
            assurances.AddRange(_context.Assurances.Select(a => new SelectListItem
            {
                Value = a.Codeassurance,
                Text = a.Nom
            }));
            ViewBag.Codeassurance = assurances;
            return View(new TitreRecetteParAssuranceVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TitreRecetteParAssurance(TitreRecetteParAssuranceVM vm)
        {
            var assurances = new List<SelectListItem> { new SelectListItem { Value = "", Text = "---" } };
            assurances.AddRange(_context.Assurances.Select(a => new SelectListItem
            {
                Value = a.Codeassurance,
                Text = a.Nom,
                Selected = a.Codeassurance == vm.Codeassurance
            }));
            ViewBag.Codeassurance = assurances;

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            ViewBag.Message = $"Période saisie : du {vm.Debut} au {vm.Fin} pour l'assurance {vm.Codeassurance}";
            return View(vm);
        }

        [HttpGet]
        public IActionResult GenererTitreRecetteParAssurance(string debut, string fin, string codeassurance)
        {
            if (string.IsNullOrWhiteSpace(debut) || !DateTime.TryParse(debut, out var parsedDebut) ||
                string.IsNullOrWhiteSpace(fin) || !DateTime.TryParse(fin, out var parsedFin))
            {
                return BadRequest("Les paramètres de dates sont invalides.");
            }

            if (parsedDebut > parsedFin)
            {
                return BadRequest("La date de début doit être antérieure à la date de fin.");
            }

            if (string.IsNullOrWhiteSpace(codeassurance))
            {
                return BadRequest("Le code assurance est obligatoire.");
            }

            // Récupérer l'assurance
            var objAssurance = _context.Assurances.FirstOrDefault(a => a.Codeassurance == codeassurance);
            if (objAssurance == null)
            {
                return BadRequest("Assurance introuvable.");
            }

            try
            {
                // Récupérer les détails de demandes avec les objets dépendants
                var detaildemandes = _context.Detaildemandes
                    .Include(dd => dd.Entetedemande)
                        .ThenInclude(ed => ed.Policeassurance)
                        .ThenInclude(ed => ed.CodeassuranceNavigation)
                    .Include(dd => dd.Detailfactures)
                        .ThenInclude(df => df.Entetefacture)
                    .Where(dd => dd.Entetedemande.Policeassurance.Codeassurance == codeassurance &&
                                dd.Entetedemande.Date.Date >= parsedDebut.Date &&
                                dd.Entetedemande.Date.Date <= parsedFin.Date)
                    .AsEnumerable()
                    .DistinctBy(x => x.Detaildemandeid)
                    .ToList();

                // Calculer les montants
                var montantTotal = detaildemandes.Sum(dd => dd.Prix);
                var remiseTotal = montantTotal * 0.20m; // 20% du montant total
                var montantNet = montantTotal - remiseTotal;

                // Première requête
                var query1Raw =
                    from facture in _context.Entetefactures
                    join demande in _context.Entetedemandes on facture.Entetedemandeid equals demande.Entetedemandeid
                    join detail in _context.Detaildemandes on demande.Entetedemandeid equals detail.Entetedemandeid
                    join police in _context.Policeassurances on demande.Policeassuranceid equals police.Policeassuranceid
                    join assurance in _context.Assurances on police.Codeassurance equals assurance.Codeassurance
                    join categorie in _context.Categories on detail.Categorieid equals categorie.Categorieid
                    join catAnalyse in _context.Categorieanalyses on categorie.Categorieid equals catAnalyse.Categorieid
                    join analyse in _context.Analyses on catAnalyse.Idanalyse equals analyse.Idanalyse
                    join labo in _context.Loboratoires on analyse.Idlaboratoire equals labo.Idlaboratoire
                    where facture.Date.Date >= parsedDebut.Date
                       && facture.Date.Date <= parsedFin.Date
                       && assurance.Codeassurance == codeassurance
                    select new
                    {
                        Date = facture.Date,
                        Id = analyse.Idanalyse,
                        Nom = analyse.Nom,
                        Prix = detail.Prix,
                        Idlaboratoire = labo.Idlaboratoire,
                        NomLaboratoire = labo.Nom
                    };

                var query2Raw =
                    from facture in _context.Entetefactures
                    join demande in _context.Entetedemandes on facture.Entetedemandeid equals demande.Entetedemandeid
                    join detail in _context.Detaildemandes on demande.Entetedemandeid equals detail.Entetedemandeid
                    join analyse in _context.Analyses on detail.Idanalyse equals analyse.Idanalyse
                    join police in _context.Policeassurances on demande.Policeassuranceid equals police.Policeassuranceid
                    join assurance in _context.Assurances on police.Codeassurance equals assurance.Codeassurance
                    join labo in _context.Loboratoires on analyse.Idlaboratoire equals labo.Idlaboratoire
                    where facture.Date.Date >= parsedDebut.Date
                       && facture.Date.Date <= parsedFin.Date
                       && assurance.Codeassurance == codeassurance
                    select new
                    {
                        Date = facture.Date,
                        Id = analyse.Idanalyse,
                        Nom = analyse.Nom,
                        Prix = detail.Prix,
                        Idlaboratoire = labo.Idlaboratoire,
                        NomLaboratoire = labo.Nom
                    };

                // Union des deux requêtes et tri final
                var unionQuery = query1Raw
                    .Concat(query2Raw)
                    .AsEnumerable() // Passe tout en mémoire ici
                    .GroupBy(x => x.Id)
                    .Select(g =>
                    {
                        var first = g.First();
                        var nbre = g.Count();
                        var prix = first.Prix;
                        var montant = nbre * prix;
                        var remise = Math.Round(montant * 0.20m, 2);

                        return new
                        {
                            Id = first.Id,
                            Nom = first.Nom,
                            Idlaboratoire = first.Idlaboratoire,
                            NomLaboratoire = first.NomLaboratoire,
                            Nbre = nbre,
                            Prix = prix,
                            Montant = montant,
                            Remise = remise
                        };
                    })
                    .OrderBy(x => x.Nom)
                    .ToList();

                var laboratoires = unionQuery
                    .Select(x => new { x.Idlaboratoire, x.NomLaboratoire })
                    .DistinctBy(x => x.Idlaboratoire)
                    .OrderBy(x => x.NomLaboratoire)
                    .ToList();

                using var stream = new MemoryStream();
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf, PageSize.A4);
                document.SetMargins(36, 36, 36, 36);

                float pageHeight = PageSize.A4.GetHeight();

                float marginTop = document.GetTopMargin();
                float marginBottom = document.GetBottomMargin();
                float marginLeft = document.GetLeftMargin();
                float marginRight = document.GetRightMargin();

                Console.WriteLine($"Top: {marginTop}, Bottom: {marginBottom}, Left: {marginLeft}, Right: {marginRight}");

                // S’assurer que les encodages étendus sont disponibles
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                PdfFont fontNormal = PdfFontFactory.CreateFont(GetFontPath(true, false), PdfEncodings.IDENTITY_H);
                PdfFont fontBold = PdfFontFactory.CreateFont(GetFontPath(false, true), PdfEncodings.IDENTITY_H);

                Console.WriteLine($"Normal: {GetFontPath(true, false)}, Bold: {GetFontPath(false, true)}");

                var debutStr = parsedDebut.ToShortDateString();
                var finStr = parsedFin.ToShortDateString();
                var assurancenom = objAssurance?.Nom;
                var date = DateTime.Now;
                var dateImpression = $"{date.ToShortDateString()} {date.ToShortTimeString()}";

                string login = User.Identity?.Name ?? string.Empty;
                string operateur = login;
                var utilisateur = _context.Utilisateurs.FirstOrDefault(u => u.Login == login);
                if (utilisateur != null)
                {
                    operateur = $"{utilisateur.Nom} {utilisateur.Prenom}".Trim();
                }

                // Colors
                var lightGray = ColorConstants.LIGHT_GRAY; // Whitesmoke n'existe pas, approximation

                // === ENTETE ===

                // Ligne 1 - Table à 3 colonnes (4, 11, 4 cm)
                {
                    var table = new Table(UnitValue.CreatePointArray(new float[] { CmToPt(4), CmToPt(11), CmToPt(4) }));
                    table.SetWidth(UnitValue.CreatePointValue(CmToPt(19)));

                    table.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                    var innerTable = new Table(1).SetWidth(UnitValue.CreatePointValue(CmToPt(11))).SetBorder(new SolidBorder(ColorConstants.BLACK, 1));
                    var cell = CreateCell("Titre de recette (Régularisation)", fontNormal, 16, TextAlignment.CENTER);
                    cell.SetBackgroundColor(lightGray);
                    innerTable.AddCell(cell);

                    Cell innerWrapper = new Cell().Add(innerTable)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE);
                    table.AddCell(innerWrapper);

                    table.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                    document.Add(table);
                }

                // Espace 0.5 cm
                document.Add(new Paragraph().SetMarginTop(CmToPt(0.5f)));

                // Période
                {
                    var table = new Table(1).SetWidth(UnitValue.CreatePointValue(CmToPt(19)));
                    table.AddCell(CreateCell($"Période du : {debutStr} au {finStr}", fontNormal, 11, TextAlignment.CENTER));
                    document.Add(table);
                }

                // Espace 0.25 cm
                document.Add(new Paragraph().SetMarginTop(CmToPt(0.25f)));

                // Type de client
                {
                    var table = new Table(1).SetWidth(UnitValue.CreatePointValue(CmToPt(19)));
                    table.AddCell(CreateCell($"Type de client : {assurancenom}", fontNormal, 11, TextAlignment.CENTER));
                    document.Add(table);
                }

                // Espace 0.5 cm
                document.Add(new Paragraph().SetMarginTop(CmToPt(0.5f)));

                // === BODY ===

                decimal totaux = 0;
                decimal totauxRemise = 0;
                decimal totauxDifference = 0;

                var cultureFr = new CultureInfo("fr-FR");

                foreach (var laboratoire in laboratoires)
                {
                    var details = unionQuery
                        .Where(x => x.Idlaboratoire == laboratoire.Idlaboratoire)
                        .ToList();

                    var total = details.Sum(x => x.Montant);
                    var remise = details.Sum(x => x.Remise);
                    var difference = total - remise;

                    totaux += total;
                    totauxRemise += remise;
                    totauxDifference += difference;

                    // Département
                    {
                        var table = new Table(UnitValue.CreatePointArray(new float[] { CmToPt(2.75f), CmToPt(16.25f) }));
                        table.AddCell(CreateCell("Département :", fontBold, 11, TextAlignment.LEFT));
                        table.AddCell(CreateCell(laboratoire.NomLaboratoire, fontNormal, 11, TextAlignment.LEFT));
                        document.Add(table);
                    }

                    document.Add(new Paragraph().SetMarginTop(CmToPt(0.25f)));

                    // Table des actes (4 colonnes)
                    {
                        var widths = new float[] { CmToPt(8), CmToPt(3), CmToPt(4), CmToPt(4) };
                        var table = new Table(UnitValue.CreatePointArray(widths)).SetWidth(UnitValue.CreatePointValue(CmToPt(19)));

                        // Header
                        table.AddHeaderCell(CreateCell("Désignation des actes", fontBold, 11, TextAlignment.LEFT).SetBackgroundColor(lightGray).SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));
                        table.AddHeaderCell(CreateCell("Nombre", fontBold, 11, TextAlignment.CENTER).SetBackgroundColor(lightGray).SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));
                        table.AddHeaderCell(CreateCell("P.U.", fontBold, 11, TextAlignment.RIGHT).SetBackgroundColor(lightGray).SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));
                        table.AddHeaderCell(CreateCell("Montant", fontBold, 11, TextAlignment.RIGHT).SetBackgroundColor(lightGray).SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                        foreach (var item in details)
                        {
                            // Body row (1 ligne exemple)
                            table.AddCell(CreateCell(item.Nom, fontNormal, 11, TextAlignment.LEFT));
                            table.AddCell(CreateCell(item.Nbre.ToString("N0", cultureFr), fontNormal, 11, TextAlignment.CENTER));
                            table.AddCell(CreateCell(item.Prix.ToString("N0", cultureFr), fontNormal, 11, TextAlignment.RIGHT));
                            table.AddCell(CreateCell(item.Montant.ToString("N0", cultureFr), fontNormal, 11, TextAlignment.RIGHT));
                        }

                        document.Add(table);

                        document.Add(new Paragraph().SetMarginTop(CmToPt(0.25f)));

                        // Totaux
                        // ligne 1 : [valeur] | Total : | [valeur]
                        {
                            var table1 = new Table(UnitValue.CreatePointArray(new float[] { CmToPt(11), CmToPt(4), CmToPt(4) }));
                            table1.AddCell(CreateCell(laboratoire.NomLaboratoire, fontNormal, 11, TextAlignment.RIGHT));
                            table1.AddCell(CreateCell("Total :", fontBold, 11, TextAlignment.RIGHT));
                            table1.AddCell(CreateCell(total.ToString("N0", cultureFr), fontNormal, 11, TextAlignment.RIGHT));
                            document.Add(table1);
                        }

                        // ligne 2 : vide | Total remise : | [valeur]
                        {
                            var table1 = new Table(UnitValue.CreatePointArray(new float[] { CmToPt(11), CmToPt(4), CmToPt(4) }));
                            table1.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER)); // vide
                            table1.AddCell(CreateCell("Total remise :", fontBold, 11, TextAlignment.RIGHT));
                            table1.AddCell(CreateCell(remise.ToString("N0", cultureFr), fontNormal, 11, TextAlignment.RIGHT));
                            document.Add(table1);
                        }

                        // ligne 3 : vide | Différence : | [valeur]
                        {
                            var table1 = new Table(UnitValue.CreatePointArray(new float[] { CmToPt(11), CmToPt(4), CmToPt(4) }));
                            table1.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER)); // vide
                            table1.AddCell(CreateCell("Différence :", fontBold, 11, TextAlignment.RIGHT));
                            table1.AddCell(CreateCell(difference.ToString("N0", cultureFr), fontNormal, 11, TextAlignment.RIGHT));
                            document.Add(table1);
                        }
                    }
                }

                // Trait noir largeur 19 cm
                {
                    document.Add(new Paragraph().SetMarginTop(CmToPt(0.25f)));
                    document.Add(new LineSeparator(new SolidLine()).SetWidth(CmToPt(19)));
                    document.Add(new Paragraph().SetMarginTop(CmToPt(0.25f)));
                }

                // Totaux finaux 
                // Ligne 1 : Total : | [valeur]
                {
                    var table = new Table(UnitValue.CreatePointArray(new float[] { CmToPt(15), CmToPt(4) }));
                    table.AddCell(CreateCell("Total :", fontBold, 11, TextAlignment.RIGHT));
                    table.AddCell(CreateCell(totaux.ToString("N0", cultureFr), fontNormal, 11, TextAlignment.RIGHT));
                    document.Add(table);
                }

                // Ligne 2 : Total remise : | [valeur]
                {
                    var table = new Table(UnitValue.CreatePointArray(new float[] { CmToPt(15), CmToPt(4) }));
                    table.AddCell(CreateCell("Total remise :", fontBold, 11, TextAlignment.RIGHT));
                    table.AddCell(CreateCell(totauxRemise.ToString("N0", cultureFr), fontNormal, 11, TextAlignment.RIGHT));
                    document.Add(table);
                }

                // Ligne 3 : Différence : | [valeur]
                {
                    var table = new Table(UnitValue.CreatePointArray(new float[] { CmToPt(15), CmToPt(4) }));
                    table.AddCell(CreateCell("Différence :", fontBold, 11, TextAlignment.RIGHT));
                    table.AddCell(CreateCell(totauxDifference.ToString("N0", cultureFr), fontNormal, 11, TextAlignment.RIGHT));
                    document.Add(table);
                }

                document.Close();

                byte[] pdfBytes = stream.ToArray();

                // Exemple de réouverture et modification
                using (var input = new MemoryStream(pdfBytes))
                using (var output = new MemoryStream())
                {
                    var reader = new PdfReader(input);
                    var writer1 = new PdfWriter(output);
                    var pdfDoc = new PdfDocument(reader, writer1);

                    // Ajout des numéros de page (code selon besoin)
                    float height = CmToPt(0.6f); // Hauteur du footer
                    float width = CmToPt(2f);    // Largeur réservée pour le numéro de page
                    var fontFooter = PdfFontFactory.CreateFont(GetFontPath(true, false), PdfEncodings.IDENTITY_H);

                    // Nombre total de pages
                    int numberOfPages = pdfDoc.GetNumberOfPages();

                    //Parcours des pages
                    for (int i = 1; i <= numberOfPages; i++)
                    {
                        PdfPage page = pdfDoc.GetPage(i);

                        float x = PageSize.A4.GetWidth() - marginRight - width / 2; // x depuis la droite
                        float y = marginBottom / 2 - height; // centré verticalement dans la marge

                        // Format : 1/3, 2/3, etc.
                        var pageNumberText = $"{i}/{numberOfPages}";

                        PdfCanvas pdfCanvas = new PdfCanvas(page);

                        //// Bordure noire
                        //pdfCanvas.SetLineWidth(1)
                        //  .SetStrokeColor(ColorConstants.BLACK)
                        //  .Rectangle(x, y, width, height)
                        //  .Stroke();

                        // Commence à écrire du texte
                        pdfCanvas.BeginText()
                            .SetFontAndSize(fontFooter, 11)
                            .SetFillColor(ColorConstants.BLACK)
                            .MoveText(x, y)
                            .ShowText(pageNumberText)
                            .EndText()
                            .Release();
                    }

                    pdfDoc.Close();

                    string base64 = Convert.ToBase64String(output.ToArray());

                    return Json(new
                    {
                        success = true,
                        pdfBase64 = base64
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "erreur lors de la génération de rapport");
                throw;
            }
        }

        [HttpGet]
        public IActionResult TitreRecetteClientPayant()
        {
            return View(new TitreRecetteClientPayantVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TitreRecetteClientPayant(TitreRecetteClientPayantVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            ViewBag.Message = $"Période saisie : du {vm.Debut} au {vm.Fin} pour les clients payants";
            return View(vm);
        }

        [HttpGet]
        public IActionResult GenererTitreRecetteClientPayant(string debut, string fin)
        {
            if (string.IsNullOrWhiteSpace(debut) || !DateTime.TryParse(debut, out var parsedDebut) ||
                string.IsNullOrWhiteSpace(fin) || !DateTime.TryParse(fin, out var parsedFin))
            {
                return BadRequest("Les paramètres de dates sont invalides.");
            }

            if (parsedDebut > parsedFin)
            {
                return BadRequest("La date de début doit être antérieure à la date de fin.");
            }

            try
            {
                // Client payant: demande sans assurance et sans partenaire.
                var detaildemandes = _context.Detaildemandes
                    .Include(dd => dd.Entetedemande)
                    .Include(dd => dd.Detailfactures)
                        .ThenInclude(df => df.Entetefacture)
                    .Where(dd => dd.Entetedemande.Policeassuranceid == null
                                && dd.Entetedemande.Partenaireid == null
                                && dd.Entetedemande.Date.Date >= parsedDebut.Date
                                && dd.Entetedemande.Date.Date <= parsedFin.Date)
                    .AsEnumerable()
                    .DistinctBy(x => x.Detaildemandeid)
                    .ToList();

                var montantTotal = detaildemandes.Sum(dd => dd.Prix);
                var remiseTotal = montantTotal * 0.20m;
                var montantNet = montantTotal - remiseTotal;

                var query1Raw =
                    from facture in _context.Entetefactures
                    join demande in _context.Entetedemandes on facture.Entetedemandeid equals demande.Entetedemandeid
                    join detail in _context.Detaildemandes on demande.Entetedemandeid equals detail.Entetedemandeid
                    join categorie in _context.Categories on detail.Categorieid equals categorie.Categorieid
                    join catAnalyse in _context.Categorieanalyses on categorie.Categorieid equals catAnalyse.Categorieid
                    join analyse in _context.Analyses on catAnalyse.Idanalyse equals analyse.Idanalyse
                    join labo in _context.Loboratoires on analyse.Idlaboratoire equals labo.Idlaboratoire
                    where facture.Date.Date >= parsedDebut.Date
                       && facture.Date.Date <= parsedFin.Date
                       && demande.Policeassuranceid == null
                       && demande.Partenaireid == null
                    select new
                    {
                        Date = facture.Date,
                        Id = analyse.Idanalyse,
                        Nom = analyse.Nom,
                        Prix = detail.Prix,
                        Idlaboratoire = labo.Idlaboratoire,
                        NomLaboratoire = labo.Nom
                    };

                var query2Raw =
                    from facture in _context.Entetefactures
                    join demande in _context.Entetedemandes on facture.Entetedemandeid equals demande.Entetedemandeid
                    join detail in _context.Detaildemandes on demande.Entetedemandeid equals detail.Entetedemandeid
                    join analyse in _context.Analyses on detail.Idanalyse equals analyse.Idanalyse
                    join labo in _context.Loboratoires on analyse.Idlaboratoire equals labo.Idlaboratoire
                    where facture.Date.Date >= parsedDebut.Date
                       && facture.Date.Date <= parsedFin.Date
                       && demande.Policeassuranceid == null
                       && demande.Partenaireid == null
                    select new
                    {
                        Date = facture.Date,
                        Id = analyse.Idanalyse,
                        Nom = analyse.Nom,
                        Prix = detail.Prix,
                        Idlaboratoire = labo.Idlaboratoire,
                        NomLaboratoire = labo.Nom
                    };

                var unionQuery = query1Raw
                    .Concat(query2Raw)
                    .AsEnumerable()
                    .GroupBy(x => x.Id)
                    .Select(g =>
                    {
                        var first = g.First();
                        var nbre = g.Count();
                        var prix = first.Prix;
                        var montant = nbre * prix;
                        var remise = Math.Round(montant * 0.20m, 2);

                        return new
                        {
                            Id = first.Id,
                            Nom = first.Nom,
                            Idlaboratoire = first.Idlaboratoire,
                            NomLaboratoire = first.NomLaboratoire,
                            Nbre = nbre,
                            Prix = prix,
                            Montant = montant,
                            Remise = remise
                        };
                    })
                    .OrderBy(x => x.Nom)
                    .ToList();

                var laboratoires = unionQuery
                    .Select(x => new { x.Idlaboratoire, x.NomLaboratoire })
                    .DistinctBy(x => x.Idlaboratoire)
                    .OrderBy(x => x.NomLaboratoire)
                    .ToList();

                using var stream = new MemoryStream();
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf, PageSize.A4);
                document.SetMargins(36, 36, 36, 36);

                float marginBottom = document.GetBottomMargin();
                float marginRight = document.GetRightMargin();

                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                PdfFont fontNormal = PdfFontFactory.CreateFont(GetFontPath(true, false), PdfEncodings.IDENTITY_H);
                PdfFont fontBold = PdfFontFactory.CreateFont(GetFontPath(false, true), PdfEncodings.IDENTITY_H);

                var debutStr = parsedDebut.ToShortDateString();
                var finStr = parsedFin.ToShortDateString();

                var lightGray = ColorConstants.LIGHT_GRAY;

                {
                    var table = new Table(UnitValue.CreatePointArray(new float[] { CmToPt(4), CmToPt(11), CmToPt(4) }));
                    table.SetWidth(UnitValue.CreatePointValue(CmToPt(19)));

                    table.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                    var innerTable = new Table(1).SetWidth(UnitValue.CreatePointValue(CmToPt(11))).SetBorder(new SolidBorder(ColorConstants.BLACK, 1));
                    var cell = CreateCell("Titre de recette (Régularisation)", fontNormal, 16, TextAlignment.CENTER);
                    cell.SetBackgroundColor(lightGray);
                    innerTable.AddCell(cell);

                    Cell innerWrapper = new Cell().Add(innerTable)
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE);
                    table.AddCell(innerWrapper);

                    table.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                    document.Add(table);
                }

                document.Add(new Paragraph().SetMarginTop(CmToPt(0.5f)));

                {
                    var table = new Table(1).SetWidth(UnitValue.CreatePointValue(CmToPt(19)));
                    table.AddCell(CreateCell($"Période du : {debutStr} au {finStr}", fontNormal, 11, TextAlignment.CENTER));
                    document.Add(table);
                }

                document.Add(new Paragraph().SetMarginTop(CmToPt(0.25f)));

                {
                    var table = new Table(1).SetWidth(UnitValue.CreatePointValue(CmToPt(19)));
                    table.AddCell(CreateCell("Type de client : CLIENT PAYANT", fontNormal, 11, TextAlignment.CENTER));
                    document.Add(table);
                }

                document.Add(new Paragraph().SetMarginTop(CmToPt(0.5f)));

                decimal totaux = 0;
                decimal totauxRemise = 0;
                decimal totauxDifference = 0;

                var cultureFr = new CultureInfo("fr-FR");

                foreach (var laboratoire in laboratoires)
                {
                    var details = unionQuery
                        .Where(x => x.Idlaboratoire == laboratoire.Idlaboratoire)
                        .ToList();

                    var total = details.Sum(x => x.Montant);
                    var remise = details.Sum(x => x.Remise);
                    var difference = total - remise;

                    totaux += total;
                    totauxRemise += remise;
                    totauxDifference += difference;

                    {
                        var table = new Table(UnitValue.CreatePointArray(new float[] { CmToPt(2.75f), CmToPt(16.25f) }));
                        table.AddCell(CreateCell("Département :", fontBold, 11, TextAlignment.LEFT));
                        table.AddCell(CreateCell(laboratoire.NomLaboratoire, fontNormal, 11, TextAlignment.LEFT));
                        document.Add(table);
                    }

                    document.Add(new Paragraph().SetMarginTop(CmToPt(0.25f)));

                    {
                        var widths = new float[] { CmToPt(8), CmToPt(3), CmToPt(4), CmToPt(4) };
                        var table = new Table(UnitValue.CreatePointArray(widths)).SetWidth(UnitValue.CreatePointValue(CmToPt(19)));

                        table.AddHeaderCell(CreateCell("Désignation des actes", fontBold, 11, TextAlignment.LEFT).SetBackgroundColor(lightGray).SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));
                        table.AddHeaderCell(CreateCell("Nombre", fontBold, 11, TextAlignment.CENTER).SetBackgroundColor(lightGray).SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));
                        table.AddHeaderCell(CreateCell("P.U.", fontBold, 11, TextAlignment.RIGHT).SetBackgroundColor(lightGray).SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));
                        table.AddHeaderCell(CreateCell("Montant", fontBold, 11, TextAlignment.RIGHT).SetBackgroundColor(lightGray).SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                        foreach (var item in details)
                        {
                            table.AddCell(CreateCell(item.Nom, fontNormal, 11, TextAlignment.LEFT));
                            table.AddCell(CreateCell(item.Nbre.ToString("N0", cultureFr), fontNormal, 11, TextAlignment.CENTER));
                            table.AddCell(CreateCell(item.Prix.ToString("N0", cultureFr), fontNormal, 11, TextAlignment.RIGHT));
                            table.AddCell(CreateCell(item.Montant.ToString("N0", cultureFr), fontNormal, 11, TextAlignment.RIGHT));
                        }

                        document.Add(table);

                        document.Add(new Paragraph().SetMarginTop(CmToPt(0.25f)));

                        {
                            var table1 = new Table(UnitValue.CreatePointArray(new float[] { CmToPt(11), CmToPt(4), CmToPt(4) }));
                            table1.AddCell(CreateCell(laboratoire.NomLaboratoire, fontNormal, 11, TextAlignment.RIGHT));
                            table1.AddCell(CreateCell("Total :", fontBold, 11, TextAlignment.RIGHT));
                            table1.AddCell(CreateCell(total.ToString("N0", cultureFr), fontNormal, 11, TextAlignment.RIGHT));
                            document.Add(table1);
                        }

                        {
                            var table1 = new Table(UnitValue.CreatePointArray(new float[] { CmToPt(11), CmToPt(4), CmToPt(4) }));
                            table1.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                            table1.AddCell(CreateCell("Total remise :", fontBold, 11, TextAlignment.RIGHT));
                            table1.AddCell(CreateCell(remise.ToString("N0", cultureFr), fontNormal, 11, TextAlignment.RIGHT));
                            document.Add(table1);
                        }

                        {
                            var table1 = new Table(UnitValue.CreatePointArray(new float[] { CmToPt(11), CmToPt(4), CmToPt(4) }));
                            table1.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                            table1.AddCell(CreateCell("Différence :", fontBold, 11, TextAlignment.RIGHT));
                            table1.AddCell(CreateCell(difference.ToString("N0", cultureFr), fontNormal, 11, TextAlignment.RIGHT));
                            document.Add(table1);
                        }
                    }
                }

                {
                    document.Add(new Paragraph().SetMarginTop(CmToPt(0.25f)));
                    document.Add(new LineSeparator(new SolidLine()).SetWidth(CmToPt(19)));
                    document.Add(new Paragraph().SetMarginTop(CmToPt(0.25f)));
                }

                {
                    var table = new Table(UnitValue.CreatePointArray(new float[] { CmToPt(15), CmToPt(4) }));
                    table.AddCell(CreateCell("Total :", fontBold, 11, TextAlignment.RIGHT));
                    table.AddCell(CreateCell(totaux.ToString("N0", cultureFr), fontNormal, 11, TextAlignment.RIGHT));
                    document.Add(table);
                }

                {
                    var table = new Table(UnitValue.CreatePointArray(new float[] { CmToPt(15), CmToPt(4) }));
                    table.AddCell(CreateCell("Total remise :", fontBold, 11, TextAlignment.RIGHT));
                    table.AddCell(CreateCell(totauxRemise.ToString("N0", cultureFr), fontNormal, 11, TextAlignment.RIGHT));
                    document.Add(table);
                }

                {
                    var table = new Table(UnitValue.CreatePointArray(new float[] { CmToPt(15), CmToPt(4) }));
                    table.AddCell(CreateCell("Différence :", fontBold, 11, TextAlignment.RIGHT));
                    table.AddCell(CreateCell(totauxDifference.ToString("N0", cultureFr), fontNormal, 11, TextAlignment.RIGHT));
                    document.Add(table);
                }

                document.Close();

                byte[] pdfBytes = stream.ToArray();

                using (var input = new MemoryStream(pdfBytes))
                using (var output = new MemoryStream())
                {
                    var reader = new PdfReader(input);
                    var writer1 = new PdfWriter(output);
                    var pdfDoc = new PdfDocument(reader, writer1);

                    float height = CmToPt(0.6f);
                    float width = CmToPt(2f);
                    var fontFooter = PdfFontFactory.CreateFont(GetFontPath(true, false), PdfEncodings.IDENTITY_H);

                    int numberOfPages = pdfDoc.GetNumberOfPages();

                    for (int i = 1; i <= numberOfPages; i++)
                    {
                        PdfPage page = pdfDoc.GetPage(i);

                        float x = PageSize.A4.GetWidth() - marginRight - width / 2;
                        float y = marginBottom / 2 - height;

                        var pageNumberText = $"{i}/{numberOfPages}";

                        PdfCanvas pdfCanvas = new PdfCanvas(page);

                        pdfCanvas.BeginText()
                            .SetFontAndSize(fontFooter, 11)
                            .SetFillColor(ColorConstants.BLACK)
                            .MoveText(x, y)
                            .ShowText(pageNumberText)
                            .EndText()
                            .Release();
                    }

                    pdfDoc.Close();

                    string base64 = Convert.ToBase64String(output.ToArray());

                    return Json(new
                    {
                        success = true,
                        pdfBase64 = base64
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "erreur lors de la génération du rapport client payant");
                throw;
            }
        }

    }
} 
