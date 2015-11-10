using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace SensorCoreExport.Services
{
    public class RouteExporter
    {
        Lumia.Sense.TrackPointMonitor _tracker;

        public async Task Activate()
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

        public async Task<bool> IsEnabled()
        {
            return await Lumia.Sense.TrackPointMonitor.IsSupportedAsync();
        }

        public async Task Deactivate()
        {
            if (_tracker != null)
            {
                await _tracker.DeactivateAsync();
            }
        }

        public async Task<IStorageItem> ExportRoutes(DateTime from, DateTime until)
        {
            var points = await _tracker.GetTrackPointsAsync(from, until-from);
            var orderedPoints = points.OrderBy(p => p.Timestamp);

            var file = await ApplicationData.Current.TemporaryFolder.CreateFileAsync($"trackpoints.{from:yyyyMMdd}-{until:yyyyMMdd}.gpx", CreationCollisionOption.ReplaceExisting);
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                new Gpx.SerializeToGpx().Serialize(orderedPoints, stream);
            }

            return file;
        }
    }
}