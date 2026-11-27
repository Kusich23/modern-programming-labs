using System.Text.Json;
using OnlineStore.Domain;

namespace OnlineStore.App;

/// <summary>
/// Handles exporting and importing <see cref="OrderReport"/> collections as JSON files.
/// </summary>
public static class OrderReportStorage
{
    public static async Task SaveAsync(
        string filePath,
        IReadOnlyCollection<OrderReport> reports,
        bool writeIndented = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(reports);

        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = writeIndented
        };

        string json = JsonSerializer.Serialize(reports, options);
        await File.WriteAllTextAsync(filePath, json, cancellationToken);
    }

    public static async Task<IReadOnlyList<OrderReport>> LoadAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Report file was not found: '{filePath}'.", filePath);
        }

        string json = await File.ReadAllTextAsync(filePath, cancellationToken);
        List<OrderReport>? restored = JsonSerializer.Deserialize<List<OrderReport>>(json);

        return restored ?? throw new JsonException("Deserialized report list was null.");
    }
}
