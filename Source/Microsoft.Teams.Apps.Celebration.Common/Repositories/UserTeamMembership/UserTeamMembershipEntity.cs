// <copyright file="UserTeamMembershipEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.Repositories.UserTeamMembership
{
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// User team membership data entity class.
    /// </summary>
    public class UserTeamMembershipEntity : TableEntity
    {
        /// <summary>
        /// Gets or sets user's teams id
        /// </summary>
        public string UserTeamsId { get; set; }

        /// <summary>
        /// Gets or sets id of team, user is member of
        /// </summary>
        public string TeamId { get; set; }

        /// <summary>
        /// Gets or sets the user's AadObjectId.
        /// </summary>
        public string UserAadObjectId { get; set; }
    }
}