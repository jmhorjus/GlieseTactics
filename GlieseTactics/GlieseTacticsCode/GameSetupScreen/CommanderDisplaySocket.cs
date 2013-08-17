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

    public class CommanderDisplaySocket : ClickableSprite
    {
        Commander m_player;
        Texture2D m_emptyPortrait;
        SpriteFont m_font;
        Vector2 m_fontScale;



        /// <summary>
        ///  Members related to the skills frame
        /// </summary>
        MenuButtonPannel m_skillsButtonPanel;
        public bool ShowingSkills
        {
            get { return m_skillsButtonPanel.Visible; }
            set
            {
                m_skillsButtonPanel.Visible = value;
                m_skillsButtonPanel.Enabled = value;
            }
        }

        protected void UpdateSkillsPanelPosition()
        {
            if (m_skillsButtonPanel == null)
                return;
            Rectangle dispRect = DisplayRect;
            m_skillsButtonPanel.DisplayRect = new Rectangle(
                dispRect.X + dispRect.Width, dispRect.Y,
                (int)(dispRect.Width * 1.5f), dispRect.Height);
        }

        public override Rectangle DisplayRect
        {   // We override the DisplayRect set function to also call UpdateSkillsPanelPosition().
            set
            {
                base.DisplayRect = value;
                UpdateSkillsPanelPosition();
            }
        }


        GameScreen ParentScreen
        {  // Can set the skill panel's parent screen through the display socket.
            get { return m_skillsButtonPanel.ParentScreen; }
            set { m_skillsButtonPanel.ParentScreen = value; }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public Commander Commander
        {
            get { return m_player; }
            set 
            { 
                m_player = value;
                if (value == null)
                {
                    Texture = m_emptyPortrait;
                    DisableClickDrag();
                }
                else
                {
                    Texture = m_player.Portrait;
                    EnableClickDrag(0,Rectangle.Empty, .5f, .5f);
                    updateFontScale();
                }
            }
        }


        public CommanderDisplaySocket(Texture2D emptyPortrait, Rectangle dispRect, SpriteFont font, GameScreen parentScreen)
            : base(emptyPortrait, dispRect, Color.White, 1f, 0f, Vector2.Zero, 1f)
        {
            m_emptyPortrait = emptyPortrait;
            m_font = font;
            
            m_skillsButtonPanel = new MenuButtonPannel(
                TextureStore.Get(TexId.player_stats_frame_skill_panel),
                TextureStore.Get(TexId.skill_empty_socket),
                Rectangle.Empty, parentScreen);

            UpdateSkillsPanelPosition();
            m_skillsButtonPanel.Visible = false;
        }


        /// Special function for setting an event which will be triggered 
        /// on a left click if the socket is empty.  
        Event m_clickWhenEmptyEvent = null;
        GameScreen m_parentScreen = null;
        public void SetClickWhenEmptyEvent(Event clickWhenEmptyEvent, GameScreen parentScreen)
        {
            m_clickWhenEmptyEvent = clickWhenEmptyEvent;
            m_parentScreen = parentScreen;
        }



        /// Static socket pointers to facilitate dragging and dropping between sockets.
        private static CommanderDisplaySocket s_dragSourceSocket = null;
        private static CommanderDisplaySocket s_dragDestSocket = null;

        public static bool s_dragTrash = false;


        /// Mouse functions
        public override void OnLeftClick(Vector2 mousePosInTexture)
        {
            // If empty, add the defined click-when-empty event to the parent screen.
            if (m_player == null && m_clickWhenEmptyEvent != null && m_parentScreen != null)
            {
                m_clickWhenEmptyEvent.Reset();
                m_parentScreen.AddEvent(m_clickWhenEmptyEvent);
            }
        }

        
        public override void OnLeftClickDragStart()
        {
            if (s_dragSourceSocket == null)
                s_dragSourceSocket = this;
        }

        public override void OnStartMouseover()
        {
            if (s_dragSourceSocket != null && s_dragSourceSocket != this && s_dragDestSocket == null)
            {
                s_dragDestSocket = this;
                Tint = Color.LightGray;
            }
        }

        public override void OnStopMouseover(TimeSpan timeHeld)
        {
            if (s_dragDestSocket == this)
            {
                s_dragDestSocket = null;
                Tint = Color.White;
            }
        }

        public override void OnLeftClickDragDrop(TimeSpan timeHeld, Vector2 endPos, Vector2 mousePosInTexture)
        {
            if (s_dragSourceSocket == this && s_dragTrash == true)
            {
                NewPlayerScreen npScreen = NewPlayerScreen.GetInstance;
                if (npScreen.ConfirmationDialog(string.Format(@"Are you sure you want to delete {0}?", Commander.Name)))
                {
                    Commander.DeleteProfile();
                    Commander = null;
                }
            }
            
            if (s_dragSourceSocket == this && s_dragDestSocket != null)
            {
                Commander temp = s_dragDestSocket.Commander;
                Rectangle tempDestRect = s_dragDestSocket.DisplayRect;
                Rectangle tempSourceRect = s_dragSourceSocket.DisplayRect;

                s_dragDestSocket.Commander = s_dragSourceSocket.Commander;
                s_dragDestSocket.Tint = Color.White;
                s_dragSourceSocket.Commander = temp;

                //Not only swap players but also locations - then animate back to their original places with the new players.
                s_dragDestSocket.DisplayRect = tempSourceRect;
                s_dragSourceSocket.DisplayRect = tempDestRect;
                s_dragDestSocket.AddAnimation(new Animation(new TimeSpan(0,0,0,0,400), tempDestRect));
                s_dragSourceSocket.AddAnimation(new Animation(new TimeSpan(0,0,0,0,400), tempSourceRect));
            }

            s_dragSourceSocket = null;
            s_dragDestSocket = null;
        }



        /// Amount by which the player frame image is larger than the portrait it frames.
        static readonly Vector2 FRAME_INFLATE_RATIO = new Vector2(0.60f, 0.33f);
        /// Rectangle in the player frame image to display the name in.
        static readonly Vector2 NAME_DISPLAY_POS = new Vector2(-14f/181f, 133f/201f);//measured from the origin of the portraite
        static readonly Vector2 NAME_DISPLAY_SIZE = new Vector2(129f/181f, 23f/201f);

        /// The draw function. 
        public override void Draw(SpriteBatch spriteBatch, GameTime time)
        {
            //The skill button panel is on the botom
            m_skillsButtonPanel.Draw(spriteBatch, time);


            base.Draw(spriteBatch, time);
            updateFontScale();

            Rectangle frameRectangle = DisplayRect;
            frameRectangle.Inflate( 
                (int)(frameRectangle.Width  * FRAME_INFLATE_RATIO.X), 
                (int)(frameRectangle.Height * FRAME_INFLATE_RATIO.Y));
            Color frameColor = m_player != null ? m_player.UnitColor : Color.White;

            spriteBatch.Draw(
                TextureStore.Get(TexId.player_stats_frame), 
                frameRectangle, 
                null, frameColor, 0f, Vector2.Zero, SpriteEffects.None, .5f);

            if (m_player != null)
            {
                Vector2 nameSize = m_font.MeasureString(m_player.Name) * m_fontScale;
                Vector2 namePos = Position + new Vector2(
                    (NAME_DISPLAY_POS.X + NAME_DISPLAY_SIZE.X/2f) * frameRectangle.Width - nameSize.X/2f, 
                    NAME_DISPLAY_POS.Y * frameRectangle.Height);

                spriteBatch.DrawString(
                    m_font, 
                    m_player.Name,
                    namePos,
                    Color.Black, 0f, Vector2.Zero, 
                    m_fontScale, 
                    SpriteEffects.None, 0f);
            }

        }


        /// Utility function to correctly size the player's name within the frame.
        private void updateFontScale()
        {
            // Find the fontScale for this name.
            if (m_player == null)
            {
                m_fontScale = Vector2.One;
                return;
            }

            Vector2 nameSize = m_font.MeasureString(m_player.Name);
            Rectangle frameRectangle = DisplayRect;
            frameRectangle.Inflate(
                (int)(frameRectangle.Width * FRAME_INFLATE_RATIO.X),
                (int)(frameRectangle.Height * FRAME_INFLATE_RATIO.Y));

            m_fontScale.Y = (NAME_DISPLAY_SIZE.Y * frameRectangle.Height) / nameSize.Y;            
            
            m_fontScale.X = Math.Min(
                NAME_DISPLAY_SIZE.X * frameRectangle.Width / nameSize.X,
                m_fontScale.Y /*Don't stretch it unneccessarily on x any more than you already did on y*/);
        }


        public override bool Update(MouseState mouseState, Matrix transformMatrix, GameTime time, bool mouseAlreadyIntercepted)
        {
            bool retVal = base.Update(mouseState, transformMatrix, time, mouseAlreadyIntercepted);
            
            retVal |= m_skillsButtonPanel.Update(mouseState, transformMatrix, time, mouseAlreadyIntercepted);

            return retVal;
        }
       
    }
}
