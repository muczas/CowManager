using CowManagerApp.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CowManager.Controllers
{
    public class CowController : Controller
    {
        private readonly CowManagerContext _context;
        public CowController(CowManagerContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            if (_context.Cows == null)
            {
                return Problem("Entity set 'ApiContext.Movie'  is null.");
            }

            var cows = _context.Cows;

            return View(await cows.ToListAsync());
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cows = await _context.Cows
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cows == null)
            {
                return NotFound();
            }

            return View(cows);
        }

        public async Task<IActionResult> Create()
        {
            var herds = await _context.Herds.ToListAsync();
            ViewBag.Herds = new SelectList(herds, "Id", "Id");

            // Diagnostyka: sprawdź, czy ViewBag.Herds zawiera dane
            if (herds == null || !herds.Any())
            {
                ViewBag.HerdsError = "No herds available.";
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Idherd,Comment")] Cow cows)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cows);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var herds = await _context.Herds.ToListAsync();
            ViewBag.Herds = new SelectList(herds, "Id", "Id", cows.Idherd);

            return View(cows);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var herds = await _context.Herds.ToListAsync();
            ViewBag.Herds = new SelectList(herds, "Id", "Id");
            if (id == null)
            {
                return NotFound();
            }

            var cows = await _context.Cows.FindAsync(id);
            if (cows == null)
            {
                return NotFound();
            }
            return View(cows);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Idherd,Comment")] Cow cows)
        {
            if (id != cows.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cows);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CowExists(cows.Id))
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
            var herds = await _context.Herds.ToListAsync();
            ViewBag.Herds = new SelectList(herds, "Id", "Id", cows.Idherd);
            return View(cows);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cows = await _context.Cows
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cows == null)
            {
                return NotFound();
            }

            return View(cows);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cows = await _context.Cows.FindAsync(id);
            if (cows != null)
            {
                _context.Cows.Remove(cows);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CowExists(int id)
        {
            return _context.Cows.Any(e => e.Id == id);
        }
        public async Task<IActionResult> Diag(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cow = await _context.Cows
                .Include(c => c.IdherdNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cow == null)
            {
                return NotFound();
            }

            var diagnoses = await _context.Diagnoses
                .Where(d => d.Idcow == id)
                .Include(d => d.IddiseaseNavigation)
                .ToListAsync();

            var viewModel = new CowDiag
            {
                Cow = cow,
                Diagnoses = diagnoses
            };

            return View(viewModel);
        }
        public async Task<IActionResult> DiagAdd(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cow = await _context.Cows.FindAsync(id);
            if (cow == null)
            {
                return NotFound();
            }

            var diseases = await _context.Diseases.ToListAsync();
            var viewModel = new CowDiagAdd
            {
                CowId = cow.Id,
                CowName = cow.Name,
                Diseases = diseases
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DiagAdd(CowDiagAdd model)
        {
            if (ModelState.IsValid)
            {
                var diagnosis = new Diagnosis
                {
                    Idcow = model.CowId,
                    Iddisease = model.SelectedDiseaseId,
                    NameOfDisease = _context.Diseases.FirstOrDefault(d => d.Id == model.SelectedDiseaseId)?.Name,
                    Comment = model.Comment
                };

                try
                {
                    _context.Add(diagnosis);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Diag", "Cow", new { id = model.CowId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Wystąpił błąd podczas dodawania diagnozy.");
                }
            }

            model.Diseases = await _context.Diseases.ToListAsync();
            return View(model);
        }

    }
}
