# Description

This document lightly details the history of the integration with Google.

In fairness to Google on any criticism: I do not know/understand what I am doing 50% of the time. In many cases I was desparate just to get things to
work to not block users from using the application. Documentation and guidance on Google + Desktop application integration is 
garbage at best.

## Round 1: Login via application

**Description:** Users would provide their username and password in the application. Easy!

**Notes**
* Terribly insecure. Desktop applications could do whatever they wanted with the credentials.

## Round 2: OAUTH2 - "you have permanent hosting now"

**Description:** Users would have to give permissions to an application involving a self-hosted landing page. 
Manual copy and paste of the access token was added here. Scopes to access user data were broad but displayed to the user.

**Notes**
* Better security
* Large burden for a simple desktop application. I had to add a permanent php page to be forever hosted along with going 
through all the Google API hoops to get things setup. I get to pay for hosting this auth flow for life. Without it users would not be able
to use the Google Sheets functionality. Fun! 

## Round 3: OAUTH2 - "you must appease us with a video"

**Description:** Google narrowed the allowed scopes for applications that do not go through a costly (10,000+ USD) certification. I had
to remove the ability to list user sheets names/ids. Now users have to manually copy and paste URLs. I also had to record 2-3 videos
showing how the application worked. Every submission resulted in some minor thing they needed to verify so a whole new video had to be
recorded.

**Notes**
* Better security, but shows how the offered scopes actually suck and should be way more customizable. I should be able to request a scope to
list sheet names only.
* My frustration with the effort required just to have read-only access to things is immense.

## Round 4: OAUTH2 - "never ever submit client_secret.json to source control but go ahead and embed it in the public accessible binary"

**Description:** For decades (centuries?) I have read repeatedly to never ever ever ever submit `client_secret.json` to source control. After digging into
documentation that is intended for Google Auth and Desktop Apps I found that the `client_secret` is in fact required for desktop applications to
authorize and get a token. The docs indicate the `client_secret` is optional (incorrect). Google very much needs to explicitly document and explain
to developers how and what the `client_secret` is for desktop applications and assure them that embedding it into a distributed binary is okay. 

So embedding is how CardMaker will do this (as of [v.1.6.0.0-unstable.v.a3](https://github.com/nhmkdev/cardmaker/releases/tag/v.1.6.0.0-unstable.v.a3)). 
If everything breaks I'll just kill the new client and continue to use the old mode.

Also `GoogleWebAuthorizationBroker` is magic and just does a bunch of work for me. 👍

**Notes**
* Uhhh better flow but embedding a client secret in a binary without explicit instructions is awful.


## Future Rounds
I am quite certain desktop application support (which is already something they barely support) will be cut.
