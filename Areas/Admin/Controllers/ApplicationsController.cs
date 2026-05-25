using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetAdoptionManagementSystem.Data;
using PetAdoptionManagementSystem.Models.Domain;
using PetAdoptionManagementSystem.Models.ViewModels;
using PetAdoptionManagementSystem.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PetAdoptionManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ApplicationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public ApplicationsController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index(ApplicationStatus? statusFilter, int page = 1)
        {
            int pageSize = 10;
            var query = _context.AdoptionApplications.Include(a => a.Pet).Include(a => a.Applicant).AsQueryable();

            if (statusFilter.HasValue)
            {
                query = query.Where(a => a.Status == statusFilter.Value);
            }

            var totalItems = await query.CountAsync();
            var applications = await query.OrderByDescending(a => a.SubmittedAt)
                                          .Skip((page - 1) * pageSize)
                                          .Take(pageSize)
                                          .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.StatusFilter = statusFilter;

            return View(applications);
        }

        public async Task<IActionResult> Details(int id)
        {
            var application = await _context.AdoptionApplications
                .Include(a => a.Pet)
                .Include(a => a.Applicant)
                .FirstOrDefaultAsync(a => a.Id == id);
                
            if (application == null) return NotFound();

            var vm = new ReviewApplicationViewModel
            {
                ApplicationId = application.Id,
                AdminRemarks = application.AdminRemarks,
                NewStatus = application.Status
            };

            ViewBag.Application = application;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(ReviewApplicationViewModel model)
        {
            var application = await _context.AdoptionApplications
                .Include(a => a.Pet)
                .Include(a => a.Applicant)
                .FirstOrDefaultAsync(a => a.Id == model.ApplicationId);

            if (application == null) return NotFound();

            application.Status = model.NewStatus;
            application.AdminRemarks = model.AdminRemarks;
            application.ReviewedAt = DateTime.UtcNow;

            if (model.NewStatus == ApplicationStatus.Approved)
            {
                if(application.Pet != null) application.Pet.Status = PetStatus.Adopted;
                // Auto-reject other pending apps for this pet? Skipping for MVP simplicity unless strict.
            }

            await _context.SaveChangesAsync();

            if (application.Applicant != null)
            {
                string subject = $"Update on your adoption application for {application.Pet?.Name}";
                string body = $"<p>Hi {application.Applicant.FullName},</p><p>Your application status is now: <strong>{model.NewStatus}</strong>.</p>";
                if (!string.IsNullOrEmpty(model.AdminRemarks)) 
                {
                     body += $"<p>Admin Remarks: {model.AdminRemarks}</p>";
                }
                await _emailService.SendEmailAsync(application.Applicant.Email!, subject, body);
            }

            TempData["Success"] = "Application reviewed and applicant notified.";
            return RedirectToAction(nameof(Index));
        }
    }
}