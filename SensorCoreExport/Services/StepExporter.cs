using System;
using System.Threading.Tasks;
using Lumia.Sense;
using Windows.Storage;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace SensorCoreExport.Services
{
    public class StepExporter
        : BaseExporter<IStepCounter>
    {
        private IOHelper _ioHelper;

        public StepExporter(IOHelper ioHelper)
        {
            _ioHelper = ioHelper;
        }

        protected async override Task<IStorageItem> Export(IStepCounter sensor, DateTimeOffset from, DateTimeOffset until)
        {
            var stepCounter = new List<StepCount>();
            for (int i = 0; i < (until - from).TotalHours; i++)
            {
                var steps = await sensor.GetStepCountForRangeAsync(from.AddHours(i), TimeSpan.FromHours(1));
                stepCounter.Add(steps);
            }

            return await _ioHelper.WriteToFile($"SensorCore.Steps.{from:yyyyMMdd}-{until:yyyyMMdd}.json", s => 
            {
                var serializer = new JsonSerializer();
                using (var sw = new StreamWriter(s))
                {
                    serializer.Serialize(sw, stepCounter.ToArray());
                }
            });
        }

        protected async override Task<IStepCounter> GetDefaultSensor()
        {
            return await StepCounter.GetDefaultAsync();
        }

        protected async override Task<bool> GetIsSupported()
        {
            return await StepCounter.IsSupportedAsync();
        }
    }
}
