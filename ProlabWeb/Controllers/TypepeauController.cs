using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProlabWeb.Db;

namespace ProlabWeb.Controllers
{
    public class TypepeauController : Controller
    {
        private readonly ProlabwebContext _context;

        public TypepeauController(ProlabwebContext context)
        {
            _context = context;
        }

        // GET: Typepeau
        public async Task<IActionResult> Index()
        {
            return View(await _context.Typepeaus.ToListAsync());
        }

        // GET: Typepeau/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var typepeau = await _context.Typepeaus
                .FirstOrDefaultAsync(m => m.Codetypepeau == id);
            if (typepeau == null)
            {
                return NotFound();
            }

            return View(typepeau);
        }

        // GET: Typepeau/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Typepeau/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Codetypepeau,Nom,Description,Isactive")] Typepeau typepeau)
        {
            if (ModelState.IsValid)
            {
                _context.Add(typepeau);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(typepeau);
        }

        // GET: Typepeau/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var typepeau = await _context.Typepeaus.FindAsync(id);
            if (typepeau == null)
            {
                return NotFound();
            }
            return View(typepeau);
        }

        // POST: Typepeau/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Codetypepeau,Nom,Description,Isactive")] Typepeau typepeau)
        {
            if (id != typepeau.Codetypepeau)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(typepeau);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TypepeauExists(typepeau.Codetypepeau))
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
            return View(typepeau);
        }

        // GET: Typepeau/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var typepeau = await _context.Typepeaus
                .FirstOrDefaultAsync(m => m.Codetypepeau == id);
            if (typepeau == null)
            {
                return NotFound();
            }

            return View(typepeau);
        }

        // POST: Typepeau/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var typepeau = await _context.Typepeaus.FindAsync(id);
            if (typepeau != null)
            {
                _context.Typepeaus.Remove(typepeau);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TypepeauExists(string id)
        {
            return _context.Typepeaus.Any(e => e.Codetypepeau == id);
        }
    }
}
