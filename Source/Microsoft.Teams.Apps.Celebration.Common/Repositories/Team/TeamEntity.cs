// <copyright file="TeamEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.Repositories.Team
{
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// Teams data entity class.
    /// </summary>
    public class TeamEntity : TableEntity
    {
        /// <summary>
        /// Gets or sets team Id.
        /// </summary>
        public string TeamId { get; set; }

        /// <summary>
        /// Gets or sets name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets service URL.
        /// </summary>
        public string ServiceUrl { get; set; }

        /// <summary>
        /// Gets or sets tenant Id.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the name of the user who added the bot in the channel.
        /// </summary>
        public string WhoAddedBotInChannel { get; set; }

        /// <summary>
        /// Gets or sets the active channel id.
        /// </summary>
        public string ActiveChannelId { get; set; }

        /// <summary>
        /// Gets the message target channel id.
        /// </summary>
        [IgnoreProperty]
        public string MessageTargetChannel
        {
            get
            {
                return string.IsNullOrWhiteSpace(this.ActiveChannelId) ? this.TeamId : this.ActiveChannelId;
            }
        }
    }
}
