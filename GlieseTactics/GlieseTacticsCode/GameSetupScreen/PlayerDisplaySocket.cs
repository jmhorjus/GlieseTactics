using System; using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Gliese581g
{

    public class PlayerTrash : ClickableSprite
    {
        Texture2D m_dim;
        Texture2D m_lit;
        public PlayerTrash(Rectangle DisplayRect) : 
            base(TextureStore.Get(TexId.player_trash_dim), DisplayRect, Color.White, 1f, 0f, Vector2.Zero, .5f) 
        {
            m_dim = TextureStore.Get(TexId.player_trash_dim);
            m_lit = TextureStore.Get(TexId.player_trash_lit);
        }

        public override void OnStartMouseover()
        {
            PlayerDisplaySocket.s_dragTrash = true;
            Texture = m_lit;
        }
        
        public override void OnStopMouseover(TimeSpan timeHeld)
        {
            PlayerDisplaySocket.s_dragTrash = false;
            Texture = m_dim;
        }
    }

    public class PlayerDisplaySocket : ClickableSprite
    {
        Player m_player;
        Texture2D m_emptyPortrait;
        SpriteFont m_font;
        Vector2 m_fontScale;

        public Player Player
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



        public PlayerDisplaySocket(Texture2D emptyPortrait, Rectangle dispRect, SpriteFont font)
            : base(emptyPortrait, dispRect, Color.White, 1f, 0f, Vector2.Zero, 1f)
        {
            m_emptyPortrait = emptyPortrait;
            m_font = font;
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
        private static PlayerDisplaySocket s_dragSourceSocket = null;
        private static PlayerDisplaySocket s_dragDestSocket = null;

        public static bool s_dragTrash = false;


        /// Mouse functions
        public override void OnLeftClick(Vector2 mousePosInTexture)
        {
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
            if (s_dragSourceSocket != null && s_dragSourceSocket != this)
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
                if (npScreen.ConfirmationDialog(string.Format(@"Are you sure you want to delete {0}?", Player.Name)))
                {
                    Player.DeleteProfile();
                    Player = null;
                }
            }
            
            if (s_dragSourceSocket == this && s_dragDestSocket != null)
            {
                Player temp = s_dragDestSocket.Player;
                Rectangle tempDestRect = s_dragDestSocket.DisplayRect;
                Rectangle tempSourceRect = s_dragSourceSocket.DisplayRect;

                s_dragDestSocket.Player = s_dragSourceSocket.Player;
                s_dragDestSocket.Tint = Color.White;
                s_dragSourceSocket.Player = temp;

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
       
    }
}
