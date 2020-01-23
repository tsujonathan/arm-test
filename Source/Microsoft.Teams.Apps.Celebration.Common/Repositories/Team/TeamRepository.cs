// <copyright file="TeamRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.Repositories.Team
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.Celebration.Common.Core;

    /// <summary>
    /// Repository of the team data stored in the table storage.
    /// </summary>
    public class TeamRepository : BaseRepository<TeamEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamRepository"/> class.
        /// </summary>
        /// <param name="configurationSettings">Represents the application configuration settings.</param>
        public TeamRepository(ConfigurationSettings configurationSettings)
            : base(
                  configurationSettings,
                  PartitionKeyNames.TeamDataTable.TableName,
                  PartitionKeyNames.TeamDataTable.TeamDataPartition,
                  false)
        {
        }

        /// <summary>
        /// Gets team data entities by ID values.
        /// </summary>
        /// <param name="teamIds">Team IDs.</param>
        /// <returns>Team data entities.</returns>
        public async Task<IEnumerable<TeamEntity>> GetTeamEntitiesByIdsAsync(IEnumerable<string> teamIds)
        {
            var rowKeysFilter = this.GetRowKeysFilter(teamIds);

            return await this.GetWithFilterAsync(rowKeysFilter);
        }

        /// <summary>
        /// Get all team data entities, and sort the result alphabetically by name.
        /// </summary>
        /// <returns>The team data entities sorted alphabetically by name.</returns>
        public async Task<IEnumerable<TeamEntity>> GetAllSortedAlphabeticallyByNameAsync()
        {
            var teamDataEntities = await this.GetAllAsync();
            var sortedSet = new SortedSet<TeamEntity>(teamDataEntities, new TeamDataEntityComparer());
            return sortedSet;
        }

        /// <summary>
        /// Add channel data in Table Storage.
        /// </summary>
        /// <param name="activity">Bot conversation update activity instance.</param>
        /// <param name="whoAddedBotInChannel">Installer name.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task CreateTeamDataAsync(IConversationUpdateActivity activity, string whoAddedBotInChannel)
        {
            var teamDataEntity = this.ParseTeamData(activity);
            if (teamDataEntity != null)
            {
                teamDataEntity.WhoAddedBotInChannel = whoAddedBotInChannel;
                await this.CreateOrUpdateAsync(teamDataEntity);
            }
        }

        /// <summary>
        /// Updates team entity in DB.
        /// </summary>
        /// <param name="activity">The bot conversation update activity instance.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task UpdateTeamDataAsync(IConversationUpdateActivity activity)
        {
            var teamDataEntity = this.ParseTeamData(activity);
            if (teamDataEntity != null)
            {
                await this.CreateOrUpdateAsync(teamDataEntity);
            }
        }

        /// <summary>
        /// Remove channel data in table storage.
        /// </summary>
        /// <param name="activity">Bot conversation update activity instance.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task RemoveTeamDataAsync(
            IConversationUpdateActivity activity)
        {
            var teamDataEntity = this.ParseTeamData(activity);
            if (teamDataEntity != null)
            {
                var found = await this.GetAsync(PartitionKeyNames.TeamDataTable.TeamDataPartition, teamDataEntity.TeamId);
                if (found != null)
                {
                    await this.DeleteAsync(found);
                }
            }
        }

        private TeamEntity ParseTeamData(IConversationUpdateActivity activity)
        {
            var channelData = activity.GetChannelData<TeamsChannelData>();
            if (channelData != null)
            {
                var teamsDataEntity = new TeamEntity
                {
                    PartitionKey = PartitionKeyNames.TeamDataTable.TeamDataPartition,
                    RowKey = channelData.Team.Id,
                    TeamId = channelData.Team.Id,
                    Name = channelData.Team.Name,
                    ServiceUrl = activity.ServiceUrl,
                    TenantId = channelData.Tenant.Id,
                };

                return teamsDataEntity;
            }

            return null;
        }

        private class TeamDataEntityComparer : IComparer<TeamEntity>
        {
            public int Compare(TeamEntity x, TeamEntity y)
            {
                return x.Name.CompareTo(y.Name);
            }
        }
    }
}
