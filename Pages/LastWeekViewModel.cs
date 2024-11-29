// Written by Kiara Vaz for CS4385.0W1, Senior Design Project, Started October 3, 2024
// Net ID: KMV200000
// Purpose: Manage last week's time tracking data for students, allowing them to view, add, 
// and edit time slots for the current week. It connects to a MySQL database 
// to fetch, add, and modify data using stored procedures.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Collections.Generic;

public class LastWeekViewModel : PageModel
{
    // List to hold time slots fetched from the database.
    public List<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();
    
    // Total time logged in the previous week.
    public string TotalTime { get; set; }

    // Error message to display if an operation fails.
    public string ErrorMessage { get; set; }

    // Properties to bind input data for adding or editing time slots.
    [BindProperty]
    public string AddTime { get; set; }

    [BindProperty]
    public string AddDescription { get; set; }

    [BindProperty]
    public DateTime SelectedDate { get; set; } // Date selected from the calendar for new time slot.

    [BindProperty]
    public string EditTime { get; set; }

    [BindProperty]
    public string EditDescription { get; set; }

    [BindProperty]
    public DateTime EditSelectedDate { get; set; }

    // Loads time slots from the previous week when the page is accessed.
    public void OnGet()
    {
        LoadLastWeekTimeSlots();
    }

    // Add a time slot to the database.
    // Inputs:SelectedDate, AddTime, AddDescription: The description of the time slot (string)
    // Outputs: 
    // - Adds a time slot to the database using a stored procedure.
    // - Displays the status message (success or error) to the view.
    // - Reloads the time slots after adding.
    public void OnPostAddTimeSlot(string SelectedDate, string AddTime, string AddDescription)
    {
        string connectionString = "server=127.0.0.1;user=root;password=Kiav@z1208;database=seniordesignproject;";
        
        // Retrieves the student's NetID from the session.
        string stuNetID = HttpContext.Session.GetString("StudentNetId");
    
        // If NetID is not found, log the error and return.
        if (string.IsNullOrEmpty(stuNetID))
        {
            Console.WriteLine("Error: StudentNetId not found in session.");
            return;
        }
    
        // Default status message.
        string statusMessage = "Success"; 

        // Open connection to MySQL database.
        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            // Create command to call stored procedure for adding a time slot.
            using (var cmd = new MySqlCommand("student_insert_timeslot", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // Add parameters for student NetID, time slot date, description, and duration.
                cmd.Parameters.AddWithValue("@student_netID", stuNetID);
                cmd.Parameters.AddWithValue("@ts_date", DateTime.Parse(SelectedDate));
                cmd.Parameters.AddWithValue("@ts_description", AddDescription);
                cmd.Parameters.AddWithValue("@ts_duration", AddTime);

                // Add output parameter for error message.
                var statusParam = new MySqlParameter("@error_message", MySqlDbType.VarChar, 255);
                statusParam.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(statusParam);

                // Execute the stored procedure.
                cmd.ExecuteNonQuery();
                statusMessage = statusParam.Value.ToString();
            }
        }

        // Display any error message from the operation.
        ViewData["ErrorMessage"] = statusMessage;

        // Reload the time slots for the page.
        LoadLastWeekTimeSlots();
    }


