// <copyright file="Event.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Event model class.
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Gets or sets Id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets Title value.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the Image Link value.
        /// </summary>
        public int Image { get; set; }

        /// <summary>
        /// Gets or sets the Message value.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the Date value.
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// Gets or sets the event's time zone code.
        /// </summary>
        public string TimeZone { get; set; }

        /// <summary>
        /// Gets or sets the event's type value.
        /// </summary>
        public string EventType { get; set; }

        /// <summary>
        /// Gets or sets the id list of the shared teams.
        /// </summary>
        public IEnumerable<string> SharedTeams { get; set; }
    }
}