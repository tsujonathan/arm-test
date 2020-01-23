// <copyright file="TurnContextService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Bot
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.Models;
    using Microsoft.Teams.Apps.Celebration.Common.Core;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.Team;
    using Microsoft.Teams.Apps.Celebration.Models;

    public class TurnContextService
    {
        private readonly string botId;
        private readonly CelebrationBotAdapter celebrationBotAdapter;

        /// <summary>
        /// Initializes a new instance of the <see cref="TurnContextService"/> class.
        /// </summary>
        /// <param name="configurationSettings">The configuration settings.</param>
        /// <param name="celebrationBotAdapter">The celebration bot adapter.</param>
        public TurnContextService(
            ConfigurationSettings configurationSettings,
            CelebrationBotAdapter celebrationBotAdapter)
        {
            this.botId = configurationSettings.MicrosoftAppId;
            this.celebrationBotAdapter = celebrationBotAdapter;
        }

        /// <summary>
        /// Gets a bot turn context, and run call back function in the turn context.
        /// </summary>
        /// <param name="teamEntity">The team entity used in here to continue a conversation with a MS Teams team.</param>
        /// <param name="callback">The callback function.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ContinueConversationAsync(
            TeamEntity teamEntity,
            Func<ITurnContext, Task> callback)
        {
            var serviceUrl = teamEntity.ServiceUrl;
            var tenantId = teamEntity.TenantId;
            var teamId = teamEntity.TeamId;

            var conversationReference = this.PrepareConversationReferenceAsync(
                serviceUrl,
                tenantId,
                teamId);

            await this.celebrationBotAdapter.ContinueConversationAsync(
                this.botId,
                conversationReference,
                async (turnContext, cancellationToken) =>
                {
                    turnContext.Activity.ChannelData = new TeamsChannelData
                    {
                        Team = new TeamInfo
                        {
                            Id = teamId,
                        },
                    };

                    await callback.Invoke(turnContext);
                },
                CancellationToken.None);
        }

        private ConversationReference PrepareConversationReferenceAsync(
            string serviceUrl,
            string tenantId,
            string teamId)
        {
            var user = new ChannelAccount
            {
                Id = teamId,
            };

            var bot = new ChannelAccount
            {
                Id = $"28:{this.botId}",
            };

            var conversationAccount = new ConversationAccount
            {
                ConversationType = BotMetadataConstants.ChannelConversationType,
                Id = teamId,
                TenantId = tenantId,
            };

            var conversationReference = new ConversationReference
            {
                Bot = bot,
                User = user,
                ChannelId = BotMetadataConstants.MsTeamsChannelId,
                Conversation = conversationAccount,
                ServiceUrl = serviceUrl,
            };

            if (!MicrosoftAppCredentials.IsTrustedServiceUrl(conversationReference.ServiceUrl))
            {
                MicrosoftAppCredentials.TrustServiceUrl(conversationReference.ServiceUrl);
            }

            return conversationReference;
        }
    }
}
