// Written by Kiara Vaz for CS4385.0W1, Senior Design Project, Started October 3, 2024
// Net ID: KMV200000
// Purpose: Display time tracking data across the entire project, including the total time logged 
// by a student, time slots for a selected month, and project start and end dates. 
// It connects to a MySQL database to fetch and process data using stored procedures.

using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

public class WholeProjectViewModel : PageModel
{
    // Properties for storing and displaying data in the Razor view
    public string TotalTime { get; set; }  
    public int StartMonth { get; set; }    
    public int EndMonth { get; set; }     
    public int SelectedMonth { get; set; } = DateTime.Today.Month;  
    public int SelectedYear { get; set; } = DateTime.Today.Year;    
    public List<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>(); 

    // Properties for project start and end dates
    public DateTime ProjectStartDate { get; set; }
    public DateTime ProjectEndDate { get; set; }

   

    // Updates the selected month and year based on navigation (previous/next).
    // Inputs: Current Month, Current Year and Navigation Change
    public void OnGet(int month = 0, int year = 0, string change = null)
    {
        // Adjust month and year based on navigation input
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

        // Load time slots and project time frame
        LoadTimeSlots();
        GetTimeFrame();
    }

    // Retrieves the project's start and end dates based on the student's section code.
    // Inputs: Student's NetID (retrieved from the session).
    // Outputs: ProjectStartDate and ProjectEndDate are updated.
   private void GetTimeFrame()
{
    // Connection string for the MySQL database.
    string connectionString = "server=127.0.0.1;user=root;password=Kiav@z1208;database=seniordesignproject;";
    
    // Retrieve the student's NetID from the session.
    string stuNetID = HttpContext.Session.GetString("StudentNetId");

    // Check if the student's NetID is available in the session.
    if (string.IsNullOrEmpty(stuNetID))
    {
        // Log an error message and exit if the NetID is not found.
        Console.WriteLine("Error: StudentNetId not found in session.");
        return;
    }

    // Open a connection to the MySQL database using the connection string.
    using (var connection = new MySqlConnection(connectionString))
    {
        connection.Open();

        // Retrieve the section code for the student.
        string sectionCode = null; // Variable to store the section code.
        using (var cmd = new MySqlCommand("SELECT SecCode FROM MemberOf WHERE StuNetID = @StuNetID", connection))
        {
           
            cmd.Parameters.AddWithValue("@StuNetID", stuNetID);

            
            var result = cmd.ExecuteScalar();
            sectionCode = result?.ToString(); // Convert result to string if not null.
        }

        // Check if the section code was successfully retrieved.
        if (string.IsNullOrEmpty(sectionCode))
        {
            // Log an error message and exit if the section code is not found.
            Console.WriteLine("Error: Section code not found for the student.");
            return;
        }

        // Retrieve the project start and end dates using the section code.
        using (var cmd = new MySqlCommand("get_section_timeframe", connection))
        {
            // Specify that the command is a stored procedure.
            cmd.CommandType = CommandType.StoredProcedure;

            // Add the section code as a parameter to the stored procedure.
            cmd.Parameters.AddWithValue("@section_code", sectionCode);

            
            using (var reader = cmd.ExecuteReader())
            {
                // Check if the reader contains any rows.
                if (reader.Read())
                {
                    // Retrieve the start and end dates from the result set.
                    ProjectStartDate = reader.GetDateTime(0); 
                    ProjectEndDate = reader.GetDateTime(1);  
                }
                else
                {
                    // Log an error message if no timeframe is found for the section code.
                    Console.WriteLine("Error: No timeframe found for the section code.");
                }
            }
        }
    }
}



    // Loads the student's time slots for the selected month and retrieves the total time logged.
    // Inputs: Student's NetID (retrieved from the session), SelectedMonth, SelectedYear.
    // Outputs: TimeSlots is populated, TotalTime is updated in HH:MM format.
 private void LoadTimeSlots()
{
    // Connection string for the MySQL database.
    string connectionString = "server=127.0.0.1;user=root;password=Kiav@z1208;database=seniordesignproject;";

    // Retrieve the student's NetID from the session.
    string stuNetID = HttpContext.Session.GetString("StudentNetId");

    // Check if the student's NetID is available in the session.
    if (string.IsNullOrEmpty(stuNetID))
    {
        // Log an error message and exit if the NetID is not found.
        Console.WriteLine("Error: StudentNetId not found in session.");
        return;
    }

    // Open a connection to the MySQL database using the connection string.
    using (var connection = new MySqlConnection(connectionString))
    {
        connection.Open();

        // Step 1: Load time slots for the selected month.
        // Determine the start date of the selected month (1st day of the month).
        DateTime startDate = new DateTime(SelectedYear, SelectedMonth, 1);

        using (var cmd = new MySqlCommand("student_timeslot_by_month", connection))
        {
            // Specify that the command is a stored procedure.
            cmd.CommandType = CommandType.StoredProcedure;

           
            cmd.Parameters.AddWithValue("@stu_netID", stuNetID);
            cmd.Parameters.AddWithValue("@start_date", startDate);

        
            using (var reader = cmd.ExecuteReader())
            {
                // Loop through the result set to retrieve and process each time slot.
                while (reader.Read())
                {
                    // Parse the duration string (HH:MM format) into total minutes.
                    string[] timeParts = reader.GetString(4).Split(':'); // Split duration by colon.
                    int totalMinutes = (int.Parse(timeParts[0]) * 60) + int.Parse(timeParts[1]); // Calculate total minutes.

                    // Add the time slot to the `TimeSlots` list.
                    TimeSlots.Add(new TimeSlot
                    {
                        StuName = reader.GetString(1),       
                        TSDate = reader.GetDateTime(2),      
                        TSDescription = reader.GetString(3),
                        TSDuration = totalMinutes           
                    });
                }
            }
        }

        // Step 2: Retrieve the total time logged by the student.
        using (var cmd = new MySqlCommand("student_total_time", connection))
        {
            // Specify that the command is a stored procedure.
            cmd.CommandType = CommandType.StoredProcedure;

            // Add the student's NetID as a parameter to the stored procedure.
            cmd.Parameters.AddWithValue("@student_netID", stuNetID);

            // Add an output parameter to retrieve the total time logged by the student.
            var statusParam = new MySqlParameter("@student_total", MySqlDbType.VarChar, 255)
            {
                Direction = ParameterDirection.Output // Specify that this parameter is an output parameter.
            };
            cmd.Parameters.Add(statusParam);

            cmd.ExecuteNonQuery();

            int totalMinutes = int.Parse(statusParam.Value.ToString());

            // Convert the total minutes to the HH:MM format and store it in the `TotalTime` property.
            TotalTime = $"{totalMinutes / 60:D2}:{totalMinutes % 60:D2}"; 
        }
    }
}
}


// Represents a single time slot entry.
public class TimeSlot
{
    public string StuNetID { get; set; }    // Student's NetID
    public string StuName { get; set; }    // Student's name
    public DateTime TSDate { get; set; }   // Date of the time slot
    public string TSDescription { get; set; } // Description of the work done
    public double TSDuration { get; set; } // Duration of the time slot in minutes
}

