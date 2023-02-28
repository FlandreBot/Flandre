<div align="center">

<img src="https://ghproxy.com/https://raw.githubusercontent.com/FlandreDevs/Flandre/dev/assets/avatar.jpg" width="200" />

# Flandre

.NET 6 实现的跨平台，现代化聊天机器人框架  
一套代码，多平台服务

[![License](https://img.shields.io/github/license/FlandreDevs/Flandre?label=License&style=flat&color=42a5f5)](https://github.com/FlandreDevs/Flandre/blob/main/LICENSE)
[![Stars](https://img.shields.io/github/stars/FlandreDevs/Flandre?label=Stars&style=flat&color=1976d2)](https://github.com/FlandreDevs/Flandre/stargazers)
[![Contributors](https://img.shields.io/github/contributors/FlandreDevs/Flandre?label=Contributors&style=flat&color=9866ca)](https://github.com/FlandreDevs/Flandre/graphs/contributors)
[![Flandre.Framework Version](https://img.shields.io/nuget/vpre/Flandre.Framework?style=flat&label=Framework&color=f06292)](https://www.nuget.org/packages/Flandre.Framework/)
[![Flandre.Core Version](https://img.shields.io/nuget/vpre/Flandre.Core?style=flat&label=Core&color=e65943)](https://www.nuget.org/packages/Flandre.Core/)
[![.NET Version](https://img.shields.io/badge/.NET-6-ffe57f?style=flat)](https://www.nuget.org/packages/Flandre.Core/)
[![Codecov](https://img.shields.io/codecov/c/gh/FlandreDevs/Flandre/dev?style=flat&color=a5d6a7&label=Coverage)](https://app.codecov.io/gh/FlandreDevs/Flandre)

\- **[使用文档](https://flandredevs.github.io/)** -

本项目的名称来源于东方 Project 中的角色芙兰朵露 · 斯卡雷特 (Flandre Scarlet) ~~(番茄炒蛋)~~

</div>

---

## ⭐ 特性

### 🌐 原生跨平台

Flandre 为跨平台而生，对聊天平台的结构进行抽象化，采用适配器模式进行兼容，使得开发者可以通过一套统一接口控制不同平台的机器人，同时提供了良好的开发体验。  
目前已经实现的适配器：

| 平台 | 介绍 |
|:--:|:--:|
| [OneBot](https://github.com/FlandreDevs/Flandre/blob/dev/src/Flandre.Adapters.OneBot/README.md) | [OneBot](https://github.com/botuniverse/onebot) v11 协议封装，主要对 [go-cqhttp](https://github.com/Mrs4s/go-cqhttp) 提供支持。支持 QQ 协议，同时基于 go-cqhttp 对 QQ 频道也进行了一定的支持。 |
| [Konata](https://github.com/FlandreDevs/Flandre/blob/dev/src/Flandre.Adapters.Konata/README.md) | QQ 协议适配，基于 [Konata.Core](https://github.com/KonataDev/Konata.Core) |
| Telegram | 计划中... |
| Discord | 计划中... |

### 🧩 灵活的开发方式
Flandre 提供两种开发方式，分别是完整的开发框架 `Framework`，以及易于嵌入已有程序的 `Core`。

<details>
<summary>详细区别</summary>

#### Flandre.Framework
[![NuGet](https://img.shields.io/nuget/vpre/Flandre.Framework?style=flat&label=NuGet&color=9866ca)](https://www.nuget.org/packages/Flandre.Framework/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Flandre.Framework?style=flat&label=Downloads&color=42a5f5)](https://www.nuget.org/packages/Flandre.Framework/)

`Flandre.Framework` 是一个使用方便、功能全面的 Bot 开发框架，在核心包 `Core` 的基础上集成了插件、指令、中间件等系统，并提供依赖注入、日志管理等等实用功能。  
Framework 基于 [Microsoft.Extensions.Hosting](https://learn.microsoft.com/zh-cn/dotnet/core/extensions/generic-host)，这意味着可以复用大量社区已有的开源库。对于一个全新的 Bot 项目，我们建议直接使用 Framework 开发。

#### Flandre.Core
[![NuGet](https://img.shields.io/nuget/vpre/Flandre.Core?style=flat&label=NuGet&color=9866ca)](https://www.nuget.org/packages/Flandre.Core/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Flandre.Core?style=flat&label=Downloads&color=42a5f5)](https://www.nuget.org/packages/Flandre.Core/)

`Flandre.Core` 是整个框架的核心组件，包含了适配器、机器人等抽象层，提供直接操作 Bot 进行平台交互的功能。相比 `Framework`，`Core` 作为一个轻量化的模块，能更容易地嵌入进已有项目中，成为功能的一部分。

> 不需要代入 .NET Framework / Core 命名方式的意义。在 Flandre 中，两者只意味着开发方式的不同，且都处于积极维护中。

</details>

下文将主要介绍 `Flandre.Framework` 的各类特性。如果你需要关于 `Flandre.Core` 的详细说明，请~~参照这里的文档~~。(还没写x)

### 📦 开箱即用的指令系统

Flandre.Framework 实现了一套开箱即用的指令解析系统，而无需开发者自己造轮子。开发者可以方便地定义一条指令：

```csharp
[Command("example")]
public MessageContent OnExample(CommandContext ctx,
    double arg1, string arg2, [Option] bool opt1)
{
    // 指令逻辑...
}
```

## 🚀 起步

我们提供了一个模板项目，可以帮助你快速上手。（[仓库在这里](https://github.com/FlandreDevs/Templates)）

首先安装模板包：

```shell
$ dotnet new install Flandre.Templates

# 如果曾经安装过，可以使用以下命令更新至最新版本
$ dotnet new update
```

创建一个名为 `MyFirstFlandreApp` 的新项目：

```shell
$ dotnet new flandre -o MyFirstFlandreApp
```

使用你喜欢的 IDE 打开项目，在 `Program.cs` 中添加想要的适配器，开始对 Flandre 的探索。

## 💻 开发

你可以查看本仓库的[里程碑](https://github.com/FlandreDevs/Flandre/milestones)或[项目](https://github.com/FlandreDevs/Flandre/projects)页，获取最新的开发进度。

如果你在开发的过程中发现了 Bug，或有建议，欢迎[提交 Issue](https://github.com/FlandreDevs/Flandre/issues/new/choose)。

如果你想要贡献代码，欢迎[与我们联系](#💬-交流)，并[发起 PR](https://github.com/FlandreDevs/Flandre/compare)。

项目目前有两个主要分支：

- `main` 分支 - 包含上一个发布版本的源代码，`dev` 分支会在版本发布时合并过来
- `dev` 分支 - 开发分支，包含最新更改，但可能不稳定。

向仓库贡献代码时，请确保目标是 `dev` 分支。

## ❤️ 致谢

项目编写过程中参考了许多开源项目，没有它们就没有 Flandre 的诞生：

- [koishijs/koishi](https://github.com/koishijs/koishi)
- [KonataDev/Konata.Core](https://github.com/KonataDev/Konata.Core)

（按字母排序）

另外，感谢 [JetBrains](https://www.jetbrains.com/) 对本项目的支持，以及免费授权的产品开源使用许可！

<img height="150" width="150" src="https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.png" alt="JetBrains Logo (Main) logo.">

## 💬 交流

GitHub 是我们的主要活动场地。您也可以加入我们的 QQ 群进行项目相关的交流：

[![QQ](https://img.shields.io/badge/Flandre.Community-164189664-blue?style=flat&logo=tencent-qq&logoColor=white)](https://jq.qq.com/?_wv=1027&k=tTNVlDR6)

本群只交流程序开发，拒绝任何形式的伸手党或商业行为。

## 📄 开源

本项目以 [MIT 许可证](https://github.com/FlandreDevs/Flandre/blob/main/LICENSE) 开源 (′▽\`)╭(′▽\`)╯

项目头像来自画师 [yasuharasora](https://www.pixiv.net/users/65707917) 的作品（[PID 91739274](https://www.pixiv.net/artworks/91739274)），若有侵权请联系我删除。  
The project avatar is from [this artwork \(PID 91739274\)](https://www.pixiv.net/artworks/91739274) by [yasuharasora](https://www.pixiv.net/users/65707917). Please contact me for deletion if it violates rights.
