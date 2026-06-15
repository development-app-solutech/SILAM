using FastReport.AdvMatrix;
using System.Globalization;
using System.Text;
using iText.IO.Font;
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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProlabWeb.Data;
using ProlabWeb.Db;
using ProlabWeb.Helpers;
using ProlabWeb.ViewModels;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace ProlabWeb.Controllers
{
    [AllowAnonymous]
    public class EnteteresultatController : Controller
    {
        private readonly ProlabwebContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ProlabIdentityUser> _usermanager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EnteteresultatController> _logger;

        public EnteteresultatController(ProlabwebContext context, IWebHostEnvironment webHostEnvironment, UserManager<ProlabIdentityUser> userManager, IConfiguration configuration, ILogger<EnteteresultatController> logger)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _usermanager = userManager;
            _configuration = configuration;
            _logger = logger;
        }

        // GET: Enteteresultat
        public async Task<IActionResult> Index()
        {
            var query = _context.Enteteresultats
                .Include(e => e.CodesiteNavigation)
                .Include(e => e.Entetedemande)
                    .ThenInclude(d => d.Patient)
                .Include(e => e.Entetedemande)
                    .ThenInclude(d => d.Prescripteur)
                .Include(e => e.Entetedemande)
                    .ThenInclude(d => d.Policeassurance)
                        .ThenInclude(pa => pa.CodeassuranceNavigation)
                .Include(e => e.IdanalyseNavigation)
                    .ThenInclude(a => a.Categorieanalyses)
                        .ThenInclude(ca => ca.Categorie)
                .Include(e => e.Technicien)
                .AsQueryable();

            // Si l'utilisateur a le rôle Technicien ou Biologiste, filtrer par laboratoire
            if (User.IsInRole(EnumRoles.Technicien.ToString()) || User.IsInRole(EnumRoles.Biologiste.ToString()))
            {
                // Récupérer l'utilisateur connecté
                var user = await _usermanager.GetUserAsync(User);
                if (user != null)
                {
                    var utilisateur = await _context.Utilisateurs.AsNoTracking()
                        .Where(x => x.Userid == user.Id)
                        .FirstOrDefaultAsync();

                    if (utilisateur != null)
                    {
                        // Récupérer les laboratoires de l'utilisateur (technicien ou biologiste)
                        var laboratoires = await _context.Utilisateurlaboratoires.AsNoTracking()
                            .Include(x => x.IdlaboratoireNavigation)
                            .Where(x => x.Utilisateurid == utilisateur.Utilisateurid)
                            .Select(x => x.IdlaboratoireNavigation.Idlaboratoire)
                            .ToListAsync();

                        if (laboratoires.Any())
                        {
                            // Filtrer les résultats pour ne montrer que ceux des laboratoires de l'utilisateur
                            query = query.Where(e => laboratoires.Contains(e.IdanalyseNavigation.Idlaboratoire));
                        }
                    }
                }
            }

            // Trier par date/heure décroissante (le plus récent d'abord) puis par ID décroissant (nouveaux GUID en premier)
            var prolabwebContext = query.OrderByDescending(e => e.Date).ThenByDescending(e => e.Enteteresultatid);
            return View(await prolabwebContext.ToListAsync());
        }

        // GET: Enteteresultat/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Enteteresultats == null)
            {
                return NotFound();
            }

            // Use same logic as Edit to load full data with parameters
            var entete = await _context.Enteteresultats
                .AsNoTracking()
                .Select(e => new
                {
                    e.Enteteresultatid,
                    e.Codesite,
                    e.Technicienid,
                    e.Entetedemandeid,
                    e.Date,
                    e.Interpretation,
                    e.Idanalyse,
                    Detailresultats = e.Detailresultats.Select(d => new
                    {
                        d.Parametreid,
                        d.Resultat,
                        d.Resultatsi,
                        d.Commentaire,
                        ParametreNom = d.Parametre.Nom,
                        ParametreCode = d.Parametre.Code,
                        UniteNom = d.Parametre.CodeuniteNavigation.Name,
                        UniteSiNom = d.Parametre.CodeunitesiNavigation.Name
                    }).ToList()
                })
                .FirstOrDefaultAsync(e => e.Enteteresultatid == id);
                
            if (entete == null) return NotFound();

            // Get laboratory info
            var laboratoireId = await _context.Utilisateurlaboratoires
                .AsNoTracking()
                .Where(x => x.Utilisateurid == entete.Technicienid)
                .Select(x => x.IdlaboratoireNavigation.Idlaboratoire)
                .FirstOrDefaultAsync();

            // Create same ViewModel as Edit for consistency
            var vm = new ProlabWeb.ViewModels.EnteteresultatEditVM
            {
                Enteteresultatid = entete.Enteteresultatid,
                Codesite = entete.Codesite,
                Technicienid = entete.Technicienid,
                Laboratoireid = laboratoireId,
                Entetedemandeid = entete.Entetedemandeid,
                Date = entete.Date,
                Interpretation = entete.Interpretation,
                Idanalyse = entete.Idanalyse,
                Parametres = entete.Detailresultats.Select(d => new ProlabWeb.ViewModels.ParametreResultatVM
                    {
                        Parametreid = d.Parametreid,
                        Nom = d.ParametreNom ?? "",
                        Code = d.ParametreCode ?? "",
                        Unite = d.UniteNom ?? "",
                        Resultat = d.Resultat,
                        UniteSI = d.UniteSiNom ?? "",
                        Resultatsi = d.Resultatsi,
                        Commentaire = d.Commentaire,
                }).ToList()
            };
            
            // Load prelevement data
            await LoadPrelevementData(vm);
            
            // Initialize ViewData for dropdowns (needed for display)
            await InitCreateViewDataOptimized(vm);
            
            return View(vm);
        }

        // GET: Enteteresultat/Create
        public async Task<IActionResult> Create(Guid? entetedemandeid = null)
        {
            // Block access for users with "Caisse" role
            if (User.IsInRole(EnumRoles.Caisse.ToString()))
            {
                return Forbid();
            }

            // Allow access only for Technicien and Biologiste roles
            if (!User.IsInRole(EnumRoles.Technicien.ToString()) && !User.IsInRole(EnumRoles.Biologiste.ToString()))
            {
                return Forbid();
            }

            // utilisateur connecté
            var user = await _usermanager.GetUserAsync(User);

            Utilisateur utilisateur = null;
            Loboratoire laboratoire = null;

            if (user != null)
            {
                utilisateur = await _context.Utilisateurs.AsNoTracking()
                    .Include(x => x.CodesiteNavigation)
                    .Where(x => x.Userid == user.Id)
                    .FirstOrDefaultAsync();

                if (utilisateur != null)
                {
                    laboratoire = await _context.Utilisateurlaboratoires.AsNoTracking()
                        .Include(x => x.IdlaboratoireNavigation)
                        .Where(x => x.Utilisateurid == utilisateur.Utilisateurid)
                        .Select(x => x.IdlaboratoireNavigation)
                        .FirstOrDefaultAsync();
                }
            }

            var viewModel = new EnteteresultatCreateVM
            {
                Codesite = utilisateur?.CodesiteNavigation?.Codesite,
                Technicienid = utilisateur?.Utilisateurid ?? Guid.Empty,
                Laboratoireid = laboratoire?.Idlaboratoire ?? 0,
                Date = DateTime.Now, // Utiliser l'heure actuelle pour que ce résultat soit en tête de liste
            };

            // Pre-fill the Entetedemandeid if provided
            if (entetedemandeid.HasValue && entetedemandeid.Value != Guid.Empty)
            {
                viewModel.Entetedemandeid = entetedemandeid.Value;
                
                // Charger les données de prélèvement existantes si disponibles
                await LoadPrelevementData(viewModel);
            }

            await InitCreateViewDataOptimized(viewModel);

            return View(viewModel);
        }

        // POST: Enteteresultat/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EnteteresultatCreateVM viewModel)
        {
            // Block access for users with "Caisse" role
            if (User.IsInRole(EnumRoles.Caisse.ToString()))
            {
                return Forbid();
            }

            // Allow access only for Technicien and Biologiste roles
            if (!User.IsInRole(EnumRoles.Technicien.ToString()) && !User.IsInRole(EnumRoles.Biologiste.ToString()))
            {
                return Forbid();
            }
            // Vérification : si tous les paramètres ont Resultat et Resultatsi vides, retourner la vue Create
            /* if (viewModel.Parametres != null && viewModel.Parametres.All(p => !p.Resultat.HasValue && !p.Resultatsi.HasValue))
            {
                ModelState.AddModelError("Parametres", "Veuillez saisir au moins un résultat ou résultat SI pour les paramètres.");
                InitCreateViewData();
                return View(viewModel);
            } */

            if (ModelState.IsValid)
            {
                var enteteresultat = new Enteteresultat
                {
                    Enteteresultatid = Guid.NewGuid(),
                    Codesite = viewModel.Codesite,
                    Technicienid = viewModel.Technicienid,
                    Entetedemandeid = viewModel.Entetedemandeid,
                    Date = DateTime.Now.AddMilliseconds(1), // Ajouter 1ms pour garantir que ce soit le plus récent
                    Interpretation = viewModel.Interpretation,
                    Idanalyse = viewModel.Idanalyse,
                    Validationtechnicien = false, // Les nouveaux résultats ne sont pas validés par défaut
                    Statut = "en_attente" // Statut initial : en attente de validation
                };

                _context.Enteteresultats.Add(enteteresultat);

                // Créer les enfants Detailresultat pour chaque paramètre ayant une valeur pour Resultat ou Resultatsi
                if (viewModel.Parametres != null)
                {
                    var parametresAvecValeur = viewModel.Parametres;//.Where(p => p.Resultat.HasValue || p.Resultatsi.HasValue).ToList();
                    var detailsToAdd = new List<Detailresultat>();
                    
                    // Récupérer les informations du patient pour l'évaluation des valeurs de référence
                    var entetedemande = await _context.Entetedemandes
                        .Include(e => e.Patient)
                        .FirstOrDefaultAsync(e => e.Entetedemandeid == viewModel.Entetedemandeid);
                    
                    int patientAge = 0;
                    string patientSexe = "";
                    if (entetedemande?.Patient != null)
                    {
                        var dateNaissance = entetedemande.Patient.Datenaissance;
                        if (dateNaissance != DateTime.MinValue)
                        {
                            patientAge = DateTime.Now.Year - dateNaissance.Year;
                            if (DateTime.Now.DayOfYear < dateNaissance.DayOfYear)
                                patientAge--;
                        }
                        patientSexe = entetedemande.Patient.Codesexe ?? "";
                    }
                    
                    foreach (var param in parametresAvecValeur)
                    {
                        // Calculer automatiquement le commentaire basé sur les valeurs de référence
                        string commentaireAuto = await CalculerCommentaireAutomatique(viewModel.Idanalyse, param.Resultat, patientAge, patientSexe);
                        
                        var detail = new Detailresultat
                        {
                            Detailresultatid = Guid.NewGuid(),
                            Enteteresultatid = enteteresultat.Enteteresultatid,
                            Parametreid = param.Parametreid,
                            Resultat = param.Resultat?.Replace(",", "."),
                            Resultatsi = param.Resultatsi?.Replace(",", "."),
                            Date = DateTime.Now.AddMilliseconds(1), // Même date/heure que l'entête
                            Commentaire = commentaireAuto, // Utiliser le commentaire auto-calculé
                            Databuilder = null
                        };
                        detailsToAdd.Add(detail);
                    }
                    if (detailsToAdd.Any())
                    {
                        _context.Detailresultats.AddRange(detailsToAdd);
                    }
                }
                
                // Gérer les données de prélèvement si fournies
                try
                {
                    await SavePrelevementData(viewModel);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de la sauvegarde des données de prélèvement");
                    ModelState.AddModelError("DatePrelevement", "Erreur lors de la sauvegarde des données de prélèvement. Veuillez vérifier les dates saisies.");
                    await InitCreateViewDataOptimized();
                    return View(viewModel);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await InitCreateViewDataOptimized();
            return View(viewModel);
        }

        // GET: Enteteresultat/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            // Block access for users with "Caisse" role
            if (User.IsInRole(EnumRoles.Caisse.ToString()))
            {
                return Forbid();
            }

            // Allow access only for Technicien and Biologiste roles
            if (!User.IsInRole(EnumRoles.Technicien.ToString()) && !User.IsInRole(EnumRoles.Biologiste.ToString()))
            {
                return Forbid();
            }

            // Optimized: Load only necessary data with minimal includes + Builder info
            var entete = await _context.Enteteresultats
                .AsNoTracking()
                .Select(e => new
                {
                    e.Enteteresultatid,
                    e.Codesite,
                    e.Technicienid,
                    e.Entetedemandeid,
                    e.Date,
                    e.Interpretation,
                    e.Idanalyse,
                    Detailresultats = e.Detailresultats.Select(d => new
                    {
                        d.Parametreid,
                        d.Resultat,
                        d.Resultatsi,
                        d.Commentaire,
                        ParametreNom = d.Parametre.Nom,
                        ParametreCode = d.Parametre.Code,
                        UniteNom = d.Parametre.CodeuniteNavigation.Name,
                        UniteSiNom = d.Parametre.CodeunitesiNavigation.Name,
                        // Ajouter les informations du Builder
                        ParametreDatabuilder = d.Parametre.Valuebuilder,
                        ParametreFacteurConversion = d.Parametre.Facteurconversionsi
                    }).ToList()
                })
                .FirstOrDefaultAsync(e => e.Enteteresultatid == id);
                
            if (entete == null) return NotFound();

            // Optimized: Get laboratory info with single query
            var laboratoireId = await _context.Utilisateurlaboratoires
                .AsNoTracking()
                .Where(x => x.Utilisateurid == entete.Technicienid)
                .Select(x => x.IdlaboratoireNavigation.Idlaboratoire)
                .FirstOrDefaultAsync();

            var vm = new ProlabWeb.ViewModels.EnteteresultatEditVM
            {
                Enteteresultatid = entete.Enteteresultatid,
                Codesite = entete.Codesite,
                Technicienid = entete.Technicienid,
                Laboratoireid = laboratoireId,
                Entetedemandeid = entete.Entetedemandeid,
                Date = entete.Date,
                Interpretation = entete.Interpretation,
                Idanalyse = entete.Idanalyse,
                Parametres = entete.Detailresultats.Select(d => {
                    // Parser le Databuilder JSON pour extraire Type et Valeur
                    ProlabWeb.ViewModels.ParametreItemBuilderVM builderInfo = new ProlabWeb.ViewModels.ParametreItemBuilderVM();
                    if (!string.IsNullOrEmpty(d.ParametreDatabuilder))
                    {
                        try
                        {
                            builderInfo = System.Text.Json.JsonSerializer.Deserialize<ProlabWeb.ViewModels.ParametreItemBuilderVM>(d.ParametreDatabuilder);
                        }
                        catch (Exception ex)
                        {
                            // En cas d'erreur de parsing, log et continuer avec valeurs par défaut
                            _logger?.LogWarning(ex, "Erreur lors du parsing du Databuilder pour le paramètre {ParametreId}", d.Parametreid);
                            builderInfo = new ProlabWeb.ViewModels.ParametreItemBuilderVM { Type = "Texte", Valeur = "" };
                        }
                    }
                    
                    return new ProlabWeb.ViewModels.ParametreResultatVM
                    {
                        Parametreid = d.Parametreid,
                        Nom = d.ParametreNom ?? "",
                        Code = d.ParametreCode ?? "",
                        Unite = d.UniteNom ?? "",
                        Resultat = d.Resultat,
                        UniteSI = d.UniteSiNom ?? "",
                        Resultatsi = d.Resultatsi,
                        Commentaire = d.Commentaire,
                        Builder = builderInfo ?? new ProlabWeb.ViewModels.ParametreItemBuilderVM { Type = "Texte", Valeur = "" },
                        FacteurConversion = d.ParametreFacteurConversion?.ToString()
                    };
                }).ToList()
            };
            
            // Charger les données de prélèvement
            await LoadPrelevementData(vm);
            
            // Use optimized ViewData initialization
            await InitCreateViewDataOptimized(vm);
            return View(vm);
        }

        // POST: Enteteresultat/Edit/5 - Optimized version
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ProlabWeb.ViewModels.EnteteresultatEditVM vm)
        {
            // Block access for users with "Caisse" role
            if (User.IsInRole(EnumRoles.Caisse.ToString()))
            {
                return Forbid();
            }

            // Allow access only for Technicien and Biologiste roles
            if (!User.IsInRole(EnumRoles.Technicien.ToString()) && !User.IsInRole(EnumRoles.Biologiste.ToString()))
            {
                return Forbid();
            }

            // Vérification de la correspondance entre l'id de l'URL et celui du VM
            if (id != vm.Enteteresultatid)
                return NotFound();

            if (!ModelState.IsValid)
            {
                await InitCreateViewDataOptimized(vm);
                return View(vm);
            }

            // Optimized: Get user information in a single query
            var user = await _usermanager.GetUserAsync(User);
            var userInfo = user != null ? await _context.Utilisateurs
                .AsNoTracking()
                .Select(u => new
                {
                    u.Utilisateurid,
                    u.Userid,
                    Codesite = u.CodesiteNavigation.Codesite
                })
                .FirstOrDefaultAsync(x => x.Userid == user.Id) : null;

            // Optimized: Load entete with only necessary data
            var entete = await _context.Enteteresultats
                .Include(e => e.Detailresultats)
                .FirstOrDefaultAsync(e => e.Enteteresultatid == vm.Enteteresultatid);
            if (entete == null) return NotFound();
            // Update entete with user information
            entete.Codesite = userInfo?.Codesite;
            entete.Technicienid = userInfo?.Utilisateurid ?? Guid.Empty;
            entete.Entetedemandeid = vm.Entetedemandeid;
            entete.Date = DateTime.Now.AddMilliseconds(1); // Mettre à jour la date pour que les résultats modifiés remontent en tête
            entete.Interpretation = vm.Interpretation;
            entete.Idanalyse = vm.Idanalyse;

            var anciensDetails = entete.Detailresultats.ToList();
            var nouveauxParametres = vm.Parametres ?? new List<ProlabWeb.ViewModels.ParametreResultatVM>();

            // Suppression : ceux qui ne sont plus dans la nouvelle liste
            var aSupprimer = anciensDetails.Where(d => !nouveauxParametres.Any(p => p.Parametreid == d.Parametreid)).ToList();
            if (aSupprimer.Any())
                _context.Detailresultats.RemoveRange(aSupprimer);

            // Optimized: Get patient info with simplified query - only needed data
            var patientInfo = await _context.Entetedemandes
                .AsNoTracking()
                .Where(e => e.Entetedemandeid == vm.Entetedemandeid)
                .Select(e => new
                {
                    PatientDateNaissance = e.Patient.Datenaissance,
                    PatientSexe = e.Patient.Codesexe
                })
                .FirstOrDefaultAsync();
            
            int patientAge = 0;
            string patientSexe = "";
            if (patientInfo != null)
            {
                if (patientInfo.PatientDateNaissance != DateTime.MinValue)
                {
                    patientAge = DateTime.Now.Year - patientInfo.PatientDateNaissance.Year;
                    if (DateTime.Now.DayOfYear < patientInfo.PatientDateNaissance.DayOfYear)
                        patientAge--;
                }
                patientSexe = patientInfo.PatientSexe ?? "";
            }

            // Optimized: Batch process details for better performance
            var aAjouter = new List<Detailresultat>();
            var parametreIds = nouveauxParametres.Select(p => p.Parametreid).ToHashSet();
            
            foreach (var param in nouveauxParametres)
            {
                // Calculate comment using reference values
                string commentaireAuto = await CalculerCommentaireAutomatique(vm.Idanalyse, param.Resultat, patientAge, patientSexe, param.Parametreid, param.Nom);
                
                var existant = anciensDetails.FirstOrDefault(d => d.Parametreid == param.Parametreid);
                if (existant == null)
                {
                    aAjouter.Add(new Detailresultat
                    {
                        Detailresultatid = Guid.NewGuid(),
                        Enteteresultatid = entete.Enteteresultatid,
                        Parametreid = param.Parametreid,
                        Resultat = param.Resultat?.Replace(",", "."),
                        Resultatsi = param.Resultatsi?.Replace(",", "."),
                        Date = vm.Date,
                        Commentaire = commentaireAuto,
                        Databuilder = null
                    });
                }
                else
                {
                    // Only update if values have changed (reduces DB overhead)
                    var newResultat = param.Resultat?.Replace(",", ".");
                    var newResultatsi = param.Resultatsi?.Replace(",", ".");
                    
                    if (existant.Resultat != newResultat ||
                        existant.Resultatsi != newResultatsi ||
                        existant.Commentaire != commentaireAuto)
                    {
                        existant.Resultat = newResultat;
                        existant.Resultatsi = newResultatsi;
                        existant.Commentaire = commentaireAuto;
                        existant.Date = vm.Date;
                    }
                }
            }
            
            // Add new details in batch
            if (aAjouter.Any())
                _context.Detailresultats.AddRange(aAjouter);
                
                // Gérer les données de prélèvement
            try
            {
                await SavePrelevementData(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la sauvegarde des données de prélèvement");
                ModelState.AddModelError("DatePrelevement", "Erreur lors de la sauvegarde des données de prélèvement. Veuillez vérifier les dates saisies.");
                await InitCreateViewDataOptimized(vm);
                return View(vm);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Enteteresultat/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Enteteresultats == null)
            {
                return NotFound();
            }

            var enteteresultat = await _context.Enteteresultats
                .Include(e => e.CodesiteNavigation)
                .Include(e => e.Entetedemande)
                .Include(e => e.IdanalyseNavigation)
                .Include(e => e.Technicien)
                .FirstOrDefaultAsync(m => m.Enteteresultatid == id);
            if (enteteresultat == null)
            {
                return NotFound();
            }

            return View(enteteresultat);
        }

        // POST: Enteteresultat/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Enteteresultats == null)
            {
                return Problem("Entity set 'ProlabwebContext.Enteteresultats'  is null.");
            }
            var enteteresultat = await _context.Enteteresultats.FindAsync(id);
            if (enteteresultat != null)
            {
                _context.Enteteresultats.Remove(enteteresultat);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Suppression AJAX
        [HttpGet]
        public async Task<IActionResult> DeleteEnteteresultat(Guid id)
        {
            var response = new JsonResponseViewModel();
            try
            {
                var entete = await _context.Enteteresultats
                    .Include(e => e.Detailresultats)
                    .FirstOrDefaultAsync(e => e.Enteteresultatid == id);
                if (entete == null)
                {
                    response.success = false;
                    response.message = "L'élément n'existe pas.";
                    return Json(response);
                }
                // Supprimer d'abord les enfants Detailresultat
                if (entete.Detailresultats != null && entete.Detailresultats.Any())
                {
                    _context.Detailresultats.RemoveRange(entete.Detailresultats);
                }
                _context.Enteteresultats.Remove(entete);
                await _context.SaveChangesAsync();
                response.success = true;
                response.message = "Suppression réussie.";
            }
            catch (Exception ex)
            {
                response.success = false;
                response.message = $"Erreur lors de la suppression : {ex.Message}";
            }
            return Json(response);
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

        public static Cell CreateCell(string text, PdfFont font, float fontSize, TextAlignment alignment, float? fixedWidth = null, VerticalAlignment verticalAlignment = VerticalAlignment.MIDDLE, bool compact = false)
        {
            text ??= "";

            Paragraph paragraph = new Paragraph(text)
                .SetFont(font)
                .SetFontSize(fontSize)
                .SetTextAlignment(alignment);

            if (compact)
            {
                paragraph.SetMargin(0);
                paragraph.SetMultipliedLeading(1f);
            }

            Cell cell = new Cell()
                .Add(paragraph)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetVerticalAlignment(verticalAlignment);

            if (compact)
            {
                cell.SetPaddingTop(0);
                cell.SetPaddingBottom(0);
            }

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

        private static string FormatReferenceRange(Valeurreference referenceRange)
        {
            if (referenceRange == null)
                return "";

            string FormatValue(decimal? value)
            {
                if (!value.HasValue)
                    return "";

                return value.Value.ToString("#,0.##", CultureInfo.GetCultureInfo("fr-FR"));
            }

            var fromValue = FormatValue(referenceRange.Referencefromvalue);
            var toValue = FormatValue(referenceRange.Referencetovalue);

            // Fallback to SI values when standard bounds are not populated.
            if (string.IsNullOrWhiteSpace(fromValue) && string.IsNullOrWhiteSpace(toValue))
            {
                fromValue = FormatValue(referenceRange.Referencefromvaluesi);
                toValue = FormatValue(referenceRange.Referencetovaluesi);
            }

            if (!string.IsNullOrWhiteSpace(fromValue) && !string.IsNullOrWhiteSpace(toValue))
                return $"{fromValue} - {toValue}";

            if (!string.IsNullOrWhiteSpace(fromValue))
                return $">= {fromValue}";

            if (!string.IsNullOrWhiteSpace(toValue))
                return $"<= {toValue}";

            return "";
        }

        private static string NormalizeReferenceKey(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            var normalized = value.Trim().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC).ToUpperInvariant();
        }

        private static string NormalizePrintableText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            var sb = new StringBuilder(value.Length);
            foreach (var c in value)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(c);

                // Remove control/format characters (e.g. zero-width chars) that can create phantom blank rows.
                if (category == UnicodeCategory.Control ||
                    category == UnicodeCategory.Format ||
                    category == UnicodeCategory.LineSeparator ||
                    category == UnicodeCategory.ParagraphSeparator)
                {
                    continue;
                }

                sb.Append(char.IsWhiteSpace(c) ? ' ' : c);
            }

            var cleaned = sb.ToString().Trim();

            while (cleaned.Contains("  "))
            {
                cleaned = cleaned.Replace("  ", " ");
            }

            return cleaned;
        }


        [HttpGet]
        public async Task<IActionResult> GenererRapportAnalyse(Guid id)
        {
            try
            {
                using var stream = new MemoryStream();
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                
                // Supprimer toutes les métadonnées qui pourraient causer l'affichage de localhost
                var docInfo = pdf.GetDocumentInfo();
                docInfo.RemoveCreationDate();
                docInfo.SetCreator("");
                docInfo.SetProducer("");
                docInfo.SetAuthor("");
                docInfo.SetTitle("");
                docInfo.SetSubject("");
                docInfo.SetKeywords("");
                
                // Désactiver les métadonnées XMP
                pdf.GetCatalog().Remove(PdfName.Metadata);
                
                var document = new Document(pdf, new PageSize(new Rectangle(595, 421))); // A4 half-height
                document.SetMargins(8, 8, 8, 8); // Minimal margins

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

                var enteteResultat = _context.Enteteresultats
                .Include(e => e.Technicien)
                .Include(e => e.Biologiste)
                .Include(e => e.Entetedemande)
                    .ThenInclude(d => d.Patient)
                .Include(e => e.Entetedemande)
                    .ThenInclude(d => d.Prescripteur)
                .Include(e => e.IdanalyseNavigation)
                    .ThenInclude(e => e.IdnatureechantillonNavigation)
                .Include(a => a.IdanalyseNavigation.IdlaboratoireNavigation)
                .FirstOrDefault(e => e.Enteteresultatid == id);
                if (enteteResultat == null)
                    return NotFound("Entête de résultat introuvable.");

                var patient = enteteResultat.Entetedemande?.Patient;
                var prescripteur = enteteResultat.Entetedemande?.Prescripteur;
                var laboratoire = enteteResultat.IdanalyseNavigation?.IdlaboratoireNavigation;
                var biologiste = enteteResultat.Biologiste;
                var technicien = enteteResultat.Technicien;
                
                // Récupérer la signature du biologiste si elle existe
                Signatureutilisateur biologisteSignature = null;
                if (biologiste != null)
                {
                    biologisteSignature = _context.Signatureutilisateurs
                        .Where(s => s.Utilisateurid == biologiste.Utilisateurid)
                        .FirstOrDefault();
                }

                string age = "";
                if (patient != null && patient.Datenaissance != DateTime.MinValue)
                {
                    age = ProlabWeb.Helpers.Utilities.CalculerAgeEnAnneeMoisJour(patient.Datenaissance);
                }

                var biologistenom = biologiste?.Nom ?? "";
                var techniciennom = technicien?.Nom ?? "";
                var numero = enteteResultat.Entetedemande?.Numero ?? "";
                var laboratoirenom = laboratoire?.Nom ?? "";
                var nom = patient?.Nom ?? "";
                var prenom = patient?.Prenom ?? "";
                var datenaissance = patient != null ? patient.Datenaissance.ToString("dd/MM/yyyy") : "";
                var prescripteurnom = prescripteur?.Nom ?? "";
                var prescripteurtel = prescripteur?.Tel ?? "";
                var interpretation = enteteResultat.Interpretation ?? "";
                var analysenom = enteteResultat.IdanalyseNavigation?.Nom ?? "";
                var natureechantillon = enteteResultat.IdanalyseNavigation?.IdnatureechantillonNavigation?.Nom ?? "";
                var enteteresultatid = enteteResultat.Enteteresultatid.ToString();
                var analysecodification = enteteResultat.IdanalyseNavigation?.Codification ?? "";
                var analyseindicerev = enteteResultat.IdanalyseNavigation?.Indicederev ?? "";
                var analyseaccredite = enteteResultat.IdanalyseNavigation?.Accredite ?? false;
                var dateapplication = "";
                var date = DateTime.Now;
                var dateimpression = $"{date.ToString("dd/MM/yyyy")} {date.ToShortTimeString()}";
                var patientAge = 0;
                var patientSexe = patient?.Codesexe ?? "";

                if (patient != null && patient.Datenaissance != DateTime.MinValue)
                {
                    patientAge = DateTime.Now.Year - patient.Datenaissance.Year;
                    if (DateTime.Now.DayOfYear < patient.Datenaissance.DayOfYear)
                        patientAge--;
                }

                var allReferenceRanges = await _context.Valeurreferences
                    .AsNoTracking()
                    .Where(vr => vr.Idanalyse == enteteResultat.Idanalyse)
                    .OrderBy(vr => vr.Agedebut)
                    .ThenBy(vr => vr.Titre)
                    .ToListAsync();

                var referenceRanges = allReferenceRanges
                    .Where(vr =>
                        string.IsNullOrEmpty(patientSexe) ||
                        vr.Sexeautorisecode == patientSexe ||
                        (vr.Sexeautorisecode == "H" && patientSexe == "M") ||
                        (vr.Sexeautorisecode == "F" && patientSexe == "F") ||
                        vr.Sexeautorisecode == "HF")
                    .Where(vr =>
                        patientAge == 0 ||
                        (vr.Agedebut == null || patientAge >= vr.Agedebut) &&
                        (vr.Agefin == null || patientAge <= vr.Agefin))
                    .ToList();

                // Fallback: if strict demographic filter returns nothing, use all reference rows for the analysis.
                if (!referenceRanges.Any())
                    referenceRanges = allReferenceRanges;

                var referenceDisplay = FormatReferenceRange(referenceRanges.FirstOrDefault());

                string NormalizeKey(string value)
                {
                    return NormalizeReferenceKey(value);
                }

                var referenceRangesByKey = referenceRanges
                    .Where(vr => !string.IsNullOrWhiteSpace(vr.Titre))
                    .GroupBy(vr => NormalizeKey(vr.Titre))
                    .ToDictionary(g => g.Key, g => g.First());

                var detailResultats = _context.Detailresultats
                    .AsNoTracking()
                    .Include(d => d.Parametre)
                        .ThenInclude(p => p.CodeuniteNavigation)
                    .Include(d => d.Parametre)
                        .ThenInclude(p => p.CodeunitesiNavigation)
                    .Where(d => d.Enteteresultatid == enteteResultat.Enteteresultatid)
                    .Select(d => new
                    {
                        d.Resultat,
                        d.Resultatsi,
                        d.Commentaire,
                        d.Detailresultatid,
                        Nom = d.Parametre.Nom,
                        Code = d.Parametre.Code,
                        CodeUnite = d.Parametre.Codeunite,
                        Unite = d.Parametre.CodeuniteNavigation != null ? d.Parametre.CodeuniteNavigation.Name : "",
                        CodeUnitesi = d.Parametre.Codeunitesi,
                        Unitesi = d.Parametre.CodeunitesiNavigation != null ? d.Parametre.CodeunitesiNavigation.Name : "",
                        d.Enteteresultatid
                    })
                    .OrderBy(x => x.Nom)
                    .ToList();


                // ========== ENTÊTE ==========
                // Adjust header structure to include accreditation logo when needed
                var tableEntete = new Table(new float[]
                {
                    CmToPt(2.75f), // Col 1 - Laboratory logo
                    CmToPt(10.5f),  // Col 2 - Title and info
                    analyseaccredite ? CmToPt(3.75f) : CmToPt(5.75f), // Col 3 - Info table
                    analyseaccredite ? CmToPt(2f) : 0f  // Col 4 - Accreditation logo (conditional)
                }).UseAllAvailableWidth().SetBorder(Border.NO_BORDER);

                // Colonne 1 : Laboratory Logo
                var imageData = ImageDataFactory.Create("wwwroot/img/inh_togo_logo.png");
                var logo = new Image(imageData)
                    .SetWidth(CmToPt(1.5f))
                    .SetHeight(CmToPt(1f))
                    .SetHorizontalAlignment(HorizontalAlignment.LEFT);
                tableEntete.AddCell(new Cell().Add(logo).SetBorder(Border.NO_BORDER));

                // Colonne 2 : Titre
                tableEntete.AddCell(CreateCell("RAPPORT D'ANALYSE", fontNormal, 11, TextAlignment.CENTER));

                // Colonne 3 : Tableau imbriqué (informations)
                // Table principale : 3 lignes, 1 colonne
                Table col3InnerTable = new Table(1).UseAllAvailableWidth();
                col3InnerTable.SetBorder(Border.NO_BORDER);

                // el1 : Codification
                Table el1 = new Table(new float[] { CmToPt(2.25f), analyseaccredite ? CmToPt(1.5f) : CmToPt(3.5f) }).UseAllAvailableWidth();
                el1.SetBorder(Border.NO_BORDER);
                el1.AddCell(CreateCell("Codification :", fontNormal, 9, TextAlignment.LEFT));
                el1.AddCell(CreateCell(analysecodification, fontNormal, 9, TextAlignment.LEFT));
                col3InnerTable.AddCell(new Cell().Add(el1).SetBorder(Border.NO_BORDER));

                // el2 : Indice de rev
                Table el2 = new Table(new float[] { CmToPt(2.25f), analyseaccredite ? CmToPt(1.5f) : CmToPt(3.5f) }).UseAllAvailableWidth();
                el2.SetBorder(Border.NO_BORDER);
                el2.AddCell(CreateCell("Indice de rev :", fontNormal, 9, TextAlignment.LEFT));
                el2.AddCell(CreateCell(analyseindicerev, fontNormal, 9, TextAlignment.LEFT));
                col3InnerTable.AddCell(new Cell().Add(el2).SetBorder(Border.NO_BORDER));

                // el3 : Date d'application
                Table el3 = new Table(new float[] { CmToPt(2.5f), analyseaccredite ? CmToPt(1.25f) : CmToPt(2.75f) }).UseAllAvailableWidth();
                el3.SetBorder(Border.NO_BORDER);
                el3.AddCell(CreateCell("Date d'application :", fontNormal, 9, TextAlignment.LEFT));
                el3.AddCell(CreateCell(dateapplication, fontNormal, 9, TextAlignment.LEFT));
                col3InnerTable.AddCell(new Cell().Add(el3).SetBorder(Border.NO_BORDER));

                tableEntete.AddCell(new Cell().Add(col3InnerTable).SetBorder(Border.NO_BORDER));

                // Colonne 4 : Logo d'accréditation (conditionnel)
                if (analyseaccredite)
                {
                    // Créer une table interne pour le logo et le texte d'accréditation
                    var accreditationTable = new Table(1).UseAllAvailableWidth().SetBorder(Border.NO_BORDER);
                    
                    // Logo d'accréditation
                    var accreditationImageData = ImageDataFactory.Create("wwwroot/img/accreditation.jpg");
                    var accreditationLogo = new Image(accreditationImageData)
                        .SetWidth(CmToPt(1.6f))
                        .SetHeight(CmToPt(1.6f))
                        .SetHorizontalAlignment(HorizontalAlignment.CENTER);
                    
                    accreditationTable.AddCell(new Cell().Add(accreditationLogo)
                        .SetBorder(Border.NO_BORDER)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetPaddingBottom(CmToPt(0.1f)));
                    
                    // Texte "Accréditation N° OM19001"
                    accreditationTable.AddCell(CreateCell("Accréditation N° OM19001", fontNormal, 8, TextAlignment.CENTER)
                        .SetPaddingTop(0)
                        .SetPaddingBottom(CmToPt(0.05f)));
                    
                    // Texte "ISO 15189 : 2012"
                    accreditationTable.AddCell(CreateCell("ISO 15189 : 2012", fontNormal, 8, TextAlignment.CENTER)
                        .SetPaddingTop(0));
                    
                    tableEntete.AddCell(new Cell().Add(accreditationTable).SetBorder(Border.NO_BORDER));
                }

                document.Add(tableEntete);

                // Ligne 2 de l'entête (adresse)
                var tableAdresse = new Table(new float[] { CmToPt(4.5f), CmToPt(14.5f) }).UseAllAvailableWidth();
                tableAdresse.SetBorder(Border.NO_BORDER);
                tableAdresse.AddCell(CreateCell("BP : 1396 Tél : 22 21 06 33 Lomé-Togo", fontNormal, 8, TextAlignment.LEFT));
                tableAdresse.AddCell(new Cell().SetBorder(Border.NO_BORDER));
                document.Add(tableAdresse);

                // Espace vide minimal
                document.Add(new Paragraph().SetMarginTop(CmToPt(0.1f)));

                // ========== IDENTIFICATION PATIENT / LABO ==========

                var tableIdent = new Table(new float[] {
                    CmToPt(9.75f),
                    CmToPt(0.5f),
                    CmToPt(8.75f)
                }).UseAllAvailableWidth().SetBorder(Border.NO_BORDER);

                // COLONNE 1 : Infos patient
                // Créer le tableau de gauche (col1), 4 lignes 1 colonne, sans bordure
                Table col1 = new Table(1).UseAllAvailableWidth();
                col1.SetBorder(iText.Layout.Borders.Border.NO_BORDER);

                // Ligne 1 : "Biologistes : [valeur]"
                Table row1 = new Table(new float[] { CmToPt(2.5f), CmToPt(7.25f) }).UseAllAvailableWidth();
                row1.SetBorder(iText.Layout.Borders.Border.NO_BORDER);
                row1.AddCell(CreateCell("Biologistes :", fontBold, 9, TextAlignment.LEFT));
                row1.AddCell(CreateCell(biologistenom, fontNormal, 9, TextAlignment.LEFT));
                col1.AddCell(new Cell().Add(row1).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                // Ligne 2 : "Responsable technique : [valeur]"
                Table row2 = new Table(new float[] { CmToPt(4.5f), CmToPt(5.25f) }).UseAllAvailableWidth();
                row2.SetBorder(iText.Layout.Borders.Border.NO_BORDER);
                row2.AddCell(CreateCell("Responsable technique :", fontBold, 9, TextAlignment.LEFT));
                row2.AddCell(CreateCell(techniciennom, fontNormal, 9, TextAlignment.LEFT));
                col1.AddCell(new Cell().Add(row2).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                // Ligne 3 : "Dossier N° : [valeur]"
                Table row3 = new Table(new float[] { CmToPt(2.5f), CmToPt(7.25f) }).UseAllAvailableWidth();
                row3.SetBorder(iText.Layout.Borders.Border.NO_BORDER);
                row3.AddCell(CreateCell("Dossier N° :", fontBold, 9, TextAlignment.LEFT));
                row3.AddCell(CreateCell(numero, fontNormal, 9, TextAlignment.LEFT));
                col1.AddCell(new Cell().Add(row3).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                // Ligne 4 : vide
                col1.AddCell(new Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                tableIdent.AddCell(new Cell().Add(col1).SetBorder(Border.NO_BORDER));
                tableIdent.AddCell(new Cell().SetBorder(Border.NO_BORDER)); // colonne vide

                // COLONNE 3 : Infos laboratoire
                var col3 = new Table(1).SetBorder(Border.NO_BORDER);

                var labo1 = new Table(new float[] { CmToPt(3.5f), CmToPt(5.25f) }).SetBorder(Border.NO_BORDER);
                labo1.AddCell(CreateCell("LABORATOIRE :", fontBold, 9, TextAlignment.LEFT));
                labo1.AddCell(CreateCell(laboratoirenom, fontNormal, 9, TextAlignment.LEFT));
                col3.AddCell(new Cell().Add(labo1).SetBorder(Border.NO_BORDER));
                col3.AddCell(CreateCell(analysenom, fontNormal, 9, TextAlignment.LEFT));
                col3.AddCell(CreateCell($"{nom} {prenom}", fontNormal, 9, TextAlignment.LEFT));
                col3.AddCell(CreateCell($"Né(e) le {datenaissance}, {age}", fontNormal, 9, TextAlignment.LEFT));

                tableIdent.AddCell(new Cell().Add(col3).SetBorder(Border.NO_BORDER));
                document.Add(tableIdent);

                // Bloc : Infos Prélèvement / Registre / Code-barres

                // Espace vide 0.25 cm
                document.Add(new Paragraph().SetMarginTop(CmToPt(0.25f)));

                // === TABLE INFOS PRÉLÈVEMENT / REGISTRE / CODE BARRE ===
                var tableInfo = new Table(new float[] {
                    CmToPt(7.5f),
                    CmToPt(6.75f),
                    CmToPt(4.75f)
                }).UseAllAvailableWidth();

                // === COLONNE 1 : Infos prélèvement ===
                // Créer la table principale (4 lignes, 1 colonne)
                Table col1Info = new Table(1).UseAllAvailableWidth();
                col1Info.SetBorder(iText.Layout.Borders.Border.NO_BORDER);

                // Ligne 1 : Prélèvement
                Table ligne1 = new Table(new float[] { CmToPt(2.75f), CmToPt(4.75f) }).UseAllAvailableWidth();
                ligne1.SetBorder(iText.Layout.Borders.Border.NO_BORDER);
                ligne1.AddCell(CreateCell("Prélèvement :", fontBold, 10, TextAlignment.LEFT));
                ligne1.AddCell(CreateCell("", fontNormal, 10, TextAlignment.LEFT));
                col1Info.AddCell(new Cell().Add(ligne1).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                // Ligne 2 : Réception au labo
                Table ligne2 = new Table(new float[] { CmToPt(3.5f), CmToPt(4f) }).UseAllAvailableWidth();
                ligne2.SetBorder(iText.Layout.Borders.Border.NO_BORDER);
                ligne2.AddCell(CreateCell("Réception au labo :", fontBold, 10, TextAlignment.LEFT));
                ligne2.AddCell(CreateCell("", fontNormal, 10, TextAlignment.LEFT));
                col1Info.AddCell(new Cell().Add(ligne2).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                // Ligne 3 : Edité le
                Table ligne3 = new Table(new float[] { CmToPt(2f), CmToPt(5.5f) }).UseAllAvailableWidth();
                ligne3.SetBorder(iText.Layout.Borders.Border.NO_BORDER);
                ligne3.AddCell(CreateCell("Edité le :", fontBold, 10, TextAlignment.LEFT));
                ligne3.AddCell(CreateCell("", fontNormal, 10, TextAlignment.LEFT));
                col1Info.AddCell(new Cell().Add(ligne3).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                // Ligne 4 : Qualité de l'échantillon
                Table ligne4 = new Table(new float[] { CmToPt(4.5f), CmToPt(3f) }).UseAllAvailableWidth();
                ligne4.SetBorder(iText.Layout.Borders.Border.NO_BORDER);
                ligne4.AddCell(CreateCell("Qualité de l'échantillon :", fontBold, 10, TextAlignment.LEFT));
                ligne4.AddCell(CreateCell("", fontNormal, 10, TextAlignment.LEFT));
                col1Info.AddCell(new Cell().Add(ligne4).SetBorder(iText.Layout.Borders.Border.NO_BORDER));

                tableInfo.AddCell(new Cell().Add(col1Info).SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                // === COLONNE 2 : Infos registre ===
                // Table principale : 4 lignes, 1 colonne
                Table col2Info = new Table(1).UseAllAvailableWidth();
                col2Info.SetBorder(Border.NO_BORDER);

                // Élément 1 : N° régistre
                Table element1 = new Table(new float[] { CmToPt(2.5f), CmToPt(4.25f) }).UseAllAvailableWidth();
                element1.SetBorder(Border.NO_BORDER);
                element1.AddCell(CreateCell("N° régistre :", fontBold, 10, TextAlignment.LEFT));
                element1.AddCell(CreateCell("", fontNormal, 10, TextAlignment.LEFT));
                col2Info.AddCell(new Cell().Add(element1).SetBorder(Border.NO_BORDER));

                // Élément 2 : Transmis à
                Table element2 = new Table(new float[] { CmToPt(2.5f), CmToPt(4.25f) }).UseAllAvailableWidth();
                element2.SetBorder(Border.NO_BORDER);
                element2.AddCell(CreateCell("Transmis à :", fontBold, 10, TextAlignment.LEFT));
                element2.AddCell(CreateCell("", fontNormal, 10, TextAlignment.LEFT));
                col2Info.AddCell(new Cell().Add(element2).SetBorder(Border.NO_BORDER));

                // Élément 3 : Prescripteur
                Table element3 = new Table(new float[] { CmToPt(2.75f), CmToPt(4f) }).UseAllAvailableWidth();
                element3.SetBorder(Border.NO_BORDER);
                element3.AddCell(CreateCell("Prescripteur :", fontBold, 10, TextAlignment.LEFT));
                element3.AddCell(CreateCell(prescripteurnom, fontNormal, 10, TextAlignment.LEFT));
                col2Info.AddCell(new Cell().Add(element3).SetBorder(Border.NO_BORDER));

                // Élément 4 : Tél.
                Table element4 = new Table(new float[] { CmToPt(1.5f), CmToPt(5.25f) }).UseAllAvailableWidth();
                element4.SetBorder(Border.NO_BORDER);
                element4.AddCell(CreateCell("Tél. :", fontBold, 10, TextAlignment.LEFT));
                element4.AddCell(CreateCell(prescripteurtel, fontNormal, 10, TextAlignment.LEFT));
                col2Info.AddCell(new Cell().Add(element4).SetBorder(Border.NO_BORDER));

                tableInfo.AddCell(new Cell().Add(col2Info).SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                // === COLONNE 3 : Code-barres + texte ===
                var col3Info = new Table(1).SetBorder(Border.NO_BORDER);
                var codeBarreTable = new Table(1).UseAllAvailableWidth().SetBorder(Border.NO_BORDER);
                var codeBarreImage = new Image(ImageDataFactory.Create("wwwroot/img/barcode.jpg"))
                    .SetHeight(CmToPt(1f))
                    .SetAutoScale(true);
                codeBarreTable.AddCell(new Cell().Add(codeBarreImage).SetBorder(Border.NO_BORDER).SetTextAlignment(TextAlignment.CENTER));
                codeBarreTable.AddCell(CreateCell("", fontNormal, 10, TextAlignment.CENTER));

                col3Info.AddCell(new Cell().Add(codeBarreTable).SetBorder(Border.NO_BORDER).SetVerticalAlignment(VerticalAlignment.MIDDLE));
                tableInfo.AddCell(new Cell().Add(col3Info).SetBorder(new SolidBorder(ColorConstants.BLACK, 1)));

                document.Add(tableInfo);


                // Bloc : Nature de l’échantillon

                // Espace vide 0.25 cm
                document.Add(new Paragraph().SetMarginTop(CmToPt(0.25f)));

                var natureText = $"{analysenom} - Nature de l'échantillon : {natureechantillon}";
                document.Add(new Paragraph(natureText)
                    .SetFont(fontBold)
                    .SetFontSize(12)
                    .SetTextAlignment(TextAlignment.LEFT));


                // Bloc : Résultats d’analyse (table 5 colonnes)

                // Espace vide minimal
                document.Add(new Paragraph().SetMarginTop(CmToPt(0.05f)));

                var resultTable = new Table(new float[] {
                    CmToPt(4.75f), CmToPt(3f), CmToPt(3f),
                    CmToPt(6f), CmToPt(4.25f)
                }).UseAllAvailableWidth();

                // En-tête
                string[] headers = { "Paramètre", "Unité", "Résultat", "Valeur de référence", "Commentaire" };
                foreach (var header in headers)
                {
                    resultTable.AddCell(CreateCell(header, fontBold, 8, TextAlignment.LEFT));
                }

                // Une ligne de données (à dupliquer dynamiquement)
                var rowIndex = 0;
                foreach (var item in detailResultats)
                {
                    var paramName = NormalizePrintableText(item.Nom);
                    var paramCode = NormalizePrintableText(item.Code);
                    var unitLabel = NormalizePrintableText(item.Unite);
                    var resultValue = NormalizePrintableText(item.Resultat);
                    var commentValue = NormalizePrintableText(item.Commentaire);

                    if (string.IsNullOrWhiteSpace(paramName))
                    {
                        continue;
                    }

                    // Ignore blank/synthetic rows to avoid visual gaps in the report.
                    if (string.IsNullOrWhiteSpace(paramName) &&
                        string.IsNullOrWhiteSpace(unitLabel) &&
                        string.IsNullOrWhiteSpace(resultValue) &&
                        string.IsNullOrWhiteSpace(commentValue))
                    {
                        continue;
                    }

                    // Defensive guard for accidental header-like rows coming from data.
                    if (string.Equals(paramName, "Paramètre", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(paramName, "Parametre", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var matchedReference = referenceRangesByKey.TryGetValue(NormalizeKey(paramName), out var referenceByName)
                        ? referenceByName
                        : referenceRangesByKey.TryGetValue(NormalizeKey(paramCode), out var referenceByCode)
                            ? referenceByCode
                            : null;

                    if (matchedReference == null)
                    {
                        if (referenceRanges.Count == 1)
                        {
                            matchedReference = referenceRanges[0];
                        }
                        else if (rowIndex < referenceRanges.Count)
                        {
                            matchedReference = referenceRanges[rowIndex];
                        }
                    }

                    resultTable.AddCell(CreateCell(paramName, fontNormal, 8, TextAlignment.LEFT));
                    resultTable.AddCell(CreateCell(unitLabel, fontNormal, 8, TextAlignment.LEFT));
                    resultTable.AddCell(CreateCell(resultValue, fontNormal, 8, TextAlignment.LEFT));
                    resultTable.AddCell(CreateCell(FormatReferenceRange(matchedReference) ?? referenceDisplay, fontNormal, 8, TextAlignment.LEFT));
                    resultTable.AddCell(CreateCell(commentValue, fontNormal, 8, TextAlignment.LEFT));

                    rowIndex++;
                }

                document.Add(resultTable);


                // Bloc : Interprétation

                // Espace vide minimal
                document.Add(new Paragraph().SetMarginTop(CmToPt(0.05f)));

                var tableInterp = new Table(new float[] {
                    CmToPt(11f), CmToPt(7f)
                }).UseAllAvailableWidth().SetBorder(Border.NO_BORDER);

                // Création de la table 2 lignes, 1 colonne, sans bordure
                Table interpInner = new Table(1).UseAllAvailableWidth();
                interpInner.SetBorder(Border.NO_BORDER);

                // Ligne 1 : "Interprétation :" (gras + souligné)
                // On crée d'abord la cellule avec CreateCell, puis on ajoute le soulignement au paragraphe
                Cell cell1 = CreateCell("Interprétation :", fontBold, 9, TextAlignment.LEFT, null, VerticalAlignment.MIDDLE);

                // Pour ajouter le soulignement, on récupère le paragraphe dans la cellule et on le modifie
                Paragraph paragraph1 = cell1.GetChildren().OfType<Paragraph>().FirstOrDefault();
                if (paragraph1 != null)
                {
                    paragraph1.SetUnderline();
                }

                interpInner.AddCell(cell1);

                // Ligne 2 : "[valeur]" (normal)
                Cell cell2 = CreateCell(interpretation, fontNormal, 9, TextAlignment.LEFT, null, VerticalAlignment.MIDDLE);
                interpInner.AddCell(cell2);

                tableInterp.AddCell(new Cell().Add(interpInner).SetBorder(Border.NO_BORDER));
                tableInterp.AddCell(new Cell().SetBorder(Border.NO_BORDER));

                document.Add(tableInterp);


                // Bloc : Signature

                // Espace vide minimal
                document.Add(new Paragraph().SetMarginTop(CmToPt(0.05f)));

                var tableSign = new Table(new float[] {
                    CmToPt(11f), CmToPt(7f)
                }).UseAllAvailableWidth().SetBorder(Border.NO_BORDER).SetKeepTogether(true);

                // Première colonne : Date
                tableSign.AddCell(CreateCell($"Lomé le, {dateimpression}", fontNormal, 9, TextAlignment.LEFT));

                // Deuxième colonne : Signature et nom du biologiste
                var signatureCell = new Cell().SetBorder(Border.NO_BORDER);
                
                // Créer une table interne pour organiser la signature
                var signatureInnerTable = new Table(1).UseAllAvailableWidth().SetBorder(Border.NO_BORDER).SetKeepTogether(true);
                
                // Titre "Le Biologiste"
                signatureInnerTable.AddCell(CreateCell("Le Biologiste", fontBold, 9, TextAlignment.CENTER));
                
                // Ajouter la signature si elle existe
                if (biologisteSignature != null && biologisteSignature.Image != null && biologisteSignature.Image.Length > 0)
                {
                    try
                    {
                        var signatureImage = new Image(ImageDataFactory.Create(biologisteSignature.Image))
                            .SetMaxWidth(CmToPt(3f))
                            .SetMaxHeight(CmToPt(1.5f))
                            .SetHorizontalAlignment(HorizontalAlignment.CENTER);
                        
                        // Cellule pour l'image de signature
                        var imageCell = new Cell().Add(signatureImage)
                            .SetBorder(Border.NO_BORDER)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetPaddingTop(CmToPt(0.1f))
                            .SetPaddingBottom(CmToPt(0.1f));
                        
                        signatureInnerTable.AddCell(imageCell);
                    }
                    catch (Exception ex)
                    {
                        // En cas d'erreur avec l'image, ajouter un espace vide
                        _logger.LogWarning($"Erreur lors de l'ajout de la signature: {ex.Message}");
                        signatureInnerTable.AddCell(new Cell().SetHeight(CmToPt(1.2f)).SetBorder(Border.NO_BORDER));
                    }
                }
                else
                {
                    // Pas de signature - ajouter un espace vide
                    signatureInnerTable.AddCell(new Cell().SetHeight(CmToPt(1.2f)).SetBorder(Border.NO_BORDER));
                }
                
                // Nom du biologiste en bas
                signatureInnerTable.AddCell(CreateCell(biologistenom, fontBold, 9, TextAlignment.CENTER));
                
                signatureCell.Add(signatureInnerTable);
                tableSign.AddCell(signatureCell);

                document.Add(tableSign);

                // Add footer with site and date
                document.Add(new Paragraph().SetMarginTop(CmToPt(0.05f)));
                
                // Get site information for footer
                var site = _context.Sites
                    .AsNoTracking()
                    .FirstOrDefault(s => s.Codesite == enteteResultat.Entetedemande.Codesite);
                var siteName = site?.Name ?? "Site";
                var footerText = $"Fait à {siteName} le {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}";
                
                Table footerTable = new Table(1).UseAllAvailableWidth();
                footerTable.AddCell(CreateCell(footerText, fontNormal, 8, TextAlignment.CENTER));
                document.Add(footerTable);

                document.Close();

                string base64 = Convert.ToBase64String(stream.ToArray());

                return Json(new
                {
                    success = true,
                    pdfBase64 = base64
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "erreur lors de la génération de rapport");
                throw;
            }
        }

        //[HttpGet]
        //public IActionResult RapportAnalyseImage(Guid id)
        //{
        //    var reportPath = Path.Combine(_webHostEnvironment.WebRootPath, "reports", "rapportanalyse.frx");
        //    if (!System.IO.File.Exists(reportPath))
        //        return NotFound("Le rapport n'existe pas.");

        //    var entete = _context.Enteteresultats
        //        .Include(e => e.Technicien)
        //        .Include(e => e.Biologiste)
        //        .Include(e => e.Entetedemande)
        //            .ThenInclude(d => d.Patient)
        //        .Include(e => e.Entetedemande)
        //            .ThenInclude(d => d.Prescripteur)
        //        .Include(e => e.IdanalyseNavigation)
        //            .ThenInclude(a => a.IdlaboratoireNavigation)
        //        .FirstOrDefault(e => e.Enteteresultatid == id);
        //    if (entete == null)
        //        return NotFound("Entête de résultat introuvable.");

        //    var patient = entete.Entetedemande?.Patient;
        //    var prescripteur = entete.Entetedemande?.Prescripteur;
        //    var laboratoire = entete.IdanalyseNavigation?.IdlaboratoireNavigation;
        //    var biologiste = entete.Biologiste;
        //    var technicien = entete.Technicien;

        //    string age = "";
        //    if (patient != null && patient.Datenaissance != DateTime.MinValue)
        //    {
        //        age = ProlabWeb.Helpers.Utilities.CalculerAgeEnAnneeMoisJour(patient.Datenaissance);
        //    }

        //    using (var report = new FastReport.Report())
        //    {
        //        report.Load(reportPath);
        //        report.SetParameterValue("biologiste", biologiste?.Nom ?? "");
        //        report.SetParameterValue("technicien", technicien?.Nom ?? "");
        //        report.SetParameterValue("numero", entete.Entetedemande?.Numero ?? "");
        //        report.SetParameterValue("laboratoirenom", laboratoire?.Nom ?? "");
        //        report.SetParameterValue("nom", patient?.Nom ?? "");
        //        report.SetParameterValue("prenom", patient?.Prenom ?? "");
        //        report.SetParameterValue("datenaissance", patient != null ? patient.Datenaissance.ToString("dd/MM/yyyy") : "");
        //        report.SetParameterValue("age", age);
        //        report.SetParameterValue("prescripteurnom", prescripteur?.Nom ?? "");
        //        report.SetParameterValue("prescripteurtel", prescripteur?.Tel ?? "");
        //        report.SetParameterValue("interpretation", entete.Interpretation ?? "");
        //        report.SetParameterValue("analysenom", entete.IdanalyseNavigation?.Nom ?? "");
        //        report.SetParameterValue("enteteresultatid", entete.Enteteresultatid.ToString());
        //        report.SetParameterValue("analysecodification", entete.IdanalyseNavigation?.Codification ?? "");
        //        report.SetParameterValue("analyseindicerev", entete.IdanalyseNavigation?.Indicederev ?? "");

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
        public async Task<IActionResult> GetReferenceRanges(Guid analyseId, int age = 0, string sexe = "", string peau = "")
        {
            try
            {
                var referenceRanges = await _context.Valeurreferences
                    .Where(vr => vr.Idanalyse == analyseId)
                    .Where(vr => 
                        // Filtrer par sexe si spécifié
                        string.IsNullOrEmpty(sexe) || vr.Sexeautorisecode == sexe ||
                        vr.Sexeautorisecode == "H" && sexe == "M" ||
                        vr.Sexeautorisecode == "F" && sexe == "F" ||
                        vr.Sexeautorisecode == "HF" // Valable pour tous
                    )
                    .Where(vr => 
                        // Filtrer par âge si spécifié
                        age == 0 || 
                        (vr.Agedebut == null || age >= vr.Agedebut) &&
                        (vr.Agefin == null || age <= vr.Agefin)
                    )
                    .Select(vr => new
                    {
                        vr.Valeurreferenceid,
                        vr.Referencefromvalue,
                        vr.Referencetovalue,
                        vr.Referencefromvaluesi,
                        vr.Referencetovaluesi,
                        vr.Titre,
                        vr.Sexeautorisecode,
                        vr.Agedebut,
                        vr.Agefin
                    })
                    .ToListAsync();

                return Json(new { success = true, data = referenceRanges });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des valeurs de référence pour l'analyse {AnalyseId}", analyseId);
                return Json(new { success = false, message = "Erreur lors du chargement des valeurs de référence" });
            }
        }

        /// <summary>
        /// Calcule automatiquement le commentaire (Haut/Bas/Normal) basé sur les valeurs de référence
        /// </summary>
        /// <param name="analyseId">ID de l'analyse</param>
        /// <param name="resultatStr">Résultat sous forme de string</param>
        /// <param name="age">Âge du patient</param>
        /// <param name="sexe">Sexe du patient</param>
        /// <param name="parametreId">ID du paramètre (optionnel)</param>
        /// <param name="parametreNom">Nom du paramètre (optionnel)</param>
        /// <returns>Commentaire calculé (Haut/Bas/Normal)</returns>
        private async Task<string> CalculerCommentaireAutomatique(Guid analyseId, string resultatStr, int age, string sexe, Guid? parametreId = null, string parametreNom = null)
        {
            try
            {
                // Si pas de résultat, retourner vide
                if (string.IsNullOrWhiteSpace(resultatStr))
                    return "";
                
                // Essayer de parser le résultat en nombre (culture invariante)
                if (!decimal.TryParse(resultatStr.Replace(",", "."), System.Globalization.NumberStyles.Float, 
                    System.Globalization.CultureInfo.InvariantCulture, out decimal resultat))
                {
                    // Si ce n'est pas un nombre, retourner Normal par défaut
                    return "Normal";
                }
                
                // Récupérer les valeurs de référence appropriées pour cette analyse, âge et sexe
                _logger.LogInformation("Recherche valeurs référence pour analyseId: {AnalyseId}, age: {Age}, sexe: {Sexe}, resultat: {Resultat}", analyseId, age, sexe, resultat);
                
                // D'abord, chercher toutes les valeurs de référence pour cette analyse
                var allReferenceRanges = await _context.Valeurreferences
                    .AsNoTracking()
                    .Where(vr => vr.Idanalyse == analyseId)
                    .ToListAsync();
                    
                _logger.LogInformation("Trouvé {Count} valeurs de référence pour l'analyse {AnalyseId}", allReferenceRanges.Count, analyseId);
                
                var referenceRange = allReferenceRanges
                    .Where(vr => 
                        // Filtrer par sexe si spécifié
                        string.IsNullOrEmpty(sexe) || 
                        vr.Sexeautorisecode == sexe ||
                        (vr.Sexeautorisecode == "H" && sexe == "M") ||
                        (vr.Sexeautorisecode == "F" && sexe == "F") ||
                        vr.Sexeautorisecode == "HF" // Valable pour tous
                    )
                    .Where(vr => 
                        // Filtrer par âge si spécifié
                        age == 0 || 
                        (vr.Agedebut == null || age >= vr.Agedebut) &&
                        (vr.Agefin == null || age <= vr.Agefin)
                    )
                    .FirstOrDefault();
                
                // Si pas de valeurs de référence trouvées, utiliser la logique de fallback
                if (referenceRange == null)
                {
                    _logger.LogWarning("Aucune valeur de référence trouvée pour l'analyse {AnalyseId}, age {Age}, sexe {Sexe}. Total valeurs disponibles: {TotalCount}. Utilisation de la logique de fallback.", 
                        analyseId, age, sexe, allReferenceRanges.Count);
                    
                    // Log les valeurs disponibles pour debug
                    foreach (var range in allReferenceRanges)
                    {
                        _logger.LogInformation("Valeur référence disponible: Sexe={Sexe}, AgeDebut={AgeDebut}, AgeFin={AgeFin}, Min={Min}, Max={Max}",
                            range.Sexeautorisecode, range.Agedebut, range.Agefin, range.Referencefromvalue, range.Referencetovalue);
                    }
                    
                    // Logique de fallback simplifiée
                    if (resultat < 0)
                        return "Bas";
                    if (resultat > 100)
                        return "Haut";
                    return "Normal";
                }
                
                // Utiliser les valeurs de référence pour déterminer le commentaire
                var minValue = referenceRange.Referencefromvalue;
                var maxValue = referenceRange.Referencetovalue;
                
                _logger.LogInformation("Utilisation valeurs référence trouvées: Min={Min}, Max={Max}, Resultat={Resultat}, Sexe={Sexe}, Age={Age}",
                    minValue, maxValue, resultat, referenceRange.Sexeautorisecode, $"{referenceRange.Agedebut}-{referenceRange.Agefin}");
                
                // Vérifier si les valeurs de référence sont définies
                if (minValue.HasValue && maxValue.HasValue)
                {
                    string commentaire;
                    if (resultat < minValue.Value)
                        commentaire = "Bas";
                    else if (resultat > maxValue.Value)
                        commentaire = "Haut";
                    else
                        commentaire = "Normal";
                        
                    _logger.LogInformation("Commentaire calculé: {Commentaire} pour resultat {Resultat} (min: {Min}, max: {Max})",
                        commentaire, resultat, minValue.Value, maxValue.Value);
                    return commentaire;
                }
                else if (minValue.HasValue) // Seulement valeur minimum définie
                {
                    if (resultat < minValue.Value)
                        return "Bas";
                    return "Normal";
                }
                else if (maxValue.HasValue) // Seulement valeur maximum définie
                {
                    if (resultat > maxValue.Value)
                        return "Haut";
                    return "Normal";
                }
                
                // Si aucune limite définie, retourner Normal
                return "Normal";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du calcul automatique du commentaire pour l'analyse {AnalyseId}", analyseId);
                return "Normal"; // Valeur par défaut en cas d'erreur
            }
        }
        
        private bool EnteteresultatExists(Guid id)
        {
          return (_context.Enteteresultats?.Any(e => e.Enteteresultatid == id)).GetValueOrDefault();
        }

        /// <summary>
        /// Crée un SelectListItem pour une demande avec toutes les informations formatées
        /// </summary>
        /// <param name="demande">La demande à convertir</param>
        /// <returns>Un SelectListItem avec le texte formaté</returns>
        private static SelectListItem CreateDemandeSelectListItem(Entetedemande demande)
        {
            var numeroDemande = demande.Numero ?? "";
            var dateDemande = demande.Date.ToString("dd/MM/yyyy");
            var nomPatient = demande.Patient?.Nom ?? "";
            var prenomPatient = demande.Patient?.Prenom ?? "";
            var codePatient = demande.Patient?.Code ?? "";
            var nomPrescripteur = demande.Prescripteur?.Nom ?? "";
            var patientComplet = $"{nomPatient} {prenomPatient} ({codePatient})".Trim();
            string assurance = "";
            if (demande.Policeassurance != null && demande.Policeassurance.CodeassuranceNavigation != null)
            {
                var nomAssurance = demande.Policeassurance.CodeassuranceNavigation.Nom ?? "";
                var taux = demande.Policeassurance.Taux;
                assurance = $"{nomAssurance} ({taux}%)";
            }
            // Format avec des tirets
            var texteFormate = $"{numeroDemande} - {dateDemande} - {patientComplet} - {nomPrescripteur} - {assurance}";
            return new SelectListItem
            {
                Value = demande.Entetedemandeid.ToString(),
                Text = texteFormate
            };
        }

        // Optimized method to initialize ViewData with minimal database calls
        private async Task InitCreateViewDataOptimized(object vm = null)
        {
            try
            {
                // Get sites (usually small table, so keep this simple)
                var sites = await _context.Sites
                    .AsNoTracking()
                    .Select(s => new SelectListItem { Value = s.Codesite ?? "", Text = s.Name ?? "" })
                    .ToListAsync();
                sites.Insert(0, new SelectListItem { Value = string.Empty, Text = "---" });
                
                if (vm != null)
                {
                    var codesite = vm.GetType().GetProperty("Codesite")?.GetValue(vm)?.ToString();
                    if (!string.IsNullOrEmpty(codesite))
                    {
                        foreach (var s in sites) { s.Selected = (s.Value == codesite); }
                    }
                }
                ViewData["Codesite"] = sites;

                // Get technicians (simplified)
                var techniciens = await _context.Utilisateurs
                    .AsNoTracking()
                    .Where(u => u.Nom != null)
                    .Select(u => new SelectListItem { Value = u.Utilisateurid.ToString(), Text = u.Nom ?? "" })
                    .ToListAsync();
                techniciens.Insert(0, new SelectListItem { Value = string.Empty, Text = "---" });
                
                if (vm != null)
                {
                    var technicienid = vm.GetType().GetProperty("Technicienid")?.GetValue(vm)?.ToString();
                    if (!string.IsNullOrEmpty(technicienid))
                    {
                        foreach (var t in techniciens) { t.Selected = (t.Value == technicienid); }
                    }
                }
                ViewData["Technicienid"] = techniciens;

                // Get laboratories (simplified)
                var laboratoires = await _context.Loboratoires
                    .AsNoTracking()
                    .Where(l => l.Nom != null)
                    .Select(l => new SelectListItem { Value = l.Idlaboratoire.ToString(), Text = l.Nom ?? "" })
                    .ToListAsync();
                laboratoires.Insert(0, new SelectListItem { Value = string.Empty, Text = "---" });
                
                if (vm != null)
                {
                    var laboratoireid = vm.GetType().GetProperty("Laboratoireid")?.GetValue(vm)?.ToString();
                    if (!string.IsNullOrEmpty(laboratoireid))
                    {
                        foreach (var l in laboratoires) { l.Selected = (l.Value == laboratoireid); }
                    }
                }
                ViewData["Laboratoireid"] = laboratoires;

                // Simplified demand loading - only recent demands (last 30 days) to improve performance
                var cutoffDate = DateTime.Now.AddDays(-30);
                var laboratoireFilterId_str = vm?.GetType().GetProperty("Laboratoireid")?.GetValue(vm)?.ToString();
                int.TryParse(laboratoireFilterId_str, out int labId);
                
                var recentDemandes = await _context.Entetedemandes
                    .AsNoTracking()
                    .Include(d => d.Detaildemandes)
                        .ThenInclude(dd => dd.IdanalyseNavigation)
                    .Include(d => d.Detaildemandes)
                        .ThenInclude(dd => dd.Categorie)
                    .Include(d => d.Patient)
                    .Include(d => d.Prescripteur)
                    .Include(d => d.Policeassurance)
                        .ThenInclude(p => p.CodeassuranceNavigation)
                    .Where(d => d.Date >= cutoffDate)
                    .OrderByDescending(d => d.Date)
                    .Take(100) // Limit to 100 most recent
                    .ToListAsync();

                // Filtrer les demandes qui ont encore des analyses à saisir
                var demandesAvecAnalysesRestantes = new List<(Guid Entetedemandeid, string Numero, DateTime Date, string PatientNom, string PatientPrenom, string PatientCode, string PrescripteurNom, string AssuranceNom, decimal AssuranceTaux)>();
                
                foreach (var demande in recentDemandes)
                {
                    // Récupérer toutes les analyses demandées (directement + via catégories)
                    var analysesDirectes = demande.Detaildemandes
                        .Where(dd => dd.Idanalyse.HasValue)
                        .Select(dd => dd.Idanalyse.Value)
                        .ToList();

                    var categorieIds = demande.Detaildemandes
                        .Where(dd => dd.Categorieid.HasValue)
                        .Select(dd => dd.Categorieid.Value)
                        .ToList();

                    var analysesViaCategories = new List<Guid>();
                    if (categorieIds.Any())
                    {
                        analysesViaCategories = await _context.Categorieanalyses
                            .AsNoTracking()
                            .Where(ca => categorieIds.Contains(ca.Categorieid))
                            .Select(ca => ca.Idanalyse)
                            .ToListAsync();
                    }

                    var toutesLesAnalyses = analysesDirectes.Concat(analysesViaCategories).Distinct().ToList();

                    // Filtrer les analyses pour ne garder que celles du laboratoire spécifié
                    var analysesLaboratoire = new List<Guid>();
                    if (labId > 0)
                    {
                        analysesLaboratoire = await _context.Analyses
                            .AsNoTracking()
                            .Where(a => toutesLesAnalyses.Contains(a.Idanalyse) && a.Idlaboratoire == labId)
                            .Select(a => a.Idanalyse)
                            .ToListAsync();
                    }
                    else
                    {
                        // Si pas de laboratoire spécifié, prendre toutes les analyses
                        analysesLaboratoire = toutesLesAnalyses;
                    }

                    // Si aucune analyse du laboratoire dans cette demande, passer à la suivante
                    if (!analysesLaboratoire.Any())
                        continue;

                    // Vérifier si toutes les analyses du laboratoire ont déjà des résultats saisis
                    var resultatsDejasSaisis = await _context.Enteteresultats
                        .AsNoTracking()
                        .Where(er => er.Entetedemandeid == demande.Entetedemandeid 
                                  && analysesLaboratoire.Contains(er.Idanalyse))
                        .Select(er => er.Idanalyse)
                        .Distinct()
                        .ToListAsync();

                    // La demande est disponible s'il reste des analyses du laboratoire à saisir
                    if (resultatsDejasSaisis.Count < analysesLaboratoire.Count)
                    {
                        demandesAvecAnalysesRestantes.Add((
                            demande.Entetedemandeid,
                            demande.Numero ?? "",
                            demande.Date,
                            demande.Patient?.Nom ?? "",
                            demande.Patient?.Prenom ?? "",
                            demande.Patient?.Code ?? "",
                            demande.Prescripteur?.Nom ?? "",
                            demande.Policeassurance?.CodeassuranceNavigation?.Nom ?? "",
                            demande.Policeassurance?.Taux ?? 0
                        ));
                    }
                }
                
                // Créer la liste des demandes filtrées pour le dropdown
                var demandeGroup = new SelectListGroup { Name = "Numéro - Date - Patient - Prescripteur - Assurance" };
                var demandes = demandesAvecAnalysesRestantes.Select(d => new SelectListItem
                {
                    Value = d.Entetedemandeid.ToString(),
                    Text = $"{d.Numero} - {d.Date:dd/MM/yyyy} - {d.PatientNom} {d.PatientPrenom} ({d.PatientCode}) - {d.PrescripteurNom} - {d.AssuranceNom} ({d.AssuranceTaux}%)",
                    Group = demandeGroup
                }).ToList();
                
                var optionDefault = new SelectListItem { Value = string.Empty, Text = "---", Group = null };
                var demandesList = new List<SelectListItem> { optionDefault };
                demandesList.AddRange(demandes);
                
                if (vm != null)
                {
                    var entetedemandeid = vm.GetType().GetProperty("Entetedemandeid")?.GetValue(vm)?.ToString();
                    if (!string.IsNullOrEmpty(entetedemandeid))
                    {
                        foreach (var d in demandesList) { d.Selected = (d.Value == entetedemandeid); }
                    }
                }
                ViewData["Entetedemandeid"] = demandesList;

                // Simplified analyses - only get relevant ones
                var analyses = new List<SelectListItem> { new SelectListItem { Value = string.Empty, Text = "---" } };
                var entetedemandeFilterId = vm?.GetType().GetProperty("Entetedemandeid")?.GetValue(vm)?.ToString();
                
                if (Guid.TryParse(entetedemandeFilterId, out Guid demandeId) && demandeId != Guid.Empty &&
                    int.TryParse(laboratoireFilterId_str, out int laboratoireId) && laboratoireId > 0)
                {
                    // Get analyses for this specific demand and laboratory
                    var relevantAnalyses = await _context.Analyses
                        .AsNoTracking()
                        .Where(a => a.Idlaboratoire == laboratoireId && a.Nom != null)
                        .Select(a => new SelectListItem { Value = a.Idanalyse.ToString(), Text = a.Nom ?? "" })
                        .ToListAsync();
                    
                    analyses.AddRange(relevantAnalyses);
                }
                else if (int.TryParse(laboratoireFilterId_str, out laboratoireId) && laboratoireId > 0)
                {
                    // Just get analyses for the laboratory
                    var labAnalyses = await _context.Analyses
                        .AsNoTracking()
                        .Where(a => a.Idlaboratoire == laboratoireId && a.Nom != null)
                        .Take(50) // Limit for performance
                        .Select(a => new SelectListItem { Value = a.Idanalyse.ToString(), Text = a.Nom ?? "" })
                        .ToListAsync();
                    
                    analyses.AddRange(labAnalyses);
                }
                
                if (vm != null)
                {
                    var idanalyse = vm.GetType().GetProperty("Idanalyse")?.GetValue(vm)?.ToString();
                    if (!string.IsNullOrEmpty(idanalyse))
                    {
                        foreach (var a in analyses) { a.Selected = (a.Value == idanalyse); }
                    }
                }
                
                ViewData["Idanalyse"] = analyses;
            }
            catch (Exception ex)
            {
                // Log the error and provide fallback ViewData
                _logger.LogError(ex, "Error in InitCreateViewDataOptimized");
                
                // Provide minimal fallback data
                ViewData["Codesite"] = new List<SelectListItem> { new SelectListItem { Value = string.Empty, Text = "---" } };
                ViewData["Technicienid"] = new List<SelectListItem> { new SelectListItem { Value = string.Empty, Text = "---" } };
                ViewData["Laboratoireid"] = new List<SelectListItem> { new SelectListItem { Value = string.Empty, Text = "---" } };
                ViewData["Entetedemandeid"] = new List<SelectListItem> { new SelectListItem { Value = string.Empty, Text = "---" } };
                ViewData["Idanalyse"] = new List<SelectListItem> { new SelectListItem { Value = string.Empty, Text = "---" } };
            }
        }

        // Keep original method for backwards compatibility but mark as obsolete
        [Obsolete("Use InitCreateViewDataOptimized for better performance")]
        private void InitCreateViewData(object vm = null)
        {
            var sites = _context.Sites.Select(s => new SelectListItem { Value = s.Codesite, Text = s.Name }).ToList();
            sites.Insert(0, new SelectListItem { Value = string.Empty, Text = "---" });
            if (vm != null)
            {
                var codesite = vm.GetType().GetProperty("Codesite")?.GetValue(vm)?.ToString();
                foreach (var s in sites) { s.Selected = (s.Value == codesite); }
            }
            ViewData["Codesite"] = sites;

            var techniciens = _context.Utilisateurs
                .Select(up => new SelectListItem { Value = up.Utilisateurid.ToString(), Text = up.Nom })
                .ToList();
            techniciens.Insert(0, new SelectListItem { Value = string.Empty, Text = "---" });

            if (vm != null)
            {
                var technicienid = vm.GetType().GetProperty("Technicienid")?.GetValue(vm)?.ToString();
                foreach (var t in techniciens) { t.Selected = (t.Value == technicienid); }
            }
            ViewData["Technicienid"] = techniciens;

            var laboratoires = _context.Loboratoires
                .Select(up => new SelectListItem { Value = up.Idlaboratoire.ToString(), Text = up.Nom })
                .ToList();
            laboratoires.Insert(0, new SelectListItem { Value = string.Empty, Text = "---" });

            if (vm != null)
            {
                var laboratoireid = vm.GetType().GetProperty("Laboratoireid")?.GetValue(vm)?.ToString();
                foreach (var t in laboratoires) { t.Selected = (t.Value == laboratoireid); }
            }
            ViewData["Laboratoireid"] = laboratoires;

            var demandeGroup = new SelectListGroup { Name = "Numéro - Date - Patient - Prescripteur - Assurance" };
            
            // Récupérer toutes les demandes avec leurs détails
            var toutesLesDemandes = _context.Entetedemandes
                .Include(d => d.Patient)
                .Include(d => d.Prescripteur)
                .Include(d => d.Policeassurance)
                .ThenInclude(p => p.CodeassuranceNavigation)
                .Include(d => d.Detaildemandes)
                .ThenInclude(dd => dd.Categorie)
                .Include(d => d.Detaildemandes)
                .ThenInclude(dd => dd.IdanalyseNavigation)
                .OrderByDescending(d => d.Date)
                .ToList();

            // filtrer les demandes par laboratoire
            var laboratoireid_str = vm.GetType().GetProperty("Laboratoireid")?.GetValue(vm)?.ToString();

            if (int.TryParse(laboratoireid_str, out _))
            {
                toutesLesDemandes = toutesLesDemandes
                    .Where(d => d.Detaildemandes
                        .Any(dd => dd.IdanalyseNavigation != null && 
                            dd.IdanalyseNavigation.Idlaboratoire == int.Parse(laboratoireid_str)))
                    .ToList();
            }

            // Filtrer les demandes qui contiennent des analyses du laboratoire du technicien ET qui ne sont pas encore toutes saisies
            var demandesDisponibles = toutesLesDemandes.Where(demande =>
            {
                // Récupérer toutes les analyses demandées (directement + via catégories)
                var analysesDirectes = demande.Detaildemandes
                    .Where(dd => dd.Idanalyse.HasValue)
                    .Select(dd => dd.Idanalyse.Value)
                    .ToList();

                var categorieIds = demande.Detaildemandes
                    .Where(dd => dd.Categorieid.HasValue)
                    .Select(dd => dd.Categorieid.Value)
                    .ToList();

                var analysesViaCategories = _context.Categorieanalyses
                    .Where(ca => categorieIds.Contains(ca.Categorieid))
                    .Select(ca => ca.Idanalyse)
                    .ToList();

                var toutesLesAnalyses = analysesDirectes.Concat(analysesViaCategories).ToList();

                // Filtrer les analyses pour ne garder que celles du laboratoire du technicien
                var analysesLaboratoire = new List<Guid>();
                if (int.TryParse(laboratoireid_str, out int labId) && labId > 0)
                {
                    analysesLaboratoire = _context.Analyses
                        .Where(a => toutesLesAnalyses.Contains(a.Idanalyse) && a.Idlaboratoire == labId)
                        .Select(a => a.Idanalyse)
                        .ToList();
                }
                else
                {
                    // Si pas de laboratoire spécifié, prendre toutes les analyses
                    analysesLaboratoire = toutesLesAnalyses;
                }

                // Si aucune analyse du laboratoire dans cette demande, ne pas afficher la demande
                if (!analysesLaboratoire.Any())
                    return false;

                // Vérifier si toutes les analyses du laboratoire ont déjà des résultats saisis
                var resultatsDejasSaisis = _context.Enteteresultats
                    .Where(er => er.Entetedemandeid == demande.Entetedemandeid 
                              && analysesLaboratoire.Contains(er.Idanalyse))
                    .Select(er => er.Idanalyse)
                    .ToList();

                // La demande est disponible s'il reste des analyses du laboratoire à saisir
                return resultatsDejasSaisis.Count < analysesLaboratoire.Count;
            }).ToList();

            var demandes = demandesDisponibles.Select(CreateDemandeSelectListItem).ToList();
            foreach (var d in demandes) { d.Group = demandeGroup; }
            var optionDefault = new SelectListItem { Value = string.Empty, Text = "---", Group = null };
            var demandesList = new List<SelectListItem> { optionDefault };
            demandesList.AddRange(demandes);
            if (vm != null)
            {
                var entetedemandeid = vm.GetType().GetProperty("Entetedemandeid")?.GetValue(vm)?.ToString();
                foreach (var d in demandesList) { d.Selected = (d.Value == entetedemandeid); }
            }
            ViewData["Entetedemandeid"] = demandesList;

            // Filtrer les analyses pour exclure celles qui ont déjà des résultats saisis
            var entetedemandeFilterId = vm?.GetType().GetProperty("Entetedemandeid")?.GetValue(vm)?.ToString();
            var laboratoireFilterId_str = vm?.GetType().GetProperty("Laboratoireid")?.GetValue(vm)?.ToString();
            
            var analysesQuery = _context.Analyses.AsQueryable();

            // Si une demande est sélectionnée, restreindre aux analyses demandées (directes + via catégories)
            if (Guid.TryParse(entetedemandeFilterId, out Guid demandeId) && demandeId != Guid.Empty)
            {
                var analysesDirectesIds = _context.Detaildemandes
                    .Where(dd => dd.Entetedemandeid == demandeId && dd.Idanalyse.HasValue)
                    .Select(dd => dd.Idanalyse.Value)
                    .ToList();

                var categorieIds = _context.Detaildemandes
                    .Where(dd => dd.Entetedemandeid == demandeId && dd.Categorieid.HasValue)
                    .Select(dd => dd.Categorieid.Value)
                    .ToList();

                var analysesViaCategoriesIds = _context.Categorieanalyses
                    .Where(ca => categorieIds.Contains(ca.Categorieid))
                    .Select(ca => ca.Idanalyse)
                    .ToList();

                var analysesDemandees = analysesDirectesIds
                    .Concat(analysesViaCategoriesIds)
                    .Distinct()
                    .ToList();

                // Restreindre la query aux analyses demandées
                analysesQuery = analysesQuery.Where(a => analysesDemandees.Contains(a.Idanalyse));

                // Exclure les analyses déjà saisies pour cette demande
                var analysesDejaSaisies = _context.Enteteresultats
                    .Where(er => er.Entetedemandeid == demandeId)
                    .Select(er => er.Idanalyse)
                    .ToList();

                analysesQuery = analysesQuery.Where(a => !analysesDejaSaisies.Contains(a.Idanalyse));
            }

            // Filtrer par laboratoire si spécifié (après restriction par demande pour limiter le set)
            if (int.TryParse(laboratoireFilterId_str, out int laboratoireId) && laboratoireId > 0)
            {
                analysesQuery = analysesQuery.Where(a => a.Idlaboratoire == laboratoireId);
            }
            
            var analyses = analysesQuery
                .Select(a => new SelectListItem { Value = a.Idanalyse.ToString(), Text = a.Nom })
                .ToList();
            
            analyses.Insert(0, new SelectListItem { Value = string.Empty, Text = "---" });
            
            if (vm != null)
            {
                var idanalyse = vm.GetType().GetProperty("Idanalyse")?.GetValue(vm)?.ToString();
                foreach (var a in analyses) { a.Selected = (a.Value == idanalyse); }
            }
            
            ViewData["Idanalyse"] = analyses;

        }
        
        /// <summary>
        /// Charge les données de prélèvement existantes dans le ViewModel
        /// </summary>
        private async Task LoadPrelevementData(dynamic viewModel)
        {
            try
            {
                var entetedemandeId = (Guid?)viewModel.GetType().GetProperty("Entetedemandeid")?.GetValue(viewModel);
                var idanalyse = (Guid?)viewModel.GetType().GetProperty("Idanalyse")?.GetValue(viewModel);
                
                if (entetedemandeId.HasValue && entetedemandeId != Guid.Empty && idanalyse.HasValue && idanalyse != Guid.Empty)
                {
                    // Trouver le prélèvement correspondant à cette demande et analyse - d'abord directement
                    var prelevement = await _context.Detaildemandes
                        .Where(dd => dd.Entetedemandeid == entetedemandeId && dd.Idanalyse == idanalyse)
                        .SelectMany(dd => dd.Prelevements)
                        .FirstOrDefaultAsync();
                        
                    // Si pas trouvé directement, chercher via les catégories
                    if (prelevement == null)
                    {
                        var categoriesIds = await _context.Categorieanalyses
                            .Where(ca => ca.Idanalyse == idanalyse)
                            .Select(ca => ca.Categorieid)
                            .ToListAsync();
                            
                        if (categoriesIds.Any())
                        {
                            prelevement = await _context.Detaildemandes
                                .Where(dd => dd.Entetedemandeid == entetedemandeId && 
                                           dd.Categorieid.HasValue && 
                                           categoriesIds.Contains(dd.Categorieid.Value))
                                .SelectMany(dd => dd.Prelevements)
                                .FirstOrDefaultAsync();
                        }
                    }
                        
                    if (prelevement != null)
                    {
                        // Assigner les valeurs aux propriétés du ViewModel
                        viewModel.GetType().GetProperty("DatePrelevement")?.SetValue(viewModel, prelevement.Dateprelevement);
                        viewModel.GetType().GetProperty("DateReception")?.SetValue(viewModel, prelevement.Datereception);
                        
                        _logger.LogInformation($"Données de prélèvement chargées: DatePrelevement={prelevement.Dateprelevement}, DateReception={prelevement.Datereception}");
                    }
                    else
                    {
                        _logger.LogInformation($"Aucun prélèvement trouvé pour EntetedemandeId: {entetedemandeId}, IdAnalyse: {idanalyse}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors du chargement des données de prélèvement : {ex.Message}");
            }
        }
        
        /// <summary>
        /// Sauvegarde les données de prélèvement depuis le ViewModel
        /// </summary>
        private async Task SavePrelevementData(dynamic viewModel)
        {
            try
            {
                var entetedemandeId = (Guid?)viewModel.GetType().GetProperty("Entetedemandeid")?.GetValue(viewModel);
                var idanalyse = (Guid?)viewModel.GetType().GetProperty("Idanalyse")?.GetValue(viewModel);
                var datePrelevement = (DateTime?)viewModel.GetType().GetProperty("DatePrelevement")?.GetValue(viewModel);
                var dateReception = (DateTime?)viewModel.GetType().GetProperty("DateReception")?.GetValue(viewModel);
                
                _logger.LogInformation($"SavePrelevementData - EntetedemandeId: {entetedemandeId}, IdAnalyse: {idanalyse}, DatePrelevement: {datePrelevement}, DateReception: {dateReception}");
                
                // Si aucune date n'est fournie, ne rien faire
                if (!datePrelevement.HasValue && !dateReception.HasValue)
                {
                    _logger.LogInformation("Aucune date de prélèvement ou de réception fournie, aucune action nécessaire.");
                    return;
                }
                
                // Vérifier que les données de référence existent
                var preleveurCount = await _context.Preleveurs.CountAsync();
                var natureEchantillonCount = await _context.Natureechantillons.CountAsync();
                
                if (preleveurCount == 0)
                {
                    _logger.LogError("Aucun préleveur n'existe dans la base de données. Impossible de créer un prélèvement.");
                    throw new InvalidOperationException("Aucun préleveur configuré dans le système. Veuillez contacter l'administrateur.");
                }
                
                if (natureEchantillonCount == 0)
                {
                    _logger.LogError("Aucune nature d'échantillon n'existe dans la base de données. Impossible de créer un prélèvement.");
                    throw new InvalidOperationException("Aucune nature d'échantillon configurée dans le système. Veuillez contacter l'administrateur.");
                }
                    
                if (!entetedemandeId.HasValue || entetedemandeId == Guid.Empty || !idanalyse.HasValue || idanalyse == Guid.Empty)
                {
                    _logger.LogWarning($"Paramètres invalides - EntetedemandeId: {entetedemandeId}, IdAnalyse: {idanalyse}");
                    return;
                }
                
                // Trouver le DetailDemande correspondant - d'abord avec l'analyse spécifique
                var detaildemande = await _context.Detaildemandes
                    .Include(dd => dd.Prelevements)
                    .FirstOrDefaultAsync(dd => dd.Entetedemandeid == entetedemandeId && dd.Idanalyse == idanalyse);
                    
                // Si pas trouvé avec l'analyse spécifique, chercher via les catégories
                if (detaildemande == null)
                {
                    _logger.LogInformation($"Pas de DetailDemande trouvé directement, recherche via catégories pour analyse {idanalyse}");
                    
                    // Chercher les catégories qui contiennent cette analyse
                    var categoriesIds = await _context.Categorieanalyses
                        .Where(ca => ca.Idanalyse == idanalyse)
                        .Select(ca => ca.Categorieid)
                        .ToListAsync();
                        
                    if (categoriesIds.Any())
                    {
                        detaildemande = await _context.Detaildemandes
                            .Include(dd => dd.Prelevements)
                            .FirstOrDefaultAsync(dd => dd.Entetedemandeid == entetedemandeId && 
                                               dd.Categorieid.HasValue && 
                                               categoriesIds.Contains(dd.Categorieid.Value));
                    }
                }
                    
                if (detaildemande != null)
                {
                    _logger.LogInformation($"DetailDemande trouvé: {detaildemande.Detaildemandeid}");
                    
                    // Chercher un prélèvement existant
                    var prelevement = detaildemande.Prelevements.FirstOrDefault();
                    
                    if (prelevement != null)
                    {
                        _logger.LogInformation($"Mise à jour du prélèvement existant: {prelevement.Prelevementid}");
                        
                        // Mettre à jour le prélèvement existant
                        if (datePrelevement.HasValue)
                            prelevement.Dateprelevement = datePrelevement.Value;
                        if (dateReception.HasValue)
                            prelevement.Datereception = dateReception.Value;
                    }
                    else if (datePrelevement.HasValue)
                    {
                        _logger.LogInformation("Création d'un nouveau prélèvement");
                        
                        // Créer un nouveau prélèvement
                        // Utiliser un préleveur par défaut ou le premier disponible
                        var preleveur = await _context.Preleveurs.FirstOrDefaultAsync();
                        
                        if (preleveur == null)
                        {
                            _logger.LogError("Aucun préleveur trouvé dans la base de données");
                            return;
                        }
                        
                        // Utiliser une nature d'échantillon par défaut
                        var natureEchantillon = await _context.Natureechantillons.FirstOrDefaultAsync();
                        
                        if (natureEchantillon == null)
                        {
                            _logger.LogError("Aucune nature d'échantillon trouvée dans la base de données");
                            return;
                        }
                        
                        var nouveauPrelevement = new Prelevement
                        {
                            Prelevementid = Guid.NewGuid(),
                            Detaildemandeid = detaildemande.Detaildemandeid,
                            Preleveurid = preleveur.Preleveurid,
                            Idnatureechantillon = natureEchantillon.Idnatureechantillon,
                            Dateprelevement = datePrelevement.Value,
                            Datereception = dateReception,
                            Statut = "En cours"
                        };
                        
                        _context.Prelevements.Add(nouveauPrelevement);
                        _logger.LogInformation($"Nouveau prélèvement créé: {nouveauPrelevement.Prelevementid}");
                    }
                }
                else
                {
                    _logger.LogWarning($"Aucun DetailDemande trouvé pour EntetedemandeId: {entetedemandeId} et IdAnalyse: {idanalyse}");
                    
                    // En dernière option, créer un DetailDemande si nécessaire
                    if (datePrelevement.HasValue)
                    {
                        _logger.LogInformation("Tentative de création d'un DetailDemande pour le prélèvement");
                        
                        var nouveauDetailDemande = new Detaildemande
                        {
                            Detaildemandeid = Guid.NewGuid(),
                            Entetedemandeid = entetedemandeId.Value,
                            Idanalyse = idanalyse.Value,
                            Prix = 0,
                            Partassurance = 0,
                            Partpatient = 0,
                            Complement = 0,
                            Net = 0
                        };
                        
                        _context.Detaildemandes.Add(nouveauDetailDemande);
                        
                        // Créer le prélèvement associé
                        var preleveurDefaut = await _context.Preleveurs.FirstOrDefaultAsync();
                        var natureEchantillon = await _context.Natureechantillons.FirstOrDefaultAsync();
                        
                        if (preleveurDefaut != null && natureEchantillon != null)
                        {
                            var nouveauPrelevement = new Prelevement
                            {
                                Prelevementid = Guid.NewGuid(),
                                Detaildemandeid = nouveauDetailDemande.Detaildemandeid,
                                Preleveurid = preleveurDefaut.Preleveurid,
                                Idnatureechantillon = natureEchantillon.Idnatureechantillon,
                                Dateprelevement = datePrelevement.Value,
                                Datereception = dateReception,
                                Statut = "En cours"
                            };
                            
                            _context.Prelevements.Add(nouveauPrelevement);
                            _logger.LogInformation($"DetailDemande et prélèvement créés: {nouveauDetailDemande.Detaildemandeid}, {nouveauPrelevement.Prelevementid}");
                        }
                        else
                        {
                            _logger.LogError("Impossible de créer le prélèvement - préleveur ou nature d'échantillon manquant");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la sauvegarde des données de prélèvement : {ex.Message}\nStackTrace: {ex.StackTrace}");
                throw; // Re-throw l'exception pour qu'elle soit gérée par le contrôleur
            }
        }
    }
} 
