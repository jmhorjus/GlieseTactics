using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;

namespace Gliese581g
{
    /// <summary>
    /// Originally a simple class to organize three objects commonly used together:
    /// A spritebatch, a Matrix to transform it when drawn, and a list of ClickableSprite
    ///  objects that are drawn in the batch and notified with the same transform.
    ///  
    /// The screen layer is now also used to filter input, so that input meant for the top layer is not 
    /// also processed by other layers of the screen. 
    /// 
    /// If any object in a layer reports a mouse-over,
    /// then we don't pass mouse-updat info to any lower layers for that update.
    /// 
    /// Gamescreen should have a list of layers; always update them from the top layer down and
    /// always draw them from the bottom layer up.
    /// </summary>
    public class ScreenLayer
    {
        public SpriteBatch Batch;
        public Matrix Transform;
        public List<IDrawnObject> DrawnObjects;
        public Vector2 Scale;

        public ScreenLayer(GraphicsDevice graphicsDevice)
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
                SpriteSortMode.Deferred, BlendState.AlphaBlend,
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


        /// <summary>
        /// Update all the updatable objects in the list; poll them to determine if any intercepted a mouse-over.
        /// Return true if this layer intercepted the mouse-over. 
        /// </summary>
        public bool Update(MouseState mouseState, GameTime gameTime)
        {
            if (DrawnObjects == null)
                return false;

            bool retVal = false;
            try
            {
                // We only need to notify the objects that support the IUpdatedObject interface, not every object in DrawnObjects.
                foreach (IDrawnObject obj in DrawnObjects)
                {
                    IUpdatedObject updated = obj as IUpdatedObject;
                    if (updated != null)
                        retVal = updated.Update(mouseState, Transform, gameTime) || retVal;
                }
            }
            catch (InvalidOperationException)
            {
                // This exception happens if the list of clickables is modified by an action taken by a clickable.
                // in this case we immediately abort the current loop and don't worry about it since 
                // it'll all be fine by the next Update iteration.  
            }
            return retVal;
        }

    }
}
