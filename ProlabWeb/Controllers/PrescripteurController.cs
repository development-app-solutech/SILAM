using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProlabWeb.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProlabWeb.Controllers
{
    [AllowAnonymous]
    public class PrescripteurController : Controller
    {
        private readonly ProlabwebContext _context;

        public PrescripteurController(ProlabwebContext context)
        {
            _context = context;
        }

        // GET: Prescripteur
        public async Task<IActionResult> Index()
        {
            return View(await _context.Prescripteurs.ToListAsync());
        }

        // GET: Prescripteur/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prescripteur = await _context.Prescripteurs
                .FirstOrDefaultAsync(m => m.Prescripteurid == id);
            if (prescripteur == null)
            {
                return NotFound();
            }

            return View(prescripteur);
        }

        // GET: Prescripteur/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Prescripteur/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Prescripteurid,Nom,Description,Isactive,Adresse,Tel")] Prescripteur prescripteur)
        {
            if (ModelState.IsValid)
            {
                prescripteur.Prescripteurid = Guid.NewGuid();
                _context.Add(prescripteur);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(prescripteur);
        }

        // GET: Prescripteur/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prescripteur = await _context.Prescripteurs.FindAsync(id);
            if (prescripteur == null)
            {
                return NotFound();
            }
            return View(prescripteur);
        }

        // POST: Prescripteur/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Prescripteurid,Nom,Description,Isactive,Adresse,Tel")] Prescripteur prescripteur)
        {
            if (id != prescripteur.Prescripteurid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(prescripteur);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PrescripteurExists(prescripteur.Prescripteurid))
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
            return View(prescripteur);
        }

        // GET: Prescripteur/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prescripteur = await _context.Prescripteurs
                .FirstOrDefaultAsync(m => m.Prescripteurid == id);
            if (prescripteur == null)
            {
                return NotFound();
            }

            return View(prescripteur);
        }

        // POST: Prescripteur/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var prescripteur = await _context.Prescripteurs.FindAsync(id);
            if (prescripteur != null)
            {
                _context.Prescripteurs.Remove(prescripteur);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PrescripteurExists(Guid id)
        {
            return _context.Prescripteurs.Any(e => e.Prescripteurid == id);
        }

        #region handlers

        [HttpGet, ActionName("GetPrescripteur")]
        public async Task<IActionResult> GetPrescripteurAsync(Guid id)
        {
            JsonResponseViewModel resultat = new JsonResponseViewModel();

            if (id != Guid.Empty)
            {
                var prescripteur = await _context.Prescripteurs.AsNoTracking()
                    .Where(x => x.Prescripteurid == id)
                    .FirstOrDefaultAsync();

                if (prescripteur != null)
                {
                    string dataJson = JsonConvert.SerializeObject(prescripteur, Formatting.Indented, new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });

                    resultat.success = true;

                    resultat.data = dataJson;
                }
            }

            return Ok(resultat);
        }

        #endregion
    }
}
