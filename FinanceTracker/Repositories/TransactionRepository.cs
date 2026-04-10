using System;
using System.Collections.Concurrent;
using FinanceTracker.Interfaces;
using FinanceTracker.Models;

namespace FinanceTracker.Repositories;


public class TransactionRepository : ITransactionRepository
{
    private readonly ConcurrentDictionary<Guid, Transaction> _transactions = new();
    private readonly IFileStorageService _fileStorageService;

    public TransactionRepository(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }


    public async Task LoadFromFileAsync()
    {
        try
        {
            var transactions = await _fileStorageService.LoadAsync();

            _transactions.Clear();

            foreach (var transaction in transactions)
            {
                if (!_transactions.TryAdd(transaction.Id, transaction))
                    throw new InvalidOperationException($"Duplicate transaction ID {transaction.Id} found in file.");
            }
                
        }
        catch (Exception ex)
        {
            throw new IOException("Failed to load transactions from file.", ex);
        }
    }


    public async Task SaveToFileAsync()
    {
        try
        {
            var snapshot = _transactions.Values.ToList();
            await _fileStorageService.SaveAsync(snapshot);
        }
        catch (Exception ex)
        {
            throw new IOException("Failed to save transactions to file.", ex);
        }
    }


    public void Add(Transaction transaction)
    {
        if (!_transactions.TryAdd(transaction.Id, transaction))
            throw new InvalidOperationException($"Transaction {transaction.Id} already exists.");
    }


    public IEnumerable<Transaction> GetAll()
    {
        return _transactions.Values.ToList();
    }


    public void Remove(Guid id)
    {
        if (!_transactions.TryRemove(id, out _))
            throw new KeyNotFoundException($"Transaction {id} not found.");
    }

}
