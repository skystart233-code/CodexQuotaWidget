using System.Text.Json;
using CodexQuotaWidget.Core;

namespace CodexQuotaWidget.Codex;

public sealed class CodexQuotaService : IAsyncDisposable
{
    private readonly CodexAppServerClient _client;
    private readonly SemaphoreSlim _refreshGate = new(1, 1);

    public CodexQuotaService(CodexAppServerClient client)
    {
        _client = client;
        _client.NotificationReceived += OnNotificationAsync;
    }

    public event Action<QuotaSnapshot>? SnapshotReceived;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _client.StartAsync(cancellationToken).ConfigureAwait(false);
        await RefreshAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        await _refreshGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var result = await _client.RequestAsync(
                "account/rateLimits/read",
                null,
                cancellationToken).ConfigureAwait(false);
            Publish(result);
        }
        finally
        {
            _refreshGate.Release();
        }
    }

    private Task OnNotificationAsync(string method, JsonElement parameters)
    {
        if (method == "account/rateLimits/updated")
        {
            // Notifications can be sparse. Re-read the authoritative snapshot instead
            // of treating omitted windows as unavailable.
            _ = ReconcileNotificationAsync();
        }

        return Task.CompletedTask;
    }

    private async Task ReconcileNotificationAsync()
    {
        try
        {
            await RefreshAsync().ConfigureAwait(false);
        }
        catch (ObjectDisposedException)
        {
        }
        catch (OperationCanceledException)
        {
        }
        catch (InvalidOperationException)
        {
            // The app coordinator will reconnect during its next periodic refresh.
        }
        catch (Exception)
        {
            // A notification-triggered reconciliation is best-effort; the periodic
            // refresh remains the visible, centrally handled recovery path.
        }
    }

    private void Publish(JsonElement payload) =>
        SnapshotReceived?.Invoke(RateLimitsMapper.Map(payload, DateTimeOffset.Now));

    public async ValueTask DisposeAsync()
    {
        _client.NotificationReceived -= OnNotificationAsync;
        _refreshGate.Dispose();
        await _client.DisposeAsync().ConfigureAwait(false);
    }
}
