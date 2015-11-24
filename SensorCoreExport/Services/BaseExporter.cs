using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace SensorCoreExport.Services
{
    public abstract class BaseExporter<T>
        where T : class, Lumia.Sense.ISensor
    {
        public bool IsEnabled { get; private set; }

        protected abstract Task<bool> GetIsSupported();

        protected abstract Task<T> GetDefaultSensor();

        protected abstract Task<IStorageItem> Export(T sensor, DateTimeOffset from, DateTimeOffset until);

        public async Task<IStorageItem> Export(DateTimeOffset from, DateTimeOffset until)
        {
            T sensor = null;
            try
            {
                sensor = await GetDefaultSensor();
                await sensor.ActivateAsync();
                return await Export(sensor, from, until);
            }
            finally
            {
                if (sensor != null)
                {
                    await sensor.DeactivateAsync();
                }
            }
        }

        public async Task Initialize()
        {
            IsEnabled = await GetIsSupported();
        }        
    }
}
