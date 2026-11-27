using System.Text.Json;
using Microsoft.Extensions.Configuration;
using OnlineStore.App;
using OnlineStore.Domain;

Console.WriteLine("Modern Programming Course");
Console.WriteLine("Student: Кущов Максим Андрійович");
Console.WriteLine("Group: д4тк");
Console.WriteLine("Variant: 1");
Console.WriteLine("Domain: Online Store");
Console.WriteLine(".NET: 10");
Console.WriteLine(new string('=', 60));

string configFile = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile(configFile, optional: false, reloadOnChange: false)
    .Build();

string relativePath = configuration["Storage:FilePath"] ?? Path.Combine("data", "orders-report.json");
bool writeIndented = bool.TryParse(configuration["Storage:WriteIndented"], out bool parsedIndented)
    ? parsedIndented
    : true;

string reportPath = Path.Combine(AppContext.BaseDirectory, relativePath);

List<OrderReport> reports = CreateSampleReports();

try
{
    await OrderReportStorage.SaveAsync(reportPath, reports, writeIndented);
    Console.WriteLine($"Exported {reports.Count} order reports (WriteIndented={writeIndented}) to:\n  {reportPath}");

    IReadOnlyList<OrderReport> loadedReports = await OrderReportStorage.LoadAsync(reportPath);
    decimal totalAmount = loadedReports.Sum(r => r.Total);

    Console.WriteLine($"\nImported reports count: {loadedReports.Count}");
    Console.WriteLine($"Imported reports total amount: {totalAmount:F2} UAH");
    Console.WriteLine("\nLoaded report entries:");
    foreach (OrderReport report in loadedReports)
    {
        Console.WriteLine($"  - {report.OrderId} | {report.CustomerName} | {report.Status} | {report.ItemsCount} items | {report.Total:F2} UAH");
    }
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"File not found: {ex.Message}");
}
catch (JsonException ex)
{
    Console.WriteLine($"Invalid JSON format: {ex.Message}");
}
catch (IOException ex)
{
    Console.WriteLine($"I/O error: {ex.Message}");
}

// Demonstrate error handling for missing file and invalid JSON
Console.WriteLine("\nTesting file and JSON error handling:");
string missingPath = Path.Combine(AppContext.BaseDirectory, "data", "missing-report.json");
try
{
    await OrderReportStorage.LoadAsync(missingPath);
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"  Caught FileNotFoundException: {ex.Message}");
}

string corruptPath = Path.Combine(AppContext.BaseDirectory, "data", "corrupt-report.json");
try
{
    Directory.CreateDirectory(Path.GetDirectoryName(corruptPath)!);
    await File.WriteAllTextAsync(corruptPath, "{ invalid json");
    await OrderReportStorage.LoadAsync(corruptPath);
}
catch (JsonException ex)
{
    Console.WriteLine($"  Caught JsonException: {ex.Message}");
}
catch (IOException ex)
{
    Console.WriteLine($"  Caught IOException: {ex.Message}");
}

static List<OrderReport> CreateSampleReports()
{
    SeededStoreData seeded = StoreDataSeeder.CreateSampleData();
    return seeded.Orders
        .GetAll()
        .Take(6)
        .Select(OrderReport.FromOrder)
        .ToList();
}
