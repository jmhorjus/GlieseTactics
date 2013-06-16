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
    public class DrawnTextEffect : IDrawnEffect
    {
        // Static font used by all instances. 
        static float s_secondsToLive = 3.0f;
        static SpriteFont s_numberEffectFont;
        public static void InitStaticFonts(SpriteFont numberEffectFont)
        {
            s_numberEffectFont = numberEffectFont;
        }

        
        // Member variables 
        string m_text;
        Vector2 m_position;
        Color m_color;
        Vector2 m_origin;
        public bool Visible = true;

        float m_secondsOld = 0f;
        
        public bool IsFinished() { return m_secondsOld >= s_secondsToLive; }


        public DrawnTextEffect(string text, Color color, Vector2 pos)
        {
            m_text = text;
            m_position = pos;
            m_color = color;
            m_origin = s_numberEffectFont.MeasureString(m_text) / 2;
        }


        public void Draw(SpriteBatch spriteBatch, GameTime time)
        {
            m_secondsOld += (float)time.ElapsedGameTime.TotalSeconds;

            spriteBatch.DrawString(s_numberEffectFont,
                m_text,
                m_position + new Vector2(0, -(m_secondsOld / s_secondsToLive) * 3f * m_origin.Y),
                m_color * ((s_secondsToLive - m_secondsOld) / (s_secondsToLive * .75f)),
                0f, m_origin, 1f, SpriteEffects.None, 0f);
        }

    }
}
