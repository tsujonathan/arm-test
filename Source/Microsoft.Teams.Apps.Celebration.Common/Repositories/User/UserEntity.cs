﻿// <copyright file="UserEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.Repositories.User
{
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// User data entity class.
    /// </summary>
    public class UserEntity : TableEntity
    {
        /// <summary>
        /// Gets or sets name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets email address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets UPN.
        /// </summary>
        public string Upn { get; set; }

        /// <summary>
        /// Gets or sets AadId.
        /// </summary>
        public string AadId { get; set; }

        /// <summary>
        /// Gets or sets UserId.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets ConversationId.
        /// </summary>
        public string ConversationId { get; set; }

        /// <summary>
        /// Gets or sets ServiceUrl.
        /// </summary>
        public string ServiceUrl { get; set; }

        /// <summary>
        /// Gets or sets TenantId.
        /// </summary>
        public string TenantId { get; set; }
    }
}
