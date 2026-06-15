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
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace ProlabWeb.Controllers
{
    public class UtilisateurManyLaboratoireController : Controller
    {
        private readonly ProlabwebContext _context;
        private readonly UserManager<ProlabIdentityUser> _usermanager;

        public UtilisateurManyLaboratoireController(ProlabwebContext context, UserManager<ProlabIdentityUser> usermanager)
        {
            _context = context;
            _usermanager = usermanager;
        }

        // GET: Index
        public async Task<IActionResult> Index()
        {
            var users = _usermanager
                .GetUsersInRoleAsync(EnumRoles.Biologiste.ToString())
                .GetAwaiter()
                .GetResult()
                .Select(x => x.Id)
                .ToList();

            var list = await _context.Utilisateurlaboratoires
                .Include(ul => ul.Utilisateur)
                .Include(ul => ul.IdlaboratoireNavigation)
                .Where(ul => users.Contains(ul.Utilisateur.Userid))
                .GroupBy(ul => ul.Utilisateurid)
                .Select(group => new UtilisateurAvecLaboratoiresVM
                {
                    Utilisateur = group.First().Utilisateur,
                    Laboratoires = group.Select(x => x.IdlaboratoireNavigation).Distinct().ToList()
                })
                .ToListAsync();

            return View(list);
        }

        // GET: Details
        public async Task<IActionResult> Details(Guid id)
        {
            var item = await _context.Utilisateurlaboratoires
                .Include(ul => ul.Utilisateur)
                .Include(ul => ul.IdlaboratoireNavigation)
                .Where(ul => ul.Utilisateurid == id)
                .GroupBy(ul => ul.Utilisateurid)
                .Select(group => new UtilisateurAvecLaboratoiresVM
                {
                    Utilisateur = group.First().Utilisateur,
                    Laboratoires = group.Select(x => x.IdlaboratoireNavigation).Distinct().ToList()
                })
                .FirstOrDefaultAsync();

            if (item == null)
                return NotFound();

            return View(item);
        }

        // GET: Create
        public IActionResult Create()
        {
            var vm = new UtilisateurManyLaboratoireCreateVM();

            var users = _usermanager
                .GetUsersInRoleAsync(EnumRoles.Biologiste.ToString())
                .GetAwaiter()
                .GetResult()
                .Select(x => x.Id)
                .ToList();

            var utilisateursLabo = users.Any()
                ? _context.Utilisateurs.AsNoTracking()
                    .Where(x => users.Contains(x.Userid))
                    .ToList()
                : new List<Utilisateur>();
            
            ViewBag.Utilisateurid = new SelectList(utilisateursLabo, "Utilisateurid", "Nom");
            ViewBag.Idlaboratoire = new SelectList(_context.Loboratoires, "Idlaboratoire", "Nom");

            return View(vm);
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UtilisateurManyLaboratoireCreateVM vm)
        {
            if (ModelState.IsValid)
            {
                foreach (var idLaboratoire in vm.Idslaboratoire)
                {
                    var newItem = new Utilisateurlaboratoire
                    {
                        Utilisateurlaboratoireid = Guid.NewGuid(),
                        Utilisateurid = vm.Utilisateurid,
                        Idlaboratoire = idLaboratoire
                    };
                    _context.Add(newItem);
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var users = _usermanager
                .GetUsersInRoleAsync(EnumRoles.Biologiste.ToString())
                .GetAwaiter()
                .GetResult()
                .Select(x => x.Id)
                .ToList();

            var utilisateursLabo = users.Any()
                ? _context.Utilisateurs.AsNoTracking()
                    .Where(x => users.Contains(x.Userid))
                    .ToList()
                : new List<Utilisateur>();

            ViewBag.Utilisateurid = new SelectList(utilisateursLabo, "Utilisateurid", "Nom");
            ViewBag.Idlaboratoire = new SelectList(_context.Loboratoires, "Idlaboratoire", "Nom");

            return View(vm);
        }

        // GET: Edit
        public async Task<IActionResult> Edit(Guid id)
        {
            var userLab = await _context.Utilisateurlaboratoires
                .Where(x => x.Utilisateurid == id)
                .ToListAsync();

            if (!userLab.Any())
                return NotFound();

            var vm = new UtilisateurManyLaboratoireEditVM
            {
                Utilisateurid = id,
                Idslaboratoire = userLab.Select(x => x.Idlaboratoire).ToArray()
            };

            var users = _usermanager
                .GetUsersInRoleAsync(EnumRoles.Biologiste.ToString())
                .GetAwaiter()
                .GetResult()
                .Select(x => x.Id)
                .ToList();

            var utilisateursLabo = users.Any()
                ? _context.Utilisateurs.AsNoTracking()
                    .Where(x => users.Contains(x.Userid))
                    .ToList()
                : new List<Utilisateur>();

            ViewBag.Utilisateurid = new SelectList(utilisateursLabo, "Utilisateurid", "Nom");

            var laboratoires = new SelectList(_context.Loboratoires, "Idlaboratoire", "Nom").ToList();
            laboratoires.Insert(0, new SelectListItem("---", string.Empty));

            var selectedLaboratoires = await _context.Utilisateurlaboratoires.AsNoTracking()
                .Include(x => x.Utilisateur)
                .Include(x => x.IdlaboratoireNavigation)
                .Where(x => x.Utilisateurid == id)
                .Select(x => x.Idlaboratoire.ToString())
                .ToListAsync();

            foreach (var item in laboratoires)
            {
                if (selectedLaboratoires.Contains(item.Value))
                    item.Selected = true;
            }

            ViewBag.Idlaboratoire = laboratoires;

            return View(vm);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UtilisateurManyLaboratoireEditVM vm)
        {
            if (ModelState.IsValid)
            {
                var utilisateurId = vm.Utilisateurid;
                var nouveauxIds = vm.Idslaboratoire?.Distinct().ToList() ?? new List<int>();

                // 1. Récupérer les anciennes associations de l'utilisateur
                var anciens = await _context.Utilisateurlaboratoires
                    .Where(x => x.Utilisateurid == utilisateurId)
                    .ToListAsync();

                var anciensIds = anciens.Select(x => x.Idlaboratoire).ToList();

                // 2. Supprimer ceux qui ne sont plus dans la nouvelle liste
                var aSupprimer = anciens.Where(x => !nouveauxIds.Contains(x.Idlaboratoire)).ToList();
                if (aSupprimer.Any())
                {
                    _context.Utilisateurlaboratoires.RemoveRange(aSupprimer);
                }

                // 3. Ajouter ceux qui sont nouveaux
                var aAjouter = nouveauxIds.Except(anciensIds).ToList();
                foreach (var idLaboratoire in aAjouter)
                {
                    _context.Utilisateurlaboratoires.Add(new Utilisateurlaboratoire
                    {
                        Utilisateurlaboratoireid = Guid.NewGuid(),
                        Utilisateurid = utilisateurId,
                        Idlaboratoire = idLaboratoire
                    });
                }

                // 4. Mettre à jour ceux qui existent déjà (si nécessaire)
                var aMettreAJour = anciens.Where(x => nouveauxIds.Contains(x.Idlaboratoire)).ToList();

                foreach (var record in aMettreAJour)
                {
                    // ➕ Ajoute ici les mises à jour spécifiques si le modèle évolue
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var users = _usermanager
                .GetUsersInRoleAsync(EnumRoles.Biologiste.ToString())
                .GetAwaiter()
                .GetResult()
                .Select(x => x.Id)
                .ToList();

            var utilisateursLabo = users.Any()
                ? _context.Utilisateurs.AsNoTracking()
                    .Where(x => users.Contains(x.Userid))
                    .ToList()
                : new List<Utilisateur>();

            ViewBag.Utilisateurid = new SelectList(utilisateursLabo, "Utilisateurid", "Nom");

            var laboratoires = new SelectList(_context.Loboratoires, "Idlaboratoire", "Nom").ToList();
            laboratoires.Insert(0, new SelectListItem("---", string.Empty));

            var selectedLaboratoires = vm.Idslaboratoire.Select(n => n.ToString()).ToArray();

            foreach (var item in laboratoires)
            {
                if (selectedLaboratoires.Contains(item.Value))
                    item.Selected = true;
            }

            ViewBag.Idlaboratoire = laboratoires;

            return View(vm);
        }

        // GET: Delete
        public async Task<IActionResult> Delete(Guid id)
        {
            var item = await _context.Utilisateurlaboratoires
                .Include(ul => ul.Utilisateur)
                .Include(ul => ul.IdlaboratoireNavigation)
                .Where(ul => ul.Utilisateurid == id)
                .GroupBy(ul => ul.Utilisateurid)
                .Select(group => new UtilisateurAvecLaboratoiresVM
                {
                    Utilisateur = group.First().Utilisateur,
                    Laboratoires = group.Select(x => x.IdlaboratoireNavigation).Distinct().ToList()
                })
                .FirstOrDefaultAsync();

            if (item == null)
                return NotFound();

            return View(item);
        }

        // POST: DeleteConfirmed
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var associations = await _context.Utilisateurlaboratoires
                .Where(x => x.Utilisateurid == id)
                .ToListAsync();

            if (associations.Any())
            {
                _context.Utilisateurlaboratoires.RemoveRange(associations);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
