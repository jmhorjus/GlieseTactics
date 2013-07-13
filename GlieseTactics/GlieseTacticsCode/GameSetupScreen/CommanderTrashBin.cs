using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Gliese581g
{
    public class CommanderTrashBin : ClickableSprite
    {
        Texture2D m_dim;
        Texture2D m_lit;

        public CommanderTrashBin(Rectangle DisplayRect) :
            base(TextureStore.Get(TexId.player_trash_dim), DisplayRect, Color.White, 1f, 0f, Vector2.Zero, .5f)
        {
            m_dim = TextureStore.Get(TexId.player_trash_dim);
            m_lit = TextureStore.Get(TexId.player_trash_lit);
        }

        public override void OnStartMouseover()
        {
            // Setting this static bool lets the PlayerDisplaySocet know to delete the player object.
            CommanderDisplaySocket.s_dragTrash = true; 
            Texture = m_lit;
        }

        public override void OnStopMouseover(TimeSpan timeHeld)
        {
            CommanderDisplaySocket.s_dragTrash = false;
            Texture = m_dim;
        }
    }
}
