using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;


namespace Gliese581g
{
    // Intended for use in army setup.  
    class UnitTileBox
    {
        // The unit tiles are ordered into stacks by type.  
        Dictionary<UnitType , List<UnitTile>> m_tilesInBox;

        Texture2D m_backgroundTexture;
        Rectangle m_dispRect;
        SpriteFont m_font;


        public UnitTileBox(Texture2D backgroundTexture, Rectangle dispRect, SpriteFont font)
        {
            m_backgroundTexture = backgroundTexture;
            m_dispRect = dispRect;
            m_font = font;
        }




    }
}
