using FlowIQ.Domain.Entities;
using FlowIQ.Domain.Enums;
using FlowIQ.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FlowIQ.Infrastructure.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FlowIQDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<FlowIQDbContext>>();

        try
        {
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrated successfully.");

            if (await context.Users.AnyAsync())
            {
                logger.LogInformation("Database already seeded. Skipping.");
                return;
            }

            // Create test user
            var user = new User
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                PhoneNumber = "+2348179709680",
                FullName = "Adewale Johnson",
                IsVerified = true,
                LastLoginAt = DateTime.UtcNow
            };
            context.Users.Add(user);

            // Create test business
            var business = new Business
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Wale's Provisions Store",
                Description = "Retail provision shop in Surulere",
                Category = "Retail",
                Address = "15 Adeniran Ogunsanya, Surulere, Lagos",
                UserId = user.Id
            };
            context.Businesses.Add(business);

            // Seed incomes for the last 30 days
            var random = new Random(42);
            var today = DateTime.UtcNow.Date;
            var incomes = new List<Income>();
            var expenses = new List<Expense>();

            string[] incomeSources = { "Shop Sales", "Mobile Transfer", "Online Order", "Wholesale", "Cash Sales" };
            ExpenseCategory[] categories = {
                ExpenseCategory.Inventory, ExpenseCategory.Rent, ExpenseCategory.Transport,
                ExpenseCategory.Salary, ExpenseCategory.Utilities, ExpenseCategory.Supplies
            };

            for (int i = 30; i >= 0; i--)
            {
                var date = today.AddDays(-i);

                // 1-3 incomes per day
                var incomeCount = random.Next(1, 4);
                for (int j = 0; j < incomeCount; j++)
                {
                    incomes.Add(new Income
                    {
                        Amount = Math.Round((decimal)(random.NextDouble() * 50000 + 5000), 2),
                        Source = incomeSources[random.Next(incomeSources.Length)],
                        TransactionDate = date.AddHours(random.Next(8, 20)),
                        Notes = null,
                        BusinessId = business.Id
                    });
                }

                // 1-2 expenses per day
                var expenseCount = random.Next(1, 3);
                for (int j = 0; j < expenseCount; j++)
                {
                    var cat = categories[random.Next(categories.Length)];
                    expenses.Add(new Expense
                    {
                        Amount = Math.Round((decimal)(random.NextDouble() * 30000 + 2000), 2),
                        Category = cat,
                        Description = $"{cat} payment",
                        TransactionDate = date.AddHours(random.Next(8, 20)),
                        Notes = null,
                        BusinessId = business.Id
                    });
                }
            }

            context.Incomes.AddRange(incomes);
            context.Expenses.AddRange(expenses);
            await context.SaveChangesAsync();

            logger.LogInformation("Database seeded with {IncomeCount} incomes and {ExpenseCount} expenses.",
                incomes.Count, expenses.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding database.");
        }
    }
}
