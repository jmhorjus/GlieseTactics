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

    // A slightly augmented event which also has a sliderValue parameter. 
    public abstract class SliderBarEvent : Event
    {
        // Value between 1 and 0.
        public float SliderValue;

        public SliderBarEvent()
            : base(TimeSpan.Zero)
        {
        }

        public SliderBarEvent(TimeSpan timeTillEvent)
            : base(timeTillEvent)
        {
        }


    }

    /// SliderBar class; inherits from MenuButton.
    public class SliderBar : MenuButton
    {
        // Redefine m_parentScreen as the appropriate type of screen. 
        Texture2D m_sliderBarTexture;
        Rectangle m_sliderBarRectangle;
        SliderBarEvent m_sliderBarEvent;

        public SliderBar(
            Texture2D lightTexture, Texture2D darkTexture,
            Texture2D sliderBarTexture,
            Vector2 pos,
            SoundEffect mouseOverSound, SoundEffect onClickSound,
            GameScreen parentScreen,
            Rectangle sliderRectangle,
            SliderBarEvent sliderBarEvent) :
            base(lightTexture, darkTexture, pos, mouseOverSound, onClickSound, null, false, parentScreen)
        {
            m_parentScreen = parentScreen;
            EnableClickDrag(0, sliderRectangle, .5f, 1f);

            m_sliderBarTexture = sliderBarTexture;
            m_sliderBarEvent   = sliderBarEvent;

            m_sliderBarRectangle = new Rectangle(
                m_dragLimits.X, m_dragLimits.Y + m_lightTexture.Height,
                m_dragLimits.Width + m_lightTexture.Width, m_sliderBarTexture.Height);
        }


        public override void OnLeftClickDragDrop(TimeSpan timeHeld, Vector2 endPoint, Vector2 mousePosInTexture)
        {
            // Change position
            Position = endPoint;
            // Calculate new resolution based on new position.
            m_sliderBarEvent.Reset();
            m_sliderBarEvent.SliderValue = (Position.X - m_dragLimits.X) / m_dragLimits.Width;

            m_parentScreen.AddEvent(m_sliderBarEvent);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime time)
        {
            Tint = Enabled ? Color.White : Color.Gray;
            spriteBatch.Draw(m_sliderBarTexture, m_sliderBarRectangle, Tint);
            base.Draw(spriteBatch, time);
        }
    }
}
