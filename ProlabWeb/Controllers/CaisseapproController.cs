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
    public class CaisseapproController : Controller
    {
        private readonly ProlabwebContext _context;

        public CaisseapproController(ProlabwebContext context)
        {
            _context = context;
        }

        // GET: Caisseappro
        public async Task<IActionResult> Index()
        {
            var prolabwebContext = _context.Caisseappros.Include(c => c.Caisse);
            return View(await prolabwebContext.ToListAsync());
        }

        // GET: Caisseappro/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var caisseappro = await _context.Caisseappros
                .Include(c => c.Caisse)
                .FirstOrDefaultAsync(m => m.Caisseapproid == id);
            if (caisseappro == null)
            {
                return NotFound();
            }

            return View(caisseappro);
        }

        // GET: Caisseappro/Create
        public IActionResult Create()
        {
            ViewData["Caisseid"] = new SelectList(_context.Caisses, "Caisseid", "Caisseid");
            return View();
        }

        // POST: Caisseappro/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Caisseapproid,Caisseid,Date,Montant")] Caisseappro caisseappro)
        {
            if (ModelState.IsValid)
            {
                caisseappro.Caisseapproid = Guid.NewGuid();
                _context.Add(caisseappro);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Caisseid"] = new SelectList(_context.Caisses, "Caisseid", "Caisseid", caisseappro.Caisseid);
            return View(caisseappro);
        }

        // GET: Caisseappro/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var caisseappro = await _context.Caisseappros.FindAsync(id);
            if (caisseappro == null)
            {
                return NotFound();
            }
            ViewData["Caisseid"] = new SelectList(_context.Caisses, "Caisseid", "Caisseid", caisseappro.Caisseid);
            return View(caisseappro);
        }

        // POST: Caisseappro/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Caisseapproid,Caisseid,Date,Montant")] Caisseappro caisseappro)
        {
            if (id != caisseappro.Caisseapproid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(caisseappro);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CaisseapproExists(caisseappro.Caisseapproid))
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
            ViewData["Caisseid"] = new SelectList(_context.Caisses, "Caisseid", "Caisseid", caisseappro.Caisseid);
            return View(caisseappro);
        }

        // GET: Caisseappro/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var caisseappro = await _context.Caisseappros
                .Include(c => c.Caisse)
                .FirstOrDefaultAsync(m => m.Caisseapproid == id);
            if (caisseappro == null)
            {
                return NotFound();
            }

            return View(caisseappro);
        }

        // POST: Caisseappro/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var caisseappro = await _context.Caisseappros.FindAsync(id);
            if (caisseappro != null)
            {
                _context.Caisseappros.Remove(caisseappro);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CaisseapproExists(Guid id)
        {
            return _context.Caisseappros.Any(e => e.Caisseapproid == id);
        }
    }
}
