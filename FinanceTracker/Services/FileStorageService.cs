using System;
using System.Text.Json;
using FinanceTracker.Interfaces;
using FinanceTracker.Models;

namespace FinanceTracker.Services;

public class FileStorageService : IFileStorageService
{

    private readonly string _filePath;

    public FileStorageService(string filepath = "transactions.json")
    {
        _filePath = filepath;
    }

    public async Task SaveAsync(IEnumerable<Transaction> transactions)
    {
        var json = JsonSerializer.Serialize(transactions, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        await File.WriteAllTextAsync(_filePath, json);
    }

    public async Task<List<Transaction>> LoadAsync()
    {
        if (!File.Exists(_filePath)) return new List<Transaction>();

        var json = await File.ReadAllTextAsync(_filePath);
        return JsonSerializer.Deserialize<List<Transaction>>(json) ?? new();
    }

    
}
