using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Back_End.Models
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                        .HasIndex(user => user.Email)
                        .IsUnique();

            modelBuilder.Entity<User>()
                        .HasIndex(user => user.FacebookId)
                        .IsUnique();
        }

        public DbSet<Tree> Trees { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
