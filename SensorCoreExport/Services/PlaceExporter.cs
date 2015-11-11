using Lumia.Sense;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace SensorCoreExport.Services
{
    public class PlaceExporter
        : BaseExporter<IPlaceMonitor>
    {
        protected override async Task<IPlaceMonitor> GetDefaultSensor()
        {
            return await PlaceMonitor.GetDefaultAsync();
        }

        protected override async Task<bool> GetIsSupported()
        {
            return await PlaceMonitor.IsSupportedAsync();
        }

        public override async Task<IStorageItem> Export(DateTimeOffset from, DateTimeOffset until)
        {
            var places = await Sensor.GetPlaceHistoryAsync(from, until - from);
            
            throw new NotImplementedException();
        }
    }
}
