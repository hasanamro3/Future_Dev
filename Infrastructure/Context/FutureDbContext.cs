using Domain.Entites.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context
{
    public class FutureDbContext : DbContext
    {
        public FutureDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Student> Students{ get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Category> Categories { get; set; }
      
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var relationShips = modelBuilder.Model
                .GetEntityTypes().SelectMany(e => e.GetForeignKeys());

            foreach (var relationship in relationShips)
            {
                relationship.DeleteBehavior = DeleteBehavior.NoAction;
            }
            modelBuilder.Entity<Enrollment>().HasKey(e => new { e.StudentId, e.CourseId });

            modelBuilder.Entity<User>().HasOne(u => u.Student).WithOne(s => s.User)
                        .HasForeignKey<Student>(s => s.UserId);

            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        }
    }
}
