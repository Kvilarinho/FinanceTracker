using System;
using FinanceTracker.Interfaces;
using FinanceTracker.Models;

namespace FinanceTracker.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly List<Transaction> _transactions = new();
    private readonly IFileStorageService _fileStorageService;

    public TransactionRepository(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    public async Task LoadFromFileAsync()
    {
        var loaded = await _fileStorageService.LoadAsync();
        _transactions.AddRange(loaded);
    }

    public async Task SaveToFileAsync()
    {
        await _fileStorageService.SaveAsync(_transactions);
    }

    public void Add(Transaction transaction)
    {
        _transactions.Add(transaction);
    }

    public IEnumerable<Transaction> getAll()
    {
        return _transactions;
    }

    public void Remove(Guid id)
    {
        var transaction = _transactions.FirstOrDefault(t => t.Id == id) 
                        ?? throw new KeyNotFoundException($"Transaction {id} not found");
        _transactions.Remove(transaction);
    }

}
