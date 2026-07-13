# ⚡ CodexQuotaWidget

> Keep your Codex quota in your peripheral vision — not in another tab.

[中文](#中文) · [English](#english) · [Download latest](https://github.com/skystart233-code/CodexQuotaWidget/releases/latest) · [Privacy & security](SECURITY.md) · [MIT](LICENSE)

<a id="中文"></a>

## 中文

一个一直在右上角、但不会碍眼的 Codex 额度浮窗。看一眼就知道还剩多少；需要时右键，剩下的都在手边。

### 你会用到的

- 5H / 周额度一键切换，展示真实的剩余百分比
- 极简模式：额度 + 最近重置卡倒计时，低存在感但信息不断线
- 周额度低于 5% 时整窗变红，提醒别在最后一刻才发现
- 自动读取最早到期的重置卡；到期前 24h / 6h / 1h 托盘提醒
- 四套主题、可自定义托盘 Emoji，中文 / English 即时切换
- 可拖动、自动贴合内容；关闭窗口只会收进托盘

### 30 秒上手

1. 从 [Releases](https://github.com/skystart233-code/CodexQuotaWidget/releases/latest) 下载并解压 `CodexQuotaWidget-*-win-x64.zip`。
2. 确保 Windows 已安装 [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)，且 `codex` CLI 已登录并在 `PATH` 中。
3. 运行 `CodexQuotaWidget.App.exe`。右键浮窗或托盘图标，随时换语言、主题、额度窗口或 Emoji。

### 它如何工作

额度来自你本机已登录的 `codex app-server`，不是网页抓取。重置卡信息只使用现有本机登录凭据向 ChatGPT 接口查询，到期时间与个人偏好仅保存在 `%LOCALAPPDATA%\CodexQuotaWidget`。不会上传 token、额度或使用记录；详见 [SECURITY.md](SECURITY.md)。

> `app-server` 与重置卡接口均可能随 Codex 更新变化。查询失败时，额度浮窗仍会正常工作，并保留最近一次成功的卡片倒计时。

### 从源码运行

```powershell
dotnet restore --ignore-failed-sources
dotnet run --project .\src\CodexQuotaWidget.App\CodexQuotaWidget.App.csproj
```

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
