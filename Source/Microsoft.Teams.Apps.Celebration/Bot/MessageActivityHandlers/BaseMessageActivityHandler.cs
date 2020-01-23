// <copyright file="BaseMessageActivityHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Bot.MessageActivityHandlers
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;

    /// <inheritdoc/>
    /// Message activity handler base class.
    public abstract class BaseMessageActivityHandler : IMessageActivityHandler
    {
        /// <inheritdoc/>
        public async Task HanldeIfApplicableAsync(ITurnContext<IMessageActivity> turnContext)
        {
            if (this.IsApplicable(turnContext))
            {
                await this.HandleAsync(turnContext);
            }
        }

        /// <summary>
        /// Checks if a message activity can be handled or not.
        /// </summary>
        /// <param name="turnContext">The bot turn context.</param>
        /// <returns>The flag indicates if a message activity can be handled or not.</returns>
        protected abstract bool IsApplicable(ITurnContext<IMessageActivity> turnContext);

        /// <summary>
        /// Handles a message activity.
        /// </summary>
        /// <param name="turnContext">The bot turn context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected abstract Task HandleAsync(ITurnContext<IMessageActivity> turnContext);
    }
}