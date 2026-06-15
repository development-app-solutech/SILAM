using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProlabWeb.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProlabWeb.Controllers
{
    [AllowAnonymous]
    public class CategorieController : Controller
    {
        private readonly ProlabwebContext _context;

        public CategorieController(ProlabwebContext context)
        {
            _context = context;
        }

        // GET: Categorie
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Include(c => c.Categorieanalyses)
                .ThenInclude(ca => ca.IdanalyseNavigation)
                .ToListAsync();
            return View(categories);
        }

        // GET: Categorie/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categorie = await _context.Categories
                .Include(c => c.Categorieanalyses)
                .ThenInclude(ca => ca.IdanalyseNavigation)
                .FirstOrDefaultAsync(m => m.Categorieid == id);
            if (categorie == null)
            {
                return NotFound();
            }

            return View(categorie);
        }

        // GET: Categorie/Create
        public IActionResult Create()
        {
            var analyses = new SelectList(_context.Analyses, "Idanalyse", "Nom").ToList();
            ViewBag.IdsAnalyse = analyses;
            return View();
        }

        // POST: Categorie/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProlabWeb.ViewModels.CategorieCreateVM model)
        {
            if (ModelState.IsValid)
            {
                var categorie = new Categorie
                {
                    Categorieid = Guid.NewGuid(),
                    Nom = model.Nom,
                    Description = model.Description,
                    Prix = model.Prix,
                    Isactive = model.Isactive
                };
                // Ajout des liaisons Categorieanalyse
                if (model.IdsAnalyse != null)
                {
                    foreach (var analyseId in model.IdsAnalyse)
                    {
                        var obj = new Categorieanalyse
                        {
                            Categorieanalyseid = Guid.NewGuid(),
                            Categorieid = categorie.Categorieid,
                            Idanalyse = Guid.Parse(analyseId)
                        };
                        categorie.Categorieanalyses.Add(obj);
                    }
                }
                _context.Add(categorie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var analyses = new SelectList(_context.Analyses, "Idanalyse", "Nom", model.IdsAnalyse).ToList();
            ViewBag.IdsAnalyse = analyses;
            return View(model);
        }

        // GET: Categorie/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categorie = await _context.Categories
                .Include(c => c.Categorieanalyses)
                .FirstOrDefaultAsync(c => c.Categorieid == id);
            if (categorie == null)
            {
                return NotFound();
            }

            var model = new ProlabWeb.ViewModels.CategorieEditVM
            {
                Categorieid = categorie.Categorieid,
                Nom = categorie.Nom,
                Description = categorie.Description,
                Prix = categorie.Prix,
                Isactive = categorie.Isactive,
                IdsAnalyse = categorie.Categorieanalyses.Select(ca => ca.Idanalyse.ToString()).ToArray()
            };

            var analyses = new SelectList(_context.Analyses, "Idanalyse", "Nom", model.IdsAnalyse).ToList();
            ViewBag.IdsAnalyse = analyses;
            return View(model);
        }

        // POST: Categorie/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ProlabWeb.ViewModels.CategorieEditVM model)
        {
            if (id != model.Categorieid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var categorie = await _context.Categories
                    .Include(c => c.Categorieanalyses)
                    .FirstOrDefaultAsync(c => c.Categorieid == id);
                if (categorie == null)
                {
                    return NotFound();
                }

                categorie.Nom = model.Nom;
                categorie.Description = model.Description;
                categorie.Prix = model.Prix;
                categorie.Isactive = model.Isactive;

                // Gestion des liaisons Categorieanalyse
                var anciennesAnalyses = categorie.Categorieanalyses.ToList();
                // Supprimer celles qui ne sont plus sélectionnées
                foreach (var liaison in anciennesAnalyses)
                {
                    if (!model.IdsAnalyse.Contains(liaison.Idanalyse.ToString()))
                    {
                        _context.Categorieanalyses.Remove(liaison);
                    }
                }
                // Ajouter les nouvelles
                foreach (var analyseId in model.IdsAnalyse)
                {
                    if (!anciennesAnalyses.Any(a => a.Idanalyse.ToString() == analyseId))
                    {
                        var obj = new Categorieanalyse
                        {
                            Categorieanalyseid = Guid.NewGuid(),
                            Categorieid = categorie.Categorieid,
                            Idanalyse = Guid.Parse(analyseId)
                        };
                        categorie.Categorieanalyses.Add(obj);
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var analyses = new SelectList(_context.Analyses, "Idanalyse", "Nom", model.IdsAnalyse).ToList();
            ViewBag.IdsAnalyse = analyses;
            return View(model);
        }

        // GET: Categorie/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categorie = await _context.Categories
                .FirstOrDefaultAsync(m => m.Categorieid == id);
            if (categorie == null)
            {
                return NotFound();
            }

            return View(categorie);
        }

        // POST: Categorie/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var categorie = await _context.Categories.FindAsync(id);
            if (categorie != null)
            {
                _context.Categories.Remove(categorie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Categorie/DeleteAjax
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DeleteAjax(Guid id)
        {
            try
            {
                var categorie = await _context.Categories
                    .Include(c => c.Categorieanalyses)
                    .FirstOrDefaultAsync(c => c.Categorieid == id);
                    
                if (categorie == null)
                {
                    return Json(new { success = false, message = "Catégorie introuvable." });
                }

                // Supprimer d'abord les liaisons Categorieanalyse
                if (categorie.Categorieanalyses != null && categorie.Categorieanalyses.Any())
                {
                    _context.Categorieanalyses.RemoveRange(categorie.Categorieanalyses);
                }

                // Ensuite supprimer la catégorie
                _context.Categories.Remove(categorie);
                await _context.SaveChangesAsync();
                
                return Json(new { success = true, message = "Catégorie supprimée avec succès." });
            }
            catch (DbUpdateException dbEx)
            {
                // Erreur spécifique à la base de données
                return Json(new { success = false, message = "Cette catégorie ne peut pas être supprimée car elle est utilisée ailleurs dans le système." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erreur lors de la suppression : {ex.Message}" });
            }
        }

        private bool CategorieExists(Guid id)
        {
            return _context.Categories.Any(e => e.Categorieid == id);
        }
    }
}
