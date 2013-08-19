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
    class MenuButtonPannel : ClickableSprite
    {
        static readonly Color s_buttonBeginColor = Color.Red;
        static readonly Color s_buttonEndColor = Color.Blue;


        Texture2D m_baseTexture;
        Texture2D m_buttonFillerTexture;
        
        GameScreen m_parentScreen;
        public GameScreen ParentScreen { get { return m_parentScreen; }
            set 
            {
                m_parentScreen = value;
                foreach (MenuButton mb in m_buttons)
                    mb.ParentScreen = value;
            }
        }


        List<MenuButton> m_buttons;
        List<Vector2> m_buttonTopLeft;
        List<Vector2> m_buttonSize;

        public override Rectangle LocationRect
        {   // We override the DisplayRect set function to also call UpdateButtonPositions().
            set
            {
                Vector2 scale = Scale;
                base.LocationRect = value;
                if (scale != Scale)
                    UpdateButtonPositions();
            }
        }
        public override bool Visible
        {
            set
            {
                base.Visible = value;
                for (int ii = 0; ii < m_buttons.Count; ii++)
                    m_buttons[ii].Visible = value;
            }
        }
        public override bool Enabled
        {
            set
            {
                base.Enabled = value;
                for (int ii = 0; ii < m_buttons.Count; ii++)
                    m_buttons[ii].Enabled = value;
            }
        }


        public MenuButtonPannel(Texture2D baseTexture, Texture2D buttonFillerTexture, Rectangle displayRect, GameScreen parentScreen, ClickableSprite anchorSprite = null)
            : base(baseTexture, displayRect, Color.White, 1f, 0f, Vector2.Zero, 0f, anchorSprite)
        {
            m_baseTexture = baseTexture;
            m_buttonFillerTexture = buttonFillerTexture;
            m_buttons = new List<MenuButton>();
            m_parentScreen = parentScreen;

            Initialize();
        }

        protected void Initialize()
        {
            //Look at the base texture, and find pixels of the trigger colors.

            Rectangle wholeImage = new Rectangle(0,0,m_baseTexture.Width, m_baseTexture.Height);
            int size = wholeImage.Width*wholeImage.Height;
            Color[] buffer = new Color[size];
            m_baseTexture.GetData(0, wholeImage, buffer, 0, size);

            m_buttonTopLeft = new List<Vector2>();
            m_buttonSize = new List<Vector2>();

            m_buttons.Clear();
            for(int xx = 0; xx<wholeImage.Width; xx++){
                for(int yy = 0; yy<wholeImage.Height; yy++)
            {
                // This algorithm won't work in every case. 
                // But in simple cases of even rows of buttons it will.
                Color c = buffer[xx + yy * wholeImage.Width];
                if (c == s_buttonBeginColor)
                { 
                    m_buttonTopLeft.Add(new Vector2((float)xx/wholeImage.Width,(float)yy/wholeImage.Height)); 
                }

                if (c == s_buttonEndColor)
                { 
                    m_buttonSize.Add(new Vector2((float)xx/wholeImage.Width,(float)yy/wholeImage.Height)); 
                }
            }}

            if (m_buttonTopLeft.Count != m_buttonSize.Count)
            { throw new Exception("Not a valid base image for a button pannel, or trigger colors are wrong."); }

            // Just create the menubuttons. Do nothing else.
            for (int ii = 0; ii < Math.Max(m_buttonTopLeft.Count, m_buttonSize.Count) ; ii++)
            {
                // Make buttonSize actually equal the size reletive to the panel.
                m_buttonSize[ii] -= m_buttonTopLeft[ii];

                m_buttons.Add(new MenuButton(m_buttonFillerTexture, m_buttonFillerTexture, Rectangle.Empty, 
                    SfxStore.Get(SfxId.menu_mouseover), SfxStore.Get(SfxId.menu_click), null, true, m_parentScreen, this));
            }

            // This function places them in the right locations/scales. 
            UpdateButtonPositions();
        }


        protected void UpdateButtonPositions()
        {
            // The rectangle for the button depends on 2 things: the panel's display area and
            // the button's internal rectangle.  
            if (m_buttons == null)
                return;
            for (int ii = 0; ii < m_buttons.Count; ii++)
            {
                m_buttons[ii].LocationRect = new Rectangle(
                    (int)(Width * m_buttonTopLeft[ii].X),  // TODO: include draworigin later
                    (int)(Height * m_buttonTopLeft[ii].Y),
                    (int)(Width * m_buttonSize[ii].X),
                    (int)(Height * m_buttonSize[ii].Y));
            }
        }



        public override void Draw(SpriteBatch spriteBatch, GameTime time)
        {
            //Draw the base first.
            base.Draw(spriteBatch, time);

            //Then draw all the buttons. 
            for (int ii = 0; ii < m_buttons.Count; ii++)
            {
                m_buttons[ii].Draw(spriteBatch, time);
            }

        }


        public override bool Update(MouseState mouseState, Matrix transformMatrix, GameTime time, bool mouseAlreadyIntercepted)
        {
            bool retVal = base.Update(mouseState, transformMatrix, time, mouseAlreadyIntercepted);

            for(int ii = 0; ii < m_buttons.Count; ii++)
            {
                retVal |= m_buttons[ii].Update(mouseState, transformMatrix, time, mouseAlreadyIntercepted); 
            }

            return retVal;
        }
    }
}
