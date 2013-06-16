using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace Gliese581g
{
    public class Animation
    {
        TimeSpan m_totalDuration;
        TimeSpan m_elapsedTime;
        //bool m_isStarted;
        bool m_isFinished;
        public bool Finished { get { return m_isFinished; } }

        bool m_stretchToRectangle;

        Vector2 m_finalPos;
        Vector2 m_finalScale;

        Rectangle m_finalRectangle;

        public Animation(TimeSpan duration, Vector2 finalPos, Vector2 finalScale)
        {
            m_totalDuration = duration;
            m_elapsedTime = TimeSpan.Zero;

            m_stretchToRectangle = false;
            m_finalPos = finalPos;
            m_finalScale = finalScale;
        }

        public Animation(TimeSpan duration, Rectangle finalRectangle)
        {
            m_totalDuration = duration;
            m_elapsedTime = TimeSpan.Zero;

            m_stretchToRectangle = true;
            m_finalRectangle = finalRectangle; 
        }


        public void Animate(ClickableSprite sprite, GameTime time)
        {
            //The portion of the rest of the way toward the end of the 
            float progress;
            if (m_totalDuration > m_elapsedTime + time.ElapsedGameTime)
                progress = (float)time.ElapsedGameTime.Ticks / (float)(m_totalDuration.Ticks - m_elapsedTime.Ticks);
            else
            {
                progress = 1f;
                m_isFinished = true;
            }
            m_elapsedTime += time.ElapsedGameTime;

            if (m_stretchToRectangle)
            {
                Rectangle lastRectangle = sprite.DisplayRect;
                sprite.DisplayRect = new Rectangle(
                    (int)(lastRectangle.X + ((m_finalRectangle.X - lastRectangle.X) * progress)),
                    (int)(lastRectangle.Y + ((m_finalRectangle.Y - lastRectangle.Y) * progress)),
                    (int)(lastRectangle.Width + ((m_finalRectangle.Width - lastRectangle.Width) * progress)),
                    (int)(lastRectangle.Height + ((m_finalRectangle.Height - lastRectangle.Height) * progress)));
            }
            else
            {

            }

        }
    }
}
