﻿using System;
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
        void Update(MouseState mouseState, Matrix transformMatrix, GameTime time);
    }
}
