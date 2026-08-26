# ISM Demo

ISM Demo 是一套本地演示与调试工具，包含 Web 管理界面和 Windows MQTT 模拟器。

## 项目结构

- `ISMDemo/`：基于 ASP.NET Core 8 的 Web 应用，包含管理界面及 MQTT over WebSocket 服务。
- `ISMSimulator/`：基于 .NET Framework 4.6.2 的 Windows Forms 模拟器，用于连接 MQTT 服务并发送测试消息。

## 开发环境

- Windows 10/11
- Visual Studio 2022（需安装 ASP.NET、桌面开发和 .NET Framework 4.6.2 targeting pack）
- .NET 8 SDK

## 运行 Web 项目

在仓库根目录执行：

```powershell
dotnet restore .\ISMDemo\ISMDemo.csproj
dotnet run --project .\ISMDemo\ISMDemo.csproj --launch-profile ChuteSideISMWebApp
```

默认访问地址：`https://localhost:8101/ism`。

本地启动参数位于 `ISMDemo/Properties/launchSettings.json`。其中的 MQTT 账号和加密密钥只用于本地演示，部署时必须通过安全的配置方式替换。

## 运行模拟器

1. 使用 Visual Studio 打开 `ISMSimulator/ISMSimulator.sln`。
2. 还原 NuGet 包。
3. 编译并运行 `ISMSimulator`。
4. 默认连接地址为 `wss://localhost:8101/ism/mqtt`，本地演示账号与 Web 项目的启动配置保持一致。

模拟器若需要本地 IoT Central 配置，可复制示例文件：

```powershell
Copy-Item .\ISMSimulator\AppSettings.local.config.example .\ISMSimulator\AppSettings.local.config
```

然后只编辑 `AppSettings.local.config`。该文件已被 Git 忽略，不应提交访问令牌或其他凭据。

## 安全说明

- 不要把生产环境密码、令牌、证书或连接字符串提交到仓库。
- 示例配置仅用于本机开发，不能直接用于生产环境。
- 若凭据曾出现在未受保护的位置，请先在对应服务中轮换，再继续使用。

