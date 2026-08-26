<div align="center">

# ⚡ CodexQuotaWidget

### 实时监控 Codex 额度与重置积分的 Windows 悬浮小组件

**别再被限速打断了。** 一个常驻桌面角落、几乎零打扰的 Codex quota monitor：5 小时额度、周额度、当前窗口的下次重置与 Reset Credit 到期时间，抬头即见；低额自动变红，重置前主动提醒。

[![Release](https://img.shields.io/github/v/release/skystart233-code/CodexQuotaWidget?style=for-the-badge)](https://github.com/skystart233-code/CodexQuotaWidget/releases/latest)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg?style=for-the-badge)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows-0078D6?style=for-the-badge)](README.md)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Language](https://img.shields.io/badge/language-C%23-239120?style=for-the-badge)](https://github.com/skystart233-code/CodexQuotaWidget)

[⬇️ 下载最新版](https://github.com/skystart233-code/CodexQuotaWidget/releases/latest) · [📖 中文文档](#中文) · [📖 English](#english) · [🔒 隐私与安全](SECURITY.md)

</div>

---

## 为什么需要 CodexQuotaWidget？

你正在用 **OpenAI Codex** 心流写代码，突然弹出 rate limit 提示——才发现 5 小时额度或周额度早就见底了。切到终端查 `codex` 命令、翻设置页面找用量，每次都要打断思路。

**CodexQuotaWidget** 把 Codex quota status 钉在屏幕角落：

- 不用切标签页、不用敲命令、不用翻设置。
- 5-hour quota 与 weekly quota **分开读取、分开显示**。
- 正常的极简模式会显示当前额度的下次重置倒计时；重置卡进入最后 3 天时优先提醒。
- 低于 5% 时整个浮窗**变红预警**，不再“突然被限速”。
- Reset Credit 到期前 **24h / 6h / 1h** 主动发送托盘通知。
- 可设置“跟随 Codex 启动和关闭”，只在使用 Codex 时出现。

这是一个为 Codex 重度用户打造的**桌面级配额监控工具**：轻量、无感，但永远在需要的地方。

---

## ✨ 核心功能

### 📊 双额度实时监控

同时追踪 Codex 的 **5 小时窗口**和**每周配额**，按 app-server 返回的窗口时长自动识别，不依赖 `primary / secondary` 字段顺序。右键一键切换视图，显示真实剩余百分比；app-server 返回重置时间时，完整模式还会显示当前视图的下次重置。

### 🪟 极简悬浮设计

- 常驻桌面角落，**不占任务栏**；可自由拖动，按内容自适应大小。
- 关闭即收进**系统托盘**，随时呼出。
- 完整模式会显示当前额度的具体下次重置时间和实时倒计时。
- 极简模式优先呈现“当前该知道什么”：左侧是主额度，右侧先是**主额度倒计时**，再是另一条额度。

```text
● 5H 57% │ 4时6分 · 周 93%
```

切到周额度时逻辑完全对称：右侧先显示周额度倒计时，再显示 5H 余量。倒计时不再有冗余的“重置”字样，但它始终属于左侧当前主额度。

### ⏰ Reset Credit 倒计时与提醒

自动追踪最近一张 **Reset Credit（重置卡）**的到期时间，并在到期前 24 小时、6 小时、1 小时发送系统托盘通知。当重置卡进入最后 **3 天**，它会临时优先占据极简模式右侧：

```text
● 5H 57% │ 卡 2天6时
```

这样既不会错过重置卡，也不会在平时挤掉当前 5H / 周额度的下次重置。

### 🔴 智能低额预警

当 5 小时额度或周额度任一低于 **5%** 时，整个浮窗变红——肉眼可见的警告，让你在被限速之前就知道该留余量了。

### 🎨 四套主题 + 自定义托盘图标

内置深夜蓝、石墨黑、纸张白、极光绿四套主题；支持自定义**彩色 Emoji** 托盘图标，以及中英文一键切换。让它融入你的桌面，而不是突兀地存在。

### 🤝 跟随 Codex 生命周期

勾选“**跟随 Codex 启动和关闭**”后：

- 登录 Windows 后静默等待，不显示浮窗。
- 检测到 Codex 打开时自动启动浮窗。
- Codex 真正退出后几秒内浮窗自动关闭。
- 使用轻量级的当前用户后台等待器，可随时取消勾选。

### 🔒 隐私优先，本地运行

额度数据直接读取本机已登录的 `codex app-server`；**不抓浏览器、不上传第三方**。Reset Credit 查询只使用本机已有的 Codex 登录凭据，令牌只在内存中使用。UI 偏好与最近缓存的重置卡到期时间保存在 `%LOCALAPPDATA%\CodexQuotaWidget`。详见 [SECURITY.md](SECURITY.md)。

---

## 🚀 30 秒快速开始

### 前置要求

- **Windows 10/11** x64
- [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
- 已登录且在系统 `PATH` 中的 `codex` CLI

### 安装步骤

1. 前往 [Releases 页面](https://github.com/skystart233-code/CodexQuotaWidget/releases/latest)，下载 `CodexQuotaWidget-*-win-x64.zip`。
2. 解压到任意目录。
3. 双击运行 `CodexQuotaWidget.App.exe`。

就这么简单——不需要额外配置，不需要注册账号，不需要 API Key。

> **提示：** 右键浮窗或托盘图标，可随时切换语言、主题、5H / 周额度视图和托盘 Emoji。

---

## ⚙️ 配置与使用

### 切换额度视图

右键浮窗 → 在 **5H 剩余** 与 **周额度剩余** 间切换。浮窗按 API 返回的窗口时长识别额度类型；完整模式显示当前额度的下一次重置，极简模式把该倒计时放在右侧第一位。

### 跟随 Codex 启动和关闭

右键浮窗或托盘图标 → 勾选 **跟随 Codex 启动和关闭**。系统会为当前用户登记一个轻量后台等待器：登录后静默等待，Codex 打开时显示浮窗，退出后自动关闭。随时取消勾选即可关闭此行为。

### 自定义托盘图标、主题与语言

右键 → 选择托盘 Emoji，可从预设中挑选或输入自己的 Emoji；同一菜单也可切换四套主题与中文 / English。

---

## 🏗️ 技术架构

```text
CodexQuotaWidget
├── Core                         # 核心领域模型与策略
│   ├── QuotaModels               # 5H / 周额度数据模型
│   ├── ResetCreditModels         # 重置卡与倒计时模型
│   ├── RateLimitsMapper          # 按窗口时长映射额度
│   ├── QuotaAlertPolicy          # 低额预警（< 5% 变红）
│   ├── ResetReminderPolicy       # 重置卡提醒（24h / 6h / 1h）
│   └── MinimalSecondaryDisplayPolicy # 极简模式显示优先级
├── Codex                        # 本地 Codex 协议客户端
│   ├── CodexAppServerClient      # app-server JSON-RPC 客户端
│   ├── CodexQuotaService         # 额度订阅与查询服务
│   ├── ResetCreditsClient        # Reset Credit 查询
│   └── ResetCreditsMapper        # 返回数据映射
├── App                          # WPF 桌面应用
│   ├── MainWindow                # 悬浮窗与倒计时显示
│   ├── CodexLifecycle            # Codex 进程生命周期监听
│   ├── SettingsStore             # 本地设置存储
│   ├── StartupRegistration       # 跟随 Codex 的启动注册
│   ├── TrayEmojiIconFactory      # 彩色 Emoji 托盘图标
│   └── UiText                    # 中英文本资源
└── Tests                         # 离线验证程序
```

### 数据来源

- **Codex quota / rate limit：** 通过本地 `codex app-server` 的 JSON-RPC 接口读取。
- **Reset Credit：** 使用本机已有登录凭据查询，不写入设置或日志。
- **本地存储：** `%LOCALAPPDATA%\CodexQuotaWidget`，仅保存 UI 偏好、窗口位置、最近缓存的到期时间与提醒状态。

> **兼容性说明：** 如果 Codex 的内部接口发生变动，Reset Credit 查询可能暂时失效；额度监控仍可继续工作，浮窗会保留最近一次成功获取的卡片倒计时。

---

## ❓ 常见问题

### Q: 这个工具会上传我的数据吗？

**不会。** 额度从本机 `codex app-server` 读取；重置卡查询仅使用现有本机凭据。应用不会上传 token、额度信息或使用记录到第三方服务器。详见 [SECURITY.md](SECURITY.md)。

### Q: 为什么需要 .NET 8 Desktop Runtime？

CodexQuotaWidget 基于 WPF（Windows Presentation Foundation）构建，需要 .NET 8 Desktop Runtime 才能运行。如果尚未安装，可从 [微软官网](https://dotnet.microsoft.com/download/dotnet/8.0) 免费下载。

### Q: 支持 macOS / Linux 吗？

目前仅支持 Windows。核心逻辑与 UI 分离；如果有跨平台需求，欢迎提交 Issue 或 PR 讨论。

### Q: 5 小时额度和周额度有什么区别？

Codex 使用独立的 5 小时窗口与周额度。前者反映短期可用量，后者反映更长周期的总可用量；CodexQuotaWidget 分别追踪两者的剩余百分比与重置时间。

### Q: 极简模式里的倒计时是谁的？

它属于左侧当前显示的主额度。比如 `● 5H 57% │ 4时6分 · 周 93%` 中，`4时6分` 是 5H 的倒计时；切到周额度后则代表周额度倒计时。只有在重置卡最后 3 天内，右侧才会优先显示 `卡 2天6时`。

### Q: 浮窗不显示额度怎么办？

1. 确认 `codex` CLI 已登录且在系统 `PATH` 中。
2. 确认 Codex 桌面端或 CLI 正在运行。
3. 右键浮窗选择立即刷新，或尝试重启 CodexQuotaWidget。

### Q: 如何从源码运行？

```powershell
dotnet restore --ignore-failed-sources
dotnet run --project .\src\CodexQuotaWidget.App\CodexQuotaWidget.App.csproj
```

---

## 🤝 贡献

欢迎提交 Issue、功能建议和 Pull Request。

### 构建与测试

```powershell
dotnet build .\CodexQuotaWidget.slnx --no-restore
dotnet run --project .\tests\CodexQuotaWidget.Tests\CodexQuotaWidget.Tests.csproj --no-build
```

---

## 📄 许可证

[MIT License](LICENSE) — 免费使用，自由修改。

---

## ⭐ 支持这个项目

如果 CodexQuotaWidget 帮你避免了被限速打断的尴尬，给个 Star 就是最大的支持。

也欢迎在 Issue 区分享你的使用场景和功能建议。

---

<a id="english"></a>

## English

### Why CodexQuotaWidget?

You are in the zone with **OpenAI Codex**, then a rate-limit message lands. Your 5-hour or weekly quota had been running low for a while; checking the terminal or a settings page breaks the flow again.

**CodexQuotaWidget** pins Codex quota status to the corner of your screen:

- No tab switching, terminal commands, or settings digging.
- **5-hour quota** and **weekly quota** read and displayed separately.
- In normal minimal mode, the next reset for the selected quota is visible; an expiring Reset Credit takes priority in its final three days.
- The whole widget **turns red** below 5%, before a limit interrupts you.
- **Reset Credit** notifications at 24h / 6h / 1h before expiry.
- Optional **Follow Codex start and exit** mode, so it appears only when Codex is in use.

It is a lightweight, desktop-grade **Codex quota monitor** for people who spend real time in Codex.

---

### ✨ Key Features

#### 📊 Dual Quota Monitoring

Tracks the **5-hour window** and **weekly quota** returned by Codex. Windows are identified by their duration, not by `primary / secondary` field order. Right-click to switch views and see the real remaining percentage plus the next reset for the selected quota.

#### 🪟 Minimal Floating Design

- Stays out of the taskbar, drags anywhere, and auto-sizes to content.
- Closing sends it to the **system tray**, ready when you need it.
- When the local app-server returns a reset time, expanded view shows the selected quota's exact next-reset time and live countdown.
- Minimal view puts the selected quota on the left, then its **countdown**, then the other quota on the right.

```text
● 5H 57% │ 4h 6m · Wk 93%
```

The weekly view follows the same rule: weekly countdown first, 5H balance second. The compact countdown has no redundant “resets” label, but always belongs to the selected quota on the left.

#### ⏰ Reset Credit Countdown & Notifications

Tracks the nearest **Reset Credit** expiry and sends system-tray notifications at 24h, 6h, and 1h before expiry. In its final **three days**, an expiring card temporarily takes over the right side of minimal mode:

```text
● 5H 57% │ Card 2d 6h
```

That keeps a card from expiring unnoticed without hiding the normal 5-hour / weekly reset at other times.

#### 🔴 Smart Low-Quota Alert

The entire widget turns **red** when either the 5-hour or weekly quota falls below **5%**. You get a visible signal before a rate limit gets in the way.

#### 🎨 4 Themes + Custom Tray Icon

Choose Midnight, Graphite, Paper, or Aurora. Use a custom, full-colour Emoji tray icon and switch Chinese / English instantly.

#### 🤝 Follow Codex Lifecycle

With **Follow Codex start and exit** enabled, the widget waits quietly after Windows sign-in, appears when Codex opens, and exits a few seconds after Codex truly closes. The per-user watcher is lightweight and can be disabled at any time.

#### 🔒 Privacy-First, Local-Only

Quota data comes from your signed-in local `codex app-server`: no browser scraping and no third-party uploads. Reset Credit lookup uses existing local Codex credentials in memory only. UI preferences and the cached nearest Reset Credit expiry stay in `%LOCALAPPDATA%\CodexQuotaWidget`. See [SECURITY.md](SECURITY.md).

---

### 🚀 Get Started in 30 Seconds

#### Prerequisites

- **Windows 10/11** x64
- [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
- A signed-in `codex` CLI on your system `PATH`

#### Installation

1. Go to [Releases](https://github.com/skystart233-code/CodexQuotaWidget/releases/latest) and download `CodexQuotaWidget-*-win-x64.zip`.
2. Extract it anywhere.
3. Double-click `CodexQuotaWidget.App.exe`.

That is it: no extra configuration, account registration, or API key.

> **Tip:** Right-click the widget or tray icon to change language, theme, 5H / weekly view, or tray Emoji.

---

### ⚙️ Configuration & Usage

#### Switch Quota View

Right-click the widget → choose **5H remaining** or **Weekly remaining**. The widget identifies the quota by the returned window duration. Expanded view shows the next reset; minimal view puts that countdown first on the right.

#### Follow Codex Start and Exit

Right-click the widget or tray icon → check **Follow Codex start and exit**. A lightweight per-user watcher waits after sign-in, starts the widget when Codex opens, and closes it after Codex quits. Uncheck it any time to disable the behavior.

#### Tray Icon, Theme, and Language

Right-click → choose a preset tray Emoji or enter your own; the same menu also switches the four themes and Chinese / English.

---

### 🏗️ Architecture

```text
CodexQuotaWidget
├── Core                         # Domain models and display policies
│   ├── QuotaModels               # 5H / weekly quota models
│   ├── ResetCreditModels         # Reset-credit and countdown models
│   ├── RateLimitsMapper          # Window-duration mapping
│   ├── QuotaAlertPolicy          # < 5% warning state
│   ├── ResetReminderPolicy       # 24h / 6h / 1h reminders
│   └── MinimalSecondaryDisplayPolicy # Compact display priority
├── Codex                        # Local Codex protocol client
│   ├── CodexAppServerClient      # app-server JSON-RPC client
│   ├── CodexQuotaService         # Quota subscription and queries
│   ├── ResetCreditsClient        # Reset Credit lookup
│   └── ResetCreditsMapper        # Response mapping
├── App                          # WPF desktop application
│   ├── MainWindow                # Floating UI and countdowns
│   ├── CodexLifecycle            # Desktop-process lifecycle listener
│   ├── SettingsStore             # Local settings
│   ├── StartupRegistration       # Follow-Codex startup registration
│   ├── TrayEmojiIconFactory      # Full-colour Emoji tray icon
│   └── UiText                    # Bilingual UI text
└── Tests                         # Offline verification program
```

#### Data Sources

- **Codex quota / rate limit:** Read from the local `codex app-server` JSON-RPC interface.
- **Reset Credits:** Queried with existing local credentials and never written to settings or logs.
- **Local storage:** `%LOCALAPPDATA%\CodexQuotaWidget`, containing only UI preferences, window position, cached nearest expiry, and reminder state.

> **Compatibility note:** If Codex changes an internal interface, Reset Credit lookup may temporarily stop working. Quota monitoring continues, and the widget keeps the most recently retrieved card countdown.

---

### ❓ FAQ

#### Q: Does this tool upload my data?

**No.** Quota data is read from the local `codex app-server`; Reset Credit lookup only uses existing local credentials. The app does not upload tokens, quota data, or activity records to third parties. See [SECURITY.md](SECURITY.md).

#### Q: Why do I need .NET 8 Desktop Runtime?

CodexQuotaWidget is built with WPF (Windows Presentation Foundation) and requires the .NET 8 Desktop Runtime. Download it free from [Microsoft](https://dotnet.microsoft.com/download/dotnet/8.0).

#### Q: Does it support macOS / Linux?

It currently supports Windows only. The core logic is separated from the UI; cross-platform support is open for discussion in Issues or PRs.

#### Q: What is the difference between 5-hour and weekly quota?

Codex uses independent 5-hour and weekly usage windows. The former represents short-term availability and the latter longer-horizon availability. CodexQuotaWidget tracks their remaining percentages and reset times independently.

#### Q: Which quota does the minimal countdown belong to?

It belongs to the selected quota on the left. In `● 5H 57% │ 4h 6m · Wk 93%`, `4h 6m` is the 5-hour countdown. Switch to weekly and it becomes the weekly countdown. Only an expiring Reset Credit in its final three days takes priority as `Card 2d 6h`.

#### Q: The widget shows no quota. What should I do?

1. Ensure `codex` CLI is signed in and on your system `PATH`.
2. Ensure the Codex desktop app or CLI is running.
3. Right-click the widget and refresh, or restart CodexQuotaWidget.

#### Q: How do I run from source?

```powershell
dotnet restore --ignore-failed-sources
dotnet run --project .\src\CodexQuotaWidget.App\CodexQuotaWidget.App.csproj
```

---

### 🤝 Contributing

Issues, feature suggestions, and Pull Requests are welcome.

#### Build & Test

```powershell
dotnet build .\CodexQuotaWidget.slnx --no-restore
dotnet run --project .\tests\CodexQuotaWidget.Tests\CodexQuotaWidget.Tests.csproj --no-build
```

---

### 📄 License

[MIT License](LICENSE) — free to use and modify.

---

### ⭐ Support This Project

If CodexQuotaWidget has saved you from a rate-limit interruption, a Star is the best support. Share real use cases and feature ideas in Issues.
