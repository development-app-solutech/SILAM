using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProlabWeb.Db;

namespace ProlabWeb.Controllers
{
    public class MethodeController : Controller
    {
        private readonly ProlabwebContext _context;

        public MethodeController(ProlabwebContext context)
        {
            _context = context;
        }

        // GET: Methode
        public async Task<IActionResult> Index()
        {
            return View(await _context.Methodes.ToListAsync());
        }

        // GET: Methode/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var methode = await _context.Methodes
                .FirstOrDefaultAsync(m => m.Idmethode == id);
            if (methode == null)
            {
                return NotFound();
            }

            return View(methode);
        }

        // GET: Methode/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Methode/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nom,Isactive")] Methode methode)
        {
            if (ModelState.IsValid)
            {
                // L'ID sera généré automatiquement par Entity Framework (auto-increment)
                _context.Add(methode);
                await _context.SaveChangesAsync();

                // Notification de succès
                TempData["MethodeCree"] = $"La méthode <strong>{methode.Nom}</strong> a été créée avec succès.";
                return RedirectToAction(nameof(Index));
            }
            return View(methode);
        }

        // GET: Methode/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var methode = await _context.Methodes.FindAsync(id);
            if (methode == null)
            {
                return NotFound();
            }
            return View(methode);
        }

        // POST: Methode/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Idmethode,Nom,Isactive")] Methode methode)
        {
            if (id != methode.Idmethode)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(methode);
                    await _context.SaveChangesAsync();

                    // Notification de succès
                    TempData["MethodeModifie"] = "success";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MethodeExists(methode.Idmethode))
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
                    TempData["MethodeModifie"] = "error";
                }
                return RedirectToAction(nameof(Index));
            }
            return View(methode);
        }

        // GET: Methode/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var methode = await _context.Methodes
                .FirstOrDefaultAsync(m => m.Idmethode == id);
            if (methode == null)
            {
                return NotFound();
            }

            return View(methode);
        }

        // POST: Methode/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var methode = await _context.Methodes.FindAsync(id);
            if (methode != null)
            {
                _context.Methodes.Remove(methode);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MethodeExists(int id)
        {
            return _context.Methodes.Any(e => e.Idmethode == id);
        }
    }
}
