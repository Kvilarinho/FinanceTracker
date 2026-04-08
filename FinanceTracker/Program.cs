using FinanceTracker.Models.Enums;
using FinanceTracker.Repositories;
using FinanceTracker.Services;

var fileStorage = new FileStorageService("transactions.json");
var repository = new TransactionRepository(fileStorage);
var service = new TransactionService(repository);


try
{
    await repository.LoadFromFileAsync();
}
catch (IOException ex)
{
    Console.WriteLine($"Warning: could not load transactions from file. {ex.Message}");
}

Console.WriteLine("=== Finance Tracker ===");

bool running = true;

while (running)
{
    Console.WriteLine("\n1. Add transaction");
    Console.WriteLine("2. Remove transaction");
    Console.WriteLine("3. List all transactions");
    Console.WriteLine("4. Filter by category");
    Console.WriteLine("5. Filter by month");
    Console.WriteLine("6. Total income / expenses");
    Console.WriteLine("0. Exit");
    Console.WriteLine("\nOptions");

    var input = Console.ReadLine();

    switch (input)
    {
        case "1":
            Console.Write("Description: ");
            var description = Console.ReadLine() ?? "";

            Console.Write("Amount: ");
            if (!decimal.TryParse(Console.ReadLine(), out var amount))
            {
                Console.WriteLine("Invalid amount.");
                break;
            }

            Console.WriteLine("Category: " + string.Join(", ", Enum.GetNames<CategoryType>()));
            if (!Enum.TryParse<CategoryType>(Console.ReadLine(), true, out var category))
            {
                Console.WriteLine("Invalid category");
                break;
            }

            Console.WriteLine("Type (Income/Expense): ");
            if (!Enum.TryParse<TransactionType>(Console.ReadLine(), true, out var type))
            {
                Console.WriteLine("Invalid type");
                break;
            }

            try
            {
                await service.AddTransaction(description, amount, category, type);
                Console.WriteLine("Transaction added");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Invalid input: {ex.Message}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Failed to save transaction: {ex.Message}");
            }
            break;

        case "2":
            Console.Write("Transaction ID: ");
            if (!Guid.TryParse(Console.ReadLine(), out var id))
            {
                Console.WriteLine("Invalid ID.");
                break;
            }

            try
            {
                await service.RemoveTransaction(id);
                Console.WriteLine("Transaction removed");
            }
            catch (KeyNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }
            break;

        case "3":
            var all = service.GetAll();
            foreach (var t in all)
            {
                Console.WriteLine($"{t.Id} | {t.Date:dd/MM/yyyy} | {t.Type} " +
                    $"| {t.Category} | {t.Amount:C} | {t.Description}");
            }
            break;

        case "4":
            Console.WriteLine("Category: " + string.Join(", ", Enum.GetNames<CategoryType>()));
            if (!Enum.TryParse<CategoryType>(Console.ReadLine(), true, out var filterCategory))
            {
                Console.WriteLine("Invalid category");
                break;
            }

            var byCategory = service.GetByCategory(filterCategory);
            foreach (var t in byCategory)
            {
                Console.WriteLine($"{t.Date:dd/MM/yyyy} | {t.Amount:C} | {t.Description}");
            }
            break;

        case "5":
            Console.Write("Year: ");
            if (!int.TryParse(Console.ReadLine(), out var year)) break;

            Console.Write("Month: ");
            if (!int.TryParse(Console.ReadLine(), out var month)) break;

            try
            {
                var byMonth = service.GetByMonth(year, month);
                foreach (var t in byMonth)
                    Console.WriteLine($"{t.Date:dd/MM/yyyy} | {t.Type} | {t.Amount:C} | {t.Description}");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Invalid input: {ex.Message}");
            }
            break;

        case "6":
            Console.WriteLine($"Total Income:  {service.GetTotalByType(TransactionType.Income):C}");
            Console.WriteLine($"Total Expense: {service.GetTotalByType(TransactionType.Expense):C}");
            break;

        case "0":
            running = false;
            break;

        default:
            Console.WriteLine("Invalid option.");
            break;
    }
}

Console.WriteLine("Goodbye!");
