using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

public class LastWeekViewModel : PageModel
{
    public List<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();

    public void OnGet()
    {
        LoadLastWeekTimeSlots();
    }

    private void LoadLastWeekTimeSlots()
    {
        string connectionString = "server=127.0.0.1;user=root;password=Kiav@z1208;database=seniordesignproject;"; // Update as needed

        // Retrieve stuNetID from the session
        string stuNetID = HttpContext.Session.GetString("StudentNetId");

        // Check if stuNetID is null or empty
        if (string.IsNullOrEmpty(stuNetID))
        {
            Console.WriteLine("Error: StudentNetId not found in session.");
            return;
        }

        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            // Calculate the dates for the last week
            DateTime endDate = DateTime.Today.AddDays(-((int)DateTime.Today.DayOfWeek + 1));
            DateTime startDate = endDate.AddDays(-6);

            using (var cmd = new MySqlCommand("student_timeslot_by_week", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@stu_netID", stuNetID);
                cmd.Parameters.AddWithValue("@start_date", startDate);

                using (var reader = cmd.ExecuteReader())
                {
                    Console.WriteLine("Executing stored procedure: student_timeslot_by_week for last week with stuNetID: " + stuNetID);

                    while (reader.Read())
                    {
                        // Assuming columns for StuName, TSDate, TSDescription, and TSDuration are in the result set
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
        }
    }
}
