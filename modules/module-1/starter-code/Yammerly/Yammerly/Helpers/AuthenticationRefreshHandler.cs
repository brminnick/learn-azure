using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Yammerly.Services;

using Microsoft.WindowsAzure.MobileServices;
using Xamarin.Forms;
using Newtonsoft.Json.Linq;

namespace Yammerly.Helpers
{
    public class AuthenticationRefreshHandler : DelegatingHandler
    {
        public MobileServiceClient Client { get; set; }

        SemaphoreSlim semaphore = new SemaphoreSlim(1);
        bool isReauthenticating = false;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Clone the request
            var clonedRequest = await CloneRequest(request);
            var response = await base.SendAsync(clonedRequest, cancellationToken);

            // If the token expired or is invalid, we need to refresh the token
            // or prompt the user to log back in.
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (isReauthenticating)
                    return response;

                if (Client == null)
                {
                    throw new InvalidOperationException("Make sure the MobileServiceClient has been set for the DelegatingHandler.");
                }

                var authenticationToken = Client.CurrentUser.MobileServiceAuthenticationToken;

                // In the event of two threads entering this method at the same time, only one should
                // do the refresh, or re-login.
                await semaphore.WaitAsync();

                // Token was already renewed by another concurrent thread
                if (authenticationToken != Client.CurrentUser.MobileServiceAuthenticationToken)
                {
                    semaphore.Release();

                    return await ResendRequest(request, cancellationToken);
                }

                // Refresh our authentication token
                isReauthenticating = true;
                bool didReceiveNewToken = false;

                try
                {
                    didReceiveNewToken = await RefreshToken();

                    // Token refresh failed, we need to re-login.
                    if (!didReceiveNewToken)
                    {
                        didReceiveNewToken = await DependencyService.Get<IAuthenticationService>().LoginAsync(Client, MobileServiceAuthenticationProvider.WindowsAzureActiveDirectory);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error Refreshing Auth: {ex.Message}");
                }
                finally
                {
                    isReauthenticating = false;
                    semaphore.Release();
                }

                if (didReceiveNewToken)
                {
                    if (!request.RequestUri.OriginalString.Contains("/.auth/me"))
                    {
                        return await ResendRequest(request, cancellationToken);
                    }
                }
            }

            return response;
        }

        async Task<HttpResponseMessage> ResendRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Clone the request
            var clonedRequest = await CloneRequest(request);

            // Set the authentication header
            clonedRequest.Headers.Remove("X-ZUMO-AUTH");
            clonedRequest.Headers.Add("X-ZUMO-AUTH", Client.CurrentUser.MobileServiceAuthenticationToken);

            // Resend the request
            return await base.SendAsync(clonedRequest, cancellationToken);
        }

        async Task<bool> RefreshToken()
        {
            try
            {
                var refreshJson = (JObject)await Client.InvokeApiAsync("/.auth/refresh", HttpMethod.Get, null);

                if (refreshJson != null)
                {
                    var newToken = refreshJson["authenticationToken"].Value<string>();
                    Client.CurrentUser.MobileServiceAuthenticationToken = newToken;
                    Settings.AuthToken = newToken;

                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error Refreshing Token: {ex.Message}");
            }

            return false;
        }

        private async Task<HttpRequestMessage> CloneRequest(HttpRequestMessage request)
        {
            var result = new HttpRequestMessage(request.Method, request.RequestUri);
            foreach (var header in request.Headers)
            {
                result.Headers.Add(header.Key, header.Value);
            }

            if (request.Content != null && request.Content.Headers.ContentType != null)
            {
                var requestBody = await request.Content.ReadAsStringAsync();
                var mediaType = request.Content.Headers.ContentType.MediaType;
                result.Content = new StringContent(requestBody, Encoding.UTF8, mediaType);
                foreach (var header in request.Content.Headers)
                {
                    if (!header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                    {
                        result.Content.Headers.Add(header.Key, header.Value);
                    }
                }
            }

            return result;
        }
    }
}