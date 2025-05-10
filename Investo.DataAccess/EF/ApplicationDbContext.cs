using Investo.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Investo.DataAccess.EF
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<UserType> UserTypes { get; set; }

        public DbSet<UserResetPasswordCode> UserResetPasswordCodes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<UserType>().HasData(DataSeed.GetUserTypes());
        }
    }
}