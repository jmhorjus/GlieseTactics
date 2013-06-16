using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Gliese581g
{
    /// <summary>
    /// A helper class that determines a rectangular portion of the screen that a screen takes up.  
    /// Given in percents in order to not make any assumptions about the screen resolution or shape.
    /// </summary>
    public class ScreenRectangle
    {
        public float StartPortionOfScreenX;
        public float StartPortionOfScreenY;
        public float SizePortionOfScreenX;
        public float SizePortionOfScreenY;

        public ScreenRectangle(float startPortionOfScreenX, float startPortionOfScreenY,
            float sizePortionOfScreenX, float sizePortionOfScreenY)
        {
            StartPortionOfScreenX = startPortionOfScreenX;
            StartPortionOfScreenY = startPortionOfScreenY;
            SizePortionOfScreenX = sizePortionOfScreenX;
            SizePortionOfScreenY = sizePortionOfScreenY;
        }

        public Matrix GetMatrixTransform(GraphicsDevice graphicsDevice)
        {
            float screenSizeX = graphicsDevice.Viewport.Width;
            float screenSizeY = graphicsDevice.Viewport.Height;

            return GetMatrixTransform(screenSizeX, screenSizeY);
        }
        public Matrix GetMatrixTransform(float screenSizeX, float screenSizeY)
        {
            // Start by scaling to the indicated portion.
            Matrix transform = Matrix.CreateScale(SizePortionOfScreenX, SizePortionOfScreenY, 1f);
            // Translate by the given potion of the screen size provided.
            transform = transform * Matrix.CreateTranslation(screenSizeX * StartPortionOfScreenX, screenSizeY * StartPortionOfScreenY, 0f);
            // Return the resulting matrix transform!
            return transform;
        }


        public Vector2 Scale
        { get { return new Vector2(SizePortionOfScreenX, SizePortionOfScreenY); } }


        public static ScreenRectangle WholeScreen
        {
            get { return new ScreenRectangle(0f, 0f, 1f, 1f); }
        }
    }
}
