using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

using Gliese581g.ComputerPlayers;

namespace Gliese581g
{
    
    
    /// <summary>
    /// The HexEffectStats class - stats about what an effect does when applied by a template.
    /// 
    /// In order to implement AI decision-making this stats object needs to also be able to keep track of 
    /// the move that caused it somehow.  i.e. the attacking unit, move hex, and target hex...how can they be
    /// found and recorded?
    /// </summary>
    public class HexEffectStats
    {
        public List<HexEffectStats> m_lesserStatsList = new List<HexEffectStats>();
        public List<HexEffectStats> m_equalStatsList = new List<HexEffectStats>();

        // Units, locations involved in calculation of these stats.
        // Only needed during AI deliberation.
        public Unit AttackingUnit = null;
        public Hex AttackOriginHex = null;
        public Hex AttackTargetHex = null;

        // Stats about damage done, kills, etc.  
        // Used for AI dicisions and player GUI feedback.
        public int Damage = 0;
        public int CommanderDamage = 0;
        public int Kills = 0;
        public int CommanderKills = 0;
        public int FriendlyDamage = 0;
        public int FriendlyCommanderDamage = 0;
        public int FriendlyKills = 0;
        public int FriendlyCommanderKills = 0;

        public int TotalDamage { get { return Damage + FriendlyDamage; } }
        public int TotalKills { get { return Kills + FriendlyKills; } }

        public bool IsZero() 
        { 
            return Damage == 0 && Kills == 0 && 
            FriendlyDamage == 0 && FriendlyKills == 0; 
        }
        public void Clear() { Damage = 0; Kills = 0; FriendlyDamage = 0; FriendlyKills = 0; }

        // Used to accumulate stats (for instance in area-effect attacks). 
        // In this case no "lesser stats" pointer is kept, since both have been combined.
        public static HexEffectStats operator +(HexEffectStats stats1, HexEffectStats stats2)
        {
            HexEffectStats statsResult = new HexEffectStats();

            // Prefer whichever one is not null, otherwise stats1.  
            statsResult.AttackingUnit = (stats1.AttackingUnit != null) ? stats1.AttackingUnit:stats2.AttackingUnit;
            statsResult.AttackOriginHex = (stats1.AttackOriginHex != null) ? stats1.AttackOriginHex : stats2.AttackOriginHex;
            statsResult.AttackTargetHex = (stats1.AttackTargetHex != null) ? stats1.AttackTargetHex : stats2.AttackTargetHex;

            statsResult.Damage = stats1.Damage + stats2.Damage;
            statsResult.Kills =  stats1.Kills + stats2.Kills;
            statsResult.CommanderDamage = stats1.CommanderDamage + stats2.CommanderDamage;
            statsResult.CommanderKills = stats1.CommanderKills + stats2.CommanderKills;
            statsResult.FriendlyDamage = stats1.FriendlyDamage + stats2.FriendlyDamage;
            statsResult.FriendlyKills = stats1.FriendlyKills + stats2.FriendlyKills;
            statsResult.FriendlyCommanderDamage = stats1.FriendlyCommanderDamage + stats2.FriendlyCommanderDamage;
            statsResult.FriendlyCommanderKills = stats1.FriendlyCommanderKills + stats2.FriendlyCommanderKills;

            // Combine the two "lesser" and "equal" lists? Likely not neccessary.

            return statsResult;
        }

        // Finds the best outcome for each catagory.  
        // We're violating the two stats identities by combining them this way, so no
        // additions to the lesser/equal lists are made. 
        public static HexEffectStats BestByCatagory(HexEffectStats stats1, HexEffectStats stats2)
        {
            HexEffectStats statsResult = new HexEffectStats();
            statsResult.Damage = Math.Max(stats1.Damage, stats2.Damage);
            statsResult.Kills = Math.Max(stats1.Kills, stats2.Kills);
            statsResult.CommanderDamage = Math.Max(stats1.CommanderDamage, stats2.CommanderDamage);
            statsResult.CommanderKills = Math.Max(stats1.CommanderKills, stats2.CommanderKills);

            statsResult.FriendlyDamage = Math.Min(stats1.FriendlyDamage, stats2.FriendlyDamage);
            statsResult.FriendlyKills = Math.Min(stats1.FriendlyKills, stats2.FriendlyKills);
            statsResult.FriendlyCommanderDamage = Math.Min(stats1.FriendlyCommanderDamage, stats2.FriendlyCommanderDamage);
            statsResult.FriendlyCommanderKills = Math.Min(stats1.FriendlyCommanderKills, stats2.FriendlyCommanderKills);
            return statsResult; 
        }

