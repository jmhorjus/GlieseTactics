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

using System.Media;

namespace Gliese581g
{
    public class ClickableSprite : IDrawnObject, IUpdatedObject
    {

        /// Things having to do with drawing the sprite.
        Texture2D m_texture;
        public Texture2D Texture
        {
            get { return m_texture; }
            set
            {
                if (m_stretchToRect)
                {
                    Rectangle currentRectangle = LocationRect;
                    m_texture = value;
                    LocationRect = currentRectangle;
                }
                else
                { m_texture = value; }
            }
        }

        bool m_stretchToRect;

        // If set, the position of this sprite is an offset from the position of the anchor.
        protected ClickableSprite m_anchorSprite;

        // Eather position or offset, depending on anchorSprite.
        private Vector2 m_positionOrOffset;
        public Vector2 Position 
        { 
            get { return (m_anchorSprite == null) ? m_positionOrOffset : m_anchorSprite.Position + m_positionOrOffset; } 
            set { m_positionOrOffset = (m_anchorSprite == null) ? value : value - m_anchorSprite.Position; } 
        }
        public Vector2 AnchorOffset
        {
            get { return (m_anchorSprite == null) ? Vector2.Zero : m_positionOrOffset; }
            set { if (m_anchorSprite == null) m_positionOrOffset = value; } 
        }
        public Vector2 Scale;

        public int Width  { get { return (int)(m_texture.Width * Scale.X); } }
        public int Height { get { return (int)(m_texture.Height * Scale.Y); } }
        
        /// May be a static location or an offset from its anchor.
        public virtual Rectangle LocationRect
        {
            get 
            {
                return new Rectangle(
                    (int)(m_positionOrOffset.X - (DrawOrigin.X * Scale.X)),
                    (int)(m_positionOrOffset.Y - (DrawOrigin.Y * Scale.Y)),
                    this.Width, this.Height);
            }
            set 
            {
                Scale.X = (float)value.Width / m_texture.Width;
                Scale.Y = (float)value.Height / m_texture.Height;

                m_positionOrOffset.X = value.X + (DrawOrigin.X * Scale.X);
                m_positionOrOffset.Y = value.Y + (DrawOrigin.Y * Scale.Y);
            }
        }
        public virtual Rectangle DisplayRect
        { 
            get
            {
                Rectangle retVal = LocationRect;

                if(m_anchorSprite != null)
                   retVal.Offset(new Point((int)m_anchorSprite.Position.X, (int)m_anchorSprite.Position.Y));

                return retVal;
            }
        }

        
        public Color Tint;
        public float Alpha;
        public float RotationAngle;
        public Vector2 DrawOrigin;
        public bool RotateInPlace = true;
        public float LayerDepth;

        private bool m_enabled = true;
        /// Do we process Update()?
        public virtual bool Enabled { get { return m_enabled; } set { m_enabled = value; } }
        private bool m_visible = true;
        /// Do We process Draw()?
        public virtual bool Visible { get { return m_visible; } set { m_visible = value; } }

        /// Things having to do with detecting clicks/mouseover.
        MouseState m_lastMouseState;
        protected bool m_lastMouseover = false;
        public bool LastMouseOver { get { return m_lastMouseover; } }

        bool m_lastRightClick = false;
        Vector2 m_rightClickOffset = Vector2.Zero;
        bool m_lastLeftClick = false;
        Vector2 m_leftClickOffset = Vector2.Zero;

        TimeSpan m_mouseOverStartTime = TimeSpan.MaxValue;
        TimeSpan m_leftClickStartTime = TimeSpan.MaxValue;
        TimeSpan m_rightClickStartTime = TimeSpan.MaxValue;


        /// Things having to do with mouse drag and drop.
        protected bool m_enableClickDrag;
        protected float m_dragTriggerRadius;
        protected Rectangle m_dragLimits;
        protected float m_originalAlphaWhenDragging;
        protected float m_dragGhostAlpha;

