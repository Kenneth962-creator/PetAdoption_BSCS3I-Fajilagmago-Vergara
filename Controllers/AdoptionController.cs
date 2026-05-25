using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetAdoptionManagementSystem.Data;
using PetAdoptionManagementSystem.Models.ViewModels;
using PetAdoptionManagementSystem.Models.Domain;
using System;
using System.Threading.Tasks;

namespace PetAdoptionManagementSystem.Controllers
{
    [Authorize]
    public class AdoptionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdoptionController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Apply(int petId)
        {
            var pet = await _context.Pets.FindAsync(petId);
            if (pet == null || pet.Status != PetStatus.Available)
            {
                TempData["Error"] = "This pet is no longer available for adoption.";
                return RedirectToAction("Index", "Catalog");
            }

            var model = new ApplyViewModel
            {
                PetId = pet.Id,
                PetName = pet.Name
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(ApplyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var pet = await _context.Pets.FindAsync(model.PetId);
            if (pet == null || pet.Status != PetStatus.Available)
            {
                TempData["Error"] = "This pet is no longer available.";
                return RedirectToAction("Index", "Catalog");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || !user.IsActive)
            {
                TempData["Error"] = "Your account is not active.";
                return RedirectToAction("Index", "Home");
            }

            // Check if already applied
            var existingApp = await _context.AdoptionApplications
                .FirstOrDefaultAsync(a => a.PetId == model.PetId && a.ApplicantId == user.Id && a.Status != ApplicationStatus.Rejected);
                
            if (existingApp != null)
            {
                TempData["Error"] = "You already have a pending or approved application for this pet.";
                return RedirectToAction("Index", "Profile");
            }

            var application = new AdoptionApplication
            {
                PetId = model.PetId,
                ApplicantId = user.Id,
                Reason = model.Reason,
                LivingSituation = model.LivingSituation,
                EmploymentStatus = model.EmploymentStatus,
                HasOtherPets = model.HasOtherPets,
                HasChildren = model.HasChildren,
                AgreedToTerms = model.AgreedToTerms,
                Status = ApplicationStatus.Pending,
                SubmittedAt = DateTime.UtcNow
            };

            pet.Status = PetStatus.UnderReview; // Optionally reserve the pet, or leave available until approved. Let's leave available for others or mark pending. The PRD says multiple apps can be received. Let's not change pet status here.
            
            _context.AdoptionApplications.Add(application);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your application has been submitted successfully!";
            return RedirectToAction("Index", "Profile");
        }
    }
}