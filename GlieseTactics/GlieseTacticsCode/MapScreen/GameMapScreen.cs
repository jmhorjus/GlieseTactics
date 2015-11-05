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

	public class GameMapScreen : GameScreen
	{
        private class EndTurnEvent : Event
        {
            public EndTurnEvent() : base(DEFAULT_BUTTON_DELAY) { }
            
            public override void OnEvent(GameScreen parentScreen)
            {
                GameMapScreen screen = parentScreen as GameMapScreen;
                
                if (screen.Game.CurrentTurnStage != Game.TurnStage.EndTurn)
                    screen.Game.EndPlacement();
                else 
                    screen.Game.EndTurn();
                
                screen.EnableKeysAndMouse();
            }
        }

        private class ShowVictoryScreenEvent : Event
        {
            Game m_game;
            public ShowVictoryScreenEvent(Game game) : base(DEFAULT_BUTTON_DELAY) { m_game = game; }
            
            public override void OnEvent(GameScreen parentScreen)
            {
                GameMapScreen screen = parentScreen as GameMapScreen;
                screen.EnableKeysAndMouse();
                screen.GetMainApp().spawnNewSubScreen(game_screen_key.VictoryScreen, new ScreenRectangle(.1f,.1f,.8f,.8f));
                (parentScreen.GetMainApp().topActiveScreen() as VictoryScreen).Game = m_game;
            }
        }


        // Needs to be replaced with a TerrainType class eventually.
        MapEnvironment m_mapEnvironment;

        ScreenLayer m_mapScreenLayer;
        
        int m_lastMouseScrollValue = 1; //Previous Mouse Scroll Wheel Value //TODO:clean up

        Map m_map;
        public void SetNewMap(Game.MapSize mapSize, MapEnvironment environment, Game.MapType mapType)
        {

            Point newDimensions;
            switch(mapSize)
            {
                case Game.MapSize.Small:
                    newDimensions = new Point(10, 8); break;
                case Game.MapSize.Medium:
                    newDimensions = new Point(12, 10); break;
                case Game.MapSize.Large:
                    newDimensions = new Point(14, 12); break;
                default:
                    newDimensions = Point.Zero; break;
            }

            Game game = (m_map != null) ? m_map.Game : null;

            m_mapEnvironment = environment;
            m_backgroundTexture = environment.BackgroundTexture;
            m_backgroundColor = environment.BackgroundTint;

            // Create the map, which in the umbrella object containing all units, hexes, and game state.
            m_map = new Map(
                mapType == Game.MapType.Random ? newDimensions.X : 18, // 18x18 - max current dimensions for custom map.
                mapType == Game.MapType.Random ? newDimensions.Y : 18, 
                m_mapCamera,
                m_mapEnvironment.DefaultHexTexture, 
                m_mapEnvironment.BlockingHexTexture);
            // Add the ap to the drawn objects list - ensuring that it will be updated and drawn.
            m_mapScreenLayer.DrawnObjects.Add(m_map);

            this.Game = game;

            if (mapType == Game.MapType.Random)
            { 
                m_map.InitRandomMap();
                m_map.PlaceArmiesRandomlyOnMap();
            }
            else
            {
                if (mapSize == Game.MapSize.Small)
                    m_map.InitMapFromFile("Content/Maps/small_lanes.map");
                else if (mapSize == Game.MapSize.Medium)
                    m_map.InitMapFromFile("Content/Maps/medium_firing line.map"); 
                else
                    m_map.InitMapFromFile("Content/Maps/large_blood_bath.map"); 
            }
        }

        public Game Game
        {
            get { return (m_map != null) ? m_map.Game : null; }
            set 
            {
                if (m_map == null)
                    //a bit of a hack. (the map needs to just not be null since it's the container for the game)
                    m_map = new Map(1, 1, null, null, null); 
                m_map.Game = value;
                if (value == null)
                    return;
                m_player1Display.Commander = value.Players[0];
                m_player2Display.Commander = value.Players[1];
            }
        }

        Camera m_mapCamera;
        Cursor m_targetCursor;
        Cursor m_moveCursor;
        Cursor m_rechargeCursor;

        public GameMapScreen(MainApplication mainApp) : base(mainApp)
		{
            m_mapScreenLayer = new ScreenLayer(mainApp.GraphicsDevice);

            m_screenLayers.Insert(0, m_mapScreenLayer); // map layer goes on the bottom.
		}


        MenuButton m_endTurnButton;
        UnitStatsDisplayPanel m_unitDisplay;
        CommanderDisplaySocket m_player1Display;
        CommanderDisplaySocket m_player2Display;
        TextLabel m_damagePreviewLabel;
        TextLabel m_killsPreviewLabel;

        //TextLabel m_gameOverLabel;

        ClickableSprite m_victoryBanner;
        ConfettiFountain m_victoryConfettiFountain;
        ConfettiFountain m_victoryConfettiFountain_2;

        public override void InitScreen(ScreenRectangle portionOfScreen, GraphicsDevice graphicsDevice)
        {

            ///Initialize m_currentScreenRectangle and m_spriteBatchExMain
            m_currentScreenRectangle = portionOfScreen;
            m_mainScreenLayer.Transform = m_currentScreenRectangle.GetMatrixTransform(graphicsDevice);

            /// Play the active music.
            MediaPlayer.Play(m_activeMusic);
            m_musicVolumeMultiplier = 1.0f; 
            MediaPlayer.Volume = ConfigManager.GlobalManager.MusicVolume * m_musicVolumeMultiplier; 
            MediaPlayer.IsRepeating = true;


            // The Unit Display panel
            m_unitDisplay = new UnitStatsDisplayPanel(m_fixedPositions["unit_display"]);
            m_mainScreenLayer.DrawnObjects.Add(m_unitDisplay);

            bool gameAlreadyStarted = (m_map != null) && (m_map.Game != null) &&
                (m_map.Game.CurrentTurn != 0 ||
                m_map.Game.CurrentPlayer != m_map.Game.Players[0] ||
                m_map.Game.CurrentTurnStage != Game.TurnStage.PlacementBegin);


            // The player display panels:
            m_player1Display = new CommanderDisplaySocket(
                TextureStore.Get(TexId.portrait_empty),
                gameAlreadyStarted ? m_fixedRectangles["player1_display_shown"] : m_fixedRectangles["player1_display_onload"],
                m_defaultFont, this);
            m_player2Display = new CommanderDisplaySocket(TextureStore.Get(TexId.portrait_empty),
                gameAlreadyStarted ? m_fixedRectangles["player2_display_shown"] : m_fixedRectangles["player2_display_onload"],
                m_defaultFont, this);
            m_player1Display.Enabled = false; // Player display panels not enabled for clicking/dragging.
            m_player2Display.Enabled = false;
            m_mainScreenLayer.DrawnObjects.Add(m_player1Display);
            m_mainScreenLayer.DrawnObjects.Add(m_player2Display);

            if (m_map != null)
            {
                m_player1Display.Commander = m_map.Game.Players[0];
                m_player2Display.Commander = m_map.Game.Players[1];
            }

            // Animations.
            if (!gameAlreadyStarted)
            {
                m_player1Display.AddAnimation(new Animation(new TimeSpan(0, 0, 2), m_fixedRectangles["player1_display_onload"]));
                m_player2Display.AddAnimation(new Animation(new TimeSpan(0, 0, 2), m_fixedRectangles["player2_display_onload"]));
                m_player1Display.AddAnimation(new Animation(new TimeSpan(0, 0, 2), m_fixedRectangles["player1_display_shown"]));
                m_player2Display.AddAnimation(new Animation(new TimeSpan(0, 0, 2), m_fixedRectangles["player2_display_shown"]));
            }
            else
            {
                if (m_map.Game.Players[0] != m_map.Game.CurrentPlayer)
                {
                    m_player1Display.AddAnimation(new Animation(new TimeSpan(0, 0, 1), m_fixedRectangles["player1_display_shown"]));
                    m_player1Display.AddAnimation(new Animation(new TimeSpan(0, 0, 2), m_fixedRectangles["player1_display_hidden"]));
                }
                if (m_map.Game.Players[1] != m_map.Game.CurrentPlayer)
                {
                    m_player2Display.AddAnimation(new Animation(new TimeSpan(0, 0, 1), m_fixedRectangles["player2_display_shown"]));
                    m_player2Display.AddAnimation(new Animation(new TimeSpan(0, 0, 2), m_fixedRectangles["player2_display_hidden"]));
                }
            }


            // EndTurn Button
            bool showEndTurnButton = Game != null && 
                (Game.CurrentTurnStage == Game.TurnStage.EndTurn || 
                Game.CurrentTurnStage == Game.TurnStage.PlacementChooseUnit || 
                Game.CurrentTurnStage == Game.TurnStage.PlacementChooseDestination);
            m_endTurnButton = new MenuButton(
                TextureStore.Get(TexId.map_end_turn_lit),
                TextureStore.Get(TexId.map_end_turn_dim),
                showEndTurnButton ? m_fixedRectangles["endturn_shown"] : m_fixedRectangles["endturn_hidden"],
                SfxStore.Get(SfxId.menu_mouseover),
                SfxStore.Get(SfxId.menu_click),
                new EndTurnEvent(),
                true,
                this);
            m_endTurnButton.Enabled = showEndTurnButton; // Need to disable the button if it is hidden.
            m_mainScreenLayer.DrawnObjects.Add(m_endTurnButton);


            m_victoryBanner = new ClickableSprite(TextureStore.Get(TexId.graphic_victory), m_fixedRectangles["victory_banner"], Color.White, 1f, 0f, Vector2.Zero, 0f);
            m_victoryBanner.Visible = false;
            m_mainScreenLayer.DrawnObjects.Add(m_victoryBanner);

            m_victoryConfettiFountain = new ConfettiFountain(m_fixedPositions["confetti1"]);
            m_mainScreenLayer.DrawnObjects.Add(m_victoryConfettiFountain);
            m_victoryConfettiFountain_2 = new ConfettiFountain(m_fixedPositions["confetti2"]);
            m_mainScreenLayer.DrawnObjects.Add(m_victoryConfettiFountain_2);

         
            
        }


        static float s_zoomSpeed = 1.04f;
        static float s_scrollSpeed_Min = 2.0f;
        static float s_scrollSpeed_Max = 6.0f;
        public float ScrollSpeed 
        { 
            get { return s_scrollSpeed_Min + (s_scrollSpeed_Max - s_scrollSpeed_Min) * ConfigManager.GlobalManager.MapScrollSpeed; } 
        }



        /* 
         * 1.) We have a great way to track time built in.  Therefore we define an interval at which, if the current player is a 
         * computer, we will perform it's next action.  We'll have "last AI action time" and "AI action interval"(configurable), which
         * will tell us when we need to perform the AI's next turn step.  
         * 2.) Then we need the "TurnInstructions" class.  This thing contains all the information you need to fully execute a turn.
         * So a turn is defined as moving through the same "TurnStage" sequence as a player would:
         *     BeginTurn
         *     a.)(ChooseUnit) pick a unit to move 
         *     b.)(ChooseMoveDestination) pick a destination he for that units move
         *     c.)(ChooseAttackTarget) pick an action to perform with that unit 
         *     
         * 
         * 
            BeginTurn,
            ChooseUnit,
            ChooseMoveDestination,
            ChooseHeading,
            ChooseAttackType,
            ChooseAttackTarget,
            EndTurn,

        */

        public override void Update(GameTime gameTime, MouseState mouseState, KeyboardState keyboardState)
        {
            // Allows the user to bring up the menu by pressing esc.
            if (KeyJustPressed(keyboardState, Keys.Escape))
                this.GetMainApp().spawnNewSubScreen(game_screen_key.MainGame_EscMenu, new ScreenRectangle(.33f, .1f, .33f, .8f));



            // Update the EventManager - this takes care of executing any events that have been submitted at the right time.
            m_eventMgr.Update(gameTime, this);

            m_lastMouseState = mouseState;
            m_lastKeyboardState = keyboardState;

            bool mouseAlreadyIntercepted = false;

            /// Update the gui layer 
            if (m_keysAndMouseEnabled)
            {
                mouseAlreadyIntercepted = m_mainScreenLayer.Update(mouseState, gameTime, mouseAlreadyIntercepted); 
            }

            BeginPhaseGuiUpdates();
            if (m_keysAndMouseEnabled)
            {
                mouseAlreadyIntercepted = m_mapScreenLayer.Update(mouseState, gameTime, mouseAlreadyIntercepted);
            }

          

            /// Check for victory 
            if (Game.CurrentTurnStage == Game.TurnStage.GameOver && !m_victoryBanner.Visible)
            {
                m_victoryBanner.Visible = true;
                
                MediaPlayer.Stop();
                
                m_victoryConfettiFountain.Enabled = true;
                m_victoryConfettiFountain.Visible = true;
                m_victoryConfettiFountain_2.Enabled = true;
                m_victoryConfettiFountain_2.Visible = true;

                CommanderDisplaySocket winner;
                if (Game.WinningPlayer == Game.Players[0])
                {
                    winner = m_player1Display;
                    m_player2Display.AddAnimation(new Animation(new TimeSpan(0, 0, 1), m_fixedRectangles["player2_display_hidden"]));
                }
                else
                {
                    m_player1Display.AddAnimation(new Animation(new TimeSpan(0, 0, 1), m_fixedRectangles["player1_display_hidden"]));
                    winner = m_player2Display;
                }
                winner.AddAnimation(new Animation(new TimeSpan(0, 0, 2), m_fixedRectangles["player_display_winner"]));

                //TEMP
                //AddEvent(new ShowVictoryScreenEvent(Game));
            }


            // Perform the base update here ( this updates the map and individual controls belonging to each layer ).

            //m_map.Update(mouseState, m_mapCamera.Transform, gameTime);
           
            

            /// Update the mouse cursor, if neccessary:
            switch (Game.CurrentTurnStage)
            {
                case Game.TurnStage.ChooseMoveDestination:
                    if (Game.CurrentPlayer == Game.Players[0])
                        m_moveCursor.Rotation = 0f;
                    else
                        m_moveCursor.Rotation = (float)Math.PI;

                    ActiveMouseCursor = m_moveCursor;
                    break;
                case Game.TurnStage.ChooseHeading:
                //    m_moveCursor.Rotation = Unit.DirectionRotationAngles[(int)m_map.ChooseHeadingDirection] - (float)Math.PI/2 ;
                //    break;
                //case Game.TurnStage.ChooseAttackTarget:
                    if(m_map.SelectedHex.LastMouseOver)
                        ActiveMouseCursor = m_rechargeCursor;
                    else 
                        ActiveMouseCursor = m_targetCursor;
                    break;
                default:
                    ActiveMouseCursor = m_defaultMouseCursor;
                    break;
            }


            // If a unit on the map is selected, display its stats.
            if (m_map.SelectedHex != null && m_map.SelectedHex.Unit != null)
                m_unitDisplay.UnitToDisplay = m_map.SelectedHex.Unit;
            //else
            //    m_unitDisplay.UnitToDisplay = null;

            
            // Are there stats we should display for the current attack?
            if (m_map.ExpectedAttackStats.IsZero())
            {
                m_damagePreviewLabel.Text = "";
                m_killsPreviewLabel.Text = "";
            }
            else
            {
                m_damagePreviewLabel.Text = "Damage: " + m_map.ExpectedAttackStats.Damage;
                m_killsPreviewLabel.Text = "Kills:" + m_map.ExpectedAttackStats.Kills;

                if (m_map.ExpectedAttackStats.FriendlyDamage > 0)
                    m_killsPreviewLabel.Text += "\n<WARNING: FRIENDLY FIRE>";
            }


            // Screen size used for scroll/zoom mouse triggers.  
            Vector2 screenSize = new Vector2(GetMainApp().GraphicsDevice.Viewport.Width, GetMainApp().GraphicsDevice.Viewport.Height);
            Vector2 mousePosCenterZero = new Vector2(
                (mouseState.X - (screenSize.X / 2)) / (screenSize.X / 2),
                (mouseState.Y - (screenSize.Y / 2)) / (screenSize.Y / 2));

            //Mouse zooming controls 
            if (mouseState.ScrollWheelValue != m_lastMouseScrollValue)   
            {
                bool zoomIn = mouseState.ScrollWheelValue > m_lastMouseScrollValue;

                Vector2 moveVector = new Vector2(
                    .5f * screenSize.X * (s_zoomSpeed - 1) * (float)Math.Cos(-m_mapCamera.Rotation) * (mousePosCenterZero.X / m_mapCamera.Zoom),
                    .5f * screenSize.X * (s_zoomSpeed - 1) * (float)Math.Sin(-m_mapCamera.Rotation) * (mousePosCenterZero.X / m_mapCamera.Zoom));
                moveVector += new Vector2(
                    .5f * screenSize.Y * (s_zoomSpeed - 1) * (float)Math.Cos(-m_mapCamera.Rotation + Math.PI / 2) * (mousePosCenterZero.Y / m_mapCamera.Zoom),
                    .5f * screenSize.Y * (s_zoomSpeed - 1) * (float)Math.Sin(-m_mapCamera.Rotation + Math.PI / 2) * (mousePosCenterZero.Y / m_mapCamera.Zoom));

                m_mapCamera.Zoom *= zoomIn ? (s_zoomSpeed) : (1 / s_zoomSpeed);
                m_mapCamera.Translation += zoomIn ? (-moveVector) : (moveVector);

                m_lastMouseScrollValue = mouseState.ScrollWheelValue;
            }


            // Rotation controls. 
            if (keyboardState.IsKeyDown(Keys.A))
                m_mapCamera.Rotation += 0.01f;
            else if (keyboardState.IsKeyDown(Keys.D))
                m_mapCamera.Rotation -= 0.01f;

            // Scroll Controls.
            bool mouseScroll = ConfigManager.GlobalManager.MouseScrollEnabled;
            if (keyboardState.IsKeyDown(Keys.Up) ||
                (mouseScroll && -1f <= mousePosCenterZero.Y && mousePosCenterZero.Y <= -0.87f))
                m_mapCamera.Translation += new Vector2(
                    (ScrollSpeed * (float)Math.Cos(-m_mapCamera.Rotation + Math.PI / 2)) / m_mapCamera.Zoom,
                    (ScrollSpeed * (float)Math.Sin(-m_mapCamera.Rotation + Math.PI / 2)) / m_mapCamera.Zoom);
            else if (keyboardState.IsKeyDown(Keys.Down) ||
                (mouseScroll && 1f >= mousePosCenterZero.Y && mousePosCenterZero.Y >= 0.87f))
                m_mapCamera.Translation += new Vector2(
                    -(ScrollSpeed * (float)Math.Cos(-m_mapCamera.Rotation + Math.PI / 2)) / m_mapCamera.Zoom,
                    -(ScrollSpeed * (float)Math.Sin(-m_mapCamera.Rotation + Math.PI / 2)) / m_mapCamera.Zoom);
            if (keyboardState.IsKeyDown(Keys.Left) ||
                (mouseScroll && -1f <= mousePosCenterZero.X && mousePosCenterZero.X <= -0.87f))
                m_mapCamera.Translation += new Vector2(
                    (ScrollSpeed * (float)Math.Cos(-m_mapCamera.Rotation)) / m_mapCamera.Zoom,
                    (ScrollSpeed * (float)Math.Sin(-m_mapCamera.Rotation)) / m_mapCamera.Zoom);
            else if (keyboardState.IsKeyDown(Keys.Right) ||
                (mouseScroll && 1f >= mousePosCenterZero.X && mousePosCenterZero.X >= 0.87f))
                m_mapCamera.Translation += new Vector2(
                    -(ScrollSpeed * (float)Math.Cos(-m_mapCamera.Rotation)) / m_mapCamera.Zoom,
                    -(ScrollSpeed * (float)Math.Sin(-m_mapCamera.Rotation)) / m_mapCamera.Zoom);

            // Impose Limits on scolling and zooming
            // First the translation
            m_mapCamera.LimitTranslation(screenSize / 2);
            // Then the zoom
            if (m_mapCamera.Zoom < 0.2f)
                m_mapCamera.Zoom = 0.2f;
            else if (m_mapCamera.Zoom > 4.0f)
                m_mapCamera.Zoom = 4.0f;
            

            /// Bring up the end turn button when needed.  
            bool endTurnShouldBeDisplayed = Game.CurrentTurnStage == Game.TurnStage.EndTurn ||
                Game.CurrentTurnStage == Game.TurnStage.PlacementChooseUnit ||
                Game.CurrentTurnStage == Game.TurnStage.PlacementChooseDestination;
            if (m_endTurnButton.Enabled == false &&
                endTurnShouldBeDisplayed)
            {
                m_endTurnButton.Enabled = true;
                 
                // When we move to show the button, we also need to put the right texture on it.
                if (Game.CurrentTurnStage == Game.TurnStage.EndTurn)
                    m_endTurnButton.SetTextures(TextureStore.Get(TexId.map_end_turn_dim), TextureStore.Get(TexId.map_end_turn_lit));
                else
                    m_endTurnButton.SetTextures(TextureStore.Get(TexId.map_end_placement_dim), TextureStore.Get(TexId.map_end_placement_lit));


                m_endTurnButton.AddAnimation( new Animation(new TimeSpan(0, 0, 1), m_fixedRectangles["endturn_shown"]) );


            }
            else if (m_endTurnButton.Enabled == true &&
                !endTurnShouldBeDisplayed)
            {
                m_endTurnButton.Enabled = false;
                m_endTurnButton.AddAnimation(new Animation(new TimeSpan(0, 0, 1), m_fixedRectangles["endturn_hidden"]));
            }

		}
        
        /// <summary>
        /// called from Update
        /// </summary>
        private void BeginPhaseGuiUpdates()
        {
            /// Bring up the player portraits during the player's turn.
            if (Game.CurrentTurnStage == Game.TurnStage.BeginTurn || Game.CurrentTurnStage == Game.TurnStage.PlacementBegin)
            {
                if (Game.CurrentPlayer == Game.Players[0])
                {
                    m_player1Display.AddAnimation(new Animation(new TimeSpan(0, 0, 1), m_fixedRectangles["player1_display_shown"]));
                    m_player2Display.AddAnimation(new Animation(new TimeSpan(0, 0, 1), m_fixedRectangles["player2_display_hidden"]));
                    m_player1Display.ShowingSkills = true;
                    m_player2Display.ShowingSkills = false;

                }
                else if (Game.CurrentPlayer == Game.Players[1])
                {
                    m_player1Display.AddAnimation(new Animation(new TimeSpan(0, 0, 1), m_fixedRectangles["player1_display_hidden"]));
                    m_player2Display.AddAnimation(new Animation(new TimeSpan(0, 0, 1), m_fixedRectangles["player2_display_shown"]));
                    m_player1Display.ShowingSkills = false;
                    m_player2Display.ShowingSkills = true;
                }
            }
        }



        
        /// The Draw function is overridden so that the 
        /// map can be drawn using a camera class instead of 
        /// just a regular transform.
        public override void Draw(GameTime gameTime,
           GraphicsDevice graphicsDevice,
           GraphicsDeviceManager graphicsDeviceManager,
           bool isTopActiveScreen)
        {
            DrawBackgroundFirst(graphicsDevice);

            if (m_map != null)
            {
                m_mapScreenLayer.Transform = m_mapCamera.Transform * m_currentScreenRectangle.GetMatrixTransform(GetMainApp().GraphicsDevice);
                m_mapScreenLayer.Draw(gameTime);
            }


            // The main spritebatchEx is used for the gui interface, and needs 
            // to be drawn after the map.
            m_mainScreenLayer.Draw(gameTime);

            if (isTopActiveScreen)
                DrawMouseCursorLast(gameTime);
		}






        /// <summary>
        /// LoadContent defines semi-constant values that will not change unless the screen resolution is changed. 
        /// </summary>
        /// <param name="Content">The content managet from the main application.</param>
        /// <param name="graphicsDevice">The GraphicsDevice from the main app - knows the current resolution.</param>
        public override void LoadContent(ContentManager Content, GraphicsDevice graphicsDevice)
        {
            // Randomly choose an environment
            if (m_mapEnvironment == null)
                m_mapEnvironment = MapEnvironment.GetRandomEnvironment();
            // Load the background image from the environment.
            m_backgroundTexture = m_mapEnvironment.BackgroundTexture;
            m_backgroundColor = m_mapEnvironment.BackgroundTint;

            //Load the battle music.
            m_activeMusic = Content.Load<Song>("music/battle1");
            // default font
            m_defaultFont = Content.Load<SpriteFont>("Fonts/default");

            //Load the cursor sprite
            m_defaultMouseCursor = Cursor.LoadDefaultCursor();
            m_moveCursor = Cursor.LoadMoveCursor();
            m_targetCursor = Cursor.LoadTargetCursor();
            m_rechargeCursor = Cursor.LoadRechargeCursor();
            ActiveMouseCursor = m_defaultMouseCursor;

            m_damagePreviewLabel = new TextLabel("", m_defaultFont, Vector2.Zero, Color.Red, true);
            m_damagePreviewLabel.Offset = new Vector2(0, 
                (m_targetCursor.Texture.Height / -2) + (m_defaultFont.MeasureString("D").Y / 2)*0);
            m_targetCursor.TextLabels.Add(m_damagePreviewLabel);
            m_moveCursor.TextLabels.Add(m_damagePreviewLabel);

            m_killsPreviewLabel = new TextLabel("", m_defaultFont, Vector2.Zero, Color.Red, true);
            m_killsPreviewLabel.Offset = new Vector2(0, 
                (m_targetCursor.Texture.Height / 2) + (m_defaultFont.MeasureString("D").Y / 2)*0);
            m_targetCursor.TextLabels.Add(m_killsPreviewLabel);
            m_moveCursor.TextLabels.Add(m_killsPreviewLabel);

            //-----------------

            if (m_mapCamera == null) // only set this if it's not already set.
                m_mapCamera = new Camera(
                    new Vector2(-40, -60),
                    0.8f, 0,
                    new Vector2(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2));



            // For brevity later on, since these values are used heavily. 
            int SizeX = graphicsDevice.Viewport.Width;
            int SizeY = graphicsDevice.Viewport.Height;

            // EndTurn button textures and starting location
            m_fixedRectangles["endturn_shown"] = new Rectangle(
                (int)(SizeX * 0.7f), (int)(SizeY * 0.8f),
                (int)(SizeX * 0.25f), (int)(SizeY * 0.21f));
            m_fixedRectangles["endturn_hidden"] = new Rectangle(
                (int)(SizeX * 0.70f), (int)(SizeY * 1.0f),
                (int)(SizeX * 0.25f), (int)(SizeY * 0.21f));


            m_fixedRectangles["player1_display_shown"] = new Rectangle(
                (int)(SizeX * 0.05f), (int)(SizeY * 0.05f),
                (int)(SizeX * 0.12f), (int)(SizeY * 0.2f));
            m_fixedRectangles["player1_display_hidden"] = new Rectangle(
                (int)(SizeX * -0.30f), (int)(SizeY * 0.05f),
                (int)(SizeX * 0.12f), (int)(SizeY * 0.2f));
            m_fixedRectangles["player1_display_onload"] = new Rectangle(
                (int)(SizeX * 0.2f), (int)(SizeY * 0.2f),
                (int)(SizeX * 0.18f), (int)(SizeY * 0.3f));

            m_fixedRectangles["player2_display_shown"] = new Rectangle(
                (int)(SizeX * 0.675f), (int)(SizeY * 0.05f),
                (int)(SizeX * 0.12f), (int)(SizeY * 0.2f));
            m_fixedRectangles["player2_display_hidden"] = new Rectangle(
                (int)(SizeX * 1.10f), (int)(SizeY * 0.05f),
                (int)(SizeX * 0.12f), (int)(SizeY * 0.2f));
            m_fixedRectangles["player2_display_onload"] = new Rectangle(
                (int)(SizeX * .6f), (int)(SizeY * 0.2f),
                (int)(SizeX * .18f), (int)(SizeY * 0.3f));


            // Victory Stuff.
            m_fixedRectangles["player_display_winner"] = new Rectangle(
                (int)(SizeX * .4f), (int)(SizeY * 0.35f),
                (int)(SizeX * .18f), (int)(SizeY * 0.3f));
            m_fixedRectangles["victory_banner"] = new Rectangle(
                (int)(SizeX * .2f), (int)(SizeY * .00f),
                (int)(SizeX * .6f), (int)(SizeY * .3f));
            m_fixedPositions["confetti1"] = new Vector2((SizeX * .25f), (SizeY * .75f));
            m_fixedPositions["confetti2"] = new Vector2((SizeX * .75f), (SizeY * .75f));
            m_fixedPositions["unit_display"] = new Vector2(
                0,
                SizeY - TextureStore.Get(TexId.unit_stats_frame).Height);
        }

	}
}
