using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace queueable_scoreboard_assistant.Common
{
    class AutocompleteModelManager
    {
        private string _prefix;
        private PrefixState[] prefixStates;

        /// <summary>
        /// Adds a name not currently handled by the DFA to it.
        /// </summary>
        /// <param name="newName">the name to add to the DFA</param>
        private void WriteNewName(string newName)
        {

        }

        /// <summary>
        /// Defines the response that the manager takes when a user selects a name.
        /// </summary>
        /// <param name="selectedName">the name that the user chose</param>
        public void OnUserSelect(string selectedName) { 

        }

        /// <summary>
        /// Adds a character to the prefix search. 
        /// 
        /// The maintained DFA requires us to keep track of the traversed path, which is done through a prefix.
        /// </summary>
        /// <param name="addition"></param>
        public void AddPrefixChar(char addition)
        {

        }
    }
}
