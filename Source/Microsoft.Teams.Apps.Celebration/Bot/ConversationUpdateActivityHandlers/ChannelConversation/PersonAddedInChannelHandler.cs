// <copyright file="PersonAddedInChannelHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Bot.ConversationUpdateActivityHandlers.ChannelConversation
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.Models;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.Team;

    /// <inheritdoc/>
    /// Handles the conversation update activity, add a person in a channel.
    public class PersonAddedInChannelHandler : BaseConversationUpdateActivityHandler
    {
        private readonly TeamRepository teamRepository;
        private readonly WelcomeTeamMembersService welcomeTeamAndMembersService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonAddedInChannelHandler"/> class.
        /// </summary>
        /// <param name="teamRepository">The team repository.</param>
        /// <param name="welcomeTeamAndMembersService">The welcome team and members service.</param>
        public PersonAddedInChannelHandler(
            TeamRepository teamRepository,
            WelcomeTeamMembersService welcomeTeamAndMembersService)
        {
            this.teamRepository = teamRepository;
            this.welcomeTeamAndMembersService = welcomeTeamAndMembersService;
        }

        /// <inheritdoc/>
        /// Checks if the conversation update activity is to add a person in a channel.
        protected override bool IsApplicable(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            if (turnContext.Activity.MembersAdded == null || turnContext.Activity.MembersAdded.Count == 0)
            {
                return false;
            }

            if (!BotMetadataConstants.ChannelConversationType.Equals(turnContext.Activity.Conversation.ConversationType, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // The bot is NOT in the turnContext.Activity.MembersAdded list.
            var result = !turnContext.Activity.MembersAdded.Any(
                p => p.Id == turnContext.Activity.Recipient.Id);

            return result;
        }

        /// <inheritdoc/>
        /// Handles a "add a person in a channel" conversation update activity.
        protected override async Task HandleAsync(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            var serviceUrl = turnContext.Activity.ServiceUrl;
            var teamsChannelData = turnContext.Activity.GetChannelData<TeamsChannelData>();
            var tenantId = teamsChannelData.Tenant.Id;
            var teamId = teamsChannelData.Team.Id;
            var teamName = teamsChannelData.Team.Name;

            var teamEntity = await this.teamRepository.GetAsync(teamId);
            var whoAddedBotInChannel = teamEntity != null ? teamEntity.WhoAddedBotInChannel : "Unknown User";

            // Send welcome message to the members added in the team.
            await this.welcomeTeamAndMembersService.WelcomeTeamMembersAsync(
                serviceUrl,
                tenantId,
                teamId,
                teamName,
                whoAddedBotInChannel,
                turnContext.Activity.MembersAdded);
        }
    }
}