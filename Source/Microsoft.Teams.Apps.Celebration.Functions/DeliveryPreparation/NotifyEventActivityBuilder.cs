// <copyright file="NotifyEventActivityBuilder.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Functions.DeliveryPreparation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.Celebration.Common.BFS;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.Cards.EventCard;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.Models;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.Occurrence;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.Team;

    /// <summary>
    /// Builds bot activity instance representing event occurrence.
    /// When event occurrence is due, the application uses the class to
    /// creates bot proactive message and send the message to users.
    /// </summary>
    public class NotifyEventActivityBuilder
    {
        private const int NotificationBatchSize = 6;
        private const int MaxNotificationsToSendIndividually = 3;
        private readonly EventCardRenderer eventCardBuilder;
        private readonly BotActivityBuilder botActivityBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyEventActivityBuilder"/> class.
        /// </summary>
        /// <param name="eventCardBuilder">EventCardBuilder service.</param>
        /// <param name="botActivityBuilder">Bot activity builder.</param>
        public NotifyEventActivityBuilder(
            EventCardRenderer eventCardBuilder,
            BotActivityBuilder botActivityBuilder)
        {
            this.eventCardBuilder = eventCardBuilder;
            this.botActivityBuilder = botActivityBuilder;
        }

        /// <summary>
        /// Build bot proactive messages for the due event occurrences.
        /// </summary>
        /// <param name="teamToNotificationsMap">The due event occurrences that are grouped by team.</param>
        /// <returns>The bot activities representing the proactive messages.</returns>
        public async Task<IEnumerable<Activity>> BuildNotificationActivitiesAsync(
            IDictionary<TeamEntity, IEnumerable<Notification>> teamToNotificationsMap)
        {
            var allActivities = new List<Activity>();
            foreach (var teamToNotificationsPair in teamToNotificationsMap)
            {
                var teamEntity = teamToNotificationsPair.Key;
                var notifications = teamToNotificationsPair.Value;
                var notificationBatches = notifications.ToAsyncEnumerable().Buffer(NotifyEventActivityBuilder.NotificationBatchSize);
                await foreach (var notifiationBatch in notificationBatches)
                {
                    if (notifiationBatch.Count <= NotifyEventActivityBuilder.MaxNotificationsToSendIndividually)
                    {
                        var activities = this.CreateActivities(teamEntity, notifiationBatch);
                        allActivities.AddRange(activities);
                    }
                    else
                    {
                        var activity = this.CreateMergedActivity(teamEntity, notifiationBatch);
                        allActivities.Add(activity);
                    }
                }
            }

            return allActivities;
        }

        private string CreateMessage(Notification notification)
        {
            var eventMessageFormat = "<at>{0}</at> is celebrating {1}";
            return string.Format(eventMessageFormat, notification.EventOwnerDisplayName, notification.EventTitle);
        }

        private Entity CreateMention(Notification notification)
        {
            return new Mention
            {
                Text = $"<at>{notification.EventOwnerDisplayName}</at>",
                Mentioned = new ChannelAccount()
                {
                    Name = notification.EventOwnerDisplayName,
                    Id = notification.EventOwnerTeamsId,
                },
            };
        }

        private IEnumerable<Activity> CreateActivities(
            TeamEntity teamEntity,
            IEnumerable<Notification> notifications)
        {
            var activities = new List<Activity>();

            foreach (var notification in notifications)
            {
                var message = this.CreateMessage(notification);
                var attachment = this.eventCardBuilder.BuildAttachment(notification);
                var mention = this.CreateMention(notification);

                var activity = this.botActivityBuilder.CreateActivity(
                    teamEntity.ServiceUrl,
                    string.IsNullOrWhiteSpace(teamEntity.ActiveChannelId) ? teamEntity.TeamId : teamEntity.ActiveChannelId);

                activity.ChannelData = new TeamsChannelData
                {
                    Team = new TeamInfo { Id = teamEntity.TeamId },
                };
                activity.Conversation.ConversationType = BotMetadataConstants.ChannelConversationType;
                activity.Text = message;
                activity.AttachmentLayout = "carousel";
                activity.Attachments.Add(attachment);
                activity.Entities = new List<Entity> { mention };

                activities.Add(activity);
            }

            return activities;
        }

        private string CreateMergedMessage(IEnumerable<Notification> notifications)
        {
            var allButLastEvent = notifications
                .Take(notifications.Count() - 1)
                .Select(notification => this.CreateMessage(notification));

            var messageIncludeAllButLastEvent = string.Join(", ", allButLastEvent);

            var lastEventMessage = this.CreateMessage(notifications.ToArray()[notifications.Count() - 1]);

            var mergedEventMessageFormat = "Stop the presses! Today {0} and {1}. That's a lot of merrymaking for one day—pace yourselves!";
            var message = string.Format(mergedEventMessageFormat, messageIncludeAllButLastEvent, lastEventMessage);

            return message;
        }

        private Activity CreateMergedActivity(
            TeamEntity teamEntity,
            IEnumerable<Notification> notifications)
        {
            var mergedMessage = this.CreateMergedMessage(notifications);
            var attachments = notifications.Select(notification => this.eventCardBuilder.BuildAttachment(notification)).ToList();
            var mentions = notifications.Select(p => this.CreateMention(p)).ToList();

            var activity = this.botActivityBuilder.CreateActivity(
                teamEntity.ServiceUrl,
                string.IsNullOrWhiteSpace(teamEntity.ActiveChannelId) ? teamEntity.TeamId : teamEntity.ActiveChannelId);

            activity.Conversation.ConversationType = BotMetadataConstants.ChannelConversationType;
            activity.Text = mergedMessage;
            activity.Summary = "We're celebrating multiple events today!";
            activity.Attachments = attachments;
            activity.AttachmentLayout = "carousel";
            activity.Entities = mentions;

            return activity;
        }
    }
}