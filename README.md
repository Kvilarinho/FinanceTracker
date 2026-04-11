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

The repository now uses a `ReaderWriterLockSlim` to guard **all** operations on in-memory and file storage. This means:

- **Full thread safety** for all operations:
  - Multiple threads can safely list/query transactions concurrently.
  - All mutation operations (add, remove, load-from-file, save-to-file) are exclusive and never race.
  - File reads/writes and in-memory mutations are always performed with the appropriate lock.
- **Atomic snapshots:**
  - `SaveToFileAsync` takes a consistent snapshot using a read-lock, blocking parallel mutations while reading but still allowing other snapshot reads.
- **No partial reads/writes:**
  - It's impossible for a load/save to overlap and cause file corruption or stale/incomplete reads, even if two UI sessions trigger them at the same time.

**Design rationale:**
- `ReaderWriterLockSlim` is the correct C# primitive when you want many readers but exclusive writers.
- This pattern means it is now safe to access from many threads, or even from multiple UI tasks (or future API endpoints) with zero risk of data races or corruption.
- All code is async/await friendly—locks only wrap minimal code and never cross thread boundaries.

**Error Handling:**
- File and memory errors are reported with clear, specific exceptions (`IOException`, `KeyNotFoundException`, etc.), ensuring robust detection and diagnosability.

This approach is robust and scalable for small- to medium-sized apps that need safe, immediate persistence with zero risk of concurrency bugs.

### Async File I/O and Snapshots
- Each save/load operation works on a complete snapshot of the transactions in memory, taken under lock for correctness.
- The file write (save) happens asynchronously right after the lock is released, which means:
  - It is *never* possible for the file to be corrupted or partially written, regardless of how often or concurrently save/load/add/remove is called.
  - Very rapid add/remove cycles may result in a saved file that slightly lags the absolute latest state in RAM (the save always reflects a real, past consistent state, matching best practices in .NET async/locking apps).
- This design is safe for real-world multi-session, multi-thread, or even future async API usage without additional concerns.

