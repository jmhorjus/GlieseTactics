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
    class IntroScreen : GameScreen
    {
        Texture2D m_teamLogoTexturePlain;
        Texture2D m_teamLogoTextureSwish;
        Rectangle m_teamLogoRectangle;
        IntroPhase m_currentPhase = IntroPhase.BlackScreen;
        float m_phaseTimer = 0f;

        public IntroScreen(MainApplication mainApp)
            : base(mainApp)
        {

        }
  
        public override void InitScreen(Gliese581g.ScreenRectangle portionOfScreen, Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice)
        {
            m_currentScreenRectangle = portionOfScreen;
            m_spriteBatchExMain.Transform = m_currentScreenRectangle.GetMatrixTransform(graphicsDevice);

            m_currentPhase = IntroPhase.BlackScreen;
            m_phaseTimer = 0f;
        }

        public override void UninitScreen()
        {
            
        }

        public override void LoadContent(ContentManager Content, GraphicsDevice graphicsDevice)
        {
            m_backgroundTexture = TextureStore.Get(TexId.default_submenu_frame);
            m_backgroundColor = Color.Black;

            m_teamLogoTextureSwish = TextureStore.Get(TexId.team_logo_swish);
            m_teamLogoTexturePlain = TextureStore.Get(TexId.team_logo);

            m_teamLogoRectangle = new Rectangle(
                (int)(graphicsDevice.Viewport.Width * .1f), (int)(graphicsDevice.Viewport.Height * .1f),
                (int)(graphicsDevice.Viewport.Width * .8f), (int)(graphicsDevice.Viewport.Height * .8f));
        }



        enum IntroPhase
        {
            BlackScreen = 0,
            FadeIn = 1,
            CrossFade = 2,
            Hold = 3,
            FadeOut = 4,
            LoadMenu = 5,
            NumPhases = 6
        }

        static readonly float[] phaseTimings = new float[(int)IntroPhase.NumPhases] 
        {  
            1f, 2f, 3f, 4f, 3f, 20f
        };


        public override void Update(GameTime gameTime, MouseState mouseState, KeyboardState keyboardState)
        {
            if (keyboardState.IsKeyDown(Keys.Enter) || keyboardState.IsKeyDown(Keys.Escape))
                m_currentPhase = IntroPhase.LoadMenu;
        }

        public override void Draw(GameTime gameTime, GraphicsDevice graphicsDevice, GraphicsDeviceManager graphicsDeviceManager, bool isTopActiveScreen)
        {
            base.DrawBackgroundFirst(graphicsDevice); // black background


            float newTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float alphaPlain = 0f;
            float alphaSwish = 0f;

            switch (m_currentPhase)
            {
                case IntroPhase.BlackScreen:
                    alphaSwish = 0f;
                    break;
                case IntroPhase.FadeIn:
                    alphaSwish = m_phaseTimer / phaseTimings[(int)m_currentPhase];
                    break;
                case IntroPhase.CrossFade:
                    alphaSwish = 1f;
                    alphaPlain = m_phaseTimer / phaseTimings[(int)m_currentPhase];
                    break;
                case IntroPhase.Hold:
                    alphaPlain = 1f;

                    //Preload All the textures.
                    TextureStore.PreloadAll();
                    
                    break;
                case IntroPhase.FadeOut:
                    alphaPlain = 1f - (m_phaseTimer / phaseTimings[(int)m_currentPhase]);
                    break;
                case IntroPhase.LoadMenu:
                    GetMainApp().changeToNewBaseActiveScreen(game_screen_key.MainMenuScreen);
                    break;
            }

            if (m_phaseTimer > phaseTimings[(int)m_currentPhase])
            {
                m_currentPhase++;
                m_phaseTimer = 0f;

                if (m_currentPhase == IntroPhase.FadeIn)
                    SfxStore.Get(SfxId.mech_fire).Play();
            }


            //Draw manually.
            m_spriteBatchExMain.Batch.Begin();
            m_spriteBatchExMain.Batch.Draw(m_teamLogoTextureSwish, m_teamLogoRectangle, Color.White * alphaSwish);
            m_spriteBatchExMain.Batch.Draw(m_teamLogoTexturePlain, m_teamLogoRectangle, Color.White * alphaPlain);
            m_spriteBatchExMain.Batch.End();

            m_phaseTimer += newTime;
        }


    }
}
