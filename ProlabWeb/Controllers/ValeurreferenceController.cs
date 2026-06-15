using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProlabWeb.Db;
using ProlabWeb.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProlabWeb.Controllers
{

    [AllowAnonymous]
    public class ValeurreferenceController : Controller
    {
        private readonly ProlabwebContext _context;
        private readonly IMapper _mapper;

        public ValeurreferenceController(ProlabwebContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: Valeurreference
        public async Task<IActionResult> Index()
        {
            var prolabwebContext = _context.Valeurreferences.Include(v => v.CodeunitagedebutNavigation).Include(v => v.CodeunitagefinNavigation).Include(v => v.CodeunitreferenceNavigation).Include(v => v.CodeunitreferencesiNavigation).Include(v => v.IdanalyseNavigation).Include(v => v.SexeautorisecodeNavigation);
            return View(await prolabwebContext.ToListAsync());
        }

        // GET: Valeurreference/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var valeurreference = await _context.Valeurreferences
                .Include(v => v.CodeunitagedebutNavigation)
                .Include(v => v.CodeunitagefinNavigation)
                .Include(v => v.CodeunitreferenceNavigation)
                .Include(v => v.CodeunitreferencesiNavigation)
                .Include(v => v.IdanalyseNavigation)
                .Include(v => v.SexeautorisecodeNavigation)
                .FirstOrDefaultAsync(m => m.Valeurreferenceid == id);
            if (valeurreference == null)
            {
                return NotFound();
            }

            return View(valeurreference);
        }

        // GET: Valeurreference/Create
        public IActionResult Create()
        {
            var uniteagedebuts = new SelectList(_context.Unites, "Code", "Name").ToList();
            uniteagedebuts.Insert(0, new SelectListItem("---", string.Empty));

            var uniteagefins = new SelectList(_context.Unites, "Code", "Name").ToList();
            uniteagefins.Insert(0, new SelectListItem("---", string.Empty));

            var unitereferences = new SelectList(_context.Unites, "Code", "Name").ToList();
            unitereferences.Insert(0, new SelectListItem("---", string.Empty));

            var unitereferencesis = new SelectList(_context.Unites, "Code", "Name").ToList();
            unitereferencesis.Insert(0, new SelectListItem("---", string.Empty));

            var analyses = new SelectList(_context.Analyses, "Idanalyse", "Nom").ToList();
            analyses.Insert(0, new SelectListItem("---", string.Empty));

            var sexeautorises = new SelectList(_context.Sexeautorises, "Sexeautorisecode", "Valeur").ToList();
            sexeautorises.Insert(0, new SelectListItem("---", string.Empty));

            ViewData["Codeunitagedebut"] = uniteagedebuts;
            ViewData["Codeunitagefin"] = uniteagefins;
            ViewData["Codeunitreference"] = unitereferences;
            ViewData["Codeunitreferencesi"] = unitereferencesis;
            ViewData["Idanalyse"] = analyses;
            ViewData["Sexeautorisecode"] = sexeautorises;

            return View();
        }

        // POST: Valeurreference/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ValeurreferenceCreateVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var valeurreference = _mapper.Map<Valeurreference>(model);
                    valeurreference.Valeurreferenceid = Guid.NewGuid();

                    _context.Add(valeurreference);
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {

                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            var uniteagedebuts = new SelectList(_context.Unites, "Code", "Name", model.Codeunitagedebut).ToList();
            uniteagedebuts.Insert(0, new SelectListItem("---", string.Empty));

            var uniteagefins = new SelectList(_context.Unites, "Code", "Name", model.Codeunitagefin).ToList();
            uniteagefins.Insert(0, new SelectListItem("---", string.Empty));

            var unitereferences = new SelectList(_context.Unites, "Code", "Name", model.Codeunitreference).ToList();
            unitereferences.Insert(0, new SelectListItem("---", string.Empty));

            var unitereferencesis = new SelectList(_context.Unites, "Code", "Name", model.Codeunitreferencesi).ToList();
            unitereferencesis.Insert(0, new SelectListItem("---", string.Empty));

            var analyses = new SelectList(_context.Analyses, "Idanalyse", "Nom", model.Idanalyse).ToList();
            analyses.Insert(0, new SelectListItem("---", string.Empty));

            var sexeautorises = new SelectList(_context.Sexeautorises, "Sexeautorisecode", "Valeur", model.Sexeautorisecode).ToList();
            sexeautorises.Insert(0, new SelectListItem("---", string.Empty));

            ViewData["Codeunitagedebut"] = uniteagedebuts;
            ViewData["Codeunitagefin"] = uniteagefins;
            ViewData["Codeunitreference"] = unitereferences;
            ViewData["Codeunitreferencesi"] = unitereferencesis;
            ViewData["Idanalyse"] = analyses;
            ViewData["Sexeautorisecode"] = sexeautorises;

            return View(model);
        }

        // GET: Valeurreference/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var valeurreference = await _context.Valeurreferences.AsNoTracking()
                .Where(x => x.Valeurreferenceid == id)
                .FirstOrDefaultAsync();

            if (valeurreference == null)
            {
                return NotFound();
            }

            var model = _mapper.Map<ValeurreferenceEditVM>(valeurreference);

            var uniteagedebuts = new SelectList(_context.Unites, "Code", "Name", model.Codeunitagedebut).ToList();
            uniteagedebuts.Insert(0, new SelectListItem("---", string.Empty));

            var uniteagefins = new SelectList(_context.Unites, "Code", "Name", model.Codeunitagefin).ToList();
            uniteagefins.Insert(0, new SelectListItem("---", string.Empty));

            var unitereferences = new SelectList(_context.Unites, "Code", "Name", model.Codeunitreference).ToList();
            unitereferences.Insert(0, new SelectListItem("---", string.Empty));

            var unitereferencesis = new SelectList(_context.Unites, "Code", "Name", model.Codeunitreferencesi).ToList();
            unitereferencesis.Insert(0, new SelectListItem("---", string.Empty));

            var analyses = new SelectList(_context.Analyses, "Idanalyse", "Nom", model.Idanalyse).ToList();
            analyses.Insert(0, new SelectListItem("---", string.Empty));

            var sexeautorises = new SelectList(_context.Sexeautorises, "Sexeautorisecode", "Valeur", model.Sexeautorisecode).ToList();
            sexeautorises.Insert(0, new SelectListItem("---", string.Empty));

            ViewData["Codeunitagedebut"] = uniteagedebuts;
            ViewData["Codeunitagefin"] = uniteagefins;
            ViewData["Codeunitreference"] = unitereferences;
            ViewData["Codeunitreferencesi"] = unitereferencesis;
            ViewData["Idanalyse"] = analyses;
            ViewData["Sexeautorisecode"] = sexeautorises;

            return View(model);
        }

        // POST: Valeurreference/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ValeurreferenceEditVM model)
        {
            if (id != model.Valeurreferenceid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var valeurreference = _mapper.Map<Valeurreference>(model);

                    _context.Update(valeurreference);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ValeurreferenceExists(model.Valeurreferenceid))
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

            var uniteagedebuts = new SelectList(_context.Unites, "Code", "Name", model.Codeunitagedebut).ToList();
            uniteagedebuts.Insert(0, new SelectListItem("---", string.Empty));

            var uniteagefins = new SelectList(_context.Unites, "Code", "Name", model.Codeunitagefin).ToList();
            uniteagefins.Insert(0, new SelectListItem("---", string.Empty));

            var unitereferences = new SelectList(_context.Unites, "Code", "Name", model.Codeunitreference).ToList();
            unitereferences.Insert(0, new SelectListItem("---", string.Empty));

            var unitereferencesis = new SelectList(_context.Unites, "Code", "Name", model.Codeunitreferencesi).ToList();
            unitereferencesis.Insert(0, new SelectListItem("---", string.Empty));

            var analyses = new SelectList(_context.Analyses, "Idanalyse", "Nom", model.Idanalyse).ToList();
            analyses.Insert(0, new SelectListItem("---", string.Empty));

            var sexeautorises = new SelectList(_context.Sexeautorises, "Sexeautorisecode", "Valeur", model.Sexeautorisecode).ToList();
            sexeautorises.Insert(0, new SelectListItem("---", string.Empty));

            ViewData["Codeunitagedebut"] = uniteagedebuts;
            ViewData["Codeunitagefin"] = uniteagefins;
            ViewData["Codeunitreference"] = unitereferences;
            ViewData["Codeunitreferencesi"] = unitereferencesis;
            ViewData["Idanalyse"] = analyses;
            ViewData["Sexeautorisecode"] = sexeautorises;

            return View(model);
        }

        // GET: Valeurreference/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var valeurreference = await _context.Valeurreferences
                .Include(v => v.CodeunitagedebutNavigation)
                .Include(v => v.CodeunitagefinNavigation)
                .Include(v => v.CodeunitreferenceNavigation)
                .Include(v => v.CodeunitreferencesiNavigation)
                .Include(v => v.IdanalyseNavigation)
                .Include(v => v.SexeautorisecodeNavigation)
                .FirstOrDefaultAsync(m => m.Valeurreferenceid == id);
            if (valeurreference == null)
            {
                return NotFound();
            }

            return View(valeurreference);
        }

        // POST: Valeurreference/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var valeurreference = await _context.Valeurreferences.FindAsync(id);
            if (valeurreference != null)
            {
                _context.Valeurreferences.Remove(valeurreference);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ValeurreferenceExists(Guid id)
        {
            return _context.Valeurreferences.Any(e => e.Valeurreferenceid == id);
        }

        #region handlers

        [HttpPost, ActionName("DeleteValeurreference")]
        public async Task<IActionResult> DeleteValeurreferenceAsync(Guid id)
        {
            JsonResponseViewModel resultat = new JsonResponseViewModel();

            if (id != Guid.Empty)
            {
                var valeurreference = await _context.Valeurreferences.AsNoTracking()
                    .Where(x => x.Valeurreferenceid == id)
                    .FirstOrDefaultAsync();

                if (valeurreference != null)
                {
                    _context.Remove(valeurreference);
                    await _context.SaveChangesAsync();

                    resultat.success = true;
                }
            }

            return Ok(resultat);
        }

        #endregion
    }
}
