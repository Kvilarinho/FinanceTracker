using System;
using FinanceTracker.Models;

namespace FinanceTracker.Interfaces;

public interface IFileStorageService
{
    Task SaveAsync(IEnumerable<Transaction> transactions);
    Task<List<Transaction>> LoadAsync();
}
