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
        NegAB = 0,
        NegAB_NegB = 1,
        NegB = 2,
        NegB_PosA = 3,
        PosA = 4,
        PosA_PosAB = 5,
        PosAB = 6,
        PosAB_PosB = 7,
        PosB = 8,
        PosB_NegA = 9,
        NegA = 10,
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

    }
    



}
