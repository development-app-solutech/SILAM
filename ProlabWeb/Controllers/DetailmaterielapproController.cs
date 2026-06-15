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
    public class DetailmaterielapproController : Controller
    {
        private readonly ProlabwebContext _context;

        public DetailmaterielapproController(ProlabwebContext context)
        {
            _context = context;
        }

        // GET: Detailmaterielappro
        public async Task<IActionResult> Index()
        {
            var prolabwebContext = _context.Detailmaterielappros.Include(d => d.Entetematerielappro).Include(d => d.Materiel);
            return View(await prolabwebContext.ToListAsync());
        }

        // GET: Detailmaterielappro/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detailmaterielappro = await _context.Detailmaterielappros
                .Include(d => d.Entetematerielappro)
                .Include(d => d.Materiel)
                .FirstOrDefaultAsync(m => m.Detailmaterielapproid == id);
            if (detailmaterielappro == null)
            {
                return NotFound();
            }

            return View(detailmaterielappro);
        }

        // GET: Detailmaterielappro/Create
        public IActionResult Create()
        {
            ViewData["Entetematerielapproid"] = new SelectList(_context.Entetematerielappros, "Entetematerielapproid", "Entetematerielapproid");
            ViewData["Materielid"] = new SelectList(_context.Materiels, "Materielid", "Materielid");
            return View();
        }

        // POST: Detailmaterielappro/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Detailmaterielapproid,Entetematerielapproid,Materielid,Quantite")] Detailmaterielappro detailmaterielappro)
        {
            if (ModelState.IsValid)
            {
                detailmaterielappro.Detailmaterielapproid = Guid.NewGuid();
                _context.Add(detailmaterielappro);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Entetematerielapproid"] = new SelectList(_context.Entetematerielappros, "Entetematerielapproid", "Entetematerielapproid", detailmaterielappro.Entetematerielapproid);
            ViewData["Materielid"] = new SelectList(_context.Materiels, "Materielid", "Materielid", detailmaterielappro.Materielid);
            return View(detailmaterielappro);
        }

        // GET: Detailmaterielappro/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detailmaterielappro = await _context.Detailmaterielappros.FindAsync(id);
            if (detailmaterielappro == null)
            {
                return NotFound();
            }
            ViewData["Entetematerielapproid"] = new SelectList(_context.Entetematerielappros, "Entetematerielapproid", "Entetematerielapproid", detailmaterielappro.Entetematerielapproid);
            ViewData["Materielid"] = new SelectList(_context.Materiels, "Materielid", "Materielid", detailmaterielappro.Materielid);
            return View(detailmaterielappro);
        }

        // POST: Detailmaterielappro/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Detailmaterielapproid,Entetematerielapproid,Materielid,Quantite")] Detailmaterielappro detailmaterielappro)
        {
            if (id != detailmaterielappro.Detailmaterielapproid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(detailmaterielappro);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DetailmaterielapproExists(detailmaterielappro.Detailmaterielapproid))
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
            ViewData["Entetematerielapproid"] = new SelectList(_context.Entetematerielappros, "Entetematerielapproid", "Entetematerielapproid", detailmaterielappro.Entetematerielapproid);
            ViewData["Materielid"] = new SelectList(_context.Materiels, "Materielid", "Materielid", detailmaterielappro.Materielid);
            return View(detailmaterielappro);
        }

        // GET: Detailmaterielappro/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detailmaterielappro = await _context.Detailmaterielappros
                .Include(d => d.Entetematerielappro)
                .Include(d => d.Materiel)
                .FirstOrDefaultAsync(m => m.Detailmaterielapproid == id);
            if (detailmaterielappro == null)
            {
                return NotFound();
            }

            return View(detailmaterielappro);
        }

        // POST: Detailmaterielappro/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var detailmaterielappro = await _context.Detailmaterielappros.FindAsync(id);
            if (detailmaterielappro != null)
            {
                _context.Detailmaterielappros.Remove(detailmaterielappro);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DetailmaterielapproExists(Guid id)
        {
            return _context.Detailmaterielappros.Any(e => e.Detailmaterielapproid == id);
        }
    }
}