        bool m_leftClickReadyToDrag = false;
        bool m_leftClickDragging = false;
        bool m_rightClickReadyToDrag = false;
        bool m_rightClickDragging = false;
        Point m_leftDragOrigin = Point.Zero;
        Point m_rightDragOrigin = Point.Zero;
        Vector2 m_leftDragTranformedOrigin = Vector2.Zero;
        Vector2 m_rightDragTranformedOrigin = Vector2.Zero;
        Vector2 m_leftDragVector = Vector2.Zero;
        Vector2 m_rightDragVector = Vector2.Zero;


        ///Things having to do with animation.
        ///
        List<Animation> m_queuedAnimations = new List<Animation>();
        public void AddAnimation(Animation animation)
        {
            m_queuedAnimations.Add(animation);
        }
        public bool HasAnyAnimation
        { get { return m_queuedAnimations.Count > 0; } }


        /// The constructor - everything must be passed in.  
        /// Classes inheriting ClickableSprite need not have this many constructor parameters.
        public ClickableSprite(
            Texture2D texture,
            Vector2 pos,
            Color tint,
            float alpha,
            Vector2 scale,
            float rotationAngle,
            Vector2 drawOrigin,
            float layerDepth,
            ClickableSprite anchorSprite = null)
        {
            m_texture = texture;
            Position = pos;
            Scale = scale;
            Tint = tint;
            Alpha = alpha;
            RotationAngle = rotationAngle;
            DrawOrigin = drawOrigin;
            LayerDepth = layerDepth;

            m_anchorSprite = anchorSprite;

            DisableClickDrag();
        }


        /// The second constructor - stretch the picture to a rectangle.
        public ClickableSprite(
            Texture2D texture,
            Rectangle dispRect,
            Color tint,
            float alpha,
            float rotationAngle,
            Vector2 drawOrigin,
            float layerDepth,
            ClickableSprite anchorSprite = null)
        {
            m_texture = texture;
            DrawOrigin = drawOrigin;
            m_anchorSprite = anchorSprite;
            m_stretchToRect = true;
            Tint = tint;
            Alpha = alpha;
            RotationAngle = rotationAngle;
            LayerDepth = layerDepth;

            LocationRect = dispRect;

            DisableClickDrag();
        }


        public void EnableClickDrag()
        {
            EnableClickDrag(5, Rectangle.Empty, 1f, .5f);
        }

        public void EnableClickDrag(
            float dragTriggerRadius,
            Rectangle dragLimits,
            float originalAlphaWhenDragging,
            float dragGhostAlphaWhenDragging)
        {
            m_enableClickDrag = true;
            m_dragTriggerRadius = dragTriggerRadius;
            m_dragLimits = dragLimits;
            m_dragGhostAlpha = dragGhostAlphaWhenDragging;
            m_originalAlphaWhenDragging = originalAlphaWhenDragging;
        }

        public void DisableClickDrag()
        {
            m_enableClickDrag = false;
            m_dragTriggerRadius = 5;
            m_dragLimits = Rectangle.Empty;
            m_dragGhostAlpha = .5f;
            m_originalAlphaWhenDragging = 1f;
        }


        /// Be default we just check whether the point is in the 
        /// display rectangle, but this function is made virtual
        /// so that sprites with non-rectangular shapes can override it.
        public virtual bool TestMouseOver(Vector2 transformedPoint)
        {
            return TestMouseOver(new Point((int)transformedPoint.X,(int)transformedPoint.Y));
        }
        public virtual bool TestMouseOver(Point transformedPoint)
        {
            Rectangle displayRect = DisplayRect;

            if (!displayRect.Contains(transformedPoint))
                return false;

            // Look for transparency.
            Rectangle pointRect = new Rectangle(
                (int)((transformedPoint.X - displayRect.X) / Scale.X),
                (int)((transformedPoint.Y - displayRect.Y) / Scale.Y), 
                1, 1);
            int size = 1;
            Color[] buffer = new Color[size];
            m_texture.GetData(0, pointRect, buffer, 0, size);
            return !buffer.All(c => c == Color.Transparent);
        }


