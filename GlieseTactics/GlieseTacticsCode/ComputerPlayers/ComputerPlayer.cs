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
        public int CommanderDamageWeight = 4;
        public int KillWeight = 25;
        public int CommanderKillWeight = 10000000;
        public int FriendlyDamageWeight = -1;
        public int FriendlyCommanderDamageWeight = -5;
        public int FriendlyKillWeight = -25;
        public int FriendlyCommanderKillWeight = -20000000;

        public int EnemyCommanderDistanceOffset = 30;
        public int EnemyCommanderDistanceWeight = -3;
        public MapLocation EnemyCommanderLocation = null;


        public int GetEffectValue(ref HexEffectStats effect)
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

            // Include the distance to enemy commander.
            if (effect.AttackOriginHex != null && 
                this.EnemyCommanderLocation != null)
            {
                int distance = Direction.GetMapDistance(
                    this.EnemyCommanderLocation.Position,
                    effect.AttackingUnit.MapLocation.Position);
                int newDistance = Direction.GetMapDistance(
                    this.EnemyCommanderLocation.Position, 
                    effect.AttackOriginHex.MapPosition);

                int value = (newDistance - distance) * EnemyCommanderDistanceWeight;

                if (!effect.AttackingUnit.IsCommander)
                    retVal += value; // Normal units prefer to approach.
                //else
                //    retVal -= value; // Commanders prefer to hang back.
            }

            effect.CalculatedUtility = retVal;

            return retVal;
        }
    }
}
