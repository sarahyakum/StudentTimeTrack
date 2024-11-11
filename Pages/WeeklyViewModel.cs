using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System;
using System.Data;

public class WeeklyViewModel : PageModel
{
    public List<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();
    public string TotalTime { get; set; }

    [BindProperty]
    public string NewTime { get; set; }

    [BindProperty]
    public string NewDescription { get; set; }

    [BindProperty]
    public DateTime NewDate { get; set; }

    public void OnGet()
    {
        LoadCurrentWeekTimeSlots();
    }

    public IActionResult OnPostAddTimeSlot()
    {
        // Get the student NetID from the session or other sources
        string stuNetID = HttpContext.Session.GetString("StudentNetId");
        string errorMessage;

        // Call the stored procedure to insert the time slot
        using (var connection = new MySqlConnection("server=127.0.0.1;user=root;password=Kiav@z1208;database=seniordesignproject;"))
        {
            connection.Open();

            using (var cmd = new MySqlCommand("student_insert_timeslot", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@student_netID", stuNetID);
                cmd.Parameters.AddWithValue("@ts_date", NewDate);
                cmd.Parameters.AddWithValue("@ts_description", NewDescription);
                cmd.Parameters.AddWithValue("@ts_duration", NewTime); // Assume NewTime is in "HH:MM" format

                MySqlParameter outParameter = new MySqlParameter("@error_message", MySqlDbType.VarChar, 100)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outParameter);

                cmd.ExecuteNonQuery();

                // Get the output parameter (error message or success)
                errorMessage = outParameter.Value.ToString();
            }
        }

        if (errorMessage == "Success")
        {
            // Reload the time slots after successfully adding a new one
            LoadCurrentWeekTimeSlots();
            return RedirectToPage();  // Reload the page after success
        }
        else
        {
            // Handle error (e.g., show message to user)
            ModelState.AddModelError(string.Empty, errorMessage);
            return Page(); // Keep the user on the same page if an error occurs
        }
    }

    private void LoadCurrentWeekTimeSlots()
    {
        string connectionString = "server=127.0.0.1;user=root;password=Kiav@z1208;database=seniordesignproject;";
        string stuNetID = HttpContext.Session.GetString("StudentNetId");

        if (string.IsNullOrEmpty(stuNetID))
        {
            Console.WriteLine("Error: StudentNetId not found in session.");
            return;
        }

        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            DateTime startDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1);
            DateTime endDate = startDate.AddDays(6);

            using (var cmd = new MySqlCommand("student_timeslot_by_week", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@stu_netID", stuNetID);
                cmd.Parameters.AddWithValue("@start_date", startDate);

                using (var reader = cmd.ExecuteReader())
                {
                    Console.WriteLine("Executing stored procedure: student_timeslot_by_week for current week with stuNetID: " + stuNetID);

                    while (reader.Read())
                    {
                        string durationString = reader.GetString(4);
                        string[] timeParts = durationString.Split(':');
                        int hours = int.Parse(timeParts[0]);
                        int minutes = int.Parse(timeParts[1]);
                        int totalMinutes = (hours * 60) + minutes;

                        TimeSlot timeSlot = new TimeSlot
                        {
                            StuName = reader.GetString(1),
                            TSDate = reader.GetDateTime(2),
                            TSDescription = reader.GetString(3),
                            TSDuration = totalMinutes
                        };

                        TimeSlots.Add(timeSlot);
                    }
                }
            }

            // Call the stored procedure for getting total student time
            using (var cmd = new MySqlCommand("student_time_in_range", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // Parameters for the stored procedure
                cmd.Parameters.AddWithValue("@student_netID", stuNetID);
                cmd.Parameters.AddWithValue("@startDate", startDate);
                cmd.Parameters.AddWithValue("@endDate", endDate);

                // Variable to hold status or error message
                var statusParam = new MySqlParameter("@student_total", MySqlDbType.VarChar, 255);
                statusParam.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(statusParam);

                cmd.ExecuteNonQuery();

                // Capture the status message from the output parameter
                int totalMinutes = int.Parse(statusParam.Value.ToString());

                // Convert total minutes to HH:MM format
                int hours = totalMinutes / 60;
                int minutes = totalMinutes % 60;
                TotalTime = $"{hours:D2}:{minutes:D2}";  // Formats with leading zeroes if needed

                Console.WriteLine("Total(HH:MM): " + TotalTime);
            }
        }


        
    }
}
