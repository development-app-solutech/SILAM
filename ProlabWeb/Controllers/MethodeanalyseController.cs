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
    public class MethodeanalyseController : Controller
    {
        private readonly ProlabwebContext _context;

        public MethodeanalyseController(ProlabwebContext context)
        {
            _context = context;
        }

        // GET: Methodeanalyse
        public async Task<IActionResult> Index()
        {
            var prolabwebContext = _context.Methodeanalyses.Include(m => m.IdanalyseNavigation).Include(m => m.IdmethodeNavigation);
            return View(await prolabwebContext.ToListAsync());
        }

        // GET: Methodeanalyse/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var methodeanalyse = await _context.Methodeanalyses
                .Include(m => m.IdanalyseNavigation)
                .Include(m => m.IdmethodeNavigation)
                .FirstOrDefaultAsync(m => m.Methodeanalyseid == id);
            if (methodeanalyse == null)
            {
                return NotFound();
            }

            return View(methodeanalyse);
        }

        // GET: Methodeanalyse/Create
        public IActionResult Create()
        {
            ViewData["Idanalyse"] = new SelectList(_context.Analyses, "Idanalyse", "Idanalyse");
            ViewData["Idmethode"] = new SelectList(_context.Methodes, "Idmethode", "Idmethode");
            return View();
        }

        // POST: Methodeanalyse/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Idanalyse,Idmethode,Isdefaultmethode,Methodeanalyseid")] Methodeanalyse methodeanalyse)
        {
            if (ModelState.IsValid)
            {
                methodeanalyse.Methodeanalyseid = Guid.NewGuid();
                _context.Add(methodeanalyse);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Idanalyse"] = new SelectList(_context.Analyses, "Idanalyse", "Idanalyse", methodeanalyse.Idanalyse);
            ViewData["Idmethode"] = new SelectList(_context.Methodes, "Idmethode", "Idmethode", methodeanalyse.Idmethode);
            return View(methodeanalyse);
        }

        // GET: Methodeanalyse/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var methodeanalyse = await _context.Methodeanalyses.FindAsync(id);
            if (methodeanalyse == null)
            {
                return NotFound();
            }
            ViewData["Idanalyse"] = new SelectList(_context.Analyses, "Idanalyse", "Idanalyse", methodeanalyse.Idanalyse);
            ViewData["Idmethode"] = new SelectList(_context.Methodes, "Idmethode", "Idmethode", methodeanalyse.Idmethode);
            return View(methodeanalyse);
        }

        // POST: Methodeanalyse/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Idanalyse,Idmethode,Isdefaultmethode,Methodeanalyseid")] Methodeanalyse methodeanalyse)
        {
            if (id != methodeanalyse.Methodeanalyseid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(methodeanalyse);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MethodeanalyseExists(methodeanalyse.Methodeanalyseid))
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
            ViewData["Idanalyse"] = new SelectList(_context.Analyses, "Idanalyse", "Idanalyse", methodeanalyse.Idanalyse);
            ViewData["Idmethode"] = new SelectList(_context.Methodes, "Idmethode", "Idmethode", methodeanalyse.Idmethode);
            return View(methodeanalyse);
        }

        // GET: Methodeanalyse/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var methodeanalyse = await _context.Methodeanalyses
                .Include(m => m.IdanalyseNavigation)
                .Include(m => m.IdmethodeNavigation)
                .FirstOrDefaultAsync(m => m.Methodeanalyseid == id);
            if (methodeanalyse == null)
            {
                return NotFound();
            }

            return View(methodeanalyse);
        }

        // POST: Methodeanalyse/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var methodeanalyse = await _context.Methodeanalyses.FindAsync(id);
            if (methodeanalyse != null)
            {
                _context.Methodeanalyses.Remove(methodeanalyse);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MethodeanalyseExists(Guid id)
        {
            return _context.Methodeanalyses.Any(e => e.Methodeanalyseid == id);
        }
    }
}
