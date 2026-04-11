using System;

using FinanceTracker.Models;
using FinanceTracker.Models.Enums;
using FinanceTracker.Repositories;

using Moq;
using Xunit;

namespace FinanceTracker.Tests.Repositories;

public class TransactionRepositoryTests
{
    private readonly Mock<FinanceTracker.Interfaces.IFileStorageService> _fileStorageMock;
    private readonly TransactionRepository _repo;

    public TransactionRepositoryTests()
    {
        _fileStorageMock = new Mock<FinanceTracker.Interfaces.IFileStorageService>();
        _repo = new TransactionRepository(_fileStorageMock.Object);
    }

    [Fact]
    public async Task Concurrent_Load_And_Add_Are_Safe()
    {
        // Arrange: Pre-load one transaction.
        var t1 = new Transaction(Guid.NewGuid(), "Init", 1, CategoryType.Other, DateTime.UtcNow, TransactionType.Expense);
        _fileStorageMock.Setup(s => s.LoadAsync())
            .ReturnsAsync(new List<Transaction> { t1 });

        // Simulate an intentionally slow load (simulate IO delay)
        var t2 = new Transaction(Guid.NewGuid(), "User Add", 2, CategoryType.Salary, DateTime.UtcNow, TransactionType.Income);
        var loadStarted = false;
        var loadContinue = new TaskCompletionSource();

        _fileStorageMock.Setup(s => s.LoadAsync())
            .Returns(async () =>
            {
                loadStarted = true;
                await loadContinue.Task; // block until signaled
                return new List<Transaction> { t1 };
            });

        // Act: Start "LoadFromFileAsync" (incomplete)
        var loadTask = Task.Run(() => _repo.LoadFromFileAsync());
        // Wait until loading is indeed started
        while (!loadStarted) await Task.Delay(10);

        // Try to add a transaction while load is in progress: should block until load is done
        var addTransactionTask = Task.Run(() => _repo.Add(t2));

        // Give it a moment to ensure add is blocked, then allow load to finish
        await Task.Delay(100);
        Assert.False(addTransactionTask.IsCompleted); // Should be waiting on lock
        loadContinue.SetResult(); // Finish the load
        await loadTask; // Should complete
        await Task.WhenAny(addTransactionTask, Task.Delay(1000));

        // Assert: Add transaction succeeded after load, both present
        var all = _repo.GetAll().ToList();
        Assert.Contains(all, tx => tx.Id == t1.Id);
        Assert.Contains(all, tx => tx.Id == t2.Id);
    }

    [Fact]
    public async Task SaveToFileAsync_Exception_Propagates()
    {
        var t1 = new Transaction(Guid.NewGuid(), "Salary", 100, CategoryType.Salary, DateTime.UtcNow, TransactionType.Income);
        _repo.Add(t1);
        _fileStorageMock.Reset();
        _fileStorageMock.Setup(s => s.SaveAsync(It.IsAny<IEnumerable<Transaction>>()))
            .ThrowsAsync(new IOException("fail IO"));

        await Assert.ThrowsAsync<IOException>(() => _repo.SaveToFileAsync());
    }

    [Fact]
    public void Remove_NonExistent_Throws_KeyNotFoundException()
    {
        Assert.Throws<KeyNotFoundException>(() => _repo.Remove(Guid.NewGuid()));
    }

    // Additional tests can be added as needed for more advanced concurrency cases.
}
