using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gliese581g.ComputerPlayers
{
    class EasyComputer : ComputerPlayer
    {
        // Easy player just use the default.
        HexEffectPriorities m_priorities = new HexEffectPriorities();

        public override TurnInstructions GetNextMove(Map currentMap)
        {
            HexEffectStats bestMoveStats = null;

            Commander me = currentMap.Game.CurrentPlayer;

            // Need to find the enemy commander. 
            m_priorities.EnemyCommanderLocation = currentMap.Game.NextPlayer.MyCommandUnit.MapLocation;

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

            if (bestMoveStats == null || bestMoveStats.Damage + bestMoveStats.CommanderDamage == 0)
            { // There was no attacking move, make a recharge move.  

                foreach (Unit unit in me.MyUnits)
                {
                    if (!unit.AliveAndReady())
                        continue;

                    // Get recharge moves for this unit.
                    HexEffectStats rechargeStats = unit.MoveTemplate.OnApply(currentMap, unit.MapLocation,
                        new ExpectedRechargeHexEffect(currentMap, unit, 
                            false, m_priorities.EnemyCommanderLocation.Position),//not every direction, just face toward the commander.
                        unit.CurrentHex, m_priorities);

                    bestMoveStats = HexEffectStats.BestSingleMove(bestMoveStats, rechargeStats, m_priorities);
                }

            }

            // We should have the "best move" picked out now.  
            // Translate/return it as a TurnInstructions object.
            return new TurnInstructions(bestMoveStats);
        }
    }
}
