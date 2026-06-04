using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;
using EventEase.Services;

namespace EventEase.Controllers
{
    public class VenuesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobStorageService _blobService;

        public VenuesController(ApplicationDbContext context, BlobStorageService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        // =========================
        // LIST VENUES
        // =========================
        public async Task<IActionResult> Index()
        {
            return View(await _context.Venues.ToListAsync());
        }

        // =========================
        // DETAILS
        // =========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venues
                .FirstOrDefaultAsync(m => m.VenueId == id);

            if (venue == null) return NotFound();

            return View(venue);
        }

        // =========================
        // CREATE (GET)
        // =========================
        public IActionResult Create()
        {
            return View();
        }

        // =========================
        // CREATE (POST) - WITH IMAGE UPLOAD
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Venue venue, IFormFile? imageFile)
        {
            ModelState.Remove("Bookings");
            ModelState.Remove("Events");
            ModelState.Remove("ImageUrl");

            try
            {
                if (ModelState.IsValid)
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        venue.ImageUrl = await _blobService.UploadImageAsync(imageFile);
                    }

                    _context.Add(venue);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Venue created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Show exactly which fields failed validation
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    TempData["Error"] = "Validation failed: " + string.Join(" | ", errors);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.InnerException != null
                    ? ex.InnerException.Message
                    : ex.Message;
            }

            return View(venue);
        }

        // =========================
        // EDIT (GET)
        // =========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venues.FindAsync(id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        // =========================
        // EDIT (POST) - WITH IMAGE UPLOAD
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Venue venue, IFormFile? imageFile)
        {
            if (id != venue.VenueId) return NotFound();

            ModelState.Remove("Bookings");
            ModelState.Remove("Events");
            ModelState.Remove("ImageUrl");

            try
            {
                if (ModelState.IsValid)
                {
                    // Upload new image if a new file was selected
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Delete old image from blob storage first
                        if (!string.IsNullOrEmpty(venue.ImageUrl))
                        {
                            await _blobService.DeleteImageAsync(venue.ImageUrl);
                        }

                        venue.ImageUrl = await _blobService.UploadImageAsync(imageFile);
                    }

                    _context.Update(venue);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Venue updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Error updating venue.";
            }

            return View(venue);
        }

        // =========================
        // DELETE (GET)
        // =========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venues
                .FirstOrDefaultAsync(m => m.VenueId == id);

            if (venue == null) return NotFound();

            return View(venue);
        }

        // =========================
        // DELETE (POST) - WITH BOOKING VALIDATION
        // =========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venues.FindAsync(id);

            if (venue == null) return NotFound();

            // Prevent delete if active bookings exist
            var hasBookings = _context.Bookings.Any(b => b.VenueId == id);

            if (hasBookings)
            {
                TempData["Error"] = "Cannot delete venue with active bookings.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Delete image from blob storage if it exists
                if (!string.IsNullOrEmpty(venue.ImageUrl))
                {
                    await _blobService.DeleteImageAsync(venue.ImageUrl);
                }

                _context.Venues.Remove(venue);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Venue deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error creating venue: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}