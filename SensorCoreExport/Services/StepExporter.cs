using System;
using System.Threading.Tasks;
using Lumia.Sense;
using Windows.Storage;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace SensorCoreExport.Services
{
    public class StepExporter
        : BaseExporter<StepCounter>
    {
        private IOHelper _ioHelper;

        public StepExporter(IOHelper ioHelper)
        {
            _ioHelper = ioHelper;
        }

        public async override Task<IStorageItem> Export(DateTimeOffset from, DateTimeOffset until)
        {
            var steps = await Sensor.GetStepCountHistoryAsync(from, until - from);

            return await _ioHelper.WriteToFile($"SensorCore.Steps.{from:yyyyMMdd}-{until:yyyyMMdd}.json", s => 
            {
                var serializer = new JsonSerializer();
                using (var sw = new StreamWriter(s))
                {
                    serializer.Serialize(sw, steps.ToArray());
                }
            });
        }

        protected async override Task<StepCounter> GetDefaultSensor()
        {
            return await StepCounter.GetDefaultAsync();
        }

        protected async override Task<bool> GetIsSupported()
        {
            return await StepCounter.IsSupportedAsync();
        }
    }
}
