
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
            LanguageDfa dfa = new LanguageDfa(prefixStates);
            
            using (var stream = new MemoryStream())
            {
                dfa.WritePrefixStates(stream);
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
            LanguageDfa dfa = new LanguageDfa();
            dfa.AddNewString("feed");
            dfa.AddNewString("farm");

            Assert.IsTrue(dfa.Contains("feed"));
            Assert.IsTrue(dfa.Contains("farm"));
        }

        [TestMethod]
        public void LongMatchingPrefixTest()
        {
            LanguageDfa dfa = new LanguageDfa();
            dfa.AddNewString("abcdefgdddf");
            dfa.AddNewString("abcdefgfff");

            Assert.IsTrue(dfa.Contains("abcdefgdddf"));
            Assert.IsTrue(dfa.Contains("abcdefgfff"));
        }

        [TestMethod]
        public void SubstringDoesNotBreakTest()
        {
            LanguageDfa dfa = new LanguageDfa();
            dfa.AddNewString("coleman");
            dfa.AddNewString("cole");

            Assert.IsTrue(dfa.Contains("cole"));
            Assert.IsTrue(dfa.Contains("coleman"));
        }

        [TestMethod]
        public void BasicListStringsTest()
        {
            LanguageDfa dfa = new LanguageDfa();
            dfa.AddNewString("abcd");
            dfa.AddNewString("wxyz");

            Assert.IsTrue(dfa.ListPossibleStrings("").Contains("abcd"));
            Assert.IsTrue(dfa.ListPossibleStrings("").Contains("wxyz"));
        }

        [TestMethod]
        public void PrefixListStringsTest()
        {
            LanguageDfa dfa = new LanguageDfa();
            dfa.AddNewString("abcd");
            dfa.AddNewString("wxyz");

            Assert.IsTrue(dfa.ListPossibleStrings("abc").Contains("abcd"));
            Assert.IsFalse(dfa.ListPossibleStrings("abc").Contains("wxyz"));
        }

        [TestMethod]
        public async Task LongListStringsTestAsync()
        {
            LanguageDfa dfa = new LanguageDfa();
            
            // Load the large list of names
            StorageFolder assets = await Package.Current.InstalledLocation.GetFolderAsync("Assets");
            StorageFile namesListFile = await assets.GetFileAsync("names.txt");
            var namesList = await FileIO.ReadLinesAsync(namesListFile);
            foreach (string name in namesList)
            {
                dfa.AddNewString(name);
            }

            foreach (string name in namesList)
            {
                if (!dfa.Contains(name))
                {
                    System.Diagnostics.Debugger.Break();
                }
                Assert.IsTrue(dfa.Contains(name));
            }
        }

        [TestMethod]
        public async Task AutocompleteLongListStringsTestAsync()
        {
            LanguageDfa dfa = new LanguageDfa();

            // Load the large list of names
            StorageFolder assets = await Package.Current.InstalledLocation.GetFolderAsync("Assets");
            StorageFile namesListFile = await assets.GetFileAsync("names.txt");
            var namesList = await FileIO.ReadLinesAsync(namesListFile);
            foreach (string name in namesList)
            {
                dfa.AddNewString(name);
            }

            string[] foundNames = dfa.ListPossibleStrings("STE");
            Assert.AreEqual(7, foundNames.Length);
            Assert.IsTrue(foundNames.Contains("STEIN"));
            Assert.IsTrue(foundNames.Contains("STEWART"));
            Assert.IsTrue(foundNames.Contains("STEVENS"));
            Assert.IsTrue(foundNames.Contains("STEPHENS"));
            Assert.IsTrue(foundNames.Contains("STEELE"));
            Assert.IsTrue(foundNames.Contains("STEVENSON"));
            Assert.IsTrue(foundNames.Contains("STEPHENSON"));
        }

        [TestMethod]
        public async Task InitDFAFromFileTestAsync()
        {
            LanguageDfa dfa = new LanguageDfa();

            // Load the test file
            StorageFolder assets = await Package.Current.InstalledLocation.GetFolderAsync("Assets");
            StorageFile testFile = await assets.GetFileAsync("test.dfa");
            dfa.ReadPrefixStates(await testFile.OpenStreamForReadAsync());

            Assert.AreEqual(2, dfa.ListPossibleStrings("").Length);
            Assert.IsTrue(dfa.Contains("feed"));
            Assert.IsTrue(dfa.Contains("farm"));
        }
    }
}
