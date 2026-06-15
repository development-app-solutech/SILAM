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
    public class UniteController : Controller
    {
        private readonly ProlabwebContext _context;

        public UniteController(ProlabwebContext context)
        {
            _context = context;
        }

        // GET: Unite
        public async Task<IActionResult> Index()
        {
            return View(await _context.Unites.ToListAsync());
        }

        // GET: Unite/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var unite = await _context.Unites
                .FirstOrDefaultAsync(m => m.Code == id);
            if (unite == null)
            {
                return NotFound();
            }

            return View(unite);
        }

        // GET: Unite/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Unite/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Code,Name,Isage,Isageday")] Unite unite)
        {
            if (ModelState.IsValid)
            {
                _context.Add(unite);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(unite);
        }

        // GET: Unite/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var unite = await _context.Unites.FindAsync(id);
            if (unite == null)
            {
                return NotFound();
            }
            return View(unite);
        }

        // POST: Unite/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Code,Name,Isage,Isageday")] Unite unite)
        {
            if (id != unite.Code)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(unite);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UniteExists(unite.Code))
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
            return View(unite);
        }

        // GET: Unite/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var unite = await _context.Unites
                .FirstOrDefaultAsync(m => m.Code == id);
            if (unite == null)
            {
                return NotFound();
            }

            return View(unite);
        }

        // POST: Unite/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var unite = await _context.Unites.FindAsync(id);
            if (unite != null)
            {
                _context.Unites.Remove(unite);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UniteExists(string id)
        {
            return _context.Unites.Any(e => e.Code == id);
        }
    }
}
