using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using DAL.Entities;

namespace DAL;

    public class DataContext: DbContext 
    {
        public DataContext(DbContextOptions<DataContext> options): base(options)
        {

        }

        //To prevent from creating multiple users with the same username
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(f=>f.Email).IsUnique();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
        => optionsBuilder.UseNpgsql(b=>b.MigrationsAssembly("Api"));

        public DbSet<User> Users => Set<User>();
    }


