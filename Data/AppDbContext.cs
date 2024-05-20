using Microsoft.EntityFrameworkCore;
using myproj.Models;

namespace myproj.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Person> People { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Person>().HasKey(p => p.Email);
        }
    }
}
