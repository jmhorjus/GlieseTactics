using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gliese581g.ComputerPlayers
{
    [Serializable]
    public class ComputerPlayer
    {
        public TurnInstructions GetNextMove(Map currentGameState)
        {
            return new TurnInstructions();
        }
    }
}
