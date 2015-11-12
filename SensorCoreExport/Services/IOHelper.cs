using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace SensorCoreExport.Services
{
    public class IOHelper
    {
        public async Task Cleanup()
        {
            await ApplicationData.Current.ClearAsync(ApplicationDataLocality.Temporary);
        }

        public async Task<StorageFile> WriteToFile(string filename, Action<Stream> fileAction)
        {
            var file = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

            using (var stream = await file.OpenStreamForWriteAsync())
            {
                fileAction(stream);
            }

            return file;
        }
    }
}
