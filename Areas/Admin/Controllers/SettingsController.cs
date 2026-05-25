using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetAdoptionManagementSystem.Data;
using PetAdoptionManagementSystem.Models.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetAdoptionManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var settings = await _context.SiteSettings.ToListAsync();
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(Dictionary<string, string> settings)
        {
            if (settings != null)
            {
                var existingSettings = await _context.SiteSettings.ToListAsync();
                foreach (var item in settings)
                {
                    var setting = existingSettings.FirstOrDefault(s => s.Key == item.Key);
                    if (setting != null)
                    {
                        setting.Value = item.Value;
                    }
                    else
                    {
                        _context.SiteSettings.Add(new SiteSetting { Key = item.Key, Value = item.Value });
                    }
                }
                await _context.SaveChangesAsync();
                TempData["Success"] = "Settings updated successfully.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}