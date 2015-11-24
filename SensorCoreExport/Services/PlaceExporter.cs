using Lumia.Sense;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using SensorCoreExport.Gpx;

namespace SensorCoreExport.Services
{
    public class PlaceExporter
        : BaseExporter<IPlaceMonitor>
    {
        private readonly IOHelper _ioHelper;
        private readonly GpxSerializer _serialize;

        public PlaceExporter(IOHelper ioHelper, Gpx.GpxSerializer serializer)
        {
            _ioHelper = ioHelper;
            _serialize = serializer;
        }

        protected override async Task<IPlaceMonitor> GetDefaultSensor()
        {
            return await PlaceMonitor.GetDefaultAsync();
        }

        protected override async Task<bool> GetIsSupported()
        {
            return await PlaceMonitor.IsSupportedAsync();
        }

        protected override async Task<IStorageItem> Export(IPlaceMonitor sensor, DateTimeOffset from, DateTimeOffset until)
        {
            var places = await sensor.GetPlaceHistoryAsync(from, until - from);

            return await _ioHelper.WriteToFile($"SensorCore.Places.{from:yyyyMMdd}-{until:yyyyMMdd}.gpx", s => {
                _serialize.Serialize(places, s);
            });
        }
    }
}
