////////////////////////////////////////////////////////////////////////////////
// The MIT License (MIT)
//
// Copyright (c) 2024 Tim Stair
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
using System.Linq;
using Google;

namespace Support.Google
{
    public static class GoogleApi
    {
        private const string ERROR_LOCATION_AUTHORIZATION = "Authorization";

        public static bool IsAuthorizationError(GoogleApiException zException)
        {
            // REFERENCE: https://developers.google.com/doubleclick-advertisers/core_errors#UNAUTHORIZED
            if (zException.Error != null &&
                zException.Error.Errors.Count > 0)
            {
                var zError = zException.Error.Errors.FirstOrDefault(error =>
                    ERROR_LOCATION_AUTHORIZATION.Equals(error.Location, StringComparison.InvariantCultureIgnoreCase));
                return zError != null;
            }

            return false;
        }
    }
}