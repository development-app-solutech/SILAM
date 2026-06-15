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
    public class SexeController : Controller
    {
        private readonly ProlabwebContext _context;

        public SexeController(ProlabwebContext context)
        {
            _context = context;
        }

        // GET: Sexe
        public async Task<IActionResult> Index()
        {
            return View(await _context.Sexes.ToListAsync());
        }

        // GET: Sexe/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sexe = await _context.Sexes
                .FirstOrDefaultAsync(m => m.Codesexe == id);
            if (sexe == null)
            {
                return NotFound();
            }

            return View(sexe);
        }

        // GET: Sexe/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Sexe/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Codesexe,Value")] Sexe sexe)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sexe);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sexe);
        }

        // GET: Sexe/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sexe = await _context.Sexes.FindAsync(id);
            if (sexe == null)
            {
                return NotFound();
            }
            return View(sexe);
        }

        // POST: Sexe/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Codesexe,Value")] Sexe sexe)
        {
            if (id != sexe.Codesexe)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sexe);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SexeExists(sexe.Codesexe))
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
            return View(sexe);
        }

        // GET: Sexe/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sexe = await _context.Sexes
                .FirstOrDefaultAsync(m => m.Codesexe == id);
            if (sexe == null)
            {
                return NotFound();
            }

            return View(sexe);
        }

        // POST: Sexe/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var sexe = await _context.Sexes.FindAsync(id);
            if (sexe != null)
            {
                _context.Sexes.Remove(sexe);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SexeExists(string id)
        {
            return _context.Sexes.Any(e => e.Codesexe == id);
        }
    }
}
