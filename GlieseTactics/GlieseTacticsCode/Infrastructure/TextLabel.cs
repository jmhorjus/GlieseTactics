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
    public class TextLabel : IDrawnObject
    {
        public string Text;
        public SpriteFont Font;
        public Vector2 Position;
        public Color Tint;
        public bool Visible = true;

        bool m_centerOnPosition;


        public TextLabel(string text, SpriteFont font, Vector2 position, Color tint, bool centerOnPosition = false)
        {
            Visible = true;

            Text = text;
            Font = font;
            Position = position;
            Tint = tint;
            m_centerOnPosition = centerOnPosition;
        }


        public void Draw(SpriteBatch spriteBatch, GameTime time)
        {
            if (!Visible)
                return;

            Vector2 pos = Position;
            if(m_centerOnPosition)
                pos -= Font.MeasureString(Text) / 2;
            spriteBatch.DrawString(Font, Text, pos, Tint);
        }

    }
}
