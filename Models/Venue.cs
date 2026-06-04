using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Venue
    {
        public int VenueId { get; set; }

        [Required(ErrorMessage = "Venue name is required.")]
        [StringLength(100, ErrorMessage = "Venue name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(150, ErrorMessage = "Location cannot exceed 150 characters.")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Capacity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than 0.")]
        public int Capacity { get; set; }

        // ✅ KEEP THIS (Azurite will store URL here)
        public string? ImageUrl { get; set; }

        // ✅ ONE VENUE → MANY EVENTS
        public ICollection<Event>? Events { get; set; }

        // ✅ ADD THIS (VERY IMPORTANT)
        public ICollection<Booking>? Bookings { get; set; }
    }
}