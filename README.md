<div align="center">

# ⚡ CodexQuotaWidget

### 别再手动查额度了，让它一直挂在你的桌面右上角

一个几乎零打扰的 Codex 额度浮窗——写代码写到一半，瞥一眼就知道还剩多少，快用完了它会主动变红提醒你。

[![Release](https://img.shields.io/github/v/release/skystart233-code/CodexQuotaWidget)](https://github.com/skystart233-code/CodexQuotaWidget/releases/latest)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows-0078D6)](README.md)

[⬇️ 下载最新版](https://github.com/skystart233-code/CodexQuotaWidget/releases/latest) · [中文](#中文) · [English](#english) · [隐私说明](SECURITY.md)

</div>

---

## 中文

你是不是也遇到过这种情况：正在用 Codex 写代码，突然被限速打断，才发现额度早就见底了，而自己完全没有察觉。

**CodexQuotaWidget** 就是为了解决这个问题而做的——一个常驻在屏幕角落、几乎不占地方的小浮窗，把你的 Codex 额度状态一直摆在眼前，不用切标签、不用敲命令，抬头就能看到。

### 它能帮你做什么

| 场景 | CodexQuotaWidget 的解法 |
| --- | --- |
| 分不清是 5 小时额度还是周额度快用完了 | 一键切换两种额度视图，直接显示真实剩余百分比 |
| 桌面太乱，不想再多个大窗口 | 极简模式，只留额度数字和倒计时，安静地待在角落 |
| 快用完了却没及时发现 | 周额度低于 5% 时整个窗口变红，肉眼可见的警告 |
| 记不清额度什么时候重置 | 自动追踪最近的重置时间，提前 24h / 6h / 1h 弹托盘提醒 |
| 想要更贴合自己的使用习惯 | 四套主题、可自定义托盘图标、中英文一键切换 |
| 不想让它占用任务栏或碍事 | 可自由拖动、随内容自适应大小，关闭即收进系统托盘 |
| 希望它只在使用 Codex 时出现 | 打开“跟随 Codex 启动和关闭”：登录后静默等待，Codex 打开时显示，真正退出 Codex 后浮窗自动退出 |

### 30 秒开始用

1. 前往 [Releases](https://github.com/skystart233-code/CodexQuotaWidget/releases/latest)，下载并解压 `CodexQuotaWidget-*-win-x64.zip`。
2. 确认电脑已安装 [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)，并且 `codex` CLI 已登录且在系统 `PATH` 中。
3. 双击运行 `CodexQuotaWidget.App.exe`。右键浮窗或托盘图标，随时切换语言、主题、额度视图和图标。

就这么简单，不需要额外配置。

> 想让它真正“跟着 Codex 走”？右键浮窗或托盘图标，勾选 **跟随 Codex 启动和关闭**。它会为当前 Windows 用户登记一个轻量后台等待器：登录后不显示浮窗，等你打开 Codex 才启动；退出 Codex 后浮窗也会在几秒内退出。随时取消勾选即可关闭此行为。

### 关于隐私,你可以放心

所有额度数据都直接来自你本机已登录的 `codex app-server`，不涉及任何网页抓取。重置时间的查询也只使用你本机现有的登录凭据，查询结果和个人偏好设置全部保存在本地的 `%LOCALAPPDATA%\CodexQuotaWidget` 目录下。整个过程不会上传任何 token、额度信息或使用记录到任何第三方服务器,细节可以查看 [SECURITY.md](SECURITY.md)。

> 小提示：Codex 官方接口如果发生变动，重置时间查询可能会暂时失效，但这不影响额度浮窗的正常显示，它会保留最近一次成功获取的倒计时。

### 想从源码运行？

```powershell
dotnet restore --ignore-failed-sources
dotnet run --project .\src\CodexQuotaWidget.App\CodexQuotaWidget.App.csproj
```

---

<a id="english"></a>

## English

Ever been interrupted by a Codex limit right in the middle of a flow — only to realise your quota had been running low for a while?

**CodexQuotaWidget** keeps a tiny, always-on quota view in the corner of your screen. No tab switching, no terminal command, no digging through settings: just look up and know where you stand.

### What it solves

| Situation | What CodexQuotaWidget does |
| --- | --- |
| Not sure whether the 5-hour or weekly quota is the one running out | Switch between both views and see the actual remaining percentage |
| Your desktop already has enough windows | Use minimal mode: just the quota and next countdown, quietly in the corner |
| You notice the limit only after it interrupts you | The whole widget turns red when weekly quota drops below 5% |
| You cannot remember when a quota resets | Tracks the nearest reset-card expiry and notifies you at 24h / 6h / 1h |
| You want it to feel like your setup | Pick from four themes, set a custom tray emoji, or switch Chinese / English instantly |
| You do not want another taskbar app in the way | Drag it anywhere, let it size itself to content, and close it back to the system tray |
| You only want it while Codex is open | Enable “Follow Codex start and exit”: it waits quietly after sign-in, appears with Codex, and exits when Codex truly quits |

### Get started in 30 seconds

1. Go to [Releases](https://github.com/skystart233-code/CodexQuotaWidget/releases/latest), download, and unzip `CodexQuotaWidget-*-win-x64.zip`.
2. Install the [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0), then make sure the signed-in `codex` CLI is on your system `PATH`.
3. Double-click `CodexQuotaWidget.App.exe`. Right-click the widget or tray icon whenever you want to switch language, theme, quota view, or emoji.

That is it — no extra setup needed.

> Want it to genuinely follow Codex? Right-click the widget or tray icon and check **Follow Codex start and exit**. A lightweight per-user watcher waits silently after sign-in, starts the widget when Codex opens, then closes it a few seconds after Codex quits. Uncheck it any time to turn this behavior off.

### Privacy, without the fine print

Quota data comes directly from the signed-in local `codex app-server`; there is no browser scraping. Reset-card lookup uses the credentials already on your machine, and results plus preferences stay in `%LOCALAPPDATA%\CodexQuotaWidget`. The app does not upload tokens, quota data, or activity records to any third party. See [SECURITY.md](SECURITY.md) for the details.

> Heads-up: if Codex changes its APIs, reset-card lookup may temporarily stop working. Quota monitoring still works, and the widget keeps the last successfully retrieved countdown.

### Run from source

```powershell
dotnet restore --ignore-failed-sources
dotnet run --project .\src\CodexQuotaWidget.App\CodexQuotaWidget.App.csproj
```

## Contributing

```powershell
dotnet build .\CodexQuotaWidget.slnx --no-restore
dotnet test .\CodexQuotaWidget.slnx --no-build
```

The repo is split into a small core, the Codex protocol client, the WPF app, and offline tests. Issues and practical ideas are welcome.
