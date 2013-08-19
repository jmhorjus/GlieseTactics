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

    public class Map : IDrawnObject, IUpdatedObject
    {
        // The way the map responds to input is very closely tied into game state,
        // and map input is the main way to change the game state. 
        public Game Game;

        public Hex[,] m_hexArray; //The array of Hex objects that make up the map.
        public int Rows { get { return m_hexArray.GetLength(0); } }
        public int Columns { get { return m_hexArray.GetLength(1); } }

        // Visible/Enabled switches.
        public bool Enabled = true;
        public bool Visible = true;



        //The Array to store each tiles of image and position.
        public Hex GetHex(Point mapCoordinates)
        {
            if (mapCoordinates.X < 0 || 
                mapCoordinates.Y < 0 ||
                mapCoordinates.X >= Rows || 
                mapCoordinates.Y >= Columns)
                return null;

            return m_hexArray[mapCoordinates.X, mapCoordinates.Y]; 
        }

        
        Camera m_camera;

        Texture2D m_blockingHexTexture;
        Texture2D m_defaultHexTexture;


        public Hex SelectedUnitOriginHex = null;
        public Direction SelectedUnitOriginDirection;
       // public List<HexEffect>
        
        Hex m_selectedHex = null;
        public Hex SelectedHex
        {
            get { return m_selectedHex; }
            set 
            {
                if (m_selectedHex != null)
                    m_selectedHex.IsSelected = false;

                if (value != null)
                    value.IsSelected = true;
                
                m_selectedHex = value;
                ClearHighlightedHexes();
            }
        }


        // Stats for displaying in the GUI.
        public HexEffectStats ExpectedAttackStats = new HexEffectStats();

        /// First Layer of highlighting - user for move and attack ranges.
        List<Hex> m_highlightedHexes = new List<Hex>();
        public void HighlightHex(Hex hex)
        {
            if (hex == null) 
                return;
            hex.IsHighlighted = true;
            m_highlightedHexes.Add(hex);
        }
        public void ClearHighlightedHexes()
        {
            foreach (Hex hex in m_highlightedHexes)
                hex.IsHighlighted = false;
            m_highlightedHexes.Clear();
        }
        public void HighlightStartingArea(int playerIndex)
        {
            ClearHighlightedHexes();
            for (int y = 0; y < m_hexArray.GetLength(1); y++)
                for (int x = 0; x < m_hexArray.GetLength(0); x++)
                    if (m_hexArray[x, y] != null && m_hexArray[x, y].PlayerStartingArea == playerIndex) 
                        HighlightHex(m_hexArray[x, y]);
        }

        // Second Layer of Highlighting - used for attack area of effect.  
        List<Hex> m_doubleHighlightedHexes = new List<Hex>();
        public void DoubleHighlightHex(Hex hex)
        {
            hex.IsDoubleHighlighted = true;
            m_doubleHighlightedHexes.Add(hex);
        }
        public void ClearDoubleHighlightedHexes()
        {
            foreach (Hex hex in m_doubleHighlightedHexes)
                hex.IsDoubleHighlighted = false;
            m_doubleHighlightedHexes.Clear();
            ExpectedAttackStats.Clear();
        }


        // An invisible temporary mark - used to ensure some effect are not applied twice (by the range template). 
        Dictionary<MapTemplate, List<Hex>> m_markedHexes = new Dictionary<MapTemplate,List<Hex>>();
        public void MarkHex(MapTemplate template, Hex hex)
        {
            if (!m_markedHexes.ContainsKey(template))
                m_markedHexes[template] = new List<Hex>();
            hex.IsMarked[template] = true;
            m_markedHexes[template].Add(hex);
        }
        public void ClearMarkedHexes(MapTemplate template)
        {
            if (m_markedHexes.ContainsKey(template))
            {
                foreach (Hex hex in m_markedHexes[template])
                    hex.IsMarked.Remove(template);
                m_markedHexes.Remove(template);
            }
        }


        public Map(int Rows, int Columns, Camera camera, Texture2D greenTexture, Texture2D blueTexture)
        {
            m_camera = camera;

            m_defaultHexTexture = greenTexture;
            m_blockingHexTexture = blueTexture;

            m_hexArray = new Hex[Rows, Columns];
        }


        /// <summary>
        /// The draw function is only for drawing!
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            for (int y = 0; y < m_hexArray.GetLength(1); y++)
            {
                for (int x = 0; x < m_hexArray.GetLength(0); x++)
                {
                    Hex hex = m_hexArray[x, y];
                    if (hex != null)
                        hex.Draw(spriteBatch, gameTime);
                }
            }
        }


        /// <summary>
        /// The update function is only for updating!
        /// </summary>
        protected MouseState m_lastMouseState = new MouseState();
        public Direction ChooseHeadingDirection = new Direction(Direction.ValueType.Right);
        public bool Update(MouseState mouseState, Matrix transformMatrix, GameTime time, bool mouseAlreadyIntercepted)
        {
            Vector2 mousePos = new Vector2(mouseState.X, mouseState.Y);
            Vector2 transformedMousePos = Vector2.Transform(mousePos, Matrix.Invert(transformMatrix));
            Point transformedPoint = new Point((int)transformedMousePos.X, (int)transformedMousePos.Y);

            /// Actions which depend on the turn stage but are not related to a specific hex.
            /// These happen even if the map is not intercepting the mouse. 
            switch (Game.CurrentTurnStage)
            {
                case Game.TurnStage.BeginTurn:
                    Game.BeginTurn();
                    break;
                case Game.TurnStage.ChooseHeading:
                    // Calculate the direction
                    Point center = m_selectedHex.DisplayRect.Center;
                    int direction = 0;
                    direction += (transformedPoint.X > center.X) ? 1 : 0;
                    direction += (transformedPoint.Y > center.Y + ((transformedPoint.X - center.X) / 2)) ? 2 : 0;
                    direction += (transformedPoint.Y > center.Y - ((transformedPoint.X - center.X) / 2)) ? 4 : 0;
                    switch (direction)
                    {
                        case 5:
                            ChooseHeadingDirection = Direction.Right;
                            break;
                        case 1:
                            ChooseHeadingDirection = Direction.UpRight;
                            break;
                        case 0:
                            ChooseHeadingDirection = Direction.UpLeft;
                            break;
                        case 2:
                            ChooseHeadingDirection = Direction.Left;
                            break;
                        case 6:
                            ChooseHeadingDirection = Direction.DownLeft;
                            break;
                        case 7:
                            ChooseHeadingDirection = Direction.DownRight;
                            break;
                        //default:
                        //throw new Exception("this should never happen.");
                    }
                    if (m_selectedHex.Unit.FacingDirection != ChooseHeadingDirection)
                    {
                        m_selectedHex.Unit.FacingDirection = ChooseHeadingDirection;

                        m_selectedHex.HighlightAttackRange();
                    }

                    break;

                case Gliese581g.Game.TurnStage.PlacementBegin:
                    HighlightStartingArea(Game.CurrentPlayerIndex);
                    Game.BeginPlacement();
                    break;
            }


            /// Now we check the mouse - actions after this point in the function
            /// depend on mouse input.
            if (!Enabled || mouseAlreadyIntercepted)
                return false;

            
            // Call update on each hex in the map.
            bool retVal = false;
            for (int y = 0; y < Columns; y++)
            {
                for (int x = 0; x < Rows; x++)
                {
                    if (m_hexArray[x, y] != null)
                        retVal = m_hexArray[x, y].Update(mouseState, transformedMousePos, time) || retVal;
                }
            }


            // Keep track of last mouse state.  
            m_lastMouseState = mouseState;

            return retVal;
        }
        
        // Should a certain unit be displayed on the map (based on owner, turn stage, fog, etc).
        public bool ShowUnit(Unit unit)
        {
            if (this.Game.CurrentTurnStage == Game.TurnStage.NotYetStarted ||
                this.Game.CurrentTurnStage == Game.TurnStage.PlacementBegin ||
                this.Game.CurrentTurnStage == Game.TurnStage.PlacementChooseUnit ||
                this.Game.CurrentTurnStage == Game.TurnStage.PlacementChooseDestination)
            {
                if (unit.Owner != this.Game.CurrentPlayer)
                    return false;
            }

            return true;
        }
        
        private bool IsValidDestination(Point pos)
        {
            Hex hex = GetHex(pos);
            return (hex != null) && hex.IsValidDestination;
        }





        ///------------------------------------------------------
        /// Functions for setting up both random and saved maps. 
        /// 
        public bool IsValidStartHex(int x_position, int playerIndex)
        {
            int startRows = m_hexArray.GetLength(0) / 3;
            if (playerIndex == 0)
                return x_position < startRows;
            else
                return x_position >= m_hexArray.GetLength(0) - startRows;
        }

        /// Initialize a random map based on the current size of the hex array.  
        public void InitRandomMap()
        {
            Random random = new Random();
            ///Assign the image to each of tiles 
            for (int x = 0; x < Rows; x++)
            {
                for (int y = 0; y < Columns; y++)
                {
                    bool blocking = random.Next(0, 5) == 0;
                    int playerStartingArea = -1;

                    // Starting areas.
                    if (IsValidStartHex(x, 0))
                        playerStartingArea = 0;
                    else if (IsValidStartHex(x, 1))
                        playerStartingArea = 1;

                    m_hexArray[x, y] = new Hex(this, 
                        blocking ? m_blockingHexTexture : m_defaultHexTexture, 
                        new Point(x, y), 
                        blocking ? false : true, 
                        playerStartingArea);
                }
            }
        }

        /// function that places the player's unit's randomly on the map. 
        public void PlaceArmiesRandomlyOnMap()
        {
            Random rand = new Random();
            for (int ii = 0; ii < Game.Players.Count; ii++)
            {
                foreach (Unit unit in Game.Players[ii].MyUnits)
                {
                    bool placementSuccessful = false;
                    int tries = 0;
                    while (!placementSuccessful)
                    {
                        Point point = Point.Zero;
                        point.X = rand.Next(0, Rows);
                        point.Y = rand.Next(0, Columns);
                        if (!IsValidStartHex(point.X, ii) ||
                            !(placementSuccessful = unit.PlaceOnMap(this, point, ii == 0 ? Direction.Right : Direction.Left)))
                        {
                            tries++;
                            if (tries > 1000)
                            {
                                // Try to find *any* place that's valid
                                foreach (Hex hex in m_hexArray)
                                {
                                    if (IsValidStartHex(hex.MapPosition.X, ii) && hex.Unit == null)
                                    {
                                        // Clear terrain if neccessary.
                                        if (!hex.LandMovementAllowed)
                                        {
                                            hex.LandMovementAllowed = true;
                                            hex.Texture = m_defaultHexTexture;
                                        }
                                        if (unit.PlaceOnMap(hex, ii == 0 ? Direction.Right : Direction.Left))
                                        {
                                            placementSuccessful = true;
                                            break;
                                        }
                                    }
                                }
                                if (!placementSuccessful)
                                {
                                    placementSuccessful = true; // For now, just let them be purged. (move to reinforcements later)
                                    //throw new Exception("could find a place for this unit!");
                                }
                            }                             
                        } // try to place
                    } // while not placed
                } // each unit
            } // each player
            PurgeUnplacedUnitsFromGame();
            return;
        }


        void PurgeUnplacedUnitsFromGame()
        {
            // How many units to they have?
            int unitsBeforePurge = Game.Players[0].MyUnits.Count + Game.Players[1].MyUnits.Count;

            // Clear the unit lists of both players completely. Any units not on the map immediately become unreferanced. 
            Game.Players[0].MyUnits = new List<Unit>();
            Game.Players[1].MyUnits = new List<Unit>();

            // Now remake the lists using only units on the map. 
            for (int x = 0; x < Rows; x++)
                for (int y = 0; y < Columns; y++)
                    if (m_hexArray[x, y] != null && m_hexArray[x,y].Unit != null)
                    {   //Add the unit to its own owners list. 
                        m_hexArray[x, y].Unit.Owner.MyUnits.Add(m_hexArray[x, y].Unit);
                    }

            if (unitsBeforePurge != Game.Players[0].MyUnits.Count + Game.Players[1].MyUnits.Count)
            {
                ;
                //throw new Exception("Some unplaced units were purged!");
            }
        }


        // Load a map from a file by parsing it in from text.
        enum ParseState { LookForHex, SecondHexChar, LookForSpace };
        bool isNumeral(char ch)
        {
            return ch == '0' || ch == '1' || ch == '2' || ch == '3' || ch == '4' ||
                ch == '5' || ch == '6' || ch == '7' || ch == '8' || ch == '9';
        }
        int chToInt(char ch)
        {
            switch (ch)
            {
                case '0': return 0;
                case '1': return 1;
                case '2': return 2;
                case '3': return 3;
                case '4': return 4;
                case '5': return 5;
                case '6': return 6;
                case '7': return 7;
                case '8': return 8;
                case '9': return 9;
                default: return int.MaxValue;
            }
        }
        public void InitMapFromFile(string fileName)
        {
            // First we have to make a dictionary of lists of units for each player. :)
            List<Dictionary<UnitType, List<Unit>>> Units = new List<Dictionary<UnitType, List<Unit>>>();
            Units.Add(new Dictionary<UnitType, List<Unit>>());
            Units.Add(new Dictionary<UnitType, List<Unit>>());

            for (int ii = 0; ii < Game.Players.Count; ii++)
                foreach (Unit unit in Game.Players[ii].MyUnits)
                {
                    if (!Units[ii].ContainsKey(unit.TypeOfUnit))
                        Units[ii][unit.TypeOfUnit] = new List<Unit>();
                    Units[ii][unit.TypeOfUnit].Add(unit);
                }


            System.IO.StreamReader sr = new System.IO.StreamReader(fileName);
            string mapText = sr.ReadToEnd();

            int currentHexInRow = 0;
            int currentRow = 0;
            int currentPlayerIndex = 0;
            UnitType currentUnitType = 0;
            ParseState state = ParseState.LookForHex;
            char lastCh = ' ';

            // This loop parses the map file character by character.
            foreach (char ch in mapText)
            {
                if (ch == '\n')
                {
                    currentRow++;
                    currentHexInRow = 0;
                    currentPlayerIndex = 0;
                    state = ParseState.LookForHex;
                    continue;
                }

                if (ch == '|')
                {
                    currentPlayerIndex = 1;
                }

                switch (state)
                {
                    case ParseState.LookForHex: // look for a numeral or x.
                        if (ch == 'X' || ch == 'x')
                        {
                            m_hexArray[currentHexInRow, currentRow] = new Hex(
                                this,
                                m_blockingHexTexture,
                                new Point(currentHexInRow, currentRow),
                                false,
                                -1);
                            currentHexInRow++;
                            state = ParseState.LookForSpace;
                        }
                        else if (isNumeral(ch))
                        {
                            m_hexArray[currentHexInRow, currentRow] = new Hex(
                                this,
                                m_defaultHexTexture,
                                new Point(currentHexInRow, currentRow),
                                true,
                                -1);
                            currentUnitType = (UnitType)chToInt(ch);
                            state = ParseState.SecondHexChar;
                        }
                        break;
                    case ParseState.SecondHexChar:
                        if (isNumeral(ch))
                        {
                            int unitIndex = chToInt(ch);
                            if (Units[currentPlayerIndex].ContainsKey(currentUnitType) &&
                                unitIndex < Units[currentPlayerIndex][currentUnitType].Count)
                                Units[currentPlayerIndex][currentUnitType][unitIndex].PlaceOnMap(
                                    m_hexArray[currentHexInRow, currentRow],
                                    currentPlayerIndex == 0 ? Direction.Right : Direction.Left);

                            state = ParseState.LookForSpace;
                        }
                        else
                        { throw new Exception("invalid map format!"); }
                        break;
                    case ParseState.LookForSpace:
                        if (ch == ' ' || ch == '|')
                            currentHexInRow++;
                        state = ParseState.LookForHex;
                        break;
                }
                lastCh = ch;
            }

            // After loading a map froma file we need to purge unplaced units.
            PurgeUnplacedUnitsFromGame(); 
        }



    }
}
