# CodexQuotaWidget

一个 Windows `.NET 8 + WPF` 额度浮窗。它通过 Codex CLI 官方本地进程协议读取当前账户的额度窗口，可在 **5H 剩余**和**周额度剩余**之间切换，并提供极简模式、主题和重置卡提醒。

## 先决条件

- Windows 10/11
- 已安装 .NET 8 Desktop Runtime；从源码构建需要可编译 `net8.0-windows` 的 .NET SDK
- `codex` CLI 已安装、位于 `PATH`，并已完成登录

本仓库的 `global.json` 固定使用 SDK `9.0.313`，因为当前开发机的 SDK 10 Roslyn 需要更新的 Windows CET 支持。目标程序仍运行在 .NET 8。

## 运行

本机已生成框架依赖版可执行文件：`dist\win-x64\CodexQuotaWidget.App.exe`。它要求已安装 .NET 8 Desktop Runtime，并且 `codex` CLI 已在 `PATH` 中。

从源码运行：

```powershell
dotnet restore --ignore-failed-sources
dotnet run --project .\src\CodexQuotaWidget.App\CodexQuotaWidget.App.csproj
```

浮窗始终置顶，可拖动；首次默认使用极简模式，显示额度百分比与最近一张重置卡的到期倒计时。右键浮窗或使用托盘菜单可以：

- 在极简/完整模式间切换；完整模式使用内容驱动的自动尺寸
- 切换 5H 或周额度
- 切换深夜蓝、石墨黑、纸张白、极光绿四套主题
- 自定义系统托盘 Emoji；支持直接输入单个 Emoji 或选择快捷图标
- 一键开启/关闭重置卡到期通知
- 手动刷新、重新显示浮窗或退出

双击浮窗也可切换极简/完整模式。按 `Alt+F4` 会隐藏到托盘。托盘图标默认使用 `⚡`，自定义后立即更新。设置保存在 `%LOCALAPPDATA%\CodexQuotaWidget\settings.json`。

## 数据与映射

唯一额度数据源是子进程：

1. 启动 `codex app-server --stdio`
2. JSON-RPC `initialize`，随后发送 `initialized`
3. 调用 `account/rateLimits/read`
4. 监听 `account/rateLimits/updated`；通知可能是稀疏的，因此收到通知后重新读取完整快照
5. 每 60 秒主动对账

窗口身份只按 `windowDurationMins` 判断：约 300 分钟为 5H，约 10080 分钟为周额度。`primary` **不会**被固定当作 5H。剩余百分比为 `100 - usedPercent`；缺少、为 `null` 或无法识别的窗口会显示“此额度窗口不可用”。例如 `primary.duration=10080, usedPercent=37, secondary=null` 会得到周剩余 63%，5H 不可用。

## 重置卡提醒

`account/rateLimits/read` 返回重置卡可用数量；程序另外使用本机 Codex 登录凭证请求 `https://chatgpt.com/backend-api/wham/rate-limit-reset-credits`，读取每张卡的发放与过期时间。界面始终选取“仍可用且最早过期”的一张显示倒计时，无需手填日期；到期前 24 小时、6 小时和 1 小时通过托盘通知，每个提醒等级只发送一次。自动查询每 10 分钟至多一次，点击“立即刷新”会立即重查。

周额度剩余严格低于 5% 时，无论当前选择显示 5H 还是周额度，整个悬浮窗都会切换为红色预警。

## 架构

- `src/CodexQuotaWidget.Core`：额度模型、duration 分类、倒计时格式和预警策略
- `src/CodexQuotaWidget.Codex`：app-server 进程、JSON-RPC 行解析/请求关联、额度对账及重置卡查询
- `src/CodexQuotaWidget.App`：WPF 浮窗、托盘、设置与应用生命周期
- `tests/CodexQuotaWidget.Tests`：无第三方测试包的离线自测试运行器

## 验证

```powershell
dotnet test .\CodexQuotaWidget.slnx
dotnet run --project .\tests\CodexQuotaWidget.Tests\CodexQuotaWidget.Tests.csproj
dotnet run --project .\tests\CodexQuotaWidget.Tests\CodexQuotaWidget.Tests.csproj -- --live
dotnet build .\CodexQuotaWidget.slnx --no-restore
```

测试覆盖 300/10080/null/unknown 窗口、小数百分比、重置卡数量与时间解析、倒计时、提醒分级、5% 预警边界、窗口形状、JSON-RPC 行解析和请求 ID 关联。

## 协议与隐私

`codex app-server` 的这组账户额度方法目前属于 experimental API，未来 Codex CLI 升级可能改变方法或 payload；界面会把连接/协议错误显示为同步失败。

程序不实现 OAuth，也不会显示、记录或上传 token。查询重置卡时只在内存中读取现有 Codex `auth.json` 的 `access_token` 与 `account_id`，直接请求 ChatGPT 的重置卡接口；本机设置文件仅缓存最近到期时间、通知开关、所选窗口、主题和浮窗坐标。该私有接口未来可能变化，查询失败时额度显示仍可正常工作，并保留上次成功查询的倒计时。

## 许可证

本项目使用 [MIT License](LICENSE)。
