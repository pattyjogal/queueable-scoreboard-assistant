using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace queueable_scoreboard_assistant.Common
{
    [Serializable()]
    public class  InvalidFileFormatException : System.Exception
    {

    }

    public class AutocompleteModelManager
    {
        private const string FileHeader = "#dfa 1.0";
        private Windows.Storage.StorageFolder storageFolder =
            Windows.Storage.ApplicationData.Current.LocalFolder;

        private string _prefix;
        private int _currentNode = 0;
        private List<PrefixState> _prefixStates;

        public AutocompleteModelManager(List<PrefixState> prefixStates)
        {
            _prefixStates = prefixStates;
        }

        public AutocompleteModelManager(string path)
        {
            ReadPrefixStatesFromFile(path);
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
            await Windows.Storage.FileIO.AppendTextAsync(dfaFile, $"{FileHeader}\n");
            await Windows.Storage.FileIO.AppendLinesAsync(dfaFile,
                _prefixStates.Select(s => s.ToString()).ToArray());
          
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
        /// Reads all of the prefix states from a file.
        /// </summary>
        /// <param name="path">the path where the states file exists</param>
        private void ReadPrefixStatesFromFile(string path)
        {
            using (System.IO.StreamReader file = new System.IO.StreamReader(path))
            {
                if (!file.ReadLine().Equals(FileHeader))
                {
                    throw new InvalidFileFormatException();
                }

                while (file.ReadLine() is string line)
                {
                    PrefixState prefixState = new PrefixState(line);
                    
                }
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
            statePath.Push((_currentNode, prefix));

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
