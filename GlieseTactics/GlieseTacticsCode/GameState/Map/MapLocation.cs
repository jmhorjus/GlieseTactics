using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;

namespace Gliese581g
{
    public class MapLocation
    {
        public Point Position;
        public Direction Direction;

        public MapLocation(Point location, Direction direction)
        {
            Position = location;
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
                    Position.X += 1;
                    break;
                case Direction.ValueType.DownRight:
                    Position.X += Position.Y % 2;
                    Position.Y += 1;
                    break;
                case Direction.ValueType.DownLeft:
                    Position.X -= 1 - (Position.Y % 2);
                    Position.Y += 1;
                    break;
                case Direction.ValueType.Left:
                    Position.X -= 1;
                    break;
                case Direction.ValueType.UpLeft:
                    Position.X -= 1 - (Position.Y % 2);
                    Position.Y -= 1;
                    break;
                case Direction.ValueType.UpRight:
                    Position.X += Position.Y % 2;
                    Position.Y -= 1;
                    break;
                default:
                    throw new Exception("invalid direction!");
            }

        }





    }
}
