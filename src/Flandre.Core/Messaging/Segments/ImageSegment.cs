namespace Flandre.Core.Messaging.Segments;

/// <summary>
/// 图片消息段
/// </summary>
public class ImageSegment : ResourceSegment
{
    /// <summary>
    /// 图片类型，提供给适配器做相应实现
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// 从数据构造
    /// </summary>
    /// <param name="data">图片数据</param>
    /// <param name="type">类型</param>
    public static ImageSegment FromData(byte[] data, string? type = null)
    {
        return new ImageSegment
        {
            Data = data,
            Type = type
        };
    }

    /// <summary>
    /// 从本地路径构建
    /// </summary>
    /// <param name="path">本地路径</param>
    /// <param name="type">类型</param>
    public static ImageSegment FromPath(string path, string? type = null)
    {
        return new ImageSegment
        {
            Path = path,
            Type = type
        };
    }

    /// <summary>
    /// 从网络 URL 构建
    /// </summary>
    /// <param name="url">网络 URL</param>
    /// <param name="type">类型</param>
    public static ImageSegment FromUrl(string url, string? type = null)
    {
        return new ImageSegment
        {
            Url = url,
            Type = type
        };
    }
}
