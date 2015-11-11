using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace SensorCoreExport.Services
{
    public abstract class BaseExporter<T>
        where T : Lumia.Sense.ISensor
    {
        protected T Sensor;

        public bool IsEnabled { get; private set; }

        protected abstract Task<bool> GetIsSupported();

        protected abstract Task<T> GetDefaultSensor();

        public abstract Task<IStorageItem> Export(DateTimeOffset from, DateTimeOffset until);

        public async Task Initialize()
        {
            IsEnabled = await GetIsSupported();

            if (IsEnabled)
            {
                Sensor = await GetDefaultSensor();
            }
        }

        public async Task Activate()
        {
            if (IsEnabled)
            {
                await Sensor.ActivateAsync();
            }
        }

        public async Task Deactivate()
        {
            if (IsEnabled)
            {
                await Sensor.DeactivateAsync();
            }
        }
    }
}
