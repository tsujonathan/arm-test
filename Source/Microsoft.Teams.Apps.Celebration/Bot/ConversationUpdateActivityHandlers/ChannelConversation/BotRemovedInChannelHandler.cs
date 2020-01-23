// <copyright file="BotRemovedInChannelHandler.cs" company="Microsoft">
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
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.Event;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.Team;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.UserTeamMembership;

    /// <inheritdoc/>
    /// Handles the conversation update activity, remove the bot in a channel.
    public class BotRemovedInChannelHandler : BaseConversationUpdateActivityHandler
    {
        private readonly TeamRepository teamRepository;
        private readonly EventRepository eventRepository;
        private readonly UserTeamMembershipRepository userTeamMembershipRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotRemovedInChannelHandler"/> class.
        /// </summary>
        /// <param name="teamRepository">The team repository.</param>
        /// <param name="eventRepository">The event repository.</param>
        /// <param name="userTeamMembershipRepository">The user team membership repository.</param>
        public BotRemovedInChannelHandler(
            TeamRepository teamRepository,
            EventRepository eventRepository,
            UserTeamMembershipRepository userTeamMembershipRepository)
        {
            this.teamRepository = teamRepository;
            this.eventRepository = eventRepository;
            this.userTeamMembershipRepository = userTeamMembershipRepository;
        }

        /// <inheritdoc/>
        /// Checks if a conversation update activity is to remove the bot in a channel.
        protected override bool IsApplicable(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            if (turnContext.Activity.MembersRemoved == null || turnContext.Activity.MembersRemoved.Count == 0)
            {
                return false;
            }

            if (!BotMetadataConstants.ChannelConversationType.Equals(turnContext.Activity.Conversation.ConversationType, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Take action if this event includes the bot being added
            return turnContext.Activity.MembersRemoved.Any(
                p => p.Id == turnContext.Activity.Recipient.Id);
        }

        /// <inheritdoc/>
        /// Handles a "remove the bot in a channel" conversation update activity.
        protected override async Task HandleAsync(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            await this.teamRepository.RemoveTeamDataAsync(turnContext.Activity);

            var channelData = turnContext.Activity.GetChannelData<TeamsChannelData>();
            if (channelData != null)
            {
                var teamId = channelData.Team.Id;

                await this.eventRepository.StopSharingAllEventsWithATeam(teamId);

                await this.userTeamMembershipRepository.DeleteUserTeamMembershipByTeamIdAsync(teamId);
            }
        }
    }
}