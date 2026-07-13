using System.Collections.Concurrent;
using System.Text.Json;

namespace CodexQuotaWidget.Codex;

public sealed record JsonRpcError(int Code, string Message);

public sealed record JsonRpcEnvelope(
    long? Id,
    string? Method,
    JsonElement? Params,
    JsonElement? Result,
    JsonRpcError? Error);

public sealed class JsonRpcException(int code, string message)
    : Exception($"JSON-RPC error {code}: {message}")
{
    public int Code { get; } = code;
}

public static class JsonRpcLineParser
{
    public static bool TryParse(string line, out JsonRpcEnvelope? envelope)
    {
        envelope = null;
        try
        {
            using var document = JsonDocument.Parse(line);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            long? id = null;
            if (root.TryGetProperty("id", out var idElement) &&
                idElement.ValueKind == JsonValueKind.Number &&
                idElement.TryGetInt64(out var numericId))
            {
                id = numericId;
            }

            string? method = root.TryGetProperty("method", out var methodElement) &&
                             methodElement.ValueKind == JsonValueKind.String
                ? methodElement.GetString()
                : null;
            JsonElement? @params = root.TryGetProperty("params", out var paramsElement)
                ? paramsElement.Clone()
                : null;
            JsonElement? result = root.TryGetProperty("result", out var resultElement)
                ? resultElement.Clone()
                : null;

            JsonRpcError? error = null;
            if (root.TryGetProperty("error", out var errorElement) &&
                errorElement.ValueKind == JsonValueKind.Object)
            {
                var code = errorElement.TryGetProperty("code", out var codeElement) &&
                           codeElement.TryGetInt32(out var parsedCode)
                    ? parsedCode
                    : -1;
                var message = errorElement.TryGetProperty("message", out var messageElement)
                    ? messageElement.GetString() ?? "Unknown error"
                    : "Unknown error";
                error = new JsonRpcError(code, message);
            }

            if (id is null && method is null)
            {
                return false;
            }

            envelope = new JsonRpcEnvelope(id, method, @params, result, error);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}

public sealed class JsonRpcRequestTracker
{
    private readonly ConcurrentDictionary<long, TaskCompletionSource<JsonElement>> _pending = new();

    public Task<JsonElement> Register(long id)
    {
        var completion = new TaskCompletionSource<JsonElement>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        if (!_pending.TryAdd(id, completion))
        {
            throw new InvalidOperationException($"Request id {id} is already pending.");
        }

        return completion.Task;
    }

    public bool TryResolve(JsonRpcEnvelope envelope)
    {
        if (envelope.Id is not long id || !_pending.TryRemove(id, out var completion))
        {
            return false;
        }

        if (envelope.Error is not null)
        {
            completion.TrySetException(new JsonRpcException(
                envelope.Error.Code,
                envelope.Error.Message));
        }
        else if (envelope.Result is JsonElement result)
        {
            completion.TrySetResult(result);
        }
        else
        {
            completion.TrySetException(new InvalidDataException(
                $"Response {id} contains neither result nor error."));
        }

        return true;
    }

    public void Cancel(long id, CancellationToken cancellationToken)
    {
        if (_pending.TryRemove(id, out var completion))
        {
            completion.TrySetCanceled(cancellationToken);
        }
    }

    public void FailAll(Exception exception)
    {
        foreach (var id in _pending.Keys)
        {
            if (_pending.TryRemove(id, out var completion))
            {
                completion.TrySetException(exception);
            }
        }
    }
}
