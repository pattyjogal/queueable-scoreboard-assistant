using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace queueable_scoreboard_assistant.Common
{
    public class ScheduledMatch
    {
        public string FirstPlayer { get; set; }
        public string SecondPlayer { get; set; }
        public string MatchName { get; set; }

        public ScheduledMatch(string firstPlayer, string secondPlayer, string matchName)
        {
            FirstPlayer = firstPlayer;
            SecondPlayer = secondPlayer;
            MatchName = matchName;
        }
    }
}
