// <copyright file="PreviewCardDTO.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.BFS.Cards.PreviewCard
{
    /// <summary>
    /// The DTO represents the event information carried by the PreviewCard as its Value property.
    ///
    /// The bot sends the preview card to users with the DTO object carried.
    ///
    /// When users reply to the bot by using the preview card's actions,
    /// the DTO's object will be sent back to the bot.
    /// </summary>
    public class PreviewCardDTO
    {
        /// <summary>
        /// Gets or sets adaptive card submit button action
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets the event id
        /// </summary>
        public string EventId { get; set; }

        /// <summary>
        /// Gets or sets the occurrence id
        /// </summary>
        public string OccurrenceId { get; set; }

        /// <summary>
        /// Gets or sets OwnerAadObjectId
        /// </summary>
        public string OwnerAadObjectId { get; set; }

        /// <summary>
        /// Gets or sets OwnerName
        /// </summary>
        public string OwnerName { get; set; }
    }
}
