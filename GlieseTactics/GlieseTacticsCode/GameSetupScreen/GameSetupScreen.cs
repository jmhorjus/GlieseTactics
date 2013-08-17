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
using System.IO;

namespace Gliese581g
{
    public class GameSetupScreen : GameScreen
    {
        // This event quits to the main menu (or to whatever screen spawned this screen)
        private class CancelEvent : Event
        {
            //400 milliseconds default pause.
            public CancelEvent() : base(DEFAULT_BUTTON_DELAY) { }
            public CancelEvent(TimeSpan time) : base(time) { }

            public override void OnEvent(GameScreen parentScreen)
            {   // This event exits the program.
                parentScreen.EnableKeysAndMouse();
                parentScreen.GetMainApp().changeToNewBaseActiveScreen(game_screen_key.MainMenuScreen);
            }
        }
        private class NewPlayerEvent : Event
        {
            CommanderDisplaySocket m_socket;

            public NewPlayerEvent(CommanderDisplaySocket socket) : base(new TimeSpan(0)) { m_socket = socket; }
            public NewPlayerEvent(CommanderDisplaySocket socket, TimeSpan time) : base(time) { m_socket = socket; }

            public override void OnEvent(GameScreen parentScreen)
            {
                // We have to disable the keys and mouse while the new-player dialog is open.
                parentScreen.DisableKeysAndMouse(); 

                NewPlayerScreen npScreen = NewPlayerScreen.GetInstance;
                npScreen.callbackGameSetupScreen = parentScreen as GameSetupScreen;
                npScreen.callbackPlayerSocket = m_socket;
                npScreen.graphics = parentScreen.GetMainApp().GraphicsDevice;

                //if displayed once, never display again
                if (!npScreen.Visible)
                    npScreen.ShowDialog();
                
            }
        }

        private class StartGameEvent : Event
        {
            public StartGameEvent() : base(DEFAULT_BUTTON_DELAY) { }
            public StartGameEvent(TimeSpan time) : base(time) { }

            public override void OnEvent(GameScreen parentScreen)
            {
                parentScreen.EnableKeysAndMouse();
                // Need a way to pass the game setup parameters to the main game. 
                parentScreen.GetMainApp().changeToNewBaseActiveScreen(game_screen_key.MainGame);

                GameMapScreen mapScreen = parentScreen.GetMainApp().topActiveScreen() as GameMapScreen;
                GameSetupScreen setupScreen = parentScreen as GameSetupScreen;
                List<Commander> players = new List<Commander>();
                players.Add(setupScreen.Player1);
                players.Add(setupScreen.Player2);
                mapScreen.Game = new Game(players, setupScreen.VictoryType);

                mapScreen.Game.InitArmies(setupScreen.ArmySize);

                mapScreen.SetNewMap(
                    setupScreen.MapSize, 
                    MapEnvironment.GetRandomEnvironment(),
                    setupScreen.MapType);
            }
        }

        private class PlayerScrollEvent : Event
        { 
            int m_up;
            public PlayerScrollEvent(int up) : base(new TimeSpan(0)) {m_up = up;}
            public PlayerScrollEvent(int up, TimeSpan time) : base(time) { m_up = up;}

            public override void OnEvent(GameScreen parentScreen)
            {
                GameSetupScreen screen = parentScreen as GameSetupScreen;
                screen.scrollPlayerProfiles(m_up);
            }
        }


