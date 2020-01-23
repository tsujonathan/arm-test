// <copyright file="ShareEventCardRenderer.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.BFS.Cards.WelcomeCards
{
    using System.Collections.Generic;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.Models;
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents the hero card renderer
    /// that renders card for the feature, share event.
    /// </summary>
    public class ShareEventCardRenderer
    {
        private const string EventShareMessage = "Looks like you joined a new team. Would you like to share your events in {0} as well?";
        private const string ShareButtonTitle = "Share";
        private const string NoThanksButtonTitle = "No, thanks";

        /// <summary>
        /// Builds the hero card that is used in sharing event.
        ///
        /// When user joined a team (or the bot is added in a team),
        /// the bot asks the users if they want to share their events with the team.
        ///
        /// The card rendered by the class is used in this case.
        /// </summary>
        /// <param name="teamId">The team id.</param>
        /// <param name="teamName">The team name.</param>
        /// <param name="userAadObjectId">The user's AadObjectId.</param>
        /// <returns>The bot attachment with the rendered card.</returns>
        public Attachment BuildAttachment(
            string teamId,
            string teamName,
            string userAadObjectId = null)
        {
            var card = this.Render(teamName, teamId, userAadObjectId);

            var attachment = new Attachment
            {
                ContentType = HeroCard.ContentType,
                Content = card,
            };

            return attachment;
        }

        /// <summary>
        /// Builds the sharing event card, but without the actions.
        /// </summary>
        /// <param name="teamName">The team name.</param>
        /// <returns>The bot attachment with the rendered card.</returns>
        public Attachment BuildAttachmentWithoutAction(string teamName)
        {
            var card = this.RenderWithoutAction(teamName);

            var attachment = new Attachment
            {
                ContentType = HeroCard.ContentType,
                Content = card,
            };

            return attachment;
        }

        private HeroCard Render(string teamName, string teamId, string userAadObjectId)
        {
            return new HeroCard()
            {
                Text = string.Format(ShareEventCardRenderer.EventShareMessage, teamName),
                Buttons = this.GetCardActions(teamName, teamId, userAadObjectId),
            };
        }

        private HeroCard RenderWithoutAction(string teamName)
        {
            return new HeroCard()
            {
                Text = string.Format(ShareEventCardRenderer.EventShareMessage, teamName),
            };
        }

        private List<CardAction> GetCardActions(string teamName, string teamId, string userAadObjectId)
        {
            if (string.IsNullOrWhiteSpace(teamId) || string.IsNullOrWhiteSpace(userAadObjectId))
            {
                return null;
            }

            return new List<CardAction>
            {
                new CardAction()
                {
                    Title = ShareEventCardRenderer.ShareButtonTitle,
                    DisplayText = ShareEventCardRenderer.ShareButtonTitle,
                    Type = ActionTypes.MessageBack,
                    Text = BotCommandConstants.ShareEvent,
                    Value = JsonConvert.SerializeObject(new ShareEventCardDTO
                    {
                        Action = BotCommandConstants.ShareEvent,
                        TeamId = teamId,
                        TeamName = teamName,
                        UserAadObjectId = userAadObjectId,
                    }),
                },
                new CardAction()
                {
                    Title = ShareEventCardRenderer.NoThanksButtonTitle,
                    DisplayText = ShareEventCardRenderer.NoThanksButtonTitle,
                    Type = ActionTypes.MessageBack,
                    Text = BotCommandConstants.IgnoreEventShare,
                    Value = JsonConvert.SerializeObject(new ShareEventCardDTO
                    {
                        Action = BotCommandConstants.IgnoreEventShare,
                        TeamId = teamId,
                        TeamName = teamName,
                        UserAadObjectId = userAadObjectId,
                    }),
                },
            };
        }
    }
}
