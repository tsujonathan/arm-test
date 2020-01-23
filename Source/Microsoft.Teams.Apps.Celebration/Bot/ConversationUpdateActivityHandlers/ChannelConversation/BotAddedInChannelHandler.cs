// <copyright file="BotAddedInChannelHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Bot.ConversationUpdateActivityHandlers.ChannelConversation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.BotConnectorClient;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.Cards.WelcomeCards;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.Models;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.Team;

    /// <inheritdoc/>
    /// Handles the conversation update activity, add the bot in a channel.
    public class BotAddedInChannelHandler : BaseConversationUpdateActivityHandler
    {
        private readonly TeamRepository teamRepository;
        private readonly BotConnectorClientFactory botConnectorClientFactory;
        private readonly WelcomeTeamMembersService welcomeTeamAndMembersService;
        private readonly WelcomeTeamMembersCardRenderer welcomeTeamMembersCardRenderer;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotAddedInChannelHandler"/> class.
        /// </summary>
        /// <param name="teamRepository">The team repository.</param>
        /// <param name="botConnectorClientFactory">The bot connect client factory.</param>
        /// <param name="welcomeTeamAndMembersService">The welcome team and members service.</param>
        /// <param name="welcomeTeamMembersCardRenderer">The welcome team members card renderer.</param>
        public BotAddedInChannelHandler(
            TeamRepository teamRepository,
            BotConnectorClientFactory botConnectorClientFactory,
            WelcomeTeamMembersService welcomeTeamAndMembersService,
            WelcomeTeamMembersCardRenderer welcomeTeamMembersCardRenderer)
        {
            this.teamRepository = teamRepository;
            this.botConnectorClientFactory = botConnectorClientFactory;
            this.welcomeTeamAndMembersService = welcomeTeamAndMembersService;
            this.welcomeTeamMembersCardRenderer = welcomeTeamMembersCardRenderer;
        }

        /// <inheritdoc/>
        /// Checks if the conversation update activity is to add the bot in a channel.
        protected override bool IsApplicable(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            if (turnContext.Activity.MembersAdded == null || turnContext.Activity.MembersAdded.Count == 0)
            {
                return false;
            }

            if (!BotMetadataConstants.ChannelConversationType.Equals(turnContext.Activity.Conversation.ConversationType, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Take action if this event includes the bot being added
            // Noticed that the bot is in the turnContext.Activity.MembersAdded list.
            var result = turnContext.Activity.MembersAdded.Any(
                p => p.Id == turnContext.Activity.Recipient.Id);

            return result;
        }

        /// <inheritdoc/>
        /// Handles a "add the bot in a channel" conversation update activity.
        protected override async Task HandleAsync(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            var activityMembers = await this.GetActivityMembers(turnContext);
            var whoAddedBotInChannel = this.GetWhoAddedBotInChannel(turnContext, activityMembers);

            await this.teamRepository.CreateTeamDataAsync(turnContext.Activity, whoAddedBotInChannel);

            // Sends welcome message to the team's dedicated channel.
            var teamsChannelData = turnContext.Activity.GetChannelData<TeamsChannelData>();
            var tenantId = teamsChannelData.Tenant.Id;
            var teamId = teamsChannelData.Team.Id;
            var teamName = teamsChannelData.Team.Name;
            await this.WelcomeTeamAsync(turnContext, teamName, whoAddedBotInChannel);

            // Sends welcome message to every team member.
            // Please noted that the message is sent to their bot personal chats.
            var serviceUrl = turnContext.Activity.ServiceUrl;
            await this.welcomeTeamAndMembersService.WelcomeTeamMembersAsync(
                serviceUrl,
                tenantId,
                teamId,
                teamName,
                whoAddedBotInChannel,
                activityMembers);
        }

        private async Task WelcomeTeamAsync(
            ITurnContext<IConversationUpdateActivity> turnContext,
            string teamName,
            string whoAddedBotInChannel)
        {
            var attachment = this.welcomeTeamMembersCardRenderer.BuildAttachment(whoAddedBotInChannel, teamName);

            var reply = MessageFactory.Attachment(attachment);

            await turnContext.SendActivityAsync(reply);
        }

        private async Task<IEnumerable<ChannelAccount>> GetActivityMembers(
            ITurnContext<IConversationUpdateActivity> turnContext)
        {
            var customConnectorClient = this.botConnectorClientFactory.Create(turnContext.Activity.ServiceUrl);
            var activityMembers = await customConnectorClient.GetActivityMembersAsync(turnContext);
            return activityMembers;
        }

        private string GetWhoAddedBotInChannel(
            ITurnContext<IConversationUpdateActivity> turnContext,
            IEnumerable<ChannelAccount> activityMembers)
        {
            var botInstallerAadObjectId = turnContext.Activity.From.AadObjectId;
            var matching = activityMembers.FirstOrDefault(activityMember =>
            {
                var objectId = activityMember.Properties["objectId"].ToString();
                return objectId.Equals(botInstallerAadObjectId, StringComparison.OrdinalIgnoreCase);
            });
            return matching != null ? matching.Name : botInstallerAadObjectId;
        }
    }
}