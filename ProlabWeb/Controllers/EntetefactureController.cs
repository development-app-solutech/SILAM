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
    public class EntetefactureController : Controller
    {
        private readonly ProlabwebContext _context;

        public EntetefactureController(ProlabwebContext context)
        {
            _context = context;
        }

        // GET: Entetefacture
        public async Task<IActionResult> Index()
        {
            var prolabwebContext = _context.Entetefactures.Include(e => e.Caisse).Include(e => e.Utilisateur).Include(e => e.Entetedemande);
            return View(await prolabwebContext.ToListAsync());
        }

        // GET: Entetefacture/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entetefacture = await _context.Entetefactures
                .Include(e => e.Caisse)
                .Include(e => e.Utilisateur)
                .Include(e => e.Entetedemande)
                .FirstOrDefaultAsync(m => m.Entetefactureid == id);
            if (entetefacture == null)
            {
                return NotFound();
            }

            return View(entetefacture);
        }

        // GET: Entetefacture/Create
        public IActionResult Create()
        {
            ViewData["Caisseid"] = new SelectList(_context.Caisses, "Caisseid", "Caisseid");
            ViewData["Caissierid"] = new SelectList(_context.Utilisateurs, "Caissierid", "Caissierid");
            ViewData["Entetedemandeid"] = new SelectList(_context.Entetedemandes, "Entetedemandeid", "Entetedemandeid");
            return View();
        }

        // POST: Entetefacture/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Entetefactureid,Entetedemandeid,Caisseid,Caissierid,Date,Numero")] Entetefacture entetefacture)
        {
            if (ModelState.IsValid)
            {
                entetefacture.Entetefactureid = Guid.NewGuid();
                _context.Add(entetefacture);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Caisseid"] = new SelectList(_context.Caisses, "Caisseid", "Caisseid", entetefacture.Caisseid);
            ViewData["Caissierid"] = new SelectList(_context.Utilisateurs, "Caissierid", "Caissierid", entetefacture.Utilisateurid);
            ViewData["Entetedemandeid"] = new SelectList(_context.Entetedemandes, "Entetedemandeid", "Entetedemandeid", entetefacture.Entetedemandeid);
            return View(entetefacture);
        }

        // GET: Entetefacture/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entetefacture = await _context.Entetefactures.FindAsync(id);
            if (entetefacture == null)
            {
                return NotFound();
            }
            ViewData["Caisseid"] = new SelectList(_context.Caisses, "Caisseid", "Caisseid", entetefacture.Caisseid);
            ViewData["Caissierid"] = new SelectList(_context.Utilisateurs, "Caissierid", "Caissierid", entetefacture.Utilisateur);
            ViewData["Entetedemandeid"] = new SelectList(_context.Entetedemandes, "Entetedemandeid", "Entetedemandeid", entetefacture.Entetedemandeid);
            return View(entetefacture);
        }

        // POST: Entetefacture/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Entetefactureid,Entetedemandeid,Caisseid,Caissierid,Date,Numero")] Entetefacture entetefacture)
        {
            if (id != entetefacture.Entetefactureid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(entetefacture);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EntetefactureExists(entetefacture.Entetefactureid))
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
            ViewData["Caisseid"] = new SelectList(_context.Caisses, "Caisseid", "Caisseid", entetefacture.Caisseid);
            ViewData["Caissierid"] = new SelectList(_context.Utilisateurs, "Caissierid", "Caissierid", entetefacture.Utilisateurid);
            ViewData["Entetedemandeid"] = new SelectList(_context.Entetedemandes, "Entetedemandeid", "Entetedemandeid", entetefacture.Entetedemandeid);
            return View(entetefacture);
        }

        // GET: Entetefacture/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entetefacture = await _context.Entetefactures
                .Include(e => e.Caisse)
                .Include(e => e.Utilisateur)
                .Include(e => e.Entetedemande)
                .FirstOrDefaultAsync(m => m.Entetefactureid == id);
            if (entetefacture == null)
            {
                return NotFound();
            }

            return View(entetefacture);
        }

        // POST: Entetefacture/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var entetefacture = await _context.Entetefactures.FindAsync(id);
            if (entetefacture != null)
            {
                _context.Entetefactures.Remove(entetefacture);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EntetefactureExists(Guid id)
        {
            return _context.Entetefactures.Any(e => e.Entetefactureid == id);
        }
    }
}
