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
    /// The game screen class.  Each one of these represents a screen or pop-up screen in the game.  
    /// The MainApplication manages a bunch of these and switches between them and pops them up and takes them down, etc.
    /// </summary>
    abstract public class GameScreen
    {
        //------------------------------------
        // Some useful constant objects for use in the various gamescreens. 

        protected static readonly TimeSpan DEFAULT_BUTTON_DELAY = new TimeSpan(0, 0, 0, 0, 400);  // 400ms



        //------------------------------------



        /// The portion of the screen that this screen is currently supposed to be displaying in. 
        protected ScreenRectangle m_currentScreenRectangle;
        public ScreenRectangle getScreenPortion()
        { return m_currentScreenRectangle; }
        
        /// Fixed positions on the screen, used for various things.
        protected Dictionary<string, Vector2> m_fixedPositions = new Dictionary<string, Vector2>();

        /// Fixed rectangles on the screen, used for various things.
        protected Dictionary<string, Rectangle> m_fixedRectangles = new Dictionary<string, Rectangle>();
        
        /// Sound effects and music.
        protected Song m_activeMusic;
        protected float m_musicVolumeMultiplier = 1.0f; // The reletive volume of music played when this screen is the base screen.
        public float MusicVolumeMultiplier {get { return m_musicVolumeMultiplier;}}

        /// A special spritebatch for drawing the screen's background.
        protected SpriteBatch m_backgroundSpriteBatch;
        protected Texture2D m_backgroundTexture;
        protected Color m_backgroundColor = Color.White;

        /// You'll want at least one main spritebatch/transfor/clickableList in each screen, 
        protected SpriteBatchEx m_spriteBatchExMain;
        
        /// You'll probably want a default SpriteFont
        protected SpriteFont m_defaultFont;

        /// Every screen will want a sprite for the mouse.
        public Cursor ActiveMouseCursor;
        /// The mouse texture has it's own batch because we never want to transform it.
        protected SpriteBatch m_spriteBatchMouse;
        /// If the mouse is changed to some other texture, this one is used to change it back.
        protected Cursor m_defaultMouseCursor;
        public void MouseRevertToDefault() { ActiveMouseCursor = m_defaultMouseCursor; }

        /// The EventManager - this lets us set up functions to be executed at certain times. 
        protected EventManager m_eventMgr = new EventManager(); 


        /// Every Screen will have some interaction with the mouse & keyboard.  
        protected bool m_keysAndMouseEnabled = true;
        public void EnableKeysAndMouse() { m_keysAndMouseEnabled = true; }
        public void DisableKeysAndMouse() { m_keysAndMouseEnabled = false; }
        protected MouseState m_lastMouseState;
        protected KeyboardState m_lastKeyboardState;

        protected bool KeyJustPressed(KeyboardState state, Keys key)
        {
            return m_keysAndMouseEnabled &&
                   state.IsKeyDown(Keys.Escape) &&
                   m_lastKeyboardState != null &&
                   !m_lastKeyboardState.IsKeyDown(Keys.Escape);
        }

        /// A weak pointer back to the MainApplication; needed for a few different things.   
        private WeakReference m_wpMainApp;

        /// Constructor - requires only a pointer to the main app.
        public GameScreen(MainApplication mainApp)
        {
            m_wpMainApp = new WeakReference(mainApp);
            m_spriteBatchExMain = new SpriteBatchEx(mainApp.GraphicsDevice);
            m_backgroundSpriteBatch = new SpriteBatch(mainApp.GraphicsDevice);
            m_spriteBatchMouse = new SpriteBatch(mainApp.GraphicsDevice);
        }

        /// Get a pointer to the main application from the weak refferance. Invarient (not virtual).
        public MainApplication GetMainApp()
        {
            if (m_wpMainApp.IsAlive)
                return (m_wpMainApp.Target as MainApplication);
            else
                return null;
        }



        public virtual void InitScreen(GraphicsDevice graphicsDevice)
        {
            if (m_currentScreenRectangle != null)
                InitScreen(m_currentScreenRectangle, graphicsDevice);
            else
                InitScreen(ScreenRectangle.WholeScreen, graphicsDevice);
        }

        /// <summary>
        /// This function is called each time the screen is "called up".
        /// </summary>
        /// <param name="portionOfScreen"> Vector2 with two floats between 0 and 1 How much of the screen </param>
        abstract public void InitScreen(ScreenRectangle portionOfScreen, GraphicsDevice graphicsDevice);

        /// This function is called whenever the screen closes or transitions to a differnet screen.
        abstract public void UninitScreen();

        /// Called only once at applecation startup to Load the content needed for this screen.
        abstract public void LoadContent(ContentManager Content, GraphicsDevice graphicsDevice);
        //{ //Examples:
            //m_spriteBatch = new SpriteBatch(graphicsDevice);
            //m_fixedPositions["name"] = new Vector2(0,0);
            //TextureStore.Store.Preload(TexId.asdf);
        //}

        /// Called from MainApplication::Update whenever this screen is active.  
        virtual public void Update(GameTime gameTime, MouseState mouseState, KeyboardState keyboardState)
        {

            if (m_keysAndMouseEnabled)
            {
                if (m_spriteBatchExMain.DrawnObjects != null)
                {
                    try
                    {
                        // We only need to notify the ClickableSprites, not every object in DrawnObjects.
                        foreach (IDrawnObject obj in m_spriteBatchExMain.DrawnObjects)
                        {
                            IUpdatedObject updated = obj as IUpdatedObject;
                            if (updated != null)
                                updated.Update(mouseState, m_spriteBatchExMain.Transform, gameTime);
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        // This exception happens if the list of clickables is modified by an action taken by a clickable.
                        // in this case we immediately abort the current loop and don't worry about it since 
                        // it'll all be fine by the next Update iteration.  
                    }
                }
            }
            // Update the EventManager - this takes care of executing any events that have been submitted at the right time.
            m_eventMgr.Update(gameTime, this);

            m_lastMouseState = mouseState;
            m_lastKeyboardState = keyboardState;
        }


        /// Draws the background image filling the desegnated m_currentScreenRectangle.
        virtual public void DrawBackgroundFirst(GraphicsDevice graphicsDevice)
        {
            //Draw the background texture coving the whole screen with the  
            if (m_backgroundTexture == null)
                throw new Exception("m_backgroundTexture missing! Did you set it in LoadContent?");
            if (m_currentScreenRectangle == null)
                throw new Exception("m_currentScreenRectangle missing! Did you set it to portionOfScreen in InitContent?");


            m_backgroundSpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend,
                null, null, null, null,
                m_currentScreenRectangle.GetMatrixTransform(graphicsDevice));
            m_backgroundSpriteBatch.Draw(m_backgroundTexture,
                new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height),
                null, m_backgroundColor, 0f, Vector2.Zero, SpriteEffects.None,
                1f // This last 1f means background, but only within the batch.
                );
            m_backgroundSpriteBatch.End();


        }

        /// Draws the mouse cursor in its own batch, after all other batches are done. 
        /// No transforms should ever be used on this batch. 
        virtual public void DrawMouseCursorLast(GameTime time)
        {
            if (ActiveMouseCursor == null)
                throw new Exception("ActiveMouseCursor missing! Did you forget to set it in LoadContent?");

            m_spriteBatchMouse.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

            ActiveMouseCursor.Draw(m_spriteBatchMouse, time, new Vector2(m_lastMouseState.X, m_lastMouseState.Y));
            
            m_spriteBatchMouse.End();
        }


        /// Called from MainApplication::Draw whenever this screen is active.  
        /// This base version only draws the background batch with the default 
        virtual public void Draw(GameTime gameTime,
            GraphicsDevice graphicsDevice,
            GraphicsDeviceManager graphicsDeviceManager,
            bool isTopActiveScreen)
        {
            DrawBackgroundFirst(graphicsDevice);

            m_spriteBatchExMain.Draw(gameTime);

            if (isTopActiveScreen)
                DrawMouseCursorLast(gameTime);
        }

        // Adds and event to the event manager.
        public void AddEvent(Event newEvent)
        {
            m_eventMgr.AddEvent(newEvent);
        }


    }
}
