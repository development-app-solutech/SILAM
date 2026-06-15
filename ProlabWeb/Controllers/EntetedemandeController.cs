using AutoMapper;
using FastReport;
//using FastReport.Export.PdfSimple;
using Humanizer;
using iText.IO.Font;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProlabWeb.Data;
using ProlabWeb.Db;
using ProlabWeb.Helpers;
using ProlabWeb.Models;
using ProlabWeb.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.IO;
using static iText.StyledXmlParser.Css.Font.CssFontFace;
using static ProlabWeb.Helpers.Utilities;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Validator = System.ComponentModel.DataAnnotations.Validator;

namespace ProlabWeb.Controllers
{
    [AllowAnonymous]
    public class EntetedemandeController : Controller
    {
        private readonly ProlabwebContext _context;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ProlabIdentityUser> _usermanager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EntetedemandeController> _logger;

        public EntetedemandeController(ProlabwebContext context, IMapper mapper, IWebHostEnvironment webHostEnvironment, UserManager<ProlabIdentityUser> usermanager, IConfiguration configuration, ILogger<EntetedemandeController> logger)
        {
            _context = context;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
            _usermanager = usermanager;
            _configuration = configuration;
            _logger = logger;
        }

        private string GetFontPath(string font, bool normal, bool bold)
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

            string fontPath = _configuration[$"{os}:Fonts:{font}:{type}"];

            return fontPath;
        }

        private async Task<(bool isCaisse, bool isTechnicien, List<int> laboratoireIds)> GetCurrentUserRoleAndLaboratoires()
        {
            var user = await _usermanager.GetUserAsync(User);
            if (user == null)
                return (false, false, new List<int>());

            var roles = await _usermanager.GetRolesAsync(user);
            bool isCaisse = roles.Contains(EnumRoles.Caisse.ToString());
            bool isTechnicien = roles.Contains(EnumRoles.Technicien.ToString());

            var laboratoireIds = new List<int>();

            if (isTechnicien)
            {
                // Récupérer l'utilisateur avec ses laboratoires affectés
                var utilisateur = await _context.Utilisateurs.AsNoTracking()
                    .Where(x => x.Userid == user.Id)
                    .FirstOrDefaultAsync();

                if (utilisateur != null)
                {
                    laboratoireIds = await _context.Utilisateurlaboratoires.AsNoTracking()
                        .Where(ul => ul.Utilisateurid == utilisateur.Utilisateurid)
                        .Select(ul => ul.Idlaboratoire)
                        .ToListAsync();
                }
            }

            return (isCaisse, isTechnicien, laboratoireIds);
        }

        // GET: Entetedemande
        public async Task<IActionResult> Index()
        {
            // Récupérer le rôle de l'utilisateur connecté et ses laboratoires
            var (isCaisse, isTechnicien, laboratoireIds) = await GetCurrentUserRoleAndLaboratoires();

            //detail demande
            var detaildemandes = await _context.Detaildemandes.AsNoTracking()
                .Include(x => x.Categorie)
                .Include(x => x.IdanalyseNavigation)
                .ToListAsync();

            // Get all category-analysis relationships
            var categorieanalyses = await _context.Categorieanalyses.AsNoTracking()
                .ToListAsync();

            // Get all results with data (not just validated ones) to calculate demande status
            // Important: Get results from ALL laboratories, not just the current user's lab
            // A result is considered "completed" if it has at least one Detailresultat with actual data
            var enteteresultats = await _context.Enteteresultats.AsNoTracking()
                .Include(x => x.Detailresultats)
                .Where(x => x.Detailresultats.Any(d => !string.IsNullOrEmpty(d.Resultat) || !string.IsNullOrEmpty(d.Resultatsi)))
                .ToListAsync();

            //entete demande - récupération de base
            var entetedemandesQuery = _context.Entetedemandes.AsNoTracking()
                .Include(e => e.CodesiteNavigation)
                .Include(e => e.Patient)
                .ThenInclude(e => e.CodesexeNavigation)
                .Include(e => e.Policeassurance)
                .ThenInclude(e => e.CodeassuranceNavigation)
                .Include(e => e.Prescripteur)
                .OrderByDescending(e => e.Date);

            List<EntetedemandeModel> entetedemandes;

            if (isCaisse)
            {
                // Rôle Caisse : voir toutes les demandes
                entetedemandes = await entetedemandesQuery
                    .Select(x => new EntetedemandeModel
                    {
                        Entetedemandeid = x.Entetedemandeid,
                        Numero = x.Numero,
                        Date = x.Date,
                        Nom = x.Patient.Nom,
                        Prenom = x.Patient.Prenom,
                        Sexe = x.Patient.CodesexeNavigation.Value,
                        Datenaissance = x.Patient.Datenaissance,
                        Prescripteur = x.Prescripteur.Nom,
                        Assurance = x.Policeassurance != null ? x.Policeassurance.CodeassuranceNavigation.Nom : "",
                        Taux = x.Policeassurance != null ? string.Join(" ", x.Policeassurance.Taux, "%") : "",
                    })
                    .ToListAsync();
            }
            else if (isTechnicien && laboratoireIds.Any())
            {
                // Rôle Technicien : voir seulement les demandes avec des analyses de ses laboratoires
                // D'abord, obtenir tous les IDs des demandes qui contiennent des analyses de ses laboratoires
                var demandesAvecAnalysesDesLaboratoires = new HashSet<Guid>();

                // Analyses directes dans les demandes
                var demandesAnalysesDirectes = await _context.Detaildemandes.AsNoTracking()
                    .Where(dd => dd.Idanalyse != null && laboratoireIds.Contains(dd.IdanalyseNavigation.Idlaboratoire))
                    .Select(dd => dd.Entetedemandeid)
                    .Distinct()
                    .ToListAsync();

                demandesAvecAnalysesDesLaboratoires.UnionWith(demandesAnalysesDirectes);

                // Analyses via les catégories
                var demandesAnalysesCategories = await _context.Detaildemandes.AsNoTracking()
                    .Where(dd => dd.Categorieid != null &&
                        dd.Categorie.Categorieanalyses.Any(ca => laboratoireIds.Contains(ca.IdanalyseNavigation.Idlaboratoire)))
                    .Select(dd => dd.Entetedemandeid)
                    .Distinct()
                    .ToListAsync();

                demandesAvecAnalysesDesLaboratoires.UnionWith(demandesAnalysesCategories);

                // Maintenant filtrer les demandes selon les IDs trouvés
                entetedemandes = await entetedemandesQuery
                    .Where(e => demandesAvecAnalysesDesLaboratoires.Contains(e.Entetedemandeid))
                    .Select(x => new EntetedemandeModel
                    {
                        Entetedemandeid = x.Entetedemandeid,
                        Numero = x.Numero,
                        Date = x.Date,
                        Nom = x.Patient.Nom,
                        Prenom = x.Patient.Prenom,
                        Sexe = x.Patient.CodesexeNavigation.Value,
                        Datenaissance = x.Patient.Datenaissance,
                        Prescripteur = x.Prescripteur.Nom,
                        Assurance = x.Policeassurance != null ? x.Policeassurance.CodeassuranceNavigation.Nom : "",
                        Taux = x.Policeassurance != null ? string.Join(" ", x.Policeassurance.Taux, "%") : "",
                    })
                    .ToListAsync();
            }
            else
            {
                // Aucun rôle reconnu ou technicien sans laboratoire : aucune demande
                entetedemandes = new List<EntetedemandeModel>();
            }

            foreach (var item in entetedemandes)
            {
                var detail = detaildemandes.Where(x => x.Entetedemandeid == item.Entetedemandeid).ToList();

                var categories = detail.Where(x => x.Categorieid != null).Select(x => x.Categorie.Nom.Trim()).ToList();

                var analyses = detail.Where(x => x.Idanalyse != null).Select(x => x.IdanalyseNavigation.Nom.Trim()).ToList();

                item.Analyses = string.Join(", ", categories.Concat(analyses));

                // Calculate status based on requested analyses and completed results
                var requestedAnalyseIds = new List<Guid>();

                // Add direct analyses
                requestedAnalyseIds.AddRange(detail.Where(x => x.Idanalyse != null).Select(x => x.Idanalyse.Value));

                // Add analyses from categories
                var categoryIds = detail.Where(x => x.Categorieid != null).Select(x => x.Categorieid.Value);
                var analysesFromCategories = categorieanalyses
                    .Where(ca => categoryIds.Contains(ca.Categorieid))
                    .Select(ca => ca.Idanalyse);
                requestedAnalyseIds.AddRange(analysesFromCategories);

                // Get completed analyses for this demande
                var completedAnalyseIds = enteteresultats
                    .Where(er => er.Entetedemandeid == item.Entetedemandeid)
                    .Select(er => er.Idanalyse)
                    .ToList();

                // Calculate status with proper deduplication
                var distinctRequested = requestedAnalyseIds.Distinct().ToList();
                var distinctCompleted = completedAnalyseIds.Distinct().ToList();
                
                item.Statut = Utilities.CalculerStatutDemande(distinctRequested, distinctCompleted);
            }

            return View(entetedemandes);
        }

        // GET: Entetedemande/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entetedemande = await _context.Entetedemandes
                .Include(e => e.CodesiteNavigation)
                .Include(e => e.Patient)
                .Include(e => e.Policeassurance)
                .Include(e => e.Prescripteur)
                .FirstOrDefaultAsync(m => m.Entetedemandeid == id);
            if (entetedemande == null)
            {
                return NotFound();
            }

