// <copyright file="WelcomeTeamMembersCardRenderer.cs" company="Microsoft">
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
    /// that renders card for the feature, welcome team members.
    /// </summary>
    public class WelcomeTeamMembersCardRenderer : WelcomeCardRendererBase
    {
        private const string WelcomeMessageForTeamTitle = "Hi!";
        private const string WelcomeMessageForTeam =
            "I'm the Celebrations bot. {0} installed me in {1}. I’m here to help everyone celebrate birthdays, anniversaries, and anything else you tell me about.";

        /// <summary>
        /// Initializes a new instance of the <see cref="WelcomeTeamMembersCardRenderer"/> class.
        /// </summary>
        /// <param name="urlLinkService">The URL link creation service.</param>
        public WelcomeTeamMembersCardRenderer(UrlLinkService urlLinkService)
            : base(urlLinkService)
        {
        }

        /// <summary>
        /// Builds the adaptive card that is used in welcoming team members.
        /// </summary>
        /// <param name="botInstallerName">The bot installer's name.</param>
        /// <param name="teamName">The team's name.</param>
        /// <returns>The bot attachment with the rendered card.</returns>
        public Attachment BuildAttachment(string botInstallerName, string teamName)
        {
            var card = this.Render(botInstallerName, teamName);

            var attachment = new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };

            return attachment;
        }

        private AdaptiveCard Render(string botInstallerName, string teamName)
        {
            var adaptiveCard = this.Render(() =>
            {
                return this.GetWelcomeMessageTextBlocks(botInstallerName, teamName);
            });

            return adaptiveCard;
        }

        private List<AdaptiveElement> GetWelcomeMessageTextBlocks(string botInstallerName, string teamName)
        {
            return new List<AdaptiveElement>()
            {
                new AdaptiveTextBlock()
                {
                    Text = WelcomeTeamMembersCardRenderer.WelcomeMessageForTeamTitle,
                    Size = AdaptiveTextSize.Large,
                    Weight = AdaptiveTextWeight.Bolder,
                },
                new AdaptiveTextBlock()
                {
                    Text = string.Format(WelcomeTeamMembersCardRenderer.WelcomeMessageForTeam, botInstallerName, teamName),
                    Size = AdaptiveTextSize.Default,
                    Wrap = true,
                    Spacing = AdaptiveSpacing.None,
                },
            };
        }
    }
}
