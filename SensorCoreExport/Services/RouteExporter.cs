using System;
using System.Linq;
using System.Threading.Tasks;
using Lumia.Sense;
using Windows.Storage;
using SensorCoreExport.Gpx;

namespace SensorCoreExport.Services
{
    public class RouteExporter
        : BaseExporter<ITrackPointMonitor>
    {
        private readonly IOHelper _ioHelper;
        private readonly GpxSerializer _serializer;

        public RouteExporter(IOHelper ioHelper, GpxSerializer serializer)
        {
            _ioHelper = ioHelper;
            _serializer = serializer;
        }

        protected override async Task<ITrackPointMonitor> GetDefaultSensor()
        {
            return await TrackPointMonitor.GetDefaultAsync();
        }

        protected override async Task<bool> GetIsSupported()
        {
            return await TrackPointMonitor.IsSupportedAsync();
        }

        public override async Task<IStorageItem> Export(DateTimeOffset from, DateTimeOffset until)
        {
            var points = await Sensor.GetTrackPointsAsync(from, until-from);
            var orderedPoints = points.Where(p => p.Timestamp >= from).OrderBy(p => p.Timestamp);

            return await _ioHelper.WriteToFile($"SensorCore.Routes.{from:yyyyMMdd}-{until:yyyyMMdd}.gpx", s => {
                _serializer.Serialize(orderedPoints, s);
            });
        }        
    }
}