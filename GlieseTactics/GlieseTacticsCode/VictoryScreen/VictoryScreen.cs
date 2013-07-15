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
    class VictoryScreen : GameScreen
    {

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

        public VictoryScreen(MainApplication mainApp) : base(mainApp)
        {

        }
       
        MenuButton MainMenuButton;
        CommanderDisplaySocket player_winner;
        CommanderDisplaySocket player_loser;
        Game m_game;
        public Game Game
        {
            get { return m_game; }
            set 
        
            { 
                m_game = value;
                player_winner.Commander = m_game.WinningPlayer;
                player_loser.Commander = m_game.LosingPlayer;
            }
        
        }
        
        public override void InitScreen(ScreenRectangle portionOfScreen, GraphicsDevice graphicsDevice)
        {
            m_currentScreenRectangle = portionOfScreen;
            m_mainScreenLayer.Transform = m_currentScreenRectangle.GetMatrixTransform(graphicsDevice);


            MainMenuButton = new MenuButton
            (TextureStore.Get(TexId.button_mm_lit), TextureStore.Get(TexId.button_mm_dim),
            m_fixedRectangles["MainMenuButton"], SfxStore.Get(SfxId.menu_mouseover), SfxStore.Get(SfxId.menu_click), new MainMenuEvent(), true, this);
            m_mainScreenLayer.DrawnObjects.Add(MainMenuButton);


            // The player display panels:
            player_winner = new CommanderDisplaySocket(
                TextureStore.Get(TexId.portrait_empty),
                m_fixedRectangles["winner_display"],
                m_defaultFont);
            player_loser = new CommanderDisplaySocket(TextureStore.Get(TexId.portrait_empty),
                m_fixedRectangles["loser_display"],
                m_defaultFont);
            player_winner.Enabled = false; // Player display panels not enabled for clicking/dragging.
            player_loser.Enabled = false;
            m_mainScreenLayer.DrawnObjects.Add(player_winner);
            m_mainScreenLayer.DrawnObjects.Add(player_loser);



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

            m_defaultFont = Content.Load<SpriteFont>("Fonts/playerName");

            int SizeX = graphicsDevice.Viewport.Width;
            int SizeY = graphicsDevice.Viewport.Height;

            m_fixedRectangles["victory_banner"] = new Rectangle(
                (int)(SizeX * .33f), (int)(SizeY * .33f),
                (int)(SizeX * .66f), (int)(SizeY * .66f));

            m_fixedRectangles["winner_display"] = new Rectangle(
                (int)(SizeX * 0.2f), (int)(SizeY * 0.2f),
                (int)(SizeX * 0.12f), (int)(SizeY * 0.24f));

            m_fixedRectangles["loser_display"] = new Rectangle(
                (int)(SizeX * .6f), (int)(SizeY * 0.2f),
                (int)(SizeX * .12f), (int)(SizeY * 0.24f));

            m_fixedRectangles["MainMenuButton"] = new Rectangle(
                (int)(SizeX * .7f), (int)(SizeY * 0.75f),
                (int)(SizeX * .2f), (int)(SizeY * 0.1f));

            //m_justLoadedScreen = true;

        }
 
    }
}
