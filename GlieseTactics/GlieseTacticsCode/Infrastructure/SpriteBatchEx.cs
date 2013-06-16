using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace Gliese581g
{
    /// <summary>
    /// A simple class to organize three objects commonly used together:
    /// A spritebatch, a Matrix to transform it when drawn, and a list of ClickableSprite
    ///  objects that are drawn in the batch and notified with the same transform.
    /// </summary>
    public class SpriteBatchEx
    {
        public SpriteBatch Batch;
        public Matrix Transform;
        public List<IDrawnObject> DrawnObjects;
        public Vector2 Scale;

        public SpriteBatchEx(GraphicsDevice graphicsDevice)
        {
            Batch      = new SpriteBatch(graphicsDevice);
            DrawnObjects = new List<IDrawnObject>();
        }

        /// <summary>
        /// A fucntion that draws a single SpriteBatchEx based on its content.
        /// </summary>
        virtual public void Draw(GameTime time)
        {
            //Start the "batch".   
            Batch.Begin(
                SpriteSortMode.BackToFront, BlendState.AlphaBlend,
                null, null, null, null,
                Transform);

            // Loop through the sprites in the list sprite.  
            foreach (IDrawnObject sprite in DrawnObjects)
            {
                sprite.Draw(Batch, time);
            }

            // Finish the batch
            Batch.End();
        }
    }
}
