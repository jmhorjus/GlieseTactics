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
    class OptionsScreen : GameScreen
    {

        // This event quits to the main menu (or to whatever screen spawned this screen)
        private class ExitEvent : Event
        {
            //800 milliseconds default pause.
            public ExitEvent() : base(DEFAULT_BUTTON_DELAY) { }
            public ExitEvent(TimeSpan time, GameScreen parent) : base(time) { }

            public override void OnEvent(GameScreen parentScreen)
            {   // This event exits the program.
                parentScreen.EnableKeysAndMouse();
                parentScreen.GetMainApp().clearCurrentScreenAndReturnToParent();
            }
        }

        /// Nested class ApplyButton; inherits from MenuButton and only has to implement 
        /// an "OnLeftClick" method.        
        private class ApplyButton : MenuButton
        {
            new OptionsScreen m_parentScreen;

            public ApplyButton(Texture2D lightTexture, Texture2D darkTexture,
                Vector2 pos,
                SoundEffect mouseOverSound, SoundEffect onClickSound,
                OptionsScreen parentScreen) :
                base(lightTexture, darkTexture, pos, mouseOverSound, onClickSound, null, true, parentScreen)
            {
                m_parentScreen = parentScreen;
            }

            public override void OnLeftClick(Vector2 mousePosInTexture)
            {
                // Set the unit voices
                ConfigManager.GlobalManager.UnitVoicesEnabled = m_parentScreen.m_unitVoicesChkBox.Checked;
                ConfigManager.GlobalManager.MouseScrollEnabled = m_parentScreen.m_mouseScrollChkBox.Checked;

                // First apply the settings
                m_parentScreen.GetMainApp().changeResolution(
                    m_parentScreen.ApplyResolution.X,
                    m_parentScreen.ApplyResolution.Y,
                    m_parentScreen.m_fullScreenChkBox.Checked);
                // Then set an ExitEvent.
                m_parentScreen.AddEvent(new ExitEvent());
                base.OnLeftClick(mousePosInTexture);
            }
        }

        /// Sets the resolution to a value between the min and max allowed based on the slider value.
        private class ResolutionSliderEvent : SliderBarEvent
        {
            public override void OnEvent(GameScreen parentScreen)
            {
                ((OptionsScreen)parentScreen).ApplyResolution.X = (int)(m_minResolution.X + ((m_maxResolution.X - m_minResolution.X) * SliderValue));
                ((OptionsScreen)parentScreen).ApplyResolution.Y = (int)(m_minResolution.Y + ((m_maxResolution.Y - m_minResolution.Y) * SliderValue));
            }
        }

        ///  Self explanitory
        private class MusicVolumeSliderEvent : SliderBarEvent
        {
            public override void OnEvent(GameScreen parentScreen)
            {
                ConfigManager.GlobalManager.MusicVolume = SliderValue;
                // For immediate effect
                MediaPlayer.Volume = SliderValue * parentScreen.GetMainApp().baseActiveScreen().MusicVolumeMultiplier; 
            }
        }

        ///  Self explanitory
        private class SfxVolumeSliderEvent : SliderBarEvent
        {
            public override void OnEvent(GameScreen parentScreen)
            {
                /// Should use the global config manager here.
                ConfigManager.GlobalManager.SfxVolume = SliderValue;
            }
        }

        ///  Self explanitory
        private class ScrollSpeedSliderEvent : SliderBarEvent
        {
            public override void OnEvent(GameScreen parentScreen)
            {
                /// Should use the global config manager here.
                ConfigManager.GlobalManager.MapScrollSpeed = SliderValue;
            }
        }




        // The things that matter in the MainMenuOptionsScreen:
        static readonly Point m_minResolution = new Point(840, 485);
        static readonly Point m_maxResolution = new Point(1400, 800);
        public Point ApplyResolution;

        // All the control objects.
        protected CheckBox m_fullScreenChkBox;  
        protected TextLabel m_fullScreenLabel;
        protected SliderBar m_resolutionSlider;
        protected TextLabel m_resolutionLabel;
        protected SliderBar m_musicVolumeSlider;
        protected TextLabel m_musicVolumeLabel;
        protected SliderBar m_sfxVolumeSlider;
        protected TextLabel m_sfxVolumeLabel;
        private ApplyButton m_applyButton;
        protected MenuButton m_cancelButton;

        protected CheckBox m_unitVoicesChkBox;
        protected TextLabel m_unitVoicesLabel;
        protected CheckBox m_mouseScrollChkBox; 
        protected TextLabel m_mouseScrollLabel;
        protected SliderBar m_scrollSpeedSlider;
        protected TextLabel m_scrollSpeedLabel;


        /// The MainMenuScreen constructor - just pass through. 
        public OptionsScreen(MainApplication mainApp) : base(mainApp)
        {
        }


        public override void InitScreen(ScreenRectangle portionOfScreen, GraphicsDevice graphicsDevice)
        {
            m_currentScreenRectangle = portionOfScreen;
            m_spriteBatchExMain.Transform = m_currentScreenRectangle.GetMatrixTransform(graphicsDevice);
            
            // Initial value 
            ApplyResolution = new Point(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);


            // The full-screen checkbox
            m_fullScreenChkBox = new CheckBox(TextureStore.Get(TexId.chkbox_false),
                TextureStore.Get(TexId.chkbox_true),
                m_fixedPositions["fullscreen_chkbox"],
                SfxStore.Get(SfxId.menu_click));
 
            // Initial value set properly. 
            m_fullScreenChkBox.Checked = GetMainApp().GraphicsDeviceManager.IsFullScreen;
            // Label for the full-screen check box
            m_fullScreenLabel = new TextLabel(
                "Enable Full-Screen Mode",
                m_defaultFont,
                m_fixedPositions["fullscreen_label"],
                Color.Black);
            
            /// FULL SCREEN MODE DISABLED: IT IS NOT COMPATIBLE WITH USE OF WINDOWS FORMS.
            //m_spriteBatchExMain.DrawnObjects.Add(m_fullScreenLabel);
            //m_spriteBatchExMain.DrawnObjects.Add(m_fullScreenChkBox);

            // The resolution slider-bar
            Rectangle resolutionSelectRect = new Rectangle(
                (int)m_fixedPositions["slider_bar_1"].X, (int)m_fixedPositions["slider_bar_1"].Y,
                (int)m_fixedPositions["slider_bar_size"].X, (int)m_fixedPositions["slider_bar_size"].Y);
            m_resolutionSlider = new SliderBar(
                TextureStore.Get(TexId.slider_knob_lit),
                TextureStore.Get(TexId.slider_knob_dim),
                TextureStore.Get(TexId.slider_bar),
                m_fixedPositions["slider_bar_1"] +
                m_fixedPositions["slider_bar_size"] * 
                ((float)(ApplyResolution.X - m_minResolution.X) / (float)(m_maxResolution.X - m_minResolution.X)),
                SfxStore.Get(SfxId.menu_mouseover),
                SfxStore.Get(SfxId.menu_click),
                this,
                resolutionSelectRect,
                new ResolutionSliderEvent() );
            m_spriteBatchExMain.DrawnObjects.Add(m_resolutionSlider);
           
            // The resolution slider's label.  
            m_resolutionLabel = new TextLabel(
                "Screen Size",
                m_defaultFont,
                m_fixedPositions["slider_bar_1"] + m_fixedPositions["slider_label_offset"],
                Color.Black);
            m_spriteBatchExMain.DrawnObjects.Add(m_resolutionLabel);


            // The music volume slider-bar
            Rectangle musicVolumeSelectRect = new Rectangle(
                (int)m_fixedPositions["slider_bar_2"].X, (int)m_fixedPositions["slider_bar_2"].Y,
                (int)m_fixedPositions["slider_bar_size"].X, (int)m_fixedPositions["slider_bar_size"].Y);
            m_musicVolumeSlider = new SliderBar(
                TextureStore.Get(TexId.slider_knob_lit),
                TextureStore.Get(TexId.slider_knob_dim),
                TextureStore.Get(TexId.slider_bar),
                m_fixedPositions["slider_bar_2"] +
                m_fixedPositions["slider_bar_size"] * ConfigManager.GlobalManager.MusicVolume,
                SfxStore.Get(SfxId.menu_mouseover),
                SfxStore.Get(SfxId.menu_click),
                this,
                musicVolumeSelectRect,
                new MusicVolumeSliderEvent());
            m_spriteBatchExMain.DrawnObjects.Add(m_musicVolumeSlider);
            // The music volume slider's label.  
            m_musicVolumeLabel = new TextLabel(
                "Music Volume",
                m_defaultFont,
                m_fixedPositions["slider_bar_2"] + m_fixedPositions["slider_label_offset"],
                Color.Black);
            m_spriteBatchExMain.DrawnObjects.Add(m_musicVolumeLabel);


            // The sfx volume slider-bar
            Rectangle sfxVolumeSelectRect = new Rectangle(
                (int)m_fixedPositions["slider_bar_3"].X, (int)m_fixedPositions["slider_bar_3"].Y,
                (int)m_fixedPositions["slider_bar_size"].X, (int)m_fixedPositions["slider_bar_size"].Y);
            m_sfxVolumeSlider = new SliderBar(
                TextureStore.Get(TexId.slider_knob_lit),
                TextureStore.Get(TexId.slider_knob_dim),
                TextureStore.Get(TexId.slider_bar),
                m_fixedPositions["slider_bar_3"] +
                m_fixedPositions["slider_bar_size"] * ConfigManager.GlobalManager.SfxVolume,
                SfxStore.Get(SfxId.menu_mouseover),
                SfxStore.Get(SfxId.menu_click),
                this,
                sfxVolumeSelectRect,
                new SfxVolumeSliderEvent());
            m_spriteBatchExMain.DrawnObjects.Add(m_sfxVolumeSlider);
            // The music volume slider's label.  
            m_sfxVolumeLabel = new TextLabel(
                "Sound Effects Volume",
                m_defaultFont,
                m_fixedPositions["slider_bar_3"] + m_fixedPositions["slider_label_offset"],
                Color.Black);
            m_spriteBatchExMain.DrawnObjects.Add(m_sfxVolumeLabel);

            //---------------------------

            Rectangle scrollSpeedSelectRect = new Rectangle(
                (int)m_fixedPositions["slider_bar_4"].X, (int)m_fixedPositions["slider_bar_4"].Y,
                (int)m_fixedPositions["slider_bar_size"].X, (int)m_fixedPositions["slider_bar_size"].Y);
            m_scrollSpeedSlider = new SliderBar(
                TextureStore.Get(TexId.slider_knob_lit),
                TextureStore.Get(TexId.slider_knob_dim),
                TextureStore.Get(TexId.slider_bar),
                m_fixedPositions["slider_bar_4"] +
                m_fixedPositions["slider_bar_size"] * ConfigManager.GlobalManager.MapScrollSpeed,
                SfxStore.Get(SfxId.menu_mouseover),
                SfxStore.Get(SfxId.menu_click),
                this,
                scrollSpeedSelectRect,
                new ScrollSpeedSliderEvent());
            m_spriteBatchExMain.DrawnObjects.Add(m_scrollSpeedSlider);
            // The music volume slider's label.  
            m_scrollSpeedLabel = new TextLabel(
                "Map Scroll Speed",
                m_defaultFont,
                m_fixedPositions["slider_bar_4"] + m_fixedPositions["slider_label_offset"],
                Color.Black);
            m_spriteBatchExMain.DrawnObjects.Add(m_scrollSpeedLabel);


            // The unit voice checkbox
            m_unitVoicesChkBox = new CheckBox(TextureStore.Get(TexId.chkbox_false),
                TextureStore.Get(TexId.chkbox_true),
                m_fixedPositions["unitvoice_chkbox"],
                SfxStore.Get(SfxId.menu_click));
            m_spriteBatchExMain.DrawnObjects.Add(m_unitVoicesChkBox);
            // Initial value set properly. 
            m_unitVoicesChkBox.Checked = ConfigManager.GlobalManager.UnitVoicesEnabled;

            // The mouse scroll checkbox
            m_mouseScrollChkBox = new CheckBox(TextureStore.Get(TexId.chkbox_false),
                TextureStore.Get(TexId.chkbox_true),
                m_fixedPositions["mousescroll_chkbox"],
                SfxStore.Get(SfxId.menu_click));
            m_spriteBatchExMain.DrawnObjects.Add(m_mouseScrollChkBox);
            // Initial value set properly. 
            m_mouseScrollChkBox.Checked = ConfigManager.GlobalManager.MouseScrollEnabled;

            // Label for the unit voice check box
            m_unitVoicesLabel = new TextLabel(
                "Enable Unit Voices",
                m_defaultFont,
                m_fixedPositions["unitvoice_label"],
                Color.Black);
            m_spriteBatchExMain.DrawnObjects.Add(m_unitVoicesLabel);

            // Label for the mouse scroll check box
            m_mouseScrollLabel = new TextLabel(
                "Enable Mouse Scrolling",
                m_defaultFont,
                m_fixedPositions["mousescroll_label"],
                Color.Black);
            m_spriteBatchExMain.DrawnObjects.Add(m_mouseScrollLabel);

            //---------------------------

            // Apply Button
            m_applyButton = new ApplyButton(
                TextureStore.Get(TexId.button_apply_lit),
                TextureStore.Get(TexId.button_apply_dim),
                m_fixedPositions["apply_button"],
                SfxStore.Get(SfxId.menu_mouseover),
                SfxStore.Get(SfxId.menu_click),
                this);
            m_spriteBatchExMain.DrawnObjects.Add(m_applyButton);

            // Cancel Button
            m_cancelButton = new MenuButton(
                TextureStore.Get(TexId.button_cancel_lit),
                TextureStore.Get(TexId.button_cancel_dim),
                m_fixedPositions["cancel_button"],
                SfxStore.Get(SfxId.menu_mouseover),
                SfxStore.Get(SfxId.menu_click),
                new ExitEvent(),
                true,
                this);
            m_spriteBatchExMain.DrawnObjects.Add(m_cancelButton);
        }

        public override void UninitScreen()
        {
            m_spriteBatchExMain.DrawnObjects.Clear();
        }

        public override void Update(GameTime gameTime, MouseState mouseState, KeyboardState keyboardState)
        {
            // Disable the resolution slider when in full-screen mode.
            if (m_resolutionSlider.Enabled == m_fullScreenChkBox.Checked)
                m_resolutionSlider.Enabled = !m_fullScreenChkBox.Checked;
            base.Update(gameTime, mouseState, keyboardState);
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
            m_backgroundColor = Color.LightSteelBlue;

            // Load a font for the labels
            m_defaultFont = Content.Load<SpriteFont>("Fonts/default");

            Vector2 screenSize = new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);

            // The Full Screen Checkbox
            TextureStore.Store.Preload(TexId.chkbox_false);
            TextureStore.Store.Preload(TexId.chkbox_true);
            m_fixedPositions["fullscreen_chkbox"] = new Vector2(
                screenSize.X * 0.45f, screenSize.Y * 0.08f);
            m_fixedPositions["fullscreen_label"] = new Vector2(
                screenSize.X * 0.1f,
                screenSize.Y * 0.08f + TextureStore.Get(TexId.chkbox_false).Height / 3);

            m_fixedPositions["unitvoice_chkbox"] = new Vector2(
                screenSize.X * 0.9f, screenSize.Y * 0.40f);
            m_fixedPositions["unitvoice_label"] = new Vector2(
                screenSize.X * 0.55f,
                screenSize.Y * 0.40f + TextureStore.Get(TexId.chkbox_false).Height / 3);
            m_fixedPositions["mousescroll_chkbox"] = new Vector2(
                screenSize.X * 0.9f, screenSize.Y * 0.60f);
            m_fixedPositions["mousescroll_label"] = new Vector2(
                screenSize.X * 0.55f,
                screenSize.Y * 0.60f + TextureStore.Get(TexId.chkbox_false).Height / 3);

            // The box within which the resolution dragger will be dragged. 
            TextureStore.Store.Preload(TexId.slider_knob_dim);
            TextureStore.Store.Preload(TexId.slider_knob_lit);
            TextureStore.Store.Preload(TexId.slider_bar);
            m_fixedPositions["slider_bar_1"] = new Vector2(
                screenSize.X * 0.075f, screenSize.Y * 0.6f);
            m_fixedPositions["slider_bar_2"] = new Vector2(
                screenSize.X * 0.075f, screenSize.Y * 0.4f);
            m_fixedPositions["slider_bar_3"] = new Vector2(
                screenSize.X * 0.075f, screenSize.Y * 0.2f);

            m_fixedPositions["slider_bar_4"] = new Vector2(
                screenSize.X * 0.525f, screenSize.Y * 0.2f);


            m_fixedPositions["slider_bar_size"] = new Vector2(
                screenSize.X * 0.4f, screenSize.Y * 0.0f);
            m_fixedPositions["slider_label_offset"] = new Vector2(
                screenSize.X * 0.0f,
                (screenSize.Y * .02f) + TextureStore.Get(TexId.slider_knob_dim).Height);


            // Apply button textures and starting location [TODO:Load the right textures]
            TextureStore.Store.Preload(TexId.button_apply_dim);
            TextureStore.Store.Preload(TexId.button_apply_lit);
            m_fixedPositions["apply_button"] = new Vector2(
                screenSize.X * 0.7f - TextureStore.Get(TexId.button_apply_dim).Width / 2,
                screenSize.Y * 0.8f);

            // Cancel button textures and starting location [TODO:Load the right textures]
            TextureStore.Store.Preload(TexId.button_cancel_dim);
            TextureStore.Store.Preload(TexId.button_cancel_lit);
            m_fixedPositions["cancel_button"] = new Vector2(
                screenSize.X * 0.3f - TextureStore.Get(TexId.button_cancel_dim).Width / 2,
                screenSize.Y * 0.8f);
        }

    }
}
