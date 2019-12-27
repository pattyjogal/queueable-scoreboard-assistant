
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using queueable_scoreboard_assistant.Common;

namespace queueable_scoreboard_assistant_test
{
    [TestClass]
    public class AutocompleteTest
    {
        [TestMethod]
        public void DFAWriteTest()
        {
            List<PrefixState> prefixStates = new List<PrefixState>()
            {
                new PrefixState("0;f:1"),
                new PrefixState("0;e:2,a:5"),
                new PrefixState("0;e:3"),
                new PrefixState("0;d:4"),
                new PrefixState("1;"),
                new PrefixState("0;r:6"),
                new PrefixState("0;m:4")
            };
            AutocompleteModelManager manager = new AutocompleteModelManager(prefixStates);

            manager.DumpPrefixStatesAsync(@"test.dfa");
        }
    }
}
