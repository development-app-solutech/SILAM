using AutoMapper;
using iText.Commons.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NCalc;
using Newtonsoft.Json;
using ProlabWeb.Db;
using ProlabWeb.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ProlabWeb.Controllers
{
    [Authorize(Roles = "Administrateur,GestionResultat,Technicien,Biologiste")]
    public class ParametreController : Controller
    {
        private readonly ProlabwebContext _context;
        private readonly IMapper _mapper;

        public ParametreController(ProlabwebContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: Parametre
        public IActionResult Index(Guid? id)
        {
            List<Parametre> parametres;
            if (id == null)
            {
                parametres = _context.Parametres
                    .Include(p => p.IdanalyseNavigation)
                    .Include(p => p.CodeuniteNavigation)
                    .Include(p => p.CodeunitesiNavigation)
                    .ToList();
            }
            else
            {
                parametres = _context.Parametres
                    .Include(p => p.IdanalyseNavigation)
                    .Include(p => p.CodeuniteNavigation)
                    .Include(p => p.CodeunitesiNavigation)
                    .Where(p => p.Idanalyse == id)
                    .ToList();
                ViewBag.Analyse = _context.Analyses.FirstOrDefault(a => a.Idanalyse == id);
            }
            return View(parametres);
        }

        // GET: Parametre/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parametre = await _context.Parametres
                .Include(p => p.IdanalyseNavigation)
                .FirstOrDefaultAsync(m => m.Parametreid == id);
            if (parametre == null)
            {
                return NotFound();
            }

            return View(parametre);
        }

        // GET: Parametre/Create
        public IActionResult Create()
        {
            // Récupérer l'utilisateur connecté et son laboratoire
            string login = User.Identity?.Name ?? string.Empty;
            var utilisateur = _context.Utilisateurs
                .Include(u => u.Utilisateurlaboratoires)
                    .ThenInclude(ul => ul.IdlaboratoireNavigation)
                .FirstOrDefault(u => u.Login == login);

            var vm = new ProlabWeb.ViewModels.ParametreCreateVM();

            if (utilisateur != null && utilisateur.Utilisateurlaboratoires.Any())
            {
                // Prendre le premier laboratoire associé à l'utilisateur
                var laboratoireUtilisateur = utilisateur.Utilisateurlaboratoires.First();
                vm.IdLaboratoire = laboratoireUtilisateur.Idlaboratoire;
                vm.NomLaboratoire = laboratoireUtilisateur.IdlaboratoireNavigation.Nom;

                // Charger uniquement les analyses de ce laboratoire
                var analyses = _context.Analyses
                    .Where(a => a.Idlaboratoire == vm.IdLaboratoire)
                    .Select(a => new { a.Idanalyse, a.Nom })
                    .OrderBy(a => a.Nom)
                    .ToList();
                ViewBag.Analyses = new SelectList(analyses, "Idanalyse", "Nom");
            }
            else
            {
                // Si aucun laboratoire trouvé, charger la liste complète (fallback)
                var laboratoires = _context.Loboratoires
                    .Where(l => l.Isactive)
                    .Select(l => new { l.Idlaboratoire, l.Nom })
                    .OrderBy(l => l.Nom)
                    .ToList();
                ViewBag.Laboratoires = new SelectList(laboratoires, "Idlaboratoire", "Nom");
                ViewBag.Analyses = new SelectList(new List<object>(), "Idanalyse", "Nom");
            }

            var unites = _context.Unites
                .Select(u => new { u.Code, u.Name })
                .ToList();
            ViewBag.Unites = new SelectList(unites, "Code", "Name");

            var types = Enum.GetValues(typeof(EnumTypeParametre))
                .Cast<EnumTypeParametre>()
                .Select(e => new SelectListItem
                {
                    Text = e.ToString(),      
                    Value = e.ToString()
                }).ToList();
            ViewBag.TypeParametres = types;

            return View(vm);
        }

        // POST: Parametre/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProlabWeb.ViewModels.ParametreCreateVM model, string submitAction)
        {
            // Récupérer le nom du laboratoire si pas déjà présent
            if (string.IsNullOrEmpty(model.NomLaboratoire) && model.IdLaboratoire > 0)
            {
                var laboratoire = _context.Loboratoires.Find(model.IdLaboratoire);
                model.NomLaboratoire = laboratoire?.Nom;
            }

            // Populate analyses dropdown based on selected laboratory
            var analyses = new List<object>();
            if (model.IdLaboratoire > 0)
            {
                analyses = _context.Analyses
                    .Where(a => a.Idlaboratoire == model.IdLaboratoire)
                    .Select(a => new { a.Idanalyse, a.Nom })
                    .ToList<object>();
            }
            ViewBag.Analyses = new SelectList(analyses, "Idanalyse", "Nom", model.IdAnalyse);

            var unites = _context.Unites
                .Select(u => new { u.Code, u.Name })
                .ToList();
            ViewBag.Unites = new SelectList(unites, "Code", "Name");

            var types = Enum.GetValues(typeof(EnumTypeParametre))
                .Cast<EnumTypeParametre>()
                .Select(e => new SelectListItem
                {
                    Text = e.ToString(),
                    Value = e.ToString()
                }).ToList();
            ViewBag.TypeParametres = types;

            // Traiter d'abord les actions de boutons (avant la validation)
            if (submitAction == "removeRow")
            {
                model.Parametres ??= new List<ProlabWeb.ViewModels.ParametreItemCreateVM>();
                if (model.Parametres.Count > 0)
                    model.Parametres.RemoveAt(model.Parametres.Count - 1);
                ModelState.Clear();
                return View(model);
            }

            if (submitAction == "addRow")
            {
                model.Parametres ??= new List<ProlabWeb.ViewModels.ParametreItemCreateVM>();
                model.Parametres.Add(new ProlabWeb.ViewModels.ParametreItemCreateVM());
                ModelState.Clear();
                return View(model);
            }
            if (string.IsNullOrEmpty(submitAction) || submitAction == "save")
            {
                // vérifier l'existence des valeurs des formules et des listes avant la validation du modèle
                if (model.Parametres != null)
                {
                    for (int i = 0; i < model.Parametres.Count; i++)
                    {
                        var parametre = model.Parametres[i];

                        if (parametre.Builder.Type == EnumTypeParametre.Formule.ToString() &&
                            string.IsNullOrWhiteSpace(parametre.Builder.Valeur))
                        {
                            ModelState.AddModelError($"Parametres[{i}].Builder.Valeur", $"Formule invalide");
                        }
                        else if (parametre.Builder.Type == EnumTypeParametre.Liste.ToString() &&
                            string.IsNullOrWhiteSpace(parametre.Builder.Valeur))
                        {
                            ModelState.AddModelError($"Parametres[{i}].Builder.Valeur", $"Valeur invalide");
                        }
                    }
                }

                // Validation des formules avant la validation du modèle
                if (model.Parametres != null)
                {
                    for (int i = 0; i < model.Parametres.Count; i++)
                    {
                        var parametre = model.Parametres[i];
                        if (parametre.Builder.Type == EnumTypeParametre.Formule.ToString() && 
                            !string.IsNullOrWhiteSpace(parametre.Builder.Valeur))
                        {
                            try
                            {
                                // Utiliser NCalc pour valider seulement la syntaxe de la formule
                                var expression = new NCalc.Expression(parametre.Builder.Valeur);
                                // Vérifier seulement que la formule peut être parsée (syntaxe correcte)
                                if (expression.HasErrors())
                                {
                                    ModelState.AddModelError($"Parametres[{i}].Builder.Valeur", $"Syntaxe de formule invalide: {expression.Error}");
                                }
                            }
                            catch (Exception ex)
                            {
                                ModelState.AddModelError($"Parametres[{i}].Builder.Valeur", $"Formule invalide: {ex.Message}");
                            }
                        }
                    }
                }

                if (ModelState.IsValid)
                {
                    foreach (var paramVm in model.Parametres)
                    {
                        var param = _mapper.Map<Parametre>(paramVm);
                        param.Parametreid = Guid.NewGuid();
                        param.Idanalyse = model.IdAnalyse;

                        // construire le value builder

                        if (paramVm.Builder.Type == EnumTypeParametre.Texte.ToString())
                        {
                            paramVm.Builder.Valeur = "";
                        }

                        string json = JsonConvert.SerializeObject(paramVm.Builder, Formatting.Indented, new JsonSerializerSettings
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });

                        param.Valuebuilder = json;

                        _context.Parametres.Add(param);
                    }
                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }
                // Si non valide, on réaffiche la vue avec les erreurs
                return View(model);
            }
            // Ici, gérer la sauvegarde réelle si besoin
            // if (ModelState.IsValid) { ... }
            return View(model);
        }

        // GET: Parametre/Edit/5 (id peut être IdAnalyse ou Parametreid)
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Guid analyseId;
            int laboratoireId;
            List<Parametre> parametres;

            // Vérifier si l'ID correspond à une analyse ou à un paramètre
            var analyseExists = await _context.Analyses.AnyAsync(a => a.Idanalyse == id);
            if (analyseExists)
            {
                // L'ID est un IdAnalyse
                analyseId = id.Value;
                var analyse = await _context.Analyses.FindAsync(analyseId);
                laboratoireId = analyse.Idlaboratoire;
                parametres = await _context.Parametres
                    .Where(p => p.Idanalyse == id)
                    .ToListAsync();
            }
            else
            {
                // L'ID pourrait être un Parametreid, récupérer l'analyse correspondante
                var parametre = await _context.Parametres
                    .Include(p => p.IdanalyseNavigation)
                    .FirstOrDefaultAsync(p => p.Parametreid == id);

                if (parametre == null)
                {
                    return NotFound();
                }

                analyseId = parametre.Idanalyse;
                laboratoireId = parametre.IdanalyseNavigation.Idlaboratoire;
                parametres = await _context.Parametres
                    .Where(p => p.Idanalyse == analyseId)
                    .ToListAsync();
            }

            // Vérifier si l'utilisateur connecté a des droits sur ce laboratoire
            string login = User.Identity?.Name ?? string.Empty;
            var utilisateur = await _context.Utilisateurs
                .Include(u => u.Utilisateurlaboratoires)
                    .ThenInclude(ul => ul.IdlaboratoireNavigation)
                .FirstOrDefaultAsync(u => u.Login == login);

            // Récupérer le nom du laboratoire
            var laboratoire = await _context.Loboratoires.FindAsync(laboratoireId);
            
            // Vérifier si l'utilisateur a accès à ce laboratoire
            bool utilisateurAAccesAuLaboratoire = utilisateur?.Utilisateurlaboratoires
                .Any(ul => ul.Idlaboratoire == laboratoireId) ?? false;

            var model = new ProlabWeb.ViewModels.ParametreEditVM
            {
                IdLaboratoire = laboratoireId,
                // Figer le nom du laboratoire si l'utilisateur y a accès
                NomLaboratoire = utilisateurAAccesAuLaboratoire ? laboratoire?.Nom : null,
                IdAnalyse = analyseId,
                Parametres = parametres.Select(p => new ProlabWeb.ViewModels.ParametreItemEditVM
                {
                    Parametreid = p.Parametreid,
                    Nom = p.Nom,
                    Code = p.Code,
                    Masquer = p.Masquerdansrapport,
                    //Formule = p.Formuleautomate,
                    ResultatStandard = (p.Decimalresultatstandard ?? 0).ToString(),
                    UniteStandard = p.Codeunite,
                    ResultatSI = (p.Decimalresultatsi ?? 0).ToString(),
                    UniteSI = p.Codeunitesi,
                    FacteurConversion = (p.Facteurconversionsi ?? 0).ToString(),
                    OrdreAffichage = p.Ordreaffichage,
                    Builder = !string.IsNullOrWhiteSpace(p.Valuebuilder) 
                        ? JsonConvert.DeserializeObject<ParametreItemBuilderVM>(p.Valuebuilder)
                        : new ParametreItemBuilderVM()
                    //Valuebuilder = p.Valuebuilder
                }).ToList()
            };

            // Populate laboratories dropdown
            var laboratoires = _context.Loboratoires
                .Where(l => l.Isactive)
                .Select(l => new { l.Idlaboratoire, l.Nom })
                .OrderBy(l => l.Nom)
                .ToList();
            ViewBag.Laboratoires = new SelectList(laboratoires, "Idlaboratoire", "Nom", laboratoireId);

            // Populate analyses dropdown based on selected laboratory
            var analyses = _context.Analyses
                .Where(a => a.Idlaboratoire == laboratoireId)
                .Select(a => new { a.Idanalyse, a.Nom })
                .ToList();
            ViewBag.Analyses = new SelectList(analyses, "Idanalyse", "Nom", analyseId);

            var unites = _context.Unites
                .Select(u => new { u.Code, u.Name })
                .ToList();
            ViewBag.Unites = new SelectList(unites, "Code", "Name");

            var types = Enum.GetValues(typeof(EnumTypeParametre))
                .Cast<EnumTypeParametre>()
                .Select(e => new SelectListItem
                {
                    Text = e.ToString(),
                    Value = e.ToString()
                }).ToList();
            ViewBag.TypeParametres = types;

            return View(model);
        }

        // POST: Parametre/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ProlabWeb.ViewModels.ParametreEditVM model, string submitAction)
        {
            if (id != model.IdAnalyse)
            {
                return NotFound();
            }

            // Vérifier si l'utilisateur connecté a des droits sur ce laboratoire
            string login = User.Identity?.Name ?? string.Empty;
            var utilisateur = await _context.Utilisateurs
                .Include(u => u.Utilisateurlaboratoires)
                    .ThenInclude(ul => ul.IdlaboratoireNavigation)
                .FirstOrDefaultAsync(u => u.Login == login);

            // Vérifier si l'utilisateur a accès à ce laboratoire
            bool utilisateurAAccesAuLaboratoire = utilisateur?.Utilisateurlaboratoires
                .Any(ul => ul.Idlaboratoire == model.IdLaboratoire) ?? false;

            // Récupérer le nom du laboratoire si pas déjà présent et si l'utilisateur a accès
            if (string.IsNullOrEmpty(model.NomLaboratoire) && model.IdLaboratoire > 0)
            {
                var laboratoire = await _context.Loboratoires.FindAsync(model.IdLaboratoire);
                // Ne figer que si l'utilisateur a accès au laboratoire
                model.NomLaboratoire = utilisateurAAccesAuLaboratoire ? laboratoire?.Nom : null;
            }

            // Populate laboratories dropdown
            var laboratoires = _context.Loboratoires
                .Where(l => l.Isactive)
                .Select(l => new { l.Idlaboratoire, l.Nom })
                .OrderBy(l => l.Nom)
                .ToList();
            ViewBag.Laboratoires = new SelectList(laboratoires, "Idlaboratoire", "Nom", model.IdLaboratoire);

            // Populate analyses dropdown based on selected laboratory
            var analyses = new List<object>();
            if (model.IdLaboratoire > 0)
            {
                analyses = _context.Analyses
                    .Where(a => a.Idlaboratoire == model.IdLaboratoire)
                    .Select(a => new { a.Idanalyse, a.Nom })
                    .ToList<object>();
            }
            ViewBag.Analyses = new SelectList(analyses, "Idanalyse", "Nom", model.IdAnalyse);

            var unites = _context.Unites
                .Select(u => new { u.Code, u.Name })
                .ToList();
            ViewBag.Unites = new SelectList(unites, "Code", "Name");

            var types = Enum.GetValues(typeof(EnumTypeParametre))
                .Cast<EnumTypeParametre>()
                .Select(e => new SelectListItem
                {
                    Text = e.ToString(),
                    Value = e.ToString()
                }).ToList();
            ViewBag.TypeParametres = types;

            // Traiter d'abord les actions de boutons (avant la validation)
            if (submitAction == "removeRow")
            {
                model.Parametres ??= new List<ProlabWeb.ViewModels.ParametreItemEditVM>();
                if (model.Parametres.Count > 0)
                    model.Parametres.RemoveAt(model.Parametres.Count - 1);
                ModelState.Clear();
                return View(model);
            }

            if (submitAction == "addRow")
            {
                model.Parametres ??= new List<ProlabWeb.ViewModels.ParametreItemEditVM>();
                model.Parametres.Add(new ProlabWeb.ViewModels.ParametreItemEditVM());
                ModelState.Clear();
                return View(model);
            }

            if (string.IsNullOrEmpty(submitAction) || submitAction == "save")
            {
                // vérifier l'existence des valeurs des formules et des listes avant la validation du modèle
                if (model.Parametres != null)
                {
                    for (int i = 0; i < model.Parametres.Count; i++)
                    {
                        var parametre = model.Parametres[i];

                        if (parametre.Builder.Type == EnumTypeParametre.Formule.ToString() &&
                            string.IsNullOrWhiteSpace(parametre.Builder.Valeur))
                        {
                            ModelState.AddModelError($"Parametres[{i}].Builder.Valeur", $"Formule invalide");
                        }
                        else if (parametre.Builder.Type == EnumTypeParametre.Liste.ToString() &&
                            string.IsNullOrWhiteSpace(parametre.Builder.Valeur))
                        {
                            ModelState.AddModelError($"Parametres[{i}].Builder.Valeur", $"Valeur invalide");
                        }
                    }
                }

                // Validation des formules avant la validation du modèle
                if (model.Parametres != null)
                {
                    for (int i = 0; i < model.Parametres.Count; i++)
                    {
                        var parametre = model.Parametres[i];
                        if (parametre.Builder.Type == EnumTypeParametre.Formule.ToString() && 
                            !string.IsNullOrWhiteSpace(parametre.Builder.Valeur))
                        {
                            try
                            {
                                // Utiliser NCalc pour valider seulement la syntaxe de la formule
                                var expression = new NCalc.Expression(parametre.Builder.Valeur);
                                // Vérifier seulement que la formule peut être parsée (syntaxe correcte)
                                if (expression.HasErrors())
                                {
                                    ModelState.AddModelError($"Parametres[{i}].Builder.Valeur", $"Syntaxe de formule invalide: {expression.Error}");
                                }
                            }
                            catch (Exception ex)
                            {
                                ModelState.AddModelError($"Parametres[{i}].Builder.Valeur", $"Formule invalide: {ex.Message}");
                            }
                        }
                    }
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        // Récupérer la liste des anciens paramètres sans tracking
                        var anciensParametres = await _context.Parametres
                            .AsNoTracking()
                            .Where(p => p.Idanalyse == model.IdAnalyse)
                            .ToListAsync();

                        // Nouvelle liste des paramètres
                        var nouveauxParametres = model.Parametres.Where(p => !string.IsNullOrWhiteSpace(p.Nom)).ToList();

                        // Liste temporaire pour suppression
                        var parametresASupprimer = new List<Parametre>();

                        // Parcours l'ancienne liste pour identifier les suppressions
                        foreach (var ancienParametre in anciensParametres)
                        {
                            var existeDansNouvelleListe = nouveauxParametres.Any(np => np.Parametreid == ancienParametre.Parametreid);
                            if (!existeDansNouvelleListe)
                            {
                                parametresASupprimer.Add(ancienParametre);
                            }
                        }

                        // Suppression en lot
                        if (parametresASupprimer.Any())
                        {
                            _context.Parametres.RemoveRange(parametresASupprimer);
                        }

                        // Listes temporaires pour ajout et mise à jour
                        var parametresAAjouter = new List<Parametre>();
                        var parametresAMettreAJour = new List<Parametre>();

                        // Parcours la nouvelle liste pour identifier ajouts et mises à jour
                        foreach (var nouveauParametreVM in nouveauxParametres)
                        {
                            if (nouveauParametreVM.Parametreid == null || nouveauParametreVM.Parametreid == Guid.Empty)
                            {
                                // Créer un nouveau objet paramètre avec AutoMapper
                                var nouveauParametre = _mapper.Map<Parametre>(nouveauParametreVM);
                                nouveauParametre.Parametreid = Guid.NewGuid();
                                nouveauParametre.Idanalyse = model.IdAnalyse;

                                // construire le value builder

                                if (nouveauParametreVM.Builder.Type == EnumTypeParametre.Texte.ToString())
                                {
                                    nouveauParametreVM.Builder.Valeur = "";
                                }

                                string json = JsonConvert.SerializeObject(nouveauParametreVM.Builder, Formatting.Indented, new JsonSerializerSettings
                                {
                                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                                });

                                nouveauParametre.Valuebuilder = json;

                                parametresAAjouter.Add(nouveauParametre);
                            }
                            else
                            {
                                // Mettre à jour l'objet existant
                                var parametreExistant = anciensParametres.FirstOrDefault(ap => ap.Parametreid == nouveauParametreVM.Parametreid);
                                if (parametreExistant != null)
                                {
                                    // Mapper les données du VM vers l'entité
                                    _mapper.Map(nouveauParametreVM, parametreExistant);

                                    // Compléter les infos manquantes
                                    parametreExistant.Idanalyse = model.IdAnalyse;

                                    // construire le value builder

                                    if (nouveauParametreVM.Builder.Type == EnumTypeParametre.Texte.ToString())
                                    {
                                        nouveauParametreVM.Builder.Valeur = "";
                                    }

                                    string json = JsonConvert.SerializeObject(nouveauParametreVM.Builder, Formatting.Indented, new JsonSerializerSettings
                                    {
                                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                                    });

                                    parametreExistant.Valuebuilder = json;

                                    parametresAMettreAJour.Add(parametreExistant);
                                }
                            }
                        }

                        // Ajout en lot
                        if (parametresAAjouter.Any())
                        {
                            _context.Parametres.AddRange(parametresAAjouter);
                        }
                        // Mise à jour en lot
                        if (parametresAMettreAJour.Any())
                        {
                            _context.Parametres.UpdateRange(parametresAMettreAJour);
                        }

                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateException dbEx)
                    {
                        ModelState.AddModelError("", $"Erreur de base de données: {dbEx.InnerException?.Message ?? dbEx.Message}");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"Erreur lors de la sauvegarde: {ex.Message}");
                    }
                }
            }

            return View(model);
        }

        // GET: Parametre/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parametre = await _context.Parametres
                .Include(p => p.IdanalyseNavigation)
                .FirstOrDefaultAsync(m => m.Parametreid == id);
            if (parametre == null)
            {
                return NotFound();
            }

            return View(parametre);
        }

        // POST: Parametre/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var parametre = await _context.Parametres.FindAsync(id);
            if (parametre != null)
            {
                _context.Parametres.Remove(parametre);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetAnalysesByLaboratoire(int laboratoireId)
        {
            var analyses = await _context.Analyses
                .Where(a => a.Idlaboratoire == laboratoireId)
                .OrderBy(a => a.Nom)
                .Select(a => new { a.Idanalyse, a.Nom })
                .ToListAsync();

            return Json(new ProlabWeb.JsonResponseViewModel
            {
                success = true,
                data = JsonConvert.SerializeObject(analyses),
                message = ""
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetParametreByAnalyse(Guid id)
        {
            var parametres = await _context.Parametres
                .Where(p => p.Idanalyse == id)
                .Include(p => p.CodeuniteNavigation)
                .Include(p => p.CodeunitesiNavigation)
                .OrderBy(p => p.Nom)
                .Select(p => new ProlabWeb.ViewModels.ParametreResultatVM
                {
                    Parametreid = p.Parametreid,
                    Nom = p.Nom ?? string.Empty,
                    Code = p.Code ?? string.Empty,
                    Unite = p.CodeuniteNavigation != null ? p.CodeuniteNavigation.Name ?? string.Empty : string.Empty,
                    Resultat = null,
                    UniteSI = p.CodeunitesiNavigation != null ? p.CodeunitesiNavigation.Name ?? string.Empty : string.Empty,
                    Resultatsi = null,
                    Builder = !string.IsNullOrWhiteSpace(p.Valuebuilder)
                        ? JsonConvert.DeserializeObject<ParametreItemBuilderVM>(p.Valuebuilder)
                        : new ParametreItemBuilderVM(),
                    FacteurConversion = p.Facteurconversionsi.HasValue
                        ? p.Facteurconversionsi.Value.ToString()
                        : ""
                })
                .ToListAsync();

            var parametresJson = JsonConvert.SerializeObject(parametres);

            return Json(new ProlabWeb.JsonResponseViewModel
            {
                success = true,
                data = parametresJson,
                message = ""
            });
        }

        private bool ParametreExists(Guid id)
        {
            return _context.Parametres.Any(e => e.Parametreid == id);
        }
    }
}
