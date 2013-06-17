using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gliese581g
{

    public abstract class MapTemplate
    {
        protected static Hex nullOutputDestination = null;
        public abstract HexEffectStats OnApply(Map map, MapLocation location, HexEffect effect, out Hex onlyOneHex);
        public abstract HexEffectStats OnApply(Map map, MapLocation location, HexEffect effect);
    }


    public class LineTemplate : MapTemplate
    {
        int m_length;
        bool m_includeSourceHex;
        bool m_stopAtUnit;
        bool m_stopAtBlockingTerrain;

        public LineTemplate(int length, bool stopAtUnit, bool stopAtBlockingTerrain, bool includeSourceHex)
        {
            m_length = length;
            m_stopAtUnit = stopAtUnit;
            m_stopAtBlockingTerrain = stopAtBlockingTerrain;
            m_includeSourceHex = includeSourceHex;
        }

        public override HexEffectStats OnApply(Map map, MapLocation location, HexEffect effect)
        {
            return OnApply(map, location, effect, out nullOutputDestination);
        }

        public override HexEffectStats OnApply(Map map, MapLocation location, HexEffect effect, out Hex onlyOneHex)
        {
            HexEffectStats retVal = new HexEffectStats();
            onlyOneHex = null;
            bool isFirstHex = true;
            for (int ii = 0; ii < m_length; ii++)
            {
                if (ii == 0 && !m_includeSourceHex)
                {   // Special case for m_includeSourceHex.
                    location.StepForward();
                    continue;
                }

                Hex hex = map.GetHex(location.Location);
                if (hex == null)
                    break;

                bool doneAfterThisHex = ii + 1 >= m_length ||
                    (m_stopAtUnit && (hex.Unit != null)) ||
                    (m_stopAtBlockingTerrain && !map.GetHex(location.Location).LandPossible);

                if (isFirstHex && doneAfterThisHex)
                    onlyOneHex = hex;
                    
                // Apply the effect.
                retVal += effect.ApplyToHex(hex);
                isFirstHex = false;

                if (doneAfterThisHex)
                    break;
                // Move to the next hex in the line.
                location.StepForward();
            }
            return retVal;
        }

    }



    public class RangeTemplate : MapTemplate
    {
        int m_range;
        public int Range { get { return m_range; } }
        bool m_pathThroughBlockHexes;
        bool m_pathThroughOtherUnits;
        bool m_includeSourceHex;
        Unit m_unitWhoseAlliesDontBlockMovement;
        public Unit UnitWhoseAlliesDontBlockMovement
        { get { return m_unitWhoseAlliesDontBlockMovement; }
            set { m_unitWhoseAlliesDontBlockMovement = value; }
        }

        public RangeTemplate(int range, 
            bool pathThroughBlockHexes, 
            bool pathThroughOtherUnits, 
            bool includeSourceHex,
            Unit unitWhoseAlliesDontBlockMovement = null)
        {
            m_range = range;
            m_pathThroughBlockHexes = pathThroughBlockHexes;
            m_pathThroughOtherUnits = pathThroughOtherUnits;
            m_includeSourceHex = includeSourceHex;
            m_unitWhoseAlliesDontBlockMovement = unitWhoseAlliesDontBlockMovement;
        }

        public override HexEffectStats OnApply(Map map, MapLocation location, HexEffect effect)
        {
            return OnApply(map, location, effect, out nullOutputDestination);
        }

        // The public OnApply
        public override HexEffectStats OnApply(Map map, MapLocation location, HexEffect effect, out Hex onlyOneHex)
        {
            HexEffectStats retVal = new HexEffectStats();
            // Ranged template never only one hex
            onlyOneHex = null;

            map.ClearMarkedHexes(); // We use hex marking to ensure we don't double-apply to some hexes.
            RecursiveApply(
                map,
                effect,
                location.Location, location.Location,
                m_range, m_range,
                (m_unitWhoseAlliesDontBlockMovement != null) ? m_unitWhoseAlliesDontBlockMovement.Owner : null,
                ref retVal);
            return retVal;
        }
 
        // This one is private and is called recursively. 
        void RecursiveApply(
            Map map,
            HexEffect effect,
            Point startingPos,
            Point pos,
            int totalRange,
            int rangeRemaining,
            Player friendlyPlayer, // the units of this player are friendly and can be moved through.
            ref HexEffectStats stats
            )
        {
            Hex hex = map.GetHex(pos);
            if (hex == null)
                return;

            bool validFinalDest = (((hex.LandPossible || m_pathThroughBlockHexes) &&
                (hex.Unit == null || m_pathThroughOtherUnits)) ||  // Hex is a valid final destination.
                ((pos == startingPos) && m_includeSourceHex));

            if (validFinalDest)
            {
                if (!hex.IsMarked && (pos != startingPos || m_includeSourceHex))
                {
                    stats += effect.ApplyToHex(hex);
                    map.MarkHex(hex);
                    hex.CurrentMoveCost = totalRange - rangeRemaining;
                }
                else if (hex.CurrentMoveCost > totalRange - rangeRemaining)
                {
                    hex.CurrentMoveCost = totalRange - rangeRemaining;
                }
            }

            if (rangeRemaining > 0 &&  // Hex is valid as a pass-through hex (and there's move left to keep going through it).  
                (validFinalDest || pos == startingPos || (hex.Unit != null && hex.Unit.Owner == friendlyPlayer))
                )
            {
                // Recurse in each of the six directions.  
                RecursiveApply(map, effect, startingPos, new Point(pos.X + 1, pos.Y), totalRange, rangeRemaining - 1,
                    friendlyPlayer, ref stats);
                RecursiveApply(map, effect, startingPos, new Point(pos.X - 1, pos.Y), totalRange, rangeRemaining - 1,
                    friendlyPlayer, ref stats);
                RecursiveApply(map, effect, startingPos, new Point(pos.X + (pos.Y % 2), pos.Y + 1), totalRange, rangeRemaining - 1,
                    friendlyPlayer, ref stats);
                RecursiveApply(map, effect, startingPos, new Point(pos.X - 1 + (pos.Y % 2), pos.Y + 1), totalRange, rangeRemaining - 1,
                    friendlyPlayer, ref stats);
                RecursiveApply(map, effect, startingPos, new Point(pos.X - 1 + (pos.Y % 2), pos.Y - 1), totalRange, rangeRemaining - 1,
                    friendlyPlayer, ref stats);
                RecursiveApply(map, effect, startingPos, new Point(pos.X + (pos.Y % 2), pos.Y - 1), totalRange, rangeRemaining - 1,
                    friendlyPlayer, ref stats);
            }
            return;
        }

    }

}
