namespace Flandre.Adapters.OneBot.Models.CqCode;

public class CqFace
{
    public string Id { get; init; } = null!;

    public static CqFace Parse(string code)
    {
        return new CqFace { Id = "" };
    }
}