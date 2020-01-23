// <copyright file="UserAadIdFilter.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.Repositories.Event
{
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// Creates OData filters used in querying user data.
    /// </summary>
    public class UserAadIdFilter
    {
        /// <summary>
        /// Gets ODate filter to filter users by UserAadId.
        /// </summary>
        /// <param name="userAadId">The user AadId.</param>
        /// <returns>The filter can filters user by UserAadId.</returns>
        public string GetUserAadIdFilter(string userAadId)
        {
            return TableQuery.GenerateFilterCondition(
                nameof(EventEntity.OwnerAadObjectId),
                QueryComparisons.Equal,
                userAadId);
        }
    }
}
