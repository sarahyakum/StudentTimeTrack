using Microsoft.EntityFrameworkCore;
using StudentTimeTrack.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add ApplicationDbContext and configure it to use MySQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 25)) // Adjust to match your MySQL version
    ));

builder.Services.AddSession();
var app = builder.Build();

app.UseSession();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

// Set the default route to Login.cshtml
app.MapGet("/", async context =>
{
    context.Response.Redirect("/Login");
});

app.Run();
