// <copyright file="GroupingNotificationsByTeamService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Functions.DeliveryPreparation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.Event;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.Occurrence;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.Team;

    /// <summary>
    /// Groups event occurrences by team.
    /// Event occurrence knows target teams. But the event occurrences are sent per team.
    /// The class analyses the event occurrences, and groups them in teams.
    /// It helps to ease the sending of the event occurrences.
    /// </summary>
    public class GroupingNotificationsByTeamService
    {
        private readonly EventRepository eventRepository;
        private readonly TeamRepository teamRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupingNotificationsByTeamService"/> class.
        /// </summary>
        /// <param name="eventRepository">The event repository service.</param>
        /// <param name="teamRepository">The team repository service.</param>
        public GroupingNotificationsByTeamService(
            EventRepository eventRepository,
            TeamRepository teamRepository)
        {
            this.eventRepository = eventRepository;
            this.teamRepository = teamRepository;
        }

        /// <summary>
        /// Groups event occurrences by team.
        /// </summary>
        /// <param name="occurrenceEntities">The event occurrences to be grouped.</param>
        /// <returns>The grouped event occurrences by team.</returns>
        public async Task<IDictionary<TeamEntity, IEnumerable<Notification>>> Process(
            IEnumerable<OccurrenceEntity> occurrenceEntities)
        {
            var allNotifications = await this.GetAllNotifications(occurrenceEntities);

            var teamToNotificationsMap = await this.GroupNotificationsByTeamAsync(allNotifications);

            return teamToNotificationsMap;
        }

        private async Task<IEnumerable<Notification>> GetAllNotifications(IEnumerable<OccurrenceEntity> occurrenceEntities)
        {
            var notifications = new List<Notification>();
            foreach (var occurrenceEntity in occurrenceEntities)
            {
                var eventEntity = await this.eventRepository.GetAsync(occurrenceEntity.EventId);
                if (eventEntity == null)
                {
                    continue;
                }

                var teamIds = eventEntity.SharedTeams;
                if (teamIds == null || teamIds.Count() == 0)
                {
                    continue;
                }

                foreach (var teamId in teamIds)
                {
                    notifications.Add(new Notification
                    {
                        TeamId = teamId,
                        OccurrenceId = occurrenceEntity.Id,
                        EventMessage = eventEntity.Message,
                        EventTitle = eventEntity.Title,
                        EventImage = eventEntity.Image,
                        EventOwnerDisplayName = eventEntity.OwnerName,
                        EventOwnerTeamsId = eventEntity.OwnerTeamsId,
                    });
                }
            }

            return notifications;
        }

        private async Task<IDictionary<TeamEntity, IEnumerable<Notification>>> GroupNotificationsByTeamAsync(
            IEnumerable<Notification> allNotifications)
        {
            var teamToNotificationsMap = new Dictionary<TeamEntity, IEnumerable<Notification>>();
            var groups = allNotifications.GroupBy(notification => notification.TeamId, notification => notification);
            foreach (var group in groups)
            {
                var teamId = group.Key;
                var teamEntity = await this.teamRepository.GetAsync(teamId);
                if (teamEntity == null)
                {
                    continue;
                }

                teamToNotificationsMap.Add(teamEntity, group);
            }

            return teamToNotificationsMap;
        }
    }
}
