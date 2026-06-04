using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EventEase.Models
{
    public class Event
    {
        public int EventId { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        public string? ImageUrl { get; set; }

        [Required]
        public int VenueId { get; set; }

        // 1. Venue must be nullable
        public Venue? Venue { get; set; }

        // 2. Bookings must be initialized (not null)
        [JsonIgnore]
        public List<Booking> Bookings { get; set; } = new List<Booking>();

        public int EventTypeId { get; set; }
        public EventType? EventType { get; set; }
    }
}
