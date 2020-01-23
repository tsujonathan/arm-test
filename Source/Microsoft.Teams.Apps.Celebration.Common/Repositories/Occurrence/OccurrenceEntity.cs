// <copyright file="OccurrenceEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.Repositories.Occurrence
{
    using System;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// Occurrence entity class.
    /// </summary>
    public class OccurrenceEntity : TableEntity
    {
        /// <summary>
        /// Gets or sets Id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the event id.
        /// </summary>
        public string EventId { get; set; }

        /// <summary>
        /// Gets or sets the EventDateTime value.
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// Gets or sets the EventTimeZone value.
        /// </summary>
        public string TimeZone { get; set; }

        /// <summary>
        /// Gets or sets the Owner's AAD object id value.
        /// </summary>
        public string OwnerAadObjectId { get; set; }

        /// <summary>
        /// Gets or sets the occurrence state as integer.
        /// Since the Azure storage account table doesn't take enum type,
        /// We have to save enum values as integer.
        /// </summary>
        public int OccurrenceStateAsInt { get; set; }

        /// <summary>
        /// Gets or sets the occurrence state value.
        /// The value is not saved in DB.
        /// </summary>
        [IgnoreProperty]
        public OccurrenceState OccurrenceState
        {
            get
            {
                return (OccurrenceState)this.OccurrenceStateAsInt;
            }

            set
            {
                this.OccurrenceStateAsInt = (int)value;
            }
        }

        /// <summary>
        /// Gets or sets the Sent DateTime value.
        /// </summary>
        public DateTime? SentDate { get; set; }

        /// <summary>
        /// Gets or sets the number of recipients who have received the event successfully.
        /// </summary>
        public int Succeeded { get; set; }

        /// <summary>
        /// Gets or sets the number of recipients who failed in receiving the event.
        /// </summary>
        public int Failed { get; set; }

        /// <summary>
        /// Gets or sets the number of recipients who were throttled out.
        /// </summary>
        public int Throttled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the sending process is completed or not.
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Gets or sets the total number of expected messages to send.
        /// </summary>
        public int TotalMessageCount { get; set; }

        /// <summary>
        /// Gets or sets the exception message.
        /// </summary>
        public string ExceptionMessage { get; set; }

        /// <summary>
        /// Gets or sets the warning message.
        /// </summary>
        public string WarningMessage { get; set; }
    }
}