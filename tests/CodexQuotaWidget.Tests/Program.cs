using System.Text.Json;
using CodexQuotaWidget.Codex;
using CodexQuotaWidget.Core;

if (args.Contains("--live", StringComparer.OrdinalIgnoreCase))
{
    return await RunLiveProbeAsync();
}

if (args.Contains("--live-reset", StringComparer.OrdinalIgnoreCase))
{
    return await RunLiveResetCreditsProbeAsync();
}

var tests = new (string Name, Func<Task> Run)[]
{
    ("300 分钟映射为 5H 并保留重置时间", () => RunSync(TestFiveHours)),
    ("额度窗口字段改名仍可识别", () => RunSync(TestRenamedWindows)),
    ("10080 分钟映射为周额度且支持小数", () => RunSync(TestWeekDecimal)),
    ("primary=周/secondary=null 的窗口形状", () => RunSync(TestPrimaryWeekShape)),
    ("null 窗口明确不可用", () => RunSync(TestNullWindow)),
    ("未知 duration 不误分类", () => RunSync(TestUnknownWindow)),
    ("读取重置卡数量", () => RunSync(TestResetCreditCount)),
    ("解析重置卡发放和到期时间", () => RunSync(TestResetCreditsMapper)),
    ("重置卡倒计时文案", () => RunSync(TestResetCreditCountdown)),
    ("重置卡提醒分级", () => RunSync(TestResetReminderPolicy)),
    ("极简模式优先展示另一条额度，重置卡三天内置顶", () => RunSync(TestMinimalSecondaryDisplay)),
    ("5H 或周余量低于 5% 整体预警", () => RunSync(TestQuotaCriticalAlert)),
    ("仅识别 Codex 桌面端进程", () => RunSync(TestCodexDesktopProcessClassifier)),
    ("托盘 Emoji 只接受一个字形", () => RunSync(TestTrayEmojiValue)),
    ("JSON-RPC 行解析", () => RunSync(TestLineParser)),
    ("JSON-RPC 请求 ID 关联", TestRequestCorrelation)
};

var failures = new List<string>();
foreach (var test in tests)
{
    try
    {
        await test.Run();
        Console.WriteLine($"PASS {test.Name}");
    }
    catch (Exception exception)
    {
        failures.Add($"FAIL {test.Name}: {exception.Message}");
        Console.Error.WriteLine(failures[^1]);
    }
}

Console.WriteLine($"{tests.Length - failures.Count}/{tests.Length} tests passed.");
return failures.Count == 0 ? 0 : 1;

static Task RunSync(Action action)
{
    action();
    return Task.CompletedTask;
}

static JsonElement Json(string value) => JsonDocument.Parse(value).RootElement.Clone();

static void TestFiveHours()
{
    var snapshot = RateLimitsMapper.Map(Json("""
        {"rateLimits":{"primary":{"usedPercent":23,"windowDurationMins":300,"resetsAt":"2030-01-01T06:00:00Z"},"secondary":null}}
        """));
    Equal(77d, snapshot.FiveHours.RemainingPercent);
    Equal(new DateTimeOffset(2030, 1, 1, 6, 0, 0, TimeSpan.Zero), snapshot.FiveHours.ResetsAt);
    False(snapshot.Week.IsAvailable);
}

static void TestRenamedWindows()
{
    var snapshot = RateLimitsMapper.Map(Json("""
        {
          "rateLimits": {
            "fiveHourWindow": {"usedPercent": 12, "windowDurationMins": 300},
            "weeklyWindow": {"usedPercent": 34, "windowDurationMins": 10080}
          }
        }
        """));
    Equal(88d, snapshot.FiveHours.RemainingPercent);
    Equal(66d, snapshot.Week.RemainingPercent);
}

static void TestWeekDecimal()
{
    var snapshot = RateLimitsMapper.Map(Json("""
        {"primary":null,"secondary":{"usedPercent":42.25,"windowDurationMins":10080}}
        """));
    Equal(57.75d, snapshot.Week.RemainingPercent);
    False(snapshot.FiveHours.IsAvailable);
}

static void TestPrimaryWeekShape()
{
    var snapshot = RateLimitsMapper.Map(Json("""
        {"rateLimits":{"primary":{"usedPercent":37,"windowDurationMins":10080},"secondary":null}}
        """));
    Equal(63d, snapshot.Week.RemainingPercent);
    True(snapshot.Week.IsAvailable);
    False(snapshot.FiveHours.IsAvailable);
}

static void TestNullWindow()
{
    var snapshot = RateLimitsMapper.Map(Json("""
        {"rateLimits":{"primary":null,"secondary":null}}
        """));
    False(snapshot.FiveHours.IsAvailable);
    False(snapshot.Week.IsAvailable);
}

static void TestUnknownWindow()
{
    var snapshot = RateLimitsMapper.Map(Json("""
        {"rateLimits":{"primary":{"usedPercent":7,"windowDurationMins":1440}}}
        """));
    False(snapshot.FiveHours.IsAvailable);
    False(snapshot.Week.IsAvailable);
}

