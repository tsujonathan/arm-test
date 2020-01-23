// <copyright file="IConversationUpdateActivityHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Bot.ConversationUpdateActivityHandlers
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;

    /// <summary>
    /// Handles bot conversation update activity.
    /// </summary>
    public interface IConversationUpdateActivityHandler
    {
        /// <summary>
        /// Handles bot conversation update activity if applicable.
        /// </summary>
        /// <param name="turnContext">The bot turn context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task HandleIfApplicableAsync(ITurnContext<IConversationUpdateActivity> turnContext);
    }
}