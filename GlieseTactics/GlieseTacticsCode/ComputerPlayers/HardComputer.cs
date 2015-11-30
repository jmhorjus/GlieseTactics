using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework; 


namespace Gliese581g.ComputerPlayers
{

    class GameStatePriorities
    {
        Dictionary<UnitType, int> LiveUnitValueByType = new Dictionary<UnitType,int>();
        Dictionary<int, int> UnitValueIgnoredByRechargeTime = new Dictionary<int,int>();

        int valuePerDistanceFromEnemyCommander = -4;
        int maxDistanceToConsider = 6;
        int valuePerUnitHP = 1;
        int valuePerCommanderHP = 4;
        int valuePerNotLosing = 10000000; // Losing is bad.

        public GameStatePriorities()
        {
            LiveUnitValueByType[UnitType.Commander] = 70; // Commander Value for recharge adjustment.
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
            UnitValueIgnoredByRechargeTime[2] = 20;
            UnitValueIgnoredByRechargeTime[3] = 40;
            UnitValueIgnoredByRechargeTime[4] = 60;
            UnitValueIgnoredByRechargeTime[5] = 60; // Cap ignored value at 50%...seems like a good ide.
            UnitValueIgnoredByRechargeTime[6] = 60;
        }

        public int CalculateUtility(Map gameState, int currentPlayerIndex)
        {
            int retVal = 0;

            for (int ii = 0; ii < gameState.Game.Players.Count; ii++)
            {
                // is this the current player? 
                int valueMultiplier = (ii == currentPlayerIndex) ? 1 : -1;
                int nextPlayerIndex = (ii + 1) % 2;

                // Find the map coordinates of the enemy commander. 
                if (gameState.Game.Players[nextPlayerIndex].MyCommandUnit == null ||
                    gameState.Game.Players[nextPlayerIndex].MyCommandUnit.MapLocation == null)
                    return valuePerNotLosing * 2; // No enemy commander = win.
                MapLocation enemyCommanderLocation = gameState.Game.Players[nextPlayerIndex].MyCommandUnit.MapLocation;

                

                foreach (Unit unit in gameState.Game.Players[ii].MyUnits)
                {
                    if (unit.CurrentHP > 0) // All living units count.
                    {
                        // Start with the unit's intrinsic value.
                        int unitValue = LiveUnitValueByType[unit.TypeOfUnit];
                        // Ignore some of this value if the unit is recharging and unavailable. 
                        unitValue = (unitValue * (100 - UnitValueIgnoredByRechargeTime[unit.CurrentRechargeTime])) / 100;
                        
                        // Add in the unit's HP (also whether our commander is alive).
                        if (unit.TypeOfUnit == UnitType.Commander)
                            unitValue += (unit.CurrentHP * valuePerCommanderHP) + valuePerNotLosing;
                        else
                            unitValue += unit.CurrentHP * valuePerUnitHP;

                        // Add in distance to enemy commander. 
                        unitValue += valuePerDistanceFromEnemyCommander * (-maxDistanceToConsider +
                            Direction.GetMapDistance(unit.MapLocation.Position, enemyCommanderLocation.Position));

                        // Multiply in which player the unit belongs to - negative value if not current player.
                        retVal += unitValue * valueMultiplier;
                    }
                }
            }

            return retVal;
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
        // Use the default for now (not really needed in this class)
        HexEffectPriorities m_priorities = new HexEffectPriorities();

        // The default GameStatePriorities.
        GameStatePriorities m_gameStatePriorities = new GameStatePriorities();

        // This function should execute the recursive mini-max/negamax function.  
        public override TurnInstructions GetNextMove(Map currentMap)
        {
            TurnInstructions retVal = null;

            retVal = NegaMax(currentMap
                    , 2/*for now try using a depth of two - one tun per player*/
                    , int.MinValue + 1 //alpha 
                    , int.MaxValue - 1 //beta
                    , currentMap.Game.CurrentPlayerIndex /*either zero or one*/
                    );

            return retVal;
        }

        // Recursive function - returns the best move after searching to a given depth. 
        // Need to experient with time/emory limitations.
        // (limit depth, or limit beam width, possibly enforce max nodes expanded on both)...
        protected TurnInstructions NegaMax(Map currentMap, int depth, int alpha, int beta, int currentPlayerIndex)
        {
            // If we've reached our depth or have reached a terminal node, just return this gamestate's utility.
            if (depth == 0 || currentMap.Game.CurrentTurnStage == Game.TurnStage.GameOver)
            {
                TurnInstructions nextMove = new TurnInstructions(); // Empty turn instructions - no actual move.
                nextMove.UtilityValue = m_gameStatePriorities.CalculateUtility(currentMap, currentPlayerIndex);
                return nextMove;
            }

            // Look through all possible moves for the best.  
            HexEffectStats allMoveStats = null;
            TurnInstructions bestMoveSoFar = null;

            // Find all possible moves, using the unit's properties and templates.
            Commander me = currentMap.Game.Players[currentPlayerIndex];
            foreach (Unit unit in me.MyUnits)
            {
                if (!unit.AliveAndReady())
                    continue;

                // Apply three nexted templates: move, attack, damage.
                HexEffectStats attackStats = unit.MoveTemplate.OnApply(currentMap, unit.MapLocation,
                    new RecursiveTemplateEffect(currentMap, unit.TargetTemplate, true, true,
                        new RecursiveTemplateEffect(currentMap, unit.AttackTemplate, false, false,
                            new ExpectedDamageHexEffect(currentMap, unit),
                        false, null), // The attack template is added up, not maximized or "get best"ed.  
                    false, m_priorities), // Target options "get best of".
                unit.CurrentHex, 
                m_priorities); // Move options "get best of". 

                // "stats" should now contains all possible attacking moves by the given unit. 
                // Add them into the list of possible moves.
                allMoveStats = HexEffectStats.BestSingleMove(allMoveStats, attackStats, m_priorities);

                //TODO: Now get recharge moves for this unit and add them to allMoveStats as well.
                // For this we only need the move template applied with all directions considered at the end.  
                HexEffectStats rechargeStats = unit.MoveTemplate.OnApply(currentMap, unit.MapLocation,
                    new ExpectedRechargeHexEffect(currentMap, unit, true, Point.Zero),
                    unit.CurrentHex, m_priorities);
  
                allMoveStats = HexEffectStats.BestSingleMove(allMoveStats, rechargeStats, m_priorities);
            }

            // Get the list of all valid moves.  
            List<HexEffectStats> allMoves = new List<HexEffectStats>();
            if (allMoveStats != null)
                allMoveStats.GetTotalMovesContained(ref allMoves);
            else
                allMoves.Add(new HexEffectStats()); // No moves available - must pass turn doing nothing.  


            // Calculate the resulting game state if we make each move in the list.  
            foreach (HexEffectStats move in allMoves)
            {
                // We're going to recurse now.  
                // 1.) Sart by making a deep copy of the current game state.
                Map newGameState = new Map(currentMap);

                // 2.) Fully perform this move on the new gamestate.
                newGameState.quickMove(new TurnInstructions(move));

                // 3.) Recurse using the new gamestate and opposite player while decrementing depth.
                TurnInstructions newTurnInstruction = NegaMax(
                    newGameState, 
                    depth - 1,
                    -beta, // -beta becomes alpha.
                    -alpha, // -alpha becomes beta.
                    currentPlayerIndex == 0 ? 1 : 0);
                if (newTurnInstruction == null)
                    continue; // There was no valid move - forced pass turn. 
                              // Might need to do something for this case.
                // Negate the returned utility value to complete negamax.  
                newTurnInstruction.UtilityValue *= -1;
                
                // 4.) Compare the result to our best move calculated so far.
                if (bestMoveSoFar == null || bestMoveSoFar.UtilityValue < newTurnInstruction.UtilityValue)
                {
                    // Attach the move at this depth to the utility from the lower depth.
                    bestMoveSoFar = new TurnInstructions(move);
                    bestMoveSoFar.UtilityValue = newTurnInstruction.UtilityValue;
                }

                // Do alpha-beta pruning.  
                if (bestMoveSoFar != null && alpha < bestMoveSoFar.UtilityValue)
                    alpha = bestMoveSoFar.UtilityValue;
                if (alpha >= beta)
                    break;
            }

            // We should have the "best move" picked out now.  
            // Translate/return it as a TurnInstructions object.
            return bestMoveSoFar;
        }


        public int GameStateUtility(Map gameState, Commander currentPlayer, HexEffectPriorities priorities)
        {
            int retVal = 0;




            return retVal;
        }
    }
}
