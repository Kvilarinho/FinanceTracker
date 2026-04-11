using System;
using System.Collections.Concurrent;
using FinanceTracker.Interfaces;
using FinanceTracker.Models;

namespace FinanceTracker.Repositories;


using System.Threading;

public class TransactionRepository : ITransactionRepository
{
    // Protects BOTH file and in-memory operations, ensures safe concurrent usage
    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.NoRecursion);
    private readonly ConcurrentDictionary<Guid, Transaction> _transactions = new();
    private readonly IFileStorageService _fileStorageService;

    public TransactionRepository(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }


    public async Task LoadFromFileAsync()
    {
        _lock.EnterWriteLock();
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
        finally
        {
            _lock.ExitWriteLock();
        }
    }


    public async Task SaveToFileAsync()
    {
        // Save snapshot with read lock: allows parallel reads but prevents concurrent writes during snapshot
        _lock.EnterReadLock(); // snapshot is safe with a read lock; writes will block.
        try
        {
            var snapshot = _transactions.Values.ToList();
            await _fileStorageService.SaveAsync(snapshot);
        }
        catch (Exception ex)
        {
            throw new IOException("Failed to save transactions to file.", ex);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }


    public void Add(Transaction transaction)
    {
        _lock.EnterWriteLock();
        try
        {
            if (!_transactions.TryAdd(transaction.Id, transaction))
                throw new InvalidOperationException($"Transaction {transaction.Id} already exists.");
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }


    public IEnumerable<Transaction> GetAll()
    {
        _lock.EnterReadLock();
        try
        {
            return _transactions.Values.ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }


    public void Remove(Guid id)
    {
        _lock.EnterWriteLock();
        try
        {
            if (!_transactions.TryRemove(id, out _))
                throw new KeyNotFoundException($"Transaction {id} not found.");
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

}
