// <copyright file="TeamDataController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AdaptiveCards;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.Celebration.Bot;
    using Microsoft.Teams.Apps.Celebration.Common.BFS;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.Cards.ChangeMessageTargetCard;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.Team;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.User;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.UserTeamMembership;
    using Microsoft.Teams.Apps.Celebration.Models;

    /// <summary>
    /// Controller for the teams data.
    /// </summary>
    [Route("api/teamData")]
    [Authorize]
    public class TeamDataController : CelebrationControllerBase
    {
        private readonly TeamRepository teamRepository;
        private readonly UserRepository userRepository;
        private readonly UserTeamMembershipRepository userTeamMembershipRepository;
        private readonly TurnContextService turnContextService;
        private readonly ChangeMessageTargetCardRenderer changeMessageTargetCardRenderer;
        private readonly MessageTargetChannelNameService messageTargetChannelNameService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamDataController"/> class.
        /// </summary>
        /// <param name="teamRepository">Team data repository instance.</param>
        /// <param name="userRepository">User repository service.</param>
        /// <param name="userTeamMembershipRepository">User team membership repository service.</param>
        /// <param name="turnContextService">The bot turn context service. It helps to create a turn context on the fly.</param>
        /// <param name="changeMessageTargetCardRenderer">The change message target card renderer.</param>
        /// <param name="messageTargetChannelNameService">The message target channel name service.</param>
        public TeamDataController(
            TeamRepository teamRepository,
            UserRepository userRepository,
            UserTeamMembershipRepository userTeamMembershipRepository,
            TurnContextService turnContextService,
            ChangeMessageTargetCardRenderer changeMessageTargetCardRenderer,
            MessageTargetChannelNameService messageTargetChannelNameService)
        {
            this.teamRepository = teamRepository;
            this.userRepository = userRepository;
            this.userTeamMembershipRepository = userTeamMembershipRepository;
            this.turnContextService = turnContextService;
            this.changeMessageTargetCardRenderer = changeMessageTargetCardRenderer;
            this.messageTargetChannelNameService = messageTargetChannelNameService;
        }

        /// <summary>
        /// Get data for all teams.
        /// </summary>
        /// <returns>A list of team data.</returns>
        [HttpGet]
        public async Task<IEnumerable<Team>> GetAllTeamDataAsync()
        {
            var userEntity = await this.userRepository.GetAsync(this.UserAadId);
            var userTeamsId = userEntity.UserId;
            var userTeamMemberships = await this.userTeamMembershipRepository.GetUserTeamMembershipByUserTeamsIdAsync(userTeamsId);
            var teamIds = userTeamMemberships.Select(p => p.TeamId);
            var teamEntities = await this.teamRepository.GetTeamEntitiesByIdsAsync(teamIds);
            return teamEntities.Select(p => new Team { TeamId = p.TeamId, Name = p.Name });
        }

        /// <summary>
        /// Gets the adaptive card for user to change the message target channel.
        /// </summary>
        /// <param name="teamId">The team id.</param>
        /// <returns>An adaptive card represents the channels of a team, and the channel selected as the message target.</returns>
        [HttpGet("{teamId}/message-target-card")]
        public async Task<ActionResult<string>> GetAdaptiveCardToChangeMessageTargetAsync(string teamId)
        {
            var teamEntity = await this.teamRepository.GetAsync(teamId);
            if (teamEntity == null)
            {
                return this.NotFound($"Cannot find the team with id {teamId}.");
            }

            AdaptiveCard adaptiveCard = null;

            await this.turnContextService.ContinueConversationAsync(
                teamEntity,
                async (turnContext) =>
                {
                    var channels = await TeamsInfo.GetTeamChannelsAsync(turnContext);
                    var targetChannelId = teamEntity.MessageTargetChannel;
                    adaptiveCard = this.changeMessageTargetCardRenderer.Build(channels, targetChannelId);
                });

            if (adaptiveCard == null)
            {
                throw new ApplicationException("Cannot build up the turn context. Failed to retrieve the message target info.");
            }

            return adaptiveCard.ToJson();
        }

        /// <summary>
        /// Save a change to the message target channel for a team.
        /// </summary>
        /// <param name="teamId">The team id.</param>
        /// <param name="targetChannelId">The changed target channel.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpPut("{teamId}/message-target/{targetChannelId}")]
        public async Task<ActionResult<string>> SaveNotificationTargetForTeamAsync(string teamId, string targetChannelId)
        {
            var teamEntity = await this.teamRepository.GetAsync(teamId);
            if (teamEntity == null)
            {
                return this.NotFound();
            }

            teamEntity.ActiveChannelId = targetChannelId;
            await this.teamRepository.CreateOrUpdateAsync(teamEntity);

            return await this.GetMessageTargetChannelNameAsync(teamEntity);
        }

        [HttpGet("{teamId}/message-target")]
        public async Task<ActionResult<string>> GetNotificationTargetChannelAsync(string teamId)
        {
            var teamEntity = await this.teamRepository.GetAsync(teamId);
            if (teamEntity == null)
            {
                return this.NotFound();
            }

            return await this.GetMessageTargetChannelNameAsync(teamEntity);
        }

        private async Task<string> GetMessageTargetChannelNameAsync(TeamEntity teamEntity)
        {
            // Get team channels.
            IEnumerable<ChannelInfo> channels = null;
            await this.turnContextService.ContinueConversationAsync(
                teamEntity,
                async (turnContext) =>
                {
                    channels = await TeamsInfo.GetTeamChannelsAsync(turnContext);
                });
            if (channels == null)
            {
                throw new ApplicationException($"Failed to get the channels for a MS Teams team {teamEntity.TeamId}.");
            }

            return await this.messageTargetChannelNameService.GetMessageTargetChannelNameAsync(channels, teamEntity);
        }
    }
}
