using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Gliese581g
{
    public class ExplosionEffect : IDrawnEffect
    {
        public Texture2D texture;
        public Vector2 position;
        public float timer;
        public float interval;
        public Vector2 origin;
        public int currentFrame;
        public int spriteWidth; 
        public int spriteHeight;
        public Rectangle soureceRect;
        public bool isVisible;
        bool m_isFinished;
        public bool IsFinished() { return m_isFinished; }

        //Constructor
        public ExplosionEffect(Vector2 new_position)
        {
            position = new_position + new Vector2(-15f,0f);
            texture = TextureStore.Get(TexId.hex_explosion);
            timer = 0f;
            interval = 30f;
            currentFrame = 1;
            spriteWidth = 117;
            spriteHeight = 128;
            isVisible = true;
        }


        public void Draw(SpriteBatch spriteBatch, GameTime time)
        {
            if (m_isFinished)
                return;

            // In the explosion image, only width change, Hight does not change
            soureceRect = new Rectangle(currentFrame * spriteWidth, 0, spriteWidth, spriteHeight);

            origin = new Vector2(soureceRect.Width / 2, soureceRect.Height / 2);

            if (isVisible)
            {
                spriteBatch.Draw(texture, position, soureceRect, Color.White, 0f, origin,1.0f, SpriteEffects.None, 0);
            }


            // Increase the timer
            timer += (float)time.ElapsedGameTime.TotalMilliseconds;

            if (timer > interval)
            {
                currentFrame++; // show the next frame
                timer = 0f;     // Reset Timer
            }

            // if on the last frame, back to the first frame
            if (currentFrame >= 17)
            {
                isVisible = false;
                m_isFinished = true;
                currentFrame = 0;
            }

        }

    }
}
