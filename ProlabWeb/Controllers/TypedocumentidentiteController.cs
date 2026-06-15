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
    public class TypedocumentidentiteController : Controller
    {
        private readonly ProlabwebContext _context;

        public TypedocumentidentiteController(ProlabwebContext context)
        {
            _context = context;
        }

        // GET: Typedocumentidentite
        public async Task<IActionResult> Index()
        {
            return View(await _context.Typedocumentidentites.ToListAsync());
        }

        // GET: Typedocumentidentite/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var typedocumentidentite = await _context.Typedocumentidentites
                .FirstOrDefaultAsync(m => m.Codetypedocumentidentite == id);
            if (typedocumentidentite == null)
            {
                return NotFound();
            }

            return View(typedocumentidentite);
        }

        // GET: Typedocumentidentite/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Typedocumentidentite/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Codetypedocumentidentite,Nom,Description")] Typedocumentidentite typedocumentidentite)
        {
            if (ModelState.IsValid)
            {
                _context.Add(typedocumentidentite);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(typedocumentidentite);
        }

        // GET: Typedocumentidentite/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var typedocumentidentite = await _context.Typedocumentidentites.FindAsync(id);
            if (typedocumentidentite == null)
            {
                return NotFound();
            }
            return View(typedocumentidentite);
        }

        // POST: Typedocumentidentite/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Codetypedocumentidentite,Nom,Description")] Typedocumentidentite typedocumentidentite)
        {
            if (id != typedocumentidentite.Codetypedocumentidentite)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(typedocumentidentite);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TypedocumentidentiteExists(typedocumentidentite.Codetypedocumentidentite))
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
            return View(typedocumentidentite);
        }

        // GET: Typedocumentidentite/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var typedocumentidentite = await _context.Typedocumentidentites
                .FirstOrDefaultAsync(m => m.Codetypedocumentidentite == id);
            if (typedocumentidentite == null)
            {
                return NotFound();
            }

            return View(typedocumentidentite);
        }

        // POST: Typedocumentidentite/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var typedocumentidentite = await _context.Typedocumentidentites.FindAsync(id);
            if (typedocumentidentite != null)
            {
                _context.Typedocumentidentites.Remove(typedocumentidentite);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TypedocumentidentiteExists(string id)
        {
            return _context.Typedocumentidentites.Any(e => e.Codetypedocumentidentite == id);
        }
    }
}
