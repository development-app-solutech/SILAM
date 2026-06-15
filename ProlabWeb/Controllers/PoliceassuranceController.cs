using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProlabWeb.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProlabWeb.Controllers
{
    [AllowAnonymous]
    public class PoliceassuranceController : Controller
    {
        private readonly ProlabwebContext _context;

        public PoliceassuranceController(ProlabwebContext context)
        {
            _context = context;
        }

        // GET: Policeassurance
        public async Task<IActionResult> Index()
        {
            var prolabwebContext = _context.Policeassurances.Include(p => p.CodeassuranceNavigation);
            return View(await prolabwebContext.ToListAsync());
        }

        // GET: Policeassurance/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var policeassurance = await _context.Policeassurances
                .Include(p => p.CodeassuranceNavigation)
                .FirstOrDefaultAsync(m => m.Policeassuranceid == id);
            if (policeassurance == null)
            {
                return NotFound();
            }

            return View(policeassurance);
        }

        // GET: Policeassurance/Create
        public IActionResult Create()
        {
            ViewData["Codeassurance"] = new SelectList(_context.Assurances, "Codeassurance", "Nom");
            return View(new ProlabWeb.ViewModels.PoliceassuranceCreateVM());
        }

        // POST: Policeassurance/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProlabWeb.ViewModels.PoliceassuranceCreateVM model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Codeassurance"] = new SelectList(_context.Assurances, "Codeassurance", "Nom", model.Codeassurance);
                return View(model);
            }
            var policeassurance = new Policeassurance
            {
                Policeassuranceid = Guid.NewGuid(),
                Codeassurance = model.Codeassurance,
                Libelle = model.Libelle,
                Taux = model.Taux
            };
            _context.Add(policeassurance);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Policeassurance/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var policeassurance = await _context.Policeassurances.FindAsync(id);
            if (policeassurance == null)
            {
                return NotFound();
            }
            var model = new ProlabWeb.ViewModels.PoliceassuranceEditVM
            {
                Policeassuranceid = policeassurance.Policeassuranceid,
                Codeassurance = policeassurance.Codeassurance,
                Libelle = policeassurance.Libelle,
                Taux = policeassurance.Taux
            };
            ViewData["Codeassurance"] = new SelectList(_context.Assurances, "Codeassurance", "Nom", model.Codeassurance);
            return View(model);
        }

        // POST: Policeassurance/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ProlabWeb.ViewModels.PoliceassuranceEditVM model)
        {
            if (id != model.Policeassuranceid)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
                ViewData["Codeassurance"] = new SelectList(_context.Assurances, "Codeassurance", "Nom", model.Codeassurance);
                return View(model);
            }
            var policeassurance = await _context.Policeassurances.FindAsync(id);
            if (policeassurance == null)
            {
                return NotFound();
            }
            policeassurance.Codeassurance = model.Codeassurance;
            policeassurance.Libelle = model.Libelle;
            policeassurance.Taux = model.Taux;
            try
            {
                _context.Update(policeassurance);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PoliceassuranceExists(model.Policeassuranceid))
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

        // GET: Policeassurance/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var policeassurance = await _context.Policeassurances
                .Include(p => p.CodeassuranceNavigation)
                .FirstOrDefaultAsync(m => m.Policeassuranceid == id);
            if (policeassurance == null)
            {
                return NotFound();
            }

            return View(policeassurance);
        }

        // POST: Policeassurance/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var policeassurance = await _context.Policeassurances.FindAsync(id);
            if (policeassurance != null)
            {
                _context.Policeassurances.Remove(policeassurance);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PoliceassuranceExists(Guid id)
        {
            return _context.Policeassurances.Any(e => e.Policeassuranceid == id);
        }

        #region handlers

        [HttpGet, ActionName("GetPoliceassuranceByAssurance")]
        public async Task<IActionResult> GetPoliceassuranceByAssuranceAsync(string id)
        {
            JsonResponseViewModel resultat = new JsonResponseViewModel();

            if (!string.IsNullOrWhiteSpace(id))
            {
                // Get the insurance  with its type to determine if it's public or private
                var assurance = await _context.Assurances.AsNoTracking()
                    .Include(a => a.CodetypeassuranceNavigation)
                    .FirstOrDefaultAsync(a => a.Codeassurance == id);

                if (assurance == null)
                {
                    resultat.success = false;
                    resultat.data = "Assurance not found";
                    return Ok(resultat);
                }

                var policeassurances = await _context.Policeassurances.AsNoTracking()
                    .Where(x => x.Codeassurance == id)
                    .Select(x => new SelectListItem
                    {
                        Value = x.Policeassuranceid.ToString(),
                        Text = string.Join(", ", new List<object> {
                            x.Libelle,
                            x.Taux
                        })
                    })
                    .ToListAsync();

                if (policeassurances != null)
                {
                    // Add metadata about the assurance type
                    string dataJson = JsonConvert.SerializeObject(new
                    {
                        policies = policeassurances,
                        isPublic = assurance.CodetypeassuranceNavigation.Nom.ToLower().Contains("public"),
                        assuranceType = assurance.CodetypeassuranceNavigation.Nom
                    }, Formatting.Indented, new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });

                    resultat.success = true;
                    resultat.data = dataJson;
                }
            }

            return Ok(resultat);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNewPolicAsync(string codeassurance, string libelle, decimal taux)
        {
            JsonResponseViewModel result = new JsonResponseViewModel();

            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(codeassurance) || string.IsNullOrWhiteSpace(libelle) || taux < 0)
                {
                    result.success = false;
                    result.data = "Données invalides";
                    return Ok(result);
                }

                // Check if assurance exists
                var assurance = await _context.Assurances.AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Codeassurance == codeassurance);

                if (assurance == null)
                {
                    result.success = false;
                    result.data = "Assurance introuvable";
                    return Ok(result);
                }

                // Check for duplicate
                var existingPolicy = await _context.Policeassurances.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Codeassurance == codeassurance && p.Libelle == libelle);

                if (existingPolicy != null)
                {
                    result.success = false;
                    result.data = "Cette police existe déjà";
                    return Ok(result);
                }

                // Create new police
                var newPolicy = new Policeassurance
                {
                    Policeassuranceid = Guid.NewGuid(),
                    Codeassurance = codeassurance,
                    Libelle = libelle,
                    Taux = taux
                };

                _context.Add(newPolicy);
                await _context.SaveChangesAsync();

                result.success = true;
                result.data = JsonConvert.SerializeObject(new
                {
                    policeId = newPolicy.Policeassuranceid.ToString(),
                    policeLibelle = $"{newPolicy.Libelle}, {newPolicy.Taux}"
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                result.success = false;
                result.data = $"Erreur: {ex.Message}";
                return Ok(result);
            }
        }

        #endregion
    }
}
