using Microsoft.EntityFrameworkCore;
using UMA.Models;
using UMA.Models.Entity;

namespace UMA.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
    }
}