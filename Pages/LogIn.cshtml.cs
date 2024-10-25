using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.Data;

namespace YourNamespace.Pages
{
    public class LogInModel : PageModel
    {
        public string ErrorMessage { get; set; }

        public IActionResult OnPost(string NetId, string UtdId)
        {
            string connectionString = "server=127.0.0.1;user=root;password=Kiav@z1208;database=seniordesignproject;"; // Update as needed

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // Call the stored procedure for student login
                string errorMessage;
                using (var cmd = new MySqlCommand("check_student_login", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@stu_input_username", NetId);
                    cmd.Parameters.AddWithValue("@stu_input_password", UtdId);
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
                    return RedirectToPage("/WeeklyView"); // Redirect to student dashboard
                }

                // Set error message to display
                ErrorMessage = errorMessage;
            }

            return Page(); // Return to the login page with error message
        }
    }
}
