using System;
using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Booking
    {
        public int BookingId { get; set; }

        [Required]
        public string CustomerName { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Range(1, 1000)]
        public int NumberOfTickets { get; set; }

        // ✅ Auto-set to today's date when booking is created
        public DateTime BookingDate { get; set; } = DateTime.Now;

        // ✅ EVENT RELATION
        [Required]
        public int EventId { get; set; }
        public Event? Event { get; set; }

        // ✅ VENUE RELATION
        [Required]
        public int VenueId { get; set; }
        public Venue? Venue { get; set; }
    }
}