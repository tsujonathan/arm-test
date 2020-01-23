// <copyright file="ShareEventCardDTO.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.BFS.Cards.WelcomeCards
{
    /// <summary>
    /// The DTO represents the event information carried by the ShareEventCard as its Value property.
    ///
    /// The bot sends the share event card to users with the DTO object carried.
    ///
    /// When users reply to the bot by using the share event card's actions,
    /// the DTO's object will be sent back to the bot.
    /// </summary>
    public class ShareEventCardDTO
    {
        /// <summary>
        /// Gets or sets adaptive card submit button action
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets userAadObjectId.
        /// </summary>
        public string UserAadObjectId { get; set; }

        /// <summary>
        /// Gets or sets teamId.
        /// </summary>
        public string TeamId { get; set; }

        /// <summary>
        /// Gets or sets teamName.
        /// </summary>
        public string TeamName { get; set; }
    }
}
