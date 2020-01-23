// <copyright file="ChangeMessageTargetHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Bot.MessageActivityHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.Models;

    /// <inheritdoc/>
    /// Handles the message activity, change message target.
    public class ChangeMessageTargetHandler : BaseMessageActivityHandler
    {
        /// <inheritdoc/>
        /// Checks if a message activity is to change message target.
        protected override bool IsApplicable(ITurnContext<IMessageActivity> turnContext)
        {
            var text = turnContext.Activity.Text;
            return BotMetadataConstants.ChannelConversationType.Equals(turnContext.Activity.Conversation.ConversationType, StringComparison.OrdinalIgnoreCase)
                && text.Contains(BotCommandConstants.ChangeMessageTarget, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        /// Handles a "change message target" message activity.
        protected override async Task HandleAsync(ITurnContext<IMessageActivity> turnContext)
        {
            var reply = MessageFactory.Attachment(this.GetTaskModuleHeroCard());
            await turnContext.SendActivityAsync(reply);
        }

        private Attachment GetTaskModuleHeroCard()
        {
            var taskModuleDTO = new TaskModuleDTO
            {
                TaskModule = TaskModuleConstants.ChangeMessageTargetTaskModuleName,
            };

            return new HeroCard()
            {
                Text = "Click the following button to target message to the current channel",
                Buttons = new List<CardAction>
                {
                    new TaskModuleAction("Change message target", taskModuleDTO),
                },
            }.ToAttachment();
        }
    }
}