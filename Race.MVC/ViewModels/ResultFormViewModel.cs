using System.ComponentModel.DataAnnotations;

namespace Race.MVC.ViewModels
{
    // One row on the Results entry page (create a result, or update an existing one)
    public class ResultFormViewModel
    {
        public int EventID { get; set; }
        public int EnrolmentID { get; set; }
        public int? ResultID { get; set; }

        [Range(0, 23, ErrorMessage = "Hours must be between 0 and 23.")]
        public int Hours { get; set; }

        [Range(0, 59, ErrorMessage = "Minutes must be between 0 and 59.")]
        public int Minutes { get; set; }

        [Range(0, 59, ErrorMessage = "Seconds must be between 0 and 59.")]
        public int Seconds { get; set; }

        [Range(1, 1000000, ErrorMessage = "Finishing position must be 1 or higher.")]
        public int Position { get; set; }

        public TimeSpan FinishingTime => new TimeSpan(Hours, Minutes, Seconds);
    }
}