        // Return the move defined as best by the given priorities.
        // The lesser ranked move is kept as a member of the greater ranked move.
        public static HexEffectStats BestSingleMove(HexEffectStats stats1, HexEffectStats stats2, 
            HexEffectPriorities priorities)
        {
            int weight1 = priorities.GetEffectValue(stats1);
            int weight2 = priorities.GetEffectValue(stats2);

            if (weight1 > weight2)
            {
                stats1.m_lesserStatsList.Add(stats2);
                return stats1;
            }
            if (weight2 > weight1)
            {
                stats2.m_lesserStatsList.Add(stats1);
                return stats2;
            }
            // they are equal - combine their lists into one.
            stats2.m_equalStatsList.Concat(stats1.m_equalStatsList);
            stats1.m_equalStatsList.Clear();

            stats2.m_lesserStatsList.Concat(stats1.m_lesserStatsList);
            stats1.m_lesserStatsList.Clear();
            
            stats2.m_equalStatsList.Add(stats1);
            return stats2;
        }
        // Get the total number of lesser options, recursively. 
        public int GetTotalMovesContained()
        {
            int retVal = 1;

            foreach (HexEffectStats stats in m_equalStatsList)
            {
                retVal += stats.GetTotalMovesContained();
            }

            foreach (HexEffectStats stats in m_lesserStatsList)
            {
                retVal += stats.GetTotalMovesContained();
            }

            return retVal;
        }
    }



    /// <summary>
    /// An effect that damages any unit in the hex.
    /// </summary>
    [Serializable]
    public class UnitDamageEffect : HexEffect
    {
        public Unit OwningUnit; 
        public int BaseDamage;
        public bool ShakeScreenOnAttack;

        private UnitDamageEffect() { }

        public UnitDamageEffect(int baseDamage, bool shakeScreen)
        {
            OwningUnit = null;
            BaseDamage = baseDamage;
            ShakeScreenOnAttack = shakeScreen;
        }

        public static HexEffectStats CalculateDamageStats(Unit attacker, Hex attackSourceHex, Unit defender)
        {
            HexEffectStats retVal = new HexEffectStats();

            if (defender != null)
            {
                // Record the units/hexes involved in this calculation.
                retVal.AttackingUnit = attacker;
                retVal.AttackOriginHex = attackSourceHex;
                retVal.AttackTargetHex = defender.CurrentHex;

                int damage = defender.Armor.AdjustDamage(
                    attackSourceHex.MapPosition, 
                    defender.MapLocation, 
                    attacker.AttackEffect.BaseDamage);

                // Limit the damage done to the units remaining HP.
                // Helps AI decision making, but also effects damage 
                // displayed in the GUI.
                damage = Math.Min(damage, defender.CurrentHP);

                if (attacker.Owner == defender.Owner)
                {
                    retVal.FriendlyDamage = damage;
                    if (defender.IsCommander)
                        retVal.FriendlyCommanderDamage = damage;
                }
                else
                {
                    retVal.Damage = damage;
                    if (defender.IsCommander)
                        retVal.CommanderDamage = damage;
                }

                if (defender.CurrentHP <= damage)
                {
                    if (attacker.Owner == defender.Owner)
                    {
                        retVal.FriendlyKills = 1;
                        if (defender.IsCommander)
                            retVal.FriendlyCommanderKills = 1;
                    }
                    else
                    {
                        retVal.Kills = 1;
                        if (defender.IsCommander)
                            retVal.CommanderKills = 1;
                    }
                }
            }
            return retVal;
        }

        public override HexEffectStats ApplyToHex(Hex hex, Direction templateDirection, Hex effectSourceHex)
        {
            HexEffectStats retVal = CalculateDamageStats(OwningUnit, effectSourceHex, hex.Unit);
                
            if (hex.Unit != null)
            {
                hex.Unit.CurrentHP -= retVal.TotalDamage;
                hex.CreateDrawnTextEffect(retVal.TotalDamage.ToString(), Color.Red);
                if (retVal.TotalKills > 0)
                    hex.AddUnitDeathEvent();
            }
            return retVal;
        }


    }


    /// <summary>
    /// An effect that highlights the hex on the map.
    /// </summary>
    public class HighlightEffect : HexEffect
    {
        Map m_map;
        public Hex SetTemplateOriginHex;

        public HighlightEffect(Map map, Hex setTemplateOriginHex = null)
        {
            m_map = map;
            SetTemplateOriginHex = setTemplateOriginHex;
        }

        public override HexEffectStats ApplyToHex(Hex hex, Direction templateDirection, Hex effectSourceHex)
        {
            m_map.HighlightHex(hex);
            hex.TemplateOriginHex = SetTemplateOriginHex;
            return new HexEffectStats();// zeroes for stats
        }
    }

    
    // TODO: Try something like this. 
    // public delegate HexEffectStats CombineStatsType(HexEffectStats stats1, HexEffectStats stats2, HexEffectPriorities priorities = null);