            return View(entetedemande);
        }

        private List<SelectListItem> SelectPatientList(Guid? selectionId = null)
        {
            var patients = _context.Patients.AsNoTracking()
                .Include(x => x.CodesexeNavigation)
                .Select(x => new SelectListItem
                {
                    Selected = selectionId != null && selectionId != Guid.Empty ? x.Patientid == selectionId : false,
                    Value = x.Patientid.ToString(),
                    Text = string.Join(", ", new List<object> {
                        $"{x.Nom} {x.Prenom}",
                        x.CodesexeNavigation.Value,
                        x.Datenaissance.ToShortDateString()
                    })
                })
                .ToList();
            patients.Insert(0, new SelectListItem("--- Nouveau ---", string.Empty));

            return patients;
        }

        private List<SelectListItem> SelectPrescripteurList(Guid? selectionId = null)
        {
            var prescripteurs = _context.Prescripteurs.AsNoTracking()
                .Select(x => new SelectListItem
                {
                    Selected = selectionId != null && selectionId != Guid.Empty ? x.Prescripteurid == selectionId : false,
                    Value = x.Prescripteurid.ToString(),
                    Text = string.Join(", ", new List<object> {
                        x.Nom,
                        x.Adresse,
                        x.Tel
                    })
                })
                .ToList();
            prescripteurs.Insert(0, new SelectListItem("--- Nouveau ---", string.Empty));

            return prescripteurs;
        }

        private List<SelectListItem> SelectUtilisateurList(Guid? selectionId = null)
        {
            var users = _usermanager
                .GetUsersInRoleAsync(EnumRoles.Caisse.ToString())
                .GetAwaiter()
                .GetResult()
                .Select(x => x.Id)
                .ToList();

            var utilisateurs = new List<SelectListItem>();

            if (users.Any())
            {
                utilisateurs = _context.Utilisateurs.AsNoTracking()
                    .Where(x => users.Contains(x.Userid))
                    .Select(x => new SelectListItem
                    {
                        Selected = selectionId != null && selectionId != Guid.Empty ? x.Utilisateurid == selectionId : false,
                        Value = x.Utilisateurid.ToString(),
                        Text = string.Join(", ", new List<object> {
                            $"{x.Nom} {x.Prenom}",
                        })
                    })
                    .ToList();
                utilisateurs.Insert(0, new SelectListItem("---", string.Empty));
            }

            return utilisateurs;
        }

        private List<SelectListItem> SelectCategorieList(Guid[]? selectionIds = null)
        {
            var frCulture = CultureInfo.GetCultureInfo("fr-FR");
            var categories = _context.Categories.AsNoTracking()
                .Select(x => new SelectListItem
                {
                    Selected = selectionIds != null && selectionIds.Length > 0 ? selectionIds.Contains(x.Categorieid) : false,
                    Value = x.Categorieid.ToString(),
                    Text = string.Join(", ", new List<object> {
                        $"{x.Nom} ({x.Prix.ToString("N0", frCulture)})"
                    })
                })
                .ToList();
            categories.Insert(0, new SelectListItem("---", string.Empty));

            return categories;
        }

        private List<SelectListItem> SelectAnalyseList(Guid[]? selectionIds = null)
        {
            var frCulture = CultureInfo.GetCultureInfo("fr-FR");
            var analyses = _context.Analyses.AsNoTracking()
                .Select(x => new SelectListItem
                {
                    Selected = selectionIds != null && selectionIds.Length > 0 ? selectionIds.Contains(x.Idanalyse) : false,
                    Value = x.Idanalyse.ToString(),
                    Text = string.Join(", ", new List<object> {
                        $"{x.Nom} ({x.Prix.ToString("N0", frCulture)})"
                    })
                })
                .ToList();
            analyses.Insert(0, new SelectListItem("---", string.Empty));

            return analyses;

        }

        private async Task PopulateViewDataForCreate(EntetedemandeCreateVM model)
        {
            var sites = new SelectList(_context.Sites, "Codesite", "Name", model.Codesite).ToList();
            sites.Insert(0, new SelectListItem("---", string.Empty));

            var utilisateurs = SelectUtilisateurList(model.Utilisateurid);

            var sexes = new SelectList(_context.Sexes, "Codesexe", "Value", model.Patient.Codesexe).ToList();
            sexes.Insert(0, new SelectListItem("---", string.Empty));

            var typepeaus = new SelectList(_context.Typepeaus, "Codetypepeau", "Nom", model.Patient.Codetypepeau).ToList();
            typepeaus.Insert(0, new SelectListItem("---", string.Empty));

            var typedocumentidentites = new SelectList(_context.Typedocumentidentites, "Codetypedocumentidentite", "Nom", model.Patient.Codetypedocumentidentite).ToList();
            typedocumentidentites.Insert(0, new SelectListItem("---", string.Empty));

            var patients = SelectPatientList(model.Patientid);

            var assurances = new SelectList(_context.Assurances, "Codeassurance", "Nom", model.Codeassurance).ToList();
            assurances.Insert(0, new SelectListItem("---", string.Empty));

            var policeassurances = new SelectList(_context.Policeassurances, "Policeassuranceid", "Libelle", model.Policeassuranceid).ToList();
            policeassurances.Insert(0, new SelectListItem("---", string.Empty));

            var prescripteurs = SelectPrescripteurList(model.Prescripteurid);

            var frCulture = CultureInfo.GetCultureInfo("fr-FR");

            // Create analyses list with prices
            var analysesList = _context.Analyses.AsNoTracking()
                .Select(a => new { a.Idanalyse, a.Nom, a.Prix })
                .ToList()
                .Select(a => new SelectListItem(
                    $"{a.Nom} ({a.Prix.ToString("N0", frCulture)})",
                    a.Idanalyse.ToString()
                ))
                .ToList();
            analysesList.Insert(0, new SelectListItem("---", string.Empty));

            // Create categories list with prices
            var categoriesList = _context.Categories.AsNoTracking()
                .Select(c => new { c.Categorieid, c.Nom, c.Prix })
                .ToList()
                .Select(c => new SelectListItem(
                    $"{c.Nom} ({c.Prix.ToString("N0", frCulture)})",
                    c.Categorieid.ToString()
                ))
                .ToList();
            categoriesList.Insert(0, new SelectListItem("---", string.Empty));

            var partenaires = new SelectList(_context.Partenaires, "Partenaireid", "Nom", model.Partenaireid).ToList();
            partenaires.Insert(0, new SelectListItem("---", string.Empty));
            ViewBag.Partenaireid = partenaires;

            ViewData["Codesite"] = sites;
            ViewData["Codesexe"] = sexes;
            ViewData["Codetypepeau"] = typepeaus;
            ViewData["Codetypedocumentidentite"] = typedocumentidentites;
            ViewData["Patientid"] = patients;
            ViewData["Codeassurance"] = assurances;
            ViewData["Policeassuranceid"] = policeassurances;
            ViewData["Prescripteurid"] = prescripteurs;
            ViewData["Idsanalyse"] = analysesList;
            ViewData["IdsCategorie"] = categoriesList;
            ViewData["Utilisateurid"] = utilisateurs;
        }

        // GET: Entetedemande/Create
        public async Task<IActionResult> Create()
        {
            // utilisateur connecté
            var user = await _usermanager.GetUserAsync(User);

            Utilisateur utilisateur = null;

            if (user != null)
            {
                utilisateur = await _context.Utilisateurs.AsNoTracking()
                    .Include(x => x.CodesiteNavigation)
                    .Where(x => x.Userid == user.Id)
                    .FirstOrDefaultAsync();
            }

            var sites = new SelectList(_context.Sites, "Codesite", "Name").ToList();
            sites.Insert(0, new SelectListItem("---", string.Empty));

            var utilisateurs = SelectUtilisateurList(utilisateur?.Utilisateurid);

            var sexes = new SelectList(_context.Sexes, "Codesexe", "Value").ToList();
            sexes.Insert(0, new SelectListItem("---", string.Empty));

            var typepeaus = new SelectList(_context.Typepeaus, "Codetypepeau", "Nom").ToList();
            typepeaus.Insert(0, new SelectListItem("---", string.Empty));

            var typedocumentidentites = new SelectList(_context.Typedocumentidentites, "Codetypedocumentidentite", "Nom").ToList();
            typedocumentidentites.Insert(0, new SelectListItem("---", string.Empty));

            var patients = SelectPatientList();

            var assurances = new SelectList(_context.Assurances, "Codeassurance", "Nom").ToList();
            assurances.Insert(0, new SelectListItem("---", string.Empty));

            var policeassurances = new SelectList(_context.Policeassurances, "Policeassuranceid", "Libelle").ToList();
            policeassurances.Insert(0, new SelectListItem("---", string.Empty));

            var prescripteurs = SelectPrescripteurList();

            var frCulture = CultureInfo.GetCultureInfo("fr-FR");

            // Create analyses list with prices
            var analyses = _context.Analyses.AsNoTracking()
                .Select(a => new { a.Idanalyse, a.Nom, a.Prix })
                .ToList()
                .Select(a => new SelectListItem(
                    $"{a.Nom} ({a.Prix.ToString("N0", frCulture)})",
                    a.Idanalyse.ToString()
                ))
                .ToList();
            analyses.Insert(0, new SelectListItem("---", string.Empty));

            // Create categories list with prices
            var categories = _context.Categories.AsNoTracking()
                .Select(c => new { c.Categorieid, c.Nom, c.Prix })
                .ToList()
                .Select(c => new SelectListItem(
                    $"{c.Nom} ({c.Prix.ToString("N0", frCulture)})",
                    c.Categorieid.ToString()
                ))
                .ToList();
            categories.Insert(0, new SelectListItem("---", string.Empty));

            var partenaires = new SelectList(_context.Partenaires, "Partenaireid", "Nom").ToList();
            partenaires.Insert(0, new SelectListItem("---", string.Empty));
            ViewBag.Partenaireid = partenaires;

            ViewData["Codesite"] = sites;
            ViewData["Codesexe"] = sexes;
            ViewData["Codetypepeau"] = typepeaus;
            ViewData["Codetypedocumentidentite"] = typedocumentidentites;
            ViewData["Patientid"] = patients;
            ViewData["Codeassurance"] = assurances;
            ViewData["Policeassuranceid"] = policeassurances;
            ViewData["Prescripteurid"] = prescripteurs;
            ViewData["IdsAnalyse"] = analyses;
            ViewData["IdsCategorie"] = categories;
            ViewData["Utilisateurid"] = utilisateurs;

            var model = new EntetedemandeCreateVM
            {
                Codesite = utilisateur?.Codesite ?? string.Empty,
                Utilisateurid = utilisateur?.Utilisateurid ?? Guid.Empty,
                Patient = new EnteteDemandePatientVM
                {
                    Code = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()
                }
            };

            return View(model);
        }

        // POST: Entetedemande/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EntetedemandeCreateVM model)
        {
            //valider utilisateur

            bool isUtilisateurValid = true;

            Utilisateur utilisateur = null;
            //Utilisateurprofil utilisateurprofil = null;
            Affectationcaisse affectationcaisse = null;

            if (model.Utilisateurid != null && model.Utilisateurid != Guid.Empty)
            {
                //utilisateurprofil = await _context.Utilisateurprofils.AsNoTracking()
                //    .Include(x => x.Utilisateur)
                //    .Include(x => x.Profil)
                //    .Where(x => x.Utilisateurid == model.Utilisateurid
                //        && x.Profil.Nom == EnumProfil.caissier.ToString()
                //        )
                //    .FirstOrDefaultAsync();

                //if (utilisateurprofil == null)
                //{
                //    ModelState.AddModelError("Utilisateurid", "Utilisateur non valide.");
                //    isUtilisateurValid = false;
                //}

                utilisateur = await _context.Utilisateurs.AsNoTracking()
                    .Where(x => x.Utilisateurid == model.Utilisateurid)
                    .FirstOrDefaultAsync();

                if (utilisateur == null)
                {
                    ModelState.AddModelError("Utilisateurid", "Utilisateur non valide.");
                    isUtilisateurValid = false;
                }
                else
                {
                    affectationcaisse = await _context.Affectationcaisses.AsNoTracking()
                        .Include(x => x.Utilisateur)
                        .Include(x => x.Caisse)
                        .Where(x => x.Utilisateurid == model.Utilisateurid)
                        .FirstOrDefaultAsync();

                    if (affectationcaisse == null)
                    {
                        ModelState.AddModelError("Utilisateurid", "Aucune affectation de caisse.");
                        isUtilisateurValid = false;
                    }
                }
            }

            //valider analyse

            bool isAnalyseValid = true;

            if (model.IdsCategorie.Count() == 0 
                && model.IdsAnalyse.Count() == 0
                )
            {
                ModelState.AddModelError("IdsCategorie", "Sélection vide.");
                ModelState.AddModelError("IdsAnalyse", "Sélection vide.");
                isAnalyseValid = false;
            }

            //valider assurance

            bool isAssuranceValid = true;

            if (model.EstAssure)
            {
                if (string.IsNullOrWhiteSpace(model.Codeassurance))
                {
                    ModelState.AddModelError("Codeassurance", "Sélection vide.");
                    isAssuranceValid = false;
                }
                if (model.Policeassuranceid == null || model.Policeassuranceid == Guid.Empty)
                {
                    ModelState.AddModelError("Policeassuranceid", "Sélection vide.");
                    isAssuranceValid = false;
                }
            }

            //valider nouveau patient

            bool isPatientValid = true;

            if (model.Patientid == null || model.Patientid == Guid.Empty)
            {
                if (model.Patient.Datenaissance.HasValue)
                {
                    model.Patient.Age = Utilities.CalculerAgeEnAnnee(model.Patient.Datenaissance.Value);
                    ModelState.Remove("Patient.Age");
                }

                if (!model.Patient.Datenaissance.HasValue && model.Patient.Age <= 0)
                {
                    ModelState.AddModelError("Patient.Age", "L'âge est obligatoire si la date de naissance n'est pas renseignée.");
                    isPatientValid = false;
                }

                ValidationContext vc = new ValidationContext(model.Patient);
                ICollection<ValidationResult> vcResults = new List<ValidationResult>();
                bool patientAnnotationsValid = Validator.TryValidateObject(model.Patient, vc, vcResults, true);
                if (!patientAnnotationsValid)
                {
                    foreach (var result in vcResults)
                    {
                        var memberName = result.MemberNames.FirstOrDefault() ?? string.Empty;
                        ModelState.AddModelError(
                            string.IsNullOrEmpty(memberName) ? "" : $"Patient.{memberName}",
                            result.ErrorMessage ?? "Champ invalide.");
                    }
                }
                isPatientValid = isPatientValid && patientAnnotationsValid;
            }

            //valider nouveau prescripteur

            bool isPrescripteurValid = true;

            if (model.Prescripteurid == null || model.Prescripteurid == Guid.Empty)
            {
                ValidationContext vc = new ValidationContext(model.Prescripteur);
                ICollection<ValidationResult> vcResults = new List<ValidationResult>();
                bool prescripteurAnnotationsValid = Validator.TryValidateObject(model.Prescripteur, vc, vcResults, true);
                if (!prescripteurAnnotationsValid)
                {
                    foreach (var result in vcResults)
                    {
                        var memberName = result.MemberNames.FirstOrDefault() ?? string.Empty;
                        ModelState.AddModelError(
                            string.IsNullOrEmpty(memberName) ? "" : $"Prescripteur.{memberName}",
                            result.ErrorMessage ?? "Champ invalide.");
                    }
                }
                isPrescripteurValid = prescripteurAnnotationsValid;
            }

            var ok = isUtilisateurValid && isAnalyseValid && isAssuranceValid && isPatientValid && isPrescripteurValid;

            if (ok)
            {
                try
                {
                    var patient = _mapper.Map<Patient>(model.Patient);
                    patient.Datenaissance = model.Patient.Datenaissance ?? DateTime.MinValue;

                    if (string.IsNullOrWhiteSpace(patient.Codetypedocumentidentite))
                    {
                        var fallbackTypeDocument = await _context.Typedocumentidentites.AsNoTracking()
                            .Select(x => x.Codetypedocumentidentite)
                            .FirstOrDefaultAsync();

                        if (string.IsNullOrWhiteSpace(fallbackTypeDocument))
                        {
                            ModelState.AddModelError("Patient.Codetypedocumentidentite", "Aucun type de document disponible.");
                            await PopulateViewDataForCreate(model);
                            return View(model);
                        }

                        patient.Codetypedocumentidentite = fallbackTypeDocument;
                    }

                    var prescripteur = _mapper.Map<Prescripteur>(model.Prescripteur);

                    if (string.IsNullOrWhiteSpace(model.Partenaireid.ToString()) || model.Partenaireid == Guid.Empty)
                    {
                        model.Partenaireid = null;
                    }

                    Policeassurance policeassurance = null;

                    if (model.Policeassuranceid != null && model.Policeassuranceid != Guid.Empty)
                    {
                        policeassurance = await _context.Policeassurances.AsNoTracking()
                            .Include(x => x.CodeassuranceNavigation)
                            .Where(x => x.Policeassuranceid == model.Policeassuranceid)
                            .FirstOrDefaultAsync();
                    }

                    if (model.Patientid != null && model.Patientid != Guid.Empty)
                    {
                        //patient selectionne

                        patient.Patientid = model.Patientid.Value;
                    }
                    else
                    {
                        //creer patient

                        patient.Patientid = Guid.NewGuid();
                        patient.Codesite = model.Codesite;

                        // Auto-generate patient code if not set by client
                        if (string.IsNullOrWhiteSpace(patient.Code))
                        {
                            patient.Code = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
                        }

                        // Remplacer null par "" pour les colonnes NOT NULL dans la DB
                        patient.Lieunaissance = patient.Lieunaissance ?? string.Empty;
                        patient.Nomusage = patient.Nomusage ?? string.Empty;
                        patient.Numerodocumentidentite = patient.Numerodocumentidentite ?? string.Empty;
                        patient.Tel = patient.Tel ?? string.Empty;
                        patient.Adresse = patient.Adresse ?? string.Empty;
                        patient.Email = patient.Email ?? string.Empty;
                        patient.Renseignementclinique = patient.Renseignementclinique ?? string.Empty;

                        _context.Add(patient);

                        var file = model.Patient.File;
                        var filename = model.Patient.Filename;

                        if (!string.IsNullOrWhiteSpace(filename) && file != null && file.Length > 0)
                        {
                            using (var ms = new MemoryStream())
                            {
                                await file.CopyToAsync(ms);
                                var fileBytes = ms.ToArray();

                                var photoPatient = new Photopatient
                                {
                                    Photopatientid = Guid.NewGuid(),
                                    Patientid = patient.Patientid,
                                    Photo = fileBytes,
                                    Extension = System.IO.Path.GetExtension(file.FileName).TrimStart('.').ToLower()
                                };
                                _context.Add(photoPatient);
                            }
                        }
                    }

                    if (model.Prescripteurid != null && model.Prescripteurid != Guid.Empty)
                    {
                        //prescripteur selectionne

                        prescripteur.Prescripteurid = model.Prescripteurid.Value;
                    }
                    else
                    {
                        //creer prescripteur

                        prescripteur.Prescripteurid = Guid.NewGuid();

                        _context.Add(prescripteur);
                    }

                    //creer demande

                    var entetedemande = new Entetedemande
                    {
                        Entetedemandeid = Guid.NewGuid(),
                        Partenaireid = model.Partenaireid,
                        Patientid = patient.Patientid,
                        Prescripteurid = prescripteur.Prescripteurid,
                        Policeassuranceid = model.Policeassuranceid,
                        Utilisateurid = model.Utilisateurid,
                        Codesite = model.Codesite,
                        Date = DateTime.Now,
                        Numero = $"---{DateTime.Now.ToString("yyyy-MM-dd:HH:mm:ss:")}---"
                    };

                    _context.Add(entetedemande);

                    //creer detail demande

                    var detaildemandeToCreate = new List<Detaildemande>();

                    if (model.IdsCategorie != null && model.IdsCategorie.Length > 0)
                    {
                        var listcategorie = await _context.Categories.AsNoTracking()
                            .Where(x => model.IdsCategorie.Contains(x.Categorieid))
                            .ToListAsync();

                        var listtarif = policeassurance != null
                            ? await _context.Tarifcategorieassurances.AsNoTracking()
                                .Where(x => model.IdsCategorie.Contains(x.Categorieid)
                                    && x.Codeassurance == policeassurance.Codeassurance)
                                .ToListAsync()
                            : new List<Tarifcategorieassurance>();

                        foreach (var item in model.IdsCategorie)
                        {
                            var prix = listcategorie.Where(x => x.Categorieid == item).Select(x => x.Prix).FirstOrDefault();
                            prix = prix != null ? prix : 0;

                            var tarif = listtarif.Where(x => x.Categorieid == item).Select(x => x.Prix).FirstOrDefault();
                            tarif = tarif != null ? tarif : 0;

                            var calculs = Utilities.CalculerDifferentPrixDetailDemande(policeassurance, prix, tarif);

                            var obj = new Detaildemande
                            {
                                Detaildemandeid = Guid.NewGuid(),
                                Entetedemandeid = entetedemande.Entetedemandeid,
                                Categorieid = item,
                                Idanalyse = null,
                                Prix = calculs.Item1,
                                Partassurance = calculs.Item2,
                                Partpatient = calculs.Item3,
                                Complement = calculs.Item4,
                                Net = calculs.Item5
                            };

                            detaildemandeToCreate.Add(obj);
                        }
                    }

                    if (model.IdsAnalyse != null && model.IdsAnalyse.Length > 0)
                    {
                        var listanalyse = await _context.Analyses.AsNoTracking()
                        .Where(x => model.IdsAnalyse.Contains(x.Idanalyse))
                        .ToListAsync();

                        var listtarif = policeassurance != null
                            ? await _context.Tarifanalyseassurances.AsNoTracking()
                                .Where(x => model.IdsAnalyse.Contains(x.Idanalyse)
                                    && x.Codeassurance == policeassurance.Codeassurance)
                                .ToListAsync()
                            : new List<Tarifanalyseassurance>();

                        foreach (var item in model.IdsAnalyse)
                        {
                            var prix = listanalyse.Where(x => x.Idanalyse == item).Select(x => x.Prix).FirstOrDefault();
                            prix = prix != null ? prix : 0;

                            var tarif = listtarif.Where(x => x.Idanalyse == item).Select(x => x.Prix).FirstOrDefault();
                            tarif = tarif != null ? tarif : 0;

                            var calculs = Utilities.CalculerDifferentPrixDetailDemande(policeassurance, prix, tarif);

                            var obj = new Detaildemande
                            {
                                Detaildemandeid = Guid.NewGuid(),
                                Entetedemandeid = entetedemande.Entetedemandeid,
                                Categorieid = null,
                                Idanalyse = item,
                                Prix = calculs.Item1,
                                Partassurance = calculs.Item2,
                                Partpatient = calculs.Item3,
                                Complement = calculs.Item4,
                                Net = calculs.Item5
                            };

                            detaildemandeToCreate.Add(obj);
                        }
                    }

                    if (detaildemandeToCreate.Any())
                    {
                        _context.AddRange(detaildemandeToCreate);
                    }

                    //save

                    await _context.SaveChangesAsync();

                    //update numero demande

                    entetedemande.Numero = Utilities.FormatNumeroRecu(
                        utilisateur.Idinh, 
                        entetedemande.Ordre
                        );

                    _context.Update(entetedemande);

                    //creer facture 

                    var entetefacture = new Entetefacture
                    {
                        Entetefactureid = Guid.NewGuid(),
                        Entetedemandeid = entetedemande.Entetedemandeid,
                        Utilisateurid = model.Utilisateurid,
                        Caisseid = affectationcaisse.Caisseid,
                        Date = entetedemande.Date,
                        Numero = entetedemande.Numero
                    };

                    _context.Add(entetefacture);

                    //creer detail facture

                    var detailfactureCreate = detaildemandeToCreate
                        .Select(x => new Detailfacture
                        {
                            Detailfactureid = Guid.NewGuid(),
                            Entetefactureid = entetefacture.Entetefactureid,
                            Detaildemandeid = x.Detaildemandeid
                        })
                        .ToList();

                    if (detailfactureCreate.Any())
                    {
                        _context.AddRange(detailfactureCreate);
                    }

                    //save

                    await _context.SaveChangesAsync();

                    // Redirect to the invoice/report page after successful creation
                    return RedirectToAction("Facture", new { id = entetedemande.Entetedemandeid });
                }
                catch (Exception ex)
                {
                    // Log the full exception chain for diagnosis
                    var innerMsg = ex.InnerException?.InnerException?.Message
                                ?? ex.InnerException?.Message
                                ?? "(aucun détail)";
                    _logger.LogError(ex, "Erreur lors de la création de la demande");
                    ModelState.AddModelError("", $"Une erreur s'est produite lors de l'enregistrement: {ex.Message} | Détail: {innerMsg}");
                    
                    // Re-populate ViewData for the form
                    await PopulateViewDataForCreate(model);
                    return View(model);
                }
            }

            var sites = new SelectList(_context.Sites, "Codesite", "Name", model.Codesite).ToList();
            sites.Insert(0, new SelectListItem("---", string.Empty));

            var utilisateurs = SelectUtilisateurList(model.Utilisateurid);

            var sexes = new SelectList(_context.Sexes, "Codesexe", "Value", model.Patient.Codesexe).ToList();
            sexes.Insert(0, new SelectListItem("---", string.Empty));

            var typepeaus = new SelectList(_context.Typepeaus, "Codetypepeau", "Nom", model.Patient.Codetypepeau).ToList();
            typepeaus.Insert(0, new SelectListItem("---", string.Empty));

            var typedocumentidentites = new SelectList(_context.Typedocumentidentites, "Codetypedocumentidentite", "Nom", model.Patient.Codetypedocumentidentite).ToList();
            typedocumentidentites.Insert(0, new SelectListItem("---", string.Empty));

            var patients = SelectPatientList(model.Patientid);

            var assurances = new SelectList(_context.Assurances, "Codeassurance", "Nom", model.Codeassurance).ToList();
            assurances.Insert(0, new SelectListItem("---", string.Empty));

            var policeassurances = new SelectList(_context.Policeassurances, "Policeassuranceid", "Libelle", model.Policeassuranceid).ToList();
            policeassurances.Insert(0, new SelectListItem("---", string.Empty));

            var prescripteurs = SelectPrescripteurList(model.Prescripteurid);
            var frCulture = CultureInfo.GetCultureInfo("fr-FR");

            // Create analyses list with prices
            var analysesList = _context.Analyses.AsNoTracking()
                .Select(a => new { a.Idanalyse, a.Nom, a.Prix })
                .ToList()
                .Select(a => new SelectListItem(
                    $"{a.Nom} ({a.Prix.ToString("N0", frCulture)})",
                    a.Idanalyse.ToString(),
                    model.IdsAnalyse != null && model.IdsAnalyse.Contains(a.Idanalyse)
                ))
                .ToList();
            analysesList.Insert(0, new SelectListItem("---", string.Empty));
            var analyses = analysesList;

            // Create categories list with prices
            var categoriesList = _context.Categories.AsNoTracking()
                .Select(c => new { c.Categorieid, c.Nom, c.Prix })
                .ToList()
                .Select(c => new SelectListItem(
                    $"{c.Nom} ({c.Prix.ToString("N0", frCulture)})",
                    c.Categorieid.ToString(),
                    model.IdsCategorie != null && model.IdsCategorie.Contains(c.Categorieid)
                ))
                .ToList();
            categoriesList.Insert(0, new SelectListItem("---", string.Empty));
            var categories = categoriesList;

            var partenaires = new SelectList(_context.Partenaires, "Partenaireid", "Nom", model.Partenaireid).ToList();
            partenaires.Insert(0, new SelectListItem("---", string.Empty));
            ViewBag.Partenaireid = partenaires;

            ViewData["Codesite"] = sites;
            ViewData["Codesexe"] = sexes;
            ViewData["Codetypepeau"] = typepeaus;
            ViewData["Codetypedocumentidentite"] = typedocumentidentites;
            ViewData["Patientid"] = patients;
            ViewData["Codeassurance"] = assurances;
            ViewData["Policeassuranceid"] = policeassurances;
            ViewData["Prescripteurid"] = prescripteurs;
            ViewData["IdsAnalyse"] = analyses;
            ViewData["IdsCategorie"] = categories;
            ViewData["Utilisateurid"] = utilisateurs;

            // Return the view with validation errors for traditional form submission
            return View(model);
        }

        //private Cell CreateCell(string text, PdfFont font, float fontSize, TextAlignment alignment, float? fixedWidth = null)
        //{
        //    Paragraph paragraph = new Paragraph(text)
        //        .SetFont(font)
        //        .SetFontSize(fontSize)
        //        .SetTextAlignment(alignment);

        //    Cell cell = new Cell().Add(paragraph)
        //        .SetBorder(iText.Layout.Borders.Border.NO_BORDER);

        //    if (fixedWidth.HasValue)
        //    {
        //        cell.SetWidth(fixedWidth.Value);
        //    }

        //    return cell;
        //}

        private Cell CreateCell(string text, PdfFont font, float fontSize, TextAlignment alignment, float? fixedWidth = null, VerticalAlignment verticalAlignment = VerticalAlignment.MIDDLE)
        {
            text ??= "";

            Paragraph paragraph = new Paragraph(text)
                .SetFont(font)
                .SetFontSize(fontSize)
                .SetTextAlignment(alignment)
                .SetMargin(0); // Remove paragraph margins

            Cell cell = new Cell()
                .Add(paragraph)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetVerticalAlignment(verticalAlignment)
                .SetPadding(0); // No padding in cells

            if (fixedWidth.HasValue)
            {
                cell.SetWidth(fixedWidth.Value);
            }

            return cell;
        }

        public static float CmToPt(float cm)
        {
            return cm * 28.3465f;
        }

        /// <summary>
        /// Génère un reçu avec un indicateur spécifique (PATIENT ou ASSURANCE)
        /// </summary>
        private void GenerateReceiptWithIndicator(
            Document document, 
            Entetefacture entetefacture, 
            List<Detaildemande> detaildemandes,
            string indicateur,
            PdfFont fontNormal,
            PdfFont fontBold,
            PdfFont himalayaFontNormal,
            float cm,
            string siteName = "Site")
        {
            var cultureFr = new CultureInfo("fr-FR");
            
            var total = detaildemandes.Sum(x => x.Prix);
            var partassurance = detaildemandes.Sum(x => x.Partassurance);
            var partpatient = detaildemandes.Sum(x => x.Partpatient);
            var complement = detaildemandes.Sum(x => x.Complement);
            var net = detaildemandes.Sum(x => x.Net);

            var recu = entetefacture.Numero;
            var idinh = entetefacture.Utilisateur.Idinh;
            var datefacture = entetefacture.Date.ToString("dd/MM/yyyy");
            var net_str = ((int)net).ToString("N0", cultureFr).Replace(",", " ");
            var nom = entetefacture.Entetedemande.Patient.Nom;
            var prenom = entetefacture.Entetedemande.Patient.Prenom;
            var age = Utilities.CalculerAgeEnAnneeMoisJour(entetefacture.Entetedemande.Patient.Datenaissance);
            var sexe = entetefacture.Entetedemande.Patient.CodesexeNavigation.Value;
            var tel = entetefacture.Entetedemande.Patient.Tel;
            var prescripteurnom = entetefacture.Entetedemande.Prescripteur.Nom;
            var somme = $"{((int)net).ToWords(cultureFr)} Francs CFA";
            var dateimpression = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            var total_str = "";
            var partassurance_str = "";
            var partpatient_str = "";
            var complement_str = "";

            if (entetefacture.Entetedemande.Policeassurance != null)
            {
                total_str = ((int)total).ToString("N0", cultureFr);
                partassurance_str = ((int)partassurance).ToString("N0", cultureFr);
                partpatient_str = ((int)partpatient).ToString("N0", cultureFr);
                complement_str = ((int)complement).ToString("N0", cultureFr);
            }

            // Calcul des analyses pour affichage
            var idcategories = detaildemandes
                .Where(x => x.Categorieid != null)
                .Select(x => x.Categorieid)
                .Distinct();

            var idanalyses = detaildemandes
                .Where(x => x.Idanalyse != null)
                .Select(x => x.Idanalyse)
                .Distinct();

            var categorienometprix = _context.Categories.AsNoTracking()
                .Where(x => idcategories.Contains(x.Categorieid))
                .Select(x => $"{x.Nom} {((int)x.Prix).ToString("N0", cultureFr).Replace(",", " ")}")
                .ToList();

            var analysenometprix = _context.Analyses.AsNoTracking()
                .Where(x => idanalyses.Contains(x.Idanalyse))
                .Select(x => $"{x.Nom} {((int)x.Prix).ToString("N0", cultureFr).Replace(",", " ")}")
                .ToList();

            // Combiner les listes en évitant la virgule au début
            var toutesAnalyses = categorienometprix.Concat(analysenometprix).ToList();
            var categorieetanalyse = string.Join(", ", toutesAnalyses);

            // --- ENTETE ---
            Table headerTable = new Table(new float[] { 9 * cm, 1, 5.25f * cm });
            headerTable.SetWidth(UnitValue.CreatePercentValue(100));

            // Colonne 1: Table 3 lignes
            Table col1Table = new Table(1);
            Paragraph p1 = new Paragraph("MINISTERE DE LA SANTE, DE L'HYGIENE PUBLIQUE ET DE L'ACCES UNIVERSEL AUX SOINS")
                .SetFont(himalayaFontNormal)
                .SetFontSize(7)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetMargin(0);
            col1Table.AddCell(new Cell().Add(p1).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetHeight(0.5f * cm));

            string imgPath = System.IO.Path.Combine(_webHostEnvironment.WebRootPath, "img", "inh_togo_logo.png");
            Image logo = new Image(ImageDataFactory.Create(imgPath))
                .ScaleAbsolute(1.5f * cm, 1f * cm)
                .SetHorizontalAlignment(HorizontalAlignment.LEFT);
            col1Table.AddCell(new Cell().Add(logo).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetHeight(1.2f * cm));

            Paragraph p3 = new Paragraph("BP : 1396 Tél : 22 21 06 33 LOME-TOGO")
                .SetFont(fontNormal)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetMargin(0);
            col1Table.AddCell(new Cell().Add(p3).SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetHeight(0.4f * cm));

            headerTable.AddCell(new Cell().Add(col1Table).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
            headerTable.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));

            // Colonne 3: République Togolaise
            Table col3Table = new Table(1).SetWidth(5.25f * cm);
            col3Table.AddCell(new Cell().Add(new Paragraph("REPUBLIQUE TOGOLAISE")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFont(fontNormal)
                .SetFontSize(8))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
            col3Table.AddCell(new Cell().Add(new Paragraph("Travail - Liberté - Patrie")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFont(fontNormal)
                .SetFontSize(8))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
            // Troisième ligne vide (pas d'indicateur)
            col3Table.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));
            
            headerTable.AddCell(new Cell().Add(col3Table).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
            document.Add(headerTable);

            if (entetefacture.Entetedemande.Partenaire != null)
            {
                document.Add(new Paragraph("").SetHeight(CmToPt(0.25f)));
                var table = new Table(1).SetWidth(CmToPt(19));
                var cell = CreateCell(
                    "BON PROVISOIRE POUR RETRAIT DES RESULTATS",
                    fontNormal,
                    16,
                    TextAlignment.CENTER,
                    null,
                    VerticalAlignment.MIDDLE
                );
                table.AddCell(cell);
                document.Add(table);
                document.Add(new Paragraph("").SetHeight(CmToPt(0.25f)));
            }

            // --- BODY ---
            Table mainTable = new Table(new float[] { 1, 1 });
            mainTable.SetWidth(UnitValue.CreatePercentValue(100));

            Table colPatientTable = new Table(new float[] { 1 });
            colPatientTable.SetWidth(UnitValue.CreatePercentValue(100));

            Table col1Main = new Table(4);
            col1Main.SetWidth(UnitValue.CreatePercentValue(100));

            col1Main.AddCell(CreateCell("Patient :", fontBold, 8, TextAlignment.LEFT, 1.75f * cm));
            col1Main.AddCell(CreateCell($"{nom} {prenom}", fontNormal, 8, TextAlignment.LEFT, 4.5f * cm));
            col1Main.AddCell(CreateCell("N° Reçu :", fontBold, 8, TextAlignment.LEFT, 2.5f * cm));
            col1Main.AddCell(CreateCell(recu, fontNormal, 8, TextAlignment.LEFT, 4f * cm));

            Table col2Main = new Table(5);
            col2Main.SetWidth(UnitValue.CreatePercentValue(100));
            col2Main.AddCell(CreateCell("Age :", fontBold, 8, TextAlignment.LEFT, 1.25f * cm));
            col2Main.AddCell(CreateCell(age, fontNormal, 8, TextAlignment.LEFT, 5f * cm));
            col2Main.AddCell(CreateCell("Date :", fontBold, 8, TextAlignment.LEFT, 2f * cm));
            col2Main.AddCell(CreateCell(datefacture, fontNormal, 8, TextAlignment.LEFT, 4.5f * cm));
            col2Main.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));

            Table col3Main = new Table(3);
            col3Main.SetWidth(UnitValue.CreatePercentValue(100));
            col3Main.AddCell(CreateCell("Sexe :", fontBold, 8, TextAlignment.LEFT, 1.5f * cm));
            col3Main.AddCell(CreateCell(sexe, fontNormal, 8, TextAlignment.LEFT, 4.75f * cm));
            col3Main.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));

            Table col4Main = new Table(3);
            col4Main.SetWidth(UnitValue.CreatePercentValue(100));
            col4Main.AddCell(CreateCell("Tél. :", fontBold, 8, TextAlignment.LEFT, 1.25f * cm));
            col4Main.AddCell(CreateCell(tel, fontNormal, 8, TextAlignment.LEFT, 5f * cm));
            col4Main.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));

            colPatientTable.AddCell(new Cell().Add(col1Main).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
            colPatientTable.AddCell(new Cell().Add(col2Main).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
            colPatientTable.AddCell(new Cell().Add(col3Main).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
            colPatientTable.AddCell(new Cell().Add(col4Main).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

            mainTable.AddCell(new Cell().Add(colPatientTable).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

            // Colonne 2: table totaux
            Table colTotalTable = new Table(new float[] { 3 * cm, 2.75f * cm });
            colTotalTable.SetWidth(UnitValue.CreatePercentValue(100));

            if (!string.IsNullOrWhiteSpace(total_str))
            {
                colTotalTable.AddCell(CreateCell("Base :", fontBold, 8, TextAlignment.LEFT));
                colTotalTable.AddCell(CreateCell(total_str, fontNormal, 8, TextAlignment.LEFT));
            }
            if (!string.IsNullOrWhiteSpace(partassurance_str))
            {
                colTotalTable.AddCell(CreateCell("Part Assurance :", fontBold, 8, TextAlignment.LEFT));
                colTotalTable.AddCell(CreateCell(partassurance_str, fontNormal, 8, TextAlignment.LEFT));
            }
            if (!string.IsNullOrWhiteSpace(partpatient_str))
            {
                colTotalTable.AddCell(CreateCell("Part Patient :", fontBold, 8, TextAlignment.LEFT));
                colTotalTable.AddCell(CreateCell(partpatient_str, fontNormal, 8, TextAlignment.LEFT));
            }
            if (!string.IsNullOrWhiteSpace(complement_str))
            {
                colTotalTable.AddCell(CreateCell("Complément :", fontBold, 8, TextAlignment.LEFT));
                colTotalTable.AddCell(CreateCell(complement_str, fontNormal, 8, TextAlignment.LEFT));
            }
            if (!string.IsNullOrWhiteSpace(net_str))
            {
                colTotalTable.AddCell(CreateCell("Net à payer :", fontBold, 8, TextAlignment.LEFT));
                colTotalTable.AddCell(CreateCell(net_str, fontNormal, 8, TextAlignment.LEFT));
            }

            mainTable.AddCell(new Cell().Add(colTotalTable).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
            document.Add(mainTable);

            // Espace ultra-minimal
            document.Add(new Paragraph("").SetHeight(0.01f * cm));

            // Prescripteur
            Table tableDe = new Table(2);
            tableDe.SetWidth(UnitValue.CreatePercentValue(100));
            tableDe.AddCell(CreateCell("De Mr/Mme :", fontBold, 8, TextAlignment.LEFT, 2.5f * cm));
            tableDe.AddCell(CreateCell(prescripteurnom, fontNormal, 8, TextAlignment.LEFT));
            document.Add(tableDe);

            // Somme
            Table tableSomme = new Table(2);
            tableSomme.SetWidth(UnitValue.CreatePercentValue(100));
            tableSomme.AddCell(CreateCell("La somme de :", fontBold, 8, TextAlignment.LEFT, 3f * cm));
            tableSomme.AddCell(CreateCell(somme, fontNormal, 8, TextAlignment.LEFT));
            document.Add(tableSomme);

            // Pour
            Table tablePour = new Table(2);
            tablePour.SetWidth(UnitValue.CreatePercentValue(100));
            tablePour.AddCell(CreateCell("Pour :", fontBold, 8, TextAlignment.LEFT, 1.5f * cm));
            tablePour.AddCell(CreateCell(categorieetanalyse, fontNormal, 8, TextAlignment.LEFT));
            document.Add(tablePour);

            // Espace ultra-minimal
            document.Add(new Paragraph("").SetHeight(0.01f * cm));

            // Footer
            Table footTable = new Table(new float[] { 9 * cm, 1, 5.25f * cm });
            footTable.SetWidth(UnitValue.CreatePercentValue(100));

            Table footCol1 = new Table(1);
            footCol1.AddCell(CreateCell("NB: Analyse(s) à effectuer dans un délai d'un mois.", fontBold, 8, TextAlignment.LEFT));

            Table footCol1Inner = new Table(new float[] { 1.5f * cm, 7.5f * cm });
            footCol1Inner.AddCell(CreateCell("Fait le :", fontBold, 8, TextAlignment.LEFT));
            footCol1Inner.AddCell(CreateCell(dateimpression, fontNormal, 8, TextAlignment.LEFT));
            footCol1.AddCell(new Cell().Add(footCol1Inner).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
            
            // Add site and date to footer
            Table footCol1SiteInfo = new Table(new float[] { 1.5f * cm, 7.5f * cm });
            footCol1SiteInfo.AddCell(CreateCell("Site :", fontBold, 8, TextAlignment.LEFT));
            footCol1SiteInfo.AddCell(CreateCell(siteName, fontNormal, 8, TextAlignment.LEFT));
            footCol1.AddCell(new Cell().Add(footCol1SiteInfo).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

            footTable.AddCell(new Cell().Add(footCol1).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
            footTable.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));

            Paragraph sign = new Paragraph("Signature du caissier")
                .SetFont(fontBold)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);
            footTable.AddCell(new Cell().Add(sign).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

            document.Add(footTable);
        }

        [HttpGet]
        public async Task<IActionResult> Facture(Guid id)
        {
            try
            {
                var entetefacture = await _context.Entetefactures
                    .AsNoTracking()
                    .Include(x => x.Utilisateur)
                    .Include(x => x.Entetedemande)
                    .ThenInclude(x => x.Patient)
                    .ThenInclude(x => x.CodesexeNavigation)
                    .Include(x => x.Entetedemande.Prescripteur)
                    .Include(x => x.Entetedemande.Policeassurance)
                    .Include(x => x.Entetedemande.Partenaire)
                    .FirstOrDefaultAsync(e => e.Entetedemande.Entetedemandeid == id);

                if (entetefacture == null)
                {
                    return NotFound();
                }

                var detaildemandes = await _context.Detaildemandes.AsNoTracking()
                    .Where(x => x.Entetedemandeid == id)
                    .ToListAsync();

                var idcategories = detaildemandes
                    .Where(x => x.Categorieid != null)
                    .Select(x => x.Categorieid)
                    .Distinct();

                var idanalyses = detaildemandes
                    .Where(x => x.Idanalyse != null)
                    .Select(x => x.Idanalyse)
                    .Distinct();

                var categorienometprix = await _context.Categories.AsNoTracking()
                    .Where(x => idcategories.Contains(x.Categorieid))
                    .Select(x => $"{x.Nom} {x.Prix}")
                    .ToListAsync();

                var analysenometprix = await _context.Analyses.AsNoTracking()
                    .Where(x => idanalyses.Contains(x.Idanalyse))
                    .Select(x => $"{x.Nom} {x.Prix}")
                    .ToListAsync();

                var analysesDeCategories =
                    (from P in _context.Entetefactures
                     join P1 in _context.Entetedemandes on P.Entetedemandeid equals P1.Entetedemandeid
                     join P2 in _context.Detailfactures on P.Entetefactureid equals P2.Entetefactureid
                     join P3 in _context.Detaildemandes on P2.Detaildemandeid equals P3.Detaildemandeid
                     join P4 in _context.Categories on P3.Categorieid equals P4.Categorieid
                     join P5 in _context.Categorieanalyses on P4.Categorieid equals P5.Categorieid
                     join P6 in _context.Analyses on P5.Idanalyse equals P6.Idanalyse
                     join P7 in _context.Loboratoires on P6.Idlaboratoire equals P7.Idlaboratoire
                     where P.Entetefactureid == entetefacture.Entetefactureid
                     select new { 
                         P6.Idanalyse, 
                         P6.Nom, 
                         P6.Codification, 
                         P6.Indicederev, 
                         P7.Idlaboratoire,
                         Laboratoirenom = P7.Nom
                     });

                var analysesDirectes =
                    (from P in _context.Entetefactures
                     join P1 in _context.Entetedemandes on P.Entetedemandeid equals P1.Entetedemandeid
                     join P2 in _context.Detailfactures on P.Entetefactureid equals P2.Entetefactureid
                     join P3 in _context.Detaildemandes on P2.Detaildemandeid equals P3.Detaildemandeid
                     join P6 in _context.Analyses on P3.Idanalyse equals P6.Idanalyse
                     join P7 in _context.Loboratoires on P6.Idlaboratoire equals P7.Idlaboratoire
                     where P.Entetefactureid == entetefacture.Entetefactureid
                     select new { 
                         P6.Idanalyse, 
                         P6.Nom, 
                         P6.Codification, 
                         P6.Indicederev,
                         P7.Idlaboratoire,
                         Laboratoirenom = P7.Nom
                     });

                var toutesLesAnalyses = analysesDeCategories.Union(analysesDirectes)
                    .OrderBy(a => a.Idlaboratoire)
                    .ThenBy(a => a.Laboratoirenom)
                    .ToList();


                var toutesLesLaboratoires = toutesLesAnalyses
                    .Select(x => new { x.Idlaboratoire, x.Laboratoirenom })
                    .DistinctBy(x => x.Idlaboratoire)
                    .ToList();

                var categorieetanalyses = string.Join(", ", string.Join(", ", categorienometprix), string.Join(", ", analysenometprix));
                var total = detaildemandes.Sum(x => x.Prix);
                var partassurance = detaildemandes.Sum(x => x.Partassurance);
                var partpatient = detaildemandes.Sum(x => x.Partpatient);
                var complement = detaildemandes.Sum(x => x.Complement);
                var net = detaildemandes.Sum(x => x.Net);

                var logoPath = System.IO.Path.Combine(_webHostEnvironment.WebRootPath, "img", "inh_togo_logo.png");

                // Get site information for footer
                var site = await _context.Sites
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Codesite == entetefacture.Entetedemande.Codesite);
                var siteName = site?.Name ?? "Site";

                // Fonts
                var fontNormal = PdfFontFactory.CreateFont(GetFontPath(EnumFont.TimesNewRoman.ToString(), true, false), PdfEncodings.IDENTITY_H);
                var fontBold = PdfFontFactory.CreateFont(GetFontPath(EnumFont.TimesNewRoman.ToString(), false, true), PdfEncodings.IDENTITY_H);
                var himalayaFontNormal = PdfFontFactory.CreateFont(GetFontPath(EnumFont.Himalaya.ToString(), true, false), PdfEncodings.IDENTITY_H);

                byte[] pdfBytes;
                float marginTop = 0;
                float marginBottom = 0;
                float marginLeft = 0;
                float marginRight = 0;

                using (var stream = new MemoryStream())
                {
                    var cultureFr = new CultureInfo("fr-FR");

                    var writer = new PdfWriter(stream);
                    var pdf = new PdfDocument(writer);
                    
                    // Use full A4 page size (595pt x 842pt)
                    var document = new Document(pdf, PageSize.A4);
                    document.SetMargins(18, 18, 18, 18);

                    marginTop = document.GetTopMargin();
                    marginBottom = document.GetBottomMargin();
                    marginLeft = document.GetLeftMargin();
                    marginRight = document.GetRightMargin();

                    var recu = entetefacture.Numero;
                    var idinh = entetefacture.Utilisateur.Idinh;
                    var factureid = entetefacture.Entetefactureid.ToString();
                    var datefacture = entetefacture.Date.ToString("dd/MM/yyyy");
                    var net_str = ((int)net).ToString();
                    var nom = entetefacture.Entetedemande.Patient.Nom;
                    var prenom = entetefacture.Entetedemande.Patient.Prenom;
                    var datenaissance = entetefacture.Entetedemande.Patient.Datenaissance.ToString("dd/MM/yyyy");
                    var age = Utilities.CalculerAgeEnAnneeMoisJour(entetefacture.Entetedemande.Patient.Datenaissance);
                    var sexe = entetefacture.Entetedemande.Patient.CodesexeNavigation.Value;
                    var adresse = entetefacture.Entetedemande.Patient.Adresse;
                    var tel = entetefacture.Entetedemande.Patient.Tel;
                    var prescripteurnom = entetefacture.Entetedemande.Prescripteur.Nom;
                    var prescripteuradresse = entetefacture.Entetedemande.Prescripteur.Adresse;
                    var prescripteurtel = entetefacture.Entetedemande.Prescripteur.Tel;
                    var somme = $"{((int)net).ToWords(cultureFr)} Francs CFA";
                    var categorieetanalyse = categorieetanalyses;
                    var dateimpression = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

                    var total_str = "";
                    var partassurance_str = "";
                    var partpatient_str = "";
                    var complement_str = "";

                    if (entetefacture.Entetedemande.Policeassurance != null)
                    {
                        total_str = ((int)total).ToString("N0", cultureFr);
                        partassurance_str = ((int)partassurance).ToString("N0", cultureFr);
                        partpatient_str = ((int)partpatient).ToString("N0", cultureFr);
                        complement_str = ((int)complement).ToString("N0", cultureFr);
                    }

                    // --- ENTETE ---
                    // Table with 3 columns: widths (9cm, *, 5.25cm)
                    float cm = 28.3465f; // 1cm in points approx
                    Table headerTable = new Table(new float[] { 9 * cm, 1, 5.25f * cm });
                    headerTable.SetWidth(UnitValue.CreatePercentValue(100));

                    // Colonne 1: Table 3 lignes
                    Table col1Table = new Table(1);
                    // Ligne 1: Text Himalaya 8pt left
                    Paragraph p1 = new Paragraph("MINISTERE DE LA SANTE, DE L'HYGIENE PUBLIQUE ET DE L'ACCES UNIVERSEL AUX SOINS")
                        .SetFont(himalayaFontNormal)
                        .SetFontSize(8)
                        .SetTextAlignment(TextAlignment.LEFT);
                    col1Table.AddCell(new Cell().Add(p1).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                    // Ligne 2: Image inh_togo_logo.png height 2cm width 2.75cm left
                    string imgPath = System.IO.Path.Combine(_webHostEnvironment.WebRootPath, "img", "inh_togo_logo.png");
                    Image logo = new Image(ImageDataFactory.Create(imgPath))
                        .ScaleAbsolute(2.75f * cm, 2 * cm)
                        .SetHorizontalAlignment(HorizontalAlignment.LEFT);
                    col1Table.AddCell(new Cell().Add(logo).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                    // Ligne 3: Text Times New Roman 10 normal left
                    Paragraph p3 = new Paragraph("BP : 1396 Tél : 22 21 06 33 LOME-TOGO")
                        .SetFont(fontNormal)
                        .SetFontSize(10)
                        .SetTextAlignment(TextAlignment.LEFT);
                    col1Table.AddCell(new Cell().Add(p3).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                    headerTable.AddCell(new Cell().Add(col1Table).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                    // Colonne 2: espace vide restant (empty cell)
                    headerTable.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                    // Colonne 3: table 1 colonne, 3 lignes
                    Table col3Table = new Table(1).SetWidth(5.25f * cm);
                    col3Table.AddCell(new Cell().Add(new Paragraph("REPUBLIQUE TOGOLAISE")
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFont(fontNormal)
                        .SetFontSize(10))
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                    col3Table.AddCell(new Cell().Add(new Paragraph("Travail - Liberté - Patrie")
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFont(fontNormal)
                        .SetFontSize(10))
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                    // Ligne 3: espace vide restant
                    col3Table.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                    headerTable.AddCell(new Cell().Add(col3Table).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                    // Vérifier si le patient est assuré pour dupliquer le reçu
                    bool patientAssure = entetefacture.Entetedemande.Policeassurance != null;
                    
                    _logger.LogWarning($"Patient assuré : {patientAssure}");
                    
                    if (patientAssure)
                    {
                        // Générer le reçu PATIENT
                        GenerateReceiptWithIndicator(document, entetefacture, detaildemandes, "PATIENT", fontNormal, fontBold, himalayaFontNormal, cm, siteName);
                        _logger.LogWarning("Reçu PATIENT généré");
                        
                        // Nouvelle page pour le reçu ASSURANCE
                        document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                        
                        // Générer le reçu ASSURANCE
                        GenerateReceiptWithIndicator(document, entetefacture, detaildemandes, "ASSURANCE", fontNormal, fontBold, himalayaFontNormal, cm, siteName);
                        _logger.LogWarning("Reçu ASSURANCE généré");
                    }
                    else
                    {
                        // Générer un seul reçu sans indicateur pour les patients non assurés
                        GenerateReceiptWithIndicator(document, entetefacture, detaildemandes, null, fontNormal, fontBold, himalayaFontNormal, cm, siteName);
                        _logger.LogWarning("Reçu unique généré (patient non assuré)");
                    }



                    /* ------------------------- bulletin d'analyse ------------------------------- */

                    // Regrouper les analyses par laboratoire
                    var analysesParLaboratoire = toutesLesAnalyses
                        .GroupBy(a => new { a.Idlaboratoire, a.Laboratoirenom })
                        .ToList();

                    for (int index = 0; index < analysesParLaboratoire.Count; index++)
                    {
                        var groupeLaboratoire = analysesParLaboratoire[index];
                        var laboratoire = groupeLaboratoire.Key;
                        var analysesGroupe = groupeLaboratoire.ToList();
                        const float bulletinScale = 0.79f;
                        const float bulletinFontScale = 1.00f;
                        float B(float cmValue) => CmToPt(cmValue * bulletinScale);
                        float F(float fontSize) => fontSize * bulletinFontScale;
                        
                        // Créer la chaîne des analyses séparées par des virgules
                        var nomsAnalyses = string.Join(", ", analysesGroupe.Select(a => a.Nom));
                        
                        // Prendre la première analyse pour les données de codification (ou utiliser une logique différente si nécessaire)
                        var premiereAnalyse = analysesGroupe.First();

                        // ➕ Nouvelle page A4
                        document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

                        // Header table avec conversion en points
                        Table headerTable1 = new Table(new float[] { B(3.25f), B(9f), B(6.75f) }).UseAllAvailableWidth();

                        Image logo1 = new Image(ImageDataFactory.Create("wwwroot/img/inh_togo_logo.png"))
                            .SetWidth(B(3f))
                            .SetHeight(B(3.25f))
                            .SetHorizontalAlignment(HorizontalAlignment.CENTER);
                        headerTable1.AddCell(new Cell().Add(logo1).SetBorder(new SolidBorder(1)));

                        Paragraph title = new Paragraph("FICHE DE PRESCRIPTION D'ANALYSE")
                            .SetFont(fontNormal)
                            .SetFontSize(F(12))
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE);
                        headerTable1.AddCell(new Cell().Add(title).SetBorder(new SolidBorder(1)).SetVerticalAlignment(VerticalAlignment.MIDDLE));

                        Table headerRight = new Table(1)
                            .UseAllAvailableWidth();
                        headerRight.AddCell(new Paragraph($"Page : {index + 1} / {analysesParLaboratoire.Count}").SetFont(fontNormal).SetFontSize(F(10)));
                        headerRight.AddCell(new Paragraph($"Codification : {premiereAnalyse.Codification ?? ""}").SetFont(fontNormal).SetFontSize(F(10)));
                        headerRight.AddCell(new Paragraph($"Indice de rev : {premiereAnalyse.Indicederev ?? ""}").SetFont(fontNormal).SetFontSize(F(10)));
                        headerRight.AddCell(new Paragraph("Date d'application : ").SetFont(fontNormal).SetFontSize(F(10)));
                        headerTable1.AddCell(new Cell().Add(headerRight).SetBorder(new SolidBorder(1)));

                        document.Add(headerTable1);

                        _logger.LogWarning("entete bulletin");

                        // Espace vertical réduit pour conserver la fiche sur la moitié haute de la page
                        document.Add(new Paragraph("\n").SetHeight(B(0.2f)));

                        // Table principale (2 colonnes) avec conversion
                        Table mainTable1 = new Table(new float[] { B(11.75f), B(7.25f) })
                            .UseAllAvailableWidth()
                            //.SetWidth(UnitValue.CreatePercentValue(100))
                            .SetBorder(new iText.Layout.Borders.SolidBorder(ColorConstants.BLACK, 1));

                        // Colonne gauche (patient)
                        Table colPatient = new Table(1).UseAllAvailableWidth();
                        colPatient.SetBorder(new iText.Layout.Borders.SolidBorder(ColorConstants.BLACK, 1));

                        colPatient.AddCell(CreateCell("IDENTIFICATION DU PATIENT", fontNormal, F(9), TextAlignment.CENTER).SetUnderline());

                        // Sous-table (2 colonnes internes)
                        Table patientSubTable = new Table(new float[] { B(6.25f), B(5.5f) }).UseAllAvailableWidth();

                        // Colonne gauche patient (6 lignes)
                        Table patientLeft = new Table(1)
                            .UseAllAvailableWidth();

                        var lignesPatientLeft = new List<(string label, string valeur, float largeurCm)>
                        {
                            ("ID INH :",       idinh, 2.25f),
                            ("Nom :",          nom, 1.75f),
                            ("Prénoms :",      prenom, 2.25f),
                            ("Date naissance :", datenaissance, 3.75f),
                            ("Sexe :",         sexe, 1.5f),
                            ("Adresse :",      adresse, 2.25f)
                        };

                        foreach (var ligne in lignesPatientLeft)
                        {
                            Table t = new Table(new float[] {
                                B(ligne.largeurCm),
                                UnitValue.CreatePercentValue(100).GetValue()
                            });

                            t.SetWidth(UnitValue.CreatePercentValue(100));
                            t.SetBorder(iText.Layout.Borders.Border.NO_BORDER);

                            t.AddCell(CreateCell(ligne.label, fontBold, F(8), TextAlignment.LEFT, B(ligne.largeurCm)));
                            t.AddCell(CreateCell(ligne.valeur, fontNormal, F(8), TextAlignment.LEFT));

                            // Ajouter la ligne dans le tableau principal
                            patientLeft.AddCell(new Cell().Add(t).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                        }

                        // Colonne droite patient (6 lignes)
                        Table patientRight = new Table(1).UseAllAvailableWidth();

                        var lignesPatientRight = new List<(string label, string valeur, float? largeurCm)>
                        {
                            ("N° Reçu", recu, 2.0f),
                            ("Date :", datefacture, 1.5f),
                            ("Age :", age, 1.25f),
                            ("Tél :", tel, 1.0f)
                        };

                        foreach (var ligne in lignesPatientRight)
                        {
                            if (string.IsNullOrEmpty(ligne.label))
                            {
                                // Ligne vide : paragraphe vide dans une cellule sans bordure
                                patientRight.AddCell(new Cell().Add(new Paragraph(" ")).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                                continue;
                            }

                            Table t = new Table(new float[]
                            {
                                B(ligne.largeurCm.HasValue ? ligne.largeurCm.Value : 0.5f),
                                UnitValue.CreatePercentValue(100).GetValue()
                            });

                            t.SetWidth(UnitValue.CreatePercentValue(100));
                            t.SetBorder(iText.Layout.Borders.Border.NO_BORDER);

                            t.AddCell(CreateCell(ligne.label, fontBold, F(8), TextAlignment.LEFT, B(ligne.largeurCm.HasValue ? ligne.largeurCm.Value : 0.5f)));
                            t.AddCell(CreateCell(ligne.valeur ?? "", fontNormal, F(8), TextAlignment.LEFT));

                            patientRight.AddCell(new Cell().Add(t).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                        }

                        // Ajout colonnes au sous-tableau patient
                        patientSubTable.AddCell(new Cell().Add(patientLeft).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                        patientSubTable.AddCell(new Cell().Add(patientRight).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                        colPatient.AddCell(new Cell().Add(patientSubTable).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                        _logger.LogWarning("body bulletin patient");

                        // Colonne droite (prescripteur)
                        Table colPrescripteur = new Table(1)
                            .UseAllAvailableWidth();
                        //.SetBorder(new iText.Layout.Borders.SolidBorder(ColorConstants.BLACK, 1));

                        colPrescripteur.AddCell(CreateCell("IDENTIFICATION DU PRESCRIPTEUR", fontNormal, F(9), TextAlignment.CENTER).SetUnderline());

                        Table prescripteurTable = new Table(1).UseAllAvailableWidth();

                        var lignes = new List<(string label, string valeur, float largeurCm)>
                        {
                            ("Nom :", prescripteurnom, 1.5f),
                            ("Adresse :", prescripteuradresse, 2.0f),
                            ("Tél :", prescripteurtel, 1.0f)
                        };

                        foreach (var ligne in lignes)
                        {
                            Table t = new Table(new float[]
                            {
                                B(ligne.largeurCm),
                                UnitValue.CreatePercentValue(100).GetValue()
                            });

                            t.SetWidth(UnitValue.CreatePercentValue(100));
                            t.SetBorder(iText.Layout.Borders.Border.NO_BORDER);

                            t.AddCell(CreateCell(ligne.label, fontBold, F(8), TextAlignment.LEFT, B(ligne.largeurCm)));
                            t.AddCell(CreateCell(ligne.valeur, fontNormal, F(8), TextAlignment.LEFT));

                            prescripteurTable.AddCell(new Cell().Add(t).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                        }

                        colPrescripteur.AddCell(new Cell().Add(prescripteurTable).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                        _logger.LogWarning("body bulletin prescripteur");

                        // Ajout colonnes à la table principale
                        mainTable1.AddCell(new Cell().Add(colPatient).SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                        mainTable1.AddCell(new Cell().Add(colPrescripteur).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                        document.Add(mainTable1);

                        // TABLE 1 : 1 ligne 2 colonnes (2.25cm, 16.75cm)
                        Table table1 = new Table(new float[] { B(2.25f), B(16.75f) })
                            .UseAllAvailableWidth()
                            //.SetWidth(UnitValue.CreatePercentValue(100))
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1));

                        // Colonne 1 : "Analyse :"
                        table1.AddCell(CreateCell("Analyse :", fontBold, F(8), TextAlignment.LEFT, B(2.25f)));

                        // Colonne 2 : [valeur]
                        table1.AddCell(CreateCell($"{nomsAnalyses} ({laboratoire.Laboratoirenom})", fontNormal, F(8), TextAlignment.LEFT));

                        document.Add(table1);
                        //document.Add(new Paragraph("\n")); // petit espace

                        _logger.LogWarning("body bulletin analyse");

                        // TABLE 2 : 2 lignes 1 colonne (19cm hauteur - la hauteur n'est pas fixe ici mais le contenu suit)
                        Table table2 = new Table(1)
                            .UseAllAvailableWidth()
                            //.SetWidth(CmToPt(19f))
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 1));

                        // Ligne 1 : "PRELEVEMENT", souligné, centrée
                        Cell c1 = CreateCell("PRELEVEMENT", fontNormal, F(9), TextAlignment.CENTER);
                        c1.SetUnderline();
                        table2.AddCell(c1);

                        // Ligne 2 : table interne 4 lignes 1 colonne
                        Table innerTable = new Table(1).UseAllAvailableWidth();

                        // Ligne 1 : table 1 ligne 2 colonnes (5.5cm, reste)
                        Table ligne1 = new Table(new float[] { B(4.9f), UnitValue.CreatePercentValue(100).GetValue() })
                            .UseAllAvailableWidth();
                        //.SetWidth(UnitValue.CreatePercentValue(100))
                        //.SetBorder(iText.Layout.Borders.Border.NO_BORDER);

                        ligne1.AddCell(CreateCell("Type d'échantillon primaire :", fontBold, F(8.5f), TextAlignment.LEFT, B(4.9f)));

                        // Colonne 2 : table 2 lignes 1 colonne
                        Table typesTable = new Table(1)
                            .UseAllAvailableWidth();

                        // Ligne 1 : table 1 ligne 17 colonnes
                        float[] widthsL1 = new float[17];
                        for (int i = 0; i < 17; i++) widthsL1[i] = i % 2 == 0 ? B(1.15f) : B(0.4f);
                        Table typeLine1 = new Table(widthsL1)
                            .UseAllAvailableWidth();
                        //.SetWidth(UnitValue.CreatePercentValue(100))
                        //.SetBorder(iText.Layout.Borders.Border.NO_BORDER);

                        // Helper pour ajouter texte, checkbox, ou vide
                        void AddText1(string text)
                        {
                            var cell = CreateCell(text, fontNormal, F(10), TextAlignment.LEFT);
                            cell.SetMinHeight(B(0.55f));
                            typeLine1.AddCell(cell);
                        }
                        void AddCheckbox1(bool checkedBox)
                        {
                            string cb = checkedBox ? "☑" : "☐";
                            var cell = CreateCell(cb, fontNormal, F(10), TextAlignment.LEFT);
                            cell.SetMinHeight(B(0.55f));
                            // Optionnel : cadre autour de checkbox
                            //cell.SetBorder(new SolidBorder(ColorConstants.BLACK, 1));
                            //cell.SetPadding(0);
                            typeLine1.AddCell(cell);
                        }
                        void AddEmpty()
                        {
                            var cell = CreateCell(" ", fontNormal, F(10), TextAlignment.LEFT);
                            cell.SetMinHeight(B(0.55f));
                            typeLine1.AddCell(cell);
                        }

                        // Colonnes 1 à 17 :
                        AddText1("Sang"); AddCheckbox1(true); AddEmpty();
                        AddText1("Urine"); AddCheckbox1(true); AddEmpty();
                        AddText1("Selle"); AddCheckbox1(true); AddEmpty();
                        AddText1("PV"); AddCheckbox1(true); AddEmpty();
                        AddText1("PU"); AddCheckbox1(true); AddEmpty();
                        AddText1("CU"); AddCheckbox1(true);

                        // Ligne 2 : table 1 ligne 6 colonnes (pas besoin de largeur fixe ici)
                        Table typeLine2 = new Table(new float[] { B(2.4f), B(0.4f), B(2.0f), B(4.0f), B(4.8f), UnitValue.CreatePercentValue(100).GetValue() })
                            .UseAllAvailableWidth();
                        //.SetWidth(UnitValue.CreatePercentValue(100))
                        //.SetBorder(iText.Layout.Borders.Border.NO_BORDER);

                        void AddText2(string text)
                        {
                            var cell = CreateCell(text, fontNormal, F(10), TextAlignment.LEFT);
                            cell.SetMinHeight(B(0.55f));
                            typeLine2.AddCell(cell);
                        }
                        void AddCheckbox2(bool checkedBox)
                        {
                            string cb = checkedBox ? "☑" : "☐";
                            var cell = CreateCell(cb, fontNormal, F(10), TextAlignment.LEFT);
                            cell.SetMinHeight(B(0.55f));
                            // Optionnel : cadre autour de checkbox
                            //cell.SetBorder(new SolidBorder(ColorConstants.BLACK, 1));
                            //cell.SetPadding(0);
                            typeLine2.AddCell(cell);
                        }
                        void AddEmpty2()
                        {
                            var cell = CreateCell(" ", fontNormal, F(10), TextAlignment.LEFT);
                            cell.SetMinHeight(B(0.55f));
                            typeLine2.AddCell(cell);
                        }

                        AddText2("Sperme"); AddCheckbox2(true); AddEmpty2();
                        AddText2("Autres"); AddText2("......................"); AddEmpty2();

                        typesTable.AddCell(typeLine1);
                        typesTable.AddCell(typeLine2);

                        ligne1.AddCell(new Cell().Add(typesTable).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                        innerTable.AddCell(new Cell().Add(ligne1).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                        // Ligne 2 : table 1 ligne 2 colonnes (6.25cm, reste)
                        Table ligne2 = new Table(new float[] { B(6.25f), UnitValue.CreatePercentValue(100).GetValue() })
                            .UseAllAvailableWidth()
                            ////.SetWidth(UnitValue.CreatePercentValue(100))
                            .SetBorder(iText.Layout.Borders.Border.NO_BORDER);

                        ligne2.AddCell(CreateCell("Site anatomique de prélèvement :", fontBold, F(8), TextAlignment.LEFT, B(6.25f)));
                        ligne2.AddCell(CreateCell("", fontNormal, F(8), TextAlignment.LEFT));

                        innerTable.AddCell(new Cell().Add(ligne2).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                        // Ligne 3 : table 1 ligne 2 colonnes (5.25cm, reste)
                        Table ligne3 = new Table(new float[] { B(5.25f), UnitValue.CreatePercentValue(100).GetValue() })
                            .UseAllAvailableWidth()
                            //.SetWidth(UnitValue.CreatePercentValue(100))
                            .SetBorder(iText.Layout.Borders.Border.NO_BORDER);

                        ligne3.AddCell(CreateCell("Renseignements cliniques :", fontBold, F(8), TextAlignment.LEFT, B(5.25f)));
                        ligne3.AddCell(CreateCell("", fontNormal, F(8), TextAlignment.LEFT));

                        innerTable.AddCell(new Cell().Add(ligne3).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                        // Ligne 4 : table 1 ligne 4 colonnes (4.25cm, 5.5cm, 3.5cm, 5.75cm)
                        Table ligne4 = new Table(new float[]
                        {
                            B(4.25f),
                            B(5.5f),
                            B(3.5f),
                            B(5.75f)
                        })
                        .UseAllAvailableWidth()
                        //.SetWidth(UnitValue.CreatePercentValue(100))
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER);

                        ligne4.AddCell(CreateCell("Date de prélèvement :", fontBold, F(8), TextAlignment.LEFT, B(4.25f)));
                        ligne4.AddCell(CreateCell("", fontNormal, F(8), TextAlignment.LEFT));
                        ligne4.AddCell(CreateCell("Date de réception :", fontBold, F(8), TextAlignment.LEFT, B(3.5f)));
                        ligne4.AddCell(CreateCell("", fontNormal, F(8), TextAlignment.LEFT));

                        innerTable.AddCell(new Cell().Add(ligne4).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                        _logger.LogWarning("body bulletin prélevement");

                        table2.AddCell(innerTable);

                        document.Add(table2);
                    }

                    document.Close();

                    pdfBytes = stream.ToArray();
                }

                // Exemple de réouverture et modification
                using (var input = new MemoryStream(pdfBytes))
                using (var output = new MemoryStream())
                {
                    var reader = new PdfReader(input);
                    var writer = new PdfWriter(output);
                    var pdfDoc = new PdfDocument(reader, writer);

                    // Ajout des numéros de page (code selon besoin)
                    float height = CmToPt(0.6f); // Hauteur du footer
                    float width = CmToPt(2f);    // Largeur réservée pour le numéro de page
                    var fontFooter = PdfFontFactory.CreateFont(GetFontPath(EnumFont.TimesNewRoman.ToString(), true, false), PdfEncodings.IDENTITY_H);

                    // Nombre total de pages
                    int numberOfPages = pdfDoc.GetNumberOfPages();
                    
                    // Calculer le nombre de pages de reçus (1 si non assuré, 2 si assuré)
                    bool patientAssure = entetefacture.Entetedemande.Policeassurance != null;
                    int nombreRecus = patientAssure ? 2 : 1;
                    int nombreBulletins = numberOfPages - nombreRecus;

                    //Parcours des pages (on commence après les reçus)
                    for (int i = nombreRecus + 1; i <= numberOfPages; i++)    
                    {
                        PdfPage page = pdfDoc.GetPage(i);

                        float x = PageSize.A4.GetWidth() - marginRight - width / 2; // x depuis la droite
                        float y = marginBottom / 2 - height; // centré verticalement dans la marge

                        // Format pour les bulletins : 1/3, 2/3, etc.
                        var pageNumberText = $"{i - nombreRecus}/{nombreBulletins}";

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

                    // Conversion en Base64
                    string base64Pdf = Convert.ToBase64String(output.ToArray());

                    // Envoi à la vue via ViewData
                    ViewData["ReportPdfBase64"] = base64Pdf;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "erreur lors de la génération de recu et bulletin");
                throw;
            }

            return View();
        }

        //[HttpGet]
        //public async Task<IActionResult> Facture(Guid id)
        //{
        //    var entetefacture = await _context.Entetefactures
        //        .AsNoTracking()
        //        .Include(x => x.Entetedemande)
        //        .ThenInclude(x => x.Patient)
        //        .ThenInclude(x => x.CodesexeNavigation)
        //        .Include(x => x.Entetedemande.Prescripteur)
        //        .Include(x => x.Entetedemande.Policeassurance)
        //        .FirstOrDefaultAsync(e => e.Entetedemande.Entetedemandeid == id);

        //    if (entetefacture == null)
        //    {
        //        return NotFound();
        //    }

        //    var detaildemandes = await _context.Detaildemandes.AsNoTracking()
        //        .Where(x => x.Entetedemandeid == id)
        //        .ToListAsync();

        //    var idcategories = detaildemandes
        //        .Where(x => x.Categorieid != null)
        //        .Select(x => x.Categorieid)
        //        .Distinct();

        //    var idanalyses = detaildemandes
        //        .Where(x => x.Idanalyse != null)
        //        .Select(x => x.Idanalyse)
        //        .Distinct();

        //    var categorienometprix = await _context.Categories.AsNoTracking()
        //        .Where(x => idcategories.Contains(x.Categorieid))
        //        .Select(x => $"{x.Nom} {x.Prix}")
        //        .ToListAsync();

        //    var analysenometprix = await _context.Analyses.AsNoTracking()
        //        .Where(x => idanalyses.Contains(x.Idanalyse))
        //        .Select(x => $"{x.Nom} {x.Prix}")
        //        .ToListAsync();

        //    var categorieanalyses = await _context.Categorieanalyses.AsNoTracking()
        //        .Include(x => x.Categorie)
        //        .Include(x => x.IdanalyseNavigation)
        //        .Where(x => idcategories.Contains(x.Categorieid))
        //        .ToListAsync();

        //    var analyses = await _context.Analyses.AsNoTracking()
        //        .Where(x => idanalyses.Contains(x.Idanalyse))
        //        .ToListAsync();

        //    var categorieetanalyses = string.Join(", ", string.Join(", ", categorienometprix), string.Join(", ", analysenometprix));
        //    var total = detaildemandes.Sum(x => x.Prix);
        //    var partassurance = detaildemandes.Sum(x => x.Partassurance);
        //    var partpatient = detaildemandes.Sum(x => x.Partpatient);
        //    var complement = detaildemandes.Sum(x => x.Complement);
        //    var net = detaildemandes.Sum(x => x.Net);

        //    var dt = new DataTable("MyData"); // 👈 Name must match "Data" for clarity

        //    dt.Columns.Add("Analyse", typeof(string));

        //    foreach (var item in categorieanalyses)
        //    {
        //        dt.Rows.Add(item.IdanalyseNavigation.Nom);
        //    }

        //    foreach (var item in analyses)
        //    {
        //        dt.Rows.Add(item.Nom);
        //    }

        //    // Choose correct report file
        //    string reportPath = Path.Combine(_webHostEnvironment.WebRootPath,
        //        "reports",
        //        entetefacture.Entetedemande.Policeassurance == null
        //            ? "facture.frx"
        //            : "factureavecassurance.frx");

        //    var report = new Report();
        //    report.Load(reportPath);

        //    report.Report.SetParameterValue("recu", entetefacture.Numero);
        //    report.Report.SetParameterValue("factureid", entetefacture.Entetefactureid.ToString());
        //    report.Report.SetParameterValue("datefacture", entetefacture.Date.ToString("dd/MM/yyyy"));
        //    report.Report.SetParameterValue("net", (int)net);
        //    report.Report.SetParameterValue("nom", entetefacture.Entetedemande.Patient.Nom);
        //    report.Report.SetParameterValue("prenom", entetefacture.Entetedemande.Patient.Prenom);
        //    report.Report.SetParameterValue("datenaissance", entetefacture.Entetedemande.Patient.Datenaissance.ToString("dd/MM/yyyy"));
        //    report.Report.SetParameterValue("age", Utilities.CalculerAge(entetefacture.Entetedemande.Patient.Datenaissance));
        //    report.Report.SetParameterValue("sexe", entetefacture.Entetedemande.Patient.CodesexeNavigation.Value);
        //    report.Report.SetParameterValue("adresse", entetefacture.Entetedemande.Patient.Adresse);
        //    report.Report.SetParameterValue("tel", entetefacture.Entetedemande.Patient.Tel);
        //    report.Report.SetParameterValue("prescripteurnom", entetefacture.Entetedemande.Prescripteur.Nom);
        //    report.Report.SetParameterValue("prescripteuradresse", entetefacture.Entetedemande.Prescripteur.Adresse);
        //    report.Report.SetParameterValue("prescripteurtel", entetefacture.Entetedemande.Prescripteur.Tel);
        //    report.Report.SetParameterValue("somme", ((int)net).ToWords(new CultureInfo("fr")));
        //    report.Report.SetParameterValue("categorieetanalyse", categorieetanalyses);
        //    report.Report.SetParameterValue("dateimpression", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

        //    if (entetefacture.Entetedemande.Policeassurance != null)
        //    {
        //        report.Report.SetParameterValue("total", (int)total);
        //        report.Report.SetParameterValue("partassurance", (int)partassurance);
        //        report.Report.SetParameterValue("partpatient", (int)partpatient);
        //        report.Report.SetParameterValue("complement", (int)complement);
        //    }

        //    // Prepare report, register data, etc.
        //    report.Prepare();

        //    using (var ms = new MemoryStream())
        //    {
        //        //var pdfExport = new FastReport.Export.PdfSimple.PDFSimpleExport();
        //        var pdfExport = new FastReport.Export.Pdf.PDFExport();
        //        report.Export(pdfExport, ms);
        //        ms.Position = 0;

        //        var pdfBytes = ms.ToArray();
        //        string base64Pdf = Convert.ToBase64String(pdfBytes);
        //        ViewData["ReportPdfBase64"] = base64Pdf;
        //    }
        //    return View();

        //}

        [HttpGet]
        public async Task<IActionResult> GetAnalyse(Guid id, string laboratoireId)
        {
            var hasLaboratoireFilter = int.TryParse(laboratoireId, out int laboratoireIdInt) && laboratoireIdInt > 0;

            // Récupérer les détails de la demande
            var details = await _context.Detaildemandes
                .Where(d => d.Entetedemandeid == id)
                .Include(d => d.Categorie)
                .Include(d => d.IdanalyseNavigation)
                .ToListAsync();

            // Analyses sélectionnées directement dans la demande
            var analysesDirectesQuery = details
                .Where(d => d.Idanalyse != null)
                .Select(d => new {
                    Id = d.Idanalyse.Value,
                    Nom = d.IdanalyseNavigation != null ? d.IdanalyseNavigation.Nom : "",
                    NomCategorie = ""
                });

            if (hasLaboratoireFilter)
            {
                analysesDirectesQuery = details
                    .Where(d => d.Idanalyse != null
                        && d.IdanalyseNavigation != null
                        && d.IdanalyseNavigation.Idlaboratoire == laboratoireIdInt)
                    .Select(d => new {
                        Id = d.Idanalyse.Value,
                        Nom = d.IdanalyseNavigation != null ? d.IdanalyseNavigation.Nom : "",
                        NomCategorie = ""
                    });
            }

            var analysesDirectes = analysesDirectesQuery.ToList();

            // Catégories sélectionnées
            var categorieIds = details
                .Where(d => d.Categorieid != null)
                .Select(d => d.Categorieid.Value)
                .Distinct()
                .ToList();

            // Analyses liées aux catégories
            var analysesParCategorieQuery = _context.Categorieanalyses
                .Where(ca => categorieIds.Contains(ca.Categorieid))
                .Include(ca => ca.IdanalyseNavigation)
                .Include(ca => ca.Categorie)
                .AsQueryable();

            if (hasLaboratoireFilter)
            {
                analysesParCategorieQuery = analysesParCategorieQuery
                    .Where(ca => ca.IdanalyseNavigation != null && ca.IdanalyseNavigation.Idlaboratoire == laboratoireIdInt);
            }

            var analysesParCategorie = await analysesParCategorieQuery
                .Select(ca => new {
                    Id = ca.Idanalyse,
                    Nom = ca.IdanalyseNavigation != null ? ca.IdanalyseNavigation.Nom : "",
                    NomCategorie = ca.Categorie != null ? ca.Categorie.Nom : ""
                })
                .ToListAsync();

            // Combiner les deux listes sans supprimer les doublons, analysesParCategorie d'abord
            var toutesLesAnalyses = analysesParCategorie
                .Concat(analysesDirectes)
                .ToList();

            // Récupérer les analyses qui ont déjà des résultats saisis pour cette demande
            var analysesDejaSaisies = await _context.Enteteresultats
                .Where(er => er.Entetedemandeid == id)
                .Select(er => er.Idanalyse)
                .ToListAsync();

            // Filtrer pour exclure les analyses déjà saisies
            var analysesDisponibles = toutesLesAnalyses
                .Where(a => !analysesDejaSaisies.Contains(a.Id))
                .ToList();

            var analysesJson = JsonConvert.SerializeObject(analysesDisponibles);

            return Json(new ProlabWeb.JsonResponseViewModel
            {
                success = true,
                data = analysesJson,
                message = ""
            });
        }

        // GET: Entetedemande/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entetedemande = await _context.Entetedemandes.AsNoTracking()
                .Include(x => x.Patient)
                .Include(x => x.Prescripteur)
                .Include(x => x.Partenaire)
                .Include(x => x.Policeassurance)
                .Include(x => x.Utilisateur)
                .Include(x => x.CodesiteNavigation)
                .Where(x => x.Entetedemandeid == id)
                .FirstOrDefaultAsync();

            var detaildemandes = await _context.Detaildemandes.AsNoTracking()
                .Include(x => x.Categorie)
                .Include(x => x.IdanalyseNavigation)
                .Where(x => x.Entetedemandeid == id)
                .ToListAsync();

            var idscategorie = detaildemandes
                .Where(x => x.Categorieid != null)
                .AsEnumerable()
                .Select(x => x.Categorieid)
                .Distinct()
                .ToList().ToArray();

            var idsanalyse = detaildemandes
                .Where(x => x.Idanalyse != null)
                .AsEnumerable()
                .Select(x => x.Idanalyse)
                .Distinct()
                .ToList().ToArray();

            if (entetedemande == null)
            {
                return NotFound();
            }

            var model = _mapper.Map<EnteteDemandeEditVM>(entetedemande);

            model.IdsCategorie = idscategorie.Cast<Guid>().ToArray();
            model.IdsAnalyse = idsanalyse.Cast<Guid>().ToArray();
            model.EstAssure = entetedemande.Policeassuranceid != null && entetedemande.Policeassuranceid != Guid.Empty 
                ? true 
                : false;
            model.Codeassurance = entetedemande.Policeassurance?.Codeassurance;

            var photopatient = await _context.Photopatients.AsNoTracking()
                    .Where(x => x.Patientid == model.Patientid)
                    .FirstOrDefaultAsync();

            model.Patient.PhotoBase64 = photopatient != null
                            ? Utilities.ConvertByteArrayToBase64(photopatient.Photo, photopatient.Extension)
                            : "";

            var sites = new SelectList(_context.Sites, "Codesite", "Name", model.Codesite).ToList();
            sites.Insert(0, new SelectListItem("---", string.Empty));

            var utilisateurs = SelectUtilisateurList(model.Utilisateurid);

            var sexes = new SelectList(_context.Sexes, "Codesexe", "Value", model.Patient.Codesexe).ToList();
            sexes.Insert(0, new SelectListItem("---", string.Empty));

            var typepeaus = new SelectList(_context.Typepeaus, "Codetypepeau", "Nom", model.Patient.Nom).ToList();
            typepeaus.Insert(0, new SelectListItem("---", string.Empty));

            var typedocumentidentites = new SelectList(_context.Typedocumentidentites, "Codetypedocumentidentite", "Nom", model.Patient.Codetypedocumentidentite).ToList();
            typedocumentidentites.Insert(0, new SelectListItem("---", string.Empty));

            var assurances = new SelectList(_context.Assurances, "Codeassurance", "Nom", model.Codeassurance).ToList();
            assurances.Insert(0, new SelectListItem("---", string.Empty));

            var policeassurances = new SelectList(_context.Policeassurances, "Policeassuranceid", "Libelle", model.Policeassuranceid).ToList();
            policeassurances.Insert(0, new SelectListItem("---", string.Empty));

            var analyses = SelectAnalyseList(model.IdsAnalyse);

            var categories = SelectCategorieList(model.IdsCategorie);

            var patients = SelectPatientList(model.Patientid);

            var prescripteurs = SelectPrescripteurList(model.Prescripteurid);

            var partenaires = new SelectList(_context.Partenaires, "Partenaireid", "Nom", model.Partenaireid).ToList();
            partenaires.Insert(0, new SelectListItem("---", string.Empty));
            ViewBag.Partenaireid = partenaires;

            ViewData["Codesite"] = sites;
            ViewData["Codesexe"] = sexes;
            ViewData["Codetypepeau"] = typepeaus;
            ViewData["Codetypedocumentidentite"] = typedocumentidentites;
            ViewData["Codeassurance"] = assurances;
            ViewData["Policeassuranceid"] = policeassurances;
            ViewData["IdsAnalyse"] = analyses;
            ViewData["IdsCategorie"] = categories;
            ViewData["Utilisateurid"] = utilisateurs;
            ViewData["Patientid"] = patients;
            ViewData["Prescripteurid"] = prescripteurs;

            return View(model);
        }

        // POST: Entetedemande/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, EnteteDemandeEditVM model)
        {
            if (id != model.Entetedemandeid)
            {
                return NotFound();
            }

            //valider utilisateur

            bool isUtilisateurValid = true;

            Utilisateur utilisateur = null;
            //Utilisateurprofil utilisateurprofil = null;
            Affectationcaisse affectationcaisse = null;

            if (model.Utilisateurid != null && model.Utilisateurid != Guid.Empty)
            {
                //utilisateurprofil = await _context.Utilisateurprofils.AsNoTracking()
                //    .Include(x => x.Utilisateur)
                //    .Include(x => x.Profil)
                //    .Where(x => x.Utilisateurid == model.Utilisateurid
                //        && x.Profil.Nom == EnumProfil.caissier.ToString()
                //        )
                //    .FirstOrDefaultAsync();

                //if (utilisateurprofil == null)
                //{
                //    ModelState.AddModelError("Utilisateurid", "Utilisateur non valide.");
                //    isUtilisateurValid = false;
                //}

                utilisateur = await _context.Utilisateurs.AsNoTracking()
                    .Where(x => x.Utilisateurid == model.Utilisateurid)
                    .FirstOrDefaultAsync();

                if (utilisateur == null)
                {
                    ModelState.AddModelError("Utilisateurid", "Utilisateur non valide.");
                    isUtilisateurValid = false;
                }
                else
                {
                    affectationcaisse = await _context.Affectationcaisses.AsNoTracking()
                        .Include(x => x.Utilisateur)
                        .Include(x => x.Caisse)
                        .Where(x => x.Utilisateurid == model.Utilisateurid)
                        .FirstOrDefaultAsync();

                    if (affectationcaisse == null)
                    {
                        ModelState.AddModelError("Utilisateurid", "Aucune affectation de caisse.");
                        isUtilisateurValid = false;
                    }
                }
            }

            //valider analyse

            bool isAnalyseValid = true;

            if (model.IdsCategorie.Count() == 0
                && model.IdsAnalyse.Count() == 0
                )
            {
                ModelState.AddModelError("IdsCategorie", "Sélection vide.");
                ModelState.AddModelError("IdsAnalyse", "Sélection vide.");
                isAnalyseValid = false;
            }

            //valider assurance

            bool isAssuranceValid = true;

            if (model.EstAssure)
            {
                if (string.IsNullOrWhiteSpace(model.Codeassurance))
                {
                    ModelState.AddModelError("Codeassurance", "Sélection vide.");
                    isAssuranceValid = false;
                }
                if (model.Policeassuranceid == null || model.Policeassuranceid == Guid.Empty)
                {
                    ModelState.AddModelError("Policeassuranceid", "Sélection vide.");
                    isAssuranceValid = false;
                }
            }

            //valider nouveau patient

            bool isPatientValid = true;
            Patient patient = null;

            if (model.Patientid == null || model.Patientid == Guid.Empty)
            {
                ModelState.AddModelError("Patientid", "Sélection vide.");
                isPatientValid = false;
            }
            else
            {
                //ValidationContext vc = new ValidationContext(model.Patient);
                //ICollection<ValidationResult> results = new List<ValidationResult>();
                //isPatientValid = Validator.TryValidateObject(model.Patient, vc, results, true);

                patient = await _context.Patients.AsNoTracking()
                    .Where(x => x.Patientid == model.Patientid)
                    .FirstOrDefaultAsync();

                if (patient == null)
                {
                    ModelState.AddModelError("Patientid", "Patient non trouvé.");
                    isPatientValid = false;
                }
            }

            //valider nouveau prescripteur

            bool isPrescripteurValid = true;
            Prescripteur prescripteur = null;

            if (model.Prescripteurid == null || model.Prescripteurid == Guid.Empty)
            {
                ModelState.AddModelError("Prescripteurid", "Sélection vide.");
                isPrescripteurValid = false;
            }
            else
            {
                //ValidationContext vc = new ValidationContext(model.Prescripteur);
                //ICollection<ValidationResult> results = new List<ValidationResult>();
                //isPrescripteurValid = Validator.TryValidateObject(model.Prescripteur, vc, results, true);

                prescripteur = await _context.Prescripteurs.AsNoTracking()
                    .Where(x => x.Prescripteurid == model.Prescripteurid)
                    .FirstOrDefaultAsync();

                if (prescripteur == null)
                {
                    ModelState.AddModelError("Prescripteurid", "Prescripteur non trouvé.");
                    isPrescripteurValid = false;
                }
            }

            var ok = isUtilisateurValid && isAnalyseValid && isAssuranceValid && isPatientValid && isPrescripteurValid;

            if (ok)
            {
                try
                {
                    //var patient = _mapper.Map<Patient>(model.Patient);

                    //var prescripteur = _mapper.Map<Prescripteur>(model.Prescripteur);

                    if (string.IsNullOrWhiteSpace(model.Partenaireid.ToString()) || model.Partenaireid == Guid.Empty)
                    {
                        model.Partenaireid = null;
                    }

                    Policeassurance policeassurance = null;

                    if (model.Policeassuranceid != null && model.Policeassuranceid != Guid.Empty)
                    {
                        policeassurance = await _context.Policeassurances.AsNoTracking()
                            .Include(x => x.CodeassuranceNavigation)
                            .Where(x => x.Policeassuranceid == model.Policeassuranceid)
                            .FirstOrDefaultAsync();
                    }

                    //update patient

                    patient.Patientid = model.Patientid;

                    _context.Update(patient);

                    var file = model.Patient.File;
                    var filename = model.Patient.Filename;

                    if (!string.IsNullOrWhiteSpace(filename) && file != null && file.Length > 0)
                    {
                        var old = await _context.Photopatients.AsNoTracking()
                            .Where(x => x.Patientid == patient.Patientid)
                            .FirstOrDefaultAsync();

                        using (var ms = new MemoryStream())
                        {
                            await file.CopyToAsync(ms);
                            var fileBytes = ms.ToArray();

                            if (old == null)
                            {
                                var photoPatient = new Photopatient
                                {
                                    Photopatientid = Guid.NewGuid(),
                                    Patientid = patient.Patientid,
                                    Photo = fileBytes,
                                    Extension = System.IO.Path.GetExtension(file.FileName).TrimStart('.').ToLower()
                                };

                                _context.Add(photoPatient);
                            }
                            else
                            {
                                old.Photo = fileBytes;
                                old.Extension = System.IO.Path.GetExtension(file.FileName).ToLower();

                                _context.Update(old);
                            }
                        }
                    }

                    //update prescripteur

                    prescripteur.Prescripteurid = model.Prescripteurid;

                    _context.Update(prescripteur);

                    //update demande

                    var entetedemande = await _context.Entetedemandes.AsNoTracking()
                        .Where(x => x.Entetedemandeid == model.Entetedemandeid)
                        .FirstOrDefaultAsync();

                    if (entetedemande != null)
                    {
                        entetedemande.Partenaireid = model.Partenaireid;
                        entetedemande.Policeassuranceid = model.Policeassuranceid;

                        _context.Update(entetedemande);
                    }

                    //update detail demande

                    var andetaildemandes = await _context.Detaildemandes.AsNoTracking()
                        .Where(x => x.Entetedemandeid == model.Entetedemandeid)
                        .ToListAsync();

                    // parcourir la selection ancienne

                    var detaildemandeToDelete = new List<Detaildemande>();

                    foreach (var item in andetaildemandes)
                    {
                        if (item.Categorieid != null) 
                        {
                            var found = model.IdsCategorie.Where(x => x == item.Categorieid).FirstOrDefault();

                            if (found == null)  //non trouvé
                            {
                                //supprimer

                                detaildemandeToDelete.Add(item);
                            }
                        }

                        if (item.Idanalyse != null) 
                        {
                            var found = model.IdsAnalyse.Where(x => x == item.Idanalyse).FirstOrDefault();

                            if (found == null)  //non trouvé
                            {
                                //supprimer

                                detaildemandeToDelete.Add(item);
                            }
                        }
                    }

                    if (detaildemandeToDelete.Any())
                    {
                        _context.RemoveRange(detaildemandeToDelete);
                    }

                    // parcourir la selection nouvelle

                    var detaildemandeToCreate = new List<Detaildemande>();
                    var detaildemandeToUpdate = new List<Detaildemande>();

                    if (model.IdsCategorie != null && model.IdsCategorie.Length > 0)
                    {
                        var listcategorie = await _context.Categories.AsNoTracking()
                            .Where(x => model.IdsCategorie.Contains(x.Categorieid))
                            .ToListAsync();

                        var listtarif = policeassurance != null
                            ? await _context.Tarifcategorieassurances.AsNoTracking()
                                .Where(x => model.IdsCategorie.Contains(x.Categorieid)
                                    && x.Codeassurance == policeassurance.Codeassurance)
                                .ToListAsync()
                            : new List<Tarifcategorieassurance>();

                        foreach (var item in model.IdsCategorie)
                        {
                            var prix = listcategorie.Where(x => x.Categorieid == item).Select(x => x.Prix).FirstOrDefault();
                            prix = prix != null ? prix : 0;

                            var tarif = listtarif.Where(x => x.Categorieid == item).Select(x => x.Prix).FirstOrDefault();
                            tarif = tarif != null ? tarif : 0;

                            var calculs = Utilities.CalculerDifferentPrixDetailDemande(policeassurance, prix, tarif);

                            var found = andetaildemandes.Where(x => x.Categorieid == item).FirstOrDefault();

                            if (found != null)
                            {
                                //update

                                found.Prix = calculs.Item1;
                                found.Partassurance = calculs.Item2;
                                found.Partpatient = calculs.Item3;
                                found.Complement = calculs.Item4;
                                found.Net = calculs.Item5;

                                detaildemandeToUpdate.Add(found);
                            }
                            else
                            {
                                //create

                                var obj = new Detaildemande
                                {
                                    Detaildemandeid = Guid.NewGuid(),
                                    Entetedemandeid = model.Entetedemandeid,
                                    Categorieid = item,
                                    Idanalyse = null,
                                    Prix = calculs.Item1,
                                    Partassurance = calculs.Item2,
                                    Partpatient = calculs.Item3,
                                    Complement = calculs.Item4,
                                    Net = calculs.Item5
                                };

                                detaildemandeToCreate.Add(obj);
                            }
                        }
                    }

                    if (model.IdsAnalyse != null && model.IdsAnalyse.Length > 0)
                    {
                        var listanalyse = await _context.Analyses.AsNoTracking()
                        .Where(x => model.IdsAnalyse.Contains(x.Idanalyse))
                        .ToListAsync();

                        var listtarif = policeassurance != null
                            ? await _context.Tarifanalyseassurances.AsNoTracking()
                                .Where(x => model.IdsAnalyse.Contains(x.Idanalyse)
                                    && x.Codeassurance == policeassurance.Codeassurance)
                                .ToListAsync()
                            : new List<Tarifanalyseassurance>();

                        foreach (var item in model.IdsAnalyse)
                        {
                            var prix = listanalyse.Where(x => x.Idanalyse == item).Select(x => x.Prix).FirstOrDefault();
                            prix = prix != null ? prix : 0;

                            var tarif = listtarif.Where(x => x.Idanalyse == item).Select(x => x.Prix).FirstOrDefault();
                            tarif = tarif != null ? tarif : 0;

                            var calculs = Utilities.CalculerDifferentPrixDetailDemande(policeassurance, prix, tarif);

                            var found = andetaildemandes.Where(x => x.Idanalyse == item).FirstOrDefault();

                            if (found != null)
                            {
                                //update

                                found.Prix = calculs.Item1;
                                found.Partassurance = calculs.Item2;
                                found.Partpatient = calculs.Item3;
                                found.Complement = calculs.Item4;
                                found.Net = calculs.Item5;

                                detaildemandeToUpdate.Add(found);
                            }
                            else
                            {
                                //create

                                var obj = new Detaildemande
                                {
                                    Detaildemandeid = Guid.NewGuid(),
                                    Entetedemandeid = model.Entetedemandeid,
                                    Categorieid = null,
                                    Idanalyse = item,
                                    Prix = calculs.Item1,
                                    Partassurance = calculs.Item2,
                                    Partpatient = calculs.Item3,
                                    Complement = calculs.Item4,
                                    Net = calculs.Item5
                                };

                                detaildemandeToCreate.Add(obj);
                            }
                        }
                    }

                    if (detaildemandeToCreate.Any())
                    {
                        _context.AddRange(detaildemandeToCreate);
                    }
                    if (detaildemandeToUpdate.Any())
                    {
                        _context.UpdateRange(detaildemandeToUpdate);
                    }

                    //save

                    await _context.SaveChangesAsync();

                    //update facture 

                    var entetefacture = await _context.Entetefactures.AsNoTracking()
                        .Include(x => x.Entetedemande)
                        .Where(x => x.Entetedemandeid == model.Entetedemandeid)
                        .FirstOrDefaultAsync();

                    if (entetefacture != null)
                    {
                        //update detail facture

                        var andetailfactures = await _context.Detailfactures.AsNoTracking()
                            .Include(x => x.Detaildemande)
                            .Include(x => x.Entetefacture)
                            .Where(x => x.Entetefactureid == entetefacture.Entetefactureid)
                            .ToListAsync();

                        var detaildemandes = await _context.Detaildemandes.AsNoTracking()
                            .Where(x => x.Entetedemandeid == model.Entetedemandeid)
                            .ToListAsync();

                        //parcourir la selection ancienne

                        var detailfactureToDelete = new List<Detailfacture>();

                        foreach (var item in andetailfactures)
                        {
                            var found = detaildemandes.Where(x => x.Detaildemandeid == item.Detaildemande.Detaildemandeid).FirstOrDefault();

                            //non trouvé

                            if (found == null)
                            {
                                //supprimer

                                detailfactureToDelete.Add(item);
                            }
                        }
                        if (detailfactureToDelete.Any())
                        {
                            _context.RemoveRange(detailfactureToDelete);
                        }

                        //parcourir la selection nouvelle

                        var detailfactureToCreate = new List<Detailfacture>();
                        var detailfactureToUpdate = new List<Detailfacture>();

                        foreach (var item in detaildemandes)
                        {
                            var found = andetailfactures.Where(x => x.Detaildemandeid == item.Detaildemandeid).FirstOrDefault();

                            //trouvé

                            if (found != null)
                            {
                                //update nothing
                            }
                            else
                            {
                                //créer

                                var obj = new Detailfacture
                                {
                                    Detailfactureid = Guid.NewGuid(),
                                    Entetefactureid = entetefacture.Entetefactureid,
                                    Detaildemandeid = item.Detaildemandeid
                                };

                                detailfactureToCreate.Add(obj);
                            }
                        }

                        if (detailfactureToCreate.Any())
                        {
                            _context.AddRange(detailfactureToCreate);
                        }
                        if (detailfactureToUpdate.Any())
                        {
                            _context.UpdateRange(detailfactureToUpdate);
                        }
                    }

                    //save

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EntetedemandeExists(model.Entetedemandeid))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            var sites = new SelectList(_context.Sites, "Codesite", "Name", model.Codesite).ToList();
            sites.Insert(0, new SelectListItem("---", string.Empty));

            var utilisateurs = SelectUtilisateurList(model.Utilisateurid);

            var sexes = new SelectList(_context.Sexes, "Codesexe", "Value", model.Patient.Codesexe).ToList();
            sexes.Insert(0, new SelectListItem("---", string.Empty));

            var typepeaus = new SelectList(_context.Typepeaus, "Codetypepeau", "Nom", model.Patient.Nom).ToList();
            typepeaus.Insert(0, new SelectListItem("---", string.Empty));

            var typedocumentidentites = new SelectList(_context.Typedocumentidentites, "Codetypedocumentidentite", "Nom", model.Patient.Codetypedocumentidentite).ToList();
            typedocumentidentites.Insert(0, new SelectListItem("---", string.Empty));

            var assurances = new SelectList(_context.Assurances, "Codeassurance", "Nom", model.Codeassurance).ToList();
            assurances.Insert(0, new SelectListItem("---", string.Empty));

            var policeassurances = new SelectList(_context.Policeassurances, "Policeassuranceid", "Libelle", model.Policeassuranceid).ToList();
            policeassurances.Insert(0, new SelectListItem("---", string.Empty));

            var analyses = SelectAnalyseList(model.IdsAnalyse);

            var categories = SelectCategorieList(model.IdsCategorie);

            var patients = SelectPatientList(model.Patientid);

            var prescripteurs = SelectPrescripteurList(model.Prescripteurid);

            var partenaires = new SelectList(_context.Partenaires, "Partenaireid", "Nom", model.Partenaireid).ToList();
            partenaires.Insert(0, new SelectListItem("---", string.Empty));
            ViewBag.Partenaireid = partenaires;

            ViewData["Codesite"] = sites;
            ViewData["Codesexe"] = sexes;
            ViewData["Codetypepeau"] = typepeaus;
            ViewData["Codetypedocumentidentite"] = typedocumentidentites;
            ViewData["Codeassurance"] = assurances;
            ViewData["Policeassuranceid"] = policeassurances;
            ViewData["IdsAnalyse"] = analyses;
            ViewData["IdsCategorie"] = categories;
            ViewData["Utilisateurid"] = utilisateurs;
            ViewData["Patientid"] = patients;
            ViewData["Prescripteurid"] = prescripteurs;

            return View(model);
        }

        // GET: Entetedemande/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entetedemande = await _context.Entetedemandes
                .Include(e => e.CodesiteNavigation)
                .Include(e => e.Patient)
                .Include(e => e.Policeassurance)
                .Include(e => e.Prescripteur)
                .FirstOrDefaultAsync(m => m.Entetedemandeid == id);
            if (entetedemande == null)
            {
                return NotFound();
            }

            return View(entetedemande);
        }

        // POST: Entetedemande/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var entetedemande = await _context.Entetedemandes.FindAsync(id);
            if (entetedemande != null)
            {
                _context.Entetedemandes.Remove(entetedemande);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EntetedemandeExists(Guid id)
        {
            return _context.Entetedemandes.Any(e => e.Entetedemandeid == id);
        }

        public sealed class MontantPreviewRequest
        {
            public Guid[]? IdsCategorie { get; set; }
            public Guid[]? IdsAnalyse { get; set; }
            public bool EstAssure { get; set; }
            public Guid? Policeassuranceid { get; set; }
        }

        #region

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CalculateNetPreview([FromBody] MontantPreviewRequest? request)
        {
            if (request == null)
            {
                return Ok(new JsonResponseViewModel
                {
                    success = false,
                    message = "Requête invalide"
                });
            }

            var idsCategorie = (request.IdsCategorie ?? Array.Empty<Guid>())
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToArray();

            var idsAnalyse = (request.IdsAnalyse ?? Array.Empty<Guid>())
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToArray();

            Policeassurance? policeassurance = null;
            if (request.EstAssure && request.Policeassuranceid.HasValue && request.Policeassuranceid.Value != Guid.Empty)
            {
                policeassurance = await _context.Policeassurances.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Policeassuranceid == request.Policeassuranceid.Value);
            }

            var categoriesPrix = idsCategorie.Length > 0
                ? await _context.Categories.AsNoTracking()
                    .Where(x => idsCategorie.Contains(x.Categorieid))
                    .ToDictionaryAsync(x => x.Categorieid, x => x.Prix)
                : new Dictionary<Guid, decimal>();

            var analysesPrix = idsAnalyse.Length > 0
                ? await _context.Analyses.AsNoTracking()
                    .Where(x => idsAnalyse.Contains(x.Idanalyse))
                    .ToDictionaryAsync(x => x.Idanalyse, x => x.Prix)
                : new Dictionary<Guid, decimal>();

            var tarifCategories = (policeassurance != null && idsCategorie.Length > 0)
                ? await _context.Tarifcategorieassurances.AsNoTracking()
                    .Where(x => x.Codeassurance == policeassurance.Codeassurance && idsCategorie.Contains(x.Categorieid))
                    .ToDictionaryAsync(x => x.Categorieid, x => x.Prix)
                : new Dictionary<Guid, decimal>();

            var tarifAnalyses = (policeassurance != null && idsAnalyse.Length > 0)
                ? await _context.Tarifanalyseassurances.AsNoTracking()
                    .Where(x => x.Codeassurance == policeassurance.Codeassurance && idsAnalyse.Contains(x.Idanalyse))
                    .ToDictionaryAsync(x => x.Idanalyse, x => x.Prix)
                : new Dictionary<Guid, decimal>();

            decimal totalPublic = 0;
            decimal partAssurance = 0;
            decimal partPatient = 0;
            decimal complement = 0;
            decimal netAPayer = 0;

            foreach (var id in idsCategorie)
            {
                var prixBase = categoriesPrix.TryGetValue(id, out var prixCategorie) ? prixCategorie : 0;
                var tarif = tarifCategories.TryGetValue(id, out var tarifCategorie) ? tarifCategorie : 0;

                var calculs = Utilities.CalculerDifferentPrixDetailDemande(policeassurance, prixBase, tarif);
                totalPublic += prixBase;
                partAssurance += calculs.Item2;
                partPatient += calculs.Item3;
                complement += calculs.Item4;
                netAPayer += calculs.Item5;
            }

            foreach (var id in idsAnalyse)
            {
                var prixBase = analysesPrix.TryGetValue(id, out var prixAnalyse) ? prixAnalyse : 0;
                var tarif = tarifAnalyses.TryGetValue(id, out var tarifAnalyse) ? tarifAnalyse : 0;

                var calculs = Utilities.CalculerDifferentPrixDetailDemande(policeassurance, prixBase, tarif);
                totalPublic += prixBase;
                partAssurance += calculs.Item2;
                partPatient += calculs.Item3;
                complement += calculs.Item4;
                netAPayer += calculs.Item5;
            }

            return Ok(new JsonResponseViewModel
            {
                success = true,
                data = new
                {
                    totalPublic,
                    partAssurance,
                    partPatient,
                    complement,
                    netAPayer
                }
            });
        }

        [HttpGet]
        public IActionResult GetPatientDetails(Guid enteteDemandeId)
        {
            var details = _context.Entetedemandes
                .Include(x => x.Patient)
                .Include(p => p.Patient.CodesexeNavigation)
                .Include(p => p.Patient.CodetypepeauNavigation)
                .Where(p => p.Entetedemandeid == enteteDemandeId)
                .Select(p => new {
                    age = Utilities.CalculerAgeEnAnnee(p.Patient.Datenaissance),
                    sexe = p.Patient.CodesexeNavigation.Value,
                    peau = p.Patient.CodetypepeauNavigation.Nom ?? ""
                })
                .FirstOrDefault();

            if (details == null)
                return NotFound(new { message = "Détails du patient non chargé" });

            return Json(new { success = true, data = details });
        }

        [HttpPost, ActionName("DeleteEntetedemande")]
        public async Task<IActionResult> DeleteEntetedemandeAsync(Guid id)
        {
            JsonResponseViewModel resultat = new JsonResponseViewModel();

            if (id != Guid.Empty)
            {
                try
                {
                    // Récupérer l'entité avec tracking pour pouvoir la supprimer
                    var entetedemande = await _context.Entetedemandes
                        .Include(e => e.Detaildemandes)
                        .Include(e => e.Entetefactures)
                        .Include(e => e.Enteteresultats)
                        .Include(e => e.Demandeanalysemateriels)
                        .FirstOrDefaultAsync(x => x.Entetedemandeid == id);

                    if (entetedemande != null)
                    {
                        // Supprimer les entités liées en premier
                        if (entetedemande.Detaildemandes.Any())
                        {
                            _context.Detaildemandes.RemoveRange(entetedemande.Detaildemandes);
                        }

                        if (entetedemande.Entetefactures.Any())
                        {
                            _context.Entetefactures.RemoveRange(entetedemande.Entetefactures);
                        }

                        if (entetedemande.Enteteresultats.Any())
                        {
                            _context.Enteteresultats.RemoveRange(entetedemande.Enteteresultats);
                        }

                        if (entetedemande.Demandeanalysemateriels.Any())
                        {
                            _context.Demandeanalysemateriels.RemoveRange(entetedemande.Demandeanalysemateriels);
                        }

                        // Supprimer l'entité principale
                        _context.Entetedemandes.Remove(entetedemande);
                        
                        await _context.SaveChangesAsync();

                        resultat.success = true;
                        resultat.message = "Demande supprimée avec succès";
                    }
                    else
                    {
                        resultat.success = false;
                        resultat.message = "Demande non trouvée";
                    }
                }
                catch (Exception ex)
                {
                    resultat.success = false;
                    resultat.message = $"Erreur lors de la suppression : {ex.Message}";
                }
            }
            else
            {
                resultat.success = false;
                resultat.message = "ID invalide";
            }

            return Ok(resultat);
        }

        #endregion
    }
}
