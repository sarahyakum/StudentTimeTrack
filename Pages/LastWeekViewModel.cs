using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Collections.Generic;

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

    [BindProperty]
    public string EditTime { get; set; }
    [BindProperty]
    public string EditDescription { get; set; }
    [BindProperty]
    public DateTime EditSelectedDate { get; set; }

    public void OnGet()
    {
        LoadLastWeekTimeSlots();
    }

    public void OnPostAddTimeSlot(string SelectedDate, string AddTime, string AddDescription)
    {
        string connectionString = "server=127.0.0.1;user=root;password=Kiav@z1208;database=seniordesignproject;";
        string stuNetID = HttpContext.Session.GetString("StudentNetId");
    
        if (string.IsNullOrEmpty(stuNetID))
        {
            Console.WriteLine("Error: StudentNetId not found in session.");
            return;
        }
    
        string statusMessage = "Success"; 

        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            using (var cmd = new MySqlCommand("student_insert_timeslot", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@student_netID", stuNetID);
                cmd.Parameters.AddWithValue("@ts_date", DateTime.Parse(SelectedDate));
                cmd.Parameters.AddWithValue("@ts_description", AddDescription);
                cmd.Parameters.AddWithValue("@ts_duration", AddTime);

                var statusParam = new MySqlParameter("@error_message", MySqlDbType.VarChar, 255);
                statusParam.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(statusParam);

                cmd.ExecuteNonQuery();
                statusMessage = statusParam.Value.ToString();
            }
        }

        ViewData["ErrorMessage"] = statusMessage;
        LoadLastWeekTimeSlots();
    }

    public void OnPostEditTimeSlot(string SelectedDate, string EditTime, string EditDescription)
    {
        string connectionString = "server=127.0.0.1;user=root;password=Kiav@z1208;database=seniordesignproject;";
        string stuNetID = HttpContext.Session.GetString("StudentNetId");

        if (string.IsNullOrEmpty(stuNetID))
        {
            Console.WriteLine("Error: StudentNetId not found in session.");
            return;
        }

        string statusMessage = "Success";

        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            using (var cmd = new MySqlCommand("student_edit_timeslot", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@student_netID", stuNetID);
                cmd.Parameters.AddWithValue("@ts_date", DateTime.Parse(SelectedDate));
                cmd.Parameters.AddWithValue("@updated_description", EditDescription);
                cmd.Parameters.AddWithValue("@updated_duration", EditTime);

                var statusParam = new MySqlParameter("@error_message", MySqlDbType.VarChar, 255);
                statusParam.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(statusParam);

                cmd.ExecuteNonQuery();
                statusMessage = statusParam.Value.ToString();
            }
        }

        ViewData["ErrorMessage"] = statusMessage;
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

            DateTime endDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek - 1); // Sunday
            DateTime startDate = endDate.AddDays(-6); // Monday of the previous week

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

            using (var cmd = new MySqlCommand("student_time_in_range", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@student_netID", stuNetID);
                cmd.Parameters.AddWithValue("@startDate", startDate);
                cmd.Parameters.AddWithValue("@endDate", endDate);

                var statusParam = new MySqlParameter("@student_total", MySqlDbType.VarChar, 255);
                statusParam.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(statusParam);

                cmd.ExecuteNonQuery();
                int totalMinutes = int.Parse(statusParam.Value.ToString());

                int hours = totalMinutes / 60;
                int minutes = totalMinutes % 60;
                TotalTime = $"{hours:D2}:{minutes:D2}";
            }
        }
    }
}
