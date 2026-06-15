using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProlabWeb.Db;
using ProlabWeb.ViewModels;
using Newtonsoft.Json;

namespace ProlabWeb.Controllers
{
    public class DetailresultatController : Controller
    {
        private readonly ProlabwebContext _context;

        public DetailresultatController(ProlabwebContext context)
        {
            _context = context;
        }

        // GET: Detailresultat
        public async Task<IActionResult> Index()
        {
            var prolabwebContext = _context.Detailresultats
                .Include(d => d.Enteteresultat)
                .Include(d => d.Parametre);
            return View(await prolabwebContext.ToListAsync());
        }

        // GET: Detailresultat/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Detailresultats == null)
            {
                return NotFound();
            }

            var detailresultat = await _context.Detailresultats
                .Include(d => d.Enteteresultat)
                .Include(d => d.Parametre)
                .FirstOrDefaultAsync(m => m.Detailresultatid == id);
            if (detailresultat == null)
            {
                return NotFound();
            }

            return View(detailresultat);
        }

        // GET: Detailresultat/Create
        public IActionResult Create()
        {
            ViewData["Enteteresultatid"] = new SelectList(_context.Enteteresultats, "Enteteresultatid", "Enteteresultatid");
            ViewData["Parametreid"] = new SelectList(_context.Parametres, "Parametreid", "Parametreid");
            return View();
        }

        // POST: Detailresultat/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Detailresultatid,Enteteresultatid,Parametreid,Date,Commentaire,Databuilder,Resultat,Resultatsi")] Detailresultat detailresultat)
        {
            if (ModelState.IsValid)
            {
                detailresultat.Detailresultatid = Guid.NewGuid();
                _context.Add(detailresultat);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Enteteresultatid"] = new SelectList(_context.Enteteresultats, "Enteteresultatid", "Enteteresultatid", detailresultat.Enteteresultatid);
            ViewData["Parametreid"] = new SelectList(_context.Parametres, "Parametreid", "Parametreid", detailresultat.Parametreid);
            return View(detailresultat);
        }

        // GET: Detailresultat/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Detailresultats == null)
            {
                return NotFound();
            }

            var detailresultat = await _context.Detailresultats.FindAsync(id);
            if (detailresultat == null)
            {
                return NotFound();
            }
            ViewData["Enteteresultatid"] = new SelectList(_context.Enteteresultats, "Enteteresultatid", "Enteteresultatid", detailresultat.Enteteresultatid);
            ViewData["Parametreid"] = new SelectList(_context.Parametres, "Parametreid", "Parametreid", detailresultat.Parametreid);
            return View(detailresultat);
        }

        // POST: Detailresultat/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Detailresultatid,Enteteresultatid,Parametreid,Date,Commentaire,Databuilder,Resultat,Resultatsi")] Detailresultat detailresultat)
        {
            if (id != detailresultat.Detailresultatid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(detailresultat);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DetailresultatExists(detailresultat.Detailresultatid))
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
            ViewData["Enteteresultatid"] = new SelectList(_context.Enteteresultats, "Enteteresultatid", "Enteteresultatid", detailresultat.Enteteresultatid);
            ViewData["Parametreid"] = new SelectList(_context.Parametres, "Parametreid", "Parametreid", detailresultat.Parametreid);
            return View(detailresultat);
        }

        // GET: Detailresultat/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Detailresultats == null)
            {
                return NotFound();
            }

            var detailresultat = await _context.Detailresultats
                .Include(d => d.Enteteresultat)
                .Include(d => d.Parametre)
                .FirstOrDefaultAsync(m => m.Detailresultatid == id);
            if (detailresultat == null)
            {
                return NotFound();
            }

            return View(detailresultat);
        }

        // POST: Detailresultat/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Detailresultats == null)
            {
                return Problem("Entity set 'ProlabwebContext.Detailresultats'  is null.");
            }
            var detailresultat = await _context.Detailresultats.FindAsync(id);
            if (detailresultat != null)
            {
                _context.Detailresultats.Remove(detailresultat);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetParamResultatByEnteteResultatAndAnalyse(Guid enteteId, Guid analyseId)
        {
            var details = await _context.Detailresultats
                .Include(d => d.Parametre)
                .Include(d => d.Parametre.CodeuniteNavigation)
                .Include(d => d.Parametre.CodeunitesiNavigation)
                .Where(d => d.Enteteresultatid == enteteId && d.Parametre.Idanalyse == analyseId)
                .ToListAsync();

            var result = details.Select(d => new ParametreResultatVM
            {
                Parametreid = d.Parametreid,
                Nom = d.Parametre?.Nom ?? string.Empty,
                Code = d.Parametre?.Code ?? string.Empty,
                Unite = d.Parametre?.CodeuniteNavigation?.Name ?? string.Empty,
                Resultat = d.Resultat?.Replace(",", "."),
                UniteSI = d.Parametre?.CodeunitesiNavigation.Name ?? string.Empty,
                Resultatsi = d.Resultatsi?.Replace(",", "."),
                Commentaire = d.Commentaire // ✅ Ajout de la propriété manquante
            }).ToList();

            var json = JsonConvert.SerializeObject(result);
            return Json(new { success = true, data = json });
        }

        private bool DetailresultatExists(Guid id)
        {
          return (_context.Detailresultats?.Any(e => e.Detailresultatid == id)).GetValueOrDefault();
        }
    }
} 