        /// The draw function: 
        /// This function does animations based on the GameTime, and also 
        /// draws the sprite if it is currently visible.
        public virtual void Draw(SpriteBatch spriteBatch, GameTime time)
        {
            //Process animations first.  
            if (m_queuedAnimations.Count > 0)
            {  // We process only the animation at the front of the queue
                m_queuedAnimations[0].Animate(this, time);

                if (m_queuedAnimations[0].Finished)
                    m_queuedAnimations.RemoveAt(0);
            }


            if (!Visible)
                return;

            float alpha = Alpha;
            if (m_rightClickDragging || m_leftClickDragging)
            {
                alpha *= m_originalAlphaWhenDragging;
            }

            spriteBatch.Draw(
                m_texture,          // The texture 
                Position,           // The location
                null,               // Draw the whole texture (assume it is not a tile-sheet or animation sheet)
                Tint * alpha,       // The Tint and Transparency
                RotationAngle,      // The rotation to apply
                DrawOrigin,         // The origin of rotation calculated above 
                Scale,              // The scale
                SpriteEffects.None, // No effects for now
                LayerDepth);        // Layer at which to draw.


            if (m_leftClickDragging)
            {
                Vector2 drag_pos = BringVectorWithinRectangle(Position + m_leftDragVector, m_dragLimits);

                spriteBatch.Draw(
                    m_texture,          // The texture 
                    drag_pos,           // The location
                    null,               // Draw the whole texture (assume it is not a tile-sheet or animation sheet)
                    Tint * m_dragGhostAlpha,// The Tint and Transparency
                    RotationAngle,      // The rotation to apply
                    DrawOrigin,         // The origin of rotation calculated above 
                    Scale,              // The scale
                    SpriteEffects.None, // No effects for now
                    LayerDepth);        // Layer at which to draw.
            }

            if (m_rightClickDragging)
            {
                Vector2 drag_pos = BringVectorWithinRectangle(Position + m_rightDragVector, m_dragLimits);

                spriteBatch.Draw(
                    m_texture,            // The texture 
                    drag_pos,           // The location
                    null,               // Draw the whole texture (assume it is not a tile-sheet or animation sheet)
                    Tint * m_dragGhostAlpha,// The Tint and Transparency
                    RotationAngle,      // The rotation to apply
                    DrawOrigin,     // The origin of rotation calculated above 
                    Scale,              // The scale
                    SpriteEffects.None, // No effects for now
                    LayerDepth);        // Layer at which to draw.
            }

        }



        /// Determines all the mouse states, using the transformMatrix to transform the 
        /// mouse co-ordinates before testing for mouse-over.
        public virtual bool Update(MouseState mouseState, Matrix transformMatrix, GameTime time, bool mouseAlreadyIntercepted)
        {
            // Transform the mouse co-ordinates with the same transform being used on the sprite.
            Vector2 pos = new Vector2((float)mouseState.X, (float)mouseState.Y);
            Vector2 transformedPos = Vector2.Transform(pos, Matrix.Invert(transformMatrix) );

            if (Enabled && !mouseAlreadyIntercepted)
                Update(mouseState, transformedPos, time);

            return Visible && TestMouseOver(transformedPos); // Don't allow invisible objects to intercept the mouse. 
        }

