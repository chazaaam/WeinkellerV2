using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Weinkeller
{
    class ErrorLog
    {
        public StorageFile LogFileWrite;
        public StorageFolder LogFolder;
        public List<string> LogList;
        public string FileName;

        public ErrorLog()
        {
            LogFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            FileName = "Log.xml";

            LogList = new List<string>();
        }

        public async void WritetoFile(string error_text)
        {
            LogList.Add(DateTime.Now.ToString() + ": " + error_text);
            LogFileWrite = await LogFolder.CreateFileAsync(FileName, Windows.Storage.CreationCollisionOption.OpenIfExists);

            Windows.Storage.FileProperties.BasicProperties basicProperties = await LogFileWrite.GetBasicPropertiesAsync();
            if (basicProperties.Size > 1000000)
                DeleteLog();

            await FileIO.AppendLinesAsync(LogFileWrite, LogList);
        }


        public async void DeleteLog()
        {
            LogList.Clear();
            LogList = new List<string>();
            LogFileWrite = await LogFolder.CreateFileAsync(FileName, Windows.Storage.CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteLinesAsync(LogFileWrite, LogList);
        }
    }
}
