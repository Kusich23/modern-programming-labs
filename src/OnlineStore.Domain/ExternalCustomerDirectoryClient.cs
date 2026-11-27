using System.Net.Http.Json;

namespace OnlineStore.Domain;

/// <summary>
/// HTTP client for retrieving external customer directory contacts.
/// </summary>
public class ExternalCustomerDirectoryClient
{
    private readonly HttpClient _httpClient;

    public ExternalCustomerDirectoryClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
    }

    /// <summary>
    /// Fetches a single external user by identifier.
    /// </summary>
    public async Task<ExternalUser?> GetUserAsync(int id, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        return await _httpClient.GetFromJsonAsync<ExternalUser>($"users/{id}", cancellationToken);
    }

    /// <summary>
    /// Fetches multiple external users concurrently using <see cref="Task.WhenAll"/>.
    /// </summary>
    public async Task<IReadOnlyList<ExternalUser>> GetUsersConcurrentlyAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ids);

        Task<ExternalUser?>[] tasks = ids
            .Select(id => GetUserAsync(id, cancellationToken))
            .ToArray();

        ExternalUser?[] results = await Task.WhenAll(tasks);
        return results.Where(u => u is not null).Cast<ExternalUser>().ToList();
    }

    /// <summary>
    /// Fetches multiple external users sequentially for timing comparison.
    /// </summary>
    public async Task<IReadOnlyList<ExternalUser>> GetUsersSequentiallyAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ids);

        var users = new List<ExternalUser>();
        foreach (int id in ids)
        {
            ExternalUser? user = await GetUserAsync(id, cancellationToken);
            if (user is not null)
            {
                users.Add(user);
            }
        }

        return users;
    }
}
