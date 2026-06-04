namespace EventEase.Models
{
    public class BookingDetailsView
    {
        public int BookingId { get; set; }
        public string EventName { get; set; }
        public string VenueName { get; set; }
        public string? VenueLocation { get; set; }
        public string? VenueImageUrl { get; set; }
        public int Capacity { get; set; }
        public int TicketsBooked { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime BookingDate { get; set; }
    }
}