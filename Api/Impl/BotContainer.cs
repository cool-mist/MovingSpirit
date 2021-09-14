using Microsoft.Extensions.DependencyInjection;
using MinecraftUtils.Api;
using MinecraftUtils.Api.Impl;
using System;

namespace MovingSpirit.Api.Impl
{
    public static class BotContainer
    {
        public static IServiceProvider CreateServiceProvider()
        {
            IServiceProvider minecraftUtils = new ServiceCollection()
                .AddSingletonMinecraftClient()
                .AddSingletonTaskExecutor()
                .BuildServiceProvider();

            IBotConfig botConfig = new BotConfig();
            TimeSpan commandTimeout = TimeSpan.FromSeconds(int.Parse(botConfig.CommandTimeoutInSeconds));
            TimeSpan deleteAfter = TimeSpan.FromSeconds(int.Parse(botConfig.DeleteAfterInSeconds));
            ulong historyChannelId = ulong.Parse(botConfig.HistoryChannelId);
            ulong serverStatusChannelId = 0;// ulong.Parse(botConfig.ServerStatusChannelId);
            IMinecraftClient minecraftClient = minecraftUtils.GetService<IMinecraftClient>();
            ITaskExecutor taskExecutor = minecraftUtils.GetService<ITaskExecutor>();
            ISpotTokenProvider innerTokenProvider = new M2MTokenProvider(botConfig);
            ISpotTokenProvider cachedTokenProvider = new CachedTokenProvider(innerTokenProvider);
            ISpotController spotController = new SpotController(cachedTokenProvider, taskExecutor, botConfig);
            ICommandHandler commandHandler = new CommandHandler(spotController, minecraftClient, commandTimeout, botConfig, taskExecutor);
            ICommandResponder commandResponder = new CommandResponder(serverStatusChannelId, historyChannelId, deleteAfter);

            IServiceProvider serviceProvider = new ServiceCollection()
                .AddSingleton(botConfig)
                .AddSingleton(commandHandler)
                .AddSingleton(commandResponder)
                .BuildServiceProvider();

            return serviceProvider;
        }
    }
}
