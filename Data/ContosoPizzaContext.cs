using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ContosoPizza.Models;

namespace ContosoPizza.Data
{
    public class ContosoPizzaContext : DbContext
    {
        public ContosoPizzaContext (DbContextOptions<ContosoPizzaContext> options)
            : base(options)
        {
        }

        public DbSet<ContosoPizza.Models.Company> Company { get; set; } = default!;

        public DbSet<ContosoPizza.Models.Contact> Contact { get; set; } = default!;

        public DbSet<Token> Token { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Company>()
                .HasMany(c => c.Contacts)
                .WithOne(c => c.Company)
                .HasForeignKey(c => c.CompanyId);


            modelBuilder.Entity<Company>()
                .HasMany(c => c.Tokens)
                .WithOne(c => c.Company)
                .HasForeignKey(c => c.CompanyId);

            modelBuilder.Entity<Token>()
                .HasOne(c => c.Company);
                
            modelBuilder.Entity<Contact>()
                .HasOne(c => c.Company);

            modelBuilder.Entity<Token>()
                .HasOne(c => c.Company);
        }
    }
}
