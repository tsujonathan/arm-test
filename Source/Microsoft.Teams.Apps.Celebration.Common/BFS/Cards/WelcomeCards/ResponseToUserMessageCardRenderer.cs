// <copyright file="ResponseToUserMessageCardRenderer.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.BFS.Cards.WelcomeCards
{
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.Celebration.Common.BFS;

    /// <summary>
    /// This class represents the adaptive card renderer
    /// that renders card for the feature, response to user message.
    /// </summary>
    public class ResponseToUserMessageCardRenderer : WelcomeCardRendererBase
    {
        private const string WelcomeMessageForUserTitle = "Hi!";
        private const string WelcomeMessagePart4 = "I’m ready to help you celebrate special events with your team. Click get started to add your own events.";

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseToUserMessageCardRenderer"/> class.
        /// </summary>
        /// <param name="urlLinkService">The URL link creation service.</param>
        public ResponseToUserMessageCardRenderer(UrlLinkService urlLinkService)
            : base(urlLinkService)
        {
        }

        /// <summary>
        /// Builds the adaptive card that is used in response to users' message sent to the bot.
        /// </summary>
        /// <returns>The bot activity attachment with the rendered card.</returns>
        public Attachment BuildAttachment()
        {
            var card = this.Render();

            var attachment = new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };

            return attachment;
        }

        private AdaptiveCard Render()
        {
            return this.Render(this.GetWelcomeMessageTextBlocks);
        }

        private List<AdaptiveElement> GetWelcomeMessageTextBlocks()
        {
            return new List<AdaptiveElement>()
            {
                new AdaptiveTextBlock()
                {
                    Text = ResponseToUserMessageCardRenderer.WelcomeMessageForUserTitle,
                    Size = AdaptiveTextSize.Large,
                    Weight = AdaptiveTextWeight.Bolder,
                },
                new AdaptiveTextBlock()
                {
                    Text = ResponseToUserMessageCardRenderer.WelcomeMessagePart4,
                    Size = AdaptiveTextSize.Default,
                    Wrap = true,
                    Spacing = AdaptiveSpacing.None,
                },
            };
        }
    }
}
