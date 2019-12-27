
using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using queueable_scoreboard_assistant.Common;
using System.Threading.Tasks;
using System.Linq;

namespace queueable_scoreboard_assistant_test
{
    [TestClass]
    public class AutocompleteTest
    {
        private Windows.Storage.StorageFolder storageFolder =
            Windows.Storage.ApplicationData.Current.LocalFolder;
        [TestMethod]
        public async Task DFAFromStringTestAsync()
        {
            List<String> expectedStates = new List<string>()
            {
                "0;f:1",
                "0;e:2,a:5",
                "0;e:3",
                "0;d:4",
                "0;r:6",
                "0;m:4"
            };
            List<PrefixState> prefixStates =
                expectedStates.Select(s => new PrefixState(s)).ToList();
            AutocompleteModelManager manager = new AutocompleteModelManager(prefixStates);
            
            Windows.Storage.StorageFile dfaFile = await storageFolder.CreateFileAsync("test.dfa",
                Windows.Storage.CreationCollisionOption.ReplaceExisting);
            var fileStream = 
                await dfaFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
            
            using (var inputStream = fileStream.GetOutputStreamAt(0))
            {
                manager.WritePrefixStates(inputStream.AsStreamForWrite());
            }

            using (var stream = new MemoryStream())
            {
                manager.WritePrefixStates(stream);
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    string actualStateString = reader.ReadToEnd();
                    Assert.AreEqual("#dfa 1.0\n" + String.Join('\n', expectedStates) + "\n",
                                    actualStateString);
                }
            }


        }
    }
}
