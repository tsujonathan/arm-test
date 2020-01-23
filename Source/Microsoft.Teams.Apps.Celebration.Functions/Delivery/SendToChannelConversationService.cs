// <copyright file="SendToChannelConversationService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Functions.Delivery
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.RestClient;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.Team;
    using Newtonsoft.Json;

    /// <summary>
    /// Sends proactive message to MS Teams channel.
    /// </summary>
    public class SendToChannelConversationService
    {
        private readonly BFSClient bfsClient;
        private readonly TeamRepository teamRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendToChannelConversationService"/> class.
        /// </summary>
        /// <param name="bfsClient">The BFS client service.</param>
        /// <param name="teamRepository">The team repository service.</param>
        public SendToChannelConversationService(
            BFSClient bfsClient,
            TeamRepository teamRepository)
        {
            this.bfsClient = bfsClient;
            this.teamRepository = teamRepository;
        }

        /// <summary>
        /// Sends proactive message to MS Teams channel.
        ///
        /// If a bot activity contains Mentions, the function creates
        /// a bot conversation first, then sends the message to the conversation.
        ///
        /// If BFS returns 404 (channel not found), the function retry to send
        /// it to the team's General channel and log the 404 error.
        ///
        /// </summary>
        /// <param name="activity">The bot activity instance.</param>
        /// <param name="originalActivityAsString">The original bot activity in string.</param>
        /// <param name="logger">The logging service.</param>
        /// <returns>The delivery status.</returns>
        public async Task<DeliveryStatus> SendToConversationAsync(Activity activity, string originalActivityAsString, ILogger logger)
        {
            var deliveryStatus = await this.SendToConversationAsync(activity);
            if (deliveryStatus != DeliveryStatus.NotFound)
            {
                return deliveryStatus;
            }

            activity = JsonConvert.DeserializeObject<Activity>(originalActivityAsString);
            return await this.RetrySendOn404ErrorAsync(activity, logger);
        }

        private async Task<DeliveryStatus> SendToConversationAsync(Activity activity)
        {
            if (activity.Entities == null || activity.Entities.Count == 0)
            {
                return await this.bfsClient.SendToConversationAsync(activity);
            }

            var activityWithoutMentions = new Activity
            {
                ServiceUrl = activity.ServiceUrl,
                Type = activity.Type,
                Text = activity.Text,
                Entities = activity.Entities,
            };
            var deliveryStatus = await this.bfsClient.CreateConversationAsync(
                activityWithoutMentions,
                activity.Conversation.Id,
                newConversationId => activity.Conversation.Id = newConversationId);
            if (deliveryStatus != DeliveryStatus.Succeeded)
            {
                return deliveryStatus;
            }

            activity.Entities = null;
            activity.Text = null;
            return await this.bfsClient.SendToConversationAsync(activity);
        }

        private async Task<DeliveryStatus> RetrySendOn404ErrorAsync(Activity activity, ILogger logger)
        {
            var channelData = activity.GetChannelData<TeamsChannelData>();
            if (channelData == null || channelData.Team == null || string.IsNullOrWhiteSpace(channelData.Team.Id))
            {
                return DeliveryStatus.Failed;
            }

            var teamEntity = await this.teamRepository.GetAsync(channelData.Team.Id);
            if (teamEntity != null)
            {
                teamEntity.ActiveChannelId = null;
                await this.teamRepository.CreateOrUpdateAsync(teamEntity);
                logger.LogCritical("Because the message target channel is not found, it resets to target messages to the general channel.");
            }

            activity.Conversation.Id = channelData.Team.Id;
            return await this.SendToConversationAsync(activity);
        }
    }
}