        /// Adjusts the positions of all the PlayerDisplaySockets in the sidebar list based on a 
        /// change in the current scroll position.
        public void scrollPlayerProfiles(int up, bool forceAnimation = false)
        {
            int newScrollPos = m_sidebarScrollPosition + up;
            if (newScrollPos > m_playerSidebarList.Count - 2)
                newScrollPos = m_playerSidebarList.Count - 2 ;
            if (newScrollPos < 1)
                newScrollPos = 1;

            if (newScrollPos == m_sidebarScrollPosition && !forceAnimation)
                return;

            m_sidebarScrollPosition = newScrollPos;
            for(int ii = 0; ii < m_playerSidebarList.Count; ii++)
            {
                CommanderDisplaySocket socket = m_playerSidebarList[ii];
                int reletivePosition = m_sidebarScrollPosition - ii;
                Rectangle destination = Rectangle.Empty;
                if (reletivePosition > 1)
                { destination = m_fixedRectangles["player_sidebar_off_top"]; socket.Enabled = false; }
                else if (reletivePosition == 1)
                { destination = m_fixedRectangles["player_sidebar1"]; socket.Enabled = true; }
                else if (reletivePosition == 0)
                { destination = m_fixedRectangles["player_sidebar2"]; socket.Enabled = true; } 
                else if (reletivePosition == -1)
                { destination = m_fixedRectangles["player_sidebar3"]; socket.Enabled = true; } 
                else if (reletivePosition < -1)
                { destination = m_fixedRectangles["player_sidebar_off_bottom"]; socket.Enabled = false; }

                socket.AddAnimation(new Animation(new TimeSpan(0, 0, 0, 0, 600), destination));
            }
        }


        public GameSetupScreen(MainApplication mainApp)
            : base(mainApp)
        {
        }
  


        MenuButton m_cancelButton;
        MenuButton m_startButton;
        MenuButton m_buttonArrowUp;
        MenuButton m_buttonArrowDown;

        int m_sidebarScrollPosition;
        const int PLAYER_SIDEBAR_MIN_TOTAL_SLOTS = 8;
        const int PLAYER_SIDEBAR_MIN_EMPTY_SLOTS = 2;
        List<CommanderDisplaySocket> m_playerSidebarList = new List<CommanderDisplaySocket>();
        CommanderTrashBin m_playerTrash;

        CommanderDisplaySocket m_playerSocket_player1;
        public Commander Player1 { get { return m_playerSocket_player1.Commander; } }
        CommanderDisplaySocket m_playerSocket_player2;
        public Commander Player2 { get { return m_playerSocket_player2.Commander; } }
        TextLabel m_labelVS;
        SpriteFont m_fontVS;

        RadioButton m_armySmall;
        RadioButton m_armyMedium;
        RadioButton m_armyLarge;

        RadioButton m_mapSmall;
        RadioButton m_mapMedium;
        RadioButton m_mapLarge;

        RadioButton m_mapRandom;
        RadioButton m_mapSymmetrical;

        RadioButton m_victoryAssassination;
        RadioButton m_victoryElimination;
  

