using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Gliese581g
{
    public class Cursor
    {
        public Texture2D Texture;
        public Vector2 MousePointInTexture;
        public Color Tint;
        public float Rotation = 0f;
        public TextLabel ToolTip = null;
        public Vector2 ToolTipOffset = Vector2.Zero;

        public Cursor(Texture2D texture, Vector2 mousePointInTexture, Color tint)
        {
            Texture = texture;
            MousePointInTexture = mousePointInTexture;
            Tint = tint;
        }

        public void Draw(SpriteBatch spriteBatchMouse, GameTime time, Vector2 mouseLocation)
        {
            spriteBatchMouse.Draw(Texture, mouseLocation, 
                null, Tint, 
                Rotation, MousePointInTexture, 
                1f, SpriteEffects.None, 0f);

            if (ToolTip != null)
            {
                ToolTip.Position = mouseLocation + ToolTipOffset;
                ToolTip.Draw(spriteBatchMouse, time); 
            }
        }

        public static Cursor LoadDefaultCursor()
        {
            return new Cursor(TextureStore.Get(TexId.cursor_default), Vector2.Zero, Color.White);
        }

        public static Cursor LoadTargetCursor()
        {
            Texture2D texture = TextureStore.Get(TexId.cursor_target);
            Vector2 center = new Vector2(texture.Height / 2, texture.Width / 2);
            return new Cursor( texture, center, Color.White );
        }

        public static Cursor LoadTargetCursorSniper()
        {
            Texture2D texture = TextureStore.Get(TexId.cursor_target_sniper);
            Vector2 center = new Vector2(texture.Height / 2, texture.Width / 2);
            return new Cursor(texture, center, Color.White);
        }

        public static Cursor LoadMoveCursor()
        {
            Texture2D texture = TextureStore.Get(TexId.cursor_move);
            Vector2 center = new Vector2(146,48);
            return new Cursor(texture, center, Color.White);
        }

        public static Cursor LoadRechargeCursor()
        {
            Texture2D texture = TextureStore.Get(TexId.cursor_recharge);
            Vector2 center = new Vector2(texture.Height / 2, texture.Width / 2);
            return new Cursor(texture, center, Color.White);
        }
    }
}
