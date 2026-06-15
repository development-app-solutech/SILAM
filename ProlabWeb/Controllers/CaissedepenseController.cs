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
    public class CaissedepenseController : Controller
    {
        private readonly ProlabwebContext _context;

        public CaissedepenseController(ProlabwebContext context)
        {
            _context = context;
        }

        // GET: Caissedepense
        public async Task<IActionResult> Index()
        {
            var prolabwebContext = _context.Caissedepenses.Include(c => c.Caisse);
            return View(await prolabwebContext.ToListAsync());
        }

        // GET: Caissedepense/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var caissedepense = await _context.Caissedepenses
                .Include(c => c.Caisse)
                .FirstOrDefaultAsync(m => m.Caissedepenseid == id);
            if (caissedepense == null)
            {
                return NotFound();
            }

            return View(caissedepense);
        }

        // GET: Caissedepense/Create
        public IActionResult Create()
        {
            ViewData["Caisseid"] = new SelectList(_context.Caisses, "Caisseid", "Caisseid");
            return View();
        }

        // POST: Caissedepense/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Caissedepenseid,Caisseid,Montant,Motif")] Caissedepense caissedepense)
        {
            if (ModelState.IsValid)
            {
                caissedepense.Caissedepenseid = Guid.NewGuid();
                _context.Add(caissedepense);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Caisseid"] = new SelectList(_context.Caisses, "Caisseid", "Caisseid", caissedepense.Caisseid);
            return View(caissedepense);
        }

        // GET: Caissedepense/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var caissedepense = await _context.Caissedepenses.FindAsync(id);
            if (caissedepense == null)
            {
                return NotFound();
            }
            ViewData["Caisseid"] = new SelectList(_context.Caisses, "Caisseid", "Caisseid", caissedepense.Caisseid);
            return View(caissedepense);
        }

        // POST: Caissedepense/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Caissedepenseid,Caisseid,Montant,Motif")] Caissedepense caissedepense)
        {
            if (id != caissedepense.Caissedepenseid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(caissedepense);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CaissedepenseExists(caissedepense.Caissedepenseid))
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
            ViewData["Caisseid"] = new SelectList(_context.Caisses, "Caisseid", "Caisseid", caissedepense.Caisseid);
            return View(caissedepense);
        }

        // GET: Caissedepense/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var caissedepense = await _context.Caissedepenses
                .Include(c => c.Caisse)
                .FirstOrDefaultAsync(m => m.Caissedepenseid == id);
            if (caissedepense == null)
            {
                return NotFound();
            }

            return View(caissedepense);
        }

        // POST: Caissedepense/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var caissedepense = await _context.Caissedepenses.FindAsync(id);
            if (caissedepense != null)
            {
                _context.Caissedepenses.Remove(caissedepense);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CaissedepenseExists(Guid id)
        {
            return _context.Caissedepenses.Any(e => e.Caissedepenseid == id);
        }
    }
}
