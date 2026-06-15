using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProlabWeb.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProlabWeb.Controllers
{
    [AllowAnonymous]
    public class LoboratoireController : Controller
    {
        private readonly ProlabwebContext _context;

        public LoboratoireController(ProlabwebContext context)
        {
            _context = context;
        }

        // GET: Loboratoire
        public async Task<IActionResult> Index()
        {
            return View(await _context.Loboratoires.ToListAsync());
        }

        // GET: Loboratoire/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loboratoire = await _context.Loboratoires
                .FirstOrDefaultAsync(m => m.Idlaboratoire == id);
            if (loboratoire == null)
            {
                return NotFound();
            }

            return View(loboratoire);
        }

        // GET: Loboratoire/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Loboratoire/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Idlaboratoire,Nom,Description,Isactive")] Loboratoire loboratoire)
        {
            if (ModelState.IsValid)
            {
                _context.Add(loboratoire);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(loboratoire);
        }

        // GET: Loboratoire/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loboratoire = await _context.Loboratoires.FindAsync(id);
            if (loboratoire == null)
            {
                return NotFound();
            }
            return View(loboratoire);
        }

        // POST: Loboratoire/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Idlaboratoire,Nom,Description,Isactive")] Loboratoire loboratoire)
        {
            if (id != loboratoire.Idlaboratoire)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(loboratoire);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LoboratoireExists(loboratoire.Idlaboratoire))
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
            return View(loboratoire);
        }

        // GET: Loboratoire/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loboratoire = await _context.Loboratoires
                .FirstOrDefaultAsync(m => m.Idlaboratoire == id);
            if (loboratoire == null)
            {
                return NotFound();
            }

            return View(loboratoire);
        }

        // POST: Loboratoire/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loboratoire = await _context.Loboratoires.FindAsync(id);
            if (loboratoire != null)
            {
                _context.Loboratoires.Remove(loboratoire);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LoboratoireExists(int id)
        {
            return _context.Loboratoires.Any(e => e.Idlaboratoire == id);
        }
    }
}
