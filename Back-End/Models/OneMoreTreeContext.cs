using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Back_End.Models
{
    public class OneMoreTreeContext : DbContext
    {
        public OneMoreTreeContext(DbContextOptions<OneMoreTreeContext> options) : base(options)
        {
        }

        

        public DbSet<Tree> Trees { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
