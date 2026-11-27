using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
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

string baseUrl = configuration["ExternalApi:BaseUrl"] ?? "https://jsonplaceholder.typicode.com/";
int[] contactIds = [1, 2, 3];

using var httpClient = new HttpClient
{
    BaseAddress = new Uri(baseUrl)
};
var directoryClient = new ExternalCustomerDirectoryClient(httpClient);

using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

Console.WriteLine("Loading external contacts...");
try
{
    var concurrentSw = Stopwatch.StartNew();
    IReadOnlyList<ExternalUser> concurrentUsers = await directoryClient.GetUsersConcurrentlyAsync(contactIds, cts.Token);
    concurrentSw.Stop();

    var sequentialSw = Stopwatch.StartNew();
    IReadOnlyList<ExternalUser> sequentialUsers = await directoryClient.GetUsersSequentiallyAsync(contactIds, cts.Token);
    sequentialSw.Stop();

    Console.WriteLine($"Контакти покупців (Received: {concurrentUsers.Count}):");
    foreach (ExternalUser user in concurrentUsers)
    {
        Console.WriteLine($"  {user.Id} | {user.Name} | {user.Email}");
    }

    Console.WriteLine($"\nTiming comparison:");
    Console.WriteLine($"  Concurrent (Task.WhenAll): {concurrentSw.ElapsedMilliseconds} ms");
    Console.WriteLine($"  Sequential (await loop):   {sequentialSw.ElapsedMilliseconds} ms ({sequentialUsers.Count} loaded)");
}
catch (OperationCanceledException ex)
{
    Console.WriteLine($"Request timed out or cancelled: {ex.Message}");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"HTTP request failed: {ex.Message}");
}
catch (JsonException ex)
{
    Console.WriteLine($"JSON deserialization failed: {ex.Message}");
}
