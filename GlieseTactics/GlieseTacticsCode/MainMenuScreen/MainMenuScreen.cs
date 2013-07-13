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
    /// <summary>
    /// The main menu inherits from GameScreen. 
    /// It is associated with game_screen_key.MainMenu.
    /// It allows the user to start a game, load a game, adjust sound/video options, and quit.
    /// </summary>
    public class MainMenuScreen : GameScreen
    {
        /// Nested class NewGameButton; inherits from MenuButton and only has to implement 
        /// an "OnLeftClick" method.        
        private class NewGameEvent : Event
        {
            public NewGameEvent() : base(DEFAULT_BUTTON_DELAY) { }

            public override void OnEvent(GameScreen parentScreen)
            {
                parentScreen.EnableKeysAndMouse();
                parentScreen.GetMainApp().changeToNewBaseActiveScreen(game_screen_key.GameSetupScreen);
            }
        }

        /// Nested class LoadGameEvent; inherits from MenuButton and only has to implement 
        /// an "OnLeftClick" method.    
        private class LoadGameEvent : Event
        {
            public LoadGameEvent() : base(DEFAULT_BUTTON_DELAY) { }

            public override void OnEvent(GameScreen parentScreen)
            {
                // Look for xml file with game data...tell the main game screen to load it somehow, then switch screens.
                parentScreen.EnableKeysAndMouse();
                GameMapScreen mapScreen = parentScreen.GetMainApp().GetScreenById(game_screen_key.MainGame) as GameMapScreen;
                if (mapScreen.Game != null)
                    parentScreen.GetMainApp().changeToNewBaseActiveScreen(game_screen_key.MainGame);
            }
        }

        /// OptionsEvent - the event triggered by the Options Button.
        private class OptionsEvent : Event
        {
            public OptionsEvent() : base(DEFAULT_BUTTON_DELAY) { }

            public override void OnEvent(GameScreen parentScreen)
            {   // This event exits the program.
                parentScreen.EnableKeysAndMouse();
                parentScreen.GetMainApp().spawnNewSubScreen(game_screen_key.OptionsScreen, new ScreenRectangle(.20f, .15f, .6f, .6f));
            }
        }

        /// ExitEvent - the event triggered by the Exit Button.  
        private class ExitEvent : Event
        {
            public ExitEvent() : base(new TimeSpan(0, 0, 2)) { }

            public override void OnEvent(GameScreen parentScreen)
            {   // This event exits the program.
                parentScreen.GetMainApp().Exit();
            }
        }




        /// The MainMenuScreen constructor - just pass through. 
        public MainMenuScreen(MainApplication mainApp) : base(mainApp)
        {
        }


        MenuButton m_newGameButton;
        MenuButton m_loadGameButton;
        MenuButton m_optionsButton;
        MenuButton m_exitButton;
        public override void InitScreen(ScreenRectangle portionOfScreen, GraphicsDevice graphicsDevice)
        {
            m_currentScreenRectangle = portionOfScreen;
            m_spriteBatchExMain.Transform = m_currentScreenRectangle.GetMatrixTransform(graphicsDevice);

            // Play the main-menu music.  
            MediaPlayer.Play(m_activeMusic);
            MediaPlayer.Volume = ConfigManager.GlobalManager.MusicVolume; // 100% volume for this song.
            MediaPlayer.IsRepeating = true;

            m_newGameButton = new MenuButton(
                TextureStore.Get(TexId.button_newgame_lit),
                TextureStore.Get(TexId.button_newgame_dim),
                m_fixedPositions["newgame_button"],
                SfxStore.Get(SfxId.menu_mouseover),
                SfxStore.Get(SfxId.menu_click), 
                new NewGameEvent(), 
                true,
                this);
            m_spriteBatchExMain.DrawnObjects.Add(m_newGameButton);

            m_loadGameButton = new MenuButton(
                 TextureStore.Get(TexId.button_loadgame_lit),
                TextureStore.Get(TexId.button_loadgame_dim),
                m_fixedPositions["loadgame_button"],
                SfxStore.Get(SfxId.menu_mouseover),
                SfxStore.Get(SfxId.menu_click),
                new LoadGameEvent(),
                true,
                this);
            m_spriteBatchExMain.DrawnObjects.Add(m_loadGameButton);

            m_optionsButton = new MenuButton(
                TextureStore.Get(TexId.button_options_lit),
                TextureStore.Get(TexId.button_options_dim),
                m_fixedPositions["options_button"],
                SfxStore.Get(SfxId.menu_mouseover),
                SfxStore.Get(SfxId.menu_click),
                new OptionsEvent(),
                true,
                this);
            m_spriteBatchExMain.DrawnObjects.Add(m_optionsButton);

            m_exitButton = new MenuButton(
                TextureStore.Get(TexId.button_exit_lit),
                TextureStore.Get(TexId.button_exit_dim),
                m_fixedPositions["exit_button"],
                SfxStore.Get(SfxId.menu_mouseover),
                SfxStore.Get(SfxId.rocket_boom),
                new ExitEvent(),
                true,
                this);
            m_spriteBatchExMain.DrawnObjects.Add(m_exitButton);

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
            m_backgroundTexture = TextureStore.Get(TexId.mainmenu_background);

            // Load the music
            m_activeMusic = Content.Load<Song>("Music/mainmenu");


            // Load the menu images:
            // New Game button textures and starting location
            TextureStore.Store.Preload(TexId.button_newgame_dim);
            TextureStore.Store.Preload(TexId.button_newgame_lit);
            m_fixedPositions["newgame_button"] = new Vector2(
                (graphicsDevice.Viewport.Width - TextureStore.Get(TexId.button_newgame_dim).Width) * 0.5f,
                (graphicsDevice.Viewport.Height - TextureStore.Get(TexId.button_newgame_dim).Height) * 0.40f);

            // Load Game button textures and starting location
            TextureStore.Store.Preload(TexId.button_loadgame_dim);
            TextureStore.Store.Preload(TexId.button_loadgame_lit);
            m_fixedPositions["loadgame_button"] = new Vector2(
                (graphicsDevice.Viewport.Width - TextureStore.Get(TexId.button_loadgame_dim).Width) * 0.5f,
                (graphicsDevice.Viewport.Height - TextureStore.Get(TexId.button_loadgame_dim).Height) * 0.55f);

            // Options button textures and starting location
            TextureStore.Store.Preload(TexId.button_options_dim);
            TextureStore.Store.Preload(TexId.button_options_lit);
            m_fixedPositions["options_button"] = new Vector2(
                (graphicsDevice.Viewport.Width - TextureStore.Get(TexId.button_options_dim).Width) * 0.5f,
                (graphicsDevice.Viewport.Height - TextureStore.Get(TexId.button_options_dim).Height) * 0.70f);

            // Exit button textures and starting location
            TextureStore.Store.Preload(TexId.button_exit_dim);
            TextureStore.Store.Preload(TexId.button_exit_lit);
            m_fixedPositions["exit_button"] = new Vector2(
                (graphicsDevice.Viewport.Width - TextureStore.Get(TexId.button_exit_dim).Width) * 0.5f,
                (graphicsDevice.Viewport.Height - TextureStore.Get(TexId.button_exit_dim).Height) * 0.85f);
        }

    }
}
