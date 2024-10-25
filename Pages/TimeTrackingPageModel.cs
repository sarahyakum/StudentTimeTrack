using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StudentWebApp.Pages
{
    public class TimeTrackingPageModel : PageModel
    {
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            // This is where you would normally populate the model
        }

        public void OnPost(string time, string description, string dateSelection)
        {
            // Handle form submission and add your logic here
            // For now, we'll just set a success message for demonstration
            SuccessMessage = "Time entry submitted successfully!";
        }
    }
}
