// <copyright file="IMessageActivityHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Bot.MessageActivityHandlers
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;

    /// <summary>
    /// Handles bot message activity.
    /// </summary>
    public interface IMessageActivityHandler
    {
        /// <summary>
        /// Handles bot message activity if applicable.
        /// </summary>
        /// <param name="turnContext">The bot turn context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task HanldeIfApplicableAsync(ITurnContext<IMessageActivity> turnContext);
    }
}