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

        protected Armor(int front, int frontFlank, int rearFlank, int rear)
        {
            m_values = new int[4]{rear,rearFlank,frontFlank,front};
        }
       
        public int Value(ArmorZone zone)
        {
            return m_values[(int)zone];
        }

        public int AdjustDamage(MapLocation damageLocation, MapLocation targetLocation, int damage)
        {
            ImpactAngle impactAngle;
            // Determine the most prominent direction by finding distance in the 3 directions.  
            int a = (targetLocation.Location.X-(targetLocation.Location.Y/2)) - 
                (damageLocation.Location.X-(damageLocation.Location.Y/2));
            int b = (targetLocation.Location.X+((targetLocation.Location.Y+1)/2)) - 
                (damageLocation.Location.X+((damageLocation.Location.Y+1)/2));
            int ab = 0;
            if ((a > 0) == (b > 0)){
                if (Math.Abs(a) < Math.Abs(b))
                    ab = a;
                else
                    ab = b;
                a -= ab;
                b -= ab;
            }
            // at least 1 zero now. There are 6 possible directions and 6 possible ties (12 cases total).  
            if (Math.Abs(a) >= Math.Max(Math.Abs(b), Math.Abs(ab))  ) { //the a direction dominates or ties (six cases)
                if (a > 0)  // three cases
                    if(a > Math.Max(-b,ab) )
                        impactAngle = ImpactAngle.PosA; //+a;
                    else if (-b > ab)
                        impactAngle = ImpactAngle.NegB_PosA; // +a ties -b;
                    else 
                        impactAngle = ImpactAngle.PosA_PosAB; // +a ties +ab;
                else   // other 3 cases
                    if(-a > Math.Max(b,-ab) )
                        impactAngle = ImpactAngle.NegA; // -a;
                    else if (b > -ab)
                        impactAngle = ImpactAngle.PosB_NegA; // -a ties +b;
                    else 
                        impactAngle = ImpactAngle.NegA_NegAB; // -a ties -ab;
            }
            else if (  Math.Abs(b) >= Math.Max(Math.Abs(a), Math.Abs(ab))  ) { //the b direction dominates or ties ab (four cases)
                if ( b > 0 ) //two cases
                    if ( b > ab )
                        impactAngle = ImpactAngle.PosB; // +b;
                    else 
                        impactAngle = ImpactAngle.PosAB_PosB; // +b ties +ab;
                else
                    if ( -b > -ab )
                        impactAngle = ImpactAngle.NegB; // -b;
                    else 
                        impactAngle = ImpactAngle.NegAB_NegB; // -b ties -ab;
            }
            else //abs(ab) is the dominant axis (two cases) {
                if ( ab > 0)
                    impactAngle = ImpactAngle.PosAB; //+ab // right
                else 
                    impactAngle = ImpactAngle.NegAB; //-ab // left

            // Now that we have a direction, we combine it with the direction the target is facing to find one of four impact zones.  
            // First get the direction offset: "*2" because of the relationship between direction and ImpactAngle. 
            int directionOffset = Math.Abs(((int)targetLocation.Direction * 2) - (int)impactAngle);
            // -6 because it's reletive to a half circle.  +1 so that we round in favor of the defender.
            ArmorZone zone = (ArmorZone) ((Math.Abs((directionOffset - 6))+1) / 2);  // integers round down to the less armored zone.

            if (zone >= ArmorZone.NumberOfZones)
                throw new Exception("something wrong with direction offset??");

            return (damage * (100-Value(zone))) / 100;
        }
    }
}
