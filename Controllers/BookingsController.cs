using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;

namespace EventEase.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // LIST BOOKINGS (WITH SEARCH)
        // =========================
        public async Task<IActionResult> Index(string? searchTerm)
        {
            ViewBag.SearchTerm = searchTerm;

            var query = _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .Select(b => new BookingDetailsView
                {
                    BookingId = b.BookingId,
                    EventName = b.Event.Name,
                    EventDate = b.Event.EventDate,
                    VenueName = b.Venue.Name,
                    VenueLocation = b.Venue.Location,
                    VenueImageUrl = b.Venue.ImageUrl,
                    BookingDate = b.BookingDate
                });

            // Search by BookingId or Event Name
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                bool isId = int.TryParse(searchTerm, out int bookingId);
                query = isId
                    ? query.Where(b => b.BookingId == bookingId)
                    : query.Where(b => b.EventName.Contains(searchTerm));
            }

            return View(await query.ToListAsync());
        }

        // =========================
        // DETAILS
        // =========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // =========================
        // CREATE (GET)
        // =========================
        public IActionResult Create()
        {
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Name");
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Name");
            return View();
        }

        // =========================
        // CREATE (POST) - WITH DOUBLE BOOKING VALIDATION
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            ModelState.Remove("Event");
            ModelState.Remove("Venue");

            if (ModelState.IsValid)
            {
                // Get the event date for the selected event
                var selectedEvent = await _context.Events
                    .FirstOrDefaultAsync(e => e.EventId == booking.EventId);

                if (selectedEvent == null)
                {
                    ModelState.AddModelError("", "Selected event not found.");
                    ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Name", booking.EventId);
                    ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Name", booking.VenueId);
                    return View(booking);
                }

                // Check for double booking - same venue on the same event date
                bool doubleBooking = await _context.Bookings
                    .Include(b => b.Event)
                    .AnyAsync(b =>
                        b.VenueId == booking.VenueId &&
                        b.Event.EventDate.Date == selectedEvent.EventDate.Date);

                if (doubleBooking)
                {
                    TempData["Error"] = "This venue is already booked for that date. Please choose a different venue or date.";
                    ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Name", booking.EventId);
                    ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Name", booking.VenueId);
                    return View(booking);
                }

                try
                {
                    _context.Add(booking);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Booking created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    TempData["Error"] = "Something went wrong while saving the booking.";
                }
            }

            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Name", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Name", booking.VenueId);
            return View(booking);
        }

        // =========================
        // DELETE (GET)
        // =========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // =========================
        // DELETE (POST)
        // =========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking == null) return NotFound();

            try
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Booking deleted successfully!";
            }
            catch (Exception)
            {
                TempData["Error"] = "Error deleting booking.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}