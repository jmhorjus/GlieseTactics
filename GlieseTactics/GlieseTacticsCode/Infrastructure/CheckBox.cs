using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;

namespace Gliese581g
{
    public class CheckBox : ClickableSprite
    {
        bool m_checkedValue;
        Texture2D m_textureChecked;
        Texture2D m_textureUnchecked;
        SoundEffect m_clickSound;

        public bool Checked
        {
            get 
            { 
                return m_checkedValue; 
            }
            set 
            {
                m_checkedValue = value;
                Texture = m_checkedValue ? m_textureChecked : m_textureUnchecked;
            }

        }


        public CheckBox(Texture2D textureUnchecked, Texture2D textureChecked, Vector2 pos, SoundEffect clickSound)
            : base(textureUnchecked, pos, Color.White, 1f, Vector2.One, 0f, Vector2.Zero, 1f)
        {
            m_textureUnchecked = textureUnchecked;
            m_textureChecked = textureChecked;
            m_clickSound = clickSound;
        }


        public override void OnLeftClick(Vector2 mousePosInTexture)
        {
            Checked = !Checked;
            //This is yet another change. 
            SfxStore.Play(m_clickSound);
        }


    }
}
