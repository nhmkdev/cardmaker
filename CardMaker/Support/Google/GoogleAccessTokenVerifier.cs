using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Support.IO;

namespace Support.Google
{
    public class GoogleAccessTokenVerifier
    {
        private readonly string OAUTH2_TOKEN_INFO_URL;
        public const string ACCESS_TYPE = "access_type";
        public const string EXPIRES_IN = "expires_in";

        private GoogleAccessTokenVerifier() { }

        public GoogleAccessTokenVerifier(string sTokenInfoURL)
        {
            OAUTH2_TOKEN_INFO_URL = sTokenInfoURL;
        }

        public bool VerifyAccessToken(string sAccessToken)
        {
            var zRequest = WebRequest.Create(OAUTH2_TOKEN_INFO_URL + "?access_token=" + sAccessToken);
            try
            {
                var zResponse = zRequest.GetResponse();
                var sJson = new StreamReader(zResponse.GetResponseStream(), Encoding.UTF8).ReadToEnd();
                var zObject = SimpleJson.SimpleJson.DeserializeObject(sJson);
                var root = (IDictionary<string, object>)zObject;
                if (root.ContainsKey(EXPIRES_IN))
                {
                    int nExpiresInSeconds;
                    if (int.TryParse(root[EXPIRES_IN].ToString(), out nExpiresInSeconds))
                    {
                        var dtExpiry = DateTime.Now.AddSeconds(nExpiresInSeconds);
                        Logger.AddLogLine($"Token expires in {nExpiresInSeconds} seconds ({dtExpiry.ToString()})");
                    }
                    else
                    {
                        nExpiresInSeconds = -1;
                    }
                    if (0 > nExpiresInSeconds)
                    {
                        Logger.AddLogLine("Authorization Token has expired or is invalid.");
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.AddLogLine(ex.ToString());
                Logger.AddLogLine("Google credentials appear to be invalid or expired. Please update them.");
            }
            return false;
        }
    }
}
