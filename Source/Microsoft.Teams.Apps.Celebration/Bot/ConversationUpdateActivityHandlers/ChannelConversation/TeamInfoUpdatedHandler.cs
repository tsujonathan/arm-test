// <copyright file="TeamInfoUpdatedHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Bot.ConversationUpdateActivityHandlers.ChannelConversation
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.Team;

    /// <summary>
    /// Handles the conversation update activity, update team info.
    /// </summary>
    public class TeamInfoUpdatedHandler : BaseConversationUpdateActivityHandler
    {
        private readonly TeamRepository teamRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamInfoUpdatedHandler"/> class.
        /// </summary>
        /// <param name="teamRepository">The team repository.</param>
        public TeamInfoUpdatedHandler(TeamRepository teamRepository)
        {
            this.teamRepository = teamRepository;
        }

        /// <inheritdoc/>
        /// Checks if a conversation update activity is to update team info.
        protected override bool IsApplicable(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            var channelData = turnContext.Activity.GetChannelData<TeamsChannelData>();
            if (channelData == null)
            {
                return false;
            }

            return !string.IsNullOrWhiteSpace(channelData.EventType)
                && channelData.EventType.Equals(
                    BaseConversationUpdateActivityHandler.TeamRenamedEventType,
                    StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        /// Handles a "update team info" conversation update activity.
        protected override async Task HandleAsync(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            await this.teamRepository.UpdateTeamDataAsync(turnContext.Activity);
        }
    }
}
