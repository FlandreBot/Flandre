using Flandre.Core.Messaging;

namespace Flandre.Core.Utils;

/// <summary>
/// Flandre 工具方法
/// </summary>
public static class FlandreUtils
{
    /// <summary>
    /// 自动检测 <see cref="ResourceSegment"/> 的属性并获取或下载资源。
    /// 优先级顺序为：直接返回 Data -> 根据 Path 读取文件 -> 根据 Url 下载文件
    /// </summary>
    /// <param name="resource">资源消息段</param>
    public static byte[]? GetOrDownloadData(this ResourceSegment resource)
    {
        return resource.GetOrDownloadDataAsync().Result;
    }

    /// <summary>
    /// 自动检测 <see cref="ResourceSegment"/> 的属性并异步获取或下载资源。
    /// 优先级顺序为：直接返回 Data -> 根据 Path 读取文件 -> 根据 Url 下载文件
    /// </summary>
    /// <param name="resource">资源消息段</param>
    public static async Task<byte[]?> GetOrDownloadDataAsync(this ResourceSegment resource)
    {
        if (resource.Data is not null) return resource.Data;

        if (resource.Path is not null && File.Exists(resource.Path))
            return await File.ReadAllBytesAsync(resource.Path);

        if (resource.Url is not null)
            return await new HttpClient().GetByteArrayAsync(resource.Url);

        return null;
    }
}