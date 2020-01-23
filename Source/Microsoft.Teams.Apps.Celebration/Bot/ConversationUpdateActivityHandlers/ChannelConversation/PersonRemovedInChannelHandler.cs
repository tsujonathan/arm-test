// <copyright file="PersonRemovedInChannelHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Bot.ConversationUpdateActivityHandlers.ChannelConversation
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.Models;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.Event;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.UserTeamMembership;

    /// <inheritdoc/>
    /// Handles the conversation update activity, remove a person from a channel.
    public class PersonRemovedInChannelHandler : BaseConversationUpdateActivityHandler
    {
        private readonly UserTeamMembershipRepository userTeamMembershipRepository;
        private readonly EventRepository eventRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonRemovedInChannelHandler"/> class.
        /// </summary>
        /// <param name="userTeamMembershipRepository">The user team membership repository.</param>
        /// <param name="eventRepository">The event repository.</param>
        public PersonRemovedInChannelHandler(
            UserTeamMembershipRepository userTeamMembershipRepository,
            EventRepository eventRepository)
        {
            this.userTeamMembershipRepository = userTeamMembershipRepository;
            this.eventRepository = eventRepository;
        }

        /// <inheritdoc/>
        /// Checks if the conversation update activity is to remove a person from a channel.
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

            var result = !turnContext.Activity.MembersRemoved.Any(
                p => p.Id == turnContext.Activity.Recipient.Id);

            return result;
        }

        /// <inheritdoc/>
        /// Handles a "remove a person from a channel" conversation update activity.
        protected override async Task HandleAsync(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            foreach (var teamMember in turnContext.Activity.MembersRemoved)
            {
                var userTeamsId = teamMember.Id;
                var teamId = turnContext.Activity.Conversation.Id;

                await this.eventRepository.StopSharingPersonalEventsWithATeam(userTeamsId, teamId);

                await this.userTeamMembershipRepository.DeleteUserTeamMembershipAsync(userTeamsId, teamId);
            }
        }
    }
}
