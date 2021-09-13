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
            IMinecraftClient minecraftClient = minecraftUtils.GetService<IMinecraftClient>();
            ITaskExecutor taskExecutor = minecraftUtils.GetService<ITaskExecutor>();
            ISpotTokenProvider innerTokenProvider = new M2MTokenProvider(botConfig);
            ISpotTokenProvider cachedTokenProvider = new CachedTokenProvider(innerTokenProvider);
            ISpotController spotController = new SpotController(cachedTokenProvider, taskExecutor, botConfig);
            ICommandHandler commandHandler = new CommandHandler(spotController, minecraftClient, commandTimeout, botConfig, taskExecutor);
            ICommandResponder commandResponder = new CommandResponder(deleteAfter);

            IServiceProvider serviceProvider = new ServiceCollection()
                .AddSingleton(commandHandler)
                .AddSingleton(commandResponder)
                .BuildServiceProvider();

            return serviceProvider;
        }
    }
}
