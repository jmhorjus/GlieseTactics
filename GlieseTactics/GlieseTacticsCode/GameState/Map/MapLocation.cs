using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;

namespace Gliese581g
{
    public class MapLocation
    {
        public Point Location;
        public Direction Direction;

        public MapLocation(Point location, Direction direction)
        {
            Location = location;
            Direction = direction;
        }

        public void StepForward()
        { 
            Step(Direction); 
        }
        public void Step(Direction dir)
        {
            switch (dir.Value)
            {
                case Direction.ValueType.Right:
                    Location.X += 1;
                    break;
                case Direction.ValueType.DownRight:
                    Location.X += Location.Y % 2;
                    Location.Y += 1;
                    break;
                case Direction.ValueType.DownLeft:
                    Location.X -= 1 - (Location.Y % 2);
                    Location.Y += 1;
                    break;
                case Direction.ValueType.Left:
                    Location.X -= 1;
                    break;
                case Direction.ValueType.UpLeft:
                    Location.X -= 1 - (Location.Y % 2);
                    Location.Y -= 1;
                    break;
                case Direction.ValueType.UpRight:
                    Location.X += Location.Y % 2;
                    Location.Y -= 1;
                    break;
                default:
                    throw new Exception("invalid direction!");
            }

        }





    }
}
