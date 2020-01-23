// <copyright file="UserValidator.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Authentication
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.Celebration.Common.Repositories.UserTeamMembership;
    using Microsoft.Teams.Apps.Celebration.Common.Validations;

    /// <summary>
    /// Validates if the following two conditions are met:
    /// 1). If a user belongs to redefined tenants.
    /// 2). If the user is a member of a certain MS Teams team.
    /// </summary>
    public class UserValidator
    {
        private readonly UserTeamMembershipRepository userTeamMembershipRepository;
        private readonly TenantValidator tenantValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserValidator"/> class.
        /// </summary>
        /// <param name="userTeamMembershipRepository">The user team membership repository.</param>
        /// <param name="tenantValidator">The tenant validator service.</param>
        public UserValidator(
            UserTeamMembershipRepository userTeamMembershipRepository,
            TenantValidator tenantValidator)
        {
            this.userTeamMembershipRepository = userTeamMembershipRepository;
            this.tenantValidator = tenantValidator;
        }

        /// <summary>
        /// Checks if a user meets the two conditions, with valid tenant id, and is a member of a certain team.
        /// </summary>
        /// <param name="tenantId">The user's tenant id.</param>
        /// <param name="teamId">The team id that the validator checks against the user. To see if the user is a member of the team. </param>
        /// <param name="userAadObjectId">The user's AadObjectId.</param>
        /// <returns>The flag indicates whether the user is meeting the conditions.</returns>
        public async Task<bool> ValidateAsync(string tenantId, string teamId, string userAadObjectId)
        {
            if (!this.tenantValidator.Validate(tenantId))
            {
                return false;
            }

            var userTeamMembershipEntities =
                await this.userTeamMembershipRepository.GetUserTeamMembershipByUserAadObjectIdAsync(teamId, userAadObjectId);
            return userTeamMembershipEntities != null && userTeamMembershipEntities.Count() > 0;
        }
    }
}
