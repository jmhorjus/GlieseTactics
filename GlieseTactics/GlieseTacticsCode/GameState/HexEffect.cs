using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gliese581g
{
    public abstract class HexEffect
    {
        public abstract void ApplyToHex(Hex hex);
        //public abstract void UnapplyToHex(Hex hex);
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

        public override void ApplyToHex(Hex hex)
        {
            if (hex.Unit != null)
            {
                int damage = hex.Unit.Armor.AdjustDamage(OwningUnit.MapLocation, hex.Unit.MapLocation, BaseDamage); 
                hex.Unit.CurrentHP -= damage;
                hex.CreateDrawnTextEffect(damage.ToString(), Color.Red);
                if (hex.Unit.CurrentHP <= 0)
                {
                    hex.AddUnitDeathEvent();
                }
            }
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

        public override void ApplyToHex(Hex hex)
        {
            m_map.HighlightHex(hex);
            hex.TemplateOriginHex = SetTemplateOriginHex;
        }
    }


    public class DoubleHighlightEffect : HexEffect
    {
        Map m_map;

        public DoubleHighlightEffect(Map map)
        {
            m_map = map;
        }

        public override void ApplyToHex(Hex hex)
        {
            m_map.DoubleHighlightHex(hex);
        }
    }


}
