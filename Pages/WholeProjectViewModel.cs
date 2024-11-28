using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

public class WholeProjectViewModel : PageModel
{
    public string TotalTime { get; set; }
    public int StartMonth { get; set; }
    public int EndMonth { get; set; }
    public int SelectedMonth { get; set; } = DateTime.Today.Month;
    public int SelectedYear { get; set; } = DateTime.Today.Year;
    public List<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();

    // Added properties for Start and End Dates of the project
    public DateTime ProjectStartDate { get; set; }
    public DateTime ProjectEndDate { get; set; }

    public void OnGet(int month = 0, int year = 0, string change = null)
    {
        // Update month and year based on navigation
        if (change == "prev")
        {
            if (month == 1)
            {
                SelectedMonth = 12;
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
            if (month == 12)
            {
                SelectedMonth = 1;
                SelectedYear = year + 1;
            }
            else
            {
                SelectedMonth = month + 1;
                SelectedYear = year;
            }
        }
        else
        {
            SelectedMonth = month == 0 ? DateTime.Today.Month : month;
            SelectedYear = year == 0 ? DateTime.Today.Year : year;
        }

        LoadTimeSlots(); // Call the method to load time slots
        GetTimeFrame();  // Get the start and end dates of the project
    }

    private void GetTimeFrame()
    {
        string connectionString = "server=127.0.0.1;user=root;password=Kiav@z1208;database=seniordesignproject;"; // Update as needed

        // Retrieve stuNetID from the session
        string stuNetID = HttpContext.Session.GetString("StudentNetId");

        // Check if stuNetID is null or empty
        if (string.IsNullOrEmpty(stuNetID))
        {
            // Log that stuNetID was not found
            Console.WriteLine("Error: StudentNetId not found in session.");
            return; // Exit the method early
        }

        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            // Get the student's section code
            string sectionCode = null;
            using (var cmd = new MySqlCommand("SELECT SecCode FROM MemberOf WHERE StuNetID = @StuNetID", connection))
            {
                cmd.Parameters.AddWithValue("@StuNetID", stuNetID);

                var result = cmd.ExecuteScalar();
                sectionCode = result?.ToString();
            }

            if (string.IsNullOrEmpty(sectionCode))
            {
                Console.WriteLine("Error: Section code not found for the student.");
                return; // Exit if no section code found
            }

            // Now retrieve the project start and end dates using the section code
            using (var cmd = new MySqlCommand("get_section_timeframe", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@section_code", sectionCode);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        ProjectStartDate = reader.GetDateTime(0); // StartDate
                        ProjectEndDate = reader.GetDateTime(1);   // EndDate
                    }
                    else
                    {
                        Console.WriteLine("Error: No timeframe found for the section code.");
                    }
                }
            }
        }
    }

    private void LoadTimeSlots()
    {
        string connectionString = "server=127.0.0.1;user=root;password=Kiav@z1208;database=seniordesignproject;"; // Update as needed

        // Retrieve stuNetID from the session
        string stuNetID = HttpContext.Session.GetString("StudentNetId");

        // Check if stuNetID is null or empty
        if (string.IsNullOrEmpty(stuNetID))
        {
            // Log that stuNetID was not found
            Console.WriteLine("Error: StudentNetId not found in session.");
            return; // Exit the method early
        }

        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            DateTime startDate = new DateTime(SelectedYear, SelectedMonth, 1);

            using (var cmd = new MySqlCommand("student_timeslot_by_month", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@stu_netID", stuNetID);
                cmd.Parameters.AddWithValue("@start_date", startDate);

                using (var reader = cmd.ExecuteReader())
                {
                    // Log a message to indicate the query has been executed
                    Console.WriteLine("Executing stored procedure: student_timeslot_by_month with stuNetID: " + stuNetID);

                    while (reader.Read())
                    {
                        string durationString = reader.GetString(4); // Change the index based on your actual schema
                        string descriptionString = reader.GetString(3);

                        // Parse the duration string "HH:MM" into total minutes
                        string[] timeParts = durationString.Split(':');
                        int hours = int.Parse(timeParts[0]);
                        int minutes = int.Parse(timeParts[1]);
                        int totalMinutes = (hours * 60) + minutes;

                        // Populate your model/list with this data
                        TimeSlot timeSlot = new TimeSlot
                        {
                            StuName = reader.GetString(1),
                            TSDate = reader.GetDateTime(2),
                            TSDescription = descriptionString,
                            TSDuration = totalMinutes // Store in minutes or use as needed
                        };

                        TimeSlots.Add(timeSlot); // Add to your model's time slots list
                    }
                }
            }

            // Call the stored procedure for getting total student time
            using (var cmd = new MySqlCommand("student_total_time", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // Parameters for the stored procedure
                cmd.Parameters.AddWithValue("@student_netID", stuNetID);

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

                Console.WriteLine("Total (HH:MM): " + TotalTime);
            }
        }
    }
}

public class TimeSlot
{
    public string StuNetID { get; set; }
    public string StuName { get; set; }
    public DateTime TSDate { get; set; }
    public string TSDescription { get; set; }
    public double TSDuration { get; set; }
}
