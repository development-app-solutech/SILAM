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
    public class CategoriematerielController : Controller
    {
        private readonly ProlabwebContext _context;

        public CategoriematerielController(ProlabwebContext context)
        {
            _context = context;
        }

        // GET: Categoriemateriel
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categoriemateriels.ToListAsync());
        }

        // GET: Categoriemateriel/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoriemateriel = await _context.Categoriemateriels
                .FirstOrDefaultAsync(m => m.Categoriematerielid == id);
            if (categoriemateriel == null)
            {
                return NotFound();
            }

            return View(categoriemateriel);
        }

        // GET: Categoriemateriel/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categoriemateriel/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Categoriematerielid,Nom,Description,Isactive")] Categoriemateriel categoriemateriel)
        {
            if (ModelState.IsValid)
            {
                categoriemateriel.Categoriematerielid = Guid.NewGuid();
                _context.Add(categoriemateriel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(categoriemateriel);
        }

        // GET: Categoriemateriel/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoriemateriel = await _context.Categoriemateriels.FindAsync(id);
            if (categoriemateriel == null)
            {
                return NotFound();
            }
            return View(categoriemateriel);
        }

        // POST: Categoriemateriel/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Categoriematerielid,Nom,Description,Isactive")] Categoriemateriel categoriemateriel)
        {
            if (id != categoriemateriel.Categoriematerielid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(categoriemateriel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoriematerielExists(categoriemateriel.Categoriematerielid))
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
            return View(categoriemateriel);
        }

        // GET: Categoriemateriel/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoriemateriel = await _context.Categoriemateriels
                .FirstOrDefaultAsync(m => m.Categoriematerielid == id);
            if (categoriemateriel == null)
            {
                return NotFound();
            }

            return View(categoriemateriel);
        }

        // POST: Categoriemateriel/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var categoriemateriel = await _context.Categoriemateriels.FindAsync(id);
            if (categoriemateriel != null)
            {
                _context.Categoriemateriels.Remove(categoriemateriel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoriematerielExists(Guid id)
        {
            return _context.Categoriemateriels.Any(e => e.Categoriematerielid == id);
        }
    }
}
