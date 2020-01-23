// <copyright file="EventRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.Repositories.Event
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Teams.Apps.Celebration.Common.Core;

    /// <summary>
    /// Repository of the event data in the table storage.
    /// </summary>
    public class EventRepository : BaseRepository<EventEntity>
    {
        private readonly TableRowKeyGenerator tableRowKeyGenerator;
        private readonly EventFilterCreator eventFilterCreator;
        private readonly UserAadIdFilter userAadIdFilter;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventRepository"/> class.
        /// </summary>
        /// <param name="configurationSettings">Represents the application configuration.</param>
        /// <param name="tableRowKeyGenerator">Table row key generator.</param>
        /// <param name="eventFilterCreator">Upcoming event filter.</param>
        /// <param name="userAadIdFilter">User AAD id.</param>
        public EventRepository(
            ConfigurationSettings configurationSettings,
            TableRowKeyGenerator tableRowKeyGenerator,
            EventFilterCreator eventFilterCreator,
            UserAadIdFilter userAadIdFilter)
            : base(
                configurationSettings,
                PartitionKeyNames.EventDataTable.TableName,
                PartitionKeyNames.EventDataTable.EventPartition,
                false)
        {
            this.tableRowKeyGenerator = tableRowKeyGenerator;
            this.eventFilterCreator = eventFilterCreator;
            this.userAadIdFilter = userAadIdFilter;
        }

        /// <summary>
        /// Gets the events due in 72 hours.
        /// </summary>
        /// <returns>Events due in 72 hours.</returns>
        public async Task<IEnumerable<EventEntity>> GetEventsDueIn72Hours()
        {
            var filter = this.eventFilterCreator.CreateUpcomingEventFilter();
            return await this.GetWithFilterAsync(filter);
        }

        /// <summary>
        /// Gets all events belonging to a user.
        /// </summary>
        /// <param name="userAadId">The user's AadObjectId.</param>
        /// <returns>The events belonging to the user.</returns>
        public async Task<IEnumerable<EventEntity>> GetAllEventsAsync(string userAadId)
        {
            var filter = this.userAadIdFilter.GetUserAadIdFilter(userAadId);
            return await this.GetWithFilterAsync(filter);
        }

        /// <summary>
        /// Create a new event.
        /// </summary>
        /// <param name="eventType">Event type.</param>
        /// <param name="title">Title</param>
        /// <param name="image">Image</param>
        /// <param name="message">Message</param>
        /// <param name="date">Date</param>
        /// <param name="timeZone">Time zone.</param>
        /// <param name="sharedTeams">Shared teams.</param>
        /// <param name="userAadId">User AAD id.</param>
        /// <param name="userName">User name.</param>
        /// <param name="userTeamsId">User teams id.</param>
        /// <returns>The newly created event's id.</returns>
        public async Task<string> CreateEventAsync(
            string eventType,
            string title,
            string image,
            string message,
            DateTime? date,
            string timeZone,
            IEnumerable<string> sharedTeams,
            string userAadId,
            string userName,
            string userTeamsId)
        {
            var newId = this.tableRowKeyGenerator.CreateNewKeyOrderingOldestToMostRecent();

            var entity = new EventEntity
            {
                PartitionKey = PartitionKeyNames.EventDataTable.EventPartition,
                RowKey = newId,
                Id = newId,
                EventType = eventType,
                Title = title,
                Image = image,
                Message = message,
                Date = date,
                MonthDayPair = date?.ToString("MMdd"),
                TimeZone = timeZone,
                SharedTeams = sharedTeams,
                OwnerAadObjectId = userAadId, // "7fae99ec-d260-4a5e-9403-0e3874415213",
                OwnerName = userName,
                OwnerTeamsId = userTeamsId,
            };

            await this.CreateOrUpdateAsync(entity);

            return newId;
        }

        /// <summary>
        /// Update an event.
        /// </summary>
        /// <param name="eventId">The event id.</param>
        /// <param name="eventType">The event type.</param>
        /// <param name="title">The event title.</param>
        /// <param name="image">The event image URL.</param>
        /// <param name="message">The event message.</param>
        /// <param name="date">The event date.</param>
        /// <param name="timeZone">The event timezone.</param>
        /// <param name="sharedTeams">The event shared teams.</param>
        /// <param name="userAadId">The event owner AadObjectId.</param>
        /// <param name="userName">The owner's name.</param>
        /// <param name="userTeamsId">The owner's MS Teams id.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateEventAsync(
            string eventId,
            string eventType,
            string title,
            string image,
            string message,
            DateTime? date,
            string timeZone,
            IEnumerable<string> sharedTeams,
            string userAadId,
            string userName,
            string userTeamsId)
        {
            var entity = new EventEntity
            {
                PartitionKey = PartitionKeyNames.EventDataTable.EventPartition,
                RowKey = eventId,
                Id = eventId,
                EventType = eventType,
                Title = title,
                Image = image,
                Message = message,
                Date = date,
                MonthDayPair = date?.ToString("MMdd"),
                TimeZone = timeZone,
                SharedTeams = sharedTeams,
                OwnerAadObjectId = userAadId,
                OwnerName = userName,
                OwnerTeamsId = userTeamsId,
            };

            await this.CreateOrUpdateAsync(entity);
        }

        /// <summary>
        /// Share an event with a team.
        /// </summary>
        /// <param name="eventId">The event id.</param>
        /// <param name="teamId">The team id.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ShareEventWithTeamAsync(string eventId, string teamId)
        {
            var eventEntity = await this.GetAsync(eventId);
            if (eventEntity == null)
            {
                return;
            }

            var teams = new List<string>(eventEntity.SharedTeams);
            if (!teams.Contains(teamId))
            {
                teams.Add(teamId);
                eventEntity.SharedTeams = teams;
                await this.CreateOrUpdateAsync(eventEntity);
            }
        }

        /// <summary>
        /// Stops sharing all events with a team.
        /// Because the bot has been removed from the team.
        /// </summary>
        /// <param name="teamId">The id of the team to be removed.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task StopSharingAllEventsWithATeam(string teamId)
        {
            var eventEntities = await this.GetAllAsync();

            await this.StopSharingEventsWithATeam(eventEntities, teamId);
        }

        /// <summary>
        /// Stops sharing a person's events with a team.
        /// Because the person has left the team.
        /// </summary>
        /// <param name="userTeamsId">The user's teams id.</param>
        /// <param name="teamId">The team id.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task StopSharingPersonalEventsWithATeam(string userTeamsId, string teamId)
        {
            var filter = TableQuery.GenerateFilterCondition(
                nameof(EventEntity.OwnerTeamsId),
                QueryComparisons.Equal,
                userTeamsId);
            var eventEntities = await this.GetWithFilterAsync(filter);

            await this.StopSharingEventsWithATeam(eventEntities, teamId);
        }

        /// <summary>
        /// Get events belonging to a team by date range.
        /// </summary>
        /// <param name="fromDateTime">The from date.</param>
        /// <param name="toDateTime">The to date.</param>
        /// <param name="teamId">The team id.</param>
        /// <returns>Events meet the criteria.</returns>
        public async Task<IEnumerable<EventEntity>> GetEventsByDateRangeAndTeamIdAsync(
            DateTime fromDateTime,
            DateTime toDateTime,
            string teamId)
        {
            var filter = this.eventFilterCreator.CreateEventDateRangeFilter(fromDateTime, toDateTime);
            var events = await this.GetWithFilterAsync(filter);
            return this.FilterEventsByTeam(events, teamId);
        }

        private IEnumerable<EventEntity> FilterEventsByTeam(IEnumerable<EventEntity> events, string teamId)
        {
            return events.Where(p => p.SharedTeams.Any(sharedTeam => teamId.Equals(sharedTeam, StringComparison.OrdinalIgnoreCase)));
        }

        private async Task StopSharingEventsWithATeam(IEnumerable<EventEntity> eventEntities, string teamId)
        {
            foreach (var eventEntity in eventEntities)
            {
                if (eventEntity.SharedTeams == null || !eventEntity.SharedTeams.Contains(teamId))
                {
                    continue;
                }

                var sharedTeams = eventEntity.SharedTeams.ToList();
                sharedTeams.Remove(teamId);
                eventEntity.SharedTeams = sharedTeams;
                await this.CreateOrUpdateAsync(eventEntity);
            }

            /*
            var toBeUpdated = eventEntities
                .Where(p => p.SharedTeams.Contains(teamId))
                .Select(p =>
                {
                    var sharedTeams = p.SharedTeams.ToList();
                    sharedTeams.Remove(teamId);
                    p.SharedTeams = sharedTeams;
                    return p;
                });
            if (toBeUpdated.Count() == 0)
            {
                return;
            }

            await this.BatchInserOrReplaceAsync(toBeUpdated);
            */
        }
    }
}