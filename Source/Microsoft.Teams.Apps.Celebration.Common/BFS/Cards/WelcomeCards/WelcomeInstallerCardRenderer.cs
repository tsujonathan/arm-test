// <copyright file="WelcomeInstallerCardRenderer.cs" company="Microsoft">
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
    /// that renders card for the feature, welcome installer.
    /// </summary>
    public class WelcomeInstallerCardRenderer : WelcomeCardRendererBase
    {
        private const string WelcomeMessagePart1 =
            "Thanks for installing me. I’m ready to help you celebrate special events with your team. Currently you don’t have me in any of your teams";

        private const string WelcomeMessagePart2 =
            "Click get started to start adding your own events.";

        private const string WelcomeMessagePart3 =
            "Then install me in a team so that I can help you and your team members remember and share their events.";

        /// <summary>
        /// Initializes a new instance of the <see cref="WelcomeInstallerCardRenderer"/> class.
        /// </summary>
        /// <param name="urlLinkService">The URL creation service.</param>
        public WelcomeInstallerCardRenderer(UrlLinkService urlLinkService)
            : base(urlLinkService)
        {
        }

        /// <summary>
        /// Builds the adaptive card that is used in welcoming the installer
        /// who install the bot in personal scope.
        /// </summary>
        /// <returns>The bot attachment with the rendered card.</returns>
        public Attachment BuildAttachment()
        {
            var card = this.Render(this.GetWelcomeMessageTextBlocks);

            var attachment = new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };

            return attachment;
        }

        private List<AdaptiveElement> GetWelcomeMessageTextBlocks()
        {
            return new List<AdaptiveElement>
            {
                new AdaptiveTextBlock
                {
                    Text = WelcomeInstallerCardRenderer.WelcomeMessagePart1,
                    Size = AdaptiveTextSize.Default,
                    Wrap = true,
                    Weight = AdaptiveTextWeight.Default,
                },
                new AdaptiveTextBlock()
                {
                  Text = WelcomeInstallerCardRenderer.WelcomeMessagePart2,
                  Size = AdaptiveTextSize.Default,
                  Wrap = true,
                },
                new AdaptiveTextBlock()
                {
                  Text = WelcomeInstallerCardRenderer.WelcomeMessagePart3,
                  Size = AdaptiveTextSize.Default,
                  Wrap = true,
                },
            };
        }
    }
}
