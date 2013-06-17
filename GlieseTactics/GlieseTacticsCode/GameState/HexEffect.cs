using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gliese581g
{
    public class HexEffectStats
    {
        public int Damage = 0;
        public int Kills = 0;

        public bool IsZero() { return Damage == 0 && Kills == 0; }
        public void Clear() { Damage = 0; Kills = 0; }

        public static HexEffectStats operator +(HexEffectStats stats1, HexEffectStats stats2)
        {
            HexEffectStats statsResult = new HexEffectStats();
            statsResult.Damage = stats1.Damage + stats2.Damage;
            statsResult.Kills =  stats1.Kills + stats2.Kills;
            return statsResult;
        }
    }

    public abstract class HexEffect
    {
        public abstract HexEffectStats ApplyToHex(Hex hex);
    }



    public class UnitDamageEffect : HexEffect
    {
        public Unit OwningUnit; 
        public int BaseDamage;
        public bool ShakeScreenOnAttack;

        public UnitDamageEffect(int baseDamage, bool shakeScreen)
        {
            OwningUnit = null;
            BaseDamage = baseDamage;
            ShakeScreenOnAttack = shakeScreen;
        }

        public override HexEffectStats ApplyToHex(Hex hex)
        {
            HexEffectStats retVal = new HexEffectStats();
            if (hex.Unit != null)
            {
                int damage = hex.Unit.Armor.AdjustDamage(OwningUnit.MapLocation, hex.Unit.MapLocation, BaseDamage); 
                hex.Unit.CurrentHP -= damage;
                retVal.Damage = damage;
                hex.CreateDrawnTextEffect(damage.ToString(), Color.Red);
                if (hex.Unit.CurrentHP <= 0)
                {
                    hex.AddUnitDeathEvent();
                    retVal.Kills = 1;
                }
            }
            return retVal;
        }


    }


    public class HighlightEffect : HexEffect
    {
        Map m_map;
        public Hex SetTemplateOriginHex;

        public HighlightEffect(Map map, Hex setTemplateOriginHex = null)
        {
            m_map = map;
            SetTemplateOriginHex = setTemplateOriginHex;
        }

        public override HexEffectStats ApplyToHex(Hex hex)
        {
            m_map.HighlightHex(hex);
            hex.TemplateOriginHex = SetTemplateOriginHex;
            return new HexEffectStats();// zeroes for stats
        }
    }


    public class DoubleHighlightEffect : HexEffect
    {
        Map m_map;
        Unit m_owningUnit;

        public DoubleHighlightEffect(Map map, Unit owningUnit)
        {
            m_map = map;
            m_owningUnit = owningUnit;
        }

        public override HexEffectStats ApplyToHex(Hex hex)
        {
            m_map.DoubleHighlightHex(hex);

            HexEffectStats retVal = new HexEffectStats();
            if (hex.Unit != null)
            {
                retVal.Damage = hex.Unit.Armor.AdjustDamage(m_owningUnit.MapLocation, hex.Unit.MapLocation, m_owningUnit.AttackEffect.BaseDamage);
                if (hex.Unit.CurrentHP <= retVal.Damage)
                {
                    retVal.Kills = 1;
                }
            }
            return retVal;
        }
    }


}
