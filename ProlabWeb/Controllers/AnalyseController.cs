using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProlabWeb.Db;
using ProlabWeb.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ProlabWeb.Controllers
{
    [AllowAnonymous]
    public class AnalyseController : Controller
    {
        private readonly ProlabwebContext _context;
        private readonly IMapper _mapper;

        public AnalyseController(ProlabwebContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: Analyse
        public async Task<IActionResult> Index()
        {
            var cultureFr = new CultureInfo("fr-FR");

            var prolabwebContext = _context.Analyses
                .Include(a => a.CodeuniteNavigation)
                .Include(a => a.CodeunitesiNavigation)
                .Include(a => a.IdanalyseparentNavigation)
                .Include(a => a.IdautomateNavigation)
                .Include(a => a.IdlaboratoireNavigation)
                .Include(a => a.IdnatureechantillonNavigation);

            return View(await prolabwebContext.ToListAsync());
        }

        // GET: Analyse/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var analyse = await _context.Analyses
                .Include(a => a.CodeuniteNavigation)
                .Include(a => a.CodeunitesiNavigation)
                .Include(a => a.IdanalyseparentNavigation)
                .Include(a => a.IdautomateNavigation)
                .Include(a => a.IdlaboratoireNavigation)
                .Include(a => a.IdnatureechantillonNavigation)
                .FirstOrDefaultAsync(m => m.Idanalyse == id);
            if (analyse == null)
            {
                return NotFound();
            }

            return View(analyse);
        }

        // GET: Analyse/Create
        public IActionResult Create()
        {
            var unites = new SelectList(_context.Unites, "Code", "Name").ToList();
            unites.Insert(0, new SelectListItem("---", string.Empty));

            var unitesis = new SelectList(_context.Unites, "Code", "Name").ToList();
            unitesis.Insert(0, new SelectListItem("---", string.Empty));

            var analyses = new SelectList(_context.Analyses, "Idanalyse", "Nom").ToList();
            analyses.Insert(0, new SelectListItem("---", string.Empty));

            var automates = new SelectList(_context.Automates, "Idautomate", "Nom").ToList();
            automates.Insert(0, new SelectListItem("---", string.Empty));

            var laboratoires = new SelectList(_context.Loboratoires, "Idlaboratoire", "Nom").ToList();
            laboratoires.Insert(0, new SelectListItem("---", string.Empty));

            var echantillons = new SelectList(_context.Natureechantillons, "Idnatureechantillon", "Nom").ToList();
            echantillons.Insert(0, new SelectListItem("---", string.Empty));

            var methodes = new SelectList(_context.Methodes, "Idmethode", "Nom").ToList();
            methodes.Insert(0, new SelectListItem("---", string.Empty));

            ViewData["Codeunite"] = unites;
            ViewData["Codeunitesi"] = unitesis;
            ViewData["Idanalyseparent"] = analyses;
            ViewData["Idautomate"] = automates;
            ViewData["Idlaboratoire"] = laboratoires;
            ViewData["Idnatureechantillon"] = echantillons;
            ViewData["IdsMethode"] = methodes;

            return View();
        }

        // POST: Analyse/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Idanalyse,Codeparametre,Aliascodeautomate,Description,Idautomate,Avecautomate,Formuleautomate,Affichermachineresultat,Decimalresultatstandard,Decimalresultatsi,Affichermethodresultat,Commentaire,Accredite,Prix,Isactive,Codeunite,Codeunitesi,Facteurconversionsi,Ordreaffichage,Idnatureechantillon,Idlaboratoire,Idanalyseparent,Indicederev,Codification")] Analyse analyse)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        analyse.Idanalyse = Guid.NewGuid();
        //        _context.Add(analyse);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["Codeunite"] = new SelectList(_context.Unites, "Code", "Code", analyse.Codeunite);
        //    ViewData["Codeunitesi"] = new SelectList(_context.Unites, "Name", "Code", analyse.Codeunitesi);
        //    ViewData["Idanalyseparent"] = new SelectList(_context.Analyses, "Idanalyse", "Idanalyse", analyse.Idanalyseparent);
        //    ViewData["Idautomate"] = new SelectList(_context.Automates, "Idautomate", "Idautomate", analyse.Idautomate);
        //    ViewData["Idlaboratoire"] = new SelectList(_context.Loboratoires, "Idlaboratoire", "Idlaboratoire", analyse.Idlaboratoire);
        //    ViewData["Idnatureechantillon"] = new SelectList(_context.Natureechantillons, "Idnatureechantillon", "Idnatureechantillon", analyse.Idnatureechantillon);
        //    return View(analyse);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AnalyseCreateVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Analyse analyse = _mapper.Map<Analyse>(model);
                    analyse.Idanalyse = Guid.NewGuid();

                    // parcourir la selection

                    foreach (var item in model.IdsMethode)
                    {
                        var obj = new Methodeanalyse
                        {
                            Methodeanalyseid = Guid.NewGuid(),
                            Idanalyse = analyse.Idanalyse,
                            Idmethode = int.Parse(item),
                            Isdefaultmethode = false
                        };
                        analyse.Methodeanalyses.Add(obj);
                    }

                    _context.Add(analyse);
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {

                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            var unites = new SelectList(_context.Unites, "Code", "Name", model.Codeunite).ToList();
            unites.Insert(0, new SelectListItem("---", string.Empty));

            var unitesis = new SelectList(_context.Unites, "Code", "Name", model.Codeunitesi).ToList();
            unitesis.Insert(0, new SelectListItem("---", string.Empty));

            var analyses = new SelectList(_context.Analyses, "Idanalyse", "Nom", model.Idanalyseparent).ToList();
            analyses.Insert(0, new SelectListItem("---", string.Empty));

            var automates = new SelectList(_context.Automates, "Idautomate", "Nom", model.Idautomate).ToList();
            automates.Insert(0, new SelectListItem("---", string.Empty));

            var laboratoires = new SelectList(_context.Loboratoires, "Idlaboratoire", "Nom", model.Idlaboratoire).ToList();
            laboratoires.Insert(0, new SelectListItem("---", string.Empty));

            var echantillons = new SelectList(_context.Natureechantillons, "Idnatureechantillon", "Nom", model.Idnatureechantillon).ToList();
            echantillons.Insert(0, new SelectListItem("---", string.Empty));

            var methodes = new SelectList(_context.Methodes, "Idmethode", "Nom", model.IdsMethode).ToList();
            methodes.Insert(0, new SelectListItem("---", string.Empty));

            ViewData["Codeunite"] = unites;
            ViewData["Codeunitesi"] = unitesis;
            ViewData["Idanalyseparent"] = analyses;
            ViewData["Idautomate"] = automates;
            ViewData["Idlaboratoire"] = laboratoires;
            ViewData["Idnatureechantillon"] = echantillons;
            ViewData["IdsMethode"] = methodes;

            return View(model);
        }

        // GET: Analyse/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var analyse = await _context.Analyses.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Idanalyse == id);

            if (analyse == null)
            {
                return NotFound();
            }

            var model = _mapper.Map<AnalyseEditVM>(analyse);

            var unites = new SelectList(_context.Unites, "Code", "Name", model.Codeunite).ToList();
            unites.Insert(0, new SelectListItem("---", string.Empty));

            var unitesis = new SelectList(_context.Unites, "Code", "Name", model.Codeunitesi).ToList();
            unitesis.Insert(0, new SelectListItem("---", string.Empty));

            var analyses = new SelectList(_context.Analyses, "Idanalyse", "Nom", model.Idanalyseparent).ToList();
            analyses.Insert(0, new SelectListItem("---", string.Empty));

            var automates = new SelectList(_context.Automates, "Idautomate", "Nom", model.Idautomate).ToList();
            automates.Insert(0, new SelectListItem("---", string.Empty));

            var laboratoires = new SelectList(_context.Loboratoires, "Idlaboratoire", "Nom", model.Idlaboratoire).ToList();
            laboratoires.Insert(0, new SelectListItem("---", string.Empty));

            var echantillons = new SelectList(_context.Natureechantillons, "Idnatureechantillon", "Nom", model.Idnatureechantillon).ToList();
            echantillons.Insert(0, new SelectListItem("---", string.Empty));

            var methodes = new SelectList(_context.Methodes, "Idmethode", "Nom").ToList();
            methodes.Insert(0, new SelectListItem("---", string.Empty));

            var selectedMethodes = await _context.Methodeanalyses.AsNoTracking()
                .Include(x => x.IdanalyseNavigation)
                .Include(x => x.IdmethodeNavigation)
                .Where(x => x.Idanalyse == id)
                .Select(x => x.Idmethode.ToString())
                .ToListAsync();

            foreach (var item in methodes)
            {
                if (selectedMethodes.Contains(item.Value))
                    item.Selected = true;
            }

            ViewData["Codeunite"] = unites;
            ViewData["Codeunitesi"] = unitesis;
            ViewData["Idanalyseparent"] = analyses;
            ViewData["Idautomate"] = automates;
            ViewData["Idlaboratoire"] = laboratoires;
            ViewData["Idnatureechantillon"] = echantillons;
            ViewData["IdsMethode"] = methodes;

            return View(model);
        }

        // POST: Analyse/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, AnalyseEditVM model)
        {
            if (id != model.Idanalyse)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var analyse = _mapper.Map<Analyse>(model);

                    _context.Update(analyse);

                    // anciennes methodes selectionnees

                    var anListMethodeAnalyse = await _context.Methodeanalyses.AsNoTracking()
                        .Where(x => x.Idanalyse == id)
                        .ToListAsync();

                    // parcourir la selection ancienne

                    var toDelete = new List<Methodeanalyse>();

                    foreach (var item in anListMethodeAnalyse)
                    {
                        // chercher dans la nouvelle selection
                        var found = model.IdsMethode.Contains(item.Idmethode.ToString());

                        if (!found)
                        {
                            // supprimer
                            toDelete.Add(item);
                        }
                    }
                    if (toDelete.Any())
                    {
                        _context.RemoveRange(toDelete);
                    }

                    // parcourir la selection nouvelle

                    var toUpdate = new List<Methodeanalyse>();
                    var toCreate = new List<Methodeanalyse>();

                    foreach (var item in model.IdsMethode)
                    {
                        // chercher dans la selection ancienne

                        var found = anListMethodeAnalyse
                            .Where(x => x.Idmethode.ToString() == item)
                            .FirstOrDefault();

                        if (found == null)
                        {
                            // ajouter
                            var obj = new Methodeanalyse
                            {
                                Methodeanalyseid = Guid.NewGuid(),
                                Idanalyse = analyse.Idanalyse,
                                Idmethode = int.Parse(item),
                                Isdefaultmethode = false
                            };
                            toCreate.Add(obj);
                        }
                        else
                        {
                            // mettre à jour
                        }
                    }
                    if (toCreate.Any())
                    {
                        _context.AddRange(toCreate);
                    }
                    if (toUpdate.Any())
                    {
                        _context.UpdateRange(toUpdate);
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnalyseExists(model.Idanalyse))
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

            var unites = new SelectList(_context.Unites, "Code", "Name", model.Codeunite).ToList();
            unites.Insert(0, new SelectListItem("---", string.Empty));

            var unitesis = new SelectList(_context.Unites, "Code", "Name", model.Codeunitesi).ToList();
            unitesis.Insert(0, new SelectListItem("---", string.Empty));

            var analyses = new SelectList(_context.Analyses, "Idanalyse", "Nom", model.Idanalyseparent).ToList();
            analyses.Insert(0, new SelectListItem("---", string.Empty));

            var automates = new SelectList(_context.Automates, "Idautomate", "Nom", model.Idautomate).ToList();
            automates.Insert(0, new SelectListItem("---", string.Empty));

            var laboratoires = new SelectList(_context.Loboratoires, "Idlaboratoire", "Nom", model.Idlaboratoire).ToList();
            laboratoires.Insert(0, new SelectListItem("---", string.Empty));

            var echantillons = new SelectList(_context.Natureechantillons, "Idnatureechantillon", "Nom", model.Idnatureechantillon).ToList();
            echantillons.Insert(0, new SelectListItem("---", string.Empty));

            var methodes = new SelectList(_context.Methodes, "Idmethode", "Nom").ToList();
            methodes.Insert(0, new SelectListItem("---", string.Empty));

            var selectedMethodes = model.IdsMethode;

            foreach (var item in methodes)
            {
                if (selectedMethodes.Contains(item.Value))
                    item.Selected = true;
            }

            ViewData["Codeunite"] = unites;
            ViewData["Codeunitesi"] = unitesis;
            ViewData["Idanalyseparent"] = analyses;
            ViewData["Idautomate"] = automates;
            ViewData["Idlaboratoire"] = laboratoires;
            ViewData["Idnatureechantillon"] = echantillons;
            ViewData["IdsMethode"] = methodes;

            return View(model);
        }

        // GET: Analyse/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var analyse = await _context.Analyses
                .Include(a => a.CodeuniteNavigation)
                .Include(a => a.CodeunitesiNavigation)
                .Include(a => a.IdanalyseparentNavigation)
                .Include(a => a.IdautomateNavigation)
                .Include(a => a.IdlaboratoireNavigation)
                .Include(a => a.IdnatureechantillonNavigation)
                .FirstOrDefaultAsync(m => m.Idanalyse == id);
            if (analyse == null)
            {
                return NotFound();
            }

            return View(analyse);
        }

        // POST: Analyse/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var analyse = await _context.Analyses.FindAsync(id);
            if (analyse != null)
            {
                _context.Analyses.Remove(analyse);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AnalyseExists(Guid id)
        {
            return _context.Analyses.Any(e => e.Idanalyse == id);
        }

        #region handlers

        [HttpPost, ActionName("DeleteAnalyse")]
        public async Task<IActionResult> DeleteAnalyseAsync(Guid id)
        {
            JsonResponseViewModel resultat = new JsonResponseViewModel();

            if (id != Guid.Empty)
            {
                var analyse = await _context.Analyses.AsNoTracking()
                    .Where(x => x.Idanalyse == id)
                    .FirstOrDefaultAsync();

                if (analyse != null)
                {
                    _context.Remove(analyse);
                    await _context.SaveChangesAsync();

                    resultat.success = true;
                }
            }

            return Ok(resultat);
        }

        [AcceptVerbs("Get", "Post")]
        public IActionResult IsNomAvailable(string nom)
        {
            var exists = _context.Analyses.Any(u => u.Nom == nom);
            if (exists)
                return Json($"Nom {nom} is already in use.");
            return Json(true);
        }

        #endregion
    }
}