        /// Use this version of update if you already know the transformed mouse position.  
        public bool Update(MouseState mouseState, Vector2 transformedPos, GameTime time)
        {
            Point transformedPoint = new Point((int)transformedPos.X, (int)transformedPos.Y);

            // Check mouseover using transformedPoint
            bool isMouseOverNow = TestMouseOver(transformedPoint);

            bool leftClickNow = (mouseState.LeftButton == ButtonState.Pressed);
            bool rightClickNow = (mouseState.RightButton == ButtonState.Pressed);


            // *** Check mouseover functions ***
            if (!m_lastMouseover && isMouseOverNow)
            {   //Starting a mouseover
                OnStartMouseover();
                m_lastMouseover = true;
                m_mouseOverStartTime = time.TotalGameTime;
            }
            else if (m_lastMouseover && isMouseOverNow)
            {   //Continuing a mouseover
                OnContinueMouseover(time.TotalGameTime - m_mouseOverStartTime);
            }
            else if (m_lastMouseover && !isMouseOverNow)
            {   //Ending a mouseover
                OnStopMouseover(time.TotalGameTime - m_mouseOverStartTime);
                m_lastMouseover = false;
                m_mouseOverStartTime = TimeSpan.MaxValue;
            }



            // **** Check Left clicks ****
            if (!m_lastLeftClick && (m_lastMouseState.LeftButton == ButtonState.Released)
                && leftClickNow && isMouseOverNow )
            {   //From un-pressed to pressed: set up.
                //Setup left click
                m_lastLeftClick = true;
                //m_leftClickOffset = transformedPos - Position;
                m_leftClickStartTime = time.TotalGameTime;
                //Setup left click drag.
                m_leftClickReadyToDrag = true;
                m_leftDragOrigin = transformedPoint;

                OnLeftClick(transformedPos - Position);
            }
            else if (m_lastLeftClick && leftClickNow && isMouseOverNow)
            {   //From pressed to still-pressed: continue.
                OnLeftClickHold(time.TotalGameTime - m_leftClickStartTime);
            }
            else if (m_lastLeftClick && (!leftClickNow || !isMouseOverNow))
            {   // From pressed to un-pressed.  Clean up the three things set above!
                OnLeftClickRelease(time.TotalGameTime - m_leftClickStartTime);

                //Cleanup left click
                m_lastLeftClick = false;
               // m_leftClickOffset = Vector2.Zero;
                m_leftClickStartTime = TimeSpan.MaxValue;
                // Only disable drag if the button was released.
                if(!leftClickNow)
                    m_leftClickReadyToDrag = false;
            }


            // **** Check Right clicks ****
            if (!m_lastRightClick && (m_lastMouseState.RightButton == ButtonState.Released)
                && rightClickNow && isMouseOverNow)
            {
                // Setup right click
                m_lastRightClick = true;
                //m_rightClickOffset = transformedPos - Position;
                m_rightClickStartTime = time.TotalGameTime;
                // Setup right click drag
                m_rightClickReadyToDrag = true;
                m_rightDragOrigin = transformedPoint;

                OnRightClick(transformedPos - Position);
            }
            else if (m_lastRightClick && rightClickNow && isMouseOverNow)
            {
                OnRightClickHold(time.TotalGameTime - m_rightClickStartTime);
            }
            else if (m_lastRightClick && (!rightClickNow || !isMouseOverNow))
            {
                OnRightClickRelease(time.TotalGameTime - m_rightClickStartTime);

                //Cleanup right click
                m_lastRightClick = false;
                //m_rightClickOffset = Vector2.Zero;
                m_rightClickStartTime = TimeSpan.MaxValue;
                // Only disable drag if the button was released.
                if(!rightClickNow)
                    m_rightClickReadyToDrag = false;
            }


            if (m_enableClickDrag)
            {
                // *** Left Click Drag Checks ***
                if (!m_leftClickDragging && m_leftClickReadyToDrag
                    && Dist(m_leftDragOrigin, transformedPoint) >= m_dragTriggerRadius)
                {   //Should we be initiating a drag?
                    OnLeftClickDragStart();

                    //Setup
                    m_leftClickDragging = true;
                }
                else if (m_leftClickDragging && leftClickNow)
                {   // Are we in the middle of a drag?
                    Vector2 dragOrigin = new Vector2(m_leftDragOrigin.X, m_leftDragOrigin.Y);

                    OnLeftClickDragContinue(
                        time.TotalGameTime - m_leftClickStartTime,
                        BringVectorWithinRectangle(Position + transformedPos - dragOrigin, m_dragLimits),
                        dragOrigin - Position);

                    m_leftDragVector = transformedPos - dragOrigin;
                }
                else if (m_leftClickDragging && !leftClickNow)
                {   // Are we dropping a drag?
                    Vector2 dragOrigin = new Vector2(m_leftDragOrigin.X, m_leftDragOrigin.Y);

                    OnLeftClickDragDrop(
                        time.TotalGameTime - m_leftClickStartTime,
                        BringVectorWithinRectangle(Position + transformedPos - dragOrigin, m_dragLimits),
                        dragOrigin - Position);

                    //Cleanup
                    m_leftClickDragging = false;
                    m_leftClickReadyToDrag = false;
                    m_leftDragOrigin = Point.Zero;
                    m_leftDragVector = Vector2.Zero;
                }



                // *** Right Click Drag Checks ***
                if (!m_rightClickDragging && m_rightClickReadyToDrag
                    && Dist(m_rightDragOrigin, transformedPoint) >= m_dragTriggerRadius)
                {   //Should we be initiating a drag?
                    OnRightClickDragStart();

                    //Setup
                    m_rightClickDragging = true;
                }
                else if (m_rightClickDragging && rightClickNow)
                {   // Are we in the middle of a drag?
                    Vector2 dragOrigin = new Vector2(m_rightDragOrigin.X, m_rightDragOrigin.Y);

                    OnRightClickDragContinue(
                        time.TotalGameTime - m_rightClickStartTime,
                        BringVectorWithinRectangle(Position + transformedPos - dragOrigin, m_dragLimits),
                        dragOrigin - Position);
                
                    m_rightDragVector = transformedPos - dragOrigin;
                }
                else if (m_rightClickDragging && !rightClickNow)
                {   // Are we dropping a drag?
                    Vector2 dragOrigin = new Vector2(m_rightDragOrigin.X, m_rightDragOrigin.Y);

                    OnRightClickDragDrop(
                        time.TotalGameTime - m_rightClickStartTime, 
                        BringVectorWithinRectangle(Position + transformedPos - dragOrigin, m_dragLimits),
                        dragOrigin - Position);

                    //Cleanup
                    m_rightClickDragging = false;
                    m_rightClickReadyToDrag = false;
                    m_rightDragOrigin = Point.Zero;
                    m_rightDragVector = Vector2.Zero;
                }
            }


            // Last of all, set the last mouse state to the current one.  
            m_lastMouseState = mouseState;

            return isMouseOverNow;
        }


