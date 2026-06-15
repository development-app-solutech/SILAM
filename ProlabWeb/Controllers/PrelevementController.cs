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
    public class PrelevementController : Controller
    {
        private readonly ProlabwebContext _context;

        public PrelevementController(ProlabwebContext context)
        {
            _context = context;
        }

        // GET: Prelevement
        public async Task<IActionResult> Index()
        {
            var prelevements = _context.Prelevements
                .Include(p => p.Detaildemande)
                    .ThenInclude(d => d.Entetedemande)
                        .ThenInclude(e => e.Patient)
                .Include(p => p.IdnatureechantillonNavigation)
                .Include(p => p.Preleveur);

            return View(await prelevements.ToListAsync());
        }

        // GET: Prelevement/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prelevement = await _context.Prelevements
                .Include(p => p.Detaildemande)
                .Include(p => p.IdnatureechantillonNavigation)
                .Include(p => p.Preleveur)
                .FirstOrDefaultAsync(m => m.Prelevementid == id);
            if (prelevement == null)
            {
                return NotFound();
            }

            return View(prelevement);
        }

        // GET: Prelevement/Create
        public IActionResult Create()
        {
            ViewData["Detaildemandeid"] = new SelectList(
                _context.Detaildemandes
                    .Include(d => d.Entetedemande)
                    .ThenInclude(e => e.Patient)
                    .Where(d => d.Entetedemande != null && d.Entetedemande.Patient != null)
                    .Select(d => new {
                        d.Detaildemandeid,
                        NomPatient = d.Entetedemande.Patient.Nom + " " + d.Entetedemande.Patient.Prenom
                    }),
                "Detaildemandeid", "NomPatient");
            ViewData["Idnatureechantillon"] = new SelectList(
                _context.Natureechantillons.Where(n => n.Nom != null).ToList(),
                "Idnatureechantillon", "Nom");
            ViewData["Preleveurid"] = new SelectList(
                _context.Preleveurs.Select(p => new { p.Preleveurid, NomComplet = p.Nom + " " + p.Prenom }),
                "Preleveurid", "NomComplet");
            return View(new ProlabWeb.ViewModels.PrelevementCreateVM());
        }

        // POST: Prelevement/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProlabWeb.ViewModels.PrelevementCreateVM model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Detaildemandeid"] = new SelectList(
                    _context.Detaildemandes
                        .Include(d => d.Entetedemande)
                        .ThenInclude(e => e.Patient)
                        .Where(d => d.Entetedemande != null && d.Entetedemande.Patient != null)
                        .Select(d => new {
                            d.Detaildemandeid,
                            NomPatient = d.Entetedemande.Patient.Nom + " " + d.Entetedemande.Patient.Prenom
                        }),
                    "Detaildemandeid", "NomPatient", model.Detaildemandeid);
                ViewData["Idnatureechantillon"] = new SelectList(
                    _context.Natureechantillons.Where(n => n.Nom != null).ToList(),
                    "Idnatureechantillon", "Nom", model.Idnatureechantillon);
                ViewData["Preleveurid"] = new SelectList(
                    _context.Preleveurs.Select(p => new { p.Preleveurid, NomComplet = p.Nom + " " + p.Prenom }),
                    "Preleveurid", "NomComplet", model.Preleveurid);
                return View(model);
            }
            var prelevement = new Prelevement
            {
                Prelevementid = Guid.NewGuid(),
                Detaildemandeid = model.Detaildemandeid,
                Preleveurid = model.Preleveurid,
                Idnatureechantillon = model.Idnatureechantillon,
                Dateprelevement = model.Date,
                Statut = model.Statut,
                Datereception = model.Datereception
            };
                _context.Add(prelevement);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
        }

        // GET: Prelevement/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var prelevement = await _context.Prelevements.FindAsync(id);
            if (prelevement == null)
            {
                return NotFound();
            }
            var model = new ProlabWeb.ViewModels.PrelevementEditVM
            {
                Prelevementid = prelevement.Prelevementid,
                Detaildemandeid = prelevement.Detaildemandeid,
                Preleveurid = prelevement.Preleveurid,
                Idnatureechantillon = prelevement.Idnatureechantillon,
                Date = prelevement.Dateprelevement,
                Statut = prelevement.Statut,
                Datereception = prelevement.Datereception
            };
            ViewData["Detaildemandeid"] = new SelectList(
                _context.Detaildemandes
                    .Include(d => d.Entetedemande)
                    .ThenInclude(e => e.Patient)
                    .Where(d => d.Entetedemande != null && d.Entetedemande.Patient != null)
                    .Select(d => new {
                        d.Detaildemandeid,
                        NomPatient = d.Entetedemande.Patient.Nom + " " + d.Entetedemande.Patient.Prenom
                    }),
                "Detaildemandeid", "NomPatient", model.Detaildemandeid);
            ViewData["Idnatureechantillon"] = new SelectList(
                _context.Natureechantillons.Where(n => n.Nom != null).ToList(),
                "Idnatureechantillon", "Nom", model.Idnatureechantillon);
            ViewData["Preleveurid"] = new SelectList(
                _context.Preleveurs.Select(p => new { p.Preleveurid, NomComplet = p.Nom + " " + p.Prenom }),
                "Preleveurid", "NomComplet", model.Preleveurid);
            return View(model);
        }

        // POST: Prelevement/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ProlabWeb.ViewModels.PrelevementEditVM model)
        {
            if (id != model.Prelevementid)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
                ViewData["Detaildemandeid"] = new SelectList(
                    _context.Detaildemandes
                        .Include(d => d.Entetedemande)
                        .ThenInclude(e => e.Patient)
                        .Where(d => d.Entetedemande != null && d.Entetedemande.Patient != null)
                        .Select(d => new {
                            d.Detaildemandeid,
                            NomPatient = d.Entetedemande.Patient.Nom + " " + d.Entetedemande.Patient.Prenom
                        }),
                    "Detaildemandeid", "NomPatient", model.Detaildemandeid);
                ViewData["Idnatureechantillon"] = new SelectList(
                    _context.Natureechantillons.Where(n => n.Nom != null).ToList(),
                    "Idnatureechantillon", "Nom", model.Idnatureechantillon);
                ViewData["Preleveurid"] = new SelectList(
                    _context.Preleveurs.Select(p => new { p.Preleveurid, NomComplet = p.Nom + " " + p.Prenom }),
                    "Preleveurid", "NomComplet", model.Preleveurid);
                return View(model);
            }
            var prelevement = await _context.Prelevements.FindAsync(id);
            if (prelevement == null)
            {
                return NotFound();
            }
            prelevement.Detaildemandeid = model.Detaildemandeid;
            prelevement.Preleveurid = model.Preleveurid;
            prelevement.Idnatureechantillon = model.Idnatureechantillon;
            prelevement.Dateprelevement = model.Date;
            prelevement.Statut = model.Statut;
            prelevement.Datereception = model.Datereception;
                try
                {
                    _context.Update(prelevement);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                if (!PrelevementExists(model.Prelevementid))
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

        // GET: Prelevement/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prelevement = await _context.Prelevements
                .Include(p => p.Detaildemande)
                .Include(p => p.IdnatureechantillonNavigation)
                .Include(p => p.Preleveur)
                .FirstOrDefaultAsync(m => m.Prelevementid == id);
            if (prelevement == null)
            {
                return NotFound();
            }

            return View(prelevement);
        }

        // POST: Prelevement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var prelevement = await _context.Prelevements.FindAsync(id);
            if (prelevement != null)
            {
                _context.Prelevements.Remove(prelevement);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PrelevementExists(Guid id)
        {
            return _context.Prelevements.Any(e => e.Prelevementid == id);
        }
    }
}