static void TestResetCreditCount()
{
    var numeric = RateLimitsMapper.Map(Json("""
        {"rateLimits":{"primary":null,"secondary":null},"rateLimitResetCredits":{"availableCount":7}}
        """));
    Equal(7, numeric.ResetCreditCount);

    var text = RateLimitsMapper.Map(Json("""
        {"rateLimits":{"primary":null,"secondary":null},"rateLimitResetCredits":{"availableCount":"3"}}
        """));
    Equal(3, text.ResetCreditCount);
}

static void TestResetCreditsMapper()
{
    var credits = ResetCreditsMapper.Map(Json("""
        {
          "data": {
            "credits": [
              {"issuedAt":1893456000000,"expiresAt":1896048000000},
              {"created_at":"2030-02-01T00:00:00Z","expires_at":"2030-03-01T00:00:00Z"}
            ]
          }
        }
        """));
    Equal(2, credits.Count);
    Equal(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero), credits[0].IssuedAt.ToUniversalTime());
    Equal(new DateTimeOffset(2030, 1, 31, 0, 0, 0, TimeSpan.Zero), credits[0].ExpiresAt.ToUniversalTime());
}

static void TestResetCreditCountdown()
{
    var now = new DateTimeOffset(2030, 1, 1, 12, 0, 0, TimeSpan.FromHours(8));
    Equal("2天6时", ResetCreditCountdown.Format(now, now.AddDays(2).AddHours(6).AddMinutes(10)));
    Equal("6时15分", ResetCreditCountdown.Format(now, now.AddHours(6).AddMinutes(15)));
    Equal("45分", ResetCreditCountdown.Format(now, now.AddMinutes(45)));
    Equal("已到期", ResetCreditCountdown.Format(now, now.AddSeconds(-1)));
}

static void TestResetReminderPolicy()
{
    var now = new DateTimeOffset(2030, 1, 1, 12, 0, 0, TimeSpan.FromHours(8));
    Equal(ResetReminderLevel.None, ResetReminderPolicy.Evaluate(now, now.AddHours(30), 2, true));
    Equal(ResetReminderLevel.Within24Hours, ResetReminderPolicy.Evaluate(now, now.AddHours(20), 2, true));
    Equal(ResetReminderLevel.Within6Hours, ResetReminderPolicy.Evaluate(now, now.AddHours(5), 2, true));
    Equal(ResetReminderLevel.Within1Hour, ResetReminderPolicy.Evaluate(now, now.AddMinutes(30), 2, true));
    Equal(ResetReminderLevel.Expired, ResetReminderPolicy.Evaluate(now, now.AddMinutes(-1), 2, true));
    Equal(ResetReminderLevel.None, ResetReminderPolicy.Evaluate(now, now.AddMinutes(30), 0, true));
    Equal(ResetReminderLevel.None, ResetReminderPolicy.Evaluate(now, now.AddMinutes(30), 2, false));
}

static void TestMinimalSecondaryDisplay()
{
    var now = new DateTimeOffset(2030, 1, 1, 12, 0, 0, TimeSpan.FromHours(8));
    var fiveHours = new QuotaValue(QuotaPeriod.FiveHours, true, 12, 88, 300, now.AddHours(2));
    var week = new QuotaValue(QuotaPeriod.Week, true, 34, 66, 10080, now.AddDays(6));
    var snapshot = new QuotaSnapshot(fiveHours, week, 1, now);

    var regular = MinimalSecondaryDisplayPolicy.Select(
        QuotaPeriod.FiveHours, snapshot, 1, now.AddDays(4), now);
    Equal(MinimalSecondaryKind.OtherQuota, regular.Kind);
    Equal(QuotaPeriod.Week, regular.Quota!.Period);
    Equal(now.AddHours(2), regular.SelectedQuotaResetsAt);

    var urgentCard = MinimalSecondaryDisplayPolicy.Select(
        QuotaPeriod.FiveHours, snapshot, 1, now.AddDays(3), now);
    Equal(MinimalSecondaryKind.ResetCredit, urgentCard.Kind);

    var switchedMain = MinimalSecondaryDisplayPolicy.Select(
        QuotaPeriod.Week, snapshot, 1, now.AddDays(4), now);
    Equal(MinimalSecondaryKind.OtherQuota, switchedMain.Kind);
    Equal(QuotaPeriod.FiveHours, switchedMain.Quota!.Period);
    Equal(now.AddDays(6), switchedMain.SelectedQuotaResetsAt);

    var oneWindow = new QuotaSnapshot(fiveHours, QuotaValue.Unavailable(QuotaPeriod.Week), 1, now);
    var fallbackCard = MinimalSecondaryDisplayPolicy.Select(
        QuotaPeriod.FiveHours, oneWindow, 1, now.AddDays(4), now);
    Equal(MinimalSecondaryKind.ResetCredit, fallbackCard.Kind);
}

