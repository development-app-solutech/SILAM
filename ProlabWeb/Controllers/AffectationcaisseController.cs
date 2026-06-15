using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProlabWeb.Data;
using ProlabWeb.Db;
using ProlabWeb.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProlabWeb.Controllers
{
    [AllowAnonymous]
    public class AffectationcaisseController : Controller
    {
        private readonly ProlabwebContext _context;
        private readonly UserManager<ProlabIdentityUser> _usermanager;

        public AffectationcaisseController(ProlabwebContext context, UserManager<ProlabIdentityUser> usermanager)
        {
            _context = context;
            _usermanager = usermanager;
        }

        // GET: Affectationcaisse
        public async Task<IActionResult> Index()
        {
            var affectations = await _context.Affectationcaisses
                .Include(a => a.Caisse)
                .ThenInclude(c => c.CodesiteNavigation)
                .Include(a => a.Utilisateur)
                .ToListAsync();
            return View(affectations);
        }

        // GET: Affectationcaisse/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var affectationcaisse = await _context.Affectationcaisses
                .Include(a => a.Caisse)
                .Include(a => a.Utilisateur)
                .FirstOrDefaultAsync(m => m.Affectationcaisseid == id);
            if (affectationcaisse == null)
            {
                return NotFound();
            }

            return View(affectationcaisse);
        }

        // GET: Affectationcaisse/Create
        public IActionResult Create()
        {
            ViewData["Caisseid"] = new SelectList(_context.Caisses, "Caisseid", "Nom");
            // Récupérer les utilisateurs ayant le profil 'caissier'
            //var caissierProfilNom = ProlabWeb.EnumProfil.caissier.ToString();
            //var utilisateursCaissiers = _context.Utilisateurprofils
            //    .Where(up => up.Profil.Nom == caissierProfilNom)
            //    .Select(up => up.Utilisateur)
            //    .Distinct()
            //    .ToList();

            var users = _usermanager
                .GetUsersInRoleAsync(EnumRoles.Caisse.ToString())
                .GetAwaiter()
                .GetResult()
                .Select(x => x.Id)
                .ToList();

            var utilisateursCaissiers = users.Any() 
                ? _context.Utilisateurs.AsNoTracking()
                    .Where(x => users.Contains(x.Userid))
                    .ToList()
                : new List<Utilisateur>();
            
            ViewData["Utilisateurid"] = new SelectList(utilisateursCaissiers, "Utilisateurid", "Nom");
            
            // Créer un modèle avec la date par défaut
            var model = new AffectationcaisseCreateVM
            {
                Date = DateTime.Today
            };
            
            return View(model);
        }

        // POST: Affectationcaisse/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AffectationcaisseCreateVM model)
        {
            // Vérification unicité caisse et caissier
            bool caisseDejaAffectee = _context.Affectationcaisses.Any(a => a.Caisseid == model.Caisseid);
            bool caissierDejaAffecte = _context.Affectationcaisses.Any(a => a.Utilisateurid == model.Utilisateurid);
            if (caisseDejaAffectee)
            {
                ModelState.AddModelError(string.Empty, "Cette caisse est déjà affectée à un caissier.");
            }
            if (caissierDejaAffecte)
            {
                ModelState.AddModelError(string.Empty, "Ce caissier est déjà affecté à une caisse.");
            }
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return Json(new { success = false, errors });
                }
                var affectationcaisse = new Affectationcaisse
                {
                    Affectationcaisseid = Guid.NewGuid(),
                    Caisseid = model.Caisseid,
                    Utilisateurid = model.Utilisateurid,
                    // Combiner la date saisie avec l'heure actuelle
                    Date = model.Date.Date.Add(DateTime.Now.TimeOfDay)
                };
                _context.Add(affectationcaisse);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            if (ModelState.IsValid)
            {
                var affectationcaisse = new Affectationcaisse
                {
                    Affectationcaisseid = Guid.NewGuid(),
                    Caisseid = model.Caisseid,
                    Utilisateurid = model.Utilisateurid,
                    // Combiner la date saisie avec l'heure actuelle
                    Date = model.Date.Date.Add(DateTime.Now.TimeOfDay)
                };
                _context.Add(affectationcaisse);
                await _context.SaveChangesAsync();
                TempData["AffectationcaisseCree"] = "Affectation créée avec succès !";
                return RedirectToAction(nameof(Index));
            }
            // Si ce n'est pas une requête AJAX, stocker les erreurs dans TempData pour affichage
            var errorList = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            if (errorList.Any())
            {
                TempData["ValidationErrors"] = errorList;
            }
            
            ViewData["Caisseid"] = new SelectList(_context.Caisses, "Caisseid", "Nom", model.Caisseid);
            // Récupérer les utilisateurs ayant le profil 'caissier' pour le POST aussi
            //var caissierProfilNom = ProlabWeb.EnumProfil.caissier.ToString();
            //var utilisateursCaissiers = _context.Utilisateurprofils
            //    .Where(up => up.Profil.Nom == caissierProfilNom)
            //    .Select(up => up.Utilisateur)
            //    .Distinct()
            //    .ToList();
            var users = _usermanager
                .GetUsersInRoleAsync(EnumRoles.Caisse.ToString())
                .GetAwaiter()
                .GetResult()
                .Select(x => x.Id)
                .ToList();

            var utilisateursCaissiers = users.Any()
                ? _context.Utilisateurs.AsNoTracking()
                    .Where(x => users.Contains(x.Userid))
                    .ToList()
                : new List<Utilisateur>();
            ViewData["Utilisateurid"] = new SelectList(utilisateursCaissiers, "Utilisateurid", "Nom", model.Utilisateurid);
            return View(model);
        }

        // GET: Affectationcaisse/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var affectationcaisse = await _context.Affectationcaisses.FindAsync(id);
            if (affectationcaisse == null)
            {
                return NotFound();
            }
            var vm = new AffectationcaisseEditVM
            {
                Affectationcaisseid = affectationcaisse.Affectationcaisseid,
                Caisseid = affectationcaisse.Caisseid,
                Utilisateurid = affectationcaisse.Utilisateurid,
                Date = affectationcaisse.Date
            };
            ViewData["Caisseid"] = new SelectList(_context.Caisses, "Caisseid", "Nom", affectationcaisse.Caisseid);
            //var caissierProfilNom = ProlabWeb.EnumProfil.caissier.ToString();
            //var utilisateursCaissiers = _context.Utilisateurprofils
            //    .Where(up => up.Profil.Nom == caissierProfilNom)
            //    .Select(up => up.Utilisateur)
            //    .Distinct()
            //    .ToList();
            var users = _usermanager
                .GetUsersInRoleAsync(EnumRoles.Caisse.ToString())
                .GetAwaiter()
                .GetResult()
                .Select(x => x.Id)
                .ToList();

            var utilisateursCaissiers = users.Any()
                ? _context.Utilisateurs.AsNoTracking()
                    .Where(x => users.Contains(x.Userid))
                    .ToList()
                : new List<Utilisateur>();
            ViewData["Utilisateurid"] = new SelectList(utilisateursCaissiers, "Utilisateurid", "Nom", affectationcaisse.Utilisateurid);
            return View(vm);
        }

        // POST: Affectationcaisse/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, AffectationcaisseEditVM model)
        {
            // Vérification unicité caisse et caissier (hors l'enregistrement en cours)
            bool caisseDejaAffectee = _context.Affectationcaisses.Any(a => a.Caisseid == model.Caisseid && a.Affectationcaisseid != id);
            bool caissierDejaAffecte = _context.Affectationcaisses.Any(a => a.Utilisateurid == model.Utilisateurid && a.Affectationcaisseid != id);
            if (caisseDejaAffectee)
            {
                ModelState.AddModelError(string.Empty, "Cette caisse est déjà affectée à un caissier.");
            }
            if (caissierDejaAffecte)
            {
                ModelState.AddModelError(string.Empty, "Ce caissier est déjà affecté à une caisse.");
            }
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return Json(new { success = false, errors });
                }
                var affectationcaisse = await _context.Affectationcaisses.FindAsync(id);
                if (affectationcaisse == null)
                {
                    return Json(new { success = false, errors = new[] { "Affectation non trouvée." } });
                }
                affectationcaisse.Caisseid = model.Caisseid;
                affectationcaisse.Utilisateurid = model.Utilisateurid;
                affectationcaisse.Date = model.Date.Date.Add(DateTime.Now.TimeOfDay);
                _context.Update(affectationcaisse);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            if (ModelState.IsValid)
            {
                var affectationcaisse = await _context.Affectationcaisses.FindAsync(id);
                if (affectationcaisse == null)
                {
                    return NotFound();
                }
                affectationcaisse.Caisseid = model.Caisseid;
                affectationcaisse.Utilisateurid = model.Utilisateurid;
                affectationcaisse.Date = model.Date.Date.Add(DateTime.Now.TimeOfDay);
                _context.Update(affectationcaisse);
                await _context.SaveChangesAsync();
                TempData["AffectationcaisseModifie"] = "Affectation caisse modifiée avec succès !";
                return RedirectToAction(nameof(Index));
            }
            // Si ce n'est pas une requête AJAX, stocker les erreurs dans TempData pour affichage
            var errorList = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            if (errorList.Any())
            {
                TempData["ValidationErrors"] = errorList;
            }
            
            ViewData["Caisseid"] = new SelectList(_context.Caisses, "Caisseid", "Nom", model.Caisseid);
            //var caissierProfilNom = ProlabWeb.EnumProfil.caissier.ToString();
            //var utilisateursCaissiers = _context.Utilisateurprofils
            //    .Where(up => up.Profil.Nom == caissierProfilNom)
            //    .Select(up => up.Utilisateur)
            //    .Distinct()
            //    .ToList();
            var users = _usermanager
                .GetUsersInRoleAsync(EnumRoles.Caisse.ToString())
                .GetAwaiter()
                .GetResult()
                .Select(x => x.Id)
                .ToList();

            var utilisateursCaissiers = users.Any()
                ? _context.Utilisateurs.AsNoTracking()
                    .Where(x => users.Contains(x.Userid))
                    .ToList()
                : new List<Utilisateur>();
            ViewData["Utilisateurid"] = new SelectList(utilisateursCaissiers, "Utilisateurid", "Nom", model.Utilisateurid);
            return View(model);
        }

        // GET: Affectationcaisse/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var affectationcaisse = await _context.Affectationcaisses
                .Include(a => a.Caisse)
                .Include(a => a.Utilisateur)
                .FirstOrDefaultAsync(m => m.Affectationcaisseid == id);
            if (affectationcaisse == null)
            {
                return NotFound();
            }

            return View(affectationcaisse);
        }

        // POST: Affectationcaisse/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var affectationcaisse = await _context.Affectationcaisses.FindAsync(id);
            if (affectationcaisse != null)
            {
                _context.Affectationcaisses.Remove(affectationcaisse);
            }

            await _context.SaveChangesAsync();
            TempData["AffectationcaisseSupprime"] = "Affectation caisse supprimée avec succès !";
            return RedirectToAction(nameof(Index));
        }

        // Suppression AJAX (comme dans Analyse)
        [HttpPost, ActionName("DeleteAjax")]
        public async Task<IActionResult> DeleteAjaxAsync(Guid id)
        {
            JsonResponseViewModel resultat = new JsonResponseViewModel();
            
            if (id != Guid.Empty)
            {
                var affectation = await _context.Affectationcaisses.AsNoTracking()
                    .Where(x => x.Affectationcaisseid == id)
                    .FirstOrDefaultAsync();
                    
                if (affectation != null)
                {
                    _context.Remove(affectation);
                    await _context.SaveChangesAsync();
                    
                    resultat.success = true;
                }
            }
            
            return Ok(resultat);
        }

        private bool AffectationcaisseExists(Guid id)
        {
            return _context.Affectationcaisses.Any(e => e.Affectationcaisseid == id);
        }
    }
}
