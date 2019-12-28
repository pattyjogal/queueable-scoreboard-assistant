
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
    }
}
