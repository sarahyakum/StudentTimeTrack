// Written by Kiara Vaz for CS4385.0W1, , Senior Design Project, Started October 3, 2024
// Net ID: KMV200000
// This file defines the `LoginModel` class for handling user login functionality within the student time-tracking system.


using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using System.Data;
using StudentTimeTrack.Data;

public class LoginModel : PageModel
{
    public string ErrorMessage { get; set; }


    public IActionResult OnGet()
    {
        if (HttpContext.Session.GetString("LoggedIn") != null) // user is logged in
        {
            return RedirectToPage("/Logout");
        }
        return Page();
    }

    // This method authenticates the student based on NetId and UtdId, retrieves their details from the database,
    // and stores them in the session if login is successful.
    // Inputs: NetID and UTDID
    public IActionResult OnPost(string NetId, string UtdId)
    {
        // Define the connection string to connect to the MySQL database.
        string connectionString = "server=127.0.0.1;user=root;password=Kiav@z1208;database=seniordesignproject;"; // Update as needed

        // Open a connection to the MySQL database using the connection string.
        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string errorMessage;

            using (var cmd = new MySqlCommand("check_student_login", connection))
            {

                cmd.CommandType = CommandType.StoredProcedure;

                // Add parameters to the stored procedure to pass the student's NetId (username) and UtdId (password).
                cmd.Parameters.AddWithValue("@stu_input_username", NetId);
                cmd.Parameters.AddWithValue("@stu_input_password", UtdId);

                // Create an output parameter to capture the error message returned from the stored procedure.
                var errorParam = new MySqlParameter("@error_message", MySqlDbType.VarChar)
                {
                    Size = 100,
                    Direction = ParameterDirection.Output
                };


                cmd.Parameters.Add(errorParam);


                cmd.ExecuteNonQuery();

                errorMessage = errorParam.Value.ToString();
            }


            if (errorMessage == "Success")
            {

                Student student = null;

                // Execute a query to retrieve the student's details from the database.
                using (var cmd = new MySqlCommand("SELECT StuNetID, StuUTDID, StuName FROM Student WHERE StuNetID = @NetId", connection))
                {
                    // Add the NetId parameter to the query to fetch the student's information.
                    cmd.Parameters.AddWithValue("@NetId", NetId);

                    // Execute the query and process the result.
                    using (var reader = cmd.ExecuteReader())
                    {
                        // If the query returns data (i.e., the student exists), populate the student object.
                        if (reader.Read())
                        {
                            student = new Student
                            {
                                // Map the fields returned from the database to the Student object properties.
                                NetId = reader["StuNetID"].ToString(),
                                UtdId = reader["StuUTDID"].ToString(),
                                Name = reader["StuName"].ToString()
                            };
                        }
                    }
                }

                // Store the student's information in the session for future use.
                // The session will allow the system to remember the student's details throughout their session.
                HttpContext.Session.SetString("StudentNetId", student.NetId);
                HttpContext.Session.SetString("StudentUtdId", student.UtdId);
                HttpContext.Session.SetString("StudentName", student.Name);
                HttpContext.Session.SetString("LoggedIn", student.NetId);


                return RedirectToPage("/WeeklyView");
            }


            else if (errorMessage == "Change password")
            {
                HttpContext.Session.SetString("StudentNetId", NetId);
                return RedirectToPage("/ChangePassword");
            }


            ErrorMessage = errorMessage;
        }

        return Page();
    }

}

