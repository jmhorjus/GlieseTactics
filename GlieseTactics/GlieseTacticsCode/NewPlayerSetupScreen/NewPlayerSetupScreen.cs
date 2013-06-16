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
    public class NewPlayerSetupScreen : GameScreen
    {
        // This event quits to the main menu (or to whatever screen spawned this screen)
        private class CancelEvent : Event
        {
            public CancelEvent() : base(DEFAULT_BUTTON_DELAY) { }
            public CancelEvent(TimeSpan time) : base(time) { }

            public override void OnEvent(GameScreen parentScreen)
            { 
                parentScreen.EnableKeysAndMouse();
                parentScreen.GetMainApp().clearCurrentScreenAndReturnToParent();
            }
        }


        TextLabel m_nameLabel;
        TextLabel m_genderLabel;
        TextLabel m_genderMaleLabel;
        TextLabel m_genderFemaleLabel;
        TextLabel m_portraitLabel;
        TextLabel m_unitcolorLabel;
        MenuButton m_cancelButton;
        public RadioButton genderMaleRadioButton;
        public RadioButton genderFemaleRadioButton;

        public NewPlayerSetupScreen(MainApplication mainApp)
            : base(mainApp)
        {
        }
        public override void InitScreen(ScreenRectangle portionOfScreen, Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice)
        {
            m_currentScreenRectangle = portionOfScreen;
            m_spriteBatchExMain.Transform = m_currentScreenRectangle.GetMatrixTransform(graphicsDevice);

            // Cancel Button
            m_cancelButton = new MenuButton(
                TextureStore.Get(TexId.button_cancel_lit),
                TextureStore.Get(TexId.button_cancel_dim),
                m_fixedPositions["cancel_button"],
                SfxStore.Get(SfxId.menu_mouseover),
                SfxStore.Get(SfxId.menu_click),
                new CancelEvent(), 
                true,
                this);
            m_spriteBatchExMain.DrawnObjects.Add(m_cancelButton);
            //gender male radio button
           // genderMaleRadioButton = new RadioButton(TextureStore.Get(TexId.rdbutton_false),
             //   m_fixedPositions["gender_male_rdbutton"],
               // m_sounds["menu_click"]);
            //m_spriteBatchExMain.DrawnObjects.Add(genderMaleRadioButton);
            //gender female radio button
            //genderFemaleRadioButton = new RadioButton(TextureStore.Get(TexId.rdbutton_false),
              //  m_fixedPositions["gender_female_rdbutton"],
               // m_sounds["menu_click"]);
            //m_spriteBatchExMain.DrawnObjects.Add(genderFemaleRadioButton);
            // Label for Name text box
            m_nameLabel = new TextLabel(
                "Name",
                m_defaultFont,
                m_fixedPositions["name_label"] + m_fixedPositions["profile_names_offset"],
                Color.Black);
            m_spriteBatchExMain.DrawnObjects.Add(m_nameLabel);

            m_genderLabel = new TextLabel(
                "Gender",
                m_defaultFont,
                m_fixedPositions["gender_label"] + m_fixedPositions["profile_names_offset"],
                Color.Black);
            m_spriteBatchExMain.DrawnObjects.Add(m_genderLabel);

            m_genderMaleLabel = new TextLabel(
                "Male",
                m_defaultFont,
                m_fixedPositions["gender_male_label"] + m_fixedPositions["profile_names_offset"],
                Color.Black);
            m_spriteBatchExMain.DrawnObjects.Add(m_genderMaleLabel);

            m_genderFemaleLabel = new TextLabel(
                "Female",
                m_defaultFont,
                m_fixedPositions["gender_female_label"] + m_fixedPositions["profile_names_offset"],
                Color.Black);
            m_spriteBatchExMain.DrawnObjects.Add(m_genderFemaleLabel);

            m_portraitLabel = new TextLabel(
               "Portrait",
               m_defaultFont,
               m_fixedPositions["portrait_label"] + m_fixedPositions["profile_names_offset"],
               Color.Black);
            m_spriteBatchExMain.DrawnObjects.Add(m_portraitLabel);

            m_unitcolorLabel = new TextLabel(
               "Unit Color",
               m_defaultFont,
               m_fixedPositions["unitcolor_label"] + m_fixedPositions["profile_names_offset"],
               Color.Black);
            m_spriteBatchExMain.DrawnObjects.Add(m_unitcolorLabel);
        }
        
        public override void UninitScreen()
        {
            m_spriteBatchExMain.DrawnObjects.Clear();
        }




        /// <summary>
        /// LoadContent defines semi-constant values that will not change unless the screen resolution is changed. 
        /// </summary>
        /// <param name="Content">The content managet from the main application.</param>
        /// <param name="graphicsDevice">The GraphicsDevice from the main app - knows the current resolution.</param>
        public override void LoadContent(ContentManager Content, GraphicsDevice graphicsDevice)
        {
            //Load the cursor sprite
            m_defaultMouseCursor = Cursor.LoadDefaultCursor();
            ActiveMouseCursor = m_defaultMouseCursor;

            // Load the background
            m_backgroundTexture = TextureStore.Get(TexId.default_submenu_frame);
            // Load a font for the labels
            m_defaultFont = Content.Load<SpriteFont>("Fonts/default");
            // Cancel button textures and starting location [TODO:Load the right textures]
            m_fixedPositions["cancel_button"] = new Vector2(
                graphicsDevice.Viewport.Width * 0.3f - TextureStore.Get(TexId.button_cancel_dim).Width / 2,
                graphicsDevice.Viewport.Height * 0.8f);


            // Set fixed positions.
            TextureStore.Store.Preload(TexId.rdbutton_false);
            m_fixedPositions["gender_male_rdbutton"] = new Vector2(
                graphicsDevice.Viewport.Width * 0.54f,
                graphicsDevice.Viewport.Height * 0.35f);

            m_fixedPositions["gender_female_rdbutton"] = new Vector2(
               graphicsDevice.Viewport.Width * 0.54f,
               graphicsDevice.Viewport.Height * 0.45f);

            //Player name Label
            m_fixedPositions["name_label"] = new Vector2(
                graphicsDevice.Viewport.Width * 0.2f,
                graphicsDevice.Viewport.Height * 0.15f);

            //Player Gender Label
            m_fixedPositions["gender_label"] = new Vector2(
                graphicsDevice.Viewport.Width * 0.2f,
                graphicsDevice.Viewport.Height * 0.3f);

            m_fixedPositions["gender_male_label"] = new Vector2(
                graphicsDevice.Viewport.Width * 0.4f,
                graphicsDevice.Viewport.Height * 0.3f);

            m_fixedPositions["gender_female_label"] = new Vector2(
                graphicsDevice.Viewport.Width * 0.4f,
                graphicsDevice.Viewport.Height * 0.4f);

            //Player Portrait Label
            m_fixedPositions["portrait_label"] = new Vector2(
             graphicsDevice.Viewport.Width * 0.2f,
             graphicsDevice.Viewport.Height * 0.45f);

            //Player unit color Label
            m_fixedPositions["unitcolor_label"] = new Vector2(
                graphicsDevice.Viewport.Width * 0.2f,
                graphicsDevice.Viewport.Height * 0.6f);

            m_fixedPositions["profile_names_offset"] = new Vector2(
                graphicsDevice.Viewport.Width * 0.0f,
                graphicsDevice.Viewport.Height * .02f + TextureStore.Get(TexId.textbox_name).Height / 2);
        }


    }
}
