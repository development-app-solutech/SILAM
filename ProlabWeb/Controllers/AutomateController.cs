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
    public class AutomateController : Controller
    {
        private readonly ProlabwebContext _context;

        public AutomateController(ProlabwebContext context)
        {
            _context = context;
        }

        // GET: Automate
        public async Task<IActionResult> Index()
        {
            return View(await _context.Automates.ToListAsync());
        }

        // GET: Automate/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var automate = await _context.Automates
                .FirstOrDefaultAsync(m => m.Idautomate == id);
            if (automate == null)
            {
                return NotFound();
            }

            return View(automate);
        }

        // GET: Automate/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Automate/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Idautomate,Nom,Description,Isactive,Modeauto,Confentree,Processconfig,Parserconfig,Confsortie,Note")] Automate automate)
        {
            if (ModelState.IsValid)
            {
                _context.Add(automate);
                await _context.SaveChangesAsync();
                TempData["AutomateCree"] = $"L'automate \"{automate.Nom}\" a été créé avec succès !";
                return RedirectToAction(nameof(Index));
            }
            return View(automate);
        }

        // GET: Automate/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var automate = await _context.Automates.FindAsync(id);
            if (automate == null)
            {
                return NotFound();
            }
            return View(automate);
        }

        // POST: Automate/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Idautomate,Nom,Description,Isactive,Modeauto,Confentree,Processconfig,Parserconfig,Confsortie,Note")] Automate automate)
        {
            if (id != automate.Idautomate)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(automate);
                    await _context.SaveChangesAsync();
                    TempData["AutomateModifie"] = "success";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AutomateExists(automate.Idautomate))
                    {
                        return NotFound();
                    }
                    else
                    {
                        TempData["AutomateModifie"] = "error";
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(automate);
        }

        // GET: Automate/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var automate = await _context.Automates
                .FirstOrDefaultAsync(m => m.Idautomate == id);
            if (automate == null)
            {
                return NotFound();
            }

            return View(automate);
        }

        // POST: Automate/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var automate = await _context.Automates.FindAsync(id);
            if (automate != null)
            {
                _context.Automates.Remove(automate);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Suppression AJAX
        [HttpPost]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            var automate = await _context.Automates.FindAsync(id);
            if (automate == null)
            {
                return Json(new { success = false, message = "Automate non trouvé." });
            }
            _context.Automates.Remove(automate);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        private bool AutomateExists(int id)
        {
            return _context.Automates.Any(e => e.Idautomate == id);
        }
    }
}
