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
        public abstract HexEffectStats OnApply(Map map, MapLocation location, HexEffect effect, Hex effectSourceHex, out Hex onlyOneHex);

        //public abstract HexEffectStats OnApply(Map map, MapLocation location, HexEffect effect, Hex effectOriginHex);
        public virtual HexEffectStats OnApply(Map map, MapLocation location, HexEffect effect, Hex effectSourceHex)
        {
            return OnApply(map, location, effect, effectSourceHex, out nullOutputDestination);
        }
        
        protected bool m_returnMaxStats = false;
        protected HexEffectStats AccumulateStats(HexEffectStats stats1, HexEffectStats stats2)
        {
            if (m_returnMaxStats)
                return HexEffectStats.BestByCatagory(stats1, stats2);
            else
                return (stats1 + stats2);
        }
    }


    public class LineTemplate : MapTemplate
    {
        int m_length;
        bool m_includeSourceHex;
        bool m_stopAtUnit;
        bool m_stopAtBlockingTerrain;

        public LineTemplate(int length, bool stopAtUnit, bool stopAtBlockingTerrain,
            bool includeSourceHex, bool returnMaxStats = false)
        {
            m_length = length;
            m_stopAtUnit = stopAtUnit;
            m_stopAtBlockingTerrain = stopAtBlockingTerrain;
            m_includeSourceHex = includeSourceHex;
            m_returnMaxStats = returnMaxStats;
        }


        public override HexEffectStats OnApply(Map map, MapLocation location, HexEffect effect, Hex effectSourceHex, out Hex onlyOneHex)
        {
            HexEffectStats retVal = new HexEffectStats();
            onlyOneHex = null;
            bool isFirstHex = true;
            Hex sourceHex =
                effectSourceHex != null ? effectSourceHex : map.GetHex(location.Position);

            for (int ii = 0; ii < m_length; ii++)
            {
                if (ii == 0 && !m_includeSourceHex)
                {   // Special case for m_includeSourceHex.
                    location.StepForward();
                    continue;
                }

                Hex hex = map.GetHex(location.Position);
                if (hex == null)
                    break;

                bool doneAfterThisHex = ii + 1 >= m_length ||
                    (m_stopAtUnit && (hex.Unit != null)) ||
                    (m_stopAtBlockingTerrain && !map.GetHex(location.Position).LandMovementAllowed);

                if (isFirstHex && doneAfterThisHex)
                    onlyOneHex = hex;
                    
                // Apply the effect.
                retVal = AccumulateStats(retVal, effect.ApplyToHex(hex, location.Direction, sourceHex));
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
            Unit unitWhoseAlliesDontBlockMovement = null,
            bool returnMaxStats = false)

        {
            m_range = range;
            m_pathThroughBlockHexes = pathThroughBlockHexes;
            m_pathThroughOtherUnits = pathThroughOtherUnits;
            m_includeSourceHex = includeSourceHex;
            m_unitWhoseAlliesDontBlockMovement = unitWhoseAlliesDontBlockMovement;
            m_returnMaxStats = returnMaxStats;
        }


        // The public OnApply
        public override HexEffectStats OnApply(Map map, MapLocation location, HexEffect effect, Hex effectSourceHex, out Hex onlyOneHex)
        {
            HexEffectStats retVal = new HexEffectStats();
            // Ranged template never only one hex
            onlyOneHex = null;

            map.ClearMarkedHexes(this); // We use hex marking to ensure we don't double-apply to some hexes.
            RecursiveApply(
                map,
                effect,
                effectSourceHex, 
                location.Position, location.Position,
                m_range, m_range,
                (m_unitWhoseAlliesDontBlockMovement != null) ? m_unitWhoseAlliesDontBlockMovement.Owner : null,
                ref retVal);
            return retVal;
        }
 
        // This one is private and is called recursively. 
        void RecursiveApply(
            Map map,
            HexEffect effect,
            Hex effectSourceHex,
            Point startingPos,
            Point pos,
            int totalRange,
            int rangeRemaining,
            Commander friendlyPlayer, // the units of this player are friendly and can be moved through.
            ref HexEffectStats stats
            )
        {
            Hex hex = map.GetHex(pos);
            if (hex == null)
                return;

            bool validFinalDest = (((hex.LandMovementAllowed || m_pathThroughBlockHexes) &&
                (hex.Unit == null || m_pathThroughOtherUnits)) ||  // Hex is a valid final destination.
                ((pos == startingPos) && m_includeSourceHex));

            if (validFinalDest)
            {
                if (!hex.IsMarked.ContainsKey(this) && (pos != startingPos || m_includeSourceHex))
                {
                    map.MarkHex(this, hex);

                    stats = AccumulateStats (stats, 
                        effect.ApplyToHex(
                            hex,
                            new Direction(0), // direction is arbitrary in this context!
                            effectSourceHex != null ? effectSourceHex : map.GetHex(startingPos))); 
                    
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
                RecursiveApply(map, effect, effectSourceHex, startingPos, new Point(pos.X + 1, pos.Y), totalRange, rangeRemaining - 1,
                    friendlyPlayer, ref stats);
                RecursiveApply(map, effect, effectSourceHex, startingPos, new Point(pos.X - 1, pos.Y), totalRange, rangeRemaining - 1,
                    friendlyPlayer, ref stats);
                RecursiveApply(map, effect, effectSourceHex, startingPos, new Point(pos.X + (pos.Y % 2), pos.Y + 1), totalRange, rangeRemaining - 1,
                    friendlyPlayer, ref stats);
                RecursiveApply(map, effect, effectSourceHex, startingPos, new Point(pos.X - 1 + (pos.Y % 2), pos.Y + 1), totalRange, rangeRemaining - 1,
                    friendlyPlayer, ref stats);
                RecursiveApply(map, effect, effectSourceHex, startingPos, new Point(pos.X - 1 + (pos.Y % 2), pos.Y - 1), totalRange, rangeRemaining - 1,
                    friendlyPlayer, ref stats);
                RecursiveApply(map, effect, effectSourceHex, startingPos, new Point(pos.X + (pos.Y % 2), pos.Y - 1), totalRange, rangeRemaining - 1,
                    friendlyPlayer, ref stats);
            }
            return;
        }

    }

}
