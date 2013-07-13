using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace Gliese581g
{
    class UnitTile : ClickableSprite
    {
        Unit m_unit;
        Texture2D m_emptyPortrait;
        SpriteFont m_font;
        Vector2 m_fontScale;


        public UnitTile(Texture2D emptyPortrait, Rectangle dispRect, SpriteFont font)
            : base(emptyPortrait, dispRect, Color.White, 1f, 0f, Vector2.Zero, 1f)
        {
            m_emptyPortrait = emptyPortrait;
            m_font = font;
        }

        
    }
}
