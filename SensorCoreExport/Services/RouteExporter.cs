using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lumia.Sense;
using Windows.Storage;

namespace SensorCoreExport.Services
{
    public class RouteExporter
        : BaseExporter<ITrackPointMonitor>
    {
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

            var file = await ApplicationData.Current.TemporaryFolder.CreateFileAsync($"trackpoints.{from:yyyyMMdd}-{until:yyyyMMdd}.gpx", CreationCollisionOption.ReplaceExisting);
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                new Gpx.SerializeToGpx().Serialize(orderedPoints, stream);
            }

            return file;
        }        
    }
}