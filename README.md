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

### 30 秒开始用

1. 前往 [Releases](https://github.com/skystart233-code/CodexQuotaWidget/releases/latest)，下载并解压 `CodexQuotaWidget-*-win-x64.zip`。
2. 确认电脑已安装 [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)，并且 `codex` CLI 已登录且在系统 `PATH` 中。
3. 双击运行 `CodexQuotaWidget.App.exe`。右键浮窗或托盘图标，随时切换语言、主题、额度视图和图标。

就这么简单，不需要额外配置。

### 关于隐私,你可以放心

所有额度数据都直接来自你本机已登录的 `codex app-server`，不涉及任何网页抓取。重置时间的查询也只使用你本机现有的登录凭据，查询结果和个人偏好设置全部保存在本地的 `%LOCALAPPDATA%\CodexQuotaWidget` 目录下。整个过程不会上传任何 token、额度信息或使用记录到任何第三方服务器,细节可以查看 [SECURITY.md](SECURITY.md)。

> 小提示：Codex 官方接口如果发生变动，重置时间查询可能会暂时失效，但这不影响额度浮窗的正常显示，它会保留最近一次成功获取的倒计时。

### 想从源码运行？

```powershell
dotnet restore --ignore-failed-sources
dotnet run --project .\src\CodexQuotaWidget.App\CodexQuotaWidget.App.csproj


<a id="english"></a>

## English

Your Codex quota, always visible and never in the way. A glance tells you what is left; a right-click gives you the controls.

### Why it sticks

- Toggle 5H and weekly quota with the actual remaining percentage
- Minimal mode shows quota plus the next reset-card countdown
- The whole widget turns red when weekly quota drops below 5%
- Automatically finds the earliest available reset card and reminds you at 24h / 6h / 1h
- Four themes, a custom tray emoji, and instant Chinese / English switching
- Draggable, content-sized, and safely minimized to the tray

### Get going in 30 seconds

1. Download and unzip the latest `CodexQuotaWidget-*-win-x64.zip` from [Releases](https://github.com/skystart233-code/CodexQuotaWidget/releases/latest).
2. Install the [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0), then ensure the signed-in `codex` CLI is on `PATH`.
3. Start `CodexQuotaWidget.App.exe`. Right-click the widget or tray icon to change language, theme, quota window, or emoji.

### Privacy, plainly

Quota data comes from the local `codex app-server`, never browser scraping. Reset-card lookup uses only your existing local Codex credentials; expiry data and preferences stay in `%LOCALAPPDATA%\CodexQuotaWidget`. No token, quota data, or activity is uploaded. Read [SECURITY.md](SECURITY.md) for the details.

> The app-server and reset-card endpoints can change as Codex evolves. If the reset-card lookup is unavailable, quota monitoring keeps working and the last successful countdown remains visible.

### Build from source

```powershell
dotnet restore --ignore-failed-sources
dotnet run --project .\src\CodexQuotaWidget.App\CodexQuotaWidget.App.csproj
```

## For contributors

```powershell
dotnet build .\CodexQuotaWidget.slnx --no-restore
dotnet test .\CodexQuotaWidget.slnx --no-build
```

The repo is split into a small core, the Codex protocol client, the WPF app, and offline tests. Issues and practical ideas are welcome.