        public override void InitScreen(ScreenRectangle portionOfScreen, Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice)
        {
            m_currentScreenRectangle = portionOfScreen;
            m_mainScreenLayer.Transform = m_currentScreenRectangle.GetMatrixTransform(graphicsDevice);
            m_playerTrash = new CommanderTrashBin(m_fixedRectangles["player_trash"]);
            m_mainScreenLayer.DrawnObjects.Add(m_playerTrash);

            //Get the full path of the profile folder
            string playerProfilePath = ConfigManager.GlobalManager.PlayerProfileDirectory;

            // Load all the files 
            int playerProfileIndex = 0;
            if (Directory.Exists(playerProfilePath))
            {
                foreach (string file in Directory.EnumerateFiles(playerProfilePath, "*.xml"))
                {
                    //Put together the file path
                    string xmlFilePath = playerProfilePath + Path.GetFileNameWithoutExtension(file) + ".xml";
                    // Create the player, and put it in a new socket on the sidebar.
                    AddEmptySocketToSidebar().Commander = Commander.LoadXmlFile(xmlFilePath, GetMainApp().GraphicsDevice);
                    playerProfileIndex += 1;
                }
            }

            for (int ii = 0; ii < PLAYER_SIDEBAR_MIN_EMPTY_SLOTS || m_playerSidebarList.Count < PLAYER_SIDEBAR_MIN_TOTAL_SLOTS; ii++)
            {
                AddEmptySocketToSidebar();
            }

            // Get the portraits into position.
            scrollPlayerProfiles(0, true);

            // Create the up and down buttons.
            m_buttonArrowDown = new MenuButton(TextureStore.Get(
                TexId.button_arrow_down_lit),
                TextureStore.Get(TexId.button_arrow_down), 
                m_fixedRectangles["button_arrow_down"], 
                SfxStore.Get(SfxId.menu_mouseover),
                SfxStore.Get(SfxId.menu_click), 
                new PlayerScrollEvent(1), 
                false, this);
            m_buttonArrowUp = new MenuButton(
                TextureStore.Get(TexId.button_arrow_up_lit),
                TextureStore.Get(TexId.button_arrow_up), 
                m_fixedRectangles["button_arrow_up"], 
                SfxStore.Get(SfxId.menu_mouseover),
                SfxStore.Get(SfxId.menu_click),
                new PlayerScrollEvent(-1), 
                false, this);
            m_mainScreenLayer.DrawnObjects.Add(m_buttonArrowDown);
            m_mainScreenLayer.DrawnObjects.Add(m_buttonArrowUp);
               

            m_playerSocket_player1 = new CommanderDisplaySocket(TextureStore.Get(TexId.portrait_empty),
                m_fixedRectangles["player_1"], m_defaultFont, this);
            m_playerSocket_player2 = new CommanderDisplaySocket(TextureStore.Get(TexId.portrait_empty),
               m_fixedRectangles["player_2"], m_defaultFont, this);

            m_mainScreenLayer.DrawnObjects.Add(m_playerSocket_player1);
            m_mainScreenLayer.DrawnObjects.Add(m_playerSocket_player2);

            m_labelVS = new TextLabel("VS", m_fontVS, m_fixedPositions["vs_label"], Color.Black, true);
            m_mainScreenLayer.DrawnObjects.Add(m_labelVS);

            ///
            /// The four groups of radio buttons.
            /// 
            m_mapSmall = new RadioButton(TextureStore.Get(TexId.button_map_small),
                m_fixedRectangles["map_small"], this, "map", 1);
            m_mapMedium = new RadioButton(TextureStore.Get(TexId.button_map_medium),
                m_fixedRectangles["map_medium"], this, "map", 2);
            m_mapLarge = new RadioButton(TextureStore.Get(TexId.button_map_large),
                m_fixedRectangles["map_large"], this, "map", 3);

            m_mapRandom = new RadioButton(TextureStore.Get(TexId.button_map_random),
                m_fixedRectangles["map_random"], this, "map_type", 1);
            m_mapSymmetrical = new RadioButton(TextureStore.Get(TexId.button_map_symmetrical),
                m_fixedRectangles["map_symmetrical"], this, "map_type", 2);

            m_armySmall = new RadioButton(TextureStore.Get(TexId.button_army_small),
                m_fixedRectangles["army_small"], this, "army", 1);
            m_armyMedium = new RadioButton(TextureStore.Get(TexId.button_army_medium),
                m_fixedRectangles["army_medium"], this, "army", 2);
            m_armyLarge = new RadioButton(TextureStore.Get(TexId.button_army_large),
                m_fixedRectangles["army_large"], this, "army", 3);

            m_victoryAssassination = new RadioButton(TextureStore.Get(TexId.button_victory_assassination),
                m_fixedRectangles["victory_assassination"], this, "victory", 1);
            m_victoryElimination = new RadioButton(TextureStore.Get(TexId.button_victory_elimination),
                m_fixedRectangles["victory_elimination"], this, "victory", 2); m_mainScreenLayer.DrawnObjects.Add(m_mapSmall);
            
            m_mainScreenLayer.DrawnObjects.Add(m_mapMedium);
            m_mainScreenLayer.DrawnObjects.Add(m_mapLarge);
          
            m_mainScreenLayer.DrawnObjects.Add(m_mapRandom);
            m_mainScreenLayer.DrawnObjects.Add(m_mapSymmetrical);
            
            m_mainScreenLayer.DrawnObjects.Add(m_armySmall);
            m_mainScreenLayer.DrawnObjects.Add(m_armyMedium);
            m_mainScreenLayer.DrawnObjects.Add(m_armyLarge);

            m_mainScreenLayer.DrawnObjects.Add(m_victoryAssassination);
            m_mainScreenLayer.DrawnObjects.Add(m_victoryElimination);
       
            /// Clear any previously sellected buttons. 
            RadioButton.ClearGroup("map"); 
            RadioButton.ClearGroup("map_type");
            RadioButton.ClearGroup("army");
            RadioButton.ClearGroup("victory");


            // Cancel Button
            m_cancelButton = new MenuButton(
                TextureStore.Get(TexId.gamesetup_cancel_lit),
                TextureStore.Get(TexId.gamesetup_cancel_dim),
                m_fixedRectangles["cancel_hidden"],
                SfxStore.Get(SfxId.menu_mouseover),
                SfxStore.Get(SfxId.menu_click),
                new CancelEvent(),
                true,
                this);
            m_mainScreenLayer.DrawnObjects.Add(m_cancelButton);
            m_cancelButton.AddAnimation( new Animation(new TimeSpan(0,0,1), m_fixedRectangles["cancel_shown"]) );

            // Start Button
            m_startButton = new MenuButton(
                TextureStore.Get(TexId.gamesetup_start_lit),
                TextureStore.Get(TexId.gamesetup_start_dim),
                m_fixedRectangles["start_hidden"],
                SfxStore.Get(SfxId.menu_mouseover),
                SfxStore.Get(SfxId.menu_click),
                new StartGameEvent(),
                true,
                this);
            m_startButton.Enabled = false;
            m_mainScreenLayer.DrawnObjects.Add(m_startButton);

        }


