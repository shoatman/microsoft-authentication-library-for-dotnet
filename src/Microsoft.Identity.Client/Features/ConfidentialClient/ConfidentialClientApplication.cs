﻿//------------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//------------------------------------------------------------------------------

using Microsoft.Identity.Client.Internal.Instance;
using Microsoft.Identity.Client.Internal.Requests;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Identity.Client
{
    /// <summary>
    /// Class to be used for confidential client applications like Web Apps/API.
    /// </summary>
    public sealed class ConfidentialClientApplication : ClientApplicationBase, IConfidentialClientApplication
    {
        /// <summary>
        /// Constructor to create instance of the class
        /// </summary>
        /// <param name="clientId">Client Id of the application. REQUIRED.</param>
        /// <param name="redirectUri">Redirect URI of the application. REQUIRED.</param>
        /// <param name="clientCredential">Client credential for the application. Could be a certificate or a secret. REQUIRED.</param>
        /// <param name="userTokenCache">Token cache for saving user tokens. OPTIONAL.</param>
        /// <param name="appTokenCache">Token cache for saving application/client tokens. OPTIONAL.</param>
        public ConfidentialClientApplication(string clientId, string redirectUri,
            ClientCredential clientCredential, TokenCache userTokenCache, TokenCache appTokenCache)
            : this(clientId, DefaultAuthority, redirectUri, clientCredential, userTokenCache, appTokenCache)
        {
        }

        /// <summary>
        /// Constructor to create instance of the class
        /// </summary>
        /// <param name="clientId">Client Id of the application. REQUIRED.</param>
        /// <param name="authority">Authority to be used for the client application. REQUIRED.</param>
        /// <param name="redirectUri">Redirect URI of the application. REQUIRED.</param>
        /// <param name="clientCredential">Client credential for the application. Could be a certificate or a secret. REQUIRED.</param>
        /// <param name="userTokenCache">Token cache for saving user tokens. OPTIONAL.</param>
        /// <param name="appTokenCache">Token cache for saving application/client tokens. OPTIONAL.</param>
        public ConfidentialClientApplication(string clientId, string authority, string redirectUri,
            ClientCredential clientCredential, TokenCache userTokenCache, TokenCache appTokenCache)
            : base(authority, clientId, redirectUri, true)
        {
            ClientCredential = clientCredential;
            UserTokenCache = userTokenCache;
            AppTokenCache = appTokenCache;
            if (AppTokenCache != null)
            {
                AppTokenCache.ClientId = clientId;
            }
        }

        /// <summary>
        /// Acquires token using On-Behalf-Of flow.
        /// </summary>
        /// <param name="scope">Array of scopes requested for resource</param>
        /// <param name="userAssertion">Instance of UserAssertion containing user's token.</param>
        /// <returns>Authentication result containing token of the user for the requested scopes</returns>
        public async Task<AuthenticationResult> AcquireTokenOnBehalfOfAsync(IEnumerable<string> scope, UserAssertion userAssertion)
        {
            Authority authority = Internal.Instance.Authority.CreateAuthority(Authority, ValidateAuthority);
            return
                await
                    AcquireTokenOnBehalfCommonAsync(authority, scope, userAssertion, ApiEvent.ApiIds.AcquireTokenOnBehalfOfWithScopeUser)
                        .ConfigureAwait(false);
        }

        /// <summary>
        /// Acquires token using On-Behalf-Of flow.
        /// </summary>
        /// <param name="scope">Array of scopes requested for resource</param>
        /// <param name="userAssertion">Instance of UserAssertion containing user's token.</param>
        /// <param name="authority">Specific authority for which the token is requested. Passing a different value than configured does not change the configured value</param>
        /// <returns>Authentication result containing token of the user for the requested scopes</returns>
        public async Task<AuthenticationResult> AcquireTokenOnBehalfOfAsync(IEnumerable<string> scope, UserAssertion userAssertion,
            string authority)
        {
            Authority authorityInstance = Internal.Instance.Authority.CreateAuthority(authority, ValidateAuthority);
            return
                await
                    AcquireTokenOnBehalfCommonAsync(authorityInstance, scope, userAssertion, ApiEvent.ApiIds.AcquireTokenOnBehalfOfWithScopeUserAuthority)
                        .ConfigureAwait(false);
        }

        /// <summary>
        /// Acquires security token from the authority using authorization code previously received.
        /// This method does not lookup token cache, but stores the result in it, so it can be looked up using other methods such as <see cref="IClientApplicationBase.AcquireTokenSilentAsync(IEnumerable{string}, IUser)"/>.
        /// </summary>
        /// <param name="authorizationCode">The authorization code received from service authorization endpoint.</param>
        /// <param name="scope">Array of scopes requested for resource</param>
        /// <returns>Authentication result containing token of the user for the requested scopes</returns>
        public async Task<AuthenticationResult> AcquireTokenByAuthorizationCodeAsync(string authorizationCode, IEnumerable<string> scope)
        {
            return
                await
                    AcquireTokenByAuthorizationCodeCommonAsync(authorizationCode, scope, new Uri(RedirectUri)).ConfigureAwait(false);
        }

        /// <summary>
        /// Acquires token from the service for the confidential client. This method attempts to look up valid access token in the cache.
        /// </summary>
        /// <param name="scope">Array of scopes requested for resource</param>
        /// <returns>Authentication result containing application token for the requested scopes</returns>
        public async Task<AuthenticationResult> AcquireTokenForClientAsync(IEnumerable<string> scope)
        {
            return
                await
                    AcquireTokenForClientCommonAsync(scope, false).ConfigureAwait(false);
        }

        /// <summary>
        /// Acquires token from the service for the confidential client. This method attempts to look up valid access token in the cache.
        /// </summary>
        /// <param name="scope">Array of scopes requested for resource</param>
        /// <param name="forceRefresh">If TRUE, API will ignore the access token in the cache and attempt to acquire new access token using client credentials</param>
        /// <returns>Authentication result containing application token for the requested scopes</returns>
        public async Task<AuthenticationResult> AcquireTokenForClientAsync(IEnumerable<string> scope, bool forceRefresh)
        {
            return
                await
                    AcquireTokenForClientCommonAsync(scope, forceRefresh).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets URL of the authorize endpoint including the query parameters.
        /// </summary>
        /// <param name="scope">Array of scopes requested for resource</param>
        /// <param name="loginHint">Identifier of the user. Generally a UPN.</param>
        /// <param name="extraQueryParameters">This parameter will be appended as is to the query string in the HTTP authentication request to the authority. The parameter can be null.</param>
        /// <returns>URL of the authorize endpoint including the query parameters.</returns>
        public async Task<Uri> GetAuthorizationRequestUrlAsync(IEnumerable<string> scope, string loginHint,
            string extraQueryParameters)
        {
            Authority authority = Internal.Instance.Authority.CreateAuthority(Authority, ValidateAuthority);
            var requestParameters =
                CreateRequestParameters(authority, scope, null, UserTokenCache);
            requestParameters.ClientId = ClientId;
            requestParameters.ExtraQueryParameters = extraQueryParameters;

            var handler =
                new InteractiveRequest(requestParameters, null, loginHint, UIBehavior.SelectAccount, null);
            return await handler.CreateAuthorizationUriAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Gets URL of the authorize endpoint including the query parameters.
        /// </summary>
        /// <param name="scope">Array of scopes requested for resource</param>
        /// <param name="redirectUri">Address to return to upon receiving a response from the authority.</param>
        /// <param name="loginHint">Identifier of the user. Generally a UPN.</param>
        /// <param name="extraQueryParameters">This parameter will be appended as is to the query string in the HTTP authentication request to the authority. The parameter can be null.</param>
        /// <param name="additionalScope">Array of scopes for which a developer can request consent upfront.</param>
        /// <param name="authority">Specific authority for which the token is requested. Passing a different value than configured does not change the configured value</param>
        /// <returns>URL of the authorize endpoint including the query parameters.</returns>
        public async Task<Uri> GetAuthorizationRequestUrlAsync(IEnumerable<string> scope, string redirectUri, string loginHint,
            string extraQueryParameters, IEnumerable<string> additionalScope, string authority)
        {
            Authority authorityInstance = Internal.Instance.Authority.CreateAuthority(authority, ValidateAuthority);
            var requestParameters = CreateRequestParameters(authorityInstance, scope, null,
                UserTokenCache);
            requestParameters.RedirectUri = new Uri(redirectUri);
            requestParameters.ClientId = ClientId;
            requestParameters.ExtraQueryParameters = extraQueryParameters;

            var handler =
                new InteractiveRequest(requestParameters, additionalScope, loginHint, UIBehavior.SelectAccount, null);
            return await handler.CreateAuthorizationUriAsync().ConfigureAwait(false);
        }

        internal ClientCredential ClientCredential { get; }

        internal TokenCache AppTokenCache { get; }

        private async Task<AuthenticationResult> AcquireTokenForClientCommonAsync(IEnumerable<string> scope, bool forceRefresh)
        {
            Authority authority = Internal.Instance.Authority.CreateAuthority(Authority, ValidateAuthority);
            AuthenticationRequestParameters parameters = CreateRequestParameters(authority, scope, null,
                AppTokenCache);
            parameters.IsClientCredentialRequest = true;
            var handler = new Internal.Requests.ClientCredentialRequest(parameters, forceRefresh);
            return await handler.RunAsync().ConfigureAwait(false);
        }

        private async Task<AuthenticationResult> AcquireTokenOnBehalfCommonAsync(Authority authority,
            IEnumerable<string> scope, UserAssertion userAssertion, ApiEvent.ApiIds apiId)
        {
            var requestParams = CreateRequestParameters(authority, scope, null, UserTokenCache);
            requestParams.UserAssertion = userAssertion;
            var handler = new OnBehalfOfRequest(requestParams){ApiId = apiId};
            return await handler.RunAsync().ConfigureAwait(false);
        }

        private async Task<AuthenticationResult> AcquireTokenByAuthorizationCodeCommonAsync(string authorizationCode,
            IEnumerable<string> scope, Uri redirectUri)
        {
            Authority authority = Internal.Instance.Authority.CreateAuthority(Authority, ValidateAuthority);
            var requestParams = CreateRequestParameters(authority, scope, null, UserTokenCache);
            requestParams.AuthorizationCode = authorizationCode;
            requestParams.RedirectUri = redirectUri;
            var handler =
                new AuthorizationCodeRequest(requestParams);
            return await handler.RunAsync().ConfigureAwait(false);
        }

        internal override AuthenticationRequestParameters CreateRequestParameters(Authority authority, IEnumerable<string> scope, IUser user, TokenCache cache)
        {
            AuthenticationRequestParameters parameters = base.CreateRequestParameters(authority, scope, user, cache);
            parameters.ClientId = ClientId;
            parameters.ClientCredential = ClientCredential;

            return parameters;
        }
    }
}