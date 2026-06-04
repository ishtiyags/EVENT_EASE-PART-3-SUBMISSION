using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;

namespace EventEase.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // LIST + SEARCH + FILTERS
        // =========================
        public async Task<IActionResult> Index(string searchString, int? eventTypeId, DateTime? startDate, DateTime? endDate)
        {
            var eventsQuery = _context.Events
                .Include(e => e.Venue)
                .Include(e => e.EventType)
                .AsQueryable();

            // 🔎 SEARCH BY NAME
            if (!string.IsNullOrEmpty(searchString))
            {
                eventsQuery = eventsQuery.Where(e => e.Name.Contains(searchString));
            }

            // 📅 START DATE FILTER
            if (startDate.HasValue)
            {
                eventsQuery = eventsQuery.Where(e => e.EventDate >= startDate.Value);
            }

            // 📅 END DATE FILTER
            if (endDate.HasValue)
            {
                eventsQuery = eventsQuery.Where(e => e.EventDate <= endDate.Value);
            }

            // 🎭 EVENT TYPE FILTER
            if (eventTypeId.HasValue)
            {
                eventsQuery = eventsQuery.Where(e => e.EventTypeId == eventTypeId.Value);
            }

            // 🏢 VENUE AVAILABILITY FILTER (STEP 9)
            eventsQuery = eventsQuery.Where(e =>
                !_context.Bookings.Any(b =>
                    b.VenueId == e.VenueId &&
                    b.BookingDate.Date == e.EventDate.Date));

            var events = await eventsQuery.ToListAsync();

            // Dropdown for Event Types
            ViewData["EventTypeId"] = new SelectList(_context.EventTypes, "EventTypeId", "Name");

            return View(events);
        }

        // =========================
        // DETAILS
        // =========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var eventItem = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.EventType)
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (eventItem == null) return NotFound();

            return View(eventItem);
        }

        // =========================
        // CREATE (GET)
        // =========================
        public IActionResult Create()
        {
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Name");
            ViewData["EventTypeId"] = new SelectList(_context.EventTypes, "EventTypeId", "Name");
            return View();
        }

        // =========================
        // CREATE (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event @event)
        {
            if (ModelState.IsValid)
            {
                _context.Add(@event);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Name", @event.VenueId);
            ViewData["EventTypeId"] = new SelectList(_context.EventTypes, "EventTypeId", "Name", @event.EventTypeId);

            return View(@event);
        }

        // =========================
        // EDIT
        // =========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null) return NotFound();

            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Name", eventItem.VenueId);
            ViewData["EventTypeId"] = new SelectList(_context.EventTypes, "EventTypeId", "Name", eventItem.EventTypeId);

            return View(eventItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event @event)
        {
            if (id != @event.EventId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Events.Any(e => e.EventId == @event.EventId))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(@event);
        }

        // =========================
        // DELETE
        // =========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var eventItem = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.EventType)
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (eventItem == null) return NotFound();

            return View(eventItem);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);

            if (eventItem != null)
            {
                _context.Events.Remove(eventItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}