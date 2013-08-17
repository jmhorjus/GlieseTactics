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
    public class MenuButton : ClickableSprite
    {
        protected GameScreen m_parentScreen;
        public GameScreen ParentScreen { get { return m_parentScreen; } set { m_parentScreen = value; } }

        protected Texture2D m_lightTexture;
        protected Texture2D m_darkTexture;
        protected SoundEffect m_mouseOverSound;
        protected SoundEffect m_onClickSound;
        protected Event m_clickEvent;
        protected bool m_disableInputOnClick;

        



        public MenuButton(
            Texture2D lightTexture, 
            Texture2D darkTexture, 
            Vector2 pos, 
            SoundEffect mouseOverSound, 
            SoundEffect onClickSound,  
            Event clickEvent,
            bool disableInputOnClick,
            GameScreen parentScreen) :
            base(darkTexture, pos, Color.White, 1f, Vector2.One, 0f, Vector2.Zero, .5f)
        { 
            m_parentScreen = parentScreen;
            m_lightTexture = lightTexture;
            m_darkTexture = darkTexture;
            m_mouseOverSound = mouseOverSound;
            m_onClickSound = onClickSound;

            m_clickEvent = clickEvent;
            m_disableInputOnClick = disableInputOnClick;
        }


        // The version of the constructor that takes a rectangle.
        public MenuButton(
            Texture2D lightTexture,
            Texture2D darkTexture,
            Rectangle displayRect,
            SoundEffect mouseOverSound,
            SoundEffect onClickSound,
            Event clickEvent,
            bool disableInputOnClick,
            GameScreen parentScreen) :
            base(darkTexture, displayRect, Color.White, 1f, 0f, Vector2.Zero, .5f)
        {
            m_parentScreen = parentScreen;
            m_lightTexture = lightTexture;
            m_darkTexture = darkTexture;
            m_mouseOverSound = mouseOverSound;
            m_onClickSound = onClickSound;

            m_clickEvent = clickEvent;
            m_disableInputOnClick = disableInputOnClick;
        }


        public void SetTextures(Texture2D dim, Texture2D lit)
        {
            m_darkTexture = dim;
            m_lightTexture = lit;
            Texture = (LastMouseOver) ? m_lightTexture : m_darkTexture;
        }


        public override void OnStartMouseover()
        {
            Texture = m_lightTexture;
            SfxStore.Play(m_mouseOverSound);
        }

        public override void OnStopMouseover(TimeSpan time)
        {
            Texture = m_darkTexture;
        }

        public override void OnLeftClick(Vector2 mousePosInTexture)
        {
            SfxStore.Play(m_onClickSound);
            if (m_clickEvent == null)
                return;

            if (m_disableInputOnClick)
                m_parentScreen.DisableKeysAndMouse();
            
            m_clickEvent.Reset();
            m_parentScreen.AddEvent(m_clickEvent);

        }





    }
}
