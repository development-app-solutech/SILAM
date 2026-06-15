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
    public class TypeassuranceController : Controller
    {
        private readonly ProlabwebContext _context;

        public TypeassuranceController(ProlabwebContext context)
        {
            _context = context;
        }

        // GET: Typeassurance
        public async Task<IActionResult> Index()
        {
            return View(await _context.Typeassurances.ToListAsync());
        }

        // GET: Typeassurance/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var typeassurance = await _context.Typeassurances
                .FirstOrDefaultAsync(m => m.Codetypeassurance == id);
            if (typeassurance == null)
            {
                return NotFound();
            }

            return View(typeassurance);
        }

        // GET: Typeassurance/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Typeassurance/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Codetypeassurance,Nom")] Typeassurance typeassurance)
        {
            if (ModelState.IsValid)
            {
                _context.Add(typeassurance);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(typeassurance);
        }

        // GET: Typeassurance/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var typeassurance = await _context.Typeassurances.FindAsync(id);
            if (typeassurance == null)
            {
                return NotFound();
            }
            return View(typeassurance);
        }

        // POST: Typeassurance/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Codetypeassurance,Nom")] Typeassurance typeassurance)
        {
            if (id != typeassurance.Codetypeassurance)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(typeassurance);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TypeassuranceExists(typeassurance.Codetypeassurance))
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
            return View(typeassurance);
        }

        // GET: Typeassurance/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var typeassurance = await _context.Typeassurances
                .FirstOrDefaultAsync(m => m.Codetypeassurance == id);
            if (typeassurance == null)
            {
                return NotFound();
            }

            return View(typeassurance);
        }

        // POST: Typeassurance/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var typeassurance = await _context.Typeassurances.FindAsync(id);
            if (typeassurance != null)
            {
                _context.Typeassurances.Remove(typeassurance);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TypeassuranceExists(string id)
        {
            return _context.Typeassurances.Any(e => e.Codetypeassurance == id);
        }
    }
}
