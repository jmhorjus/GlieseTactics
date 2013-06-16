using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gliese581g
{
    public interface IDrawnObject
    {
        void Draw(SpriteBatch spriteBatch, GameTime time);
    }
}
