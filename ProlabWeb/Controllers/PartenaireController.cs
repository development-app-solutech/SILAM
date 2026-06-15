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
    public class PartenaireController : Controller
    {
        private readonly ProlabwebContext _context;

        public PartenaireController(ProlabwebContext context)
        {
            _context = context;
        }

        // GET: Partenaire
        public async Task<IActionResult> Index()
        {
            return View(await _context.Partenaires.ToListAsync());
        }

        // GET: Partenaire/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var partenaire = await _context.Partenaires
                .FirstOrDefaultAsync(m => m.Partenaireid == id);
            if (partenaire == null)
            {
                return NotFound();
            }

            return View(partenaire);
        }

        // GET: Partenaire/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Partenaire/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nom,Description,Isactive,Adresse,Tel")] Partenaire partenaire)
        {
            if (ModelState.IsValid)
            {
                // Génération automatique de l'ID
                partenaire.Partenaireid = Guid.NewGuid();
                _context.Add(partenaire);
                await _context.SaveChangesAsync();

                // Notification de succès
                TempData["PartenaireCree"] = $"Le partenaire <strong>{partenaire.Nom}</strong> a été créé avec succès.";
                return RedirectToAction(nameof(Index));
            }
            return View(partenaire);
        }

        // GET: Partenaire/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var partenaire = await _context.Partenaires.FindAsync(id);
            if (partenaire == null)
            {
                return NotFound();
            }
            return View(partenaire);
        }

        // POST: Partenaire/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Partenaireid,Nom,Description,Isactive,Adresse,Tel")] Partenaire partenaire)
        {
            if (id != partenaire.Partenaireid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(partenaire);
                    await _context.SaveChangesAsync();

                    // Notification de succès
                    TempData["PartenaireModifie"] = "success";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PartenaireExists(partenaire.Partenaireid))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception)
                {
                    // Notification d'erreur
                    TempData["PartenaireModifie"] = "error";
                }
                return RedirectToAction(nameof(Index));
            }
            return View(partenaire);
        }

        // GET: Partenaire/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var partenaire = await _context.Partenaires
                .FirstOrDefaultAsync(m => m.Partenaireid == id);
            if (partenaire == null)
            {
                return NotFound();
            }

            return View(partenaire);
        }

        // POST: Partenaire/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var partenaire = await _context.Partenaires.FindAsync(id);
            if (partenaire != null)
            {
                _context.Partenaires.Remove(partenaire);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Partenaire/DeleteAjax
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAjax(Guid id)
        {
            try
            {
                var partenaire = await _context.Partenaires.FindAsync(id);
                if (partenaire == null)
                {
                    return Json(new { success = false, message = "Partenaire non trouvé." });
                }

                _context.Partenaires.Remove(partenaire);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Partenaire supprimé avec succès." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erreur lors de la suppression : " + ex.Message });
            }
        }

        private bool PartenaireExists(Guid id)
        {
            return _context.Partenaires.Any(e => e.Partenaireid == id);
        }
    }
}
