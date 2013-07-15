using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Gliese581g
{
    class SkillsScreen : GameScreen
    {


        /// The SkillsScreen constructor - just pass through. 
        public SkillsScreen(MainApplication mainApp)
            : base(mainApp)
        {
        }

        TextLabel m_InspirationLabel;
        TextLabel m_OperationsLabel;
        TextLabel m_PilotingLabel;
        TextLabel m_OrdnanceLabel;

        public override void InitScreen(ScreenRectangle portionOfScreen, GraphicsDevice graphicsDevice)
        {
            m_currentScreenRectangle = portionOfScreen;
            m_mainScreenLayer.Transform = m_currentScreenRectangle.GetMatrixTransform(graphicsDevice);
        }



        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager Content, GraphicsDevice graphicsDevice)
        {
            

        }

    }
}
