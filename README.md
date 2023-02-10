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

## 🚧 注意

项目仍在早期开发阶段，功能尚未完善，且处于快速迭代过程中。  
如果您对项目的开发感兴趣，诚挚欢迎您的改进建议或 PR 贡献。

**1.0 版本发布前随时可能发生 API 的非兼容性变更，不建议用于生产环境。**

## ⭐ 特性

### 🌐 原生跨平台

Flandre 为跨平台而生，对聊天平台的结构进行抽象化，采用适配器模式进行兼容，同时提供了良好的开发体验。  
目前已经实现的适配器：

| 平台 | 介绍 |
|:--:|:--:|
| [OneBot](https://github.com/FlandreDevs/Flandre/blob/dev/src/Flandre.Adapters.OneBot/README.md) | [OneBot](https://github.com/botuniverse/onebot) v11 协议封装，主要对 [go-cqhttp](https://github.com/Mrs4s/go-cqhttp) 提供支持。支持 QQ 协议，同时基于 go-cqhttp 对 QQ 频道也进行了一定的支持。 |
| [Konata](https://github.com/FlandreDevs/Flandre/blob/dev/src/Flandre.Adapters.Konata/README.md) | QQ 协议适配，基于 [Konata.Core](https://github.com/KonataDev/Konata.Core) |
| Telegram | 计划中... |
| Discord | 计划中... |

### 🧩 灵活的开发方式
Flandre 提供两种开发方式，分别是完整的开发框架 `Framework`，以及易于嵌入已有程序的 `Core`。
#### Flandre.Framework
[![NuGet](https://img.shields.io/nuget/vpre/Flandre.Framework?style=flat&label=NuGet&color=9866ca)](https://www.nuget.org/packages/Flandre.Framework/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Flandre.Framework?style=flat&label=Downloads&color=42a5f5)](https://www.nuget.org/packages/Flandre.Framework/)

`Flandre.Framework` 是一个使用方便、功能全面的 Bot 开发框架，在核心包 `Core` 的基础上集成了插件、指令、中间件等系统，并提供依赖注入、日志管理等等实用功能。  
Framework 基于 [Microsoft.Extensions.Hosting](https://learn.microsoft.com/zh-cn/dotnet/core/extensions/generic-host)，这意味着可以复用大量社区已有的开源库。对于一个全新的 Bot 项目，我们建议直接使用 Framework 开发。

#### Flandre.Core
[![NuGet](https://img.shields.io/nuget/vpre/Flandre.Core?style=flat&label=NuGet&color=9866ca)](https://www.nuget.org/packages/Flandre.Core/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Flandre.Core?style=flat&label=Downloads&color=42a5f5)](https://www.nuget.org/packages/Flandre.Core/)

`Flandre.Core` 是整个框架的核心组件，包含了适配器、机器人等重要内容，提供直接操作 Bot 进行平台交互的功能。相比 `Framework`，`Core` 作为一个轻量化的模块，能更容易地嵌入进已有项目中，成为功能的一部分。

> 不需要代入 .NET Framework / Core 命名方式的意义。在 Flandre 中，两者只意味着开发方式的不同，且都处于积极维护中。

下文将主要介绍 `Flandre.Framework` 的各类特性。如果你需要关于 `Flandre.Core` 的详细说明，请~~参照这里的文档~~。(还没写x)

### 📦 开箱即用的指令系统

Flandre.Framework 实现了一套开箱即用的指令解析系统，而无需开发者自己造轮子。
开发者可以方便地掌控指令的参数信息，包括但不限于参数数量检查，类型检查，参数默认值等等。而所有的定义可以在一个字符串内完成，例如：

```csharp
[Command("example <foo:string> [bar:double] [baz:int=114514]")]
```

### 🏗 事件驱动

Flandre 内部采用各类事件控制，开发者可以轻松地通过订阅事件/重写相关方法的方式控制应用的运行流程。

## 🚀 起步

遵循不知道哪里来的惯例，我们以一个复读小程序开始。

首先通过 NuGet 包管理器引用 `Flandre.Framework` 和 `Flandre.Adapters.Konata` 包，然后：

```csharp
using Flandre.Framework;
using Flandre.Framework.Common;
using Flandre.Adapters.Konata;
using Flandre.Core.Messaging;
using Konata.Core.Common;
using Microsoft.Extensions.Hosting;

var builder = FlandreApp.CreateBuilder(args);

// 添加 Bot 配置
var config = new KonataAdapterConfig();
config.Bots.Add(new KonataBotConfig
{
    KeyStore = new BotKeyStore("<QQ 号>", "<密码>")
});

// 构造应用实例
var app = builder
    .AddAdapter(new KonataAdapter(config))
    .AddPlugin<ExamplePlugin>()
    .Build();

app.Run();


class ExamplePlugin : Plugin
{
    public override async Task OnMessageReceived(MessageContext ctx)
        => await ctx.Bot.SendMessage(ctx.Message);
}
```

运行程序，向我们 Bot 的 QQ 号发送一条消息，bot 会将消息原封不动地发回来。 ~~复读不仅仅是人类的本质.jpg~~

### 基本指令解析

来个高级点的例子，我们定义一条指令：

```csharp
class AnotherExamplePlugin : Plugin
{
    [Command("example <foo> [bar]")]
    public MessageContent OnExample(MessageContext ctx, ParsedArgs args)
    {
        var foo = args.GetArgument<string>("foo");
        var bar = args.GetArgument<string>("bar");
        
        if (string.IsNullOrWhiteSpace(bar))
            bar = "(empty)";

        var mb = new MessageBuilder();

        mb.Text($"Foo: {foo}, ")
            .Text($"Bar: {bar}");

        return mb;
    }
}
```

这个插件包含一条有两个参数的指令，类型都为 `string`，其中 `foo` 为必选参数，`bar` 为可选参数。如果调用指令时未提供可选参数，参数将被初始化为类型默认值；如果为提供必选参数，bot 将向其发送一条提示信息并停止执行指令。

向 bot 发送 `example qwq ovo`（~~随便什么~~），bot 会将参数的值发送回来。

### 类型约束

如果我们不对指令的参数进行类型约束，那么参数的类型将默认为 `string`。如要添加参数，可以在参数名称后添加 `:` 号和类型名称。类型名称支持 C# 中绝大多数的基本类型，如 `int`, `double`, `long`, `bool` 等等，在解析过程中会自动进行类型检查和转换。

举个例子：

```csharp
[Command("example <foo:double> <bar:bool>")]
public MessageContent? OnExample(MessageContext ctx, ParsedArgs args)
{
    var foo = args.GetArgument<double>("foo");
    var bar = args.GetArgument<bool>("bar");

    Logger.Info(foo.GetType().Name);   // Double
    Logger.Info(bar.GetType().Name);   // Boolean

    return null;
}
```

### 参数默认值

有时我们需要对参数指定默认值，可以在定义中使用 `=` 号：

```csharp
[Command("example [foo:int=1145] [bar:bool=true]")]
```

如果不人为指定默认值，参数将被初始化为 C# 中的类型默认值（即 `default(T)`）。`string` 比较特殊，在参数中它的默认值是空字符串，而不是 `null`。

### 灵活的表现形式

Flandre 内置的指令解析器允许留下空格。如果你觉得参数的各种定义挤在一起乱糟糟的，可以适度空开：

```csharp
[Command("example [foo: int = 1145] [bar: bool = true]")]
```

这样写的缺点是可能导致指令定义过于冗长，可以结合实际情况选择。

## 🛣 路线

- [x] 基本框架搭建
- [x] 消息段实现
- [x] 消息相关工具链
- [x] 甜甜的语法糖
- [x] 事件系统
- [x] 指令系统
- [x] 指令的选项系统
- [x] 编写单元测试
- [x] OneBot 协议适配
- [x] Session 系统

## 💻 分支

项目目前有两个主要分支：

- `main` 分支 - 包含上一个发布版本的源代码，`dev` 分支会在版本发布时合并过来
- `dev` 分支 - 开发分支，包含最新更改，但可能不稳定。

向仓库贡献代码时，请确保目前正处于 `dev` 分支上。

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
