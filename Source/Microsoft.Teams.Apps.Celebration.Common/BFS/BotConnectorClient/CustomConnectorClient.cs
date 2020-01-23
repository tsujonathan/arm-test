// <copyright file="CustomConnectorClient.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.BFS.BotConnectorClient
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Schema;

    /// <summary>
    /// Customizes the ConnectorClient class.
    /// It includes helper methods that make it easier to use the ConnectorClient class.
    /// </summary>
    public class CustomConnectorClient : ConnectorClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomConnectorClient"/> class.
        /// </summary>
        /// <param name="serviceUrl">The bot service URL.</param>
        /// <param name="botId">The bot id.</param>
        /// <param name="botPassword">The bot password</param>
        public CustomConnectorClient(Uri serviceUrl, string botId, string botPassword)
            : base(serviceUrl, botId, botPassword)
        {
        }

        /// <summary>
        /// Creates a new bot conversation.
        /// </summary>
        /// <param name="botId">The bot id</param>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="conversationMemberId">The conversation member id.</param>
        /// <returns>The created conversation id.</returns>
        public async Task<string> GetConversationIdAsync(string botId, string tenantId, string conversationMemberId)
        {
            var conversationParameters = new ConversationParameters
            {
                Bot = new ChannelAccount
                {
                    Id = $"28:{botId}",
                },
                TenantId = tenantId,
                Members = new ChannelAccount[]
                {
                    new ChannelAccount(conversationMemberId),
                },
            };

            var conversationWithHttpMessages =
                await this.Conversations.CreateConversationWithHttpMessagesAsync(conversationParameters);
            var conversation = conversationWithHttpMessages.Body;
            return conversation.Id;
        }

        /// <summary>
        /// Gets a bot activity's members.
        /// </summary>
        /// <param name="turnContext">Bot turn context.</param>
        /// <returns>The bot activity's members.</returns>
        public async Task<IEnumerable<ChannelAccount>> GetActivityMembersAsync(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            var activityMembersWithHttpMessages =
                await this.Conversations.GetActivityMembersWithHttpMessagesAsync(
                    turnContext.Activity.Conversation.Id,
                    turnContext.Activity.Id);

            return activityMembersWithHttpMessages.Body;
        }
    }
}