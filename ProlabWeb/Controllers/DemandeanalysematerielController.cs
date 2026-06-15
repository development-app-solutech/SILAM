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
    public class DemandeanalysematerielController : Controller
    {
        private readonly ProlabwebContext _context;

        public DemandeanalysematerielController(ProlabwebContext context)
        {
            _context = context;
        }

        // GET: Demandeanalysemateriel
        public async Task<IActionResult> Index()
        {
            var prolabwebContext = _context.Demandeanalysemateriels.Include(d => d.Analysemateriel).Include(d => d.Entetedemande);
            return View(await prolabwebContext.ToListAsync());
        }

        // GET: Demandeanalysemateriel/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var demandeanalysemateriel = await _context.Demandeanalysemateriels
                .Include(d => d.Analysemateriel)
                .Include(d => d.Entetedemande)
                .FirstOrDefaultAsync(m => m.Demandeanalysematerielid == id);
            if (demandeanalysemateriel == null)
            {
                return NotFound();
            }

            return View(demandeanalysemateriel);
        }

        // GET: Demandeanalysemateriel/Create
        public IActionResult Create()
        {
            ViewData["Analysematerielid"] = new SelectList(_context.Analysemateriels, "Analysematerielid", "Analysematerielid");
            ViewData["Entetedemandeid"] = new SelectList(_context.Entetedemandes, "Entetedemandeid", "Entetedemandeid");
            return View();
        }

        // POST: Demandeanalysemateriel/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Demandeanalysematerielid,Entetedemandeid,Analysematerielid,Quantite")] Demandeanalysemateriel demandeanalysemateriel)
        {
            if (ModelState.IsValid)
            {
                demandeanalysemateriel.Demandeanalysematerielid = Guid.NewGuid();
                _context.Add(demandeanalysemateriel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Analysematerielid"] = new SelectList(_context.Analysemateriels, "Analysematerielid", "Analysematerielid", demandeanalysemateriel.Analysematerielid);
            ViewData["Entetedemandeid"] = new SelectList(_context.Entetedemandes, "Entetedemandeid", "Entetedemandeid", demandeanalysemateriel.Entetedemandeid);
            return View(demandeanalysemateriel);
        }

        // GET: Demandeanalysemateriel/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var demandeanalysemateriel = await _context.Demandeanalysemateriels.FindAsync(id);
            if (demandeanalysemateriel == null)
            {
                return NotFound();
            }
            ViewData["Analysematerielid"] = new SelectList(_context.Analysemateriels, "Analysematerielid", "Analysematerielid", demandeanalysemateriel.Analysematerielid);
            ViewData["Entetedemandeid"] = new SelectList(_context.Entetedemandes, "Entetedemandeid", "Entetedemandeid", demandeanalysemateriel.Entetedemandeid);
            return View(demandeanalysemateriel);
        }

        // POST: Demandeanalysemateriel/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Demandeanalysematerielid,Entetedemandeid,Analysematerielid,Quantite")] Demandeanalysemateriel demandeanalysemateriel)
        {
            if (id != demandeanalysemateriel.Demandeanalysematerielid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(demandeanalysemateriel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DemandeanalysematerielExists(demandeanalysemateriel.Demandeanalysematerielid))
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
            ViewData["Analysematerielid"] = new SelectList(_context.Analysemateriels, "Analysematerielid", "Analysematerielid", demandeanalysemateriel.Analysematerielid);
            ViewData["Entetedemandeid"] = new SelectList(_context.Entetedemandes, "Entetedemandeid", "Entetedemandeid", demandeanalysemateriel.Entetedemandeid);
            return View(demandeanalysemateriel);
        }

        // GET: Demandeanalysemateriel/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var demandeanalysemateriel = await _context.Demandeanalysemateriels
                .Include(d => d.Analysemateriel)
                .Include(d => d.Entetedemande)
                .FirstOrDefaultAsync(m => m.Demandeanalysematerielid == id);
            if (demandeanalysemateriel == null)
            {
                return NotFound();
            }

            return View(demandeanalysemateriel);
        }

        // POST: Demandeanalysemateriel/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var demandeanalysemateriel = await _context.Demandeanalysemateriels.FindAsync(id);
            if (demandeanalysemateriel != null)
            {
                _context.Demandeanalysemateriels.Remove(demandeanalysemateriel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DemandeanalysematerielExists(Guid id)
        {
            return _context.Demandeanalysemateriels.Any(e => e.Demandeanalysematerielid == id);
        }
    }
}
