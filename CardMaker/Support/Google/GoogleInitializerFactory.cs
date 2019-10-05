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

            var tokenX = new TokenResponse
            {
                AccessToken = AccessToken,
                ExpiresInSeconds = TokenExpirationSeconds,
                Issued = DateTime.UtcNow
            };

            // TODO: token validation to get timeout
            //                VerifyAccessToken(tokenX.AccessToken);

            var credential = new UserCredential(zAuthCodeFlow, Environment.UserName, tokenX);

            return new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = m_sApplicationName
            };
        }
    }
}