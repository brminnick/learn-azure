using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Yammerly.Models;

using Xamarin.Forms;
using Yammerly.Helpers;
using Yammerly.Services;
using Plugin.Media.Abstractions;
using Plugin.Media;

namespace Yammerly.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private string name;
        private string position;
        private string photoUrl;

        public string Name
        {
            get { return name; }
            set { name = value;  OnPropertyChanged("Name"); }
        }

        public string Position
        {
            get { return position; }
            set { position = value;  OnPropertyChanged("Position"); }
        }

        public string PhotoUrl
        {
            get { return photoUrl; }
            set { photoUrl = value;  OnPropertyChanged("PhotoUrl"); }
        }

        ObservableCollection<TimelineItem> timelineItems;
        public ObservableCollection<TimelineItem> TimelineItems
        {
            get { return timelineItems; }
            set { timelineItems = value; OnPropertyChanged("TimelineItems"); }
        }

        public ProfileViewModel()
        {
            Title = "Profile";

            Name = $"{Settings.FirstName} {Settings.LastName}";
            Position = "Software Engineer";
            PhotoUrl = Settings.PhotoUrl;

            TimelineItems = new ObservableCollection<TimelineItem>();
            ExecuteRefreshCommandAsync();
        }

        Command refreshCommand;
        public Command RefreshCommand
        {
            get { return refreshCommand ?? (refreshCommand = new Command(async () => await ExecuteRefreshCommandAsync())); }
        }

        async Task ExecuteRefreshCommandAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                var client = DependencyService.Get<IDataService>() as AzureService;
                var table = client.MobileService.GetSyncTable<TimelineItem>();
                var items = await table.Where((item) => item.Id == Settings.UserId).ToListAsync();
                TimelineItems.Clear();
                foreach (var item in items)
                {
                    TimelineItems.Add(item);
                }
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

        Command changeProfilePhotoCommand;
        public Command ChangeProfilePhotoCommand
        {
            get { return changeProfilePhotoCommand ?? (changeProfilePhotoCommand = new Command(async () => await ExecuteChangeProfilePhotoCommandAsync())); }
        }   

        async Task ExecuteChangeProfilePhotoCommandAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                await CrossMedia.Current.Initialize();

                MediaFile file;
                if (CrossMedia.Current.IsCameraAvailable)
                {
                    file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                    {
                        Directory = "Photos",
                        Name = "photo.jpg"
                    });
                }
                else
                {
                    file = await CrossMedia.Current.PickPhotoAsync();
                }

                var client = DependencyService.Get<IDataService>() as AzureService;
                var url = await client.StoreBlob(file.GetStream());
                Settings.PhotoUrl = url;
                PhotoUrl = url;

                var user = await client.GetItem<Employee>(Settings.UserId);
                user.PhotoUrl = url;
                await client.UpdateItem(user);
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
