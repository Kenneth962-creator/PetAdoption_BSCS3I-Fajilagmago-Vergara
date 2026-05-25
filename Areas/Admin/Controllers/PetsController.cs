using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetAdoptionManagementSystem.Data;
using PetAdoptionManagementSystem.Models.Domain;
using PetAdoptionManagementSystem.Models.ViewModels;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PetAdoptionManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PetsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PetsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(string searchString, PetStatus? statusFilter, int page = 1)
        {
            int pageSize = 10;
            var query = _context.Pets.Where(p => !p.IsDeleted).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Name.Contains(searchString) || p.Species!.Contains(searchString));
            }

            if (statusFilter.HasValue)
            {
                query = query.Where(p => p.Status == statusFilter.Value);
            }

            var totalItems = await query.CountAsync();
            var pets = await query.OrderByDescending(p => p.CreatedAt)
                                  .Skip((page - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)System.Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.SearchString = searchString;
            ViewBag.StatusFilter = statusFilter;

            return View(pets);
        }

        public IActionResult Create()
        {
            return View(new PetFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PetFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                var pet = new Pet
                {
                    Name = model.Name,
                    Species = model.Species,
                    Breed = model.Breed,
                    AgeYears = model.AgeYears,
                    Gender = model.Gender,
                    Description = model.Description,
                    HealthStatus = model.HealthStatus,
                    IsVaccinated = model.IsVaccinated,
                    AdoptionFee = model.AdoptionFee,
                    Status = model.Status
                };

                if (model.Photo != null && model.Photo.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "pets");
                    Directory.CreateDirectory(uploadsFolder);
                    string uniqueFileName = System.Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Photo.CopyToAsync(fileStream);
                    }
                    pet.PhotoPath = "/uploads/pets/" + uniqueFileName;
                }

                _context.Pets.Add(pet);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Pet added successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var pet = await _context.Pets.FindAsync(id);
            if (pet == null || pet.IsDeleted) return NotFound();

            var vm = new PetFormViewModel
            {
                Id = pet.Id,
                Name = pet.Name,
                Species = pet.Species ?? string.Empty,
                Breed = pet.Breed,
                AgeYears = pet.AgeYears,
                Gender = pet.Gender ?? "Unknown",
                Description = pet.Description,
                HealthStatus = pet.HealthStatus,
                IsVaccinated = pet.IsVaccinated,
                AdoptionFee = pet.AdoptionFee,
                Status = pet.Status,
                ExistingPhotoPath = pet.PhotoPath
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PetFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                var pet = await _context.Pets.FindAsync(model.Id);
                if (pet == null || pet.IsDeleted) return NotFound();

                pet.Name = model.Name;
                pet.Species = model.Species;
                pet.Breed = model.Breed;
                pet.AgeYears = model.AgeYears;
                pet.Gender = model.Gender;
                pet.Description = model.Description;
                pet.HealthStatus = model.HealthStatus;
                pet.IsVaccinated = model.IsVaccinated;
                pet.AdoptionFee = model.AdoptionFee;
                pet.Status = model.Status;
                pet.UpdatedAt = System.DateTime.UtcNow;

                if (model.Photo != null && model.Photo.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "pets");
                    Directory.CreateDirectory(uploadsFolder);
                    string uniqueFileName = System.Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Photo.CopyToAsync(fileStream);
                    }
                    pet.PhotoPath = "/uploads/pets/" + uniqueFileName;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Pet updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var pet = await _context.Pets.FindAsync(id);
            if (pet != null)
            {
                pet.IsDeleted = true;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Pet deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}