using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetAdoptionManagementSystem.Data;
using PetAdoptionManagementSystem.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace PetAdoptionManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new DashboardViewModel
            {
                TotalPets = await _context.Pets.CountAsync(p => !p.IsDeleted),
                AvailablePets = await _context.Pets.CountAsync(p => p.Status == Models.Domain.PetStatus.Available && !p.IsDeleted),
                AdoptedPets = await _context.Pets.CountAsync(p => p.Status == Models.Domain.PetStatus.Adopted && !p.IsDeleted),
                PendingApplications = await _context.AdoptionApplications.CountAsync(a => a.Status == Models.Domain.ApplicationStatus.Pending),
                TotalMembers = await _context.Users.CountAsync()
            };

            var petsBySpecies = await _context.Pets
                .Where(p => !p.IsDeleted && p.Species != null)
                .GroupBy(p => p.Species)
                .Select(g => new { Species = g.Key, Count = g.Count() })
                .ToListAsync();
            
            foreach (var item in petsBySpecies)
            {
                vm.PetsBySpecies[item.Species!] = item.Count;
            }

            var appsByStatus = await _context.AdoptionApplications
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                .ToListAsync();

            foreach (var item in appsByStatus)
            {
                vm.ApplicationsByStatus[item.Status] = item.Count;
            }

            vm.RecentApplications = await _context.AdoptionApplications
                .Include(a => a.Applicant)
                .Include(a => a.Pet)
                .OrderByDescending(a => a.SubmittedAt)
                .Take(10)
                .ToListAsync();

            return View(vm);
        }
    }
}