// <copyright file="UserTeamMembershipRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.Repositories.UserTeamMembership
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Teams.Apps.Celebration.Common.Core;

    /// <summary>
    /// Repository of the user team membership data stored in the table storage.
    /// </summary>
    public class UserTeamMembershipRepository : BaseRepository<UserTeamMembershipEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserTeamMembershipRepository"/> class.
        /// </summary>
        /// <param name="configurationSettings">Configuration settings.</param>
        public UserTeamMembershipRepository(ConfigurationSettings configurationSettings)
            : base(
                configurationSettings,
                PartitionKeyNames.UserTeamMembershipDataTable.TableName,
                PartitionKeyNames.UserTeamMembershipDataTable.UserTeamMembershipPartition,
                false)
        {
        }

        /// <summary>
        /// Adds a user team membership entity in DB.
        /// </summary>
        /// <param name="userTeamsId">The user's MS Teams id.</param>
        /// <param name="teamId">The team id.</param>
        /// <param name="userAadObjectId">The user's AadObjectId.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task AddUserTeamMembershipAsync(string userTeamsId, string teamId, string userAadObjectId)
        {
            var filter1 = TableQuery.GenerateFilterCondition(
                nameof(UserTeamMembershipEntity.TeamId),
                QueryComparisons.Equal,
                teamId);

            var filter2 = TableQuery.GenerateFilterCondition(
                nameof(UserTeamMembershipEntity.UserTeamsId),
                QueryComparisons.Equal,
                userTeamsId);

            var combinedFilter = TableQuery.CombineFilters(filter1, TableOperators.And, filter2);

            var userTeamMembershipEntities = await this.GetWithFilterAsync(combinedFilter);
            if (userTeamMembershipEntities == null || userTeamMembershipEntities.Count() == 0)
            {
                var userTeamMembershipEntity = new UserTeamMembershipEntity
                {
                    PartitionKey = PartitionKeyNames.UserTeamMembershipDataTable.UserTeamMembershipPartition,
                    RowKey = $"{userTeamsId}-{teamId}",
                    UserTeamsId = userTeamsId,
                    TeamId = teamId,
                    UserAadObjectId = userAadObjectId,
                };

                await this.CreateOrUpdateAsync(userTeamMembershipEntity);
            }
        }

        /// <summary>
        /// Deletes a user team membership in DB.
        /// </summary>
        /// <param name="userTeamsId">The user's MS Teams id.</param>
        /// <param name="teamId">The team id.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeleteUserTeamMembershipAsync(string userTeamsId, string teamId)
        {
            var filter1 = TableQuery.GenerateFilterCondition(
                nameof(UserTeamMembershipEntity.TeamId),
                QueryComparisons.Equal,
                teamId);

            var filter2 = TableQuery.GenerateFilterCondition(
                nameof(UserTeamMembershipEntity.UserTeamsId),
                QueryComparisons.Equal,
                userTeamsId);

            var combinedFilter = TableQuery.CombineFilters(filter1, TableOperators.And, filter2);

            var userTeamMembershipEntities = await this.GetWithFilterAsync(combinedFilter);
            if (userTeamMembershipEntities != null)
            {
                foreach (var userTeamMembershipEntity in userTeamMembershipEntities)
                {
                    await this.DeleteAsync(userTeamMembershipEntity);
                }
            }
        }

        /// <summary>
        /// Deletes all memberships belonging to a team.
        /// </summary>
        /// <param name="teamId">The team id.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeleteUserTeamMembershipByTeamIdAsync(string teamId)
        {
            var filter = TableQuery.GenerateFilterCondition(
                nameof(UserTeamMembershipEntity.TeamId),
                QueryComparisons.Equal,
                teamId);

            var userTeamMembershipEntities = await this.GetWithFilterAsync(filter);
            if (userTeamMembershipEntities != null)
            {
                foreach (var userTeamMembershipEntity in userTeamMembershipEntities)
                {
                    await this.DeleteAsync(userTeamMembershipEntity);
                }
            }
        }

        /// <summary>
        /// Deletes all memberships belonging to a user.
        /// </summary>
        /// <param name="userTeamsId">The user's MS Teams id.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeleteUserTeamMembershipByUserTeamsIdAsync(string userTeamsId)
        {
            var filter = TableQuery.GenerateFilterCondition(
                nameof(UserTeamMembershipEntity.UserTeamsId),
                QueryComparisons.Equal,
                userTeamsId);

            var userTeamMembershipEntities = await this.GetWithFilterAsync(filter);
            if (userTeamMembershipEntities != null)
            {
                foreach (var userTeamMembershipEntity in userTeamMembershipEntities)
                {
                    await this.DeleteAsync(userTeamMembershipEntity);
                }
            }
        }

        /// <summary>
        /// Gets user team memberships by teamId.
        /// </summary>
        /// <param name="teamId">The team id.</param>
        /// <returns>The membership list of the team.</returns>
        public async Task<IEnumerable<UserTeamMembershipEntity>> GetUserTeamMembershipByTeamIdAsync(string teamId)
        {
            var filter = TableQuery.GenerateFilterCondition(
                nameof(UserTeamMembershipEntity.TeamId),
                QueryComparisons.Equal,
                teamId);

            var userTeamMembershipEntities = await this.GetWithFilterAsync(filter);

            return userTeamMembershipEntities;
        }

        /// <summary>
        /// Gets all the memberships belonging to a user.
        /// </summary>
        /// <param name="userTeamsId">The user's MS Teams id.</param>
        /// <returns>The memberships belonging to the user.</returns>
        public async Task<IEnumerable<UserTeamMembershipEntity>> GetUserTeamMembershipByUserTeamsIdAsync(string userTeamsId)
        {
            var filter = TableQuery.GenerateFilterCondition(
                nameof(UserTeamMembershipEntity.UserTeamsId),
                QueryComparisons.Equal,
                userTeamsId);

            var userTeamMembershipEntities = await this.GetWithFilterAsync(filter);

            return userTeamMembershipEntities;
        }

        /// <summary>
        /// Gets all memberships by a user's AadObjectId and team id.
        /// </summary>
        /// <param name="teamId">The team id.</param>
        /// <param name="userAadObjectId">The user's AadObjectId.</param>
        /// <returns>The memberships meet the search criteria.</returns>
        public async Task<IEnumerable<UserTeamMembershipEntity>> GetUserTeamMembershipByUserAadObjectIdAsync(string teamId, string userAadObjectId)
        {
            var filter1 = TableQuery.GenerateFilterCondition(
                nameof(UserTeamMembershipEntity.TeamId),
                QueryComparisons.Equal,
                teamId);

            var filter2 = TableQuery.GenerateFilterCondition(
                nameof(UserTeamMembershipEntity.UserAadObjectId),
                QueryComparisons.Equal,
                userAadObjectId);

            var combinedFilter = TableQuery.CombineFilters(filter1, TableOperators.And, filter2);

            var userTeamMembershipEntities = await this.GetWithFilterAsync(combinedFilter);

            return userTeamMembershipEntities;
        }
    }
}
