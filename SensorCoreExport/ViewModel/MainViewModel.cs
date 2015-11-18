using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using SensorCoreExport.Infrastructure;
using SensorCoreExport.Services;

namespace SensorCoreExport.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly RouteExporter _routeExporter;

        public MainViewModel(INavigationService navigationService, RouteExporter exporter)
        {
            _navigationService = navigationService;
            _routeExporter = exporter;

            Export = new DependentRelayCommand(DataTransferManager.ShowShareUI, () => IsRouteTrackingEnabled && ExportRoutes, this, nameof(IsRouteTrackingEnabled), nameof(ExportRoutes));
            ActivateTracker = new RelayCommand(OnScreenVisible);
            DeactivateTracker = new RelayCommand(OnScreenHidden);
            ShareRequested = new RelayCommand<DataRequest>(OnShareRequested);
            Settings = new RelayCommand(()=> _navigationService.NavigateTo("Settings"));

            From = DateTime.Today;
            Until = DateTime.Today;

            IsRouteTrackingEnabled = false;
            ExportRoutes = false;
        }

        public RelayCommand Export { get; private set; }
        public ICommand ActivateTracker { get; private set; }
        public ICommand DeactivateTracker { get; private set; }
        public RelayCommand<DataRequest> ShareRequested { get; private set; }
        public ICommand Settings { get; private set; }
        
        private DateTimeOffset _from;
        public DateTimeOffset From
        {
            get { return _from; }
            set
            {
                _from = value;
                RaisePropertyChanged();
            }
        }

        private DateTimeOffset _until;
        public DateTimeOffset Until
        {
            get { return _until; }
            set
            {
                _until = value;
                RaisePropertyChanged();
            }
        }

        private bool _exportRoutes;
        public bool ExportRoutes
        {
            get { return _exportRoutes; }
            set
            {
                _exportRoutes = value;
                RaisePropertyChanged();
            }
        }

        private bool _isRouteTrackingEnabled;
        public bool IsRouteTrackingEnabled
        {
            get { return _isRouteTrackingEnabled; }
            set
            {
                _isRouteTrackingEnabled = value;
                RaisePropertyChanged();
            }
        }
        //TODO: this can be better
        public async Task Initialize()
        {   
            await _routeExporter.Initialize();

            IsRouteTrackingEnabled = _routeExporter.IsEnabled;
            ExportRoutes = IsRouteTrackingEnabled;
        }

        private async void OnScreenVisible()
        {
            await _routeExporter.Activate();
        }

        private async void OnScreenHidden()
        {
            await _routeExporter.Deactivate();
        }

        private async void OnShareRequested(DataRequest request)
        {
            var defferal = request.GetDeferral();
            
            await ApplicationData.Current.ClearAsync(ApplicationDataLocality.Temporary);

            var from = From.Date;
            var until = Until.Date.AddDays(1).AddMilliseconds(-1);
            var exportFiles = GetExportFiles(from, until).ToArray();

            if (exportFiles.Any())
            {
                request.Data.Properties.Title = "Exported SensorCore files";
                request.Data.Properties.Description = "Exported SensorCore files";
                request.Data.SetStorageItems(await Task.WhenAll(exportFiles));
            }
            else
            {
                request.FailWithDisplayText("Nothing selected to Export.");
            }

            defferal.Complete();
        }

        private IEnumerable<Task<IStorageItem>> GetExportFiles(DateTime from, DateTime until)
        {
            if (ExportRoutes)
            {
                yield return _routeExporter.Export(from, until);
            }
        }
    }
}