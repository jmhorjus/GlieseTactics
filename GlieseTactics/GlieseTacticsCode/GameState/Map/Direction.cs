using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;

namespace Gliese581g
{
    /// <summary>
    /// This class is used to manage referances to direction which units may be facting on the board
    /// and in which they might move.  It should primarily be used as a component of a location.  
    /// </summary>
    public enum ImpactAngle
    {
        // ImpactAngle must match Direction.ValueType, such that
        // Direction.ValueType * 2 == the proper angle.
        // There are also six possible "tied" angles, which are used
        // when two hexes are equally distant on 2 of the 3 "hex derection" axis. 
        NegAB = 0,      // Right
        NegAB_NegB = 1,
        NegB = 2,       // DownRight
        NegB_PosA = 3,
        PosA = 4,       // DownLeft
        PosA_PosAB = 5,
        PosAB = 6,      // Left
        PosAB_PosB = 7,
        PosB = 8,       // UpLeft
        PosB_NegA = 9,
        NegA = 10,      // UpRight
        NegA_NegAB = 11
    }
 
    [Serializable]
    public class Direction
    {
        public enum ValueType
        {
            Right = 0,
            DownRight = 1,
            DownLeft = 2,
            Left = 3,
            UpLeft = 4,
            UpRight = 5,
            NumDirections = 6
        }

        private int m_value;

        public ValueType Value
        {
            get { return (ValueType)m_value; }
            set { m_value = (int)value; }
        }

        public Direction() { } // for serialization
        public Direction(ValueType direction)
        { m_value = (int)direction; }
        public Direction(int direction)
        { m_value = direction; }
        public Direction(ImpactAngle angle, bool roundClockwise = false)
        {
            // Divide by two to get the right direction.  
            // By deafult will round ties "down" (i.e. it will round them
            // counter-clockwise). Adding a rounding value of 1 will cause 
            // ties to be rounded to the clockwise side instead.  
            int roundingValue = roundClockwise ? 1 : 0;
            m_value = ((int)angle + roundingValue) / 2; 
        }

        public static Direction operator ++(Direction dir)
        {
            dir.m_value = (dir.m_value + 1) % (int)ValueType.NumDirections;
            return dir;
        }
        public static Direction operator --(Direction dir)
        {
            dir.m_value = (dir.m_value - 1) % (int)ValueType.NumDirections;
            return dir;
        }

        public static bool operator ==(Direction dir1, Direction dir2)
        {
            if ((Object)dir1 == null && (Object)dir2 == null)
                return true;
            if ((Object)dir1 == null || (Object)dir2 == null)
                return false;

            return dir1.m_value == dir2.m_value;
        }
        public static bool operator !=(Direction dir1, Direction dir2)
        {
            return dir1.m_value != dir2.m_value;
        }
        public static explicit operator int(Direction dir)
        {
            return dir.m_value;
        }
        public static explicit operator Direction.ValueType(Direction dir)
        {
            return dir.Value;
        }