static void TestQuotaCriticalAlert()
{
    True(QuotaAlertPolicy.IsCritical(new QuotaValue(
        QuotaPeriod.Week, true, 95.1, 4.9, 10080, null)));
    True(QuotaAlertPolicy.IsCritical(new QuotaValue(
        QuotaPeriod.FiveHours, true, 97, 3, 300, null)));
    False(QuotaAlertPolicy.IsCritical(new QuotaValue(
        QuotaPeriod.Week, true, 95, 5, 10080, null)));
    False(QuotaAlertPolicy.IsCritical(QuotaValue.Unavailable(QuotaPeriod.FiveHours)));
    True(QuotaAlertPolicy.IsAnyCritical(new QuotaSnapshot(
        new QuotaValue(QuotaPeriod.FiveHours, true, 97, 3, 300, null),
        new QuotaValue(QuotaPeriod.Week, true, 5, 95, 10080, null),
        null,
        DateTimeOffset.Now)));
}

static void TestCodexDesktopProcessClassifier()
{
    True(CodexDesktopProcessClassifier.IsDesktopApp(
        "Codex", @"C:\Program Files\WindowsApps\OpenAI.Codex_26.0_x64__2p2nqsd0c76g0\app\Codex.exe", false));
    True(CodexDesktopProcessClassifier.IsDesktopApp("codex", null, true));
    False(CodexDesktopProcessClassifier.IsDesktopApp("codex", null, false));
    False(CodexDesktopProcessClassifier.IsDesktopApp("node", @"C:\tools\codex.exe", true));
    True(CodexDesktopProcessClassifier.IsCodexDesktopPackage("OpenAI.Codex_2p2nqsd0c76g0"));
    False(CodexDesktopProcessClassifier.IsCodexDesktopPackage("OpenAI.ChatGPT_2p2nqsd0c76g0"));
}

static void TestTrayEmojiValue()
{
    True(TrayEmojiValue.TryNormalize("  🤖  ", out var robot));
    Equal("🤖", robot);
    True(TrayEmojiValue.TryNormalize("👨‍💻", out var developer));
    Equal("👨‍💻", developer);
    False(TrayEmojiValue.TryNormalize("🤖⚡", out _));
    False(TrayEmojiValue.TryNormalize("", out _));
}

static void TestLineParser()
{
    True(JsonRpcLineParser.TryParse(
        "{\"jsonrpc\":\"2.0\",\"method\":\"account/rateLimits/updated\",\"params\":{\"rateLimits\":{}}}",
        out var notification));
    Equal("account/rateLimits/updated", notification!.Method);
    True(notification.Params.HasValue);

    True(JsonRpcLineParser.TryParse(
        "{\"jsonrpc\":\"2.0\",\"id\":9,\"result\":{\"ok\":true}}",
        out var response));
    Equal(9L, response!.Id);
    True(response.Result!.Value.GetProperty("ok").GetBoolean());
    False(JsonRpcLineParser.TryParse("not json", out _));
}

static async Task TestRequestCorrelation()
{
    var tracker = new JsonRpcRequestTracker();
    var pending = tracker.Register(42);
    False(tracker.TryResolve(new JsonRpcEnvelope(
        41, null, null, Json("{\"wrong\":true}"), null)));
    True(tracker.TryResolve(new JsonRpcEnvelope(
        42, null, null, Json("{\"remaining\":84}"), null)));
    var result = await pending;
    Equal(84, result.GetProperty("remaining").GetInt32());
}

static void True(bool condition)
{
    if (!condition) throw new InvalidOperationException("Expected true.");
}

static void False(bool condition) => True(!condition);

static void Equal<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"Expected {expected}, got {actual}.");
    }
}

static async Task<int> RunLiveProbeAsync()
{
    await using var service = new CodexQuotaService(new CodexAppServerClient());
    QuotaSnapshot? snapshot = null;
    service.SnapshotReceived += value => snapshot = value;
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
    await service.StartAsync(timeout.Token);
    if (snapshot is null)
    {
        Console.Error.WriteLine("Live probe returned no snapshot.");
        return 1;
    }

    Console.WriteLine(
        $"LIVE 5H={Format(snapshot.FiveHours)} WEEK={Format(snapshot.Week)} RESET_CARDS={snapshot.ResetCreditCount?.ToString() ?? "unknown"} SYNCED={snapshot.SyncedAt:O}");
    return 0;

    static string Format(QuotaValue value) => value.IsAvailable
        ? $"{value.RemainingPercent:0.#}%({value.WindowDurationMins}m; resets={value.ResetsAt:O})"
        : "unavailable";
}

static async Task<int> RunLiveResetCreditsProbeAsync()
{
    using var client = new ResetCreditsClient();
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(25));
    var credits = await client.ReadAsync(timeout.Token);
    Console.WriteLine($"LIVE_RESET_CARDS={credits.Count}");
    foreach (var credit in credits)
    {
        Console.WriteLine($"CARD issued={credit.IssuedAt.ToLocalTime():yyyy-MM-dd HH:mm:ss} expires={credit.ExpiresAt.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
    }
    return 0;
}
