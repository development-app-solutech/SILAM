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
    public class CategorieanalyseController : Controller
    {
        private readonly ProlabwebContext _context;

        public CategorieanalyseController(ProlabwebContext context)
        {
            _context = context;
        }

        // GET: Categorieanalyse
        public async Task<IActionResult> Index()
        {
            var prolabwebContext = _context.Categorieanalyses.Include(c => c.Categorie).Include(c => c.IdanalyseNavigation);
            return View(await prolabwebContext.ToListAsync());
        }

        // GET: Categorieanalyse/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categorieanalyse = await _context.Categorieanalyses
                .Include(c => c.Categorie)
                .Include(c => c.IdanalyseNavigation)
                .FirstOrDefaultAsync(m => m.Categorieanalyseid == id);
            if (categorieanalyse == null)
            {
                return NotFound();
            }

            return View(categorieanalyse);
        }

        // GET: Categorieanalyse/Create
        public IActionResult Create()
        {
            ViewData["Categorieid"] = new SelectList(_context.Categories, "Categorieid", "Categorieid");
            ViewData["Idanalyse"] = new SelectList(_context.Analyses, "Idanalyse", "Idanalyse");
            return View();
        }

        // POST: Categorieanalyse/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Categorieid,Idanalyse,Categorieanalyseid")] Categorieanalyse categorieanalyse)
        {
            if (ModelState.IsValid)
            {
                categorieanalyse.Categorieanalyseid = Guid.NewGuid();
                _context.Add(categorieanalyse);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Categorieid"] = new SelectList(_context.Categories, "Categorieid", "Categorieid", categorieanalyse.Categorieid);
            ViewData["Idanalyse"] = new SelectList(_context.Analyses, "Idanalyse", "Idanalyse", categorieanalyse.Idanalyse);
            return View(categorieanalyse);
        }

        // GET: Categorieanalyse/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categorieanalyse = await _context.Categorieanalyses.FindAsync(id);
            if (categorieanalyse == null)
            {
                return NotFound();
            }
            ViewData["Categorieid"] = new SelectList(_context.Categories, "Categorieid", "Categorieid", categorieanalyse.Categorieid);
            ViewData["Idanalyse"] = new SelectList(_context.Analyses, "Idanalyse", "Idanalyse", categorieanalyse.Idanalyse);
            return View(categorieanalyse);
        }

        // POST: Categorieanalyse/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Categorieid,Idanalyse,Categorieanalyseid")] Categorieanalyse categorieanalyse)
        {
            if (id != categorieanalyse.Categorieanalyseid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(categorieanalyse);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategorieanalyseExists(categorieanalyse.Categorieanalyseid))
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
            ViewData["Categorieid"] = new SelectList(_context.Categories, "Categorieid", "Categorieid", categorieanalyse.Categorieid);
            ViewData["Idanalyse"] = new SelectList(_context.Analyses, "Idanalyse", "Idanalyse", categorieanalyse.Idanalyse);
            return View(categorieanalyse);
        }

        // GET: Categorieanalyse/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categorieanalyse = await _context.Categorieanalyses
                .Include(c => c.Categorie)
                .Include(c => c.IdanalyseNavigation)
                .FirstOrDefaultAsync(m => m.Categorieanalyseid == id);
            if (categorieanalyse == null)
            {
                return NotFound();
            }

            return View(categorieanalyse);
        }

        // POST: Categorieanalyse/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var categorieanalyse = await _context.Categorieanalyses.FindAsync(id);
            if (categorieanalyse != null)
            {
                _context.Categorieanalyses.Remove(categorieanalyse);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategorieanalyseExists(Guid id)
        {
            return _context.Categorieanalyses.Any(e => e.Categorieanalyseid == id);
        }
    }
}
