using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gliese581g
{

    /// <summary>
    /// The HexEffectStats class - stats about what an effect does when applied by a template.
    /// </summary>
    public class HexEffectStats
    {
        public int Damage = 0;
        public int Kills = 0;
        public int FriendlyDamage = 0;
        public int FriendlyKills = 0;

        public int TotalDamage { get { return Damage + FriendlyDamage; } }
        public int TotalKills { get { return Kills + FriendlyKills; } }

        public bool IsZero() 
        { 
            return Damage == 0 && Kills == 0 && 
            FriendlyDamage == 0 && FriendlyKills == 0; 
        }
        public void Clear() { Damage = 0; Kills = 0; FriendlyDamage = 0; FriendlyKills = 0; }

        public static HexEffectStats operator +(HexEffectStats stats1, HexEffectStats stats2)
        {
            HexEffectStats statsResult = new HexEffectStats();
            statsResult.Damage = stats1.Damage + stats2.Damage;
            statsResult.Kills =  stats1.Kills + stats2.Kills;
            statsResult.FriendlyDamage = stats1.FriendlyDamage + stats2.FriendlyDamage;
            statsResult.FriendlyKills = stats1.FriendlyKills + stats2.FriendlyKills;
            return statsResult;
        }
        public static HexEffectStats Best(HexEffectStats stats1, HexEffectStats stats2)
        {
            HexEffectStats statsResult = new HexEffectStats();
            statsResult.Damage = Math.Max(stats1.Damage, stats2.Damage);
            statsResult.Kills = Math.Max(stats1.Kills, stats2.Kills);
            statsResult.FriendlyDamage = Math.Min(stats1.FriendlyDamage, stats2.FriendlyDamage);
            statsResult.FriendlyKills = Math.Min(stats1.FriendlyKills, stats2.FriendlyKills);
            return statsResult; 
        }
    }


    /// <summary>
    /// The HexEffect interface - defines the ApplyToHex function. (All HexEffects can be applied to a hex.)
    /// </summary>
    public abstract class HexEffect
    {
        public abstract HexEffectStats ApplyToHex(Hex hex, Direction templateDirection, Hex effectSourceHex);
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
                int damage = defender.Armor.AdjustDamage(
                    attackSourceHex.MapPosition, 
                    defender.MapLocation, 
                    attacker.AttackEffect.BaseDamage);
                
                if (attacker.Owner == defender.Owner)
                    retVal.FriendlyDamage = damage;
                else
                    retVal.Damage = damage;

                if (defender.CurrentHP <= damage)
                {
                    if (attacker.Owner == defender.Owner)
                        retVal.FriendlyKills = 1;
                    else
                        retVal.Kills = 1;
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


    /// <summary>
    /// An effect that double-highlights the hex on the map (for highlighting 
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
        bool m_returnMaxStats;


        public RecursiveTemplateEffect(
            Map map, 
            MapTemplate subTemplate, 
            bool allDirections,
            bool redefineEffectSourceHex,
            HexEffect subTemplateEffect,
            bool returnMaxStats = true)
        {
            m_map = map;
            m_subTemplate = subTemplate;
            m_subTemplateEffect = subTemplateEffect;
            m_allDirections = allDirections;
            m_redefineEffectSourceHex = redefineEffectSourceHex;
            m_returnMaxStats = returnMaxStats;
        }

        public override HexEffectStats ApplyToHex(Hex hex, Direction templateDirection, Hex effectOriginHex)
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
                    m_redefineEffectSourceHex ? hex : effectOriginHex);

                if (!m_returnMaxStats)
                    retVal += stats;
                else
                    retVal = HexEffectStats.Best(stats, retVal);

                if (!m_allDirections)
                    break;
                currentDirection++;
            }
            return retVal;
        }




    }




}
