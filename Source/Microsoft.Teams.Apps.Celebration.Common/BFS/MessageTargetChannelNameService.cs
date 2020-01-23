// <copyright file="MessageTargetChannelNameService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.BFS
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.Team;

    /// <summary>
    /// Gets the message target channel name.
    /// If a team's target channel is absent in the channel list,
    /// Then reset the team's target channel to the general channel.
    /// </summary>
    public class MessageTargetChannelNameService
    {
        private readonly TeamRepository teamRepository;

        public MessageTargetChannelNameService(TeamRepository teamRepository)
        {
            this.teamRepository = teamRepository;
        }

        /// <summary>
        /// Gets the team's message target channel name.
        /// </summary>
        /// <param name="channels">The teams' channel list.</param>
        /// <param name="teamEntity">The team entity.</param>
        /// <returns>The message target channel name.</returns>
        public async Task<string> GetMessageTargetChannelNameAsync(
            IEnumerable<ChannelInfo> channels,
            TeamEntity teamEntity)
        {
            var matching = channels.FirstOrDefault(p => p.Id.Equals(teamEntity.MessageTargetChannel, StringComparison.OrdinalIgnoreCase));
            if (matching != null)
            {
                return string.IsNullOrWhiteSpace(matching.Name) ? "General" : matching.Name;
            }

            teamEntity.ActiveChannelId = null;
            await this.teamRepository.CreateOrUpdateAsync(teamEntity);

            return "General";
        }
    }
}
