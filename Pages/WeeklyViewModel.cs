// Written by Kiara Vaz for CS4385.0W1, , Senior Design Project, Started October 3, 2024
// Net ID: KMV200000
// Purpose: Manage weekly time tracking data for students, allowing them to view, add, 
// and edit time slots for the current week. It connects to a MySQL database 
// to fetch, add, and modify data using stored procedures.


using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using Microsoft.Extensions.Configuration;

public class WeeklyViewModel : PageModel
{
    // List of time slots for the student
    public List<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();
    public string TotalTime { get; set; }   
    public string ErrorMessage { get; set; } 

    // Properties for adding or editing time slots
    [BindProperty]
    public string AddTime { get; set; }      
    [BindProperty]
    public string AddDescription { get; set; } 
    [BindProperty]
    public DateTime SelectedDate { get; set; }

    private readonly string connectionString;

    // Constructor to initialize the model with a connection string from the configuration.
    // Input: IConfiguration configuration (contains application settings).
    public WeeklyViewModel(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("DefaultConnection");
    }


    // OnGet method to load the current week's time slots.
    public void OnGet()
    {
        LoadCurrentWeekTimeSlots();
    }

    // Add a time slot to the database.
    // Inputs:SelectedDate, AddTime, AddDescription: The description of the time slot (string)
    // Outputs: 
    // - Adds a time slot to the database using a stored procedure.
    // - Displays the status message (success or error) to the view.
    // - Reloads the time slots after adding.
    public void OnPostAddTimeSlot(string SelectedDate, string AddTime, string AddDescription)
    {
        string stuNetID = HttpContext.Session.GetString("StudentNetId");

        
        if (string.IsNullOrEmpty(stuNetID))
        {
            Console.WriteLine("Error: StudentNetId not found in session.");
            return;
        }

        string statusMessage = "Success";  

        // Establish a database connection using the connection string.
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
                cmd.Parameters.AddWithValue("@ts_duration", AddTime); 

                // Variable to hold status or error message
                var statusParam = new MySqlParameter("@error_message", MySqlDbType.VarChar, 255);
                statusParam.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(statusParam);

                cmd.ExecuteNonQuery();

                
                statusMessage = statusParam.Value.ToString();
                Console.WriteLine("Stored Procedure Status: " + statusMessage);
            }
        }

        // Pass the status message to the view (error message from stored procedure)
        ViewData["ErrorMessage"] = statusMessage;

        // Reload the time slots after the insertion
        LoadCurrentWeekTimeSlots();
    }



    // Edit an existing time slot in the database.
    // Inputs: SelectedDate, UpdatedTime, UpdatedDescription
    // Outputs:
    // - Edits the time slot in the database using a stored procedure.
    // - Displays the status message (success or error) to the view.
    // - Reloads the time slots after editing.
    public void OnPostEditTimeSlot(string SelectedDate, string UpdatedTime, string UpdatedDescription)
    {
        string stuNetID = HttpContext.Session.GetString("StudentNetId");

        
        if (string.IsNullOrEmpty(stuNetID))
        {
            Console.WriteLine("Error: StudentNetId not found in session.");
            return;
        }

        string statusMessage = "Success";  

        // Establish a database connection using the connection string.
        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            // Call the stored procedure for editing a time slot
            using (var cmd = new MySqlCommand("student_edit_timeslot", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // Parameters for the stored procedure
                cmd.Parameters.AddWithValue("@student_netID", stuNetID);
                cmd.Parameters.AddWithValue("@ts_date", DateTime.Parse(SelectedDate));
                cmd.Parameters.AddWithValue("@updated_description", UpdatedDescription);
                cmd.Parameters.AddWithValue("@updated_duration", UpdatedTime); 

                // Variable to hold status or error message
                var statusParam = new MySqlParameter("@error_message", MySqlDbType.VarChar, 255);
                statusParam.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(statusParam);

                cmd.ExecuteNonQuery();

                statusMessage = statusParam.Value.ToString();
                Console.WriteLine("Stored Procedure Status: " + statusMessage);
            }
        }

        ViewData["ErrorMessage"] = statusMessage;

        // Reload the time slots after the update
        LoadCurrentWeekTimeSlots();
    }

    // Method to load all time slots for the current week.
    // Inputs: None (uses the session to retrieve the student NetID)
    // Outputs:
    // - Retrieves the list of time slots for the current week from the database
    // - Populates the `TimeSlots` property with the time slot details
    // Method to load the current week's time slots and total time for a student.
    private void LoadCurrentWeekTimeSlots()
{
    
    string stuNetID = HttpContext.Session.GetString("StudentNetId");

    if (string.IsNullOrEmpty(stuNetID))
    {
        Console.WriteLine("Error: StudentNetId not found in session.");
        return;
    }

    // Establish a database connection using the connection string. 
    using (var connection = new MySqlConnection(connectionString))
    {
        connection.Open(); // Open the connection to the database

        // Calculate the start and end date of the current week
        DateTime startDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek); // Start of the week (Monday)
        DateTime endDate = startDate.AddDays(6); // End of the week (Sunday)

        // Prepare and execute the stored procedure to retrieve time slots for the current week
        using (var cmd = new MySqlCommand("student_timeslot_by_week", connection))
        {
            cmd.CommandType = CommandType.StoredProcedure;

            // Add parameters for the stored procedure
            cmd.Parameters.AddWithValue("@stu_netID", stuNetID);
            cmd.Parameters.AddWithValue("@start_date", startDate);

            
            using (var reader = cmd.ExecuteReader())
            {
                Console.WriteLine("Executing stored procedure: student_timeslot_by_week for current week with stuNetID: " + stuNetID);

               
                while (reader.Read())
                {
                    // Extract duration from the database in HH:MM format
                    string durationString = reader.GetString(4);

                    int totalMinutes = 0; // Default to 0 if duration is invalid
                    if (!string.IsNullOrEmpty(durationString))
                    {
                        string[] timeParts = durationString.Split(':');
                        if (timeParts.Length == 2
                            && int.TryParse(timeParts[0], out int hours)
                            && int.TryParse(timeParts[1], out int minutes))
                        {
                            totalMinutes = (hours * 60) + minutes; // Convert to total minutes
                        }
                    }

                    // Create a TimeSlot object and populate its properties
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

        // Call a second stored procedure to calculate the total time spent by the student in the current week
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

            
            int totalMinutes = 0;

            if (!string.IsNullOrEmpty(statusParam.Value?.ToString())
                && int.TryParse(statusParam.Value.ToString(), out int parsedMinutes))
            {
                totalMinutes = parsedMinutes;
            }

            // Convert total minutes to hours and minutes
            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;

    
            TotalTime = $"{hours:D2}:{minutes:D2}";

            Console.WriteLine("Total(HH:MM): " + TotalTime);
        }
    }
}



}
