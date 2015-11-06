using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gliese581g.ComputerPlayers
{
    [Serializable]
    public class ComputerPlayer
    {
        public virtual TurnInstructions GetNextMove(Map currentGameState)
        {
            return new TurnInstructions();
        }
    }

    /// <summary>
    /// Class used by AI players to determine their priorities. 
    /// Each type of effect is given a value weight.
    /// </summary>
    public class HexEffectPriorities
    {
        // Some default values that are somewhat sensible. 
        public int DamageWeight = 1;
        public int CommanderDamageWeight = 2;
        public int KillWeight = 25;
        public int CommanderKillWeight = 1000000;
        public int FriendlyDamageWeight = -1;
        public int FriendlyCommanderDamageWeight = -3;
        public int FriendlyKillWeight = -25;
        public int FriendlyCommanderKillWeight = -2000000;

        public int GetEffectValue(HexEffectStats effect)
        {
            // Add up all the values times their respective weights.
            int retVal =
                (DamageWeight * effect.Damage) +
                (KillWeight * effect.Kills) +
                (CommanderDamageWeight * effect.CommanderDamage) +
                (CommanderKillWeight * effect.CommanderKills) +
                (FriendlyDamageWeight * effect.FriendlyDamage) +
                (FriendlyKillWeight * effect.FriendlyKills) +
                (FriendlyCommanderDamageWeight * effect.FriendlyCommanderDamage) +
                (FriendlyCommanderKillWeight * effect.FriendlyCommanderKills); 

            return retVal;
        }
    }
}
