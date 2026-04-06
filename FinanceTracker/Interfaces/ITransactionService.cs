using System;
using FinanceTracker.Models;
using FinanceTracker.Models.Enums;

namespace FinanceTracker.Interfaces;

public interface ITransactionService
{
    Task AddTransaction(string description, decimal amount, CategoryType category, TransactionType type);

    Task RemoveTransaction(Guid id);

    IEnumerable<Transaction> GetByCategory(CategoryType category);

    IEnumerable<Transaction> GetByMonth(int year, int month);

    decimal GetTotalByType(TransactionType type);
    
    IEnumerable<Transaction> GetAll();
}
