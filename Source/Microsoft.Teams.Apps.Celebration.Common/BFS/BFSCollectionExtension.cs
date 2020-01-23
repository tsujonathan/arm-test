// <copyright file="BFSCollectionExtension.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.BFS
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Identity.Client;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.BotConnectorClient;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.Cards.ChangeMessageTargetCard;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.Cards.EventCard;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.Cards.PreviewCard;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.Cards.WelcomeCards;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.RestClient;
    using Microsoft.Teams.Apps.Celebration.Common.Core;
    using Polly;

    /// <summary>
    /// Extensions class for registering BFS services in the DI container.
    /// </summary>
    public static class BFSCollectionExtension
    {
        /// <summary>
        /// Registers the BFS services in the DI container.
        /// </summary>
        /// <param name="serviceCollection">The IServiceCollection instance.</param>
        /// <param name="configurationSettings">The configuration settings object.</param>
        public static void AddCelebrationsBFS(
            this IServiceCollection serviceCollection,
            ConfigurationSettings configurationSettings)
        {
            serviceCollection
                .AddHttpClient<BFSClient>(client =>
                {
                    client.Timeout = TimeSpan.FromMinutes(5);
                })
                .AddHttpMessageHandler<TokenDelegatingHandler>()
                .AddIOExceptionHandlerPolicy(configurationSettings)
                .AddThrottlingExceptionHandlerPolicy(configurationSettings)
                .AddTransientExceptionHandlerPolicy(configurationSettings);

            serviceCollection.AddSingleton<BotConnectorClientFactory>();

            serviceCollection
                .AddTransient<TokenDelegatingHandler>();

            serviceCollection
                .AddSingleton(serviceProvider =>
                {
                    return ConfidentialClientApplicationBuilder
                        .Create(configurationSettings.MicrosoftAppId)
                        .WithClientSecret(configurationSettings.MicrosoftAppPassword)
                        .WithAuthority(new Uri($"https://login.microsoftonline.com/botframework.com/oauth2/v2.0/token"))
                        .Build();
                });

            serviceCollection.AddSingleton<UrlLinkService>();
            serviceCollection.AddSingleton<MessageTargetChannelNameService>();

            serviceCollection
                .AddTransient<UrlLinkService>()
                .AddTransient<BotActivityBuilder>();

            serviceCollection
                .AddTransient<PreviewCardRenderer>()
                .AddTransient<EventCardRenderer>()
                .AddTransient<ResponseToUserMessageCardRenderer>()
                .AddTransient<WelcomeInstallerCardRenderer>()
                .AddTransient<WelcomeTeamMembersCardRenderer>()
                .AddTransient<ShareEventCardRenderer>()
                .AddTransient<ChangeMessageTargetCardRenderer>();
        }

        /// <summary>
        /// The extension method to handle IO exceptions.
        /// It leverages the Polly lib in handling the exceptions.
        /// </summary>
        /// <param name="httpClientBuilder">The IHttpClientBuilder service.</param>
        /// <param name="configurationSettings">The configuration settings object.</param>
        /// <returns>The IHttpClientBuilder instance.</returns>
        public static IHttpClientBuilder AddIOExceptionHandlerPolicy(
            this IHttpClientBuilder httpClientBuilder,
            ConfigurationSettings configurationSettings)
        {
            var retryCount = configurationSettings.RetryCount;
            var retryDelay = configurationSettings.RetryDelay;

            var asyncRetryPolicy = Policy
                .HandleInner<IOException>()
                .OrResult<HttpResponseMessage>(response => response == null)
                .WaitAndRetryAsync(
                    retryCount + 1,
                    (count) => TimeSpan.FromSeconds(retryDelay),
                    (response, timespan, count, context) =>
                    {
                        if (count > retryCount)
                        {
                            throw new ApplicationException($"IO exception still happens after {retryCount} re-tries!");
                        }
                    });

            return httpClientBuilder.AddPolicyHandler(asyncRetryPolicy);
        }

        /// <summary>
        /// The extension method to handle transient exceptions.
        /// It leverages the Polly lib in handling the exceptions.
        /// </summary>
        /// <param name="httpClientBuilder">The IHttpClientBuilder object.</param>
        /// <param name="configurationSettings">The configuration settings object.</param>
        /// <returns>Returns the same IHttpClientBuilder object.</returns>
        public static IHttpClientBuilder AddTransientExceptionHandlerPolicy(
            this IHttpClientBuilder httpClientBuilder,
            ConfigurationSettings configurationSettings)
        {
            var retryCount = configurationSettings.RetryCount;
            var retryDelay = configurationSettings.RetryDelay;

            return httpClientBuilder.AddTransientHttpErrorPolicy(policyBuilder =>
            {
                return policyBuilder.WaitAndRetryAsync(
                    retryCount + 1,
                    (count) => TimeSpan.FromSeconds(retryDelay),
                    (response, timespan, count, context) =>
                    {
                        if (count > retryCount)
                        {
                            throw new ApplicationException($"Transient exception still happens after {retryCount} re-tries!");
                        }
                    });
            });
        }

        /// <summary>
        /// The extension method to handle the throttling exceptions.
        /// It leverages the Polly lib in handling the exceptions.
        /// </summary>
        /// <param name="httpClientBuilder">The IHttpClientBuilder object.</param>
        /// <param name="configurationSettings">The configuration settings object.</param>
        /// <returns>Returns the same IHttpClientBuilder object.</returns>
        public static IHttpClientBuilder AddThrottlingExceptionHandlerPolicy(
            this IHttpClientBuilder httpClientBuilder,
            ConfigurationSettings configurationSettings)
        {
            var retryCountOnThrottling = configurationSettings.RetryCountOnThrottling;
            var retryDelay = configurationSettings.RetryDelay;

            var asyncRetryPolicy = Policy
                .HandleResult<HttpResponseMessage>(response => (int)response.StatusCode == 429) // HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(
                    retryCountOnThrottling,
                    (count, response, context) =>
                    {
                        return TimeSpan.FromSeconds(retryDelay);
                    },
                    async (response, timeSpan, count, context) =>
                    {
                        await Task.CompletedTask;
                    });

            return httpClientBuilder.AddPolicyHandler(asyncRetryPolicy);
        }
    }
}
