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
    public class AnalysematerielController : Controller
    {
        private readonly ProlabwebContext _context;

        public AnalysematerielController(ProlabwebContext context)
        {
            _context = context;
        }

        // GET: Analysemateriel
        public async Task<IActionResult> Index()
        {
            var analysemateriels = await _context.Analysemateriels
                .Include(a => a.IdanalyseNavigation)
                .Include(a => a.Materiel)
                .ToListAsync();

            // Regrouper par analyse
            var groupedData = analysemateriels
                .GroupBy(am => new { am.Idanalyse, am.IdanalyseNavigation?.Nom })
                .Select(g => new ProlabWeb.ViewModels.AnalysematerielIndexVM
                {
                    Idanalyse = g.Key.Idanalyse,
                    AnalyseNom = g.Key.Nom ?? "Analyse inconnue",
                    Materiels = g.ToList()
                })
                .OrderBy(vm => vm.AnalyseNom)
                .ToList();

            return View(groupedData);
        }

        // GET: Analysemateriel/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var analysemateriel = await _context.Analysemateriels
                .Include(a => a.IdanalyseNavigation)
                .Include(a => a.Materiel)
                .FirstOrDefaultAsync(m => m.Analysematerielid == id);
            if (analysemateriel == null)
            {
                return NotFound();
            }

            return View(analysemateriel);
        }

        // GET: Analysemateriel/Create
        public IActionResult Create()
        {
            var vm = new ProlabWeb.ViewModels.AnalysematerielCreateVM();

            // Ne pas initialiser avec une ligne - le tableau doit être vide au départ

            ViewBag.Idanalyse = new SelectList(_context.Analyses, "Idanalyse", "Nom");
            ViewBag.Materiels = new SelectList(_context.Materiels, "Materielid", "Nom");
            return View(vm);
        }

        // POST: Analysemateriel/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProlabWeb.ViewModels.AnalysematerielCreateVM model, string submitAction)
        {
            // Traiter d'abord les actions de boutons (avant la validation)
            if (submitAction == "removeRow")
            {
                if (model.Materiels.Count > 0)
                {
                    model.Materiels.RemoveAt(model.Materiels.Count - 1);
                    ViewBag.Action = "removeRow";
                }
                else
                {
                    ViewBag.Action = "removeRow";
                    // Pas de message d'erreur, juste ignorer l'action
                }

                // Préparer les listes déroulantes
                ViewBag.Idanalyse = new SelectList(_context.Analyses, "Idanalyse", "Nom", model.Idanalyse);
                ViewBag.Materiels = new SelectList(_context.Materiels, "Materielid", "Nom");

                return View(model);
            }

            // Gestion des boutons + et -
            if (submitAction == "addRow")
            {
                // Vérifier qu'une analyse est sélectionnée avant d'ajouter une ligne
                if (model.Idanalyse == Guid.Empty)
                {
                    ModelState.AddModelError("Idanalyse", "Veuillez d'abord sélectionner une analyse avant d'ajouter des matériels.");
                    ViewBag.Action = "addRow";
                }
                else
                {
                    model.Materiels.Add(new ProlabWeb.ViewModels.DetailAnalyseMaterielVM());
                    ViewBag.Action = "addRow";
                }
            }
            else if (submitAction == "save")
            {
                ViewBag.Action = "save";

                if (ModelState.IsValid)
                {
                    try
                    {
                        // Créer les liaisons Analyse-Matériel
                        foreach (var materielVM in model.Materiels)
                        {
                            // Vérifier que le matériel et la quantité sont valides
                            if (materielVM.Materielid != Guid.Empty && materielVM.Quantite > 0)
                            {
                                var analysemateriel = new Analysemateriel
                                {
                                    Analysematerielid = Guid.NewGuid(),
                                    Idanalyse = model.Idanalyse,
                                    Materielid = materielVM.Materielid,
                                    Quantite = materielVM.Quantite
                                };
                                _context.Analysemateriels.Add(analysemateriel);
                            }
                        }

                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"Erreur lors de la sauvegarde: {ex.Message}");
                    }
                }
            }

            // Préparer les listes déroulantes
            ViewBag.Idanalyse = new SelectList(_context.Analyses, "Idanalyse", "Nom", model.Idanalyse);
            ViewBag.Materiels = new SelectList(_context.Materiels, "Materielid", "Nom");

            return View(model);
        }

        // GET: Analysemateriel/Edit/5 (id = Idanalyse)
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Récupérer tous les matériels liés à cette analyse
            var analysemateriels = await _context.Analysemateriels
                .Where(am => am.Idanalyse == id)
                .ToListAsync();

            if (!analysemateriels.Any())
            {
                return NotFound();
            }

            var model = new ProlabWeb.ViewModels.AnalysematerielEditVM
            {
                Idanalyse = id.Value,
                Materiels = analysemateriels.Select(am => new ProlabWeb.ViewModels.DetailAnalyseMaterielVM
                {
                    Materielid = am.Materielid,
                    Quantite = am.Quantite
                }).ToList()
            };

            ViewBag.Idanalyse = new SelectList(_context.Analyses, "Idanalyse", "Nom", model.Idanalyse);
            ViewBag.Materiels = new SelectList(_context.Materiels, "Materielid", "Nom");

            return View(model);
        }

        // POST: Analysemateriel/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ProlabWeb.ViewModels.AnalysematerielEditVM model, string submitAction)
        {
            if (id != model.Idanalyse)
            {
                return NotFound();
            }

            // Traiter d'abord les actions de boutons (avant la validation)
            if (submitAction == "removeRow")
            {
                if (model.Materiels.Count > 0)
                {
                    model.Materiels.RemoveAt(model.Materiels.Count - 1);
                    ViewBag.Action = "removeRow";
                }
                else
                {
                    ViewBag.Action = "removeRow";
                    // Pas de message d'erreur, juste ignorer l'action
                }

                // Préparer les listes déroulantes
                ViewBag.Idanalyse = new SelectList(_context.Analyses, "Idanalyse", "Nom", model.Idanalyse);
                ViewBag.Materiels = new SelectList(_context.Materiels, "Materielid", "Nom");

                return View(model);
            }

            // Gestion des boutons + et -
            if (submitAction == "addRow")
            {
                model.Materiels.Add(new ProlabWeb.ViewModels.DetailAnalyseMaterielVM());
                ViewBag.Action = "addRow";
            }
            else if (submitAction == "save")
            {
                ViewBag.Action = "save";

                if (ModelState.IsValid)
                {
                    try
                    {
                        // Supprimer tous les anciens matériels de cette analyse
                        var existingMateriels = _context.Analysemateriels.Where(am => am.Idanalyse == model.Idanalyse);
                        _context.Analysemateriels.RemoveRange(existingMateriels);

                        // Ajouter les nouveaux matériels
                        foreach (var materielVM in model.Materiels)
                        {
                            if (materielVM.Materielid != Guid.Empty && materielVM.Quantite > 0)
                            {
                                var analysemateriel = new Analysemateriel
                                {
                                    Analysematerielid = Guid.NewGuid(),
                                    Idanalyse = model.Idanalyse,
                                    Materielid = materielVM.Materielid,
                                    Quantite = materielVM.Quantite
                                };
                                _context.Analysemateriels.Add(analysemateriel);
                            }
                        }

                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"Erreur lors de la sauvegarde: {ex.Message}");
                    }
                }
            }

            // Préparer les listes déroulantes
            ViewBag.Idanalyse = new SelectList(_context.Analyses, "Idanalyse", "Nom", model.Idanalyse);
            ViewBag.Materiels = new SelectList(_context.Materiels, "Materielid", "Nom");

            return View(model);
        }

        // GET: Analysemateriel/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var analysemateriel = await _context.Analysemateriels
                .Include(a => a.IdanalyseNavigation)
                .Include(a => a.Materiel)
                .FirstOrDefaultAsync(m => m.Analysematerielid == id);
            if (analysemateriel == null)
            {
                return NotFound();
            }

            return View(analysemateriel);
        }

        // POST: Analysemateriel/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var analysemateriel = await _context.Analysemateriels.FindAsync(id);
            if (analysemateriel != null)
            {
                _context.Analysemateriels.Remove(analysemateriel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AnalysematerielExists(Guid id)
        {
            return _context.Analysemateriels.Any(e => e.Analysematerielid == id);
        }
    }
}
