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
        public bool Surrender = false;
        public Queue<ClickableSprite> ThingsToClickOn = new Queue<ClickableSprite>();
    }
}
