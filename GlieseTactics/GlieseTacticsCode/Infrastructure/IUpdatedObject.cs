using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Gliese581g
{
    public interface IUpdatedObject
    {
        // Returns true if the updated element detected a mouse-over.
        bool Update(MouseState mouseState, Matrix transformMatrix, GameTime time);
    }
}
