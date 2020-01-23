// <copyright file="CelebrationControllerBase.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Controllers
{
    using System;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Implements the shared functions used by the controllers of the Celebrations app.
    /// </summary>
    public class CelebrationControllerBase : ControllerBase
    {
        /// <summary>
        /// Gets the user tenant id from the HttpContext.
        /// </summary>
        protected string UserTenantId
        {
            get
            {
                var tenantClaimType = "http://schemas.microsoft.com/identity/claims/tenantid";
                var claim = this.User.Claims.FirstOrDefault(p => tenantClaimType.Equals(p.Type, StringComparison.OrdinalIgnoreCase));
                if (claim == null)
                {
                    return null;
                }

                return claim.Value;
            }
        }

        /// <summary>
        /// Gets the user AadId from the HttpContext.
        /// </summary>
        protected string UserAadId
        {
            get
            {
                var oidClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";
                var claim = this.User.Claims.FirstOrDefault(p => oidClaimType.Equals(p.Type, StringComparison.OrdinalIgnoreCase));
                if (claim == null)
                {
                    return null;
                }

                return claim.Value;
            }
        }

        /// <summary>
        /// Gets the user user name from the HttpContext.
        /// </summary>
        protected string UserName
        {
            get
            {
                var claim = this.User.Claims.FirstOrDefault(p => "name".Equals(p.Type, StringComparison.OrdinalIgnoreCase));
                if (claim == null)
                {
                    return null;
                }

                return claim.Value;
            }
        }
    }
}