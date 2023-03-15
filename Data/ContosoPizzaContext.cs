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
    }
}
