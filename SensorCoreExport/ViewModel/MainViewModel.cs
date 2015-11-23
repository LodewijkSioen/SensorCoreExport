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
        private readonly PlaceExporter _placeExporter;
        private readonly StepExporter _stepExporter;

        public MainViewModel(INavigationService navigationService, RouteExporter routeExporter, PlaceExporter placeExporter, StepExporter stepExporter)
        {
            _navigationService = navigationService;
            _routeExporter = routeExporter;
            _placeExporter = placeExporter;
            _stepExporter = stepExporter;

            From = DateTime.Today;
            Until = DateTime.Today;

            Places = new ExportItemViewModel();
            Routes = new ExportItemViewModel();
            Steps = new ExportItemViewModel();

            Export = new MultiDependentRelayCommand(DataTransferManager.ShowShareUI,
                IsExportEnabled, 
                new CommandDependency(Routes, nameof(Routes.IsSelected), nameof(Routes.IsEnabled)),
                new CommandDependency(Places, nameof(Places.IsSelected), nameof(Places.IsEnabled)),
                new CommandDependency(Steps, nameof(Steps.IsSelected), nameof(Steps.IsEnabled)));
            ActivateTracker = new RelayCommand(OnScreenVisible);
            DeactivateTracker = new RelayCommand(OnScreenHidden);
            ShareRequested = new RelayCommand<DataRequest>(OnShareRequested);
            Settings = new RelayCommand(()=> _navigationService.NavigateTo("Settings"));
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

        public ExportItemViewModel Routes { get; }
        public ExportItemViewModel Places { get; }
        public ExportItemViewModel Steps { get; }

        public async Task Initialize()
        {   
            await _routeExporter.Initialize();
            await _placeExporter.Initialize();
            await _stepExporter.Initialize();

            Places.Setup(_placeExporter.IsEnabled);
            Routes.Setup(_routeExporter.IsEnabled);
            Steps.Setup(_stepExporter.IsEnabled);
        }

        private bool IsExportEnabled()
        {
            return Places.CanExport() || Routes.CanExport() || Steps.CanExport();
        }

        private async void OnScreenVisible()
        {
            await _routeExporter.Activate();
            await _placeExporter.Activate();
            await _stepExporter.Activate();
        }

        private async void OnScreenHidden()
        {
            await _routeExporter.Deactivate();
            await _placeExporter.Deactivate();
            await _stepExporter.Deactivate();
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
            if (Routes.IsSelected)
            {
                yield return _routeExporter.Export(from, until);
            }
            if (Places.IsSelected)
            {
                yield return _placeExporter.Export(from, until);
            }
            if (Steps.IsSelected)
            {
                yield return _stepExporter.Export(from, until);
            }
        }
    }
}