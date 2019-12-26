﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace queueable_scoreboard_assistant.Common
{
    class PrefixState
    {
        // Is this state an accepting state?
        public bool isAccepting;

        // The list of state-indices that this state can transition to
        public int[] transitions;
    }
}
