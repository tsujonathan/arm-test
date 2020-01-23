// <copyright file="UserRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.Repositories.User
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.Celebration.Common.Core;

    /// <summary>
    /// Repository of the user data stored in the table storage.
    /// </summary>
    public class UserRepository : BaseRepository<UserEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserRepository"/> class.
        /// </summary>
        /// <param name="configurationSettings">Configuration settings.</param>
        public UserRepository(ConfigurationSettings configurationSettings)
            : base(
                configurationSettings,
                PartitionKeyNames.UserDataTable.TableName,
                PartitionKeyNames.UserDataTable.UserDataPartition,
                false)
        {
        }

        /// <summary>
        /// Add personal data in Table Storage.
        /// </summary>
        /// <param name="activity">Bot conversation update activity instance.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task CreateUserDataAsync(IConversationUpdateActivity activity)
        {
            var userDataEntity = this.ParseUserData(activity);
            await this.CreateOrUpdateAsync(userDataEntity);
        }

        /// <summary>
        /// Remove personal data in table storage.
        /// </summary>
        /// <param name="activity">Bot conversation update activity instance.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task RemoveUserDataAsync(
            IConversationUpdateActivity activity)
        {
            var userDataEntity = this.ParseUserData(activity);
            if (userDataEntity != null)
            {
                var found = await this.GetAsync(PartitionKeyNames.UserDataTable.UserDataPartition, userDataEntity.UserId);
                if (found != null)
                {
                    await this.DeleteAsync(found);
                }
            }
        }

        private UserEntity ParseUserData(IConversationUpdateActivity activity)
        {
            var rowKey = activity?.From?.AadObjectId;
            if (rowKey != null)
            {
                var userDataEntity = new UserEntity
                {
                    PartitionKey = PartitionKeyNames.UserDataTable.UserDataPartition,
                    RowKey = activity?.From?.AadObjectId,
                    AadId = activity?.From?.AadObjectId,
                    UserId = activity?.From?.Id,
                    ConversationId = activity?.Conversation?.Id,
                    ServiceUrl = activity?.ServiceUrl,
                    TenantId = activity?.Conversation?.TenantId,
                };

                return userDataEntity;
            }

            return null;
        }
    }
}
