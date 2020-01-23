// <copyright file="EventSummary.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Models
{
    /// <summary>
    /// Event model class.
    /// </summary>
    public class EventSummary
    {
        /// <summary>
        /// Gets or sets Id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets title value.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the Message value.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the Date value.
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// Gets or sets the event type value.
        /// </summary>
        public string EventType { get; set; }

        /// <summary>
        /// Gets or sets the event owner value.
        /// </summary>
        public string Owner { get; set; }
    }
}