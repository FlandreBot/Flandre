namespace Flandre.Framework.Tests;

public static class Utils
{
    public static FlandreAppBuilder CreateTestBuilder<TPlugin>() where TPlugin : Plugin
    {
        var builder = FlandreApp.CreateBuilder();
        builder.Plugins.Add<TPlugin>();
        return builder;
    }

    public static FlandreApp CreateTestApp<TPlugin>(out MockClient client, bool useFriendClient = false)
        where TPlugin : Plugin
    {
        var builder = CreateTestBuilder<TPlugin>();

        var adapter = new MockAdapter();
        client = useFriendClient ? adapter.GetFriendClient() : adapter.GetChannelClient();
        builder.Adapters.Add(adapter);

        return builder.Build();
    }

    public static FlandreApp CreateTestApp(out MockClient client, bool useFriendClient = false)
    {
        var builder = FlandreApp.CreateBuilder();

        var adapter = new MockAdapter();
        client = useFriendClient ? adapter.GetFriendClient() : adapter.GetChannelClient();
        builder.Adapters.Add(adapter);

        return builder.Build();
    }

    public static FlandreApp StartTestApp<TPlugin>(out MockClient client, bool useFriendClient = false)
        where TPlugin : Plugin
    {
        var app = CreateTestApp<TPlugin>(out client, useFriendClient);
        app.StartWithDefaultsAsync().Wait();
        return app;
    }

    public static FlandreApp StartTestApp(out MockClient client, bool useFriendClient = false)
    {
        var app = CreateTestApp(out client, useFriendClient);
        app.StartWithDefaultsAsync().Wait();
        return app;
    }
}
