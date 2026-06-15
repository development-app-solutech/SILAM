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
    public class UtilisateurlaboratoireController : Controller
    {
        private readonly ProlabwebContext _context;
        private readonly UserManager<ProlabIdentityUser> _usermanager;

        public UtilisateurlaboratoireController(ProlabwebContext context, UserManager<ProlabIdentityUser> usermanager)
        {
            _context = context;
            _usermanager = usermanager;
        }

        // GET: Utilisateurlaboratoire
        public async Task<IActionResult> Index()
        {
            var list = await _context.Utilisateurlaboratoires
                .Include(u => u.Utilisateur)
                .Include(u => u.IdlaboratoireNavigation)
                .ToListAsync();
            return View(list);
        }

        // GET: Utilisateurlaboratoire/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entity = await _context.Utilisateurlaboratoires
                .Include(u => u.Utilisateur)
                .Include(u => u.IdlaboratoireNavigation)
                .FirstOrDefaultAsync(m => m.Utilisateurlaboratoireid == id);
            if (entity == null)
            {
                return NotFound();
            }

            var vm = new UtilisateurlaboratoireCreateVM
            {
                Utilisateurlaboratoireid = entity.Utilisateurlaboratoireid,
                Utilisateurid = entity.Utilisateurid,
                Idlaboratoire = entity.Idlaboratoire
            };
            ViewBag.UtilisateurNom = entity.Utilisateur?.Nom;
            ViewBag.LaboratoireNom = entity.IdlaboratoireNavigation?.Nom;
            return View(vm);
        }

        // GET: Utilisateurlaboratoire/Create
        public IActionResult Create()
        {
            var vm = new UtilisateurlaboratoireCreateVM();
            //var utilisateursLabo = _context.Utilisateurprofils
            //    .Where(up => up.Profil.Nom == "technicien" || up.Profil.Nom == "biologiste")
            //    .Select(up => up.Utilisateur)
            //    .Distinct()
            //    .ToList();
            var users1 = _usermanager
                .GetUsersInRoleAsync(EnumRoles.Technicien.ToString())
                .GetAwaiter()
                .GetResult()
                .Select(x => x.Id)
                .ToList();

            var users2 = _usermanager
                .GetUsersInRoleAsync(EnumRoles.Biologiste.ToString())
                .GetAwaiter()
                .GetResult()
                .Select(x => x.Id)
                .ToList();

            var users = users1.Concat(users2).Distinct().ToList();

            var utilisateursLabo = users.Any()
                ? _context.Utilisateurs.AsNoTracking()
                    .Where(x => users.Contains(x.Userid))
                    .ToList()
                : new List<Utilisateur>();
            ViewBag.Utilisateurid = new SelectList(utilisateursLabo, "Utilisateurid", "Nom");
            ViewBag.Idlaboratoire = new SelectList(_context.Loboratoires, "Idlaboratoire", "Nom");
            return View(vm);
        }

        // POST: Utilisateurlaboratoire/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UtilisateurlaboratoireCreateVM model)
        {
            if (ModelState.IsValid)
            {
                var entity = new Utilisateurlaboratoire
                {
                    Utilisateurlaboratoireid = Guid.NewGuid(),
                    Utilisateurid = model.Utilisateurid,
                    Idlaboratoire = model.Idlaboratoire
                };
                _context.Add(entity);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            //var utilisateursLabo = _context.Utilisateurprofils
            //    .Where(up => up.Profil.Nom == "technicien" || up.Profil.Nom == "biologiste")
            //    .Select(up => up.Utilisateur)
            //    .Distinct()
            //    .ToList();
            var users1 = (await _usermanager
                .GetUsersInRoleAsync(EnumRoles.Technicien.ToString()))
                .Select(x => x.Id)
                .ToList();

            var users2 = (await _usermanager
                .GetUsersInRoleAsync(EnumRoles.Biologiste.ToString()))
                .Select(x => x.Id)
                .ToList();

            var users = users1.Concat(users2).Distinct().ToList();

            var utilisateursLabo = users.Any()
                ? _context.Utilisateurs.AsNoTracking()
                    .Where(x => users.Contains(x.Userid))
                    .ToList()
                : new List<Utilisateur>();
            ViewBag.Utilisateurid = new SelectList(utilisateursLabo, "Utilisateurid", "Nom", model.Utilisateurid);
            ViewBag.Idlaboratoire = new SelectList(_context.Loboratoires, "Idlaboratoire", "Nom", model.Idlaboratoire);
            return View(model);
        }

        // GET: Utilisateurlaboratoire/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entity = await _context.Utilisateurlaboratoires.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            var vm = new UtilisateurlaboratoireCreateVM
            {
                Utilisateurlaboratoireid = entity.Utilisateurlaboratoireid,
                Utilisateurid = entity.Utilisateurid,
                Idlaboratoire = entity.Idlaboratoire
            };

            //var utilisateursLabo = _context.Utilisateurprofils
            //    .Where(up => up.Profil.Nom == "technicien" || up.Profil.Nom == "biologiste")
            //    .Select(up => up.Utilisateur)
            //    .Distinct()
            //    .ToList();

            var users1 = (await _usermanager
                .GetUsersInRoleAsync(EnumRoles.Technicien.ToString()))
                .Select(x => x.Id)
                .ToList();

            var users2 = (await _usermanager
                .GetUsersInRoleAsync(EnumRoles.Biologiste.ToString()))
                .Select(x => x.Id)
                .ToList();

            var users = users1.Concat(users2).Distinct().ToList();

            var utilisateursLabo = users.Any()
                ? _context.Utilisateurs.AsNoTracking()
                    .Where(x => users.Contains(x.Userid))
                    .ToList()
                : new List<Utilisateur>();
            ViewBag.Utilisateurid = new SelectList(utilisateursLabo, "Utilisateurid", "Nom", vm.Utilisateurid);
            ViewBag.Idlaboratoire = new SelectList(_context.Loboratoires, "Idlaboratoire", "Nom", vm.Idlaboratoire);

            return View(vm);
        }

        // POST: Utilisateurlaboratoire/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UtilisateurlaboratoireCreateVM model)
        {
            if (id != model.Utilisateurlaboratoireid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var entity = await _context.Utilisateurlaboratoires.FindAsync(id);
                    if (entity == null)
                    {
                        return NotFound();
                    }
                    entity.Utilisateurid = model.Utilisateurid;
                    entity.Idlaboratoire = model.Idlaboratoire;
                    _context.Update(entity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UtilisateurlaboratoireExists(model.Utilisateurlaboratoireid))
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
            //var utilisateursLabo = _context.Utilisateurprofils
            //    .Where(up => up.Profil.Nom == "technicien" || up.Profil.Nom == "biologiste")
            //    .Select(up => up.Utilisateur)
            //    .Distinct()
            //    .ToList();
            var users1 = (await _usermanager
                .GetUsersInRoleAsync(EnumRoles.Technicien.ToString()))
                .Select(x => x.Id)
                .ToList();

            var users2 = (await _usermanager
                .GetUsersInRoleAsync(EnumRoles.Biologiste.ToString()))
                .Select(x => x.Id)
                .ToList();

            var users = users1.Concat(users2).Distinct().ToList();

            var utilisateursLabo = users.Any()
                ? _context.Utilisateurs.AsNoTracking()
                    .Where(x => users.Contains(x.Userid))
                    .ToList()
                : new List<Utilisateur>();
            ViewBag.Utilisateurid = new SelectList(utilisateursLabo, "Utilisateurid", "Nom", model.Utilisateurid);
            ViewBag.Idlaboratoire = new SelectList(_context.Loboratoires, "Idlaboratoire", "Nom", model.Idlaboratoire);
            return View(model);
        }

        // GET: Utilisateurlaboratoire/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entity = await _context.Utilisateurlaboratoires
                .Include(u => u.Utilisateur)
                .Include(u => u.IdlaboratoireNavigation)
                .FirstOrDefaultAsync(m => m.Utilisateurlaboratoireid == id);
            if (entity == null)
            {
                return NotFound();
            }

            var vm = new UtilisateurlaboratoireCreateVM
            {
                Utilisateurlaboratoireid = entity.Utilisateurlaboratoireid,
                Utilisateurid = entity.Utilisateurid,
                Idlaboratoire = entity.Idlaboratoire
            };
            ViewBag.UtilisateurNom = entity.Utilisateur?.Nom;
            ViewBag.LaboratoireNom = entity.IdlaboratoireNavigation?.Nom;
            return View(vm);
        }

        // POST: Utilisateurlaboratoire/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var entity = await _context.Utilisateurlaboratoires.FindAsync(id);
            if (entity != null)
            {
                _context.Utilisateurlaboratoires.Remove(entity);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool UtilisateurlaboratoireExists(Guid id)
        {
            return _context.Utilisateurlaboratoires.Any(e => e.Utilisateurlaboratoireid == id);
        }
    }
}
