using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ContosoPizza.Data;
using System;
using System.Linq;

namespace ContosoPizza.Models;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using (var context = new ContosoPizzaContext(
            serviceProvider.GetRequiredService<
                DbContextOptions<ContosoPizzaContext>>()))
        {
            // Look for any Companys.
            if (context.Company.Any())
            {
                return;   // DB has been seeded
            }
            context.Company.AddRange(
                new Company
                {
                    Name = "Deja Vu",
                    erp = "Zoho", 
                    LedgerId = "761713361",
                    IsActive = true,
                    MultiCurrency = true,
                    FullUpdate = true,
                },
                new Company
                {
                    Name = "Sunday Funday",
                    erp = "Zoho",
                    LedgerId = "762107966",
                    IsActive = true,
                    MultiCurrency = true,
                    FullUpdate = true,
                },
                new Company
                {
                    Name = "NashAfrica",
                    erp = "Zoho",
                    LedgerId = "761907541",
                    IsActive = true,
                    MultiCurrency = true,
                    FullUpdate = true,
                },
                new Company
                { 
                    Name = "Sandbox",
                    erp = "QuickBooksOnlineSandbox",
                    LedgerId = "4620816365244119680",
                    IsActive = true,
                    MultiCurrency = false,
                    FullUpdate = false,
                }
            );
            context.SaveChanges();
        }
    }
}