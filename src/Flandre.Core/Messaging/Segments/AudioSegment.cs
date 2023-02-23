namespace Flandre.Core.Messaging.Segments;

/// <summary>
/// 语音消息段
/// </summary>
public class AudioSegment : ResourceSegment
{
    /// <summary>
    /// 从数据构造
    /// </summary>
    /// <param name="data">图片数据</param>
    public static AudioSegment FromData(byte[] data)
    {
        return new AudioSegment { Data = data };
    }

    /// <summary>
    /// 从本地路径构建
    /// </summary>
    /// <param name="path">本地路径</param>
    public static AudioSegment FromPath(string path)
    {
        return new AudioSegment { Path = path };
    }

    /// <summary>
    /// 从网络 URL 构建
    /// </summary>
    /// <param name="url">网络 URL</param>
    public static AudioSegment FromUrl(string url)
    {
        return new AudioSegment { Url = url };
    }
}
