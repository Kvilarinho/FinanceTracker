using System;
using FinanceTracker.Models;

namespace FinanceTracker.Interfaces;

public interface ITransactionRepository
{
    void Add(Transaction transaction);
    void Remove(Guid id);
    IEnumerable<Transaction> GetAll();
}
