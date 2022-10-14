using Flandre.Adapters.OneBot.Segments;
using Flandre.Core.Messaging;
using Flandre.Core.Messaging.Segments;
using Flandre.Core.Utils;

namespace Flandre.Adapters.OneBot;

public static class CqCodeParser
{
    /// <summary>
    /// 将含有 CQ 码的消息解析为 <see cref="MessageContent"/>。
    /// </summary>
    /// <param name="message">含有 CQ 码的消息</param>
    public static MessageContent ParseCqMessage(string message)
    {
        if (string.IsNullOrEmpty(message)) return "";

        var parser = new StringParser(message);
        var segments = new List<MessageSegment>();

        while (!parser.IsEnd())
            if (parser.Current == '[')
            {
                // CQCode
                var code = parser.Read(']', true);
                var sections = code[4..^1].Split(',');
                segments.Add(sections[0] switch
                {
                    "face" => ParseFace(sections[1..]),
                    "record" => ParseRecord(sections[1..]),
                    "image" => ParseImage(sections[1..]),
                    _ => new TextSegment(code)
                });
            }
            else
            {
                var text = parser.Read('[');
                segments.Add(new TextSegment(text));
            }

        return new MessageContent(segments);
    }

    public static string ToCqMessage(this MessageContent content)
    {
        // TODO
        return content.GetText();
    }

    private static FaceSegment ParseFace(string[] data)
    {
        return new FaceSegment(data[0][3..]); // id=xxx
    }

    private static OneBotRecordSegment ParseRecord(string[] data)
    {
        var segment = new OneBotRecordSegment();
        foreach (var d in data)
        {
            var kv = d.Split('=');
            switch (kv[0])
            {
                case "file":
                    segment.Filename = kv[1];
                    break;
                case "url":
                    segment.Url = string.Join('=', kv[1..]);
                    break;
                case "magic":
                    segment.Magic = int.Parse(kv[1]);
                    break;
            }
        }

        return segment;
    }

    private static OneBotImageSegment ParseImage(string[] data)
    {
        var segment = new OneBotImageSegment();
        foreach (var d in data)
        {
            var kv = d.Split('=');
            switch (kv[0])
            {
                case "file":
                    segment.Filename = kv[1];
                    break;
                case "type":
                    segment.Type = kv[1];
                    break;
                case "subType":
                    segment.SubType = kv[1];
                    break;
                case "url":
                    segment.Url = string.Join('=', kv[1..]);
                    break;
                case "id":
                    segment.Id = int.Parse(kv[1]);
                    break;
            }
        }

        return segment;
    }
}