    // Edit an existing time slot in the database.
    // Inputs: SelectedDate, UpdatedTime, UpdatedDescription
    // Outputs:
    // - Edits the time slot in the database using a stored procedure.
    // - Displays the status message (success or error) to the view.
    // - Reloads the time slots after editing.
    public void OnPostEditTimeSlot(string SelectedDate, string EditTime, string EditDescription)
    {
        string connectionString = "server=127.0.0.1;user=root;password=Kiav@z1208;database=seniordesignproject;";
        string stuNetID = HttpContext.Session.GetString("StudentNetId");

        // Validate if the student ID is null or empty
        if (string.IsNullOrEmpty(stuNetID))
        {
            Console.WriteLine("Error: StudentNetId not found in session.");
            return;
        }

        string statusMessage = "Success";

        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            // Call the stored procedure for editing a time slot
            using (var cmd = new MySqlCommand("student_edit_timeslot", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // Add parameters for student NetID, updated time slot details.
                cmd.Parameters.AddWithValue("@student_netID", stuNetID);
                cmd.Parameters.AddWithValue("@ts_date", DateTime.Parse(SelectedDate));
                cmd.Parameters.AddWithValue("@updated_description", EditDescription);
                cmd.Parameters.AddWithValue("@updated_duration", EditTime);

                var statusParam = new MySqlParameter("@error_message", MySqlDbType.VarChar, 255);
                statusParam.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(statusParam);

                // Execute the stored procedure.
                cmd.ExecuteNonQuery();
                statusMessage = statusParam.Value.ToString();
            }
        }

        // Display the error message if any.
        ViewData["ErrorMessage"] = statusMessage;

        // Reload the time slots for the page.
        LoadLastWeekTimeSlots();
    }

    // Method to load all time slots for the current week.
    // Inputs: None (uses the session to retrieve the student NetID)
    // Outputs:
    // - Retrieves the list of time slots for the current week from the database
    // - Populates the `TimeSlots` property with the time slot details
    // Method to load the current week's time slots and total time for a student.
    private void LoadLastWeekTimeSlots()
    {
        // Connection string for the MySQL database
        string connectionString = "server=127.0.0.1;user=root;password=Kiav@z1208;database=seniordesignproject;";

        // Retrieve the student's NetID from the session (used for identifying the student in the database)
        string stuNetID = HttpContext.Session.GetString("StudentNetId");

        // Check if the student NetID is null or empty, if so, log an error and return early
        if (string.IsNullOrEmpty(stuNetID))
        {
            Console.WriteLine("Error: StudentNetId not found in session.");
            return;
        }

        // Establish a connection to the database using the provided connection string
        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open(); // Open the connection to the database

            // Calculate the start and end dates for the previous week.
            DateTime endDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek - 1); // Sunday
            DateTime startDate = endDate.AddDays(-6); // Monday of the previous week

            // Retrieve time slots from the previous week using the stored procedure.
            using (var cmd = new MySqlCommand("student_timeslot_by_week", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@stu_netID", stuNetID);
                cmd.Parameters.AddWithValue("@start_date", startDate);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // Convert the duration from string (HH:MM) to total minutes.
                        string durationString = reader.GetString(4);
                        string[] timeParts = durationString.Split(':');
                        int hours = int.Parse(timeParts[0]);
                        int minutes = int.Parse(timeParts[1]);
                        int totalMinutes = (hours * 60) + minutes;

                        // Add the time slot to the list.
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

            // Retrieve the total time spent during the week and convert to HH:MM format.
            using (var cmd = new MySqlCommand("student_time_in_range", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // Add parameters for the stored procedure
                cmd.Parameters.AddWithValue("@student_netID", stuNetID);
                cmd.Parameters.AddWithValue("@startDate", startDate);
                cmd.Parameters.AddWithValue("@endDate", endDate);

                // Define a parameter to hold the output for the total time in minutes
                var statusParam = new MySqlParameter("@student_total", MySqlDbType.VarChar, 255);
                statusParam.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(statusParam);

                cmd.ExecuteNonQuery();

                // Capture the result (total time in minutes) from the output parameter
                int totalMinutes = int.Parse(statusParam.Value.ToString());

                // Convert total minutes to hours and minutes
                int hours = totalMinutes / 60;
                int minutes = totalMinutes % 60;

                // Format the total time as HH:MM and assign to the TotalTime property
                TotalTime = $"{hours:D2}:{minutes:D2}";  

                // Log the total time in the console for debugging purposes
                Console.WriteLine("Total(HH:MM): " + TotalTime);
            }
        }
    }
}
