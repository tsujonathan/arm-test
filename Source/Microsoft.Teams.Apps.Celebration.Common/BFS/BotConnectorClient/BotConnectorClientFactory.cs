// <copyright file="BotConnectorClientFactory.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.BFS.BotConnectorClient
{
    using System;
    using System.Collections.Concurrent;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Teams.Apps.Celebration.Common.Core;

    /// <summary>
    /// Bot connector client factory.
    /// </summary>
    public class BotConnectorClientFactory
    {
        private readonly ConfigurationSettings configurationSettings;
        private readonly ConcurrentDictionary<string, CustomConnectorClient> serviceUrlToConnectorClientMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotConnectorClientFactory"/> class.
        /// </summary>
        /// <param name="configurationSettings">Configuration settings.</param>
        public BotConnectorClientFactory(ConfigurationSettings configurationSettings)
        {
            this.configurationSettings = configurationSettings;
            this.serviceUrlToConnectorClientMap =
                new ConcurrentDictionary<string, CustomConnectorClient>();
        }

        /// <summary>
        /// This method creates a bot connector client per service URL.
        /// </summary>
        /// <param name="serviceUrl">Service URL.</param>
        /// <returns>It returns a bot connector client.</returns>
        public CustomConnectorClient Create(string serviceUrl)
        {
            if (!this.serviceUrlToConnectorClientMap.ContainsKey(serviceUrl))
            {
                MicrosoftAppCredentials.TrustServiceUrl(serviceUrl);

                var connectorClient = new CustomConnectorClient(
                    new Uri(serviceUrl),
                    this.configurationSettings.MicrosoftAppId,
                    this.configurationSettings.MicrosoftAppPassword);

                this.serviceUrlToConnectorClientMap.TryAdd(serviceUrl, connectorClient);
            }

            return this.serviceUrlToConnectorClientMap[serviceUrl];
        }
    }
}