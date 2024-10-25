using Microsoft.AspNetCore.Mvc.RazorPages;

public class WholeProjectViewModel : PageModel
{
    public int SelectedMonth { get; set; } = DateTime.Today.Month;
    public int SelectedYear { get; set; } = DateTime.Today.Year;

    public void OnGet(int month = 0, int year = 0, string change = null)
    {
        // Update the month and year based on the user's navigation
        if (change == "prev")
        {
            if (month == 1) // January
            {
                SelectedMonth = 12; // December
                SelectedYear = year - 1;
            }
            else
            {
                SelectedMonth = month - 1;
                SelectedYear = year;
            }
        }
        else if (change == "next")
        {
            if (month == 12) // December
            {
                SelectedMonth = 1; // January
                SelectedYear = year + 1;
            }
            else
            {
                SelectedMonth = month + 1;
                SelectedYear = year;
            }
        }
        else // Default case, retain current month/year
        {
            SelectedMonth = month == 0 ? DateTime.Today.Month : month;
            SelectedYear = year == 0 ? DateTime.Today.Year : year;
        }
    }
}