        // These two must be overridden if you defind == and != operators. 
        public override bool Equals(object obj)
        {
            return this.m_value == (obj as Direction).m_value;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        // some convenient fixed direction definitions.  
        public static readonly Direction Right = new Direction(ValueType.Right);
        public static readonly Direction DownRight = new Direction(ValueType.DownRight);
        public static readonly Direction DownLeft = new Direction(ValueType.DownLeft);
        public static readonly Direction Left = new Direction(ValueType.Left);
        public static readonly Direction UpLeft = new Direction(ValueType.UpLeft);
        public static readonly Direction UpRight = new Direction(ValueType.UpRight);



        /// <summary>
        /// Get the prominant direction you'd have to go from startingHex to get to endingHex.
        /// </summary>
        static public Direction GetDirectionFromHex(Hex startingHex, Hex endingHex)
        {
            ImpactAngle angle = GetImpactAngle(startingHex.MapPosition, endingHex.MapPosition);

            return new Direction(angle);
        }
        /// <summary>
        /// Get the impact angle between two points on the map (map coordinates).  
        /// </summary>
        static public ImpactAngle GetImpactAngle(Point startingMapPoint, Point endingMapPoint)
        {
            ImpactAngle impactAngle;
            // Determine the most prominent direction by finding distance in the 3 directions.  
            int a = (startingMapPoint.X - (startingMapPoint.Y / 2)) -
                (endingMapPoint.X - (endingMapPoint.Y / 2));
            int b = (startingMapPoint.X + ((startingMapPoint.Y + 1) / 2)) -
                (endingMapPoint.X + ((endingMapPoint.Y + 1) / 2));
            int ab = 0;
            if ((a > 0) == (b > 0))
            {
                if (Math.Abs(a) < Math.Abs(b))
                    ab = a;
                else
                    ab = b;
                a -= ab;
                b -= ab;
            }
            // at least 1 zero now. There are 6 possible directions and 6 possible ties (12 cases total).  
            if (Math.Abs(a) >= Math.Max(Math.Abs(b), Math.Abs(ab)))
            { //the a direction dominates or ties (six cases)
                if (a > 0)  // three cases
                    if (a > Math.Max(-b, ab))
                        impactAngle = ImpactAngle.PosA; //+a;
                    else if (-b > ab)
                        impactAngle = ImpactAngle.NegB_PosA; // +a ties -b;
                    else
                        impactAngle = ImpactAngle.PosA_PosAB; // +a ties +ab;
                else   // other 3 cases
                    if (-a > Math.Max(b, -ab))
                        impactAngle = ImpactAngle.NegA; // -a;
                    else if (b > -ab)
                        impactAngle = ImpactAngle.PosB_NegA; // -a ties +b;
                    else
                        impactAngle = ImpactAngle.NegA_NegAB; // -a ties -ab;
            }
            else if (Math.Abs(b) >= Math.Max(Math.Abs(a), Math.Abs(ab)))
            { //the b direction dominates or ties ab (four cases)
                if (b > 0) //two cases
                    if (b > ab)
                        impactAngle = ImpactAngle.PosB; // +b;
                    else
                        impactAngle = ImpactAngle.PosAB_PosB; // +b ties +ab;
                else
                    if (-b > -ab)
                        impactAngle = ImpactAngle.NegB; // -b;
                    else
                        impactAngle = ImpactAngle.NegAB_NegB; // -b ties -ab;
            }
            else //abs(ab) is the dominant axis (two cases) {
                if (ab > 0)
                    impactAngle = ImpactAngle.PosAB; //+ab // right
                else
                    impactAngle = ImpactAngle.NegAB; //-ab // left

            return impactAngle;
        }

        /// <summary>
        /// Get the direction from a certain hex to a certain "point" anywhere in space (i.e. often the mouse cursor).
        /// </summary>
        static public Direction GetDirectionFromHex(Hex centerHex, Point transformedPoint)
        {
            Direction retVal = null;
            // Calculate the direction
            Point center = centerHex.DisplayRect.Center;
            int direction = 0;
            direction += (transformedPoint.X > center.X) ? 1 : 0;
            direction += (transformedPoint.Y > center.Y + ((transformedPoint.X - center.X) / 2)) ? 2 : 0;
            direction += (transformedPoint.Y > center.Y - ((transformedPoint.X - center.X) / 2)) ? 4 : 0;
            switch (direction)
            {
                case 5:
                    retVal = Direction.Right;
                    break;
                case 1:
                    retVal = Direction.UpRight;
                    break;
                case 0:
                    retVal = Direction.UpLeft;
                    break;
                case 2:
                    retVal = Direction.Left;
                    break;
                case 6:
                    retVal = Direction.DownLeft;
                    break;
                case 7:
                    retVal = Direction.DownRight;
                    break;
                default:
                    throw new Exception("This should never happen.");
            }

            return retVal;
        }

        public static Vector2 NudgeVectorInDirection(Vector2 vector, Direction direction)
        {
            if (direction == null)
                return vector;

            switch (direction.m_value)
            {
                case (int)ValueType.Right:
                    return vector + new Vector2(0, -2);
                case (int)ValueType.DownRight:
                    return vector + new Vector2(-2, -2);
                case (int)ValueType.DownLeft:
                    return vector + new Vector2(-2, 2);
                case (int)ValueType.Left:
                    return vector + new Vector2(0, 2);
                case (int)ValueType.UpLeft:
                    return vector + new Vector2(2, 2);
                case (int)ValueType.UpRight:
                    return vector + new Vector2(2, -2);
            }
            return vector;
        }

    }
    



}
