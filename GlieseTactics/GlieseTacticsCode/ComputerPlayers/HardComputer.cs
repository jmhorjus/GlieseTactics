using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gliese581g.ComputerPlayers
{
    class GameStatePriorities
    {
        Dictionary<UnitType, int> LiveUnitValueByType;
        Dictionary<int, int> UnitValueIgnoredByRechargeTime;
        
        int valuePerUnitHP = 1;
        int valuePerCommanderHP = 3;

        public GameStatePriorities()
        {
            LiveUnitValueByType[UnitType.Commander] = 100000000; //Commander is Everything
            LiveUnitValueByType[UnitType.Artillery] = 50;
            LiveUnitValueByType[UnitType.Infantry] = 25;
            LiveUnitValueByType[UnitType.Mech] = 60;
            LiveUnitValueByType[UnitType.RoughRider] = 30;
            LiveUnitValueByType[UnitType.Scout] = 20;
            LiveUnitValueByType[UnitType.Tank] = 40;

            // Ignore units somewhat during turns when they are not active.
            // (if the minimax goes deep enough, this should not be needed)
            UnitValueIgnoredByRechargeTime[0] = 0;
            UnitValueIgnoredByRechargeTime[1] = -20; //Actively try to kill units the turn before they recharge.
            UnitValueIgnoredByRechargeTime[2] = 30;
            UnitValueIgnoredByRechargeTime[3] = 40;
            UnitValueIgnoredByRechargeTime[4] = 50;
            UnitValueIgnoredByRechargeTime[5] = 50; // Cap ignored value at 50%...seems like a good ide.
            UnitValueIgnoredByRechargeTime[6] = 50;
        }
    }

    /// <summary>
    /// Hard computer should be using a true minimax/negamax algorithm and looking 
    /// multiple turns ahead.  In order to look further, to may use a "beam search" 
    /// of sorts or alpha-beta pruning to eliminate less desirable paths from 
    /// consideration.  
    /// 
    /// Problem:
    /// We need a way to simulate game states moving forward over multiple turns 
    /// without the use of code tied to the GUI and without wasting memory. 
    /// At the very least, this means starting out with a deep-copy of the entire
    /// game start that is handed to us, units and all.  As the search progresses 
    /// already expanded nodes need only keep...
    /// </summary>
    class HardComputer : ComputerPlayer
    {
        // Use the default for now.
        HexEffectPriorities m_priorities = new HexEffectPriorities();

        public override TurnInstructions GetNextMove(Map currentMap)
        {
            HexEffectStats bestMoveStats = null;

            Commander me = currentMap.Game.CurrentPlayer;

            foreach (Unit unit in me.MyUnits)
            {
                if (!unit.AliveAndReady())
                    continue;

                // Apply three nexted templates: move, attack, damage.
                HexEffectStats stats = unit.MoveTemplate.OnApply(currentMap, unit.MapLocation,
                    new RecursiveTemplateEffect(currentMap, unit.TargetTemplate, true, true,
                        new RecursiveTemplateEffect(currentMap, unit.AttackTemplate, false, false,
                            new ExpectedDamageHexEffect(currentMap, unit),
                        false, null), // The attack template is added up, not maximized or "get best"ed.  
                    false, m_priorities), // Target options "get best of".
                unit.CurrentHex, 
                m_priorities); // Move options "get best of". 

                // The stats returned should be the "best move" available to this unit based 
                // on the priorities given.  Zero consideration whatsoever to defensive positioning.
                // Just "do the most damage/kill the most units with this one move."
                if (bestMoveStats == null || bestMoveStats.AttackingUnit == null)
                    bestMoveStats = stats;
                else 
                    bestMoveStats = HexEffectStats.BestSingleMove(bestMoveStats, stats, m_priorities);
            }

            //Debug info 
            int totalMoves = bestMoveStats.GetTotalMovesContained();


            // We should have the "best move" picked out now.  
            // Translate/return it as a TurnInstructions object.
            return new TurnInstructions(bestMoveStats);
        }


        public int GameStateUtility(Map gameState, Commander currentPlayer, HexEffectPriorities priorities)
        {
            int retVal = 0;




            return retVal;
        }
    }
}
