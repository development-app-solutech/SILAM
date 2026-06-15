using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProlabWeb.Db;
using ProlabWeb.ViewModels;

namespace ProlabWeb.Controllers
{
    [AllowAnonymous]
    public class TarifanalyseassuranceController : Controller
    {
        private readonly ProlabwebContext _context;

        public TarifanalyseassuranceController(ProlabwebContext context)
        {
            _context = context;
        }

        // GET: Tarifanalyseassurance
        public async Task<IActionResult> Index()
        {
            var prolabwebContext = _context.Tarifanalyseassurances.AsNoTracking()
                .Include(t => t.CodeassuranceNavigation)
                .Include(t => t.IdanalyseNavigation)
                .AsEnumerable()
                .Select(t => t.CodeassuranceNavigation)
                .DistinctBy(t => t.Codeassurance)
                .OrderBy(t => t.Nom)
                .ToList();

            return View(prolabwebContext);
        }

        // GET: Tarifanalyseassurance/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tarifanalyseassurance = await _context.Tarifanalyseassurances
                .Include(t => t.CodeassuranceNavigation)
                .Include(t => t.IdanalyseNavigation)
                .FirstOrDefaultAsync(m => m.Tarifanalyseassuranceid == id);
            if (tarifanalyseassurance == null)
            {
                return NotFound();
            }

            return View(tarifanalyseassurance);
        }

        // GET: Tarifanalyseassurance/Create
        public async Task<IActionResult> Create(int? idlaboratoire = null)
        {
            var analysesQuery = _context.Analyses.AsNoTracking();
            
            // Filtrer par laboratoire si spécifié
            if (idlaboratoire.HasValue)
            {
                analysesQuery = analysesQuery.Where(x => x.Idlaboratoire == idlaboratoire.Value);
            }
            
            var tarifs = await analysesQuery
                .Select(x => new TarifAnalyseVM
                {
                    Idanalyse = x.Idanalyse,
                    Nom = x.Nom,
                    Prix = ""
                })
                .ToListAsync();

            var model = new TarifanalyseassuranceCreateVM
            {
                Codeassurance = string.Empty,
                Idlaboratoire = idlaboratoire,
                Tarif = tarifs
            };

            var assurances = new SelectList(_context.Assurances, "Codeassurance", "Nom").ToList();
            assurances.Insert(0, new SelectListItem("---", string.Empty));

            var laboratoires = new SelectList(_context.Loboratoires.Where(l => l.Isactive), "Idlaboratoire", "Nom", idlaboratoire).ToList();
            laboratoires.Insert(0, new SelectListItem("---", string.Empty));

            ViewData["Codeassurance"] = assurances;
            ViewData["Idlaboratoire"] = laboratoires;

            return View(model);
        }

