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

    public async Task AddTransaction(string description, decimal amount, 
                                CategoryType category, TransactionType type)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty");

        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero");

        var transaction = new Transaction(
            Guid.NewGuid(),
            description,
            amount,
            category, 
            DateTime.Now,
            type
        );

        try {
            _repository.Add(transaction);
            await _repository.SaveToFileAsync();
        } 
        catch 
        {
            _repository.Remove(transaction.Id);
            throw;
        }
    }

    public IEnumerable<Transaction> GetAll()
    {
        return _repository.GetAll();
    }

    public IEnumerable<Transaction> GetByMonth(int year, int month)
    {
        if (month < 1 || month > 12)
            throw new ArgumentException("Invalid month");

        if (year < 1)
            throw new ArgumentException("Invalid year");

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

    public async Task RemoveTransaction(Guid id)
    {
        var transaction = _repository.GetAll().FirstOrDefault(t => t.Id == id) 
            ?? throw new KeyNotFoundException($"Transaction {id} not found");

        try 
        {
            _repository.Remove(id);
            await _repository.SaveToFileAsync();
        }
        catch 
        {
            _repository.Add(transaction);
            throw;
        }
        
    }
}
