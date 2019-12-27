using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace queueable_scoreboard_assistant.Common
{
    class PrefixState
    {
        /// <summary>
        /// Creates a prefix state from a string that ToString() creates.
        /// </summary>
        /// <param name="state">a string in the PrefixState.ToString() format</param>
        public PrefixState(string state)
        {
            string[] parts = state.Split(';');
            if (parts.Length < 2)
            {
                throw new InvalidFileFormatException();
            }

            isAccepting = parts[0].Equals("1");
            foreach (string pair in parts[1].Split(','))
            {
                string[] separatedPair = pair.Split(':');
                transitions.Add(char.Parse(separatedPair[0]), int.Parse(separatedPair[1]));
            }
        }

        // Is this state an accepting state?
        public bool isAccepting;

        // The dict of char transitions to state-indices on this state
        public Dictionary<char, int> transitions;

        public override string ToString()
        {
            StringBuilder outputBuilder = new StringBuilder();
            outputBuilder.Append(isAccepting ? "0" : "1");
            outputBuilder.Append(";");
            foreach (var item in transitions)
            {
                outputBuilder.Append($"{item.Key}:{item.Value}");
            }

            return outputBuilder.ToString();
        }
    }
}
