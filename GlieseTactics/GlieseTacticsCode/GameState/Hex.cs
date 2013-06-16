using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input; // add for test
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Gliese581g
{
    public class Hex : ClickableSprite 
    {

        public const int HEX_SIZE = 80;
        public const int HEX_SPACING = 2;
        
        public static Color SELECTED_TINT = new Color(50, 50, 50, 255);
        public static Color MOUSEOVER_TINT = new Color(100, 100, 100, 255);

        public static Color HIGHLIGHT_TINT_MOVE = new Color(50, 150, 150, 255);
        public static Color HIGHLIGHT_TINT_ATTACK = new Color(150, 150, 50, 255);

        public static Color DOUBLE_HIGHLIGHT_TINT = new Color(100, 50, 50, 255);

        // Pointer to the map that this hex is a part of.
        Map m_map;

        // Hex properties related to current map input. 
        public bool IsSelected = false; 
        public bool IsHighlighted = false;
        public bool IsDoubleHighlighted = false;

        public bool IsMarked = false; // used for the range template, to indicate if an effect has already been applied.
        public int CurrentMoveCost; // used in range calculations.
        public Hex TemplateOriginHex = null; // used for during damage template placment

        // Can units move onto-through this Hex?
        public bool LandPossible;
        
        // Is there a unit currently occupying this Hex?
        protected Unit m_unit = null;
        public Unit Unit { get { return m_unit; } }

        public void PlaceUnit(Unit unit) { m_unit = unit; }
        public void ClearUnit() { m_unit = null; }
        
        // Is a valid destination if it's possable and not already occupied.
        public bool IsValidDestination
        { get { return LandPossible && Unit == null; } }

        List<IDrawnEffect> drawnEffectList = new List<IDrawnEffect>();

        // Appearance variables.  
        protected Point m_mapPosition;
        protected Rectangle m_displayRectangle;


        public new Rectangle DisplayRect
        { get { return m_displayRectangle; } }

        public Point MapPosition
        {
            get { return m_mapPosition; }

            set
            {   // When map position changes, m_displayRectangle also needs to be updated.
                m_mapPosition = value;
                m_displayRectangle = screenRectangleFromMapCoordinates(value);
            }

        }

        // Constructor
        public Hex(Texture2D texture, Point mapPosition, bool landPossible, Map owningMap)
            : base(texture, Rectangle.Empty, Color.White, 1f, 0f, Vector2.Zero, 0f)
        {
            Texture = texture;
            MapPosition = mapPosition;
            LandPossible = landPossible;
            Tint = Color.White;
            m_map = owningMap;
        }


        // Support the draw interface (we're using the composite design pattern)
        public override void Draw(SpriteBatch spriteBatch, GameTime time)
        {
         
            // Possible tint colors checked in order of precedence. 
            if (IsSelected)
                Tint = SELECTED_TINT;
            else if (IsDoubleHighlighted)
            {   // Cool flashy effect.
                DOUBLE_HIGHLIGHT_TINT.R = (byte)(100 + 154 * Math.Abs(Math.Sin(time.TotalGameTime.TotalSeconds * 3)));
                Tint = DOUBLE_HIGHLIGHT_TINT;
            }
            else if (m_lastMouseover)
                Tint = MOUSEOVER_TINT;
            else if (IsHighlighted)
                if (m_map.Game.CurrentTurnStage == Game.TurnStage.ChooseHeading ||
                    m_map.Game.CurrentTurnStage == Game.TurnStage.ChooseAttackTarget)
                    Tint = HIGHLIGHT_TINT_ATTACK;
                else
                    Tint = HIGHLIGHT_TINT_MOVE;
            else
                Tint = Color.White;

            spriteBatch.Draw(Texture, m_displayRectangle, null, Tint, 0f, Vector2.Zero, SpriteEffects.None, 0f);
            //base.Draw(spriteBatch, time);

            if (Unit != null)
                Unit.Draw(spriteBatch, time);

            // Draw Explosion - they are removed from the list when finished
            for (int ii = 0; ii < drawnEffectList.Count; ii++)
            {
                drawnEffectList[ii].Draw(spriteBatch, time);
                if (drawnEffectList[ii].IsFinished())
                {
                    drawnEffectList.RemoveAt(ii);
                    ii--;
                }
            }

        }


        /// <summary>
        /// Get the Screen position of a hex based on its map coordinates.  
        /// </summary>
        private Rectangle screenRectangleFromMapCoordinates(Point mapCoordinates)
        {
            int x = mapCoordinates.X;
            int y = mapCoordinates.Y;
            return new Rectangle(
                HEX_SPACING + (int)(x * HEX_SIZE + (y % 2) * (HEX_SIZE / 2f)),
                HEX_SPACING + (int)(y * (HEX_SIZE * 3f / 4f)),
                HEX_SIZE - 2 * HEX_SPACING,
                HEX_SIZE - 2 * HEX_SPACING
                );
        }

        /// <summary>
        /// Override clickablesprite's TestMouseOver method.
        /// </summary>
        public override bool TestMouseOver(Point pos)
        {
            // Take advantage of the m_displayRectangle we already know.
            Vector2 posWithinHex = new Vector2(
                (float)(pos.X - m_displayRectangle.X) / m_displayRectangle.Width,
                (float)(pos.Y - m_displayRectangle.Y) / m_displayRectangle.Height);

            // Check all 6 sides of the hex.
            return (posWithinHex.X > 0f) &&                          // Left Side
                (posWithinHex.X < 1f) &&                             // Right Side
                (posWithinHex.Y < 0.75f + (posWithinHex.X / 2f)) &&  // Top Left Edge 
                (posWithinHex.Y < 1.25f - (posWithinHex.X / 2f)) &&  // Top Right Edge
                (posWithinHex.Y > 0.25f - (posWithinHex.X / 2f)) &&  // Bottom Left Edge
                (posWithinHex.Y > -.25f + (posWithinHex.X / 2f));    // Bottom Right Edge

        }



        public override void OnRightClick(Vector2 mousePosInTexture)
        {
            switch (m_map.Game.CurrentTurnStage)
            {
                case Game.TurnStage.ChooseUnit:
                    m_map.SelectedHex = null;
                    break;
                case Game.TurnStage.ChooseMoveDestination:
                    m_map.SelectedHex = null;
                    m_map.Game.CurrentTurnStage = Game.TurnStage.ChooseUnit;
                    break;
                case Game.TurnStage.ChooseHeading:

                    // Move the unit back to its previous location and heading; then return to ChooseMoveDestination.
                    Unit tempUnit = m_map.SelectedHex.Unit;
                    tempUnit.PlaceOnMap(m_map.SelectedUnitOriginHex, m_map.SelectedUnitOriginDirection);

                    m_map.SelectedHex = m_map.SelectedUnitOriginHex;
                    
                    // Take the unit off recharge.
                    tempUnit.CurrentRechargeTime = 0;

                    // Transition the game state.
                    m_map.SelectedHex.Unit.MoveTemplate.OnApply(
                        m_map,
                        new MapLocation(m_map.SelectedHex.m_mapPosition, m_map.SelectedHex.Unit.FacingDirection),
                        new HighlightEffect(m_map));
                    
                    m_map.Game.CurrentTurnStage = Game.TurnStage.ChooseMoveDestination;

                    break;
                case Game.TurnStage.ChooseAttackTarget:
                    // Go back to ChooseHeading
                    m_map.SelectedHex.HighlightAttackRange();

                    m_map.Game.CurrentTurnStage = Game.TurnStage.ChooseHeading;
                    break;
            }
        }

        
        

        public override void OnLeftClick(Vector2 mousePosInTexture)
        {
            switch (m_map.Game.CurrentTurnStage)
            {
                case Game.TurnStage.ChooseUnit:
                    m_map.SelectedHex = this;
                    if (Unit != null)
                    {
                        this.Unit.MoveTemplate.OnApply(
                            m_map,
                            new MapLocation(this.m_mapPosition, this.Unit.FacingDirection),
                            new HighlightEffect(m_map));

                        if (Unit.Owner == m_map.Game.CurrentPlayer)
                        {
                            if (Unit.CurrentRechargeTime > 0)
                            { SfxStore.Get(SfxId.unit_not_ready).Play(); }
                            else
                            {
                                m_map.Game.CurrentTurnStage = Game.TurnStage.ChooseMoveDestination;
                                Unit.PlaySfxSelected();
                            }
                        }
                    }
                    break;


                case Game.TurnStage.ChooseMoveDestination:
                    // This is hex that's just been clicked to move the unit to.
                    // m_map.selectedHex is the hex that the unit is moving from. 
                    if (this.IsHighlighted)
                    {   // Move the unit, and transition into choose-heading.

                        //Remember the origin position.
                        m_map.SelectedUnitOriginDirection = m_map.SelectedHex.Unit.FacingDirection;
                        m_map.SelectedUnitOriginHex = m_map.SelectedHex;
                        
                        
                        //Move the unit and selected hex
                        Unit tempUnit = m_map.SelectedHex.Unit;
                        tempUnit.PlaceOnMap(this);

                        m_map.SelectedHex = this;
                        // Put the unit on recharge.
                        Unit.CurrentRechargeTime = Unit.MaxRechargeTime;
                        // Transition the game state.
                        m_map.ClearHighlightedHexes();

                        tempUnit.TargetTemplate.OnApply(
                            m_map,
                            new MapLocation(this.m_mapPosition, tempUnit.FacingDirection),
                            new HighlightEffect(m_map)); 

                        m_map.Game.CurrentTurnStage = Game.TurnStage.ChooseHeading;
                        Unit.PlaySfxMove();
                    }
                    else
                    { // Go back to chooseUnit.  
                        m_map.Game.CurrentTurnStage = Game.TurnStage.ChooseUnit;
                        OnLeftClick(mousePosInTexture);
                    }
                
                    break;

                case Game.TurnStage.ChooseHeading:
                    // Valid attack targets are already highlighted. The selected hex can be clicked for recharge.
                    if (this.IsHighlighted || this.IsSelected)
                    {
                        m_map.Game.CurrentTurnStage = Game.TurnStage.ChooseAttackTarget;

                        // For now, always select your final heading and fire with the same click.
                        OnLeftClick(mousePosInTexture);
                    }
                    break;

                case Game.TurnStage.ChooseAttackTarget:
                    if (this.IsHighlighted)
                    {
                       

                        // The clicked hex is a valid target hex: attack it.
                        Unit attackingUnit = m_map.SelectedHex.Unit;
                        if (this.Unit == null && !attackingUnit.CanTargetEmptyHex)
                        {
                            SfxStore.Play(SfxId.thump);
                            m_map.Game.CurrentTurnStage = Game.TurnStage.ChooseHeading;
                            break;
                        }
                        attackingUnit.PlaySfxFire();

                        // Screen shaking (don't do this in hex-effect to avoid doing it for every hex effected.
                        // Only do it for units that are set to shake the screen when attacking.  
                        if ((attackingUnit.AttackEffect as UnitDamageEffect) != null &&
                            (attackingUnit.AttackEffect as UnitDamageEffect).ShakeScreenOnAttack)
                            ShakeForm.Shake();

                        ApplyDamageTemplate(attackingUnit, attackingUnit.AttackEffect);

                        m_map.SelectedHex = this;
                        m_map.Game.CurrentTurnStage = Game.TurnStage.EndTurn;
                    }
                    else if (this.IsSelected)
                    {
                        // The clicked hex is the units own hex: command it to recharge instead of firing.
                        Unit.CurrentRechargeTime = (Unit.CurrentRechargeTime +1) / 2;
                        // Recharging also heals a damaged unit slightly (20% of max).
                        int previousHP = Unit.CurrentHP;
                        Unit.CurrentHP = Math.Min(Unit.MaxHP, Unit.CurrentHP + (Unit.MaxHP / 5));
                        CreateDrawnTextEffect("+" + (Unit.CurrentHP - previousHP).ToString(), Color.DarkGreen);

                        if (ConfigManager.GlobalManager.UnitVoicesEnabled)
                            SfxStore.Play(SfxId.unit_recharge);
                        else
                            SfxStore.Play(SfxId.thump);

                        m_map.ClearHighlightedHexes();
                        m_map.Game.CurrentTurnStage = Game.TurnStage.EndTurn;
                    }

                    break;

                case Game.TurnStage.EndTurn:
                    // Let them continue to look at units before ending the turn.
                    m_map.SelectedHex = this;
                    break;

                default:
                    break;
            }
        }


        protected bool m_currentlyOriginOfDoubleHighlightArea = false;
        public override void OnContinueMouseover(TimeSpan timeHeld)
        {
            if ((m_map.Game.CurrentTurnStage == Game.TurnStage.ChooseHeading ||
                m_map.Game.CurrentTurnStage == Game.TurnStage.ChooseAttackTarget) &&
                this.IsHighlighted &&
                m_map.SelectedHex.Unit != null)
            {
                ApplyDamageTemplate(m_map.SelectedHex.Unit, new DoubleHighlightEffect(m_map));
                m_currentlyOriginOfDoubleHighlightArea = true;
            }
        }

        public override void OnStopMouseover(TimeSpan timeHeld)
        {
            if (m_currentlyOriginOfDoubleHighlightArea)
            {
                m_map.ClearDoubleHighlightedHexes();
                m_currentlyOriginOfDoubleHighlightArea = false;
            }
        }

        public void CreateExplosion()
        {
            drawnEffectList.Add(new ExplosionEffect(new Vector2(m_displayRectangle.Center.X, m_displayRectangle.Center.Y)));
        }
        public void CreateDrawnTextEffect(string text, Color color)
        {
            drawnEffectList.Add(new DrawnTextEffect(text, color, new Vector2(m_displayRectangle.Center.X, m_displayRectangle.Center.Y)));
        }


        public void AddUnitDeathEvent()
        {
            Unit.PlaySfxKilled();
            ClearUnit(); // if the unit's hp is <= 0 it dies.
            CreateExplosion(); // Show Explosion
        }


        public void HighlightAttackRange()
        {
            m_map.ClearHighlightedHexes();

            Hex onlyOneHex = null;
            Unit.TargetTemplate.OnApply(
                m_map,
                new MapLocation(m_mapPosition, Unit.FacingDirection),
                new HighlightEffect(m_map), 
                out onlyOneHex);

            if (onlyOneHex != null)
            {
                Unit.AttackTemplate.OnApply(
                    m_map,
                    new MapLocation(onlyOneHex.MapPosition, Unit.FacingDirection),
                    new HighlightEffect(m_map, onlyOneHex));
            }
        }


        public void ApplyDamageTemplate(Unit attackingUnit, HexEffect effect)
        {
            if (attackingUnit == null)
                throw new Exception("invalid unit!");
            // Apply the attack to this hex, using the proper template and effect. 
            Hex attackOrigin = (TemplateOriginHex == null) ? this : TemplateOriginHex;
            attackingUnit.AttackTemplate.OnApply(
                m_map,
                new MapLocation(attackOrigin.m_mapPosition, attackingUnit.FacingDirection),
                effect);
        }

    }
}
