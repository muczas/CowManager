﻿using CowManagerApp.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CowManagerApp.MVC.Controllers
{
    public class MedicineController : Controller
    {
        private readonly CowManagerContext _context;
        public MedicineController(CowManagerContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            if (_context.Medicines == null)
            {
                return Problem("Entity set 'ApiContext.Movie'  is null.");
            }

            var meds = _context.Medicines;

            return View(await meds.ToListAsync());
        }
        public async Task<IActionResult> MedicineDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var meds = await _context.Medicines
                .FirstOrDefaultAsync(m => m.Id == id);
            if (meds == null)
            {
                return NotFound();
            }

            return View(meds);
        }
    }
}
