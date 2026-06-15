using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ProlabWeb.Db;
using ProlabWeb.Mappers;
using ProlabWeb.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static iText.Layout.Borders.Border;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace ProlabWeb.Controllers
{
    public class AnalyseAvecOngletController : Controller
    {
        private readonly ProlabwebContext _context;
        private readonly IMapper _mapper;

        public AnalyseAvecOngletController(ProlabwebContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: AnalyseAvecOnglet
        public async Task<IActionResult> Index()
        {
            var cultureFr = new CultureInfo("fr-FR");

            var prolabwebContext = _context.Analyses
                .Include(a => a.CodeuniteNavigation)
                .Include(a => a.CodeunitesiNavigation)
                .Include(a => a.IdanalyseparentNavigation)
                .Include(a => a.IdautomateNavigation)
                .Include(a => a.IdlaboratoireNavigation)
                .Include(a => a.IdnatureechantillonNavigation);

            return View(await prolabwebContext.ToListAsync());
        }

        // GET: AnalyseAvecOnglet/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var analyse = await _context.Analyses
                .Include(a => a.CodeuniteNavigation)
                .Include(a => a.CodeunitesiNavigation)
                .Include(a => a.IdanalyseparentNavigation)
                .Include(a => a.IdautomateNavigation)
                .Include(a => a.IdlaboratoireNavigation)
                .Include(a => a.IdnatureechantillonNavigation)
                .FirstOrDefaultAsync(m => m.Idanalyse == id);
            if (analyse == null)
            {
                return NotFound();
            }

            return View(analyse);
        }

        // GET: AnalyseAvecOnglet/Create
        public IActionResult Create()
        {
            var model = new AnalyseAvecOngletCreateVM
            {
                Parametres = new List<ParametreCreateAvecOngletVM>
                {
                    new ParametreCreateAvecOngletVM()
                },
                ValeursReference = new List<ValeurreferenceCreateAvecOngletVM>
                {
                    new ValeurreferenceCreateAvecOngletVM()
                },
                TarifsAssurance = _context.Assurances.AsNoTracking()
                    .Select(x => new TarifanalyseassuranceCreateAvecOngletVM
                    {
                        Codeassurance = x.Codeassurance,
                        Nom = x.Nom,
                    })
                    .ToList()
            };

            var unites = _context.Unites?.ToList();
            var unitesList = new List<SelectListItem> { new SelectListItem("---", string.Empty) };
            if (unites != null)
                unitesList.AddRange(unites.Select(u => new SelectListItem(u.Name, u.Code)));

            var unitesis = _context.Unites?.ToList();
            var unitesisList = new List<SelectListItem> { new SelectListItem("---", string.Empty) };
            if (unitesis != null)
                unitesisList.AddRange(unitesis.Select(u => new SelectListItem(u.Name, u.Code)));

            var analyses = _context.Analyses?.ToList();
            var analysesList = new List<SelectListItem> { new SelectListItem("---", string.Empty) };
            if (analyses != null)
                analysesList.AddRange(analyses.Select(a => new SelectListItem(a.Nom, a.Idanalyse.ToString())));

            var automates = _context.Automates?.ToList();
            var automatesList = new List<SelectListItem> { new SelectListItem("---", string.Empty) };
            if (automates != null)
                automatesList.AddRange(automates.Select(a => new SelectListItem(a.Nom, a.Idautomate.ToString())));

            var laboratoires = _context.Loboratoires?.ToList();
            var laboratoiresList = new List<SelectListItem> { new SelectListItem("---", string.Empty) };
            if (laboratoires != null)
                laboratoiresList.AddRange(laboratoires.Select(l => new SelectListItem(l.Nom, l.Idlaboratoire.ToString())));

            var echantillons = _context.Natureechantillons?.ToList();
            var echantillonsList = new List<SelectListItem> { new SelectListItem("---", string.Empty) };
            if (echantillons != null)
                echantillonsList.AddRange(echantillons.Select(e => new SelectListItem(e.Nom, e.Idnatureechantillon.ToString())));

            var methodes = _context.Methodes?.ToList();
            var methodesList = new List<SelectListItem> { new SelectListItem("---", string.Empty) };
            if (methodes != null)
                methodesList.AddRange(methodes.Select(m => new SelectListItem(m.Nom, m.Idmethode.ToString())));

            var assurances = _context.Assurances?.ToList();
            var assurancesList = new List<SelectListItem> { new SelectListItem("---", string.Empty) };
            if (assurances != null)
                assurancesList.AddRange(assurances.Select(a => new SelectListItem(a.Nom, a.Codeassurance)));

            var sexeautorises = new SelectList(_context.Sexeautorises, "Sexeautorisecode", "Valeur").ToList();
            sexeautorises.Insert(0, new SelectListItem("---", string.Empty));

            var types = Enum.GetValues(typeof(EnumTypeParametre))
                .Cast<EnumTypeParametre>()
                .Select(e => new SelectListItem
                {
                    Text = e.ToString(),
                    Value = e.ToString()
                }).ToList();
            ViewBag.TypeParametres = types;

            ViewData["Sexeautorisecode"] = sexeautorises;
            ViewData["Codeunite"] = unitesList;
            ViewData["Codeunitesi"] = unitesisList;
            ViewData["Idanalyseparent"] = analysesList;
            ViewData["Idautomate"] = automatesList;
            ViewData["Idlaboratoire"] = laboratoiresList;
            ViewData["Idnatureechantillon"] = echantillonsList;
            ViewData["IdsMethode"] = methodesList;
            ViewData["Assurances"] = assurancesList;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AnalyseAvecOngletCreateVM model)
        {
            if (model.Parametres != null && model.Parametres.Any())
            {
                // Vérifier l'existence des valeurs des formules et des listes avant la validation du modèle
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

                // Validation des formules avant la validation du modèle
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

            if (model.TarifsAssurance != null && model.TarifsAssurance.Any())
            {
                // Vérifier que les prix des tarifs sont valide (décimal supérieure à 0)
                for (int i = 0; i < model.TarifsAssurance.Count; i++)
                {
                    var erreur = (!string.IsNullOrWhiteSpace(model.TarifsAssurance[i].Prix)
                        && decimal.TryParse(model.TarifsAssurance[i].Prix, out _)
                        && decimal.Parse(model.TarifsAssurance[i].Prix) <= 0)
                        ||
                        (!string.IsNullOrWhiteSpace(model.TarifsAssurance[i].Prix)
                        && !decimal.TryParse(model.TarifsAssurance[i].Prix, out _));

                    if (erreur)
                    {
                        ModelState.AddModelError($"TarifsAssurance[{i}].Prix", "Prix non valide.");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Analyse analyse = _mapper.Map<Analyse>(model);
                    analyse.Idanalyse = Guid.NewGuid();

                    // parcourir la selection des méthodes
                    if (model.IdsMethode != null)
                    {
                        foreach (var item in model.IdsMethode)
                        {
                            var obj = new Methodeanalyse
                            {
                                Methodeanalyseid = Guid.NewGuid(),
                                Idanalyse = analyse.Idanalyse,
                                Idmethode = int.Parse(item),
                                Isdefaultmethode = false
                            };
                            analyse.Methodeanalyses.Add(obj);
                        }
                    }

                    _context.Add(analyse);

                    // Enregistrer les paramètres

                    if (model.Parametres != null && model.Parametres.Any())
                    {
                        foreach (var paramVm in model.Parametres)
                        {
                            var param = _mapper.Map<Parametre>(paramVm);
                            param.Parametreid = Guid.NewGuid();
                            param.Idanalyse = analyse.Idanalyse;

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
                    }

                    // Enregistrer les valeurs de référence

                    if (model.ValeursReference != null && model.ValeursReference.Any())
                    {
                        foreach (var valRef in model.ValeursReference)
                        {
                            var valeurreference = _mapper.Map<Valeurreference>(valRef);
                            valeurreference.Valeurreferenceid = Guid.NewGuid();
                            valeurreference.Idanalyse = analyse.Idanalyse;

                            _context.Add(valeurreference);
                        }
                    }

                    // Enregistrer les tarifs d'assurance

                    if (model.TarifsAssurance != null && model.TarifsAssurance.Any())
                    {
                        var tarifanalyseassurances = model.TarifsAssurance
                            .Where(x => !string.IsNullOrWhiteSpace(x.Prix)
                                && decimal.TryParse(x.Prix, out _)
                                && decimal.Parse(x.Prix) > 0)
                            .Select(x => new Tarifanalyseassurance
                            {
                                Tarifanalyseassuranceid = Guid.NewGuid(),
                                Codeassurance = x.Codeassurance,
                                Idanalyse = analyse.Idanalyse,
                                Prix = decimal.Parse(x.Prix),
                            });

                        _context.AddRange(tarifanalyseassurances);
                    }

                    await _context.SaveChangesAsync();
                    
                    // Add success message to TempData
                    TempData["SuccessMessage"] = "L'analyse a été créée avec succès.";
                }
                catch (Exception)
                {
                    throw;
                }
                
                return RedirectToAction(nameof(Index));
            }

            // Recharger les listes pour l'affichage en cas d'erreur de validation
            var unitesError = _context.Unites?.ToList();
            var unitesErrorList = new List<SelectListItem> { new SelectListItem("---", string.Empty) };
            if (unitesError != null)
                unitesErrorList.AddRange(unitesError.Select(u => new SelectListItem(u.Name, u.Code)));

            var unitesisError = _context.Unites?.ToList();
            var unitesisErrorList = new List<SelectListItem> { new SelectListItem("---", string.Empty) };
            if (unitesisError != null)
                unitesisErrorList.AddRange(unitesisError.Select(u => new SelectListItem(u.Name, u.Code)));

            var analysesError = _context.Analyses?.ToList();
            var analysesErrorList = new List<SelectListItem> { new SelectListItem("---", string.Empty) };
            if (analysesError != null)
                analysesErrorList.AddRange(analysesError.Select(a => new SelectListItem(a.Nom, a.Idanalyse.ToString())));

            var automatesError = _context.Automates?.ToList();
            var automatesErrorList = new List<SelectListItem> { new SelectListItem("---", string.Empty) };
            if (automatesError != null)
                automatesErrorList.AddRange(automatesError.Select(a => new SelectListItem(a.Nom, a.Idautomate.ToString())));

            var laboratoiresError = _context.Loboratoires?.ToList();
            var laboratoiresErrorList = new List<SelectListItem> { new SelectListItem("---", string.Empty) };
            if (laboratoiresError != null)
                laboratoiresErrorList.AddRange(laboratoiresError.Select(l => new SelectListItem(l.Nom, l.Idlaboratoire.ToString())));

            var echantillonsError = _context.Natureechantillons?.ToList();
            var echantillonsErrorList = new List<SelectListItem> { new SelectListItem("---", string.Empty) };
            if (echantillonsError != null)
                echantillonsErrorList.AddRange(echantillonsError.Select(e => new SelectListItem(e.Nom, e.Idnatureechantillon.ToString())));

            var methodesError = _context.Methodes?.ToList();
            var methodesErrorList = new List<SelectListItem> { new SelectListItem("---", string.Empty) };
            if (methodesError != null)
                methodesErrorList.AddRange(methodesError.Select(m => new SelectListItem(m.Nom, m.Idmethode.ToString())));

            var assurancesError = _context.Assurances?.ToList();
            var assurancesErrorList = new List<SelectListItem> { new SelectListItem("---", string.Empty) };
            if (assurancesError != null)
                assurancesErrorList.AddRange(assurancesError.Select(a => new SelectListItem(a.Nom, a.Codeassurance)));

            var sexeautorises = new SelectList(_context.Sexeautorises, "Sexeautorisecode", "Valeur").ToList();
            sexeautorises.Insert(0, new SelectListItem("---", string.Empty));

            var types = Enum.GetValues(typeof(EnumTypeParametre))
                .Cast<EnumTypeParametre>()
                .Select(e => new SelectListItem
                {
                    Text = e.ToString(),
                    Value = e.ToString()
                }).ToList();
            ViewBag.TypeParametres = types;

            ViewData["Sexeautorisecode"] = sexeautorises;
            ViewData["Codeunite"] = unitesErrorList;
            ViewData["Codeunitesi"] = unitesisErrorList;
            ViewData["Idanalyseparent"] = analysesErrorList;
            ViewData["Idautomate"] = automatesErrorList;
            ViewData["Idlaboratoire"] = laboratoiresErrorList;
            ViewData["Idnatureechantillon"] = echantillonsErrorList;
            ViewData["IdsMethode"] = methodesErrorList;
            ViewData["Assurances"] = assurancesErrorList;

            return View(model);
        }

        // GET: AnalyseAvecOnglet/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var analyse = await _context.Analyses.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Idanalyse == id);

            if (analyse == null)
            {
                return NotFound();
            }

            var model = _mapper.Map<AnalyseAvecOngletEditVM>(analyse);

            // Charger les données des onglets existants
            var parametres = await _context.Parametres.AsNoTracking()
                .Where(p => p.Idanalyse == id)
                .ToListAsync();

            var valeursReference = await _context.Valeurreferences.AsNoTracking()
                .Where(p => p.Idanalyse == id)
                .ToListAsync();

            var TarifsAssurance = await _context.Tarifanalyseassurances.AsNoTracking()
                .Include(p => p.CodeassuranceNavigation)
                .Where(p => p.Idanalyse == id)
                .ToListAsync();

            if (parametres.Any())
            {
                var parametreEdits = _mapper.Map<List<ParametreEditAvecOngletVM>>(parametres);

                model.Parametres = parametreEdits;
            }
            else
            {
                model.Parametres = new List<ParametreEditAvecOngletVM>();
            }

            if (valeursReference.Any())
            {
                var valeurreferenceEdits = _mapper.Map<List<ValeurreferenceEditAvecOngletVM>>(valeursReference);

                model.ValeursReference = valeurreferenceEdits;
            }
            else
            {
                model.ValeursReference = new List<ValeurreferenceEditAvecOngletVM>();
            }

            if (TarifsAssurance.Any())
            {
                var tarifanalyseassuranceEdits = _mapper.Map<List<TarifanalyseassuranceEditAvecOngletVM>>(TarifsAssurance);

                var codesExistants = tarifanalyseassuranceEdits
                    .Select(x => x.Codeassurance)
                    .ToList();

                var placeholders = _context.Assurances.AsNoTracking()
                    .Where(x => !codesExistants.Contains(x.Codeassurance)) // uniquement ceux non présents
                    .Select(x => new TarifanalyseassuranceEditAvecOngletVM
                    {
                        Idanalyse = analyse.Idanalyse,
                        Codeassurance = x.Codeassurance,
                        Nom = x.Nom
                    })
                    .ToList();

                model.TarifsAssurance = tarifanalyseassuranceEdits.Concat(placeholders).ToList();
            }
            else
            {
                var placeholders = _context.Assurances.AsNoTracking()
                    .Select(x => new TarifanalyseassuranceEditAvecOngletVM
                    {
                        Idanalyse = analyse.Idanalyse,
                        Codeassurance = x.Codeassurance,
                        Nom = x.Nom
                    })
                    .ToList();

                model.TarifsAssurance = placeholders;
            }

            var unites = new SelectList(_context.Unites, "Code", "Name").ToList();
            unites.Insert(0, new SelectListItem("---", string.Empty));

            var unitesis = new SelectList(_context.Unites, "Code", "Name").ToList();
            unitesis.Insert(0, new SelectListItem("---", string.Empty));

            var analyses = new SelectList(_context.Analyses, "Idanalyse", "Nom", model.Idanalyseparent).ToList();
            analyses.Insert(0, new SelectListItem("---", string.Empty));

            var automates = new SelectList(_context.Automates, "Idautomate", "Nom", model.Idautomate).ToList();
            automates.Insert(0, new SelectListItem("---", string.Empty));

            var laboratoires = new SelectList(_context.Loboratoires, "Idlaboratoire", "Nom", model.Idlaboratoire).ToList();
            laboratoires.Insert(0, new SelectListItem("---", string.Empty));

            var echantillons = new SelectList(_context.Natureechantillons, "Idnatureechantillon", "Nom", model.Idnatureechantillon).ToList();
            echantillons.Insert(0, new SelectListItem("---", string.Empty));

            var methodes = new SelectList(_context.Methodes, "Idmethode", "Nom").ToList();
            methodes.Insert(0, new SelectListItem("---", string.Empty));

            var selectedMethodes = await _context.Methodeanalyses.AsNoTracking()
                .Include(x => x.IdanalyseNavigation)
                .Include(x => x.IdmethodeNavigation)
                .Where(x => x.Idanalyse == id)
                .Select(x => x.Idmethode.ToString())
                .ToListAsync();

            foreach (var item in methodes)
            {
                if (selectedMethodes.Contains(item.Value))
                    item.Selected = true;
            }

            var sexeautorises = new SelectList(_context.Sexeautorises, "Sexeautorisecode", "Valeur").ToList();
            sexeautorises.Insert(0, new SelectListItem("---", string.Empty));

            var types = Enum.GetValues(typeof(EnumTypeParametre))
                .Cast<EnumTypeParametre>()
                .Select(e => new SelectListItem
                {
                    Text = e.ToString(),
                    Value = e.ToString()
                }).ToList();
            ViewBag.TypeParametres = types;

            ViewData["Sexeautorisecode"] = sexeautorises;
            ViewData["Codeunite"] = unites;
            ViewData["Codeunitesi"] = unitesis;
            ViewData["Idanalyseparent"] = analyses;
            ViewData["Idautomate"] = automates;
            ViewData["Idlaboratoire"] = laboratoires;
            ViewData["Idnatureechantillon"] = echantillons;
            ViewData["IdsMethode"] = methodes;

            var assurances = new SelectList(_context.Assurances, "Codeassurance", "Nom").ToList();
            assurances.Insert(0, new SelectListItem("---", string.Empty));
            ViewData["Assurances"] = assurances;

            return View(model);
        }

        //POST: AnalyseAvecOnglet/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, AnalyseAvecOngletEditVM model)
        {
            if (model == null)
            {
                return BadRequest("Model binding failed");
            }
            
            if (id != model.Idanalyse)
            {
                return NotFound();
            }

            if (model.Parametres != null && model.Parametres.Any())
            {
                // Vérifier l'existence des valeurs des formules et des listes avant la validation du modèle
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

                // Validation des formules avant la validation du modèle
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

            if (model.TarifsAssurance != null && model.TarifsAssurance.Any())
            {
                // Vérifier que les prix des tarifs sont valide (décimal supérieure à 0)
                for (int i = 0; i < model.TarifsAssurance.Count; i++)
                {
                    var erreur = (!string.IsNullOrWhiteSpace(model.TarifsAssurance[i].Prix)
                        && decimal.TryParse(model.TarifsAssurance[i].Prix, out _)
                        && decimal.Parse(model.TarifsAssurance[i].Prix) <= 0)
                        ||
                        (!string.IsNullOrWhiteSpace(model.TarifsAssurance[i].Prix)
                        && !decimal.TryParse(model.TarifsAssurance[i].Prix, out _));

                    if (erreur)
                    {
                        ModelState.AddModelError($"TarifsAssurance[{i}].Prix", "Prix non valide.");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var analyse = _mapper.Map<Analyse>(model);

                    _context.Update(analyse);

                    // ------------------------ Gestion des methodes ---------------------------

                    // Anciennes valeurs en base
                    var anListeMethode = await _context.Methodeanalyses.AsNoTracking()
                        .Where(x => x.Idanalyse == analyse.Idanalyse)
                        .ToListAsync();

                    // Nouvelles valeurs du formulaire (ex: ["1", "2", "3"])
                    var nvIdsMethode = model.IdsMethode?.
                        Select(id => int.Parse(id))
                        .ToList() ?? new List<int>();

                    //LINQ — Ce qui n’existe plus → à supprimer
                    var listeMethodeToDelete = anListeMethode
                        .Where(x => !nvIdsMethode.Contains(x.Idmethode))
                        .ToList();

                    if (listeMethodeToDelete.Any())
                    {
                        _context.Methodeanalyses.RemoveRange(listeMethodeToDelete);
                    }

                    //LINQ — Ce qui est nouveau → à créer
                    var anIdsMethode = anListeMethode.Select(x => x.Idmethode).ToHashSet();

                    var listeMethodeToCreate = nvIdsMethode
                        .Where(id => !anIdsMethode.Contains(id)) // uniquement les nouveaux
                        .Select(id => new Methodeanalyse
                        {
                            Methodeanalyseid = Guid.NewGuid(),
                            Idanalyse = analyse.Idanalyse,
                            Idmethode = id,
                            Isdefaultmethode = false // ou autre valeur depuis un autre champ de model
                        })
                        .ToList();

                    if (listeMethodeToCreate.Any())
                    {
                        _context.Methodeanalyses.AddRange(listeMethodeToCreate);
                    }

                    //LINQ — Ce qui existe toujours → à mettre à jour
                    var listeMethodeToUpdate = anListeMethode
                        .Where(x => nvIdsMethode.Contains(x.Idmethode)) // seulement ceux qui restent
                        .Select(x =>
                        {
                            x.Isdefaultmethode = x.Isdefaultmethode; // ou autre mise à jour depuis un modèle plus riche
                            return x;
                        })
                        .ToList();

                    if (listeMethodeToUpdate.Any())
                    {
                        _context.Methodeanalyses.UpdateRange(listeMethodeToUpdate);
                    }

                    // ----------------------- Gestion des paramètres ------------------------------

                    // 1. Charger les anciens paramètres sans tracking
                    var anListeParametre = await _context.Parametres
                        .AsNoTracking()
                        .Where(x => x.Idanalyse == id)
                        .ToListAsync();

                    // 2. Marquer ceux à supprimer
                    var parametreToDelete = anListeParametre
                        .Where(old => model.Parametres?.All(m => m.Parametreid != old.Parametreid) ?? true)
                        .ToList();

                    if (parametreToDelete.Any())
                    {
                        _context.Parametres.RemoveRange(parametreToDelete);
                    }

                    // 3. Ajouter les nouveaux
                    var parametreToCreate = (model.Parametres ?? new List<ParametreEditAvecOngletVM>())
                        .Where(p => p.Parametreid == null && !anListeParametre.Any(x => x.Parametreid == p.Parametreid))
                        .Select(p =>
                        {
                            var entity = _mapper.Map<Parametre>(p);
                            entity.Parametreid = Guid.NewGuid();
                            entity.Idanalyse = analyse.Idanalyse;

                            if (p.Builder.Type == EnumTypeParametre.Texte.ToString())
                            {
                                p.Builder.Valeur = "";
                            }

                            entity.Valuebuilder = JsonConvert.SerializeObject(p.Builder, Formatting.Indented, new JsonSerializerSettings
                            {
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                            });

                            return entity;
                        })
                        .ToList();

                    if (parametreToCreate.Any())
                    {
                        _context.Parametres.AddRange(parametreToCreate);
                    }

                    // 4. Mise à jour des existants
                    var parametreToUpdate = (model.Parametres ?? new List<ParametreEditAvecOngletVM>())
                        .Where(p => p.Parametreid != null && anListeParametre.Any(a => a.Parametreid == p.Parametreid))
                        .Select(p =>
                        {
                            var entity = _mapper.Map<Parametre>(p);

                            if (p.Builder.Type == EnumTypeParametre.Texte.ToString())
                            {
                                p.Builder.Valeur = "";
                            }

                            entity.Valuebuilder = JsonConvert.SerializeObject(p.Builder, Formatting.Indented, new JsonSerializerSettings
                            {
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                            });

                            return entity;
                        })
                        .ToList();

                    if (parametreToUpdate.Any())
                    {
                        _context.Parametres.UpdateRange(parametreToUpdate);
                    }

                    // ------------------- Gestion des valeurs de référence --------------------------

                    // 1. Charger les anciens valeurreferences sans tracking
                    var anListeValeurreference = await _context.Valeurreferences
                        .AsNoTracking()
                        .Where(x => x.Idanalyse == id)
                        .ToListAsync();

                    // 2. Marquer ceux à supprimer
                    var valeurreferenceToDelete = anListeValeurreference
                        .Where(old => model.ValeursReference?.All(m => m.Valeurreferenceid != old.Valeurreferenceid) ?? true)
                        .ToList();

                    if (valeurreferenceToDelete.Any())
                    {
                        _context.Valeurreferences.RemoveRange(valeurreferenceToDelete);
                    }

                    // 3. Ajouter les nouveaux
                    var valeurreferenceToCreate = (model.ValeursReference ?? new List<ValeurreferenceEditAvecOngletVM>())
                        .Where(p => p.Valeurreferenceid == null && !anListeValeurreference.Any(x => x.Valeurreferenceid == p.Valeurreferenceid))
                        .Select(p =>
                        {
                            var entity = _mapper.Map<Valeurreference>(p);
                            entity.Valeurreferenceid = Guid.NewGuid();
                            entity.Idanalyse = analyse.Idanalyse;

                            return entity;
                        })
                        .ToList();

                    if (valeurreferenceToCreate.Any())
                    {
                        _context.Valeurreferences.AddRange(valeurreferenceToCreate);
                    }

                    // 4. Mise à jour des existants
                    var valeurreferenceToUpdate = (model.ValeursReference ?? new List<ValeurreferenceEditAvecOngletVM>())
                        .Where(p => p.Valeurreferenceid != null && anListeValeurreference.Any(a => a.Valeurreferenceid == p.Valeurreferenceid))
                        .Select(p =>
                        {
                            var entity = _mapper.Map<Valeurreference>(p);

                            return entity;
                        })
                        .ToList();

                    if (valeurreferenceToUpdate.Any())
                    {
                        _context.Valeurreferences.UpdateRange(valeurreferenceToUpdate);
                    }

                    //// ------------------- Gestion des tarifs d'assurance ----------------------------

                    // 1. Charger les anciens tarifanalyseassurances sans tracking
                    var anListeTarifanalyseassurance = await _context.Tarifanalyseassurances
                        .AsNoTracking()
                        .Where(x => x.Idanalyse == id)
                        .ToListAsync();

                    // 2. Marquer ceux à supprimer
                    var tarifanalyseassuranceToDelete = anListeTarifanalyseassurance
                        .Where(old => model.TarifsAssurance?.All(m => m.Tarifanalyseassuranceid != old.Tarifanalyseassuranceid) ?? true)
                        .ToList();

                    if (tarifanalyseassuranceToDelete.Any())
                    {
                        _context.Tarifanalyseassurances.RemoveRange(tarifanalyseassuranceToDelete);
                    }

                    // 3. Ajouter les nouveaux
                    var tarifanalyseassuranceToCreate = (model.TarifsAssurance ?? new List<TarifanalyseassuranceEditAvecOngletVM>())
                        .Where(p => p.Tarifanalyseassuranceid == null && !string.IsNullOrWhiteSpace(p.Prix) && !anListeTarifanalyseassurance.Any(x => x.Tarifanalyseassuranceid == p.Tarifanalyseassuranceid))
                        .Select(p =>
                        {
                            var entity = _mapper.Map<Tarifanalyseassurance>(p);
                            entity.Tarifanalyseassuranceid = Guid.NewGuid();
                            entity.Idanalyse = analyse.Idanalyse;

                            return entity;
                        })
                        .ToList();

                    if (tarifanalyseassuranceToCreate.Any())
                    {
                        _context.Tarifanalyseassurances.AddRange(tarifanalyseassuranceToCreate);
                    }

                    // 4. Mise à jour des existants
                    var tarifanalyseassuranceToUpdate = (model.TarifsAssurance ?? new List<TarifanalyseassuranceEditAvecOngletVM>())
                        .Where(p => p.Tarifanalyseassuranceid != null && anListeTarifanalyseassurance.Any(a => a.Tarifanalyseassuranceid == p.Tarifanalyseassuranceid))
                        .Select(p =>
                        {
                            var entity = _mapper.Map<Tarifanalyseassurance>(p);

                            return entity;
                        });

                    if (tarifanalyseassuranceToUpdate.Any())
                    {
                        _context.Tarifanalyseassurances.UpdateRange(tarifanalyseassuranceToUpdate);
                    }

                    await _context.SaveChangesAsync();
                    
                    // Add success message to TempData
                    TempData["SuccessMessage"] = "L'analyse a été modifiée avec succès.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnalyseExists(model.Idanalyse))
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

            var unites = new SelectList(_context.Unites, "Code", "Name").ToList();
            unites.Insert(0, new SelectListItem("---", string.Empty));

            var unitesis = new SelectList(_context.Unites, "Code", "Name").ToList();
            unitesis.Insert(0, new SelectListItem("---", string.Empty));

            var analyses = new SelectList(_context.Analyses, "Idanalyse", "Nom", model.Idanalyseparent).ToList();
            analyses.Insert(0, new SelectListItem("---", string.Empty));

            var automates = new SelectList(_context.Automates, "Idautomate", "Nom", model.Idautomate).ToList();
            automates.Insert(0, new SelectListItem("---", string.Empty));

            var laboratoires = new SelectList(_context.Loboratoires, "Idlaboratoire", "Nom", model.Idlaboratoire).ToList();
            laboratoires.Insert(0, new SelectListItem("---", string.Empty));

            var echantillons = new SelectList(_context.Natureechantillons, "Idnatureechantillon", "Nom", model.Idnatureechantillon).ToList();
            echantillons.Insert(0, new SelectListItem("---", string.Empty));

            var methodes = new SelectList(_context.Methodes, "Idmethode", "Nom").ToList();
            methodes.Insert(0, new SelectListItem("---", string.Empty));

            var selectedMethodes = model.IdsMethode;

            foreach (var item in methodes)
            {
                if (selectedMethodes.Contains(item.Value))
                    item.Selected = true;
            }

            var sexeautorises = new SelectList(_context.Sexeautorises, "Sexeautorisecode", "Valeur").ToList();
            sexeautorises.Insert(0, new SelectListItem("---", string.Empty));

            var types = Enum.GetValues(typeof(EnumTypeParametre))
                .Cast<EnumTypeParametre>()
                .Select(e => new SelectListItem
                {
                    Text = e.ToString(),
                    Value = e.ToString()
                }).ToList();
            ViewBag.TypeParametres = types;

            ViewData["Sexeautorisecode"] = sexeautorises;
            ViewData["Codeunite"] = unites;
            ViewData["Codeunitesi"] = unitesis;
            ViewData["Idanalyseparent"] = analyses;
            ViewData["Idautomate"] = automates;
            ViewData["Idlaboratoire"] = laboratoires;
            ViewData["Idnatureechantillon"] = echantillons;
            ViewData["IdsMethode"] = methodes;

            var assurances = new SelectList(_context.Assurances, "Codeassurance", "Nom").ToList();
            assurances.Insert(0, new SelectListItem("---", string.Empty));
            ViewData["Assurances"] = assurances;

            return View(model);
        }

        //GET: AnalyseAvecOnglet/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var analyse = await _context.Analyses
                .Include(a => a.CodeuniteNavigation)
                .Include(a => a.CodeunitesiNavigation)
                .Include(a => a.IdanalyseparentNavigation)
                .Include(a => a.IdautomateNavigation)
                .Include(a => a.IdlaboratoireNavigation)
                .Include(a => a.IdnatureechantillonNavigation)
                .FirstOrDefaultAsync(m => m.Idanalyse == id);
            if (analyse == null)
            {
                return NotFound();
            }

            return View(analyse);
        }

        // POST: AnalyseAvecOnglet/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var analyse = await _context.Analyses.FindAsync(id);
            if (analyse != null)
            {
                _context.Analyses.Remove(analyse);
            }

            await _context.SaveChangesAsync();
            
            // Add success message to TempData
            TempData["SuccessMessage"] = "L'analyse a été supprimée avec succès.";
            
            return RedirectToAction(nameof(Index));
        }

        private bool AnalyseExists(Guid id)
        {
            return _context.Analyses.Any(e => e.Idanalyse == id);
        }

        #region handlers

        [HttpPost, ActionName("DeleteAnalyse")]
        public async Task<IActionResult> DeleteAnalyseAsync(Guid id)
        {
            JsonResponseViewModel resultat = new JsonResponseViewModel();

            if (id != Guid.Empty)
            {
                var analyse = await _context.Analyses.AsNoTracking()
                    .Where(x => x.Idanalyse == id)
                    .FirstOrDefaultAsync();

                if (analyse != null)
                {
                    _context.Remove(analyse);
                    await _context.SaveChangesAsync();

                    resultat.success = true;
                }
            }

            return Ok(resultat);
        }

        [AcceptVerbs("Get", "Post")]
        public IActionResult IsNomAvailable(string nom)
        {
            var exists = _context.Analyses.Any(u => u.Nom == nom);
            if (exists)
                return Json($"Nom {nom} is already in use.");
            return Json(true);
        }

        #endregion
    }
}
