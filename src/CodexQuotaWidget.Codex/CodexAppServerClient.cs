using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CodexQuotaWidget.Codex;

public sealed class CodexAppServerClient : IAsyncDisposable
{
    private readonly JsonRpcRequestTracker _tracker = new();
    private readonly SemaphoreSlim _writeGate = new(1, 1);
    private readonly CancellationTokenSource _lifetime = new();
    private Process? _process;
    private Task? _readerTask;
    private Task? _errorReaderTask;
    private long _nextId;

    public event Func<string, JsonElement, Task>? NotificationReceived;

    public bool IsRunning => _process is { HasExited: false };

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_process is not null)
        {
            throw new InvalidOperationException("The app-server client has already been started.");
        }

        var startInfo = CreateStartInfo();

        _process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        _process.Exited += (_, _) => _tracker.FailAll(
            new EndOfStreamException("codex app-server exited unexpectedly."));

        try
        {
            if (!_process.Start())
            {
                throw new InvalidOperationException("Unable to start codex app-server.");
            }
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                "无法启动 Codex CLI。请确认 codex 已安装并位于 PATH 中。",
                exception);
        }

        _readerTask = ReadOutputAsync(_lifetime.Token);
        _errorReaderTask = DrainErrorsAsync(_lifetime.Token);

        await RequestAsync(
            "initialize",
            new
            {
                clientInfo = new
                {
                    name = "codex-quota-widget",
                    title = "Codex Quota Widget",
                    version = "0.1.0"
                },
                capabilities = new
                {
                    experimentalApi = true,
                    requestAttestation = false
                }
            },
            cancellationToken).ConfigureAwait(false);
        await SendNotificationAsync("initialized", null, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<JsonElement> RequestAsync(
        string method,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        EnsureRunning();
        var id = Interlocked.Increment(ref _nextId);
        var responseTask = _tracker.Register(id);

        try
        {
            await WriteAsync(new RpcRequest("2.0", id, method, parameters), cancellationToken)
                .ConfigureAwait(false);
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(15));
            try
            {
                return await responseTask.WaitAsync(timeout.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _tracker.Cancel(id, timeout.Token);
                throw;
            }
        }
        catch
        {
            _tracker.Cancel(id, cancellationToken);
            throw;
        }
    }

    public Task SendNotificationAsync(
        string method,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        EnsureRunning();
        return WriteAsync(new RpcNotification("2.0", method, parameters), cancellationToken);
    }

    private async Task WriteAsync(object message, CancellationToken cancellationToken)
    {
        await _writeGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var line = JsonSerializer.Serialize(message, JsonOptions);
            await _process!.StandardInput.WriteLineAsync(line.AsMemory(), cancellationToken)
                .ConfigureAwait(false);
            await _process.StandardInput.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _writeGate.Release();
        }
    }

    private async Task ReadOutputAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var line = await _process!.StandardOutput.ReadLineAsync(cancellationToken)
                    .ConfigureAwait(false);
                if (line is null)
                {
                    break;
                }

                if (!JsonRpcLineParser.TryParse(line, out var envelope) || envelope is null)
                {
                    continue;
                }

                if (envelope.Id.HasValue)
                {
                    _tracker.TryResolve(envelope);
                    continue;
                }

                if (envelope.Method is not null && envelope.Params is JsonElement @params)
                {
                    var handlers = NotificationReceived;
                    if (handlers is not null)
                    {
                        foreach (Func<string, JsonElement, Task> handler in handlers.GetInvocationList())
                        {
                            await handler(envelope.Method, @params).ConfigureAwait(false);
                        }
                    }
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            _tracker.FailAll(exception);
        }
    }

    private async Task DrainErrorsAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested &&
                   await _process!.StandardError.ReadLineAsync(cancellationToken).ConfigureAwait(false) is not null)
            {
                // Intentionally drain stderr so the child process cannot block on a full pipe.
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private void EnsureRunning()
    {
        if (!IsRunning)
        {
            throw new InvalidOperationException("codex app-server is not running.");
        }
    }

    private static ProcessStartInfo CreateStartInfo()
    {
        var executable = ResolveCodexExecutable();
        var startInfo = new ProcessStartInfo
        {
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        if (OperatingSystem.IsWindows() &&
            (executable.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase) ||
             executable.EndsWith(".bat", StringComparison.OrdinalIgnoreCase)))
        {
            startInfo.FileName = Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe";
            startInfo.ArgumentList.Add("/d");
            startInfo.ArgumentList.Add("/s");
            startInfo.ArgumentList.Add("/c");
            startInfo.ArgumentList.Add($"\"{executable}\" app-server --stdio");
        }
        else
        {
            startInfo.FileName = executable;
            startInfo.ArgumentList.Add("app-server");
            startInfo.ArgumentList.Add("--stdio");
        }

        return startInfo;
    }

    private static string ResolveCodexExecutable()
    {
        if (!OperatingSystem.IsWindows())
        {
            return "codex";
        }

        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var directory in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var cleanDirectory = directory.Trim().Trim('"');
            foreach (var filename in new[] { "codex.exe", "codex.cmd", "codex.bat" })
            {
                var candidate = Path.Combine(cleanDirectory, filename);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        return "codex";
    }

    public async ValueTask DisposeAsync()
    {
        _lifetime.Cancel();
        _tracker.FailAll(new ObjectDisposedException(nameof(CodexAppServerClient)));

        if (_process is { HasExited: false })
        {
            try
            {
                _process.StandardInput.Close();
                if (!await Task.Run(() => _process.WaitForExit(1_000)).ConfigureAwait(false))
                {
                    _process.Kill(entireProcessTree: true);
                }
            }
            catch (InvalidOperationException)
            {
            }
        }

        if (_readerTask is not null)
        {
            await IgnoreCancellationAsync(_readerTask).ConfigureAwait(false);
        }
        if (_errorReaderTask is not null)
        {
            await IgnoreCancellationAsync(_errorReaderTask).ConfigureAwait(false);
        }

        _process?.Dispose();
        _writeGate.Dispose();
        _lifetime.Dispose();
    }

    private static async Task IgnoreCancellationAsync(Task task)
    {
        try { await task.ConfigureAwait(false); }
        catch (OperationCanceledException) { }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private sealed record RpcRequest(string Jsonrpc, long Id, string Method, object? Params);
    private sealed record RpcNotification(string Jsonrpc, string Method, object? Params);
}
