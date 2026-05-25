using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetAdoptionManagementSystem.Data;
using PetAdoptionManagementSystem.Models.Domain;
using System.Linq;
using System.Threading.Tasks;

namespace PetAdoptionManagementSystem.Controllers
{
    public class CatalogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CatalogController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string species = null)
        {
            var query = _context.Pets.Where(p => p.Status == PetStatus.Available).AsQueryable();

            if (!string.IsNullOrEmpty(species))
            {
                query = query.Where(p => p.Species == species);
            }

            ViewBag.Species = species;
            var pets = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
            return View(pets);
        }

        public async Task<IActionResult> Details(int id)
        {
            var pet = await _context.Pets.FindAsync(id);
            if (pet == null) return NotFound();

            return View(pet);
        }
    }
}