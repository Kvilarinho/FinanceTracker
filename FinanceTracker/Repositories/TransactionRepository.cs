using System;
using FinanceTracker.Interfaces;
using FinanceTracker.Models;

namespace FinanceTracker.Repositories;


public class TransactionRepository : ITransactionRepository
{
    private readonly List<Transaction> _transactions = new();
    private readonly IFileStorageService _fileStorageService;
    private readonly object _lock = new();

    public TransactionRepository(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }


    public async Task LoadFromFileAsync()
    {
        try
        {
            var transactions = await _fileStorageService.LoadAsync();

            lock (_lock)
            {
                _transactions.Clear(); // Avoid duplicates if called more than once
                _transactions.AddRange(transactions);
            }
        }
        catch (Exception ex)
        {
            throw new IOException("Failed to load transactions from file.", ex);
        }
    }


    public async Task SaveToFileAsync()
    {
        List<Transaction> snapshot;
        
        lock (_lock)
        {
            snapshot = _transactions.ToList(); // Copy for thread safety
        }
        try
        {
            await _fileStorageService.SaveAsync(snapshot);
        }
        catch (Exception ex)
        {
            throw new IOException("Failed to save transactions to file.", ex);
        }
    }


    public void Add(Transaction transaction)
    {
        lock (_lock)
        {
            _transactions.Add(transaction);
        }
    }


    public IEnumerable<Transaction> GetAll()
    {
        lock (_lock)
        {
            return _transactions.ToList().AsReadOnly();
        }
    }


    public void Remove(Guid id)
    {
        lock (_lock)
        {
            var transaction = _transactions.FirstOrDefault(t => t.Id == id)
                ?? throw new KeyNotFoundException($"Transaction {id} not found");
                
            _transactions.Remove(transaction);
        }
    }

}
