using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProlabWeb.Db;

namespace ProlabWeb.Controllers
{
    public class PhotopatientController : Controller
    {
        private readonly ProlabwebContext _context;

        public PhotopatientController(ProlabwebContext context)
        {
            _context = context;
        }

        // GET: Photopatient
        public async Task<IActionResult> Index()
        {
            var prolabwebContext = _context.Photopatients.Include(p => p.Patient);
            return View(await prolabwebContext.ToListAsync());
        }

        // GET: Photopatient/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var photopatient = await _context.Photopatients
                .Include(p => p.Patient)
                .FirstOrDefaultAsync(m => m.Photopatientid == id);
            if (photopatient == null)
            {
                return NotFound();
            }

            return View(photopatient);
        }

        // GET: Photopatient/Create
        public IActionResult Create()
        {
            ViewData["Patientid"] = new SelectList(_context.Patients, "Patientid", "Patientid");
            return View();
        }

        // POST: Photopatient/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Photopatientid,Patientid,Photo")] Photopatient photopatient)
        {
            if (ModelState.IsValid)
            {
                photopatient.Photopatientid = Guid.NewGuid();
                _context.Add(photopatient);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Patientid"] = new SelectList(_context.Patients, "Patientid", "Patientid", photopatient.Patientid);
            return View(photopatient);
        }

        // GET: Photopatient/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var photopatient = await _context.Photopatients.FindAsync(id);
            if (photopatient == null)
            {
                return NotFound();
            }
            ViewData["Patientid"] = new SelectList(_context.Patients, "Patientid", "Patientid", photopatient.Patientid);
            return View(photopatient);
        }

        // POST: Photopatient/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Photopatientid,Patientid,Photo")] Photopatient photopatient)
        {
            if (id != photopatient.Photopatientid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(photopatient);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhotopatientExists(photopatient.Photopatientid))
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
            ViewData["Patientid"] = new SelectList(_context.Patients, "Patientid", "Patientid", photopatient.Patientid);
            return View(photopatient);
        }

        // GET: Photopatient/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var photopatient = await _context.Photopatients
                .Include(p => p.Patient)
                .FirstOrDefaultAsync(m => m.Photopatientid == id);
            if (photopatient == null)
            {
                return NotFound();
            }

            return View(photopatient);
        }

        // POST: Photopatient/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var photopatient = await _context.Photopatients.FindAsync(id);
            if (photopatient != null)
            {
                _context.Photopatients.Remove(photopatient);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PhotopatientExists(Guid id)
        {
            return _context.Photopatients.Any(e => e.Photopatientid == id);
        }
    }
}
