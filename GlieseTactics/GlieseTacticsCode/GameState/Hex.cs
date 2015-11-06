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
    [Serializable]
    public class Hex : ClickableSprite 
    {
        [NonSerialized]
        public const int HEX_SIZE = 80;
        [NonSerialized]
        public const int HEX_SPACING = 2;
        [NonSerialized]
        public static Color SELECTED_TINT = new Color(50, 50, 50, 255);
        [NonSerialized]
        public static Color MOUSEOVER_TINT = new Color(100, 100, 100, 255);
        [NonSerialized]
        public static Color HIGHLIGHT_TINT_MOVE = new Color(50, 150, 150, 255);
        [NonSerialized]
        public static Color HIGHLIGHT_TINT_ATTACK = new Color(150, 150, 50, 255);
        [NonSerialized]
        public static Color DOUBLE_HIGHLIGHT_TINT = new Color(100, 50, 50, 255);

        // Pointer to the map that this hex is a part of.
        Map m_map;

        // Hex properties related to current map input. 
        [NonSerialized]
        public bool IsSelected = false;
        [NonSerialized]
        public bool IsHighlighted = false;
        [NonSerialized]
        public bool IsDoubleHighlighted = false;

        [NonSerialized] // used for the range template, to indicate if an effect has already been applied.
        public Dictionary<MapTemplate, bool> IsMarked = new Dictionary<MapTemplate,bool>();
        [NonSerialized] // used in range calculations.
        public int CurrentMoveCost;
        [NonSerialized] // used in range calculations.
        public Hex TemplateOriginHex = null; // used during damage template placment
        [NonSerialized]
        List<IDrawnEffect> drawnEffectList = new List<IDrawnEffect>();


        ///
        ///Perminant, serializable properties. 
        ///

        // Can units move onto-through this Hex?
        public bool LandMovementAllowed;

        // Is this hex part of any player's starting area?
        // (-1 means no.  0 means player 1, 1 means player 2 player 2, etc - index into the player list)
        public int PlayerStartingArea;

        
        // Is there a unit currently occupying this Hex?
        protected Unit m_unit = null;
        public Unit Unit { get { return m_unit; } }
        public void PlaceUnit(Unit unit) { m_unit = unit; }
        public void ClearUnit() 
        {
            Unit tempUnit = m_unit;
            m_unit = null;
            if (tempUnit != null && tempUnit.CurrentHex == this)
                tempUnit.ClearCurrentHex();
        }
        
        // Is a valid destination if it's possable and not already occupied.
        public bool IsValidDestination
        { get { return LandMovementAllowed && Unit == null; } }

        // Appearance variables.  
        protected Point m_mapPosition;
        protected Rectangle m_displayRectangle;


        public override Rectangle DisplayRect
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
        public Hex(Map owningMap, Texture2D texture, Point mapPosition, bool landMovementAllowed, int playerStartingArea)
            : base(texture, Rectangle.Empty, Color.White, 1f, 0f, Vector2.Zero, 0f)
        {
            m_map = owningMap;
            Texture = texture;
            MapPosition = mapPosition;
            LandMovementAllowed = landMovementAllowed;
            PlayerStartingArea = playerStartingArea;

            Tint = Color.White;
        }
        // Needed to make Hex Serializable.
        public Hex()
            : base(null, Rectangle.Empty, Color.White, 1f, 0f, Vector2.Zero, 0f)
        { }


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
            else if (LastMouseOver)
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

            // If there's a unit in this hex, draw it.  
            if (Unit != null && m_map.ShowUnit(Unit))
                Unit.Draw(spriteBatch, time);

            // Draw any effects (explosions, etc) - they are removed from the list when finished
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
        /// Needed because of the hex's non-standard shape.
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


        /// <summary>
        /// The Right click fuction is used to go back or undo actions becofe the turn is completed.
        /// </summary>
        public override void OnRightClick(Vector2 mousePosInTexture)
        {
            switch (m_map.Game.CurrentTurnStage)
            {
                // Right clicking unselects any hex.
                case Game.TurnStage.ChooseUnit:
                    m_map.SelectedHex = null;
                    break;

                // If choosing a move destination, right click brings you back to the chooseUnit phase and unselects the unit.
                case Game.TurnStage.ChooseMoveDestination:
                    m_map.SelectedHex = null;
                    m_map.Game.CurrentTurnStage = Game.TurnStage.ChooseUnit;
                    break;

                // Move the unit back to its previous location and heading; then return to ChooseMoveDestination.
                case Game.TurnStage.ChooseHeading:
                    Unit tempUnit = m_map.SelectedHex.Unit;
                    tempUnit.PlaceOnMap(m_map.SelectedUnitOriginHex, m_map.SelectedUnitOriginDirection);

                    m_map.SelectedHex = m_map.SelectedUnitOriginHex;
                    
                    // Take the unit off recharge.
                    tempUnit.CurrentRechargeTime = 0;

                    // Transition the game state.
                    m_map.SelectedHex.Unit.MoveTemplate.OnApply(
                        m_map,
                        new MapLocation(m_map.SelectedHex.m_mapPosition, m_map.SelectedHex.Unit.FacingDirection),
                        new HighlightEffect(m_map),
                        null, null);
                    
                    m_map.Game.CurrentTurnStage = Game.TurnStage.ChooseMoveDestination;

                    break;

                // Go back to ChooseHeading
                case Game.TurnStage.ChooseAttackTarget:
                    m_map.SelectedHex.HighlightAttackRange();
                    m_map.Game.CurrentTurnStage = Game.TurnStage.ChooseHeading;
                    break;
            }
        }

        
        
        /// <summary>
        /// Left clicking a hex is used to select units, movement, and targets and to confirm choices.
        /// </summary>
        public override void OnLeftClick(Vector2 mousePosInTexture)
        {
            Unit tempUnit; // placeholder unit pointer used in various operations in the state machine.

            switch (m_map.Game.CurrentTurnStage)
            {
                /////////////////////////////////////////////////////////////////////////////////////
                // Left clicking should select a unit and allow you to move it (if its yours)
                case Game.TurnStage.ChooseUnit:
                    m_map.SelectedHex = this;
                    if (this.Unit != null)
                    {
                        this.Unit.MoveTemplate.OnApply(
                            m_map,
                            new MapLocation(this.m_mapPosition, this.Unit.FacingDirection),
                            new HighlightEffect(m_map),
                            null, null);

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

                /////////////////////////////////////////////////////////////////////////////////////
                /// Left click to move a unit. 
                // 'this' is hex that's just been clicked to move the unit to.
                // m_map.selectedHex is the hex that the unit is moving from.                 
                case Game.TurnStage.ChooseMoveDestination:
                    
                    if (!this.IsHighlighted)
                    {   // If this isn't a valid move destination, go to ChooseUnit and then 
                        // treat it as if they had clicked here during that phase. 
                        m_map.Game.CurrentTurnStage = Game.TurnStage.ChooseUnit;
                        OnLeftClick(mousePosInTexture);
                        break;
                    }

                    // Move the unit, and transition into choose-heading.

                    //Remember the origin position (for undoing move).
                    m_map.SelectedUnitOriginDirection = m_map.SelectedHex.Unit.FacingDirection;
                    m_map.SelectedUnitOriginHex = m_map.SelectedHex;
                        
                    //Move the unit and selected hex
                    tempUnit = m_map.SelectedHex.Unit;
                    tempUnit.PlaceOnMap(this);
                    m_map.SelectedHex = this;

                    // Put the unit on recharge.
                    Unit.CurrentRechargeTime = Unit.MaxRechargeTime;
                        
                    // Transition the game state: highlight target-template hexes and go to choose-heading.
                    m_map.ClearHighlightedHexes();
                    tempUnit.TargetTemplate.OnApply(
                        m_map,
                        new MapLocation(this.m_mapPosition, tempUnit.FacingDirection),
                        new HighlightEffect(m_map), 
                        null, null); 
                    m_map.Game.CurrentTurnStage = Game.TurnStage.ChooseHeading;
                    Unit.PlaySfxMove();
                
                    break;

                /////////////////////////////////////////////////////////////////////////////////////
                /// If this is a valid target hex, then go to ChooseAttackTarget immediately, and 
                /// then perform the action for the attack (call OnLeftClick again in the ChooseAttackTarget state).
                case Game.TurnStage.ChooseHeading:
                    // Valid attack targets are already highlighted. The selected hex can be clicked for recharge.
                    if (this.IsHighlighted || this.IsSelected)
                    {
                        m_map.Game.CurrentTurnStage = Game.TurnStage.ChooseAttackTarget;

                        // For now, always select your final heading and fire with the same click.
                        OnLeftClick(mousePosInTexture);
                    }
                    break;

                /////////////////////////////////////////////////////////////////////////////////////
                /// If this is a valid hex to perform an attack/recharge action, then do so and proceed 
                /// to end the turn.  
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


                /////////////////////////////////////////////////////////////////////////////////////
                /// The turn is over...can't do anything or get to any other state by 
                /// clicking on hexes.
                case Game.TurnStage.EndTurn:
                    // Let them continue to look at units before ending the turn.
                    m_map.SelectedHex = this;
                    break;

                /////////////////////////////////////////////////////////////////////////////////////
                /// Select a unit on the map (if it's yours) and get ready 
                /// to move it anywhere in the current player's starting area.
                case Game.TurnStage.PlacementChooseUnit:
                    m_map.SelectedHex = this;

                    m_map.HighlightStartingArea(m_map.Game.CurrentPlayerIndex);

                    if (this.Unit != null && this.Unit.Owner == m_map.Game.CurrentPlayer)
                    {
                        Unit.PlaySfxSelected();
                        m_map.Game.CurrentTurnStage = Game.TurnStage.PlacementChooseDestination;
                    }
                    break;

                /////////////////////////////////////////////////////////////////////////////////////
                /// If the hex is in the current player's starting area, move the selected unit
                /// there and go back to PlacementChooseUnit.
                case Game.TurnStage.PlacementChooseDestination:

                    if (this.PlayerStartingArea == m_map.Game.CurrentPlayerIndex && LandMovementAllowed)
                    {
                        tempUnit = m_map.SelectedHex.Unit;
                        tempUnit.ClearCurrentHex();
                        tempUnit.PlaySfxMove();

                        if (this.Unit != null)
                        {
                            this.Unit.PlaceOnMap(m_map.SelectedHex);
                            this.ClearUnit();
                        }

                        tempUnit.PlaceOnMap(this);

                        m_map.SelectedHex = this;
                        m_map.HighlightStartingArea(m_map.Game.CurrentPlayerIndex);
                    }

                    m_map.Game.CurrentTurnStage = Game.TurnStage.PlacementChooseUnit;
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
                HexEffectStats stats = ApplyDamageTemplate(m_map.SelectedHex.Unit, new DoubleHighlightEffect(m_map, m_map.SelectedHex.Unit));
                m_map.ExpectedAttackStats = stats;
                m_currentlyOriginOfDoubleHighlightArea = true;
            }

            // Highlight the total threat range and maximum possible damage and kills.
            if (IsSelected && Unit != null && 
                timeHeld > new TimeSpan(0, 0, 2) && 
                !m_currentlyOriginOfDoubleHighlightArea)
            {
                HexEffectStats stats = Unit.MoveTemplate.OnApply(m_map, Unit.MapLocation, 
                    new RecursiveTemplateEffect(m_map, Unit.TargetTemplate, true, true,
                        new RecursiveTemplateEffect(m_map, Unit.AttackTemplate, false, false,
                            new DoubleHighlightEffect(m_map, Unit))), 
                            Unit.CurrentHex, null/*could change to show player what the AI would do*/);
                
                m_currentlyOriginOfDoubleHighlightArea = true;
                m_map.ExpectedAttackStats = stats;
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
                null, null,
                out onlyOneHex);

            if (onlyOneHex != null)
            {
                Unit.AttackTemplate.OnApply(
                    m_map,
                    new MapLocation(onlyOneHex.MapPosition, Unit.FacingDirection),
                    new HighlightEffect(m_map, onlyOneHex), 
                    Unit.CurrentHex, 
                    null);
            }
        }


        public HexEffectStats ApplyDamageTemplate(Unit attackingUnit, HexEffect effect)
        {
            if (attackingUnit == null)
                throw new Exception("invalid unit!");
            // Apply the attack to this hex, using the proper template and effect. 
            Hex attackOrigin = (TemplateOriginHex == null) ? this : TemplateOriginHex;
            return attackingUnit.AttackTemplate.OnApply(
                m_map,
                new MapLocation(attackOrigin.m_mapPosition, attackingUnit.FacingDirection),
                effect,
                attackingUnit.CurrentHex, 
                null);
        }

    }
}
