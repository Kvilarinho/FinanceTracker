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

## Save to File: Automatic vs Manual

Currently, the application calls `SaveToFileAsync` automatically after every change (add/remove). This ensures that data is always persisted immediately, reducing the risk of data loss if the app crashes.

**Trade-offs:**
- **Pros:**
  - Simplicity: No need to track unsaved changes or manage a 'dirty' state.
  - Data safety: Every operation is atomic from the user's perspective; changes are never lost if the app exits unexpectedly.
- **Cons:**
  - Performance: For large datasets or frequent operations, saving after every change can be slow.
  - Flexibility: Harder to implement batch operations or 'undo' functionality, since every change is persisted right away.

## Manual Atomicity Handling

To ensure atomicity in write operations (add/remove), the service uses a try/catch block: all changes are made in memory first, and only then persisted with `SaveToFileAsync`. If an exception occurs during saving, the previous state is restored (manual rollback), reverting any changes made to the in-memory list.

**Advantages:**
- Simulates transactions even without a database
- Prevents inconsistencies if saving fails

**Limitations:**
- Rollback only works while the application is running (does not cover power loss or crashes between operations)
- Not thread-safe and does not handle concurrent scenarios

This approach is sufficient for a simple CLI, but in larger systems it's recommended to use real database transactions.

## Thread Safety and Robustness

The repository now uses a lock (`_lock`) to ensure thread safety for all operations that access or modify the in-memory transaction list. This prevents race conditions and data corruption when multiple threads interact with the repository.

- **Async and Lock:**
  - Asynchronous file operations (`LoadAsync`, `SaveAsync`) are performed outside the lock to avoid deadlocks and blocking the thread.
  - The lock is used only to protect quick, in-memory operations (like adding, removing, or copying the list).

- **Data Exposure:**
  - The `GetAll()` method returns a read-only copy of the transaction list, preventing external modification.

- **Duplicate Prevention:**
  - The list is cleared before loading new transactions from file, avoiding duplicates if loading occurs multiple times.

- **Error Handling:**
  - File operations are wrapped in try/catch blocks and throw IOExceptions with context if they fail, making error sources clearer.

This approach is robust for a CLI app and demonstrates best practices for combining async, locking, and file IO in C#.

