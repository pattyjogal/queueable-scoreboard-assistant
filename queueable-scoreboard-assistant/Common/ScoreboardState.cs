using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace queueable_scoreboard_assistant.Common
{
    public struct ScoreboardState
    {
        public int LeftScore { get; set; }
        public int RightScore { get; set; }

        public string LeftPlayerName { get; set; }
        public string RightPlayerName { get; set; }
    }
}
