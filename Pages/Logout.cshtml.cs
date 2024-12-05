/*
	Written by Darya Anbar for CS 4485.0W1, Senior Design Project, Started November 13, 2024.
    Net ID: dxa200020

    This file defines the model that handles logging out of the application.
*/

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StudentPR.Pages
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnPost() // user wants to logout
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Login");
        }
    }
}
