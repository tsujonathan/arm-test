// <copyright file="EventEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.Repositories.Event
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Azure.Cosmos.Table;
    using Newtonsoft.Json;

    /// <summary>
    /// Event data entity class.
    /// </summary>
    public class EventEntity : TableEntity
    {
        /// <summary>
        /// Gets or sets Id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets EventType value.
        /// e.g. BirthDay or Anniversary.
        /// </summary>
        public string EventType { get; set; }

        /// <summary>
        /// Gets or sets Title value.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the Image Link value.
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets the Summary value.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the EventDateTime value.
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// Gets or sets the EventTimeZone value.
        /// </summary>
        public string TimeZone { get; set; }

        /// <summary>
        /// Gets or sets the Month Day pair value.
        /// </summary>
        public string MonthDayPair { get; set; }

        /// <summary>
        /// Gets or sets the Owner's AAD object id value.
        /// </summary>
        public string OwnerAadObjectId { get; set; }

        /// <summary>
        /// Gets or sets the owner name value.
        /// </summary>
        public string OwnerName { get; set; }

        /// <summary>
        /// Gets or sets the owner's teams id.
        /// </summary>
        public string OwnerTeamsId { get; set; }

        /// <summary>
        /// Gets or sets TeamsInString value.
        /// This property helps to save the Teams data in Azure Table storage.
        /// Table Storage doesn't support array type of property directly.
        /// </summary>
        public string SharedTeamsInString { get; set; }

        /// <summary>
        /// Gets or sets shared teams collection.
        /// </summary>
        [IgnoreProperty]
        public IEnumerable<string> SharedTeams
        {
            get
            {
                return JsonConvert.DeserializeObject<IEnumerable<string>>(this.SharedTeamsInString);
            }

            set
            {
                this.SharedTeamsInString = JsonConvert.SerializeObject(value);
            }
        }
    }
}