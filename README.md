<div align="center">

<img src="https://raw.githubusercontent.com/FlandreDevs/Flandre/main/avatar.jpg" width="200" />

# Flandre

.NET 6 实现的跨平台，低耦合的聊天机器人框架  
一次编写，多处运行

[![License](https://img.shields.io/github/license/FlandreDevs/Flandre?label=License&style=flat-square&color=42a5f5)](https://github.com/FlandreDevs/Flandre/blob/main/LICENSE)
[![Stars](https://img.shields.io/github/stars/FlandreDevs/Flandre?label=Stars&style=flat-square&color=1976d2)](https://github.com/FlandreDevs/Flandre/stargazers)
[![Contributors](https://img.shields.io/github/contributors/FlandreDevs/Flandre?label=Contributors&style=flat-square&color=ab47bc)](https://github.com/FlandreDevs/Flandre/graphs/contributors)
[![NuGet](https://img.shields.io/nuget/vpre/Flandre.Core?style=flat-square&label=NuGet&color=ec407a)](https://www.nuget.org/packages/Flandre.Core/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Flandre.Core?style=flat-square&label=Downloads&color=ffb300)](https://www.nuget.org/packages/Flandre.Core/)

本项目的名称来源于东方 Project 中的角色芙兰朵露 · 斯卡雷特 (Flandre Scarlet) ~~(番茄炒蛋)~~

</div>

---

**项目仍在早期开发阶段，功能尚未完善，可能带来 API 的非兼容性变更。**  
**如果您对项目的开发感兴趣，诚挚欢迎您的改进建议或 PR 贡献。**

Flandre 对聊天平台的结构进行抽象化，采用适配器模式兼容各大聊天平台，同时提供了良好的开发体验。

目前已经实现的适配器：

- [Flandre.Adapters.Konata](https://github.com/FlandreDevs/Flandre/blob/main/Flandre.Adapters.Konata/README.md) - QQ 协议适配，基于 [Konata.Core](https://github.com/KonataDev/Konata.Core)

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

## 路线图

- [x] 基本框架搭建
- [x] 消息段实现
- [x] 消息相关工具链
- [x] 甜甜的语法糖
- [x] 事件系统
- [ ] 指令系统
- [ ] OneBot 协议适配

## 致谢

项目编写过程中参考了许多开源项目，没有它们就没有 Flandre 的诞生：

- [koishijs/koishi](https://github.com/koishijs/koishi)
- [KonataDev/Konata.Core](https://github.com/KonataDev/Konata.Core)

（按字母排序）

## License

本项目以 [MIT 许可证](https://github.com/FlandreDevs/Flandre/blob/main/LICENSE) 开源 (′▽\`)╭(′▽\`)╯

项目头像来自画师 [yasuharasora](https://www.pixiv.net/users/65707917) 的作品（[PID 91739274](https://www.pixiv.net/artworks/91739274)），若有侵权请联系我删除。  
The project avatar is from [this artwork \(PID 91739274\)](https://www.pixiv.net/artworks/91739274) by [yasuharasora](https://www.pixiv.net/users/65707917). Please contact me for deletion if it violates rights.
