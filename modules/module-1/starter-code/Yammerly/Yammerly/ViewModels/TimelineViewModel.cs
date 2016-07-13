using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Yammerly.Models;
using Yammerly.Services;

using Xamarin.Forms;

using Plugin.Media;
using Plugin.Media.Abstractions;
using Yammerly.Helpers;

namespace Yammerly.ViewModels
{
    public class TimelineViewModel : BaseViewModel
    {
        public TimelineViewModel()
        {
            Title = "Timeline";

            ExecuteRefreshCommandAsync();
        }

        ObservableCollection<TimelineItem> timelineItems;
        public ObservableCollection<TimelineItem> TimelineItems
        {
            get { return timelineItems; }
            set { timelineItems = value; OnPropertyChanged("TimelineItems"); }
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
                if (TimelineItems == null)
                    TimelineItems = new ObservableCollection<TimelineItem>();

                var items = await DependencyService.Get<IDataService>().GetItems<TimelineItem>();
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


        Command postTimelineItemCommand;
        public Command PostTimelineItemCommand
        {
            get { return postTimelineItemCommand ?? (postTimelineItemCommand = new Command(async () => await ExecutePostTimelineItemCommandAsync())); }
        }

        async Task ExecutePostTimelineItemCommandAsync()
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
                        Directory = "TimelineItems",
                        Name = "photo.jpg"
                    });
                }
                else
                {
                    file = await CrossMedia.Current.PickPhotoAsync();
                }

                var client = DependencyService.Get<IDataService>() as AzureService;
                var author = await client.GetItem<Employee>(Settings.UserId);
                var url = await client.StoreBlob(file.GetStream());
                var text = "Having a blast at this Azure workshop!";

                var timelineItem = new TimelineItem
                {
                    Author = author,
                    Text = text,
                    PhotoUrl = url
                };

                TimelineItems.Add(timelineItem);
                await client.AddItem<TimelineItem>(timelineItem);
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
