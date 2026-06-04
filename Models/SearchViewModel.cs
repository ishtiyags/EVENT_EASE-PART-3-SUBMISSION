using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventEase.Models
{
    public class SearchViewModel
    {
        public string SearchTerm { get; set; }

        public int? EventTypeId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public List<SelectListItem>? EventTypes { get; set; }

        public List<Event>? Results { get; set; }
    }
}