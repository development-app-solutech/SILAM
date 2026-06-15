using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProlabWeb.Db;

namespace ProlabWeb.Controllers
{
    [AllowAnonymous]
    public class EntetematerielapproController : Controller
    {
        private readonly ProlabwebContext _context;

        public EntetematerielapproController(ProlabwebContext context)
        {
            _context = context;
        }

        // GET: Entetematerielappro
        public async Task<IActionResult> Index()
        {
            var prolabwebContext = _context.Entetematerielappros
                .Include(e => e.Fournisseur)
                .Include(e => e.Utilisateur)
                .Include(e => e.Detailmaterielappros)
                    .ThenInclude(d => d.Materiel);
            return View(await prolabwebContext.ToListAsync());
        }

        // GET: Entetematerielappro/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entetematerielappro = await _context.Entetematerielappros
                .Include(e => e.Fournisseur)
                .Include(e => e.Utilisateur)
                .FirstOrDefaultAsync(m => m.Entetematerielapproid == id);
            if (entetematerielappro == null)
            {
                return NotFound();
            }

            return View(entetematerielappro);
        }

        // GET: Entetematerielappro/Create
        public IActionResult Create()
        {
            var fournisseurs = new List<SelectListItem> { new SelectListItem { Value = "", Text = "--" } };
            fournisseurs.AddRange(_context.Fournisseurs.Select(f => new SelectListItem { Value = f.Fournisseurid.ToString(), Text = f.Nom }));
            ViewData["Fournisseurid"] = fournisseurs;

            var utilisateurs = new List<SelectListItem> { new SelectListItem { Value = "", Text = "--" } };
            utilisateurs.AddRange(_context.Utilisateurs.Select(u => new SelectListItem { Value = u.Utilisateurid.ToString(), Text = u.Nom }));
            ViewData["Utilisateurid"] = utilisateurs;

            ViewBag.Materiels = new SelectList(_context.Materiels, "Materielid", "Nom");

            var model = new ProlabWeb.ViewModels.EntetematerielapproCreateVM();
            // Les valeurs par défaut sont maintenant définies dans le ViewModel

            ViewBag.Action = "get";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProlabWeb.ViewModels.EntetematerielapproCreateVM model, string submitAction)
        {
            var fournisseurs = new List<SelectListItem> { new SelectListItem { Value = "", Text = "--" } };
            fournisseurs.AddRange(_context.Fournisseurs.Select(f => new SelectListItem
            {
                Value = f.Fournisseurid.ToString(),
                Text = f.Nom,
                Selected = f.Fournisseurid != Guid.Empty && f.Fournisseurid == model.Fournisseurid
            }));
            ViewData["Fournisseurid"] = fournisseurs;

            var utilisateurs = new List<SelectListItem> { new SelectListItem { Value = "", Text = "--" } };
            utilisateurs.AddRange(_context.Utilisateurs.Select(u => new SelectListItem
            {
                Value = u.Utilisateurid.ToString(),
                Text = u.Nom,
                Selected = u.Utilisateurid != Guid.Empty && u.Utilisateurid == model.Utilisateurid
            }));
            ViewData["Utilisateurid"] = utilisateurs;

            ViewBag.Materiels = new SelectList(_context.Materiels, "Materielid", "Nom");

            // Traiter d'abord les actions de boutons (avant la validation)
            if (submitAction == "removeRow")
            {
                model.Materiels ??= new List<ProlabWeb.ViewModels.DetailMaterielApproVM>();
                if (model.Materiels.Count > 0)
                {
                    model.Materiels.RemoveAt(model.Materiels.Count - 1);
                }
                ModelState.Clear(); // Désactiver complètement la validation
                ViewBag.Action = "removeRow";
                return View(model);
            }

            if (submitAction == "addRow")
            {
                // Vérifier que tous les champs obligatoires sont remplis avant d'ajouter une ligne
                bool hasErrors = false;

                if (model.Fournisseurid == Guid.Empty)
                {
                    ModelState.AddModelError("Fournisseurid", "Veuillez sélectionner un fournisseur.");
                    hasErrors = true;
                }

                if (model.Utilisateurid == Guid.Empty)
                {
                    ModelState.AddModelError("Utilisateurid", "Veuillez sélectionner un utilisateur.");
                    hasErrors = true;
                }

                if (string.IsNullOrWhiteSpace(model.Numero))
                {
                    ModelState.AddModelError("Numero", "Veuillez saisir un numéro.");
                    hasErrors = true;
                }

                if (model.Date == default(DateTime))
                {
                    ModelState.AddModelError("Date", "Veuillez sélectionner une date.");
                    hasErrors = true;
                }

                if (hasErrors)
                {
                    ViewBag.Action = "addRow";
                }
                else
                {
                    model.Materiels ??= new List<ProlabWeb.ViewModels.DetailMaterielApproVM>();
                    model.Materiels.Add(new ProlabWeb.ViewModels.DetailMaterielApproVM());
                    ModelState.Clear(); // Désactiver complètement la validation
                    ViewBag.Action = "addRow";
                }
                return View(model);
            }

            ViewBag.Action = "save";
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Vérifications supplémentaires
            if (model.Fournisseurid == Guid.Empty)
            {
                ModelState.AddModelError("Fournisseurid", "Veuillez sélectionner un fournisseur");
                return View(model);
            }

            if (model.Utilisateurid == Guid.Empty)
            {
                ModelState.AddModelError("Utilisateurid", "Veuillez sélectionner un utilisateur");
                return View(model);
            }

            if (model.Materiels == null || model.Materiels.Count == 0)
            {
                ModelState.AddModelError("", "Veuillez ajouter au moins un matériel");
                return View(model);
            }

            try
            {
                // Créer l'entête d'approvisionnement
                var entetematerielappro = new Entetematerielappro
                {
                    Entetematerielapproid = Guid.NewGuid(),
                    Date = model.Date,
                    Numero = model.Numero,
                    Fournisseurid = model.Fournisseurid,
                    Utilisateurid = model.Utilisateurid
                };

                _context.Entetematerielappros.Add(entetematerielappro);

                // Ajouter les détails
                foreach (var materielVM in model.Materiels)
                {
                    // Vérifier que le matériel et la quantité sont valides
                    if (materielVM.Materielid != Guid.Empty && materielVM.Quantite > 0)
                    {
                        var detail = new Detailmaterielappro
                        {
                            Detailmaterielapproid = Guid.NewGuid(),
                            Entetematerielapproid = entetematerielappro.Entetematerielapproid,
                            Materielid = materielVM.Materielid,
                            Quantite = materielVM.Quantite
                        };
                        _context.Detailmaterielappros.Add(detail);
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log l'erreur pour le débogage
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la sauvegarde: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");

                // En cas d'erreur, retourner à la vue avec le modèle et l'erreur
                ModelState.AddModelError("", $"Erreur lors de la sauvegarde: {ex.Message}");
                return View(model);
            }
        }

        // GET: Entetematerielappro/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entetematerielappro = await _context.Entetematerielappros
                .Include(e => e.Detailmaterielappros)
                .FirstOrDefaultAsync(e => e.Entetematerielapproid == id);

            if (entetematerielappro == null)
            {
                return NotFound();
            }

            var model = new ProlabWeb.ViewModels.EntetematerielapproEditVM
            {
                Entetematerielapproid = entetematerielappro.Entetematerielapproid,
                Date = entetematerielappro.Date,
                Numero = entetematerielappro.Numero,
                Fournisseurid = entetematerielappro.Fournisseurid,
                Utilisateurid = entetematerielappro.Utilisateurid,
                Materiels = entetematerielappro.Detailmaterielappros.Select(d => new ProlabWeb.ViewModels.DetailMaterielApproVM
                {
                    Materielid = d.Materielid,
                    Quantite = d.Quantite
                }).ToList()
            };

            // Préparer les listes déroulantes avec les valeurs sélectionnées
            var fournisseurs = new List<SelectListItem> { new SelectListItem { Value = "", Text = "--" } };
            fournisseurs.AddRange(_context.Fournisseurs.Select(f => new SelectListItem
            {
                Value = f.Fournisseurid.ToString(),
                Text = f.Nom,
                Selected = f.Fournisseurid == model.Fournisseurid
            }));
            ViewData["Fournisseurid"] = fournisseurs;

            var utilisateurs = new List<SelectListItem> { new SelectListItem { Value = "", Text = "--" } };
            utilisateurs.AddRange(_context.Utilisateurs.Select(u => new SelectListItem
            {
                Value = u.Utilisateurid.ToString(),
                Text = u.Nom,
                Selected = u.Utilisateurid == model.Utilisateurid
            }));
            ViewData["Utilisateurid"] = utilisateurs;

            ViewBag.Materiels = new SelectList(_context.Materiels, "Materielid", "Nom");

            ViewBag.Action = "get";
            return View(model);
        }

        // POST: Entetematerielappro/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ProlabWeb.ViewModels.EntetematerielapproEditVM model, string submitAction)
        {
            if (id != model.Entetematerielapproid)
            {
                return NotFound();
            }

            // Préparer les listes déroulantes avec les valeurs sélectionnées
            var fournisseurs = new List<SelectListItem> { new SelectListItem { Value = "", Text = "--" } };
            fournisseurs.AddRange(_context.Fournisseurs.Select(f => new SelectListItem
            {
                Value = f.Fournisseurid.ToString(),
                Text = f.Nom,
                Selected = f.Fournisseurid == model.Fournisseurid
            }));
            ViewData["Fournisseurid"] = fournisseurs;

            var utilisateurs = new List<SelectListItem> { new SelectListItem { Value = "", Text = "--" } };
            utilisateurs.AddRange(_context.Utilisateurs.Select(u => new SelectListItem
            {
                Value = u.Utilisateurid.ToString(),
                Text = u.Nom,
                Selected = u.Utilisateurid == model.Utilisateurid
            }));
            ViewData["Utilisateurid"] = utilisateurs;

            ViewBag.Materiels = new SelectList(_context.Materiels, "Materielid", "Nom");

            // Traiter d'abord les actions de boutons (avant la validation)
            if (submitAction == "removeRow")
            {
                model.Materiels ??= new List<ProlabWeb.ViewModels.DetailMaterielApproVM>();
                if (model.Materiels.Count > 0)
                {
                    model.Materiels.RemoveAt(model.Materiels.Count - 1);
                }
                ModelState.Clear(); // Désactiver complètement la validation
                ViewBag.Action = "removeRow";
                return View(model);
            }

            if (submitAction == "addRow")
            {
                model.Materiels ??= new List<ProlabWeb.ViewModels.DetailMaterielApproVM>();
                model.Materiels.Add(new ProlabWeb.ViewModels.DetailMaterielApproVM());
                ModelState.Clear(); // Désactiver complètement la validation
                ViewBag.Action = "addRow";
                return View(model);
            }

            ViewBag.Action = "save";
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var entetematerielappro = await _context.Entetematerielappros
                    .Include(e => e.Detailmaterielappros)
                    .FirstOrDefaultAsync(e => e.Entetematerielapproid == id);

                if (entetematerielappro == null)
                {
                    return NotFound();
                }

                // Mettre à jour les propriétés principales
                entetematerielappro.Date = model.Date;
                entetematerielappro.Numero = model.Numero;
                entetematerielappro.Fournisseurid = model.Fournisseurid;
                entetematerielappro.Utilisateurid = model.Utilisateurid;

                // Supprimer tous les détails existants
                _context.Detailmaterielappros.RemoveRange(entetematerielappro.Detailmaterielappros);

                // Ajouter les nouveaux détails
                foreach (var materielVM in model.Materiels)
                {
                    // Vérifier que le matériel et la quantité sont valides
                    if (materielVM.Materielid != Guid.Empty && materielVM.Quantite > 0)
                    {
                        var detail = new Detailmaterielappro
                        {
                            Detailmaterielapproid = Guid.NewGuid(),
                            Entetematerielapproid = entetematerielappro.Entetematerielapproid,
                            Materielid = materielVM.Materielid,
                            Quantite = materielVM.Quantite
                        };
                        _context.Detailmaterielappros.Add(detail);
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EntetematerielapproExists(model.Entetematerielapproid))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // GET: Entetematerielappro/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entetematerielappro = await _context.Entetematerielappros
                .Include(e => e.Fournisseur)
                .Include(e => e.Utilisateur)
                .FirstOrDefaultAsync(m => m.Entetematerielapproid == id);
            if (entetematerielappro == null)
            {
                return NotFound();
            }

            return View(entetematerielappro);
        }

        // POST: Entetematerielappro/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var entetematerielappro = await _context.Entetematerielappros.FindAsync(id);
            if (entetematerielappro != null)
            {
                _context.Entetematerielappros.Remove(entetematerielappro);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EntetematerielapproExists(Guid id)
        {
            return _context.Entetematerielappros.Any(e => e.Entetematerielapproid == id);
        }
    }
}
