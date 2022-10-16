# Flandre.Adapters.Konata

基于 [Konata.Core](https://github.com/KonataDev/Konata.Core) 实现的 QQ 协议适配器

[![NuGet](https://img.shields.io/nuget/vpre/Flandre.Adapters.Konata?label=NuGet&color=blue)](https://www.nuget.org/packages/Flandre.Adapters.Konata/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Flandre.Adapters.Konata?label=Downloads&color=f06292)](https://www.nuget.org/packages/Flandre.Adapters.Konata/)

## 小贴士

- 由于 Konata 机制，在接收到的消息中，图片消息段 (`ImageSegment`) 将固定只包含 `Url` 属性，为图片的链接，需要自行下载。
- ~~想到什么再补~~
