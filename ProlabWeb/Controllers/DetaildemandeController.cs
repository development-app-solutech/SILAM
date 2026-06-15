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
    public class DetaildemandeController : Controller
    {
        private readonly ProlabwebContext _context;

        public DetaildemandeController(ProlabwebContext context)
        {
            _context = context;
        }

        // GET: Detaildemande
        public async Task<IActionResult> Index()
        {
            var prolabwebContext = _context.Detaildemandes.Include(d => d.Categorie).Include(d => d.Entetedemande).Include(d => d.IdanalyseNavigation);
            return View(await prolabwebContext.ToListAsync());
        }

        // GET: Detaildemande/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detaildemande = await _context.Detaildemandes
                .Include(d => d.Categorie)
                .Include(d => d.Entetedemande)
                .Include(d => d.IdanalyseNavigation)
                .FirstOrDefaultAsync(m => m.Detaildemandeid == id);
            if (detaildemande == null)
            {
                return NotFound();
            }

            return View(detaildemande);
        }

        // GET: Detaildemande/Create
        public IActionResult Create()
        {
            ViewData["Categorieid"] = new SelectList(_context.Categories, "Categorieid", "Categorieid");
            ViewData["Entetedemandeid"] = new SelectList(_context.Entetedemandes, "Entetedemandeid", "Entetedemandeid");
            ViewData["Idanalyse"] = new SelectList(_context.Analyses, "Idanalyse", "Idanalyse");
            return View();
        }

        // POST: Detaildemande/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Detaildemandeid,Categorieid,Idanalyse,Entetedemandeid")] Detaildemande detaildemande)
        {
            if (ModelState.IsValid)
            {
                detaildemande.Detaildemandeid = Guid.NewGuid();
                _context.Add(detaildemande);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Categorieid"] = new SelectList(_context.Categories, "Categorieid", "Categorieid", detaildemande.Categorieid);
            ViewData["Entetedemandeid"] = new SelectList(_context.Entetedemandes, "Entetedemandeid", "Entetedemandeid", detaildemande.Entetedemandeid);
            ViewData["Idanalyse"] = new SelectList(_context.Analyses, "Idanalyse", "Idanalyse", detaildemande.Idanalyse);
            return View(detaildemande);
        }

        // GET: Detaildemande/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detaildemande = await _context.Detaildemandes.FindAsync(id);
            if (detaildemande == null)
            {
                return NotFound();
            }
            ViewData["Categorieid"] = new SelectList(_context.Categories, "Categorieid", "Categorieid", detaildemande.Categorieid);
            ViewData["Entetedemandeid"] = new SelectList(_context.Entetedemandes, "Entetedemandeid", "Entetedemandeid", detaildemande.Entetedemandeid);
            ViewData["Idanalyse"] = new SelectList(_context.Analyses, "Idanalyse", "Idanalyse", detaildemande.Idanalyse);
            return View(detaildemande);
        }

        // POST: Detaildemande/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Detaildemandeid,Categorieid,Idanalyse,Entetedemandeid")] Detaildemande detaildemande)
        {
            if (id != detaildemande.Detaildemandeid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(detaildemande);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DetaildemandeExists(detaildemande.Detaildemandeid))
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
            ViewData["Categorieid"] = new SelectList(_context.Categories, "Categorieid", "Categorieid", detaildemande.Categorieid);
            ViewData["Entetedemandeid"] = new SelectList(_context.Entetedemandes, "Entetedemandeid", "Entetedemandeid", detaildemande.Entetedemandeid);
            ViewData["Idanalyse"] = new SelectList(_context.Analyses, "Idanalyse", "Idanalyse", detaildemande.Idanalyse);
            return View(detaildemande);
        }

        // GET: Detaildemande/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detaildemande = await _context.Detaildemandes
                .Include(d => d.Categorie)
                .Include(d => d.Entetedemande)
                .Include(d => d.IdanalyseNavigation)
                .FirstOrDefaultAsync(m => m.Detaildemandeid == id);
            if (detaildemande == null)
            {
                return NotFound();
            }

            return View(detaildemande);
        }

        // POST: Detaildemande/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var detaildemande = await _context.Detaildemandes.FindAsync(id);
            if (detaildemande != null)
            {
                _context.Detaildemandes.Remove(detaildemande);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DetaildemandeExists(Guid id)
        {
            return _context.Detaildemandes.Any(e => e.Detaildemandeid == id);
        }
    }
}