        // POST: Tarifanalyseassurance/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TarifanalyseassuranceCreateVM model)
        {
            if (await _context.Tarifanalyseassurances.AsNoTracking()
                .AnyAsync(u => u.Codeassurance == model.Codeassurance))
            {
                ModelState.AddModelError("Codeassurance", "Tarif Assurance already exists.");
            }

            for (int i = 0; i < model.Tarif.Count; i++)
            {
                var erreur = (!string.IsNullOrWhiteSpace(model.Tarif[i].Prix)
                    && decimal.TryParse(model.Tarif[i].Prix, out _)
                    && decimal.Parse(model.Tarif[i].Prix) <= 0)
                    ||
                    (!string.IsNullOrWhiteSpace(model.Tarif[i].Prix)
                    && !decimal.TryParse(model.Tarif[i].Prix, out _));

                if (erreur)
                {
                    ModelState.AddModelError($"Tarif[{i}].Prix", "Incorrect value.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var tarifanalyseassurances = model.Tarif
                        .Where(x => !string.IsNullOrWhiteSpace(x.Prix) 
                            && decimal.TryParse(x.Prix, out _) 
                            && decimal.Parse(x.Prix) > 0)
                        .Select(x => new Tarifanalyseassurance
                        {
                            Tarifanalyseassuranceid = Guid.NewGuid(),
                            Codeassurance = model.Codeassurance,
                            Idanalyse = x.Idanalyse,
                            Prix = decimal.Parse(x.Prix),
                        });

                    _context.AddRange(tarifanalyseassurances);
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            var assurances = new SelectList(_context.Assurances, "Codeassurance", "Nom", model.Codeassurance).ToList();
            assurances.Insert(0, new SelectListItem("---", string.Empty));

            var laboratoires = new SelectList(_context.Loboratoires.Where(l => l.Isactive), "Idlaboratoire", "Nom", model.Idlaboratoire).ToList();
            laboratoires.Insert(0, new SelectListItem("---", string.Empty));

            ViewData["Codeassurance"] = assurances;
            ViewData["Idlaboratoire"] = laboratoires;

            return View(model);
        }

        // GET: Tarifanalyseassurance/Edit/5
        public async Task<IActionResult> Edit(string? id, int? idlaboratoire = null)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assurance = await _context.Assurances
                .FirstOrDefaultAsync(m => m.Codeassurance == id);

            if (assurance == null)
            {
                return NotFound();
            }

            var tarifanalyseassurances = await _context.Tarifanalyseassurances.AsNoTracking()
                .Where(x => x.Codeassurance == id)
                .ToListAsync();

            var analysesQuery = _context.Analyses.AsNoTracking();
            
            // Filtrer par laboratoire si spécifié
            if (idlaboratoire.HasValue)
            {
                analysesQuery = analysesQuery.Where(x => x.Idlaboratoire == idlaboratoire.Value);
            }
            
            var tarifs = await analysesQuery
                .Select(x => new TarifAnalyseVM
                {
                    Idanalyse = x.Idanalyse,
                    Nom = x.Nom,
                    Prix = ""
                })
                .ToListAsync();

            // completer les prix

            foreach (var item in tarifs)
            {
                var found = tarifanalyseassurances
                    .Where(x => x.Idanalyse == item.Idanalyse)
                    .FirstOrDefault();

                if (found != null)
                {
                    item.Prix = found.Prix.ToString();
                }
            }

            var model = new TarifanalyseassuranceEditVM
            {
                Codeassurance = id,
                Idlaboratoire = idlaboratoire,
                Tarif = tarifs
            };

            var assurances = new SelectList(_context.Assurances, "Codeassurance", "Nom", model.Codeassurance).ToList();
            assurances.Insert(0, new SelectListItem("---", string.Empty));

            var laboratoires = new SelectList(_context.Loboratoires.Where(l => l.Isactive), "Idlaboratoire", "Nom", idlaboratoire).ToList();
            laboratoires.Insert(0, new SelectListItem("---", string.Empty));

            ViewData["Codeassurance"] = assurances;
            ViewData["Idlaboratoire"] = laboratoires;

            return View(model);
        }

        // POST: Tarifanalyseassurance/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, TarifanalyseassuranceEditVM model)
        {
            if (id != model.Codeassurance)
            {
                return NotFound();
            }

            for (int i = 0; i < model.Tarif.Count; i++)
            {
                var erreur = (!string.IsNullOrWhiteSpace(model.Tarif[i].Prix)
                    && decimal.TryParse(model.Tarif[i].Prix, out _)
                    && decimal.Parse(model.Tarif[i].Prix) <= 0)
                    ||
                    (!string.IsNullOrWhiteSpace(model.Tarif[i].Prix)
                    && !decimal.TryParse(model.Tarif[i].Prix, out _));

                if (erreur)
                {
                    ModelState.AddModelError($"Tarif[{i}].Prix", "Incorrect value.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var anListTarifanalyseassurance = await _context.Tarifanalyseassurances.AsNoTracking()
                        .Where(x => x.Codeassurance == id)
                        .ToListAsync();

                    var nvListTarif = model.Tarif
                        .Where(x => !string.IsNullOrWhiteSpace(x.Prix) 
                            && decimal.TryParse(x.Prix, out _) 
                            && decimal.Parse(x.Prix) > 0)
                        .ToList();

                    // parcourir la selection ancienne

                    var toDelete = new List<Tarifanalyseassurance>();

                    foreach (var item in anListTarifanalyseassurance)
                    {
                        // chercher dans la nouvelle selection

                        var found = nvListTarif
                            .Where(x => x.Idanalyse == item.Idanalyse)
                            .FirstOrDefault();

                        if (found == null)
                        {
                            // supprimer

                            toDelete.Add(item);
                        }
                    }
                    if (toDelete.Any())
                    {
                        _context.RemoveRange(toDelete);
                    }

                    // parcourir la selection nouvelle

                    var toUpdate = new List<Tarifanalyseassurance>();
                    var toCreate = new List<Tarifanalyseassurance>();

                    foreach (var item in nvListTarif)
                    {
                        // chercher dans la selection ancienne

                        var found = anListTarifanalyseassurance
                            .Where(x => x.Idanalyse == item.Idanalyse)
                            .FirstOrDefault();

                        if (found == null)
                        {
                            // ajouter

                            var obj = new Tarifanalyseassurance
                            {
                                Tarifanalyseassuranceid = Guid.NewGuid(),
                                Codeassurance = model.Codeassurance,
                                Idanalyse = item.Idanalyse,
                                Prix = decimal.Parse(item.Prix),
                            };
                            toCreate.Add(obj);
                        }
                        else
                        {
                            // mettre � jour

                            found.Prix = decimal.Parse(item.Prix);
                            toUpdate.Add(found);
                        }
                    }
                    if (toCreate.Any())
                    {
                        _context.AddRange(toCreate);
                    }
                    if (toUpdate.Any())
                    {
                        _context.UpdateRange(toUpdate);
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            var assurances = new SelectList(_context.Assurances, "Codeassurance", "Nom", model.Codeassurance).ToList();
            assurances.Insert(0, new SelectListItem("---", string.Empty));

            var laboratoires = new SelectList(_context.Loboratoires.Where(l => l.Isactive), "Idlaboratoire", "Nom", model.Idlaboratoire).ToList();
            laboratoires.Insert(0, new SelectListItem("---", string.Empty));

            ViewData["Codeassurance"] = assurances;
            ViewData["Idlaboratoire"] = laboratoires;

            return View(model);
        }

        // GET: Tarifanalyseassurance/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tarifanalyseassurance = await _context.Tarifanalyseassurances
                .Include(t => t.CodeassuranceNavigation)
                .Include(t => t.IdanalyseNavigation)
                .FirstOrDefaultAsync(m => m.Tarifanalyseassuranceid == id);
            if (tarifanalyseassurance == null)
            {
                return NotFound();
            }

            return View(tarifanalyseassurance);
        }

        // POST: Tarifanalyseassurance/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            //var tarifanalyseassurance = await _context.Tarifanalyseassurances.FindAsync(id);
            //if (tarifanalyseassurance != null)
            //{
            //    _context.Tarifanalyseassurances.Remove(tarifanalyseassurance);
            //}

            //await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TarifanalyseassuranceExists(Guid id)
        {
            return _context.Tarifanalyseassurances.Any(e => e.Tarifanalyseassuranceid == id);
        }

        #region handlers

        [HttpPost, ActionName("DeleteTarifanalyseassurance")]
        public async Task<IActionResult> DeleteTarifanalyseassuranceAsync(string id)
        {
            JsonResponseViewModel resultat = new JsonResponseViewModel();

            //if (!string.IsNullOrWhiteSpace(id))
            //{
            //    var tarifanalyseassurances = await _context.Tarifanalyseassurances.AsNoTracking()
            //        .Where(x => x.Codeassurance == id)
            //        .ToListAsync();

            //    if (tarifanalyseassurances.Any())
            //    {
            //        _context.Tarifanalyseassurances.RemoveRange(tarifanalyseassurances);
            //        await _context.SaveChangesAsync();

            //        resultat.success = true;
            //    }
            //}

            return Ok(resultat);
        }

        [HttpPost, ActionName("GetAnalysesByLaboratoire")]
        public async Task<IActionResult> GetAnalysesByLaboratoireAsync(int? idlaboratoire, string? codeassurance = null)
        {
            var analysesQuery = _context.Analyses.AsNoTracking();
            
            // Filtrer par laboratoire si spécifié
            if (idlaboratoire.HasValue && idlaboratoire.Value > 0)
            {
                analysesQuery = analysesQuery.Where(x => x.Idlaboratoire == idlaboratoire.Value);
            }
            
            var analyses = await analysesQuery
                .Select(x => new TarifAnalyseVM
                {
                    Idanalyse = x.Idanalyse,
                    Nom = x.Nom,
                    Prix = ""
                })
                .ToListAsync();

            // Si on est en mode édition, récupérer les prix existants
            if (!string.IsNullOrEmpty(codeassurance))
            {
                var tarifanalyseassurances = await _context.Tarifanalyseassurances.AsNoTracking()
                    .Where(x => x.Codeassurance == codeassurance)
                    .ToListAsync();

                foreach (var item in analyses)
                {
                    var found = tarifanalyseassurances
                        .Where(x => x.Idanalyse == item.Idanalyse)
                        .FirstOrDefault();

                    if (found != null)
                    {
                        item.Prix = found.Prix.ToString();
                    }
                }
            }

            return Json(analyses);
        }

        // Méthode GET temporaire pour test
        [HttpGet, ActionName("GetAnalysesByLaboratoireTest")]
        public async Task<IActionResult> GetAnalysesByLaboratoireTestAsync(int? idlaboratoire, string? codeassurance = null)
        {
            var analysesQuery = _context.Analyses.AsNoTracking();
            
            // Filtrer par laboratoire si spécifié
            if (idlaboratoire.HasValue && idlaboratoire.Value > 0)
            {
                analysesQuery = analysesQuery.Where(x => x.Idlaboratoire == idlaboratoire.Value);
            }
            
            var analyses = await analysesQuery
                .Select(x => new TarifAnalyseVM
                {
                    Idanalyse = x.Idanalyse,
                    Nom = x.Nom,
                    Prix = ""
                })
                .ToListAsync();

            // Si on est en mode édition, récupérer les prix existants
            if (!string.IsNullOrEmpty(codeassurance))
            {
                var tarifanalyseassurances = await _context.Tarifanalyseassurances.AsNoTracking()
                    .Where(x => x.Codeassurance == codeassurance)
                    .ToListAsync();

                foreach (var item in analyses)
                {
                    var found = tarifanalyseassurances
                        .Where(x => x.Idanalyse == item.Idanalyse)
                        .FirstOrDefault();

                    if (found != null)
                    {
                        item.Prix = found.Prix.ToString();
                    }
                }
            }

            return Json(analyses);
        }

        #endregion
    }
}

