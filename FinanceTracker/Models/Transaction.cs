using System;
using FinanceTracker.Models.Enums;

namespace FinanceTracker.Models;

public record Transaction(
    Guid Id,
    string Description,
    decimal Amount,
    CategoryType Category,
    DateTime Date,
    TransactionType Type
);

