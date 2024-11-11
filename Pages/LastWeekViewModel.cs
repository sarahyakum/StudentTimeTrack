using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System;
using System.Data;
public class LastWeekViewModel : PageModel
{
    public List<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();

    public string TotalTime { get; set; }
    public string ErrorMessage { get; set; }

    // Properties for adding or editing time slots
    [BindProperty]
    public string AddTime { get; set; }
    [BindProperty]
    public string AddDescription { get; set; }
    [BindProperty]
    public DateTime SelectedDate { get; set; } // Selected date from the calendar

    public void OnGet()
    {
        LoadLastWeekTimeSlots();
    }

   public void OnPostAddTimeSlot(string SelectedDate, string AddTime, string AddDescription)
{
    string connectionString = "server=127.0.0.1;user=root;password=Kiav@z1208;database=seniordesignproject;"; // Update as needed
    string stuNetID = HttpContext.Session.GetString("StudentNetId");
    
    // Validate if the student ID is null or empty
    if (string.IsNullOrEmpty(stuNetID))
    {
        Console.WriteLine("Error: StudentNetId not found in session.");
        return;
    }
    
    string statusMessage = "Success";  // Default to success message

    using (var connection = new MySqlConnection(connectionString))
    {
        connection.Open();

        // Call the stored procedure for adding a time slot
        using (var cmd = new MySqlCommand("student_insert_timeslot", connection))
        {
            cmd.CommandType = CommandType.StoredProcedure;
            Console.WriteLine("Passing Parameters");

            // Parameters for the stored procedure
            cmd.Parameters.AddWithValue("@student_netID", stuNetID);
            cmd.Parameters.AddWithValue("@ts_date", DateTime.Parse(SelectedDate));
            cmd.Parameters.AddWithValue("@ts_description", AddDescription);
            cmd.Parameters.AddWithValue("@ts_duration", AddTime); // Assumed in HH:MM format

            // Variable to hold status or error message
            var statusParam = new MySqlParameter("@error_message", MySqlDbType.VarChar, 255);
            statusParam.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(statusParam);

            cmd.ExecuteNonQuery();
            
            // Capture the status message from the output parameter
            statusMessage = statusParam.Value.ToString();
            Console.WriteLine("Stored Procedure Status: " + statusMessage);
        }
    }

    // Pass the status message to the view (error message from stored procedure)
    ViewData["ErrorMessage"] = statusMessage;

    // Reload the time slots after the insertion
    LoadLastWeekTimeSlots();
}


    private void LoadLastWeekTimeSlots()
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

            DateTime endDate = DateTime.Today.AddDays(-((int)DateTime.Today.DayOfWeek + 1));
            DateTime startDate = endDate.AddDays(-6);

            using (var cmd = new MySqlCommand("student_timeslot_by_week", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@stu_netID", stuNetID);
                cmd.Parameters.AddWithValue("@start_date", startDate);

                using (var reader = cmd.ExecuteReader())
                {
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
