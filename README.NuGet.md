# Flandre

.NET 6 实现的跨平台，低耦合的聊天机器人框架  
一次编写，多处运行

[![License](https://img.shields.io/github/license/FlandreDevs/Flandre?label=License&style=flat-square&color=42a5f5)](https://github.com/FlandreDevs/Flandre/blob/main/LICENSE)
[![Stars](https://img.shields.io/github/stars/FlandreDevs/Flandre?label=Stars&style=flat-square&color=1976d2)](https://github.com/FlandreDevs/Flandre/stargazers)
[![Contributors](https://img.shields.io/github/contributors/FlandreDevs/Flandre?label=Contributors&style=flat-square&color=ab47bc)](https://github.com/FlandreDevs/Flandre/graphs/contributors)
[![NuGet](https://img.shields.io/nuget/vpre/Flandre.Core?style=flat-square&label=NuGet&color=f06292)](https://www.nuget.org/packages/Flandre.Core/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Flandre.Core?style=flat-square&label=Downloads&color=ffb300)](https://www.nuget.org/packages/Flandre.Core/)
[![.NET Version](https://img.shields.io/badge/.NET-6-ffe57f?style=flat-square)](https://www.nuget.org/packages/Flandre.Core/)

本项目的名称来源于东方 Project 中的角色芙兰朵露 · 斯卡雷特 (Flandre Scarlet) ~~(番茄炒蛋)~~

---

**项目仍在早期开发阶段，功能尚未完善，可能带来 API 的非兼容性变更。**  
**如果您对项目的开发感兴趣，诚挚欢迎您的改进建议或 PR 贡献。**

## 特性

### 跨平台

Flandre 自设计之初就是为了跨平台，对聊天平台的结构进行抽象化，采用适配器模式兼容各大聊天平台，同时提供了良好的开发体验。

目前已经实现的适配器：

- [Flandre.Adapters.Konata](https://github.com/FlandreDevs/Flandre/blob/main/Flandre.Adapters.Konata/README.md) - QQ 协议适配，基于 [Konata.Core](https://github.com/KonataDev/Konata.Core)

### 指令系统

得益于内置的指令解析系统，开发者可以方便地掌控指令的参数信息，包括但不限于参数数量检查，类型检查，参数默认值等等。而所有的定义可以在一个字符串内完成，例如：

```csharp
[Command("example <foo:string> [bar:double] [baz:int=114514]")]
```

### 事件驱动

Flandre 内部采用各类事件控制，开发者可以轻松地通过订阅事件/重写相关方法的方式控制应用的运行流程。  
_注：事件系统仍在完善当中_

~~才发布没多久的项目再吹就吹过了~~

## 起步

遵循不知道哪里来的惯例，我们以一个复读小程序开始：

```csharp
using Flandre.Core;
using Flandre.Adapters.Konata;
using Konata.Core.Common;

var app = new FlandreApp();

var config = new KonataAdapterConfig();
config.Bots.Add(new KonataBotConfig
{
    KeyStore = new BotKeyStore("<QQ 号>", "<密码>")
});

class ExamplePlugin : Plugin
{
    public override void OnMessageReceived(Context ctx)
        => ctx.Bot.SendMessage(ctx.Message);
}

app
    .UseKonataAdapter(config)
    .Use(new ExamplePlugin())
    .Start();
```

运行程序，向我们 bot 的 QQ 号发送一条消息，bot 会将消息原封不动地发回来。 ~~复读不仅仅是人类的本质.jpg~~

### 基本指令解析

来个高级点的例子，我们定义一条指令：

```csharp
class ExamplePlugin2 : Plugin
{
    [Command("example <foo> [bar]")]
    public MessageContent OnExample(MessageContext ctx, ParsedArgs args)
    {
        var foo = args.GetArgument<string>("foo");
        var bar = args.GetArgument<string>("bar");
        var baz = args.GetArgumentOrDefault<string>("baz");

        var mb = new MessageBuilder();

        mb.Text($"Foo: {foo}, ")
            .Text($"Bar: {bar}, ")
            .Text("Baz: " + (baz ?? "no arg named baz!"));

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

## 路线

- [x] 基本框架搭建
- [x] 消息段实现
- [x] 消息相关工具链
- [x] 甜甜的语法糖
- [x] 事件系统
- [x] 指令系统
- [ ] 指令的选项系统
- [ ] 编写单元测试
- [ ] OneBot 协议适配

## 分支说明

项目目前有两个主要分支：

- `main` 分支 - 包含上一个发布版本的源代码，`dev` 分支会在版本发布时合并过来
- `dev` 分支 - 开发分支，包含最新更改，但可能不稳定。

向仓库贡献代码时，请确保目前正处于 `dev` 分支上。

## 致谢

项目编写过程中参考了许多开源项目，没有它们就没有 Flandre 的诞生：

- [koishijs/koishi](https://github.com/koishijs/koishi)
- [KonataDev/Konata.Core](https://github.com/KonataDev/Konata.Core)

（按字母排序）

## License

本项目以 [MIT 许可证](https://github.com/FlandreDevs/Flandre/blob/main/LICENSE) 开源 (′▽\`)╭(′▽\`)╯