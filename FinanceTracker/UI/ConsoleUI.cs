using FinanceTracker.Interfaces;
using FinanceTracker.Models.Enums;

namespace FinanceTracker.UI;

public class ConsoleUI(ITransactionService service)
{
    private readonly ITransactionService _service = service;

    public async Task RunAsync()
    {
        Console.WriteLine("=== Finance Tracker ===");
        bool running = true;

        while (running)
        {
            PrintMenu();
            var input = Console.ReadLine();

            switch (input)
            {
                case "1": await HandleAddAsync(); break;
                case "2": await HandleRemoveAsync(); break;
                case "3": HandleListAll(); break;
                case "4": HandleFilterByCategory(); break;
                case "5": HandleFilterByMonth(); break;
                case "6": HandleTotals(); break;
                case "0": running = false; break;
                default: Console.WriteLine("Invalid option."); break;
            }
        }

        Console.WriteLine("Goodbye!");
    }

    private static void PrintMenu()
    {
        Console.WriteLine("\n1. Add transaction");
        Console.WriteLine("2. Remove transaction");
        Console.WriteLine("3. List all transactions");
        Console.WriteLine("4. Filter by category");
        Console.WriteLine("5. Filter by month");
        Console.WriteLine("6. Total income / expenses");
        Console.WriteLine("0. Exit");
        Console.WriteLine("\nOptions");
    }

    private async Task HandleAddAsync()
    {
        Console.Write("Description: ");
        var description = Console.ReadLine() ?? "";

        Console.Write("Amount: ");
        if (!decimal.TryParse(Console.ReadLine(), out var amount))
        {
            Console.WriteLine("Invalid amount.");
            return;
        }

        Console.WriteLine("Category: " + string.Join(", ", Enum.GetNames<CategoryType>()));
        if (!Enum.TryParse<CategoryType>(Console.ReadLine(), true, out var category))
        {
            Console.WriteLine("Invalid category.");
            return;
        }

        Console.WriteLine("Type (Income/Expense): ");
        if (!Enum.TryParse<TransactionType>(Console.ReadLine(), true, out var type))
        {
            Console.WriteLine("Invalid type.");
            return;
        }

        try
        {
            await _service.AddTransaction(description, amount, category, type);
            Console.WriteLine("Transaction added.");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Invalid input: {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Failed to save transaction: {ex.Message}");
        }
    }

    private async Task HandleRemoveAsync()
    {
        Console.Write("Transaction ID: ");
        if (!Guid.TryParse(Console.ReadLine(), out var id))
        {
            Console.WriteLine("Invalid ID.");
            return;
        }

        try
        {
            await _service.RemoveTransaction(id);
            Console.WriteLine("Transaction removed.");
        }
        catch (KeyNotFoundException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Failed to save changes: {ex.Message}");
        }
    }

    private void HandleListAll()
    {
        var transactions = _service.GetAll();

        if (!transactions.Any())
        {
            Console.WriteLine("No transactions found.");
            return;
        }

        foreach (var t in transactions)
            Console.WriteLine($"{t.Id} | {t.Date:dd/MM/yyyy} | {t.Type} | {t.Category} | {t.Amount:C} | {t.Description}");
    }

    private void HandleFilterByCategory()
    {
        Console.WriteLine("Category: " + string.Join(", ", Enum.GetNames<CategoryType>()));
        if (!Enum.TryParse<CategoryType>(Console.ReadLine(), true, out var category))
        {
            Console.WriteLine("Invalid category.");
            return;
        }

        var results = _service.GetByCategory(category);

        if (!results.Any())
        {
            Console.WriteLine("No transactions found for this category.");
            return;
        }

        foreach (var t in results)
            Console.WriteLine($"{t.Date:dd/MM/yyyy} | {t.Amount:C} | {t.Description}");
    }

    private void HandleFilterByMonth()
    {
        Console.Write("Year: ");
        if (!int.TryParse(Console.ReadLine(), out var year))
        {
            Console.WriteLine("Invalid year.");
            return;
        }

        Console.Write("Month: ");
        if (!int.TryParse(Console.ReadLine(), out var month))
        {
            Console.WriteLine("Invalid month.");
            return;
        }

        try
        {
            var results = _service.GetByMonth(year, month);

            if (!results.Any())
            {
                Console.WriteLine("No transactions found for this period.");
                return;
            }

            foreach (var t in results)
                Console.WriteLine($"{t.Date:dd/MM/yyyy} | {t.Type} | {t.Amount:C} | {t.Description}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Invalid input: {ex.Message}");
        }
    }

    private void HandleTotals()
    {
        Console.WriteLine($"Total Income:  {_service.GetTotalByType(TransactionType.Income):C}");
        Console.WriteLine($"Total Expense: {_service.GetTotalByType(TransactionType.Expense):C}");
    }
}
