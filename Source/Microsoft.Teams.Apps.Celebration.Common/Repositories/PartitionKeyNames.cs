// <copyright file="PartitionKeyNames.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.Repositories
{
    /// <summary>
    /// Partition key names used in the table storage.
    /// </summary>
    public static class PartitionKeyNames
    {
        /// <summary>
        /// Users data table partition key names.
        /// </summary>
        public static class UserDataTable
        {
            /// <summary>
            /// Table name for user data table
            /// </summary>
            public static readonly string TableName = "UserData";

            /// <summary>
            /// Users data partition key name.
            /// </summary>
            public static readonly string UserDataPartition = "UserData";
        }

        /// <summary>
        /// Table name for user team membership table.
        /// </summary>
        public static class UserTeamMembershipDataTable
        {
            /// <summary>
            /// The user team membership table name.
            /// </summary>
            public static readonly string TableName = "UserTeamMembershipData";

            /// <summary>
            /// The user team membership table partition key.
            /// </summary>
            public static readonly string UserTeamMembershipPartition = "UserTeamMembershipData";
        }

        /// <summary>
        /// Teams data table partition key names.
        /// </summary>
        public static class TeamDataTable
        {
            /// <summary>
            /// Table name for team data table
            /// </summary>
            public static readonly string TableName = "TeamData";

            /// <summary>
            /// Team data partition key name.
            /// </summary>
            public static readonly string TeamDataPartition = "TeamData";
        }

        /// <summary>
        /// Event data table partition key names.
        /// </summary>
        public static class EventDataTable
        {
            /// <summary>
            /// Table name for event entity table
            /// </summary>
            public static readonly string TableName = "EventData";

            /// <summary>
            /// Event partition key name.
            /// </summary>
            public static readonly string EventPartition = "EventData";
        }

        /// <summary>
        /// Event occurrence data table partition key names.
        /// </summary>
        public static class OccurrenceDataTable
        {
            /// <summary>
            /// Table name for event occurrence entity table
            /// </summary>
            public static readonly string TableName = "OccurrenceData";

            /// <summary>
            /// Event occurrence partition key name.
            /// </summary>
            public static readonly string OccurrencePartition = "OccurrenceData";
        }
    }
}