        public override void UninitScreen()
        {
            base.UninitScreen();
            m_playerSidebarList.Clear();
        }



        public override void Update(GameTime gameTime, MouseState mouseState, KeyboardState keyboardState)
        {
            if (m_startButton.Enabled == false && ReadyForNewGame)
            {
                m_startButton.Enabled = true;
                m_startButton.AddAnimation( new Animation(new TimeSpan(0, 0, 1), m_fixedRectangles["start_shown"]) );
            }
            else if (m_startButton.Enabled == true && !ReadyForNewGame)
            {
                m_startButton.Enabled = false;
                m_startButton.AddAnimation(new Animation(new TimeSpan(0, 0, 1), m_fixedRectangles["start_hidden"]));
            }

            base.Update(gameTime, mouseState, keyboardState);
        }


        // Utility functio that adds an empty socket to the player sidebar list.
        protected CommanderDisplaySocket AddEmptySocketToSidebar()
        {
            CommanderDisplaySocket socket;
            socket = new CommanderDisplaySocket(TextureStore.Get(TexId.portrait_newplayer),
                m_fixedRectangles["player_sidebar_off_bottom"], m_defaultFont, this);
            socket.SetClickWhenEmptyEvent(new NewPlayerEvent(socket), this);
            m_playerSidebarList.Add(socket);
            m_mainScreenLayer.DrawnObjects.Add(socket);
            return socket;
        }

        public bool ReadyForNewGame
        {  
            get 
            {
                return m_playerSocket_player1.Commander != null &&
                    m_playerSocket_player2.Commander != null &&
                    VictoryType != Game.VictoryType.NotSet &&
                    MapSize != Game.MapSize.NotSet &&
                    ArmySize != Game.ArmySize.NotSet &&
                    MapType != Game.MapType.NotSet;
            }  
        }


        public Game.VictoryType VictoryType
        {
            get
            {
                if (m_victoryAssassination.Selected)
                    return Game.VictoryType.Assassination;
                else if (m_victoryElimination.Selected)
                    return Game.VictoryType.Elimination;
                else
                    return Game.VictoryType.NotSet;
            }
        }

