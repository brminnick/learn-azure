using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Yammerly.Helpers;
using Yammerly.Views;

using Xamarin.Forms;
using Yammerly.Services;
using Microsoft.WindowsAzure.MobileServices;
using Yammerly.Models;

namespace Yammerly.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        Command loginCommand;
        public Command LoginCommand
        {
            get { return loginCommand ?? (loginCommand = new Command(async () => await ExecuteLoginCommandAsync())); }
        }

        async Task ExecuteLoginCommandAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                var client = DependencyService.Get<IDataService>() as AzureService;
                await DependencyService.Get<IAuthenticationService>().LoginAsync(client.MobileService, MobileServiceAuthenticationProvider.WindowsAzureActiveDirectory);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
