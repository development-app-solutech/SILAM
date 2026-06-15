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
    public class SexeautoriseController : Controller
    {
        private readonly ProlabwebContext _context;

        public SexeautoriseController(ProlabwebContext context)
        {
            _context = context;
        }

        // GET: Sexeautorise
        public async Task<IActionResult> Index()
        {
            return View(await _context.Sexeautorises.ToListAsync());
        }

        // GET: Sexeautorise/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sexeautorise = await _context.Sexeautorises
                .FirstOrDefaultAsync(m => m.Sexeautorisecode == id);
            if (sexeautorise == null)
            {
                return NotFound();
            }

            return View(sexeautorise);
        }

        // GET: Sexeautorise/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Sexeautorise/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Sexeautorisecode,Valeur")] Sexeautorise sexeautorise)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sexeautorise);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sexeautorise);
        }

        // GET: Sexeautorise/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sexeautorise = await _context.Sexeautorises.FindAsync(id);
            if (sexeautorise == null)
            {
                return NotFound();
            }
            return View(sexeautorise);
        }

        // POST: Sexeautorise/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Sexeautorisecode,Valeur")] Sexeautorise sexeautorise)
        {
            if (id != sexeautorise.Sexeautorisecode)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sexeautorise);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SexeautoriseExists(sexeautorise.Sexeautorisecode))
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
            return View(sexeautorise);
        }

        // GET: Sexeautorise/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sexeautorise = await _context.Sexeautorises
                .FirstOrDefaultAsync(m => m.Sexeautorisecode == id);
            if (sexeautorise == null)
            {
                return NotFound();
            }

            return View(sexeautorise);
        }

        // POST: Sexeautorise/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var sexeautorise = await _context.Sexeautorises.FindAsync(id);
            if (sexeautorise != null)
            {
                _context.Sexeautorises.Remove(sexeautorise);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SexeautoriseExists(string id)
        {
            return _context.Sexeautorises.Any(e => e.Sexeautorisecode == id);
        }
    }
}
