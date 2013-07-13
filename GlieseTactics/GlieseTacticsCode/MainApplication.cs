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

    /// Enum for use as a key into a dictionary of screens.  
    public enum game_screen_key
    {
        IntroScreen,
        MainMenuScreen,
        OptionsScreen,
        GameSetupScreen,
        NewPlayerScreen,
        SkillsScreen,
        ArmyCustomizationScreen,
        MainGame,
        MainGame_EscMenu,
        GameOver,
        VictoryScreen
    };
    

    /// <summary>
    /// The main application class that inherits from Microsoft.Xna.Framework.Game.
    /// There is only one of these. 
    /// It delegates function to the currently active screen, and manages transitions between screens.
    /// </summary>
    public class MainApplication : Microsoft.Xna.Framework.Game
    {
        public GraphicsDeviceManager GraphicsDeviceManager;
        private Dictionary<game_screen_key, GameScreen> m_allGameScreens;

        /// The list of active screens - the base screen is always activeGameScreens[0], 
        /// and the actual active screen is activeGameScreens[activeGameScreens.Count - 1]
        private List<GameScreen> m_activeGameScreens = null;

        public GameScreen baseActiveScreen()
        {
            return m_activeGameScreens[0];
        }
        public GameScreen topActiveScreen()
        { 
            return m_activeGameScreens[m_activeGameScreens.Count - 1];
        }
        public GameScreen topScreensParent() 
        {
            if (m_activeGameScreens.Count >= 2)
                return m_activeGameScreens[m_activeGameScreens.Count - 2];
            else
                return null;
        }
        public GameScreen GetScreenById(game_screen_key key)
        {
            return m_allGameScreens[key];
        }

        
        /// <summary>
        /// Constructor - set up the main graphicsDeviceManager and create allGameScreens dictionary.
        /// </summary>
        public MainApplication()
        {
            /// Check the configuration for the currently set resolution?
            GraphicsDeviceManager = new GraphicsDeviceManager(this);
            GraphicsDeviceManager.PreferredBackBufferWidth = 1000; 
            GraphicsDeviceManager.PreferredBackBufferHeight = 562;
            GraphicsDeviceManager.IsFullScreen = false;
            
            m_allGameScreens = new Dictionary<game_screen_key,GameScreen>();

            /// The directory from which all graphics/sound content will be loaded.  
            Content.RootDirectory = "Content";
            /// Initialize the global TextureStore.
            TextureStore.InitPublicStore(Content);
            SfxStore.InitPublicStore(Content);
            ConfigManager.InitPublicConfigManager();
        }


        /// All one-time initialization has been done in the constructor for now.
        /// (not sure the advantage of using Initialize)
        protected override void Initialize()
        {
            /// Create the game screen objects, and put them in the dictionary.
            m_allGameScreens[game_screen_key.IntroScreen] = new IntroScreen(this);
            m_allGameScreens[game_screen_key.MainMenuScreen] = new MainMenuScreen(this);
            m_allGameScreens[game_screen_key.OptionsScreen] = new OptionsScreen(this);
            m_allGameScreens[game_screen_key.MainGame] = new GameMapScreen(this);
            m_allGameScreens[game_screen_key.GameSetupScreen] = new GameSetupScreen(this);
            m_allGameScreens[game_screen_key.NewPlayerScreen] = new NewPlayerSetupScreen(this);
            m_allGameScreens[game_screen_key.SkillsScreen] = new SkillsScreen(this);
            m_allGameScreens[game_screen_key.ArmyCustomizationScreen] = new ArmySetupScreen(this);
            m_allGameScreens[game_screen_key.MainGame_EscMenu] = new EscMenuScreen(this);
            m_allGameScreens[game_screen_key.VictoryScreen] = new VictoryScreen(this);

            /// Set the initial activeGameScreen
            m_activeGameScreens = new List<GameScreen>();

            //m_activeGameScreens.Add(m_allGameScreens[game_screen_key.MainMenu]);
            m_activeGameScreens.Add(m_allGameScreens[game_screen_key.IntroScreen]);

            ///Shake
            ShakeForm.s_MainApp = this;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Call LoadContent for all the different screens.
            foreach(GameScreen screen in m_allGameScreens.Values)
            {
                screen.LoadContent(this.Content, this.GraphicsDevice);
            }
            // Go ahead and load these fonts only once in the main application.
            UnitStatsDisplayPanel.InitStaticFonts(Content.Load<SpriteFont>("Fonts/unitName"), Content.Load<SpriteFont>("Fonts/unitStats"));
            DrawnTextEffect.InitStaticFonts(Content.Load<SpriteFont>("Fonts/numbereffect"));

            // Must initialize the first top active screen.
            topActiveScreen().InitScreen(ScreenRectangle.WholeScreen, this.GraphicsDevice);
        }

        /// Shouldn't need to do anything in this function I think.
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            // (Shouldn't be any non-ContentManager content, right?)
        }


        /// The Update event is forwarded to the active screen.
        protected override void Update(GameTime gameTime)
        {
            // The Update event is forwarded to the top active screen.
            topActiveScreen().Update(gameTime, Mouse.GetState(), Keyboard.GetState());

            base.Update(gameTime);
        }

        /// The Draw event is forwarded to the active screen.
        protected override void Draw(GameTime gameTime)
        {
            // Clear the device - the individual screens shouldn't do this.  
            GraphicsDevice.Clear(Color.Black);

            // The draw event is forwarded to all active screens.
            for(int ii = 0; ii < m_activeGameScreens.Count; ii++)
            {   // Draw them from zero on up, so that they overlap each other in that order.
                m_activeGameScreens[ii].Draw(gameTime, GraphicsDevice, GraphicsDeviceManager, 
                    (ii == m_activeGameScreens.Count-1)  // Is this is the top active screen?
                    );
            }

            base.Draw(gameTime);
        }



        /// Clears the current active screen and all sub-screens and starts fresh
        /// with a new active screen.  
        public void changeToNewBaseActiveScreen(game_screen_key newActiveScreen)
        {
            // Uninitialize all current screens in reverse order (top one first).
            for(int ii = m_activeGameScreens.Count -1; ii >= 0; ii--)
            {
                m_activeGameScreens[ii].UninitScreen();
            }
            // Clear the activeGameScreens list.
            m_activeGameScreens.Clear();

            // Initialize the new base screen and add it to the newly cleaned active list.  
            m_allGameScreens[newActiveScreen].InitScreen(ScreenRectangle.WholeScreen, this.GraphicsDevice);
            m_activeGameScreens.Add(m_allGameScreens[newActiveScreen]);
        }

        /// Removes the current top active screen and replaces it with a new one by key. 
        public void changeTopActiveScreen(game_screen_key newActiveScreen, ScreenRectangle portionOfScreen)
        {
            // Uninit the current top active screen.  
            topActiveScreen().UninitScreen();
            // Init the screen that's about to become the new top active screen.
            m_allGameScreens[newActiveScreen].InitScreen(portionOfScreen, this.GraphicsDevice);
            // Replace the top active screen with the new one.  
            m_activeGameScreens[m_activeGameScreens.Count - 1] = m_allGameScreens[newActiveScreen];
        }

        /// Initializes a new screen and adds it to the top of the activeGameScreen stack.   
        public void spawnNewSubScreen(game_screen_key newActiveScreen, ScreenRectangle portionOfScreen)
        {
            // Init the screen that's about to become the new top active screen.
            m_allGameScreens[newActiveScreen].InitScreen(portionOfScreen, this.GraphicsDevice);
            // Add it as a new sub-screen on top of the current one.  
            m_activeGameScreens.Add(m_allGameScreens[newActiveScreen]);
        }

        /// Unititializes the current top screen and returns focus to its parent.  
        /// Throws an error if you try to clear the last screen this way.
        public void clearCurrentScreenAndReturnToParent()
        {
            /// Make sure 
            if (topScreensParent() != null)
            {
                topActiveScreen().UninitScreen();
                m_activeGameScreens.RemoveAt(m_activeGameScreens.Count - 1);
            }
            else
            {
                // Throw an error
                throw new Exception("Can't clearCurrentScreenAndReturnToParent - already at base screen!");
            }

            
        }



        /// Changes the resolution of the main application.
        /// Currently this means re-initializing *everything*.  
        /// No doing this in the middle of a game unless you save to a file and re-load.
        public void changeResolution(int Width, int Height, bool isFullScreen)
        {
            GraphicsDeviceManager.PreferredBackBufferWidth = Width;
            GraphicsDeviceManager.PreferredBackBufferHeight = Height;
            GraphicsDeviceManager.IsFullScreen = isFullScreen;
            GraphicsDeviceManager.ApplyChanges();

            // Reinitialize everything! No state will survive this change.
            foreach (KeyValuePair<game_screen_key, GameScreen> screen in m_allGameScreens)
            {
                screen.Value.LoadContent(Content, GraphicsDevice);
            }
            foreach (GameScreen screen in m_activeGameScreens)
            {
                screen.UninitScreen();
                screen.InitScreen(GraphicsDevice);
            }

        }

    }
}
