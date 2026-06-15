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
    public class PreleveurController : Controller
    {
        private readonly ProlabwebContext _context;

        public PreleveurController(ProlabwebContext context)
        {
            _context = context;
        }

        // GET: Preleveur
        public async Task<IActionResult> Index()
        {
            var prolabwebContext = _context.Preleveurs.Include(p => p.CodesexeNavigation);
            return View(await prolabwebContext.ToListAsync());
        }

        // GET: Preleveur/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var preleveur = await _context.Preleveurs
                .Include(p => p.CodesexeNavigation)
                .FirstOrDefaultAsync(m => m.Preleveurid == id);
            if (preleveur == null)
            {
                return NotFound();
            }

            return View(preleveur);
        }

        // GET: Preleveur/Create
        public IActionResult Create()
        {
            ViewData["Codesexe"] = new SelectList(_context.Sexes, "Codesexe", "Codesexe");
            return View(new ProlabWeb.ViewModels.PreleveurCreateVM());
        }

        // POST: Preleveur/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProlabWeb.ViewModels.PreleveurCreateVM model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Codesexe"] = new SelectList(_context.Sexes, "Codesexe", "Codesexe", model.Codesexe);
                return View(model);
            }
            var preleveur = new Preleveur
            {
                Preleveurid = Guid.NewGuid(),
                Nom = model.Nom,
                Prenom = model.Prenom,
                Codesexe = model.Codesexe,
                Datenaissance = model.Datenaissance,
                Tel1 = model.Tel1,
                Tel2 = model.Tel2,
                Mob1 = model.Mob1,
                Mob2 = model.Mob2,
                Email = model.Email,
                Fonction = model.Type,
            };
            _context.Add(preleveur);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Preleveur/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var preleveur = await _context.Preleveurs.FindAsync(id);
            if (preleveur == null)
            {
                return NotFound();
            }
            var model = new ProlabWeb.ViewModels.PreleveurEditVM
            {
                Preleveurid = preleveur.Preleveurid,
                Nom = preleveur.Nom,
                Prenom = preleveur.Prenom,
                Codesexe = preleveur.Codesexe,
                Datenaissance = preleveur.Datenaissance,
                Tel1 = preleveur.Tel1,
                Tel2 = preleveur.Tel2,
                Mob1 = preleveur.Mob1,
                Mob2 = preleveur.Mob2,
                Email = preleveur.Email,
                Type = preleveur.Fonction,
            };
            ViewData["Codesexe"] = new SelectList(_context.Sexes, "Codesexe", "Codesexe", model.Codesexe);
            return View(model);
        }

        // POST: Preleveur/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ProlabWeb.ViewModels.PreleveurEditVM model)
        {
            if (id != model.Preleveurid)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
                ViewData["Codesexe"] = new SelectList(_context.Sexes, "Codesexe", "Codesexe", model.Codesexe);
                return View(model);
            }
            var preleveur = await _context.Preleveurs.FindAsync(id);
            if (preleveur == null)
            {
                return NotFound();
            }
            preleveur.Nom = model.Nom;
            preleveur.Prenom = model.Prenom;
            preleveur.Codesexe = model.Codesexe;
            preleveur.Datenaissance = model.Datenaissance;
            preleveur.Tel1 = model.Tel1;
            preleveur.Tel2 = model.Tel2;
            preleveur.Mob1 = model.Mob1;
            preleveur.Mob2 = model.Mob2;
            preleveur.Email = model.Email;
            preleveur.Fonction = model.Type;
            try
            {
                _context.Update(preleveur);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PreleveurExists(model.Preleveurid))
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

        // GET: Preleveur/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var preleveur = await _context.Preleveurs
                .Include(p => p.CodesexeNavigation)
                .FirstOrDefaultAsync(m => m.Preleveurid == id);
            if (preleveur == null)
            {
                return NotFound();
            }

            return View(preleveur);
        }

        // POST: Preleveur/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var preleveur = await _context.Preleveurs.FindAsync(id);
            if (preleveur != null)
            {
                _context.Preleveurs.Remove(preleveur);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PreleveurExists(Guid id)
        {
            return _context.Preleveurs.Any(e => e.Preleveurid == id);
        }
    }
}