        public Game.MapSize MapSize
        {
            get
            {
                if (m_mapSmall.Selected)
                    return Game.MapSize.Small;
                else if (m_mapMedium.Selected)
                    return Game.MapSize.Medium;
                else if (m_mapLarge.Selected)
                    return Game.MapSize.Large;
                else
                    return Game.MapSize.NotSet;
            }
        }

        public Game.ArmySize ArmySize
        {
            get
            {
                if (m_armySmall.Selected)
                    return Game.ArmySize.Small;
                else if (m_armyMedium.Selected)
                    return Game.ArmySize.Medium;
                else if (m_armyLarge.Selected)
                    return Game.ArmySize.Large;
                else
                    return Game.ArmySize.NotSet;
            }
        }

        public Game.MapType MapType
        {
            get
            {
                if (m_mapRandom.Selected)
                    return Game.MapType.Random;
                else if (m_mapSymmetrical.Selected)
                    return Game.MapType.FromFile;
                else
                    return Game.MapType.NotSet;
            }
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
            m_fontVS = Content.Load<SpriteFont>("Fonts/default");


            // For brevity later on, since these values are used heavily. 
            int SizeX = graphicsDevice.Viewport.Width;
            int SizeY = graphicsDevice.Viewport.Height;

            // Cancel button textures and starting location 
            m_fixedRectangles["cancel_shown"] = new Rectangle(
                (int)(SizeX * 0.175f), (int)(SizeY * 0.875f),
                (int)(SizeX * 0.2f), (int)(SizeY * 0.125f));
            m_fixedRectangles["cancel_hidden"] = new Rectangle(
                (int)(SizeX * 0.175f), (int)(SizeY * 1.0f),
                (int)(SizeX * 0.2f), (int)(SizeY * 0.125f));

            // NewPlayer button textures and starting location 
            m_fixedRectangles["start_shown"] = new Rectangle(
                (int)(SizeX * 0.6f), (int)(SizeY * 0.875f),
                (int)(SizeX * 0.2f), (int)(SizeY * 0.125f));
            m_fixedRectangles["start_hidden"] = new Rectangle(
                (int)(SizeX * 0.6f), (int)(SizeY * 1.0f),
                (int)(SizeX * 0.2f), (int)(SizeY * 0.125f));

            // Arrows for Playersockets sidebars locations
            m_fixedRectangles["button_arrow_up"] = new Rectangle(
                (int)(SizeX * 0.065f), (int)(SizeY * 0.02f),
                (int)(SizeX * 0.07f), (int)(SizeY * 0.06f));
            m_fixedRectangles["button_arrow_down"] = new Rectangle(
                (int)(SizeX * 0.065f), (int)(SizeY * 0.8f),
                (int)(SizeX * 0.07f), (int)(SizeY * 0.06f));

            // Player Trash Location
            m_fixedRectangles["player_trash"] = new Rectangle(
                (int)(SizeX * 0.06f), (int)(SizeY * 0.875f),
                (int)(SizeX * 0.085f), (int)(SizeY * 0.085f));

            // Playersocket sidebar locations
            m_fixedRectangles["player_sidebar1"] = new Rectangle(
                (int)(SizeX * 0.06f), (int)(SizeY * 0.11f),
                (int)(SizeX * 0.075f), (int)(SizeY * 0.15f));
            m_fixedRectangles["player_sidebar2"] = new Rectangle(
                (int)(SizeX * 0.06f), (int)(SizeY * 0.35f),
                (int)(SizeX * 0.075f), (int)(SizeY * 0.15f));
            m_fixedRectangles["player_sidebar3"] = new Rectangle(
                (int)(SizeX * 0.06f), (int)(SizeY * 0.59f),
                (int)(SizeX * 0.075f), (int)(SizeY * 0.15f));
            m_fixedRectangles["player_sidebar_off_top"] = new Rectangle(
                (int)(SizeX * -0.36f), (int)(SizeY * -0.3f),
                (int)(SizeX * 0.075f), (int)(SizeY * 0.15f));
            m_fixedRectangles["player_sidebar_off_bottom"] = new Rectangle(
                (int)(SizeX * -0.36f), (int)(SizeY * 1.1f),
                (int)(SizeX * 0.075f), (int)(SizeY * 0.15f));

            // Playersocket selected players locations
            m_fixedRectangles["player_1"] = new Rectangle(
                (int)(SizeX * 0.225f), (int)(SizeY * 0.1f),
                (int)(SizeX * 0.10f), (int)(SizeY * 0.20f));
            m_fixedRectangles["player_2"] = new Rectangle(
                (int)(SizeX * 0.225f), (int)(SizeY * 0.475f),
                (int)(SizeX * 0.10f), (int)(SizeY * 0.20f));

            // Versus text label location
            m_fixedPositions["vs_label"] = new Vector2(
                m_fixedRectangles["player_1"].Left + m_fixedRectangles["player_1"].Width / 2,
                (m_fixedRectangles["player_1"].Bottom + m_fixedRectangles["player_2"].Top) / 2 + m_fixedRectangles["player_1"].Height * .1f);


            Vector2 buttonSize = new Vector2(0.17f, 0.15f);
            // Army buttons locations
            m_fixedRectangles["army_small"] = new Rectangle(
                (int)(SizeX * 0.4f), (int)(SizeY * 0.08f),
                (int)(SizeX * buttonSize.X), (int)(SizeY * buttonSize.Y));
            m_fixedRectangles["army_medium"] = new Rectangle(
                (int)(SizeX * 0.59f), (int)(SizeY * 0.08f),
                (int)(SizeX * buttonSize.X), (int)(SizeY * buttonSize.Y));
            m_fixedRectangles["army_large"] = new Rectangle(
                (int)(SizeX * 0.78f), (int)(SizeY * 0.08f),
                (int)(SizeX * buttonSize.X), (int)(SizeY * buttonSize.Y));

            // Map buttons locations
            m_fixedRectangles["map_small"] = new Rectangle(
                (int)(SizeX * 0.4f), (int)(SizeY * 0.28f),
                (int)(SizeX * buttonSize.X), (int)(SizeY * buttonSize.Y));
            m_fixedRectangles["map_medium"] = new Rectangle(
                (int)(SizeX * 0.59f), (int)(SizeY * 0.28f),
                (int)(SizeX * buttonSize.X), (int)(SizeY * buttonSize.Y));
            m_fixedRectangles["map_large"] = new Rectangle(
                (int)(SizeX * 0.78f), (int)(SizeY * 0.28f),
                (int)(SizeX * buttonSize.X), (int)(SizeY * buttonSize.Y));

            //Additional Map buttons
            m_fixedRectangles["map_random"] = new Rectangle(
                (int)(SizeX * 0.4f), (int)(SizeY * 0.48f),
                (int)(SizeX * buttonSize.X), (int)(SizeY * buttonSize.Y));
            m_fixedRectangles["map_symmetrical"] = new Rectangle(
                (int)(SizeX * 0.59f), (int)(SizeY * 0.48f),
                (int)(SizeX * buttonSize.X), (int)(SizeY * buttonSize.Y));

            //Victory conditions buttons locations
            m_fixedRectangles["victory_assassination"] = new Rectangle(
                (int)(SizeX * 0.4f), (int)(SizeY * 0.68f),
                (int)(SizeX * buttonSize.X), (int)(SizeY * buttonSize.Y));
            m_fixedRectangles["victory_elimination"] = new Rectangle(
                (int)(SizeX * 0.59f), (int)(SizeY * 0.68f),
                (int)(SizeX * buttonSize.X), (int)(SizeY * buttonSize.Y));

        }


    }
}
