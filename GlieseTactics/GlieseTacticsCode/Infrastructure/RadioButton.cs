using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
namespace Gliese581g
{

    public class RadioButton : MenuButton
    {
        // This stores the actual values of all the 
        protected static Dictionary<string, int> s_values = new Dictionary<string,int>();
        public static void ClearGroup(string groupName)
        {
            if (s_values.ContainsKey(groupName))
                s_values.Remove(groupName);
        }

        public class RadioButtonEvent : Event
        {
            public RadioButton Owner;
            public RadioButtonEvent() : base(new TimeSpan(0)) { }
            public RadioButtonEvent(RadioButton owner) : base(new TimeSpan(0)) { Owner = owner; }

            public override void OnEvent(GameScreen parentScreen)
            {
                // Give the dict your value. 
                s_values[Owner.Group] = Owner.Value;
            }
        }

        public bool Selected { get { return s_values.ContainsKey(m_buttonGroup) && s_values[m_buttonGroup] == m_buttonValue; } }
        public int CurrentValue { get { return s_values.ContainsKey(m_buttonGroup) ? s_values[m_buttonGroup] : -1; } }
        Texture2D m_textureRadioButton;
        string m_buttonGroup;
        public string Group { get { return m_buttonGroup; } } 
        int m_buttonValue;
        public int Value { get { return m_buttonValue; } } 

        public RadioButton(Texture2D textureRadioButton, Rectangle displayRect, GameScreen parentScreen, string buttonGroup, int buttonValue)
            : base(TextureStore.Get(TexId.button_frame_lit),
            TextureStore.Get(TexId.button_frame_dim), displayRect,
            SfxStore.Get(SfxId.menu_mouseover),SfxStore.Get(SfxId.menu_click),
            new RadioButtonEvent(), false, parentScreen)
        {
            (m_clickEvent as RadioButtonEvent).Owner = this;
            m_textureRadioButton = textureRadioButton;
            m_buttonGroup = buttonGroup;
            m_buttonValue = buttonValue;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime time)
        {
            if (Selected)
                Tint = Color.Gray;
            else
                Tint = Color.White;

            base.Draw(spriteBatch, time);
            spriteBatch.Draw(m_textureRadioButton, DisplayRect, null, Tint, 0f, Vector2.Zero, SpriteEffects.None, 0f);
        }

    } 
}
