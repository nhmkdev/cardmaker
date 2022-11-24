////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2022 Tim Stair
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
////////////////////////////////////////////////////////////////////////////////

using System;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;

namespace Support.Google
{
    /// <summary>
    /// Desktop application specific wrapper for building Google service initializers. 
    /// </summary>
    public class GoogleInitializerFactory
    {
        private const int DEFAULT_TOKEN_EXPIRATION_SECONDS = 3600;
        private string m_sApplicationName;
        private string m_sClientId;
        private string[] m_arrayScopes;

        public string AccessToken { get; set; }
        public int TokenExpirationSeconds { get; set; }

        public GoogleInitializerFactory(string sApplicationName, string sClientId, string[] arrayScopes)
        {
            m_sApplicationName = sApplicationName;
            m_sClientId = sClientId;
            m_arrayScopes = arrayScopes;
            TokenExpirationSeconds = DEFAULT_TOKEN_EXPIRATION_SECONDS;
        }

        /**
         * Creates a simple initializer with the user provided access token for CardMaker (from the cardmaker site)
         */
        public BaseClientService.Initializer CreateInitializer()
        {
            var zAuthCodeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = m_sClientId
                },
                Scopes = m_arrayScopes
            });

            var zTokenResponse = new TokenResponse
            {
                AccessToken = AccessToken,
                ExpiresInSeconds = TokenExpirationSeconds,
                IssuedUtc = DateTime.UtcNow
            };

            var zUserCredential = new UserCredential(zAuthCodeFlow, Environment.UserName, zTokenResponse);

            return new BaseClientService.Initializer()
            {
                HttpClientInitializer = zUserCredential,
                ApplicationName = m_sApplicationName
            };
        }
    }
}