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
    public class NatureechantillonController : Controller
    {
        private readonly ProlabwebContext _context;

        public NatureechantillonController(ProlabwebContext context)
        {
            _context = context;
        }

        // GET: Natureechantillon
        public async Task<IActionResult> Index()
        {
            return View(await _context.Natureechantillons.ToListAsync());
        }

        // GET: Natureechantillon/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var natureechantillon = await _context.Natureechantillons
                .FirstOrDefaultAsync(m => m.Idnatureechantillon == id);
            if (natureechantillon == null)
            {
                return NotFound();
            }

            return View(natureechantillon);
        }

        // GET: Natureechantillon/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Natureechantillon/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Idnatureechantillon,Nom,Description,Isactive")] Natureechantillon natureechantillon)
        {
            if (ModelState.IsValid)
            {
                _context.Add(natureechantillon);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(natureechantillon);
        }

        // GET: Natureechantillon/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var natureechantillon = await _context.Natureechantillons.FindAsync(id);
            if (natureechantillon == null)
            {
                return NotFound();
            }
            return View(natureechantillon);
        }

        // POST: Natureechantillon/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Idnatureechantillon,Nom,Description,Isactive")] Natureechantillon natureechantillon)
        {
            if (id != natureechantillon.Idnatureechantillon)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(natureechantillon);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NatureechantillonExists(natureechantillon.Idnatureechantillon))
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
            return View(natureechantillon);
        }

        // GET: Natureechantillon/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var natureechantillon = await _context.Natureechantillons
                .FirstOrDefaultAsync(m => m.Idnatureechantillon == id);
            if (natureechantillon == null)
            {
                return NotFound();
            }

            return View(natureechantillon);
        }

        // POST: Natureechantillon/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var natureechantillon = await _context.Natureechantillons.FindAsync(id);
            if (natureechantillon != null)
            {
                _context.Natureechantillons.Remove(natureechantillon);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NatureechantillonExists(int id)
        {
            return _context.Natureechantillons.Any(e => e.Idnatureechantillon == id);
        }
    }
}
