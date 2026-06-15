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
    public class AssuranceController : Controller
    {
        private readonly ProlabwebContext _context;
        private readonly IMapper _mapper;

        public AssuranceController(ProlabwebContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: Assurance
        public async Task<IActionResult> Index()
        {
            try
            {
                return View(await _context.Assurances.ToListAsync());
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        // GET: Assurance/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assurance = await _context.Assurances
                .FirstOrDefaultAsync(m => m.Codeassurance == id);
            if (assurance == null)
            {
                return NotFound();
            }

            return View(assurance);
        }

        // GET: Assurance/Create
        public IActionResult Create()
        {
            var vm = new AssuranceCreateVM();
            ViewBag.Codetypeassurance = new SelectList(_context.Typeassurances, "Codetypeassurance", "Nom");
            return View(vm);
        }

        // POST: Assurance/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssuranceCreateVM vm)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var assurance = _mapper.Map<Assurance>(vm);
                _context.Add(assurance);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message + (ex.InnerException != null ? " | " + ex.InnerException.Message : "");
            }
            ViewBag.Codetypeassurance = new SelectList(_context.Typeassurances, "Codetypeassurance", "Nom", vm.Codetypeassurance);
            return View(vm);
        }

        // GET: Assurance/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assurance = await _context.Assurances.FindAsync(id);
            if (assurance == null)
            {
                return NotFound();
            }
            var vm = _mapper.Map<AssuranceEditVM>(assurance);
            ViewBag.Codetypeassurance = new SelectList(_context.Typeassurances, "Codetypeassurance", "Nom", vm.Codetypeassurance);
            return View(vm);
        }

        // POST: Assurance/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, AssuranceEditVM vm)
        {
            if (id != vm.Codeassurance)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var assurance = _mapper.Map<Assurance>(vm);
                    _context.Update(assurance);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                    }
            catch (Exception ex)
                    {
                ViewBag.Error = ex.Message + (ex.InnerException != null ? " | " + ex.InnerException.Message : "");
            }
            ViewBag.Codetypeassurance = new SelectList(_context.Typeassurances, "Codetypeassurance", "Nom", vm.Codetypeassurance);
            return View(vm);
        }

        // GET: Assurance/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assurance = await _context.Assurances
                .FirstOrDefaultAsync(m => m.Codeassurance == id);
            if (assurance == null)
            {
                return NotFound();
            }

            return View(assurance);
        }

        // POST: Assurance/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var assurance = await _context.Assurances.FindAsync(id);
            if (assurance != null)
            {
                _context.Assurances.Remove(assurance);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AssuranceExists(string id)
        {
            return _context.Assurances.Any(e => e.Codeassurance == id);
        }
    }
}
