// <copyright file="SendToConversationFunction.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Functions.Delivery
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.Models;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.RestClient;
    using Microsoft.Teams.Apps.Celebration.Common.Queues;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.Team;
    using Newtonsoft.Json;

    /// <summary>
    /// Azure Functions application triggered by message from a Azure service bus queue.
    /// It sends overdue event occurrence from the bot.
    /// </summary>
    public class SendToConversationFunction
    {
        private const string ConnectionName = "ServiceBusConnection";
        private readonly BFSClient bfsClient;
        private readonly TeamRepository teamRepository;
        private readonly SendToChannelConversationService sendToChannelConversationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendToConversationFunction"/> class.
        /// </summary>
        /// <param name="bfsClient">BFS client.</param>
        /// <param name="teamRepository">Team data repository.</param>
        /// <param name="sendToChannelConversationService">Send to channel conversation service.</param>
        public SendToConversationFunction(
            BFSClient bfsClient,
            TeamRepository teamRepository,
            SendToChannelConversationService sendToChannelConversationService)
        {
            this.bfsClient = bfsClient;
            this.teamRepository = teamRepository;
            this.sendToChannelConversationService = sendToChannelConversationService;
        }

        /// <summary>
        /// It sends a Bot activity to a conversation.
        /// The overdue event occurrences are sent to users via this function.
        /// </summary>
        /// <param name="myQueueItem">The Service Bus queue item.</param>
        /// <param name="logger">Logging service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [FunctionName("SendToConversationFunction")]
        public async Task RunAsync(
            [ServiceBusTrigger(SendToConversationQueue.QueueName, Connection = SendToConversationFunction.ConnectionName)]
            string myQueueItem,
            ILogger logger)
        {
            logger.LogCritical($"DeliveryFunction executed.");
            try
            {
                var activity = JsonConvert.DeserializeObject<Activity>(myQueueItem);
                if (!BotMetadataConstants.ChannelConversationType.Equals(
                    activity.Conversation.ConversationType,
                    StringComparison.OrdinalIgnoreCase))
                {
                    await this.bfsClient.SendToConversationAsync(activity);
                }
                else
                {
                    await this.sendToChannelConversationService.SendToConversationAsync(activity, myQueueItem, logger);
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"{nameof(SendToConversationFunction)}{Environment.NewLine}{ex.ToString()}");
            }
        }
    }
}