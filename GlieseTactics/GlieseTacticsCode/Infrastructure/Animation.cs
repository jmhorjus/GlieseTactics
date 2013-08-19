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

        bool m_isFinished;
        public bool Finished { get { return m_isFinished; } }

        Vector2 m_finalPos;
        Vector2 m_finalScale;

        bool m_stretchToRectangle;
        Rectangle m_finalRectangle;

        
        // If not null, this is a pointer to a sprite whose location/movements are incorperated into the animation.
        // the final position becomes and offset from the location of this object.  
        ClickableSprite m_anchorObject; 

        public Animation(TimeSpan duration, Vector2 finalPos, Vector2 finalScale, ClickableSprite anchorObject = null)
        {
            m_totalDuration = duration;
            m_elapsedTime = TimeSpan.Zero;

            m_stretchToRectangle = false;
            m_finalPos = finalPos;
            m_finalScale = finalScale;

            m_anchorObject = anchorObject;
        }

        public Animation(TimeSpan duration, Rectangle finalRectangle, ClickableSprite anchorObject = null)
        {
            m_totalDuration = duration;
            m_elapsedTime = TimeSpan.Zero;

            m_stretchToRectangle = true;
            m_finalRectangle = finalRectangle; 

            m_anchorObject = anchorObject;
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

            //Anchor offset
            Vector2 anchorOffset = Vector2.Zero;
            if (m_anchorObject != null)
            {
                anchorOffset = m_anchorObject.Position;
            }

            if (m_stretchToRectangle)
            {
                Rectangle lastRectangle = sprite.DisplayRect;
                sprite.DisplayRect = new Rectangle(
                    (int)(lastRectangle.X + ((m_finalRectangle.X - lastRectangle.X) * progress) + anchorOffset.X ),
                    (int)(lastRectangle.Y + ((m_finalRectangle.Y - lastRectangle.Y) * progress) + anchorOffset.Y ),
                    (int)(lastRectangle.Width + ((m_finalRectangle.Width - lastRectangle.Width) * progress)),
                    (int)(lastRectangle.Height + ((m_finalRectangle.Height - lastRectangle.Height) * progress)));
            }
            else
            {
                throw new Exception("don't support non rectangle-based animations.");
            }

        }
    }
}
