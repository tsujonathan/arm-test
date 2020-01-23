// <copyright file="BotRemovedInPersonalChatHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Bot.ConversationUpdateActivityHandlers.PersonalChatConversation
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.Models;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.User;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.UserTeamMembership;

    /// <inheritdoc/>
    /// Handles the conversation update activity, remove the bot in personal chat.
    public class BotRemovedInPersonalChatHandler : BaseConversationUpdateActivityHandler
    {
        private readonly UserRepository userRepository;
        private readonly UserTeamMembershipRepository userTeamMembershipRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotRemovedInPersonalChatHandler"/> class.
        /// </summary>
        /// <param name="userRepository">The user repository.</param>
        /// <param name="userTeamMembershipRepository">The user team membership repository.</param>
        public BotRemovedInPersonalChatHandler(
            UserRepository userRepository,
            UserTeamMembershipRepository userTeamMembershipRepository)
        {
            this.userRepository = userRepository;
            this.userTeamMembershipRepository = userTeamMembershipRepository;
        }

        /// <inheritdoc/>
        /// Checks if a conversation update activity is to remove the bot in personal chat.
        protected override bool IsApplicable(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            if (turnContext.Activity.MembersRemoved == null || turnContext.Activity.MembersRemoved.Count == 0)
            {
                return false;
            }

            if (!BotMetadataConstants.PersonalConversationType.Equals(turnContext.Activity.Conversation.ConversationType, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Take action if this event includes the bot being added
            return turnContext.Activity.MembersRemoved.Any(
                p => p.Id == turnContext.Activity.Recipient.Id);
        }

        /// <inheritdoc/>
        /// Handles a "remove the bot in personal chat" conversation update activity.
        protected override async Task HandleAsync(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            await this.userRepository.RemoveUserDataAsync(turnContext.Activity);

            await this.userTeamMembershipRepository.DeleteUserTeamMembershipByUserTeamsIdAsync(turnContext.Activity.From?.Id);
        }
    }
}