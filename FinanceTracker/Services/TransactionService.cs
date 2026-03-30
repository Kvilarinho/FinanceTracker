using System;
using FinanceTracker.Interfaces;
using FinanceTracker.Models;
using FinanceTracker.Models.Enums;

namespace FinanceTracker.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _repository;

    public TransactionService(ITransactionRepository repository)
    {
        _repository = repository;
    }

    public void AddTransaction(string description, decimal amount, 
                                CategoryType category, TransactionType type)
    {
        var transaction = new Transaction(
            Guid.NewGuid(),
            description,
            amount,
            category, 
            DateTime.Now,
            type
        );
        _repository.Add(transaction);
    }

    public IEnumerable<Transaction> GetAll()
    {
        return _repository.GetAll();
    }

    public IEnumerable<Transaction> GetByMonth(int year, int month)
    {
        return _repository.GetAll()
            .Where(t => t.Date.Year == year && t.Date.Month == month);
    }

    public IEnumerable<Transaction> GetByCategory(CategoryType category)
    {
        return _repository.GetAll().Where(t => t.Category == category);
    }

    public decimal GetTotalByType(TransactionType type)
    {
        return _repository.GetAll()
            .Where(t => t.Type == type)
            .Sum(t => t.Amount);
    }

    public void RemoveTransaction(Guid id)
    {
        _repository.Remove(id);
    }
}
