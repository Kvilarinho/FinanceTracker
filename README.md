# Finance Tracker

A CLI application to track personal income and expenses, built with .NET 9 and C#.

## How to run
```bash
dotnet run --project FinanceTracker
```

## How to test
```bash
dotnet test
```

## Project structure
```
FinanceTracker/
├── Interfaces      # Contracts for services and repositories
│   ├── IFileStorageService.cs
│   ├── ITransactionRepository.cs
│   └── ITransactionService.cs
├── Models           # Domain models and enums
│   ├── Enums
│   │   ├── CategoryType.cs
│   │   └── TransactionType.cs
│   └── Transaction.cs
├── Program.cs       # Entry point and CLI loop
├── Repositories     # Data access
│   └── TransactionRepository.cs
├── Services         # Business logic
│   ├── FileStorageService.cs
│   └── TransactionService.cs
└── transactions.json #FileStorage
```

## Architecture decisions

- **`record` for `Transaction`** — transactions are immutable by design; a transaction is never edited, only removed
- **`ITransactionRepository` and `ITransactionService` interfaces** — decouples business logic from data access; allows swapping implementations without changing consumers
- **`FileStorageService` separate from `TransactionRepository`** — persistence is an infrastructure concern, not a domain concern
- **Save on every mutation** — simpler than tracking dirty state; acceptable tradeoff for a CLI app with small datasets
- **`KeyNotFoundException` on invalid remove** — fails loudly instead of silently; easier to debug

## Known tradeoffs

- Data stored in local JSON — in production this would be a database
- No authentication — data is not isolated per user
- No pagination — all transactions are loaded into memory at startup
- No edit/update operation — by design; transactions are immutable

## What I would add next

- Unit tests for `TransactionRepository`
- A monthly summary with top 3 expenses using LINQ
- Dependency injection container instead of manual wiring in `Program.cs`