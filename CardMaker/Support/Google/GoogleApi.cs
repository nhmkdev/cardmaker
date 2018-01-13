////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2018 Tim Stair
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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Support.IO;

namespace Support.UI
{
    public static class GoogleApi
    {
        public const string OAUTH2_TOKEN_INFO_URL = "https://www.googleapis.com/oauth2/v1/tokeninfo";
        public const string ACCESS_TYPE = "access_type";
        public const string EXPIRES_IN = "expires_in";

        public static bool VerifyAccessToken(string sAccessToken)
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