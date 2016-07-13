using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Yammerly.Helpers;
using Yammerly.Models;
using Yammerly.Services;
using Yammerly.Views;

[assembly: Xamarin.Forms.Dependency(typeof(Yammerly.UWP.Helpers.Authentication))]
namespace Yammerly.UWP.Helpers
{
    public class Authentication : IAuthenticationService
    {
        public async Task<bool> LoginAsync(IMobileServiceClient client, MobileServiceAuthenticationProvider provider)
        {
            var success = false;

            try
            {
                var user = await client.LoginAsync(provider);

                if (user != null)
                {
                    Settings.AuthToken = user.MobileServiceAuthenticationToken;

                    var employee = await client.InvokeApiAsync<Employee>("UserInfo", System.Net.Http.HttpMethod.Get, null);
                    Settings.FirstName = employee.FirstName;
                    Settings.LastName = employee.LastName;
                    Settings.PhotoUrl = employee.PhotoUrl;
                    Settings.UserId = employee.Id;

                    Xamarin.Forms.Application.Current.MainPage = new RootPage();

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error logging in: {ex.Message}");
            }

            return success;
        }
    }
}