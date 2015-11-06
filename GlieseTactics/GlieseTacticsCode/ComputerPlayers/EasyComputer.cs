﻿using System;
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

            foreach (Unit unit in me.MyUnits)
            {
                if (!unit.AliveAndReady())
                    continue;

                // Apply three nexted templates: move, attack, damage.
                HexEffectStats stats = unit.MoveTemplate.OnApply(currentMap, unit.MapLocation,
                    new RecursiveTemplateEffect(currentMap, unit.TargetTemplate, true, true,
                        new RecursiveTemplateEffect(currentMap, unit.AttackTemplate, false, false,
                            new ExpectedDamageHexEffect(currentMap, unit),
                        false, m_priorities),
                    false, m_priorities),
                unit.CurrentHex, 
                m_priorities);

                // The stats returned should be the "best move" available to this unit based 
                // on the priorities given.  Zero consideration whatsoever to defensive positioning.
                // Just "do the most damage/kill the most units with this one move."
                if (bestMoveStats == null || bestMoveStats.AttackingUnit == null)
                    bestMoveStats = stats;
                else 
                    bestMoveStats = HexEffectStats.BestSingleMove(bestMoveStats, stats, m_priorities);
            }

            // We should have the "best move" picked out now.  
            // Translate/return it as a TurnInstructions object.
            return new TurnInstructions(bestMoveStats);
        }
    }
}