// <copyright file="BotAddedInPersonalChatHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Bot.ConversationUpdateActivityHandlers.PersonalChatConversation
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.Cards.WelcomeCards;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.Models;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.User;

    /// <inheritdoc/>
    /// Handles the conversation update activity, add the bot in personal chat.
    public class BotAddedInPersonalChatHandler : BaseConversationUpdateActivityHandler
    {
        private readonly UserRepository userRepository;
        private readonly WelcomeInstallerCardRenderer welcomeInstallerCardRenderer;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotAddedInPersonalChatHandler"/> class.
        /// </summary>
        /// <param name="userRepository">The user repository.</param>
        /// <param name="welcomeInstallerCardRenderer">The welcome installer card renderer.</param>
        public BotAddedInPersonalChatHandler(
            UserRepository userRepository,
            WelcomeInstallerCardRenderer welcomeInstallerCardRenderer)
        {
            this.userRepository = userRepository;
            this.welcomeInstallerCardRenderer = welcomeInstallerCardRenderer;
        }

        /// <inheritdoc/>
        /// Checks if the conversation update activity is to add the bot in personal chat.
        protected override bool IsApplicable(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            if (turnContext.Activity.MembersAdded == null || turnContext.Activity.MembersAdded.Count == 0)
            {
                return false;
            }

            if (!BotMetadataConstants.PersonalConversationType.Equals(turnContext.Activity.Conversation.ConversationType, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Take action if this event includes the bot being added
            return turnContext.Activity.MembersAdded.Any(
                p => p.Id == turnContext.Activity.Recipient.Id);
        }

        /// <inheritdoc/>
        /// Handles a "add the bot in personal chat" conversation update activity.
        protected override async Task HandleAsync(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            var existing = await this.userRepository.GetAsync(turnContext.Activity.From.AadObjectId);
            if (existing != null)
            {
                return;
            }

            await this.userRepository.CreateUserDataAsync(turnContext.Activity);

            var attachment = this.welcomeInstallerCardRenderer.BuildAttachment();

            var reply = MessageFactory.Attachment(attachment);

            await turnContext.SendActivityAsync(reply);
        }
    }
}