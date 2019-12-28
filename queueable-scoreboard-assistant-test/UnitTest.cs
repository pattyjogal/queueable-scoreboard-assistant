
using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using queueable_scoreboard_assistant.Common;
using System.Threading.Tasks;
using System.Linq;
using Windows.Storage;
using Windows.ApplicationModel;

namespace queueable_scoreboard_assistant_test
{
    [TestClass]
    public class AutocompleteTest
    {
        private Windows.Storage.StorageFolder storageFolder =
            Windows.Storage.ApplicationData.Current.LocalFolder;
        [TestMethod]
        public void DFAFromStringTest()
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

        [TestMethod]
        public void AddNameToDFATest()
        {
            AutocompleteModelManager manager = new AutocompleteModelManager();
            manager.WriteNewName("feed");
            manager.WriteNewName("farm");

            Assert.IsTrue(manager.CheckInLanguage("feed"));
            Assert.IsTrue(manager.CheckInLanguage("farm"));
        }

        [TestMethod]
        public void LongMatchingPrefixTest()
        {
            AutocompleteModelManager manager = new AutocompleteModelManager();
            manager.WriteNewName("abcdefgdddf");
            manager.WriteNewName("abcdefgfff");

            Assert.IsTrue(manager.CheckInLanguage("abcdefgdddf"));
            Assert.IsTrue(manager.CheckInLanguage("abcdefgfff"));
        }

        [TestMethod]
        public void SubstringDoesNotBreakTest()
        {
            AutocompleteModelManager manager = new AutocompleteModelManager();
            manager.WriteNewName("coleman");
            manager.WriteNewName("cole");

            Assert.IsTrue(manager.CheckInLanguage("cole"));
            Assert.IsTrue(manager.CheckInLanguage("coleman"));
        }

        [TestMethod]
        public void BasicListStringsTest()
        {
            AutocompleteModelManager manager = new AutocompleteModelManager();
            manager.WriteNewName("abcd");
            manager.WriteNewName("wxyz");

            Assert.IsTrue(manager.ListPossibleNames("").Contains("abcd"));
            Assert.IsTrue(manager.ListPossibleNames("").Contains("wxyz"));
        }

        [TestMethod]
        public void PrefixListStringsTest()
        {
            AutocompleteModelManager manager = new AutocompleteModelManager();
            manager.WriteNewName("abcd");
            manager.WriteNewName("wxyz");

            Assert.IsTrue(manager.ListPossibleNames("abc").Contains("abcd"));
            Assert.IsFalse(manager.ListPossibleNames("abc").Contains("wxyz"));
        }

        [TestMethod]
        public async Task LongListStringsTestAsync()
        {
            AutocompleteModelManager manager = new AutocompleteModelManager();
            
            // Load the large list of names
            StorageFolder assets = await Package.Current.InstalledLocation.GetFolderAsync("Assets");
            StorageFile namesListFile = await assets.GetFileAsync("names.txt");
            var namesList = await FileIO.ReadLinesAsync(namesListFile);
            foreach (string name in namesList)
            {
                manager.WriteNewName(name);
            }

            foreach (string name in namesList)
            {
                if (!manager.CheckInLanguage(name))
                {
                    System.Diagnostics.Debugger.Break();
                }
                Assert.IsTrue(manager.CheckInLanguage(name));
            }
        }

        [TestMethod]
        public async Task AutocompleteLongListStringsTestAsync()
        {
            AutocompleteModelManager manager = new AutocompleteModelManager();

            // Load the large list of names
            StorageFolder assets = await Package.Current.InstalledLocation.GetFolderAsync("Assets");
            StorageFile namesListFile = await assets.GetFileAsync("names.txt");
            var namesList = await FileIO.ReadLinesAsync(namesListFile);
            foreach (string name in namesList)
            {
                manager.WriteNewName(name);
            }

            string[] foundNames = manager.ListPossibleNames("STE");
            Assert.AreEqual(7, foundNames.Length);
            Assert.IsTrue(foundNames.Contains("STEIN"));
            Assert.IsTrue(foundNames.Contains("STEWART"));
            Assert.IsTrue(foundNames.Contains("STEVENS"));
            Assert.IsTrue(foundNames.Contains("STEPHENS"));
            Assert.IsTrue(foundNames.Contains("STEELE"));
            Assert.IsTrue(foundNames.Contains("STEVENSON"));
            Assert.IsTrue(foundNames.Contains("STEPHENSON"));
        }
    }
}
