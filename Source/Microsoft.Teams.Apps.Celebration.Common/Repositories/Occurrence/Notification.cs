// <copyright file="Notification.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.Repositories.Occurrence
{
    /// <summary>
    /// An event can have many target teams.
    /// The notification class represents one of the teams + the event data.
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// Gets or sets team id value.
        /// </summary>
        public string TeamId { get; set; }

        /// <summary>
        /// Gets or sets the occurrence id.
        /// </summary>
        public string OccurrenceId { get; set; }

        /// <summary>
        /// Gets or sets the event owner's display name.
        /// </summary>
        public string EventOwnerDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the event owner's id.
        /// </summary>
        public string EventOwnerTeamsId { get; set; }

        /// <summary>
        /// Gets or sets the event title.
        /// </summary>
        public string EventTitle { get; set; }

        /// <summary>
        /// Gets or sets the event message.
        /// </summary>
        public string EventMessage { get; set; }

        /// <summary>
        /// Gets or sets the event image.
        /// </summary>
        public string EventImage { get; set; }
    }
}
