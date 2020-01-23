// <copyright file="Startup.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

[assembly: Microsoft.Azure.Functions.Extensions.DependencyInjection.FunctionsStartup(
    typeof(Microsoft.Teams.Apps.Celebration.Functions.Startup))]

namespace Microsoft.Teams.Apps.Celebration.Functions
{
    using Microsoft.Azure.Functions.Extensions.DependencyInjection;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Teams.Apps.Celebration.Common.BFS;
    using Microsoft.Teams.Apps.Celebration.Common.Core;
    using Microsoft.Teams.Apps.Celebration.Common.Queues;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories;
    using Microsoft.Teams.Apps.Celebration.Functions.Delivery;
    using Microsoft.Teams.Apps.Celebration.Functions.DeliveryPreparation;
    using Microsoft.Teams.Apps.Celebration.Functions.OccurrenceInitialization;

    /// <summary>
    /// Register services in DI container of the Azure functions system.
    /// </summary>
    public class Startup : FunctionsStartup
    {
        /// <inheritdoc/>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Initializes configuration settings.
            var serviceProvider = builder.Services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();
            var configurationSettings = new ConfigurationSettings();
            configuration.Bind(configurationSettings);

            builder.Services
                .AddSingleton(configurationSettings)
                .AddSingleton<GroupingNotificationsByTeamService>()
                .AddSingleton<SendToChannelConversationService>()
                .AddSingleton<NotifyEventActivityBuilder>()
                .AddSingleton<OccurrenceInitializationExecutor>()
                .AddSingleton<DeliveryPreparationExecutor>();

            builder.Services.AddCelebrationsMessageQueue();

            builder.Services.AddCelebrationsBFS(configurationSettings);

            builder.Services.AddCelebrationsRepositories();
        }
    }
}