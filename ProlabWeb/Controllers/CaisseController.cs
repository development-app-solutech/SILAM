using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProlabWeb.Db;
using AutoMapper;
using ProlabWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace ProlabWeb.Controllers
{
    [AllowAnonymous]
    public class CaisseController : Controller
    {
        private readonly ProlabwebContext _context;
        private readonly IMapper _mapper;

        public CaisseController(ProlabwebContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: Caisse
        public async Task<IActionResult> Index()
        {
            var caisses = await _context.Caisses.Include(c => c.CodesiteNavigation).ToListAsync();
            return View(caisses);
        }

        // GET: Caisse/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var caisse = await _context.Caisses
                .Include(c => c.CodesiteNavigation)
                .FirstOrDefaultAsync(m => m.Caisseid == id);
            if (caisse == null)
            {
                return NotFound();
            }

            return View(caisse);
        }

        // GET: Caisse/Create
        public IActionResult Create()
        {
            var vm = new CaisseCreateVM();
            vm.Sites = _context.Sites.Select(s => new SiteItem { Codesite = s.Codesite, Name = s.Name }).ToList();
            return View(vm);
        }

        // POST: Caisse/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CaisseCreateVM vm)
        {
            if (ModelState.IsValid)
            {
                var caisse = _mapper.Map<Caisse>(vm);
                caisse.Caisseid = Guid.NewGuid();
                _context.Add(caisse);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            vm.Sites = _context.Sites.Select(s => new SiteItem { Codesite = s.Codesite, Name = s.Name }).ToList();
            return View(vm);
        }

        // GET: Caisse/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var caisse = await _context.Caisses.FindAsync(id);
            if (caisse == null)
            {
                return NotFound();
            }
            var vm = _mapper.Map<CaisseEditVM>(caisse);
            vm.Sites = _context.Sites.Select(s => new SiteItem { Codesite = s.Codesite, Name = s.Name }).ToList();
            return View(vm);
        }

        // POST: Caisse/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CaisseEditVM vm)
        {
            if (id != vm.Caisseid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var caisse = _mapper.Map<Caisse>(vm);
                    _context.Update(caisse);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CaisseExists(vm.Caisseid))
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
            vm.Sites = _context.Sites.Select(s => new SiteItem { Codesite = s.Codesite, Name = s.Name }).ToList();
            return View(vm);
        }

        // GET: Caisse/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var caisse = await _context.Caisses
                .Include(c => c.CodesiteNavigation)
                .FirstOrDefaultAsync(m => m.Caisseid == id);
            if (caisse == null)
            {
                return NotFound();
            }

            return View(caisse);
        }

        // POST: Caisse/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var caisse = await _context.Caisses.FindAsync(id);
            if (caisse != null)
            {
                _context.Caisses.Remove(caisse);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CaisseExists(Guid id)
        {
            return _context.Caisses.Any(e => e.Caisseid == id);
        }
    }
}
