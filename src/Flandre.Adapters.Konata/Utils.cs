using Flandre.Core.Messaging;
using Flandre.Core.Messaging.Segments;
using Flandre.Core.Models;
using Konata.Core.Message;
using Konata.Core.Message.Model;
using FlandreMessageBuilder = Flandre.Core.Messaging.MessageBuilder;
using KonataMessageBuilder = Konata.Core.Message.MessageBuilder;

namespace Flandre.Adapters.Konata;

internal static class CommonUtils
{
    public static string GetAvatarUrl(uint userId)
    {
        return $"http://q.qlogo.cn/headimg_dl?dst_uin={userId}&spec=640";
    }
}

internal static class MessageUtils
{
    internal static Message ToFlandreMessage(this MessageStruct message, string platform)
    {
        var mb = new FlandreMessageBuilder();

        foreach (var chain in message.Chain)
            switch (chain)
            {
                case TextChain textChain:
                    mb.Text(textChain.Content);
                    break;

                case ImageChain imageChain:
                    mb.Image(ImageSegment.FromUrl(imageChain.ImageUrl));
                    break;
            }

        var groupId = message.Type == MessageStruct.SourceType.Group
            ? message.Receiver.Uin.ToString()
            : null;

        return new Message
        {
            Time = DateTimeOffset.FromUnixTimeSeconds(message.Time).LocalDateTime,
            Platform = platform,
            SourceType = message.Type == MessageStruct.SourceType.Group
                ? MessageSourceType.Channel
                : MessageSourceType.Private,
            MessageId = message.Uuid.ToString(),
            GuildId = groupId,
            ChannelId = groupId,
            Sender = new User
            {
                Name = message.Sender.Name,
                // Nickname = message.Sender.Name,   (can't get user's nickname)
                UserId = message.Sender.Uin.ToString(),
                AvatarUrl = CommonUtils.GetAvatarUrl(message.Sender.Uin)
            },
            Content = mb.Build()
        };
    }

    internal static MessageChain ToKonataMessageChain(this MessageContent content)
    {
        var mb = new KonataMessageBuilder();

        var prefixChecked = false;
        foreach (var segment in content)
            switch (segment)
            {
                case QuoteSegment quoteSegment:
                    if (!prefixChecked)
                    {
                        var messageStruct = new MessageStruct(
                            uint.Parse(quoteSegment.QuotedMessage.Sender.UserId),
                            quoteSegment.QuotedMessage.Sender.Nickname,
                            quoteSegment.QuotedMessage.Content.ToKonataMessageChain(),
                            quoteSegment.QuotedMessage.SourceType ==
                            MessageSourceType.Channel
                                ? MessageStruct.SourceType.Group
                                : MessageStruct.SourceType.Friend);
                        mb.Add(ReplyChain.Create(messageStruct));
                        prefixChecked = true;
                    }

                    break;

                case TextSegment textSegment:
                    mb.Text(textSegment.Text);
                    break;

                case ImageSegment imageSegment:
                    if (imageSegment.Path is not null)
                        mb.Image(imageSegment.Path);
                    else if (imageSegment.Data is not null)
                        mb.Image(imageSegment.Data);
                    else if (imageSegment.Url is not null)
                        mb.Add(ImageChain.CreateFromUrl(imageSegment.Url));
                    break;
            }

        return mb.Build();
    }
}
