using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using CodexQuotaWidget.Core;

namespace CodexQuotaWidget.Codex;

public sealed class ResetCreditsClient : IDisposable
{
    private static readonly Uri Endpoint = new(
        "https://chatgpt.com/backend-api/wham/rate-limit-reset-credits");

    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;

    public ResetCreditsClient(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
        _ownsHttpClient = httpClient is null;
    }

    public async Task<IReadOnlyList<ResetCredit>> ReadAsync(CancellationToken cancellationToken = default)
    {
        var authPath = FindAuthPath() ?? throw new FileNotFoundException(
            "未找到 Codex 本机凭证；请先登录 Codex。", "auth.json");
        using var authDocument = JsonDocument.Parse(await File.ReadAllTextAsync(authPath, cancellationToken)
            .ConfigureAwait(false));
        var root = authDocument.RootElement;
        if (!root.TryGetProperty("tokens", out var tokens) ||
            !tokens.TryGetProperty("access_token", out var accessTokenNode) ||
            string.IsNullOrWhiteSpace(accessTokenNode.GetString()))
        {
            throw new InvalidDataException("Codex 本机凭证中没有可用的 access token。");
        }

        var accessToken = accessTokenNode.GetString()!;
        var accountId = tokens.TryGetProperty("account_id", out var accountIdNode)
            ? accountIdNode.GetString()
            : null;

        using var request = new HttpRequestMessage(HttpMethod.Get, Endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.UserAgent.ParseAdd("Codex Desktop");
        request.Headers.TryAddWithoutValidation("OAI-Language", "zh-CN");
        if (!string.IsNullOrWhiteSpace(accountId))
        {
            request.Headers.TryAddWithoutValidation("ChatGPT-Account-ID", accountId);
        }

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new InvalidOperationException("重置卡查询凭证已失效，请重新登录 Codex。");
        }

        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var payload = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return ResetCreditsMapper.Map(payload.RootElement);
    }

    private static string? FindAuthPath()
    {
        var candidates = new[]
        {
            Environment.GetEnvironmentVariable("CODEX_HOME"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".codex"),
            @"C:\CodexHome"
        };

        return candidates
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => Path.Combine(path!, "auth.json"))
            .FirstOrDefault(File.Exists);
    }

    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }
}
