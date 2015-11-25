using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gliese581g.ComputerPlayers
{
    /// <summary>
    /// Instructions for all actions neccessary to take a turn.
    /// Basically includes a list of things to "click on" sequentially.  
    /// In general, the common case should be: 
    ///  1.) a hex containing a friendly unit that can move this turn.
    ///  2.) a hex that that unit will move to (must be legal).
    ///  3.) a hex that that unit will target with its default attack or ability.
    /// </summary>
    public class TurnInstructions
    {
        public bool PassTurn = false;
        public bool Surrender = false;
        
        public Queue<ClickableSprite> ThingsToClickOn = new Queue<ClickableSprite>();
        public Direction RechargeFacing;

        public bool IsFinished() 
        { 
            return ThingsToClickOn.Count == 0 || PassTurn == true; 
        }

        // Utility value used in minimax calculation.
        public int UtilityValue = 0; 

        // Default empty constructor.
        public TurnInstructions() { ; }
        // Usual constructor taking a HexEffectStats.
        public TurnInstructions(HexEffectStats moveData)
        {
            Surrender = false;
            ThingsToClickOn = new Queue<ClickableSprite>();

            if (moveData.AttackingUnit == null) 
            {
                this.PassTurn = true;
                return;
            }

            ThingsToClickOn.Enqueue(moveData.AttackingUnit.CurrentHex);
            ThingsToClickOn.Enqueue(moveData.AttackOriginHex);
            ThingsToClickOn.Enqueue(moveData.AttackTargetHex);

            RechargeFacing = moveData.RechargeFacing;
            // Add the end-turn button as an instruction to end the turn? 
            // For now no; let Map.Update handle it.
        }




    }
}
