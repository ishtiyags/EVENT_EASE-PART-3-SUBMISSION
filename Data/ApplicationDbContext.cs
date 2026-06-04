using Microsoft.EntityFrameworkCore;
using EventEase.Models;

namespace EventEase.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Event> Events { get; set; }
        public DbSet<Venue> Venues { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<EventType> EventTypes { get; set; }

        // 👇 ADD THIS FOR VIEW SUPPORT
        public DbSet<BookingDetailsView> BookingDetailsView { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ RELATIONSHIP: Event -> Venue
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Venue)
                .WithMany(v => v.Events)
                .HasForeignKey(e => e.VenueId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ RELATIONSHIP: Booking -> Event
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Event)
                .WithMany(e => e.Bookings)
                .HasForeignKey(b => b.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ RELATIONSHIP: Booking -> Venue
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Venue)
                .WithMany(v => v.Bookings)
                .HasForeignKey(b => b.VenueId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ MAP SQL VIEW (NO PRIMARY KEY)
            modelBuilder.Entity<BookingDetailsView>()
                .HasNoKey()
                .ToView("BookingDetailsView");

            // ✅ SEED EVENT TYPES
            modelBuilder.Entity<EventType>().HasData(

                new EventType
                {
                    EventTypeId = 1,
                    Name = "Conference"
                },

                new EventType
                {
                    EventTypeId = 2,
                    Name = "Wedding"
                },

                new EventType
                {
                    EventTypeId = 3,
                    Name = "Concert"
                },

                new EventType
                {
                    EventTypeId = 4,
                    Name = "Sports"
                },

                new EventType
                {
                    EventTypeId = 5,
                    Name = "Corporate"
                }

            );
        }
    }
    
}
