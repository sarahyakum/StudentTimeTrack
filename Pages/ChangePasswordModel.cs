// Written by Kiara Vaz and Darya Anbar for CS4385.0W1, , Senior Design Project, Started October 3, 2024
// Net ID: KMV200000 and dxa200020
// Purpose: Handles the change password functionality. It connects to a MySQL database 
// to do so and modify data using stored procedure

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;

public class ChangePasswordModel : PageModel
{
    public string ErrorMessage { get; set; } = string.Empty;

    private readonly string connectionString;

    // Constructor to initialize the model with a connection string from the configuration.
    // Input: IConfiguration configuration (contains application settings).
    public ChangePasswordModel(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    // This method handles the form submission for changing a password.
    // Input: OldPassword (string), NewPassword (string), ConfirmPassword (string)
    // Output: IActionResult - the page to redirect or return after processing.
    public IActionResult OnPost(string OldPassword, string NewPassword, string ConfirmPassword)
    {
        Console.WriteLine("Change Password");

        // Check if the new password and confirmation password match.
        if (NewPassword != ConfirmPassword)
        {
            ErrorMessage = "Passwords Do Not Match. Try Again.";
            return Page(); 
        }
        else
        {
            
            string stuNetID = HttpContext.Session.GetString("StudentNetId");

           
            if (string.IsNullOrEmpty(stuNetID))
            {
                ErrorMessage = "Session expired or invalid. Please log in again.";
                return RedirectToPage("/Login");
            }

            // Establish a database connection using the connection string.
            using (var connection = new MySqlConnection(connectionString))
            {
                Console.WriteLine("Open Connection");
                connection.Open();

                string errorMessage;

                // Call the stored procedure `change_student_password` to update the password.
                using (var cmd = new MySqlCommand("change_student_password", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Add input parameters for the stored procedure.
                    cmd.Parameters.AddWithValue("@stu_username", stuNetID);
                    Console.WriteLine(stuNetID);
                    cmd.Parameters.AddWithValue("@old_student_password", OldPassword);
                    Console.WriteLine(OldPassword);
                    cmd.Parameters.AddWithValue("@new_student_password", NewPassword);
                    Console.WriteLine(NewPassword);

                    
                    var errorParam = new MySqlParameter("@error_message", MySqlDbType.VarChar)
                    {
                        Size = 100,
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(errorParam);

                    Console.WriteLine("Executing change_student_password");
                    cmd.ExecuteNonQuery();

                    errorMessage = errorParam.Value.ToString() ?? string.Empty;
                    Console.WriteLine(errorMessage);
                }

                
                if (errorMessage == "Success")
                {
                    // Attempt to retrieve the student's details from the database.
                    Student? student = null;
                    using (var cmd = new MySqlCommand("SELECT StuPassword FROM Student WHERE StuNetID = @NetId", connection))
                    {
                        cmd.Parameters.AddWithValue("@NetId", stuNetID);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Map the retrieved data to a Student object.
                                student = new Student
                                {
                                    Password = reader["StuPassword"]?.ToString() ?? string.Empty
                                };
                            }
                        }
                    }

                    
                    if (student != null)
                    {
                        HttpContext.Session.SetString("StudentPassword", student.Password);
                        return RedirectToPage("/Login"); // Redirect user to the login page.
                    }
                    else
                    {
                        
                        ErrorMessage = "Student not found.";
                        return Page();
                    }
                }
                else
                {
                    ErrorMessage = errorMessage;
                    return Page(); 
                }
            }
        }
    }
}