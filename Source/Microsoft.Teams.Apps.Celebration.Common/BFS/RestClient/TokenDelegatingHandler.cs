// <copyright file="TokenDelegatingHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Common.BFS.RestClient
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Identity.Client;

    /// <summary>
    /// This class is a custom DelegatingHandler. Here is a link explains DelegatingHandler:
    /// https://docs.microsoft.com/en-us/aspnet/web-api/overview/advanced/httpclient-message-handlers
    ///
    /// The TokenDelegatingHandler is designed to perform the following two actions:
    /// 1. Before calling Graph API
    ///   1). Retrieves token from https://login.microsoftonline.com
    ///   2). Attach the token as a bearer authorization header in each call to Graph API.
    /// 2. When token expiration happens, it refresh the token and re-try sending the request once.
    /// </summary>
    public class TokenDelegatingHandler : DelegatingHandler
    {
        private readonly IConfidentialClientApplication confidentialClientApplication;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenDelegatingHandler"/> class.
        /// </summary>
        /// <param name="confidentialClientApplication">ConfidentialClientApplication instance.</param>
        public TokenDelegatingHandler(IConfidentialClientApplication confidentialClientApplication)
        {
            this.confidentialClientApplication = confidentialClientApplication;
        }

        /// <summary>
        /// This methods acquires token form https://login.microsoftonline.com.
        /// </summary>
        /// <returns>The access token.</returns>
        public async Task<string> AcquireTokenAsync()
        {
            const string Scope = "https://api.botframework.com/.default";

            // Acquire tokens for Graph API
            var scopes = new[] { Scope };
            var authenticationResult = await this.confidentialClientApplication
                .AcquireTokenForClient(scopes)
                .ExecuteAsync();

            return authenticationResult.AccessToken;
        }

        /// <summary>
        /// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
        /// </summary>
        /// <param name="request">HttpRequestMessage instance.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>HttpResponseMessage instance.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            const string AuthorizationHeader = "Authorization";

            return await this.RetryOnTokenExpirationAsync(async () =>
            {
                var token = await this.AcquireTokenAsync();

                // Clear the Authorization header in case of token expiration
                // and re-try to acquire a new token.
                if (request.Headers.Contains(AuthorizationHeader))
                {
                    request.Headers.Remove(AuthorizationHeader);
                }

                request.Headers.Add(AuthorizationHeader, $"Bearer {token}");

                var response = await base.SendAsync(request, default);

                return response;
            });
        }

        // Re-try sending request once on token expiration.
        private async Task<HttpResponseMessage> RetryOnTokenExpirationAsync(
            Func<Task<HttpResponseMessage>> sendAsync)
        {
            var response = await sendAsync();
            if (response.StatusCode != HttpStatusCode.Unauthorized)
            {
                return response;
            }

            response = await sendAsync();
            if (response.StatusCode != HttpStatusCode.Unauthorized)
            {
                return response;
            }

            throw new ApplicationException("Failed to acquire a valid token after a re-try!");
        }
    }
}