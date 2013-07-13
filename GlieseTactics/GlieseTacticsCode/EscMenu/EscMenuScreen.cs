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

    class EscMenuScreen : GameScreen
    {
        /// <summary>
        /// Define Events used by this screen.
        /// </summary>

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

        private class MainMenuEvent : Event
        {
            public MainMenuEvent() : base(DEFAULT_BUTTON_DELAY) { }
            public MainMenuEvent(TimeSpan time, GameScreen parent) : base(time) { }

            public override void OnEvent(GameScreen parentScreen)
            {
                parentScreen.EnableKeysAndMouse();
                parentScreen.GetMainApp().changeToNewBaseActiveScreen(game_screen_key.MainMenuScreen);
            }
        }

        private class OptionsEvent : Event
        {
            public OptionsEvent() : base(DEFAULT_BUTTON_DELAY) { }

            public override void OnEvent(GameScreen parentScreen)
            {
                parentScreen.EnableKeysAndMouse();
                parentScreen.GetMainApp().spawnNewSubScreen(game_screen_key.OptionsScreen, new ScreenRectangle(.20f, .15f, .6f, .6f));
            }
        }

        private class LoadGameEvent : Event
        {
            public LoadGameEvent() : base(DEFAULT_BUTTON_DELAY) { }

            public override void OnEvent(GameScreen parentScreen)
            {
                parentScreen.EnableKeysAndMouse();
                

                // Disabled for now.
                //GameMapScreen mapScreen = parentScreen.GetMainApp().GetScreenById(game_screen_key.MainGame) as GameMapScreen;
                //if (mapScreen.Game != null)  
                //    parentScreen.GetMainApp().changeToNewBaseActiveScreen(game_screen_key.MainGame);
            }
        }

        private class SurrenderGameEvent : Event
        {
            public SurrenderGameEvent() : base(DEFAULT_BUTTON_DELAY) { }

            public override void OnEvent(GameScreen parentScreen)
            {
                parentScreen.EnableKeysAndMouse();
                GameMapScreen mapscreen = (parentScreen.GetMainApp().topScreensParent() as GameMapScreen);

                // is the game already over?
                if (mapscreen.Game.WinningPlayer != null)
                    return;

                // make sure they are sure they want to surrender.
                if (!NewPlayerScreen.GetInstance.ConfirmationDialog("Are you sure you want to surrender to your opponent?"))
                    return;
                    
                //Surrender and end the game.
                mapscreen.Game.CurrentPlayer.Surrender = true;
                mapscreen.Game.CurrentTurnStage = Game.TurnStage.EndTurn;
                parentScreen.GetMainApp().clearCurrentScreenAndReturnToParent();
            }
        }
        //---------------------------------------



        MenuButton m_cancelButton;
        MenuButton m_mainMenuButton;
        MenuButton m_optionsButton;
        MenuButton m_loadButton;
        MenuButton m_surrenderButton;

        bool m_justLoadedScreen = true;

        public EscMenuScreen(MainApplication mainApp)
            : base(mainApp)
        {

        }

        public static float leftPositionForCenteredObject(float objectWidth, float screenWidth)
        {
            return (screenWidth - objectWidth) * 0.5f;
        }
            

        public override void InitScreen(ScreenRectangle portionOfScreen, GraphicsDevice graphicsDevice)
        {
            m_currentScreenRectangle = portionOfScreen;
            m_spriteBatchExMain.Transform = m_currentScreenRectangle.GetMatrixTransform(graphicsDevice);

            //Cancel
            m_cancelButton = new MenuButton(
            TextureStore.Get(TexId.button_g_cancel_lit),
            TextureStore.Get(TexId.button_g_cancel_dim),
            m_fixedPositions["cancel_button"],
            SfxStore.Get(SfxId.menu_mouseover),
            SfxStore.Get(SfxId.menu_click),
            new ExitEvent(),
            true,
            this);
            m_spriteBatchExMain.DrawnObjects.Add(m_cancelButton);

            //Return to Main Menu
            m_mainMenuButton = new MenuButton(
                TextureStore.Get(TexId.button_g_mm_lit),
                TextureStore.Get(TexId.button_g_mm_dim),
                m_fixedPositions["mainMenu_button"],
                SfxStore.Get(SfxId.menu_mouseover),
                SfxStore.Get(SfxId.menu_click),
                new MainMenuEvent(),
                true,
                this);
            m_spriteBatchExMain.DrawnObjects.Add(m_mainMenuButton);


            //Options
            m_optionsButton = new MenuButton(
                TextureStore.Get(TexId.button_g_option_lit),
                TextureStore.Get(TexId.button_g_option_dim),
                m_fixedPositions["options_button"],
                SfxStore.Get(SfxId.menu_mouseover),
                SfxStore.Get(SfxId.menu_click),
                new OptionsEvent(),
                true,
                this);
            m_spriteBatchExMain.DrawnObjects.Add(m_optionsButton);

            m_surrenderButton = new MenuButton(
                    TextureStore.Get(TexId.button_g_surrender_lit),
                    TextureStore.Get(TexId.button_g_surrender_dim),
                    m_fixedPositions["escape_load_button"],
                    SfxStore.Get(SfxId.menu_mouseover),
                    SfxStore.Get(SfxId.menu_click),
                    new SurrenderGameEvent(),
                    true,
                    this);
            m_spriteBatchExMain.DrawnObjects.Add(m_surrenderButton);

            ////m_loadButton = new MenuButton(
            ////    TextureStore.Get(TexId.button_g_load_lit),
            ////    TextureStore.Get(TexId.button_g_load_dim),
            ////    m_fixedPositions["escape_load_button"],
            ////    SfxStore.Get(SfxId.menu_mouseover),
            ////    SfxStore.Get(SfxId.menu_click),
            ////    new LoadGameEvent(),
            ////    true,
            ////    this);
            ////m_spriteBatchExMain.DrawnObjects.Add(m_loadButton);

            m_justLoadedScreen = true;
        }

        public override void UninitScreen()
        {
            m_spriteBatchExMain.DrawnObjects.Clear();
        }



        public override void DrawBackgroundFirst(GraphicsDevice graphicsDevice)
        {
            m_backgroundSpriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend,
                null, null, null, null,
                m_currentScreenRectangle.GetMatrixTransform(graphicsDevice));

            // First side of the background is from 0 to .2
            m_backgroundSpriteBatch.Draw(TextureStore.Get(TexId.esc_menu_background_left),
                new Rectangle(
                    0, 0, 
                    (int)(graphicsDevice.Viewport.Width * .2f), graphicsDevice.Viewport.Height),
                null, Color.White, 0f, Vector2.Zero, SpriteEffects.None,
                1f);

            // Middle is from .2 to .8
            m_backgroundSpriteBatch.Draw(TextureStore.Get(TexId.esc_menu_background_middle),
                new Rectangle(
                    (int)(graphicsDevice.Viewport.Width * .2f), 0,
                    (int)(graphicsDevice.Viewport.Width * .6f), graphicsDevice.Viewport.Height),
                null, Color.White, 0f, Vector2.Zero, SpriteEffects.None,
                1f);

            // Other side of the background is from .8 to 1.0
            m_backgroundSpriteBatch.Draw(TextureStore.Get(TexId.esc_menu_background_right),
                new Rectangle((int)(
                    graphicsDevice.Viewport.Width * .8f), 0,
                    (int)(graphicsDevice.Viewport.Width * .2f), graphicsDevice.Viewport.Height),
                null, Color.White, 0f, Vector2.Zero, SpriteEffects.None,
                1f);

            m_backgroundSpriteBatch.End();
        }


        public override void Update(GameTime gameTime, MouseState mouseState, KeyboardState keyboardState)
        {
            //check it.
            if (KeyJustPressed(keyboardState,Keys.Escape) && !m_justLoadedScreen)
            {
                GetMainApp().clearCurrentScreenAndReturnToParent();
            }

            base.Update(gameTime, mouseState, keyboardState);
            m_justLoadedScreen = false;
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

            Point screenSize = new Point(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);

            TextureStore.Store.Preload(TexId.button_g_mm_dim);
            TextureStore.Store.Preload(TexId.button_g_mm_lit);
            m_fixedPositions["mainMenu_button"] = new Vector2(
                (int)leftPositionForCenteredObject(TextureStore.Get(TexId.button_g_mm_dim).Width, screenSize.X),
                screenSize.Y * .1f
                );

            TextureStore.Store.Preload(TexId.button_g_option_dim);
            TextureStore.Store.Preload(TexId.button_g_option_lit);
            m_fixedPositions["options_button"] = new Vector2(
                (int)leftPositionForCenteredObject(TextureStore.Get(TexId.button_g_option_dim).Width, screenSize.X),
                screenSize.Y * .27f
                );

            ////TextureStore.Store.Preload(TexId.button_g_load_dim);
            ////TextureStore.Store.Preload(TexId.button_g_load_lit);
            ////m_fixedPositions["escape_load_button"] = new Vector2(
            ////    (int)leftPositionForCenteredObject(TextureStore.Get(TexId.button_g_load_dim).Width, screenSize.X),
            ////    screenSize.Y * .43f);

            TextureStore.Store.Preload(TexId.button_g_surrender_dim);
            TextureStore.Store.Preload(TexId.button_g_surrender_lit);
            m_fixedPositions["escape_load_button"] = new Vector2(
                (int)leftPositionForCenteredObject(TextureStore.Get(TexId.button_g_surrender_dim).Width, screenSize.X),
                screenSize.Y * .43f
                );

            TextureStore.Store.Preload(TexId.button_g_cancel_dim);
            TextureStore.Store.Preload(TexId.button_g_cancel_lit);
            m_fixedPositions["cancel_button"] = new Vector2(
                (int)leftPositionForCenteredObject(TextureStore.Get(TexId.button_g_cancel_dim).Width, screenSize.X),
                screenSize.Y * .6f);

        }


    }
}