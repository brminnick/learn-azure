using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Yammerly.Helpers;
using Yammerly.Services;

using Microsoft.WindowsAzure.MobileServices;
using Xamarin.Forms;
using System.Threading.Tasks;
using UIKit;
using Yammerly.Models;
using Yammerly.Views;

[assembly: Xamarin.Forms.Dependency(typeof(Yammerly.iOS.Helpers.Authentication))]
namespace Yammerly.iOS.Helpers
{
    public class Authentication : IAuthenticationService
    {
        public async Task<bool> LoginAsync(IMobileServiceClient client, MobileServiceAuthenticationProvider provider)
        {
            var success = false;

            try
            {
                var user = await client.LoginAsync(UIApplication.SharedApplication.KeyWindow.RootViewController, provider);

                if (user != null)
                {
                    Settings.AuthToken = user.MobileServiceAuthenticationToken;

                    var employee = await client.InvokeApiAsync<Employee>("UserInfo", System.Net.Http.HttpMethod.Get, null);
                    Settings.FirstName = employee.FirstName;
                    Settings.LastName = employee.LastName;
                    Settings.PhotoUrl = employee.PhotoUrl;
                    Settings.UserId = employee.Id;

                    App.Current.MainPage = new RootPage();

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging in: {ex.Message}");
            }

            return success;
        }
    }
}