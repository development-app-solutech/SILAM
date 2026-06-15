using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProlabWeb.Db;
using ProlabWeb.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProlabWeb.Controllers
{
    [AllowAnonymous]
    public class TarifcategorieassuranceController : Controller
    {
        private readonly ProlabwebContext _context;

        public TarifcategorieassuranceController(ProlabwebContext context)
        {
            _context = context;
        }

        // GET: Tarifcategorieassurance
        public async Task<IActionResult> Index()
        {
            var prolabwebContext = _context.Tarifcategorieassurances.AsNoTracking()
                .Include(t => t.Categorie)
                .Include(t => t.CodeassuranceNavigation)
                .AsEnumerable()
                .Select(t => t.CodeassuranceNavigation)
                .DistinctBy(t => t.Codeassurance)
                .OrderBy(t => t.Nom)
                .ToList();

            return View(prolabwebContext);
        }

        // GET: Tarifcategorieassurance/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tarifcategorieassurance = await _context.Tarifcategorieassurances
                .Include(t => t.Categorie)
                .Include(t => t.CodeassuranceNavigation)
                .FirstOrDefaultAsync(m => m.Tarifcategorieassuranceid == id);
            if (tarifcategorieassurance == null)
            {
                return NotFound();
            }

            return View(tarifcategorieassurance);
        }

        // GET: Tarifcategorieassurance/Create
        public async Task<IActionResult> CreateAsync(int? idlaboratoire = null)
        {
            var query = _context.Categories.AsNoTracking();

            // Filtrer par laboratoire si fourni
            if (idlaboratoire.HasValue)
            {
                // Filtrer les catégories qui ont des analyses dans le laboratoire spécifié
                query = query.Where(c => c.Categorieanalyses
                    .Any(ca => ca.IdanalyseNavigation.Idlaboratoire == idlaboratoire.Value));
            }

            var tarifs = await query
                .Select(x => new TarifCategorieVM
                {
                    Categorieid = x.Categorieid,
                    Nom = x.Nom,
                    Prix = ""
                })
                .ToListAsync();

            var model = new TarifcategorieassuranceCreateVM
            {
                Codeassurance = string.Empty,
                Idlaboratoire = idlaboratoire,
                Tarif = tarifs
            };

            var assurances = new SelectList(_context.Assurances, "Codeassurance", "Nom").ToList();
            assurances.Insert(0, new SelectListItem("---", string.Empty));

            var laboratoires = new SelectList(_context.Loboratoires, "Idlaboratoire", "Nom", idlaboratoire).ToList();
            laboratoires.Insert(0, new SelectListItem("---", string.Empty));

            ViewData["Codeassurance"] = assurances;
            ViewData["Idlaboratoire"] = laboratoires;

            return View(model);
        }

        // POST: Tarifcategorieassurance/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TarifcategorieassuranceCreateVM model)
        {
            if (await _context.Tarifcategorieassurances.AsNoTracking()
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
                    var tarifcategorieassurances = model.Tarif
                        .Where(x => !string.IsNullOrWhiteSpace(x.Prix)
                            && decimal.TryParse(x.Prix, out _)
                            && decimal.Parse(x.Prix) > 0)
                        .Select(x => new Tarifcategorieassurance
                        {
                            Tarifcategorieassuranceid = Guid.NewGuid(),
                            Codeassurance = model.Codeassurance,
                            Categorieid = x.Categorieid,
                            Prix = decimal.Parse(x.Prix),
                        });

                    _context.AddRange(tarifcategorieassurances);
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

            ViewData["Codeassurance"] = assurances;

            return View(model);
        }

        // GET: Tarifcategorieassurance/Edit/5
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

            var tarifcategorieassurances = await _context.Tarifcategorieassurances.AsNoTracking()
                .Where(x => x.Codeassurance == id)
                .ToListAsync();

            var query = _context.Categories.AsNoTracking();

            // Filtrer par laboratoire si fourni
            if (idlaboratoire.HasValue)
            {
                // Filtrer les catégories qui ont des analyses dans le laboratoire spécifié
                query = query.Where(c => c.Categorieanalyses
                    .Any(ca => ca.IdanalyseNavigation.Idlaboratoire == idlaboratoire.Value));
            }

            var tarifs = await query
                .Select(x => new TarifCategorieVM
                {
                    Categorieid = x.Categorieid,
                    Nom = x.Nom,
                    Prix = ""
                })
                .ToListAsync();

            // completer les prix

            foreach (var item in tarifs)
            {
                var found = tarifcategorieassurances
                    .Where(x => x.Categorieid == item.Categorieid)
                    .FirstOrDefault();

                if (found != null)
                {
                    item.Prix = found.Prix.ToString();
                }
            }

            var model = new TarifcategorieassuranceEditVM
            {
                Codeassurance = id,
                Idlaboratoire = idlaboratoire,
                Tarif = tarifs
            };

            var assurances = new SelectList(_context.Assurances, "Codeassurance", "Nom", model.Codeassurance).ToList();
            assurances.Insert(0, new SelectListItem("---", string.Empty));

            var laboratoires = new SelectList(_context.Loboratoires, "Idlaboratoire", "Nom", idlaboratoire).ToList();
            laboratoires.Insert(0, new SelectListItem("---", string.Empty));

            ViewData["Codeassurance"] = assurances;
            ViewData["Idlaboratoire"] = laboratoires;

            return View(model);
        }

        // POST: Tarifcategorieassurance/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, TarifcategorieassuranceEditVM model)
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
                    var anListTarifcategorieassurance = await _context.Tarifcategorieassurances.AsNoTracking()
                        .Where(x => x.Codeassurance == id)
                        .ToListAsync();

                    var nvListTarif = model.Tarif
                        .Where(x => !string.IsNullOrWhiteSpace(x.Prix)
                            && decimal.TryParse(x.Prix, out _)
                            && decimal.Parse(x.Prix) > 0)
                        .ToList();

                    // parcourir la selection ancienne

                    var toDelete = new List<Tarifcategorieassurance>();

                    foreach (var item in anListTarifcategorieassurance)
                    {
                        // chercher dans la nouvelle selection

                        var found = nvListTarif
                            .Where(x => x.Categorieid == item.Categorieid)
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

                    var toUpdate = new List<Tarifcategorieassurance>();
                    var toCreate = new List<Tarifcategorieassurance>();

                    foreach (var item in nvListTarif)
                    {
                        // chercher dans la selection ancienne

                        var found = anListTarifcategorieassurance
                            .Where(x => x.Categorieid == item.Categorieid)
                            .FirstOrDefault();

                        if (found == null)
                        {
                            // ajouter

                            var obj = new Tarifcategorieassurance
                            {
                                Tarifcategorieassuranceid = Guid.NewGuid(),
                                Codeassurance = model.Codeassurance,
                                Categorieid = item.Categorieid,
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

            ViewData["Codeassurance"] = assurances;

            return View(model);
        }

        // GET: Tarifcategorieassurance/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tarifcategorieassurance = await _context.Tarifcategorieassurances
                .Include(t => t.Categorie)
                .Include(t => t.CodeassuranceNavigation)
                .FirstOrDefaultAsync(m => m.Tarifcategorieassuranceid == id);
            if (tarifcategorieassurance == null)
            {
                return NotFound();
            }

            return View(tarifcategorieassurance);
        }

        // POST: Tarifcategorieassurance/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var tarifcategorieassurance = await _context.Tarifcategorieassurances.FindAsync(id);
            if (tarifcategorieassurance != null)
            {
                _context.Tarifcategorieassurances.Remove(tarifcategorieassurance);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TarifcategorieassuranceExists(Guid id)
        {
            return _context.Tarifcategorieassurances.Any(e => e.Tarifcategorieassuranceid == id);
        }

        #region handlers

        [HttpPost, ActionName("DeleteTarifcategorieassurance")]
        public async Task<IActionResult> DeleteTarifcategorieassuranceAsync(string id)
        {
            JsonResponseViewModel resultat = new JsonResponseViewModel();

            if (!string.IsNullOrWhiteSpace(id))
            {
                var tarifcategorieassurances = await _context.Tarifcategorieassurances.AsNoTracking()
                    .Where(x => x.Codeassurance == id)
                    .ToListAsync();

                if (tarifcategorieassurances.Any())
                {
                    _context.RemoveRange(tarifcategorieassurances);
                    await _context.SaveChangesAsync();

                    resultat.success = true;
                }
            }

            return Ok(resultat);
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoriesByLaboratoire(int? idlaboratoire, string? codeassurance = null)
        {
            try
            {
                var query = _context.Categories.AsNoTracking();

                // Filtrer par laboratoire si fourni
                if (idlaboratoire.HasValue)
                {
                    // Filtrer les catégories qui ont des analyses dans le laboratoire spécifié
                    query = query.Where(c => c.Categorieanalyses
                        .Any(ca => ca.IdanalyseNavigation.Idlaboratoire == idlaboratoire.Value));
                }

                var categories = await query
                    .Select(x => new TarifCategorieVM
                    {
                        Categorieid = x.Categorieid,
                        Nom = x.Nom,
                        Prix = ""
                    })
                    .ToListAsync();

                // Si on est en mode édition (codeassurance fourni), récupérer les prix existants
                if (!string.IsNullOrWhiteSpace(codeassurance))
                {
                    var tarifcategorieassurances = await _context.Tarifcategorieassurances.AsNoTracking()
                        .Where(x => x.Codeassurance == codeassurance)
                        .ToListAsync();

                    foreach (var item in categories)
                    {
                        var found = tarifcategorieassurances
                            .FirstOrDefault(x => x.Categorieid == item.Categorieid);

                        if (found != null)
                        {
                            item.Prix = found.Prix.ToString();
                        }
                    }
                }

                return Json(categories);
            }
            catch (Exception ex)
            {
                return Json(new { error = "Erreur lors du chargement des catégories: " + ex.Message });
            }
        }

        #endregion
    }
}