    /// <summary>
    /// The HexEffect interface - defines the ApplyToHex function. (All HexEffects can be applied to a hex.)
    /// </summary>
    public abstract class HexEffect
    {
        public abstract HexEffectStats ApplyToHex(Hex hex, Direction templateDirection, Hex effectSourceHex);

        // TODO: Try something like this.
        //public CombineStatsType StatsCombiner = HexEffectStats.BestSingleMove;
    }

    

    /// <summary>
    /// Used by AI to inspect possibile outcomes of moves involving the application of map templates. 
    /// </summary>
    public class ExpectedDamageHexEffect : HexEffect
    {
        Map m_map;
        Unit m_owningUnit;

        public ExpectedDamageHexEffect(Map map, Unit owningUnit)
        {
            m_map = map;
            m_owningUnit = owningUnit;
        }

        public override HexEffectStats ApplyToHex(Hex hex, Direction templateDirection, Hex effectSourceHex)
        {
            return UnitDamageEffect.CalculateDamageStats(m_owningUnit, effectSourceHex, hex.Unit);
        }
    }


    /// <summary>
    /// An effect that double-highlights the hex on the map (i.e. for highlighting 
    /// attak areas when a target area is already highlighted). 
    /// </summary>
    public class DoubleHighlightEffect : HexEffect
    {
        Map m_map;
        Unit m_owningUnit;

        public DoubleHighlightEffect(Map map, Unit owningUnit)
        {
            m_map = map;
            m_owningUnit = owningUnit;
        }

        public override HexEffectStats ApplyToHex(Hex hex, Direction templateDirection, Hex effectSourceHex)
        {
            m_map.DoubleHighlightHex(hex);
            // Does calculations as though it were a unit-damage effect - so expected damage can be displayed in GUI. 
            return UnitDamageEffect.CalculateDamageStats(m_owningUnit, effectSourceHex, hex.Unit);
        }

    }



    /// <summary>
    /// An effect that recursively applies another effect based on a template starting at this hex. 
    /// </summary>
    public class RecursiveTemplateEffect : HexEffect
    {
        Map m_map;
        MapTemplate m_subTemplate;
        HexEffect m_subTemplateEffect;
        bool m_allDirections;
        bool m_redefineEffectSourceHex;

        // TODO: Implement a slick, general case solution to specifying the 
        // recursive template stats accumulation/combination method.  Maybe
        // have them pass in a function pointer (Delegate) to a function that 
        // takes two stats and returns one. 
        bool m_returnMaxStats;
        bool m_returnBestMove;
        HexEffectPriorities m_bestMovePriorities;
        


        public RecursiveTemplateEffect(
            Map map, 
            MapTemplate subTemplate, 
            bool allDirections,
            bool redefineEffectSourceHex,
            HexEffect subTemplateEffect,
            bool returnMaxStats = true,
            HexEffectPriorities bestMovePriorities = null)
        {
            m_map = map;
            m_subTemplate = subTemplate;
            m_subTemplateEffect = subTemplateEffect;
            m_allDirections = allDirections;
            m_redefineEffectSourceHex = redefineEffectSourceHex;
            m_returnMaxStats = returnMaxStats;

            if (bestMovePriorities != null)
            {
                m_returnBestMove = true;
                m_returnMaxStats = false;
                m_bestMovePriorities = bestMovePriorities;
            }
        }

        public override HexEffectStats ApplyToHex(
            Hex hex, Direction templateDirection, 
            Hex effectOriginHex)
        {
            Direction currentDirection = templateDirection;
            HexEffectStats retVal = new HexEffectStats();
            // loop through to apply the sub-template in all 6 directions (if configured to)
            for (int ii = 0; ii < (int)Direction.ValueType.NumDirections; ii++)
            {
                MapLocation loc = new MapLocation(hex.MapPosition, currentDirection);
                
                HexEffectStats stats = m_subTemplate.OnApply(
                    m_map,
                    loc,
                    m_subTemplateEffect,
                    m_redefineEffectSourceHex ? hex : effectOriginHex, 
                    m_bestMovePriorities);

                if (m_returnMaxStats)
                    retVal = HexEffectStats.BestByCatagory(stats, retVal);
                else if (m_returnBestMove)
                    retVal = HexEffectStats.BestSingleMove(stats, retVal, m_bestMovePriorities);
                else
                {
                    retVal += stats;
                    // HACK. Because this case applies to attack templates whose effects are summed
                    // rather than being compared by value or maximized catagorically, the AI will want to 
                    // target the actual HEX the effect is being applied to, rather than any of the units damaged.
                    retVal.AttackTargetHex = hex; 
                }

                // If we're not actually doing all the directions, break out here.
                if (!m_allDirections)
                    break;

                currentDirection++;
            }
            return retVal;
        }

    }


}
