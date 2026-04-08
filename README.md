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
├── Program.cs       # Entry point and dependency wiring
├── Repositories     # Data access
│   └── TransactionRepository.cs
├── Services         # Business logic
│   ├── FileStorageService.cs
│   └── TransactionService.cs
├── transactions.json #FileStorage
└── UI
    └── ConsoleUI.cs  # Menu loop and user interaction
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

This approach is sufficient for a simple CLI, but in larger systems it's recommended to use real database transactions.

## Thread Safety and Robustness

The repository uses `ConcurrentDictionary<Guid, Transaction>` instead of a `List<Transaction>` with a manual lock. This provides built-in thread safety through fine-grained internal locking, without requiring explicit `lock` blocks throughout the code.

- **No manual lock needed:**
  - `TryAdd`, `TryRemove`, and `Values` are all atomic operations; the dictionary handles synchronisation internally.
  - Async file operations (`LoadAsync`, `SaveAsync`) are naturally outside any critical section.

- **Keyed by `Id`:**
  - Storing transactions by `Guid` key makes lookups and removals O(1) instead of O(n) with `FirstOrDefault`.
  - `TryAdd` also prevents duplicate `Id`s by design.

- **Data Exposure:**
  - `GetAll()` returns a `ReadOnlyCollection` snapshot of `Values`, preventing external modification.

- **Duplicate Prevention:**
  - `_transactions.Clear()` followed by `TryAdd` on reload avoids duplicates if `LoadFromFileAsync` is called more than once.

- **Error Handling:**
  - File operations are wrapped in try/catch blocks and throw `IOException` with context if they fail, making error sources clearer.

This approach is robust for a CLI app and demonstrates best practices for combining async, locking, and file IO in C#.

