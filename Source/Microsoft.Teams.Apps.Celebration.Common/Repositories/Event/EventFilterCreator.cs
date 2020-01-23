// <copyright file="EventFilterCreator.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.Repositories.Event
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// Creates OData filters used in querying event data.
    /// </summary>
    public class EventFilterCreator
    {
        /// <summary>
        /// Creates event date range OData filter.
        /// </summary>
        /// <param name="fromDateTime">The beginning date of the range.</param>
        /// <param name="toDateTime">The ending date of the range.</param>
        /// <returns>OData filter string.</returns>
        public string CreateEventDateRangeFilter(DateTime fromDateTime, DateTime toDateTime)
        {
            if (fromDateTime.Year == toDateTime.Year)
            {
                return this.CreatEventDateRangeWithinAYearFilter(fromDateTime, toDateTime);
            }

            var fromDateTimeYearEnd = new DateTime(fromDateTime.Year, 12, 31);
            var filter1 = this.CreatEventDateRangeWithinAYearFilter(fromDateTime, fromDateTimeYearEnd);

            var toDateTimeNewYear = new DateTime(toDateTime.Year, 1, 1);
            var filter2 = this.CreatEventDateRangeWithinAYearFilter(toDateTimeNewYear, toDateTime);

            return TableQuery.CombineFilters(filter1, TableOperators.Or, filter2);
        }

        /// <summary>
        /// Create filter for the upcoming events due in 72 hours.
        /// </summary>
        /// <returns>OData filter string.</returns>
        public string CreateUpcomingEventFilter()
        {
            var upcomingMonthDayPairs = this.GetUpcomingMonthDatePairs();
            string combinedfilters = null;
            foreach (var upcomingMonthDayPair in upcomingMonthDayPairs)
            {
                var filter = TableQuery.GenerateFilterCondition(
                    nameof(EventEntity.MonthDayPair),
                    QueryComparisons.Equal,
                    upcomingMonthDayPair);

                combinedfilters = string.IsNullOrWhiteSpace(combinedfilters)
                    ? filter
                    : TableQuery.CombineFilters(combinedfilters, TableOperators.Or, filter);
            }

            return combinedfilters;
        }

        // Create upcoming month-day pair list. (in next 72 hours)
        private IEnumerable<string> GetUpcomingMonthDatePairs()
        {
            var currentDateTime = DateTime.UtcNow;
            var upcomingMonthDatePairs = new List<string>();
            for (int i = 0; i < 3; i++)
            {
                var upcomingDateTime = currentDateTime.AddDays(i);
                upcomingMonthDatePairs.Add(upcomingDateTime.ToString("MMdd"));
            }

            // Add 29th Feb in reference set if the current year is not leap year. so, the events which occurs on 29th Feb would not get skipped this year
            if (!DateTime.IsLeapYear(currentDateTime.Year)
                && currentDateTime.Month == 2
                && currentDateTime.Day <= 29
                && 29 - currentDateTime.Day < 3)
            {
                upcomingMonthDatePairs.Add("0229");
            }

            return upcomingMonthDatePairs;
        }

        private string CreatEventDateRangeWithinAYearFilter(DateTime fromDateTime, DateTime toDateTime)
        {
            var filter1 = TableQuery.GenerateFilterCondition(
                nameof(EventEntity.MonthDayPair),
                QueryComparisons.GreaterThanOrEqual,
                fromDateTime.ToString("MMdd"));

            var filter2 = TableQuery.GenerateFilterCondition(
                nameof(EventEntity.MonthDayPair),
                QueryComparisons.LessThanOrEqual,
                toDateTime.ToString("MMdd"));

            var combinedfilter = TableQuery.CombineFilters(filter1, TableOperators.And, filter2);

            return combinedfilter;
        }
    }
}
