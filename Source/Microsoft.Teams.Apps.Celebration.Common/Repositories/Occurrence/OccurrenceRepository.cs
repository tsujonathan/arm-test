// <copyright file="OccurrenceRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.Repositories.Occurrence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Teams.Apps.Celebration.Common.Core;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.Event;

    /// <summary>
    /// Repository of the event data in the table storage.
    /// </summary>
    public class OccurrenceRepository : BaseRepository<OccurrenceEntity>
    {
        private readonly TableRowKeyGenerator tableRowKeyGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="OccurrenceRepository"/> class.
        /// </summary>
        /// <param name="configurationSettings">Represents the application configuration.</param>
        /// <param name="tableRowKeyGenerator">Table row key generator.</param>
        public OccurrenceRepository(
            ConfigurationSettings configurationSettings,
            TableRowKeyGenerator tableRowKeyGenerator)
            : base(
                configurationSettings,
                PartitionKeyNames.OccurrenceDataTable.TableName,
                PartitionKeyNames.OccurrenceDataTable.OccurrencePartition,
                false)
        {
            this.tableRowKeyGenerator = tableRowKeyGenerator;
        }

        /// <summary>
        /// Gets an event's occurrence in the current year.
        /// </summary>
        /// <param name="eventId">The event id.</param>
        /// <returns>The occurrence meets the criteria.</returns>
        public async Task<OccurrenceEntity> GetOccurrenceInCurrentYearByEventIdAsync(string eventId)
        {
            var filter = TableQuery.GenerateFilterCondition(
                nameof(OccurrenceEntity.EventId),
                QueryComparisons.Equal,
                eventId);

            var occurrences = await this.GetWithFilterAsync(filter);

            var result = occurrences.FirstOrDefault(p => p.Date.Value.Year == DateTime.UtcNow.Year);
            return result;
        }

        /// <summary>
        /// Delete initial occurrences belonging to an event.
        /// </summary>
        /// <param name="eventId">The event id.</param>
        /// <returns>The occurrence entities meet the criteria.</returns>
        public async Task DeleteInitialOccurrencesByEventIdAsync(string eventId)
        {
            var filter1 = TableQuery.GenerateFilterCondition(
                nameof(OccurrenceEntity.EventId),
                QueryComparisons.Equal,
                eventId);

            var filter2 = TableQuery.GenerateFilterConditionForInt(
                nameof(OccurrenceEntity.OccurrenceStateAsInt),
                QueryComparisons.Equal,
                (int)OccurrenceState.Initial);

            var combinedFilter = TableQuery.CombineFilters(filter1, TableOperators.And, filter2);

            var occurrences = await this.GetWithFilterAsync(combinedFilter);

            foreach (var occurrence in occurrences)
            {
                await this.DeleteAsync(occurrence);
            }
        }

        /// <summary>
        /// Get the overdue occurrences that are in initial state.
        /// </summary>
        /// <returns>The occurrence entities meet the criteria.</returns>
        public async Task<IEnumerable<OccurrenceEntity>> GetDueOccurencesInInitialStateAsync()
        {
            var filter1 = TableQuery.GenerateFilterConditionForDate(
                nameof(OccurrenceEntity.Date),
                QueryComparisons.LessThanOrEqual,
                DateTimeOffset.UtcNow);

            var filter2 = TableQuery.GenerateFilterConditionForInt(
                nameof(OccurrenceEntity.OccurrenceStateAsInt),
                QueryComparisons.Equal,
                (int)OccurrenceState.Initial);

            var combinedFilter = TableQuery.CombineFilters(filter1, TableOperators.And, filter2);

            return await this.GetWithFilterAsync(combinedFilter);
        }

        /// <summary>
        /// Set an occurrence state.
        /// </summary>
        /// <param name="id">The occurrence id.</param>
        /// <param name="state">The state to be set to the occurrence.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SetOccurrenceStateAsync(string id, OccurrenceState state)
        {
            var occurrenceEntity = await this.GetAsync(id);
            if (occurrenceEntity == null)
            {
                return;
            }

            occurrenceEntity.OccurrenceState = state;
            await this.CreateOrUpdateAsync(occurrenceEntity);
        }

        /// <summary>
        /// Create occurrence for an event.
        /// </summary>
        /// <param name="eventEntity">The event entity.</param>
        /// <returns>The created occurrence entity.</returns>
        public async Task<OccurrenceEntity> CreateOccurrenceAsync(EventEntity eventEntity)
        {
            var existing = await this.GetOccurrenceInCurrentYearByEventIdAsync(eventEntity.Id);
            if (existing != null)
            {
                return null;
            }

            var id = this.tableRowKeyGenerator.CreateNewKeyOrderingMostRecentToOldest();
            var occurrenceDateTime = this.PrepareOccurrenceDateTime(eventEntity.Date.Value);
            var occurrenceEntity = new OccurrenceEntity
            {
                PartitionKey = PartitionKeyNames.OccurrenceDataTable.OccurrencePartition,
                RowKey = id,
                Id = id,
                EventId = eventEntity.Id,
                Date = occurrenceDateTime,
                TimeZone = eventEntity.TimeZone,
                OwnerAadObjectId = eventEntity.OwnerAadObjectId,
                OccurrenceState = OccurrenceState.Initial,
            };
            await this.CreateOrUpdateAsync(occurrenceEntity);
            return occurrenceEntity;
        }

        // If event date is Feb, 29th, and the current year is not a leap year,
        // then the app uses Feb, 28th as the occurrence date.
        // Otherwise, the app will fail to send the occurrence.
        private DateTime PrepareOccurrenceDateTime(DateTime eventDateTime)
        {
            var year = DateTime.UtcNow.Year;
            var month = eventDateTime.Month;
            var day = eventDateTime.Day;
            var hour = eventDateTime.Hour;
            var minute = eventDateTime.Minute;
            var second = eventDateTime.Second;

            if (!DateTime.IsLeapYear(year) && month == 2 && day > 28)
            {
                day = 28;
            }

            return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
        }
    }
}