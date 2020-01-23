// <copyright file="BaseConversationUpdateActivityHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Bot.ConversationUpdateActivityHandlers
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;

    /// <inheritdoc/>
    /// Conversation update activity handler base class.
    public abstract class BaseConversationUpdateActivityHandler : IConversationUpdateActivityHandler
    {
        /// <summary>
        /// Team rename event type constant.
        /// </summary>
        public static readonly string TeamRenamedEventType = "teamRenamed";

        /// <inheritdoc/>
        public async Task HandleIfApplicableAsync(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            if (this.IsApplicable(turnContext))
            {
                await this.HandleAsync(turnContext);
            }
        }

        /// <summary>
        /// Checks if a conversation activity can be handled or not.
        /// </summary>
        /// <param name="turnContext">The bot turn context.</param>
        /// <returns>The flag indicates if a conversation update activity can be handled or not.</returns>
        protected abstract bool IsApplicable(ITurnContext<IConversationUpdateActivity> turnContext);

        /// <summary>
        /// Handle a conversation update activity.
        /// </summary>
        /// <param name="turnContext">The bot turn context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected abstract Task HandleAsync(ITurnContext<IConversationUpdateActivity> turnContext);
    }
}