using GalaSoft.MvvmLight;

namespace SensorCoreExport.ViewModel
{
    public class ExportItemViewModel : ViewModelBase
    {
        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                RaisePropertyChanged();
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                RaisePropertyChanged();
            }
        }

        public void Setup(bool isSensorEnabled)
        {
            IsEnabled = isSensorEnabled;
            IsSelected = isSensorEnabled;
        }

        public bool CanExport()
        {
            return IsEnabled && IsSelected;
        }
    }
}