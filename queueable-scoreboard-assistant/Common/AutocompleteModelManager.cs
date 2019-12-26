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
        private List<PrefixState> _prefixStates;

        /// <summary>
        /// Writes all of the prefix states into a file.
        /// 
        /// Overwrites the file at the given path.
        /// </summary>
        /// <param name="path">the path where the states file will be written</param>
        private void DumpPrefixStatesToFile(string path)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(path, false))
            {
                foreach (PrefixState state in _prefixStates)
                {
                    file.WriteLine(state.ToString());
                }
            }
        }

        /// <summary>
        /// Reads all of the prefix states from a file.
        /// </summary>
        /// <param name="path">the path where the states file exists</param>
        private void ReadPrefixStatesFromFile(string path)
        {

        }

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
