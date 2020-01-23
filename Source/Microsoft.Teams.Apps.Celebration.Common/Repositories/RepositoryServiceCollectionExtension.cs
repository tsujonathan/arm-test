// <copyright file="RepositoryServiceCollectionExtension.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.Repositories
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.Event;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.Occurrence;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.Team;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.User;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.UserTeamMembership;

    /// <summary>
    /// Extension class for registering repository services in DI container.
    /// </summary>
    public static class RepositoryServiceCollectionExtension
    {
        /// <summary>
        /// Extension method for registering repository services in DI container.
        /// </summary>
        /// <param name="serviceCollection">IServiceCollection instance.</param>
        public static void AddCelebrationsRepositories(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<EventFilterCreator>()
                .AddSingleton<UserAadIdFilter>();

            serviceCollection
                .AddSingleton<EventRepository>()
                .AddSingleton<OccurrenceRepository>()
                .AddSingleton<UserRepository>()
                .AddSingleton<TeamRepository>()
                .AddSingleton<TableRowKeyGenerator>()
                .AddSingleton<UserTeamMembershipRepository>();
        }
    }
}
