using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace queueable_scoreboard_assistant.Common
{
    [Serializable()]
    public class InvalidFileFormatException : System.Exception
    {

    }

    public class LanguageDfa
    {
        private const string FileHeader = "#dfa 1.0";
        private Windows.Storage.StorageFolder storageFolder =
            Windows.Storage.ApplicationData.Current.LocalFolder;

        private List<PrefixState> _prefixStates;

        public LanguageDfa()
        {
            _prefixStates = new List<PrefixState>();
        }

        public LanguageDfa(List<PrefixState> prefixStates)
        {
            _prefixStates = prefixStates;
        }

        /// <summary>
        /// Writes all of the prefix states into a file.
        /// 
        /// Overwrites the file at the given path.
        /// </summary>
        /// <param name="fileName">the file name to write to in the app's local dir</param>
        public async void DumpPrefixStatesAsync(string fileName)
        {
            Windows.Storage.StorageFile dfaFile = await storageFolder.CreateFileAsync(fileName,
                Windows.Storage.CreationCollisionOption.ReplaceExisting);
            var fileStream =
                await dfaFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);

            using (var inputStream = fileStream.GetOutputStreamAt(0))
            {
                WritePrefixStates(inputStream.AsStreamForWrite());
            }


        }

        /// <summary>
        /// Writes the prefix states as bytes to a stream.
        /// </summary>
        /// <param name="stream">the stream to write bytes to</param>
        public void WritePrefixStates(System.IO.Stream stream)
        {
            stream.Write(Encoding.UTF8.GetBytes($"{FileHeader}\n"),
                0, FileHeader.Length + 1);

            foreach (var state in _prefixStates)
            {
                stream.Write(Encoding.UTF8.GetBytes($"{state.ToString()}\n"),
                    0, state.ToString().Length + 1);
            }

            stream.Flush();
        }

        /// <summary>
        /// Reads all of the prefix states from a stream.
        /// </summary>
        /// <param name="path">the stream to read bytes from</param>
        public void ReadPrefixStates(System.IO.Stream stream)
        {
            StreamReader streamReader = new StreamReader(stream);

            if (!streamReader.ReadLine().Equals(FileHeader))
            {
                throw new InvalidFileFormatException();
            }

            while (streamReader.ReadLine() is string line)
            {
                _prefixStates.Add(new PrefixState(line));
                
            }
        }

        /// <summary>
        /// Lists all of the possible names in the language starting from the prefix.
        /// </summary>
        /// <param name="prefix">the starting point for the traversal</param>
        /// <returns></returns>
        public string[] ListPossibleNames(string prefix)
        {
            List<string> names = new List<string>();
            Stack<(int, string)> statePath = new Stack<(int, string)>();

            // Traverse to the state after reading the prefix
            var (startStateIndex, _) = FindPrefixState(prefix);

            statePath.Push((startStateIndex, prefix));

            while (statePath.Count() > 0)
            {
                var (stateIndex, currentName) = statePath.Pop();
                PrefixState state = _prefixStates[stateIndex];
                if (state.isAccepting)
                {
                    names.Add(currentName);
                }

                foreach (var (c, s) in state.transitions)
                {
                    statePath.Push((s, currentName + c));
                }
            }

            return names.ToArray();
        }

        /// <summary>
        /// Adds a name not currently handled by the DFA to it.
        /// </summary>
        /// <param name="newName">the name to add to the DFA</param>
        public void WriteNewName(string newName)
        {
            // The language does not accept empty strings
            if (newName.Length == 0)
            {
                return;
            }

            // Handle the case where the language is empty
            if (_prefixStates.Count == 0)
            {
                _prefixStates.Add(new PrefixState(false));
            }

            PrefixState state = _prefixStates[0];

            while (newName.Length > 0 && state.transitions.ContainsKey(newName[0]))
            {
                state = _prefixStates[state.transitions[newName[0]]];
                newName = newName.Remove(0, 1);
            }

            // If the whole name was consumed, then this name is a substring of another name
            // in the language, so we just make this an accepting state.
            if (newName.Length == 0)
            {
                state.isAccepting = true;
            }

            while (newName.Length > 0)
            {
                _prefixStates.Add(new PrefixState(newName.Length == 1));
                // Link the current state to the newly created one
                state.transitions.Add(newName[0], _prefixStates.Count - 1);
                state = _prefixStates.Last();
                newName = newName.Remove(0, 1);
            }
        }

        /// <summary>
        /// Determines if a string is accepted by the autocomplete
        /// </summary>
        /// <param name="word">the word to check against the DFA's language</param>
        /// <returns></returns>
        public bool Contains(string word)
        {
            var (_, state) = FindPrefixState(word);
            return state.isAccepting;
        }

        private (int, PrefixState) FindPrefixState(string prefix)
        {
            int stateIndex = 0;
            PrefixState state = _prefixStates[stateIndex];
            while (prefix.Count() > 0 && state.transitions.ContainsKey(prefix[0]))
            {
                stateIndex = state.transitions[prefix[0]];
                state = _prefixStates[stateIndex];
                prefix = prefix.Remove(0, 1);
            }

            return (stateIndex, state);
        }
    }
}
