namespace Race.MVC.ViewModels
{
    // Values of the filter bar on the Home page
    public class EventFilterViewModel
    {
        public string? Search { get; set; }
        public string? EventType { get; set; }
        public DateTime? FromDate { get; set; }
        public decimal? MaxDistance { get; set; }

        public bool IsActive =>
            !string.IsNullOrWhiteSpace(Search) || !string.IsNullOrWhiteSpace(EventType)
            || FromDate.HasValue || MaxDistance.HasValue;
    }
}
