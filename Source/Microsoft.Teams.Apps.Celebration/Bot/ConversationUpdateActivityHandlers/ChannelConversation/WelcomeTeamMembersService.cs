// <copyright file="WelcomeTeamMembersService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Bot.ConversationUpdateActivityHandlers.ChannelConversation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.Celebration.Common.BFS;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.BotConnectorClient;
    using Microsoft.Teams.Apps.Celebration.Common.BFS.Cards.WelcomeCards;
    using Microsoft.Teams.Apps.Celebration.Common.Core;
    using Microsoft.Teams.Apps.Celebration.Common.Queues;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.Event;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.User;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.UserTeamMembership;

    /// <summary>
    /// Sends welcome messages to team members' personal chat.
    /// </summary>
    public class WelcomeTeamMembersService
    {
        private readonly ConfigurationSettings configurationSettings;
        private readonly EventRepository eventRepository;
        private readonly BotConnectorClientFactory botConnectorClientFactory;
        private readonly SendToConversationQueue sendToConversationQueue;
        private readonly UserRepository userRepository;
        private readonly UserTeamMembershipRepository userTeamMembershipRepository;
        private readonly WelcomeTeamMembersCardRenderer welcomeTeamMembersCardRenderer;
        private readonly ShareEventCardRenderer shareEventCardRenderer;
        private readonly BotActivityBuilder botActivityBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="WelcomeTeamMembersService"/> class.
        /// </summary>
        /// <param name="configurationSettings">the configuration settings object.</param>
        /// <param name="eventRepository">The event repository.</param>
        /// <param name="botConnectorClientFactory">The bot connector client factory service.</param>
        /// <param name="sendToConversationQueue">The Azure service bus queue which triggers the send bot messages Azure function.</param>
        /// <param name="userRepository">The user repository.</param>
        /// <param name="userTeamMembershipRepository">The user membership repository.</param>
        /// <param name="welcomeTeamMembersCardRenderer">The welcome team members card renderer.</param>
        /// <param name="shareEventCardRenderer">The share event card renderer.</param>
        /// <param name="botActivityBuilder">The bot activity builder.</param>
        public WelcomeTeamMembersService(
            ConfigurationSettings configurationSettings,
            EventRepository eventRepository,
            BotConnectorClientFactory botConnectorClientFactory,
            SendToConversationQueue sendToConversationQueue,
            UserRepository userRepository,
            UserTeamMembershipRepository userTeamMembershipRepository,
            WelcomeTeamMembersCardRenderer welcomeTeamMembersCardRenderer,
            ShareEventCardRenderer shareEventCardRenderer,
            BotActivityBuilder botActivityBuilder)
        {
            this.configurationSettings = configurationSettings;
            this.eventRepository = eventRepository;
            this.botConnectorClientFactory = botConnectorClientFactory;
            this.sendToConversationQueue = sendToConversationQueue;
            this.userRepository = userRepository;
            this.userTeamMembershipRepository = userTeamMembershipRepository;
            this.welcomeTeamMembersCardRenderer = welcomeTeamMembersCardRenderer;
            this.shareEventCardRenderer = shareEventCardRenderer;
            this.botActivityBuilder = botActivityBuilder;
        }

        /// <summary>
        /// Sends welcome message to team members.
        /// The method is called in the following two scenarios:
        /// 1). When the bot is added in a team.
        /// 2). When a person is added in a team.
        /// </summary>
        /// <param name="serviceUrl">The bot service URL.</param>
        /// <param name="tenantId">The bot tenant id.</param>
        /// <param name="teamId">The team id.</param>
        /// <param name="teamName">The team name.</param>
        /// <param name="whoAddedBotInChannel">The name of the user who added the bot in channel.</param>
        /// <param name="teamMembers">The team members.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task WelcomeTeamMembersAsync(
            string serviceUrl,
            string tenantId,
            string teamId,
            string teamName,
            string whoAddedBotInChannel,
            IEnumerable<ChannelAccount> teamMembers)
        {
            var customConnectorClient = this.botConnectorClientFactory.Create(serviceUrl);
            var botId = this.configurationSettings.MicrosoftAppId;
            foreach (var teamMember in teamMembers)
            {
                var userAadObjectId = this.GetUserAadObjectId(teamMember);

                var personalConversationId = await customConnectorClient.GetConversationIdAsync(
                    botId,
                    tenantId,
                    teamMember.Id);

                await this.SendWelcomeCardToTeamMemberAsync(
                    serviceUrl,
                    personalConversationId,
                    userAadObjectId,
                    whoAddedBotInChannel,
                    teamId,
                    teamName);

                await this.SaveUserEntityInDBAsync(
                    serviceUrl,
                    tenantId,
                    userAadObjectId,
                    teamMember.Id,
                    teamMember.Name,
                    personalConversationId);

                await this.userTeamMembershipRepository.AddUserTeamMembershipAsync(teamMember.Id, teamId, userAadObjectId);
            }
        }

        private string GetUserAadObjectId(ChannelAccount channelAccount)
        {
            if (!string.IsNullOrWhiteSpace(channelAccount.AadObjectId))
            {
                return channelAccount.AadObjectId;
            }

            return channelAccount.Properties["objectId"].ToString();
        }

        // Send welcome card to the activity member via personal chat.
        private async Task SendWelcomeCardToTeamMemberAsync(
            string serviceUrl,
            string personalConversationId,
            string userAadObjectId,
            string whoAddedBotInChannel,
            string teamId,
            string teamName)
        {
            Activity activity;

            var eventEntitiesOwnedByActivityMember = await this.eventRepository.GetAllEventsAsync(userAadObjectId);
            if (eventEntitiesOwnedByActivityMember == null || eventEntitiesOwnedByActivityMember.Count() == 0)
            {
                activity = this.BuildActivityWithWelcomeCard(
                    serviceUrl,
                    personalConversationId,
                    whoAddedBotInChannel,
                    teamName);
            }
            else
            {
                activity = this.BuildActivityWithWelcomeAndEventShareCard(
                    serviceUrl,
                    personalConversationId,
                    whoAddedBotInChannel,
                    teamId,
                    teamName,
                    userAadObjectId);
            }

            await this.sendToConversationQueue.SendActivityAsync(activity);
        }

        private Activity BuildActivityWithWelcomeCard(
            string serviceUrl,
            string conversationId,
            string whoAddedBotInChannel,
            string teamName)
        {
            var attachment = this.welcomeTeamMembersCardRenderer.BuildAttachment(whoAddedBotInChannel, teamName);
            var activity = this.botActivityBuilder.CreateActivity(serviceUrl, conversationId);
            activity.Attachments.Add(attachment);
            return activity;
        }

        private Activity BuildActivityWithWelcomeAndEventShareCard(
            string serviceUrl,
            string conversationId,
            string whoAddedBotInChannel,
            string teamId,
            string teamName,
            string personAadObjectId)
        {
            var attachment1 = this.welcomeTeamMembersCardRenderer.BuildAttachment(whoAddedBotInChannel, teamName);
            var attachment2 = this.shareEventCardRenderer.BuildAttachment(teamId, teamName, personAadObjectId);
            var activity = this.botActivityBuilder.CreateActivity(serviceUrl, conversationId);
            activity.Attachments.Add(attachment1);
            activity.Attachments.Add(attachment2);
            return activity;
        }

        private async Task SaveUserEntityInDBAsync(
            string serviceUrl,
            string tenantId,
            string userAadObjectId,
            string userId,
            string userName,
            string personalConversationId)
        {
            var userEntity = new UserEntity
            {
                PartitionKey = PartitionKeyNames.UserDataTable.UserDataPartition,
                RowKey = userAadObjectId,
                ServiceUrl = serviceUrl,
                TenantId = tenantId,
                AadId = userAadObjectId,
                UserId = userId,
                Name = userName,
                ConversationId = personalConversationId,
            };

            await this.userRepository.CreateOrUpdateAsync(userEntity);
        }
    }
}
