using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProlabWeb.Db;
using ProlabWeb.ViewModels;

namespace ProlabWeb.Controllers
{
    public class MaterielController : Controller
    {
        private readonly ProlabwebContext _context;
        private readonly IMapper _mapper;

        public MaterielController(ProlabwebContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: Materiels
        public async Task<IActionResult> Index()
        {
            return View(await _context.Materiels.ToListAsync());
        }

        // GET: Materiels/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var materiel = await _context.Materiels
                .FirstOrDefaultAsync(m => m.Materielid == id);
            if (materiel == null)
            {
                return NotFound();
            }

            return View(materiel);
        }

        // GET: Materiels/Create
        public IActionResult Create()
        {
            ViewBag.Categoriematerielid = new SelectList(_context.Categoriemateriels, "Categoriematerielid", "Nom");
            ViewBag.Idlaboratoire = new SelectList(_context.Loboratoires, "Idlaboratoire", "Nom");
            return View();
        }

        // POST: Materiels/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MaterielCreateVM model)
        {
            if (ModelState.IsValid)
            {
                var materiel = _mapper.Map<Materiel>(model);
                materiel.Materielid = Guid.NewGuid();
                _context.Add(materiel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categoriematerielid = new SelectList(_context.Categoriemateriels, "Categoriematerielid", "Nom", model.Categoriematerielid);
            ViewBag.Idlaboratoire = new SelectList(_context.Loboratoires, "Idlaboratoire", "Nom", model.Idlaboratoire);
            return View(model);
        }

        // GET: Materiels/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var materiel = await _context.Materiels.FindAsync(id);
            if (materiel == null)
            {
                return NotFound();
            }
            var model = new MaterielEditVM
            {
                Materielid = materiel.Materielid,
                Nom = materiel.Nom,
                Description = materiel.Description,
                Categoriematerielid = materiel.Categoriematerielid,
                Idlaboratoire = materiel.Idlaboratoire,
                Prix = materiel.Prix,
                Quantitemin = materiel.Quantitemin,
                Dateperemption = materiel.Dateperemption,
                Zonestockage = materiel.Zonestockage,
                Conditionstockage = materiel.Conditionstockage,
                Codebarre = materiel.Codebarre
            };
            ViewBag.Categoriematerielid = new SelectList(_context.Categoriemateriels, "Categoriematerielid", "Nom", model.Categoriematerielid);
            ViewBag.Idlaboratoire = new SelectList(_context.Loboratoires, "Idlaboratoire", "Nom", model.Idlaboratoire);
            return View(model);
        }

        // POST: Materiels/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, MaterielEditVM model)
        {
            if (id != model.Materielid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var materiel = await _context.Materiels.FindAsync(id);
                    if (materiel == null)
                    {
                        return NotFound();
                    }
                    materiel.Nom = model.Nom;
                    materiel.Description = model.Description;
                    materiel.Categoriematerielid = model.Categoriematerielid;
                    materiel.Idlaboratoire = model.Idlaboratoire;
                    materiel.Prix = model.Prix;
                    materiel.Quantitemin = model.Quantitemin;
                    materiel.Dateperemption = model.Dateperemption;
                    materiel.Zonestockage = model.Zonestockage;
                    materiel.Conditionstockage = model.Conditionstockage;
                    materiel.Codebarre = model.Codebarre;

                    _context.Update(materiel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MaterielExists(model.Materielid))
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
            ViewBag.Categoriematerielid = new SelectList(_context.Categoriemateriels, "Categoriematerielid", "Nom", model.Categoriematerielid);
            ViewBag.Idlaboratoire = new SelectList(_context.Loboratoires, "Idlaboratoire", "Nom", model.Idlaboratoire);
            return View(model);
        }

        // GET: Materiels/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var materiel = await _context.Materiels
                .FirstOrDefaultAsync(m => m.Materielid == id);
            if (materiel == null)
            {
                return NotFound();
            }

            return View(materiel);
        }

        // POST: Materiels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var materiel = await _context.Materiels.FindAsync(id);
            if (materiel != null)
            {
                _context.Materiels.Remove(materiel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MaterielExists(Guid id)
        {
            return _context.Materiels.Any(e => e.Materielid == id);
        }
    }
}