        /// All the virtual functions that people will override for their own purposes.
        virtual public void OnRightClick(Vector2 mousePosInTexture) { }
        virtual public void OnRightClickHold(TimeSpan timeHeld) { }
        virtual public void OnRightClickRelease(TimeSpan timeHeld) { }

        virtual public void OnRightClickDragStart() { }
        virtual public void OnRightClickDragContinue(TimeSpan timeHeld, Vector2 endPos, Vector2 mousePosInTexture) { }
        virtual public void OnRightClickDragDrop(TimeSpan timeHeld, Vector2 endPos, Vector2 mousePosInTexture) { }

        virtual public void OnLeftClick(Vector2 mousePosInTexture) { }
        virtual public void OnLeftClickHold(TimeSpan timeHeld) { }
        virtual public void OnLeftClickRelease(TimeSpan timeHeld) { }
        
        virtual public void OnLeftClickDragStart() { }
        virtual public void OnLeftClickDragContinue(TimeSpan timeHeld, Vector2 endPos, Vector2 mousePosInTexture) { }
        virtual public void OnLeftClickDragDrop(TimeSpan timeHeld, Vector2 endPos, Vector2 mousePosInTexture) { }

        virtual public void OnStartMouseover() { }
        virtual public void OnContinueMouseover(TimeSpan timeHeld) { }
        virtual public void OnStopMouseover(TimeSpan timeHeld) { }






        ///Utility Functions 

        ///A quick utility function that returns the distance between two points.
        /// Used in NotifyMouseSate.
        private float Dist(Point A, Point B)
        {
            return (float)Math.Sqrt((A.X - B.X) ^ 2 + (A.Y - B.Y) ^ 2);
        }

        private Vector2 BringVectorWithinRectangle(Vector2 vector, Rectangle rect)
        {
            if (rect == Rectangle.Empty)
                return vector;

            // Gotta check each side of the rectangle individually.
            if (vector.X < rect.Left)
                vector.X = rect.Left;
            if (vector.X > rect.Right)
                vector.X = rect.Right;
            if (vector.Y < rect.Top)
                vector.Y = rect.Top;
            if (vector.Y > rect.Bottom)
                vector.Y = rect.Bottom;

            return vector;
        }


    }
}
