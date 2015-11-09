using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace SensorCoreExport
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Lumia.Sense.TrackPointMonitor _tracker;

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            Window.Current.VisibilityChanged += async (oo, ee) =>
            {
                if (!ee.Visible)
                {
                    if (_tracker != null)
                    {
                        await _tracker.DeactivateAsync();
                    }
                }
                else
                {
                    if (_tracker == null)
                    {
                        _tracker = await Lumia.Sense.TrackPointMonitor.GetDefaultAsync();
                    }
                    else
                    {
                        await _tracker.ActivateAsync();
                    }
                }
            };

        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!await Lumia.Sense.TrackPointMonitor.IsSupportedAsync())
            {
                var dialog = new MessageDialog("Sensorcore Disabled");
                await dialog.ShowAsync();
                App.Current.Exit();
                return;
            }

            RegisterForShare();

            _tracker = await Lumia.Sense.TrackPointMonitor.GetDefaultAsync();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            UnregisterForShare();
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }

        private void RegisterForShare()
        {
            var dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += new TypedEventHandler<DataTransferManager,
                DataRequestedEventArgs>(this.ShareStorageItemsHandler);
        }

        private void UnregisterForShare()
        {
            var dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested -= new TypedEventHandler<DataTransferManager,
                DataRequestedEventArgs>(this.ShareStorageItemsHandler);
        }

        private async void ShareStorageItemsHandler(DataTransferManager sender,
            DataRequestedEventArgs e)
        {
            var defferal = e.Request.GetDeferral();

            e.Request.Data.Properties.Title = "Export Location Data";
            e.Request.Data.Properties.Description = "Choose Location to save the data";
            e.Request.Data.SetStorageItems(new[] { await SaveLocationData() });

            defferal.Complete();
        }

        private async Task<IStorageItem> SaveLocationData()
        {
            try
            {
                ItemList.Items.Clear();

                var from = DatePickerFrom.Date.Date;
                var to = DatePickerTo.Date.Date.AddDays(1);
                var length = to - from;

                var points = await _tracker.GetTrackPointsAsync(from, length);

                var orderedPoints = points.OrderBy(p => p.Timestamp);
                ItemList.Items.Add($"From {from} to {to}. Length is {length}");
                ItemList.Items.Add($"First item: {orderedPoints.First().Timestamp}");
                ItemList.Items.Add($"Last item: {orderedPoints.Last().Timestamp}");

                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("trackpoints.gpx", CreationCollisionOption.ReplaceExisting);
                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    new Gpx.SerializeToGpx().Serialize(orderedPoints, stream);
                }

                return file;
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog(ex.Message);
                await dialog.ShowAsync();
                throw;
            }
        }
    }
}
