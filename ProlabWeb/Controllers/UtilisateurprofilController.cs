using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProlabWeb.Db;
using ProlabWeb.ViewModels;

namespace ProlabWeb.Controllers
{
    public class UtilisateurprofilController : Controller
    {
        private readonly ProlabwebContext _context;

        public UtilisateurprofilController(ProlabwebContext context)
        {
            _context = context;
        }

        // GET: Utilisateurprofil
        public async Task<IActionResult> Index()
        {
            var prolabwebContext = _context.Utilisateurprofils.Include(u => u.Profil).Include(u => u.Utilisateur);
            return View(await prolabwebContext.ToListAsync());
        }

        // GET: Utilisateurprofil/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var utilisateurprofil = await _context.Utilisateurprofils
                .Include(u => u.Profil)
                .Include(u => u.Utilisateur)
                .FirstOrDefaultAsync(m => m.Utilisateurprofilid == id);
            if (utilisateurprofil == null)
            {
                return NotFound();
            }

            return View(utilisateurprofil);
        }

        // GET: Utilisateurprofil/Create
        public IActionResult Create()
        {
            var vm = new UtilisateurprofilCreateVM();
            ViewBag.Profilid = new SelectList(_context.Profils, "Profilid", "Nom");
            ViewBag.Utilisateurid = new SelectList(_context.Utilisateurs, "Utilisateurid", "Nom");
            return View(vm);
        }

        // POST: Utilisateurprofil/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UtilisateurprofilCreateVM model)
        {
            // Vérifier unicité du profil par utilisateur
            bool existeDeja = await _context.Utilisateurprofils.AnyAsync(up => up.Utilisateurid == model.Utilisateurid);
            if (existeDeja)
            {
                ModelState.AddModelError("", "Cet utilisateur a déjà un profil attribué.");
                ViewBag.Profilid = new SelectList(_context.Profils, "Profilid", "Nom", model.Profilid);
                ViewBag.Utilisateurid = new SelectList(_context.Utilisateurs, "Utilisateurid", "Nom", model.Utilisateurid);
                return View(model);
            }
            if (ModelState.IsValid)
            {
                var entity = new Utilisateurprofil
                {
                    Utilisateurprofilid = Guid.NewGuid(),
                    Utilisateurid = model.Utilisateurid,
                    Profilid = model.Profilid
                };
                _context.Add(entity);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Profilid = new SelectList(_context.Profils, "Profilid", "Nom", model.Profilid);
            ViewBag.Utilisateurid = new SelectList(_context.Utilisateurs, "Utilisateurid", "Nom", model.Utilisateurid);
            return View(model);
        }

        // GET: Utilisateurprofil/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entity = await _context.Utilisateurprofils.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }
            var vm = new UtilisateurprofilCreateVM
            {
                Utilisateurprofilid = entity.Utilisateurprofilid,
                Utilisateurid = entity.Utilisateurid,
                Profilid = entity.Profilid
            };
            ViewBag.Profilid = new SelectList(_context.Profils, "Profilid", "Nom", vm.Profilid);
            ViewBag.Utilisateurid = new SelectList(_context.Utilisateurs, "Utilisateurid", "Nom", vm.Utilisateurid);
            return View(vm);
        }

        // POST: Utilisateurprofil/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UtilisateurprofilCreateVM model)
        {
            if (id != model.Utilisateurprofilid)
            {
                return NotFound();
            }
            // Vérifier unicité du profil par utilisateur (hors ligne en cours d'édition)
            bool existeDeja = await _context.Utilisateurprofils.AnyAsync(up => up.Utilisateurid == model.Utilisateurid && up.Utilisateurprofilid != id);
            if (existeDeja)
            {
                ModelState.AddModelError("", "Cet utilisateur a déjà un profil attribué.");
                ViewBag.Profilid = new SelectList(_context.Profils, "Profilid", "Nom", model.Profilid);
                ViewBag.Utilisateurid = new SelectList(_context.Utilisateurs, "Utilisateurid", "Nom", model.Utilisateurid);
                return View(model);
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var entity = await _context.Utilisateurprofils.FindAsync(id);
                    if (entity == null)
                    {
                        return NotFound();
                    }
                    entity.Utilisateurid = model.Utilisateurid;
                    entity.Profilid = model.Profilid;
                    _context.Update(entity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UtilisateurprofilExists(model.Utilisateurprofilid))
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
            ViewBag.Profilid = new SelectList(_context.Profils, "Profilid", "Nom", model.Profilid);
            ViewBag.Utilisateurid = new SelectList(_context.Utilisateurs, "Utilisateurid", "Nom", model.Utilisateurid);
            return View(model);
        }

        // GET: Utilisateurprofil/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var utilisateurprofil = await _context.Utilisateurprofils
                .Include(u => u.Profil)
                .Include(u => u.Utilisateur)
                .FirstOrDefaultAsync(m => m.Utilisateurprofilid == id);
            if (utilisateurprofil == null)
            {
                return NotFound();
            }

            return View(utilisateurprofil);
        }

        // POST: Utilisateurprofil/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var utilisateurprofil = await _context.Utilisateurprofils.FindAsync(id);
            if (utilisateurprofil != null)
            {
                _context.Utilisateurprofils.Remove(utilisateurprofil);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UtilisateurprofilExists(Guid id)
        {
            return _context.Utilisateurprofils.Any(e => e.Utilisateurprofilid == id);
        }
    }
}
