using Microsoft.EntityFrameworkCore;
using StudentTimeTrack.Models;

namespace StudentTimeTrack.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}

        public DbSet<Student> Students { get; set; }
    }
}
