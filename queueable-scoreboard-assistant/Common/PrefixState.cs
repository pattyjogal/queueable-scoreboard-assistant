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
            transitions = parts[1].Split(',').Select(Int32.Parse).ToArray();
        }
        // Is this state an accepting state?
        public bool isAccepting;

        // The list of state-indices that this state can transition to
        public int[] transitions;

        public override string ToString()
        {
            string output = isAccepting ? "0" : "1";
            output += ";";
            output += string.Join(",", transitions);

            return output;
        }
    }
}
