// <copyright file="DeliveryPreparationExecutor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Functions.DeliveryPreparation
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.Celebration.Common.Queues;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.Occurrence;

    /// <summary>
    /// It finds the overdue event occurrences in DB
    /// and send the occurrences to target users.
    /// </summary>
    public class DeliveryPreparationExecutor
    {
        private readonly OccurrenceRepository occurrenceRepository;
        private readonly GroupingNotificationsByTeamService groupingNotificationsByTeamService;
        private readonly NotifyEventActivityBuilder notifyEventActivityBuilder;
        private readonly SendToConversationQueue sendToConversationQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeliveryPreparationExecutor"/> class.
        /// </summary>
        /// <param name="occurrenceRepository">The occurrence repository service.</param>
        /// <param name="groupingNotificationsByTeamService">The service that groups occurrences by team.</param>
        /// <param name="notifyEventActivityBuilder">The service that creates bot proactive message for event occurrence.</param>
        /// <param name="sendToConversationQueue">The message queue that keeps the bot proactive messages to be sent to users.</param>
        public DeliveryPreparationExecutor(
            OccurrenceRepository occurrenceRepository,
            GroupingNotificationsByTeamService groupingNotificationsByTeamService,
            NotifyEventActivityBuilder notifyEventActivityBuilder,
            SendToConversationQueue sendToConversationQueue)
        {
            this.occurrenceRepository = occurrenceRepository;
            this.groupingNotificationsByTeamService = groupingNotificationsByTeamService;
            this.notifyEventActivityBuilder = notifyEventActivityBuilder;
            this.sendToConversationQueue = sendToConversationQueue;
        }

        /// <summary>
        /// Finds overdue event occurrences, creates proactive message,
        /// and enqueues the message in the delivery message queue.
        /// </summary>
        /// <param name="log">The logging service.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task ExecuteAsync(ILogger log)
        {
            log.LogCritical($"DeliveryPreparationFunction executed at: {DateTime.Now}.");

            try
            {
                var occurrenceEntities = await this.occurrenceRepository.GetDueOccurencesInInitialStateAsync();

                var teamToNotificationsMap = await this.groupingNotificationsByTeamService.Process(occurrenceEntities);

                var activities = await this.notifyEventActivityBuilder.BuildNotificationActivitiesAsync(teamToNotificationsMap);

                await this.DeliverAsync(activities);

                await this.SetOccurrencesToDeliveringStatusAsync(occurrenceEntities);
            }
            catch (Exception ex)
            {
                log.LogError($"{nameof(DeliveryPreparationFunction)}{Environment.NewLine}{ex.ToString()}");
            }
        }

        private async Task DeliverAsync(IEnumerable<Activity> activities)
        {
            foreach (var activity in activities)
            {
                await this.sendToConversationQueue.SendActivityAsync(activity);
            }
        }

        private async Task SetOccurrencesToDeliveringStatusAsync(IEnumerable<OccurrenceEntity> occurrenceEntities)
        {
            foreach (var occurrenceEntity in occurrenceEntities)
            {
                await this.occurrenceRepository.SetOccurrenceStateAsync(occurrenceEntity.Id, OccurrenceState.Delivering);
            }
        }
    }
}