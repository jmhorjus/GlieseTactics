using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Gliese581g
{
    // Impact Zones: 0=rear, 1=rearFlank, 2=frontFlank, 3=front
    public enum ArmorZone
    {
        Rear = 0,
        RearFlank = 1,
        FrontFlank = 2,
        Front = 3,
        NumberOfZones = 4
    }

    public enum ArmorType
    {
        Heavy,
        Medium,
        Light,
        None
    }

    [Serializable]
    public class Armor
    {
        public class ArmorFactory
        {
            public static Armor MakeArmor(ArmorType type)
            {
                switch(type)
                {
                    case ArmorType.Heavy:
                        return new Armor(60,40,20,0);
                    case ArmorType.Medium:
                        return new Armor(40,20,20,0);
                    case ArmorType.Light:
                        return new Armor(20,20,0,0);
                    case ArmorType.None:
                        return new Armor(0,0,0,0);
                    default:
                        throw new Exception("invalid armor type");
                }
            }
        }


        private int[] m_values;

        private Armor() { } // for serialization

        protected Armor(int front, int frontFlank, int rearFlank, int rear)
        {
            m_values = new int[4]{rear,rearFlank,frontFlank,front};
        }
       
        public int Value(ArmorZone zone)
        {
            return m_values[(int)zone];
        }

        public int AdjustDamage(Point damageSource, MapLocation targetLocation, int damage)
        {
            ImpactAngle impactAngle = Direction.GetImpactAngle(targetLocation.Position, damageSource);

            // Now that we have a direction, we combine it with the direction the target is facing to find one of four impact zones.  
            // First get the direction offset: "*2" because of the relationship between direction and ImpactAngle. 
            int directionOffset = Math.Abs(((int)targetLocation.Direction * 2) - (int)impactAngle);
            // -6 because armor zone is reletive to a half circle(front to back). +1 so that we round in favor of the defender.
            ArmorZone zone = (ArmorZone) ((Math.Abs((directionOffset - 6))+1) / 2);  // integers round down to the less armored zone.

            if (zone >= ArmorZone.NumberOfZones)
                throw new Exception("something wrong with direction offset??");

            return (damage * (100-Value(zone))) / 100;
        }
    }
}
