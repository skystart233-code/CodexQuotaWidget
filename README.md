<div align="center">

# ⚡ CodexQuotaWidget

### Keep your Codex limits in sight — before they interrupt your flow.

A tiny Windows widget for Codex's 5-hour and weekly limits. It stays out of the way, tells you what remains, and makes the next reset impossible to miss.

[![Latest release](https://img.shields.io/github/v/release/skystart233-code/CodexQuotaWidget?label=release)](https://github.com/skystart233-code/CodexQuotaWidget/releases/latest)
[![Windows](https://img.shields.io/badge/platform-Windows-0078D6)](https://github.com/skystart233-code/CodexQuotaWidget/releases/latest)
[![MIT License](https://img.shields.io/badge/license-MIT-1f6feb)](LICENSE)

[⬇ Download for Windows](https://github.com/skystart233-code/CodexQuotaWidget/releases/latest) · [中文](#中文) · [English](#english) · [Privacy](SECURITY.md)

</div>

---

## 中文

写得正顺，最不该发生的事，就是被 Codex 的额度上限突然打断。

**CodexQuotaWidget** 只做一件事：在你来得及调整节奏之前，把 **5H、周额度和下次重置时间** 放在桌面一眼可见的位置。它不占任务栏、不要求你切网页，也不打断正在写的代码。

### 一眼就知道现在该不该收手

| 你看到的 | 它告诉你的事 |
| --- | --- |
| `5H 72%` | 当前 5 小时窗口还剩多少 |
| `2时 · 周 96%` | **当前 5H 额度**多久重置，以及周额度够不够 |
| `卡 2天6时` | 有重置卡将在 3 天内到期，应该优先用掉 |
| 整个浮窗变红 | 5H 或周额度任一项低于 5%，该留点余量了 |

### 该显示什么，它有明确的优先级

正常的极简模式会把主额度放左侧、另一条额度和**主额度的重置倒计时**放右侧：

```text
● 5H 72% │ 2时 · 周 96%
```

如果最近一张重置卡在 3 天内到期，右侧自动切换为卡的倒计时：

```text
● 5H 72% │ 卡 2天6时
```

切到周额度时也是同一套逻辑：右侧会显示 5H 余量与**周额度**的下次重置。没有模糊的“这是谁的倒计时”。

### 30 秒上手

1. 从 [Releases](https://github.com/skystart233-code/CodexQuotaWidget/releases/latest) 下载并解压 `CodexQuotaWidget-*-win-x64.zip`。
2. 安装 [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)。确保 `codex` CLI 已登录，并且在系统 `PATH` 中。
3. 双击 `CodexQuotaWidget.App.exe`。
4. 右键浮窗或托盘图标，即可切换 **5H / 周额度**、主题、语言、托盘 Emoji 与跟随 Codex 模式。

完成。关闭窗口会收进系统托盘，不会消失。

### 小到不碍眼，也足够顺手

- 极简 / 完整两种浮窗，按内容自动收放，任意拖拽。
- 深夜蓝、石墨黑、纸张白、极光绿四套主题。
- 自定义彩色 Emoji 托盘图标。
- 5H 与周额度低于 5% 时整体变红。
- 最近重置卡将在 24 小时、6 小时、1 小时到期时托盘提醒。
- 可选“跟随 Codex 启动和关闭”：登录后静默等待，打开 Codex 才显示；真正退出 Codex 后浮窗也会退出。

### 隐私，没有绕弯子

额度直接由你本机已登录的 `codex app-server` 读取；不会抓网页。重置卡信息只使用本机已有登录凭据。偏好设置与最近一次成功读取的结果只保存在 `%LOCALAPPDATA%\CodexQuotaWidget`，不会上传 token、额度数据或使用记录。

完整边界见 [SECURITY.md](SECURITY.md)。如果 Codex 内部接口变化，重置卡查询可能暂时不可用；额度浮窗仍会继续工作，并保留最近一次成功的卡片倒计时。

### 从源码运行

```powershell
dotnet restore --ignore-failed-sources
dotnet run --project .\src\CodexQuotaWidget.App\CodexQuotaWidget.App.csproj
```

---

<a id="english"></a>

## English

The worst time to discover a Codex limit is in the middle of a good run.

**CodexQuotaWidget** does one job: it keeps your **5-hour limit, weekly limit, and next reset** visible before you need them. No browser tab, no terminal command, no extra taskbar window — just the information that protects your flow.

### Know what matters at a glance

| What you see | What it means |
| --- | --- |
| `5H 72%` | What remains in the current five-hour window |
| `2h · Wk 96%` | When the **current 5H limit** resets, plus your weekly headroom |
| `Card 2d 6h` | A reset card expires within three days, so it takes priority |
| The whole widget turns red | Either the 5-hour or weekly limit is below 5%; time to leave some headroom |

### A compact display with deliberate priorities

In its usual minimal mode, the left side shows the selected quota. The right side shows the other quota and the **selected quota's reset countdown**:

```text
● 5H 72% │ 2h · Wk 96%
```

When the nearest reset card expires within three days, the right side becomes the card countdown instead:

```text
● 5H 72% │ Card 2d 6h
```

Switch to the weekly view and the rule remains symmetrical: see the 5H balance and the next **weekly** reset. Every countdown has an owner.

### Start in 30 seconds

1. Download and unzip `CodexQuotaWidget-*-win-x64.zip` from [Releases](https://github.com/skystart233-code/CodexQuotaWidget/releases/latest).
2. Install the [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0). Make sure the signed-in `codex` CLI is on your system `PATH`.
3. Double-click `CodexQuotaWidget.App.exe`.
4. Right-click the widget or tray icon to change the **5H / weekly view**, theme, language, tray emoji, or Follow Codex mode.

That is all. Closing the window sends it to the system tray; it stays available.

### Small on screen, complete in the details

- Minimal and expanded views that resize to content and drag anywhere.
- Four themes: Midnight, Graphite, Paper, and Aurora.
- A custom, full-colour Emoji tray icon.
- A red warning state when either quota drops below 5%.
- Tray reminders at 24h, 6h, and 1h before the nearest reset card expires.
- Optional **Follow Codex start and exit** mode: wait silently after Windows sign-in, appear when Codex opens, and exit shortly after Codex truly closes.

### Privacy, plainly stated

Quota data comes from your signed-in local `codex app-server`; the app does not scrape a browser. Reset-card lookup uses credentials already present on your machine. Preferences and the most recent successful result stay in `%LOCALAPPDATA%\CodexQuotaWidget`. The app does not upload tokens, quota data, or activity records.

See [SECURITY.md](SECURITY.md) for the complete boundary. If Codex changes an internal interface, reset-card lookup may temporarily stop working; quota monitoring continues and the widget keeps the last successful card countdown.

### Run from source

```powershell
dotnet restore --ignore-failed-sources
dotnet run --project .\src\CodexQuotaWidget.App\CodexQuotaWidget.App.csproj
```

## Contributing

```powershell
dotnet build .\CodexQuotaWidget.slnx --no-restore
dotnet run --project .\tests\CodexQuotaWidget.Tests\CodexQuotaWidget.Tests.csproj --no-build
```

The project is intentionally small: a core quota model, a local Codex protocol client, a WPF widget, and offline checks. Issues and practical ideas are welcome.
