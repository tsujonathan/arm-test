// <copyright file="BotServiceCollectionExtensions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Bot
{
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Teams.Apps.Celebration.Bot.ConversationUpdateActivityHandlers;
    using Microsoft.Teams.Apps.Celebration.Bot.ConversationUpdateActivityHandlers.ChannelConversation;
    using Microsoft.Teams.Apps.Celebration.Bot.ConversationUpdateActivityHandlers.PersonalChatConversation;
    using Microsoft.Teams.Apps.Celebration.Bot.MessageActivityHandlers;
    using Microsoft.Teams.Apps.Celebration.Bot.Middlewares;
    using Microsoft.Teams.Apps.Celebration.Bot.TaskModuleServices;

    /// <summary>
    /// Extension class for registering the bot services in DI container.
    /// </summary>
    public static class BotServiceCollectionExtensions
    {
        /// <summary>
        /// Extension method to register bot services in DI container. Use this method to register bot services in DI container.
        /// </summary>
        /// <param name="services">IServiceCollection instance.</param>
        public static void AddCelebrationsBot(this IServiceCollection services)
        {
            // Create the credential provider to be used with the Bot Framework Adapter.
            services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();

            // Create the Celebrations Bot Adapter.
            services.AddSingleton<CelebrationBotAdapter>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddSingleton<IBot, CelebrationBot>();

            // Create the Bot's Teams message filter middle-ware service.
            services.AddSingleton<CelebrationBotFilterMiddleware>();

            services.AddSingleton<WelcomeTeamMembersService>();

            services.AddTransient<IConversationUpdateActivityHandler, BotAddedInChannelHandler>()
                .AddTransient<IConversationUpdateActivityHandler, BotRemovedInChannelHandler>()
                .AddTransient<IConversationUpdateActivityHandler, PersonAddedInChannelHandler>()
                .AddTransient<IConversationUpdateActivityHandler, PersonRemovedInChannelHandler>()
                .AddTransient<IConversationUpdateActivityHandler, BotAddedInPersonalChatHandler>()
                .AddTransient<IConversationUpdateActivityHandler, BotRemovedInPersonalChatHandler>()
                .AddTransient<IConversationUpdateActivityHandler, TeamInfoUpdatedHandler>();

            services.AddTransient<IMessageActivityHandler, IgnoreEventShareHandler>()
                .AddTransient<IMessageActivityHandler, ShareEventHandler>()
                .AddTransient<IMessageActivityHandler, SkipEventHandler>()
                .AddTransient<IMessageActivityHandler, UserMessageHandler>()
                .AddTransient<IMessageActivityHandler, ChangeMessageTargetHandler>()
                .AddTransient<ChangeMessageTargetTaskModuleService>();

            services.AddTransient<ITaskModuleService, ChangeMessageTargetTaskModuleService>();

            services.AddTransient<TurnContextService>();
        }
    }
}
