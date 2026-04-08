using FinanceTracker.Repositories;
using FinanceTracker.Services;
using FinanceTracker.UI;

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

var ui = new ConsoleUI(service);
await ui.RunAsync();
