using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProlabWeb.Db;
using ProlabWeb.Helpers;
using ProlabWeb.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProlabWeb.Controllers
{
    [AllowAnonymous]
    public class PatientController : Controller
    {
        private readonly ProlabwebContext _context;
        private readonly IMapper _mapper;

        public PatientController(ProlabwebContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: Patient
        public async Task<IActionResult> Index()
        {
            var patients = await _context.Patients
                .Include(p => p.CodesexeNavigation)  // Charge le genre
                .OrderByDescending(p => p.Creationdate)  // ajout du tri
                .ToListAsync();

            var photoPatients = await _context.Photopatients.ToListAsync();

            var list = patients.Select(x => new PatientItem
            {
                Patient = x,
                Photopatient = photoPatients.FirstOrDefault(y => y.Patientid == x.Patientid)
            });

            return View(list);
        }


        // GET: Patient/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patients
                .FirstOrDefaultAsync(m => m.Patientid == id);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        // GET: Patient/Create
        public IActionResult Create()
        {
            var sites = new SelectList(_context.Sites, "Codesite", "Name").ToList();
            sites.Insert(0, new SelectListItem("---", string.Empty));

            var sexes = new SelectList(_context.Sexes, "Codesexe", "Value").ToList();
            sexes.Insert(0, new SelectListItem("---", string.Empty));

            var typepeaus = new SelectList(_context.Typepeaus, "Codetypepeau", "Nom").ToList();
            typepeaus.Insert(0, new SelectListItem("---", string.Empty));

            var typedocumentidentites = new SelectList(_context.Typedocumentidentites, "Codetypedocumentidentite", "Nom").ToList();
            typedocumentidentites.Insert(0, new SelectListItem("---", string.Empty));

            ViewData["Codesite"] = sites;
            ViewData["Codesexe"] = sexes;
            ViewData["Codetypepeau"] = typepeaus;
            ViewData["Codetypedocumentidentite"] = typedocumentidentites;

            var model = new PatientCreateVM
            {
                Code = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()
            };

            return View(model);
        }

        // POST: Patient/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PatientCreateVM model)
        {
            if (model.Datenaissance.HasValue)
            {
                model.Age = Utilities.CalculerAgeEnAnnee(model.Datenaissance.Value);
                ModelState.Remove("Age");
            }

            if (!model.Datenaissance.HasValue && model.Age <= 0)
            {
                ModelState.AddModelError("Age", "L'âge est obligatoire si la date de naissance n'est pas renseignée.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var patient = _mapper.Map<Patient>(model);
                    patient.Datenaissance = model.Datenaissance ?? DateTime.MinValue;

                    patient.Patientid = Guid.NewGuid();

                    // AJOUT : date de cr�ation
                    patient.Creationdate = DateTime.Now;

                    _context.Add(patient);

                    var file = model.File;
                    var filename = model.Filename;

                    if (!string.IsNullOrWhiteSpace(filename) && file != null && file.Length > 0)
                    {
                        using (var ms = new MemoryStream())
                        {
                            await file.CopyToAsync(ms);
                            var fileBytes = ms.ToArray();

                            var photoPatient = new Photopatient
                            {
                                Photopatientid = Guid.NewGuid(),
                                Patientid = patient.Patientid,
                                Photo = fileBytes,
                                Extension = Path.GetExtension(file.FileName).TrimStart('.').ToLower()
                            };
                            _context.Add(photoPatient);
                        }
                    }

                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {

                    throw;
                }
            }

            var sites = new SelectList(_context.Sites, "Codesite", "Name", model.Codesite).ToList();
            sites.Insert(0, new SelectListItem("---", string.Empty));

            var sexes = new SelectList(_context.Sexes, "Codesexe", "Value", model.Codesexe).ToList();
            sexes.Insert(0, new SelectListItem("---", string.Empty));

            var typepeaus = new SelectList(_context.Typepeaus, "Codetypepeau", "Nom", model.Codetypepeau).ToList();
            typepeaus.Insert(0, new SelectListItem("---", string.Empty));

            var typedocumentidentites = new SelectList(_context.Typedocumentidentites, "Codetypedocumentidentite", "Nom", model.Codetypedocumentidentite).ToList();
            typedocumentidentites.Insert(0, new SelectListItem("---", string.Empty));

            ViewData["Codesite"] = sites;
            ViewData["Codesexe"] = sexes;
            ViewData["Codetypepeau"] = typepeaus;
            ViewData["Codetypedocumentidentite"] = typedocumentidentites;

            return View(model);
        }

        // GET: Patient/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patients.AsNoTracking()
                .Where(x => x.Patientid == id)
                .FirstOrDefaultAsync();

            if (patient == null)
            {
                return NotFound();
            }

            var photoPatient = await _context.Photopatients.AsNoTracking()
                .Where(x => x.Patientid == patient.Patientid)
                .FirstOrDefaultAsync();

            string src = photoPatient != null
                ? Utilities.ConvertByteArrayToBase64(photoPatient.Photo, photoPatient.Extension)
                : "";

            var model = _mapper.Map<PatientEditVM>(patient);

            var sites = new SelectList(_context.Sites, "Codesite", "Name", model.Codesite).ToList();
            sites.Insert(0, new SelectListItem("---", string.Empty));

            var sexes = new SelectList(_context.Sexes, "Codesexe", "Value", model.Codesexe).ToList();
            sexes.Insert(0, new SelectListItem("---", string.Empty));

            var typepeaus = new SelectList(_context.Typepeaus, "Codetypepeau", "Nom", model.Codetypepeau).ToList();
            typepeaus.Insert(0, new SelectListItem("---", string.Empty));

            var typedocumentidentites = new SelectList(_context.Typedocumentidentites, "Codetypedocumentidentite", "Nom", model.Codetypedocumentidentite).ToList();
            typedocumentidentites.Insert(0, new SelectListItem("---", string.Empty));

            ViewData["PhotoSrc"] = src;
            ViewData["Codesite"] = sites;
            ViewData["Codesexe"] = sexes;
            ViewData["Codetypepeau"] = typepeaus;
            ViewData["Codetypedocumentidentite"] = typedocumentidentites;

            return View(model);
        }

        // POST: Patient/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, PatientEditVM model)
        {
            if (id != model.Patientid)
            {
                return NotFound();
            }

            if (model.Datenaissance.HasValue)
            {
                model.Age = Utilities.CalculerAgeEnAnnee(model.Datenaissance.Value);
                ModelState.Remove("Age");
            }

            if (!model.Datenaissance.HasValue && model.Age <= 0)
            {
                ModelState.AddModelError("Age", "L'âge est obligatoire si la date de naissance n'est pas renseignée.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Chargement de l'entit� existante
                    var patient = await _context.Patients.FindAsync(id);
                    if (patient == null)
                    {
                        return NotFound();
                    }

                    // Mise � jour des champs avec AutoMapper
                    _mapper.Map(model, patient);
                    patient.Datenaissance = model.Datenaissance ?? DateTime.MinValue;

                    // AJOUT : date de mise � jour
                    patient.Updatedate = DateTime.Now;

                    _context.Update(patient);

                    var file = model.File;
                    var filename = model.Filename;

                    if (!string.IsNullOrWhiteSpace(filename) && file != null && file.Length > 0)
                    {
                        var old = await _context.Photopatients.AsNoTracking()
                            .Where(x => x.Patientid == patient.Patientid)
                            .FirstOrDefaultAsync();

                        using (var ms = new MemoryStream())
                        {
                            await file.CopyToAsync(ms);
                            var fileBytes = ms.ToArray();

                            if (old == null)
                            {
                                var photoPatient = new Photopatient
                                {
                                    Photopatientid = Guid.NewGuid(),
                                    Patientid = patient.Patientid,
                                    Photo = fileBytes,
                                    Extension = Path.GetExtension(file.FileName).TrimStart('.').ToLower()
                                };

                                _context.Add(photoPatient);
                            }
                            else
                            {
                                old.Photo = fileBytes;
                                old.Extension = Path.GetExtension(file.FileName).TrimStart('.').ToLower();

                                _context.Update(old);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientExists(model.Patientid))
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

            var sites = new SelectList(_context.Sites, "Codesite", "Name", model.Codesite).ToList();
            sites.Insert(0, new SelectListItem("---", string.Empty));

            var sexes = new SelectList(_context.Sexes, "Codesexe", "Value", model.Codesexe).ToList();
            sexes.Insert(0, new SelectListItem("---", string.Empty));

            var typepeaus = new SelectList(_context.Typepeaus, "Codetypepeau", "Nom", model.Codetypepeau).ToList();
            typepeaus.Insert(0, new SelectListItem("---", string.Empty));

            var typedocumentidentites = new SelectList(_context.Typedocumentidentites, "Codetypedocumentidentite", "Nom", model.Codetypedocumentidentite).ToList();
            typedocumentidentites.Insert(0, new SelectListItem("---", string.Empty));

            ViewData["Codesite"] = sites;
            ViewData["Codesexe"] = sexes;
            ViewData["Codetypepeau"] = typepeaus;
            ViewData["Codetypedocumentidentite"] = typedocumentidentites;

            return View(model);
        }

        // GET: Patient/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patients
                .FirstOrDefaultAsync(m => m.Patientid == id);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        // POST: Patient/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                _context.Patients.Remove(patient);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PatientExists(Guid id)
        {
            return _context.Patients.Any(e => e.Patientid == id);
        }

        #region handlers

        [HttpGet, ActionName("GetPatient")]
        public async Task<IActionResult> GetPatientAsync(Guid id)
        {
            JsonResponseViewModel resultat = new JsonResponseViewModel();

            if (id != Guid.Empty)
            {
                var photopatient = await _context.Photopatients.AsNoTracking()
                    .Where(x => x.Patientid == id)
                    .FirstOrDefaultAsync();

                var patient = await _context.Patients.AsNoTracking()
                    .Include(x => x.CodesexeNavigation)
                    .Include(x => x.CodetypepeauNavigation)
                    .Include(x => x.CodetypedocumentidentiteNavigation)
                    .Where(x => x.Patientid == id)
                    .Select(x => new EnteteDemandePatientVM
                    {
                        Code = x.Code,
                        Nom = x.Nom,
                        Prenom = x.Prenom,
                        Nomusage = x.Nomusage,
                        Codesexe = x.Codesexe,
                        Codetypepeau = x.Codetypepeau,
                        Datenaissance = x.Datenaissance == DateTime.MinValue ? null : x.Datenaissance,
                        Age = x.Age,
                        Lieunaissance = x.Lieunaissance,
                        Codetypedocumentidentite = x.Codetypedocumentidentite,
                        Numerodocumentidentite = x.Numerodocumentidentite,
                        Adresse = x.Adresse,
                        Tel = x.Tel,
                        Email = x.Email,
                        Ville = x.Ville,
                        Quartier = x.Quartier,
                        Renseignementclinique = x.Renseignementclinique,
                        PhotoBase64 = photopatient != null
                            ? Utilities.ConvertByteArrayToBase64(photopatient.Photo, photopatient.Extension)
                            : ""
                    })
                    .FirstOrDefaultAsync();

                if (patient != null)
                {
                    string dataJson = JsonConvert.SerializeObject(patient, Formatting.Indented, new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });

                    resultat.success = true;

                    resultat.data = dataJson;
                }
            }

            return Ok(resultat);
        }

        #endregion
    }
}
