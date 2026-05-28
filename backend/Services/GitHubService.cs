using System.Net.Http.Headers;
using System.Text.Json;

namespace LibraryPlatform.Services;

public class GitHubService
{
    private readonly HttpClient _http;
    private readonly string? _token;

    public GitHubService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _http.BaseAddress = new Uri("https://api.github.com/");
        _http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("LibraryPlatform", "1.0"));
        _token = config["GitHub:Token"];
        if (!string.IsNullOrWhiteSpace(_token))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
    }

    public static (string owner, string repo)? ParseGitHubUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        try
        {
            var uri = new Uri(url.StartsWith("http") ? url : "https://" + url);
            if (!uri.Host.Contains("github.com")) return null;
            var parts = uri.AbsolutePath.Trim('/').Split('/');
            if (parts.Length < 2) return null;
            return (parts[0], parts[1]);
        }
        catch { return null; }
    }

    public record GitHubRepoInfo(int Stars, int OpenIssues, DateTime LastUpdated);

    public async Task<GitHubRepoInfo?> FetchRepoInfoAsync(string repositoryUrl)
    {
        var parsed = ParseGitHubUrl(repositoryUrl);
        if (parsed is null) return null;

        var (owner, repo) = parsed.Value;
        try
        {
            var response = await _http.GetAsync($"repos/{owner}/{repo}");
            if (!response.IsSuccessStatusCode) return null;

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            var root = doc.RootElement;

            return new GitHubRepoInfo(
                Stars: root.GetProperty("stargazers_count").GetInt32(),
                OpenIssues: root.GetProperty("open_issues_count").GetInt32(),
                LastUpdated: root.GetProperty("updated_at").GetDateTime()
            );
        }
        catch
        {
            return null;
        }
    }
}
