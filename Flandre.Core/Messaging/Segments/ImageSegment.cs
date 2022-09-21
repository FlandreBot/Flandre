namespace Flandre.Core.Messaging.Segments;

public class ImageSegment : ResourceSegment
{
    public string? Type { get; set; }

    public static ImageSegment FromData(byte[] data, string? type = null)
    {
        return new() { Data = data, Type = type };
    }

    public static ImageSegment FromPath(string path, string? type = null)
    {
        return new() { Path = path, Type = type };
    }

    public static ImageSegment FromUrl(string url, string? type = null)
    {
        return new() { Url = url, Type = type };
    }
}