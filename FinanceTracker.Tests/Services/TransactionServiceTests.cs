using System;
using FinanceTracker.Interfaces;
using FinanceTracker.Models;
using FinanceTracker.Services;
using FinanceTracker.Models.Enums;

using Moq;

namespace FinanceTracker.Tests.Services;

public class TransactionServiceTests
{
    private readonly Mock<ITransactionRepository> _repositoryMock;
    private readonly TransactionService _service;

    public TransactionServiceTests()
    {
        _repositoryMock = new Mock<ITransactionRepository>();
        _service = new TransactionService(_repositoryMock.Object);
    }

    [Fact]
    public void AddTransaction_ShouldCallRepositoryAdd()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetAll())
            .Returns(new List<Transaction>());

        // Act
        _service.AddTransaction("Salary", 1000m, CategoryType.Salary, TransactionType.Income);

        // Assert
        _repositoryMock.Verify(r => r.Add(It.IsAny<Transaction>()), Times.Once);

    }

    [Fact]
    public void GetByCategory_ShouldReturnOnlyTransactionOfGivenCategory()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new(Guid.NewGuid(), "Salary", 1000m, CategoryType.Salary, DateTime.Now, TransactionType.Income),
            new(Guid.NewGuid(), "Groceries", 50m, CategoryType.Food, DateTime.Now, TransactionType.Expense),
            new(Guid.NewGuid(), "Bonus", 200m, CategoryType.Salary, DateTime.Now, TransactionType.Income)
        };

        _repositoryMock
            .Setup(r => r.GetAll())
            .Returns(transactions);

        // Act
        var result = _service.GetByCategory(CategoryType.Salary);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, t => Assert.Equal(CategoryType.Salary, t.Category));
    }

    [Fact]
    public void GetTotalByType_ShouldReturnTotalOfGivenType()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new(Guid.NewGuid(), "Salary", 1000m, CategoryType.Salary, DateTime.Now, TransactionType.Income),
            new(Guid.NewGuid(), "Groceries", 50m, CategoryType.Food, DateTime.Now, TransactionType.Expense),
            new(Guid.NewGuid(), "Bonus", 200m, CategoryType.Salary, DateTime.Now, TransactionType.Income)
        };

        _repositoryMock
            .Setup(r => r.GetAll())
            .Returns(transactions);

        // Act
        var result = _service.GetTotalByType(TransactionType.Income);

        // Assert
        Assert.Equal(1200m, result);
        Assert.Equal(50m, _service.GetTotalByType(TransactionType.Expense));
    }

    [Fact]
    public void GetByMonth_ShouldOnlyReturnByMonth()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new(Guid.NewGuid(), "Salary", 1000m, CategoryType.Salary, new DateTime(2026, 3, 1), TransactionType.Income),
            new(Guid.NewGuid(), "Groceries", 50m, CategoryType.Food, new DateTime(2026, 3, 15), TransactionType.Expense),
            new(Guid.NewGuid(), "Bonus", 200m, CategoryType.Salary, new DateTime(2025, 2, 15), TransactionType.Income)
        };

        _repositoryMock
            .Setup(r => r.GetAll())
            .Returns(transactions);

        // Act
        var result = _service.GetByMonth(2026, 3);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, t => Assert.Equal(3, t.Date.Month));
    }

    [Fact]
    public void RemoveTransaction_ShouldRemoveTransaction()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new(Guid.NewGuid(), "Salary", 1000m, CategoryType.Salary, new DateTime(2026, 3, 1), TransactionType.Income),
            new(Guid.NewGuid(), "Groceries", 50m, CategoryType.Food, new DateTime(2026, 3, 15), TransactionType.Expense),
            new(Guid.NewGuid(), "Bonus", 200m, CategoryType.Salary, new DateTime(2025, 2, 15), TransactionType.Income)
        };

        _repositoryMock
            .Setup(r => r.GetAll())
            .Returns(transactions);

        // Act
        _service.RemoveTransaction(transactions[1].Id);

        // Assert
        _repositoryMock.Verify(r => r.Remove(transactions[1].Id), Times.Once);
    }

    [Fact]
    public void RemoveTransaction_WithoutIdShouldThrowKeyNotFoundException()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.Remove(It.IsAny<Guid>()))
            .Throws<KeyNotFoundException>();

        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() =>
            _service.RemoveTransaction(Guid.NewGuid()));

    }
}
