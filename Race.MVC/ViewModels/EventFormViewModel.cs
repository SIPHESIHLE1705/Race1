using System.ComponentModel.DataAnnotations;

namespace Race.MVC.ViewModels
{
    // Used by both Create Event and Edit Event
    public class EventFormViewModel
    {
        public static readonly string[] EventTypes = { "Run", "Walk", "Cycle" };

        public int EventID { get; set; }

        [Required(ErrorMessage = "Please enter the event name.")]
        [StringLength(100, ErrorMessage = "Event name can be at most 100 characters.")]
        [Display(Name = "Event name")]
        public string EventName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter a description.")]
        [StringLength(500, ErrorMessage = "Description can be at most 500 characters.")]
        [Display(Name = "Description")]
        public string EventDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the location.")]
        [StringLength(100, ErrorMessage = "Location can be at most 100 characters.")]
        [Display(Name = "Location")]
        public string EventLocation { get; set; } = string.Empty;

        [Range(0.01, 1000, ErrorMessage = "Distance must be between 0.01 and 1000 km.")]
        [Display(Name = "Distance (km)")]
        public decimal EventDistance { get; set; }

        [Required(ErrorMessage = "Please choose the event date.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date")]
        public DateTime? EventDate { get; set; }

        [Required(ErrorMessage = "Please choose the event type.")]
        [RegularExpression("Run|Walk|Cycle", ErrorMessage = "Event type must be Run, Walk or Cycle.")]
        [Display(Name = "Event type")]
        public string EventType { get; set; } = "Run";
    }
}
