using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Gliese581g.ComputerPlayers;


namespace Gliese581g
{
    public class Game
    {
        public enum MapType
        {
            Random,
            FromFile,
            NotSet
        }

        public enum ArmySize
        {
            Small = 1,
            Medium = 2,
            Large = 3,
            NotSet = 0
        }

        public enum MapSize
        {
            Small,
            Medium,
            Large,
            NotSet
        }

        public enum VictoryType
        {
            Elimination,
            Assassination,
            NotSet
        }

        public enum TurnStage
        {
            NotYetStarted,

            PlacementBegin,
            PlacementChooseUnit,
            PlacementChooseDestination,

            BeginTurn,
            ChooseUnit,
            ChooseMoveDestination,
            ChooseHeading,
            ChooseAttackType,
            ChooseAttackTarget,
            EndTurn,

            GameOver
        }

        public VictoryType VictoryCondition;
        public List<Commander> Players;

        private bool m_surrender;
        public bool Surrender
        {
            get { return m_surrender; }
            set { m_surrender = value; }
        }


        // The current overall turn, player, and stage within that player's turn.
        int m_currentTurn;
        public int CurrentTurn { get { return m_currentTurn; } }
        Commander m_currentPlayer;
        public Commander CurrentPlayer { get { return m_currentPlayer; } }
        public int CurrentPlayerIndex { get { return Players.IndexOf(m_currentPlayer); } }

        // Remains null until the game is over and a winner has been determined.
        protected Commander m_winningPlayer = null;
        public Commander WinningPlayer { get { return m_winningPlayer; } }
        public Commander LosingPlayer { get { return m_winningPlayer == Players[0] ? Players[1] : Players[0]; } }

        TurnStage m_currentTurnStage;
        public TurnStage CurrentTurnStage
        {
            get { return m_currentTurnStage; }
            set 
            { 
                m_currentTurnStage = value;
                if (m_currentTurnStage == TurnStage.EndTurn)
                    CheckForGameOver();
            }
        }

        // In the case of AI or network/scripted game of some kind, there may be pending instructions for the current turn.
        private TurnInstructions m_pendingInstructions = null;
        public bool HasInstructions { get { return m_pendingInstructions != null; } }
        public TurnInstructions PendingInstructions { get { return m_pendingInstructions; } }


        public Game(List<Commander> players, VictoryType victoryCondition)
        {
            Players = players;
            VictoryCondition = victoryCondition;

            m_currentTurn = 0;
            m_currentPlayer = Players[0];
            m_currentTurnStage = TurnStage.PlacementBegin;
        }


        public void BeginPlacement()
        {
            if (m_currentTurnStage == TurnStage.PlacementBegin)
            {
                m_currentTurnStage = TurnStage.PlacementChooseUnit;
            }
        } 


        public void BeginTurn(Map gameMap /*needed by AI to generate new turn's instructions*/)
        {
            if (m_currentTurnStage == TurnStage.BeginTurn)
            {
                // At the beginning of the turn, the units of the current player recharge
                // get their movement points back, etc.  
                bool canMove = false;
                
                foreach (Unit unit in m_currentPlayer.MyUnits)
                {
                    if (unit.CurrentRechargeTime > 0)
                        unit.CurrentRechargeTime--;
                    // If any unit is both alive and ready then this player can move on his turn.
                    if (unit.CurrentRechargeTime <= 0 && unit.CurrentHP > 0)
                        canMove = true;                    
                }

                //If no unit is both alive and ready, skip directly to end turn. 
                if (!canMove)
                    m_currentTurnStage = TurnStage.EndTurn;
                else
                {
                    m_currentTurnStage = TurnStage.ChooseUnit;

                    // If the current player is an AI, he now needs to consider the game board and
                    // formulate instructions for the coming turn. 
                    // These instructions can then be set in the Map and executed during subsequent updates.
                    if (!m_currentPlayer.IsHuman)
                    {
                        m_pendingInstructions = m_currentPlayer.GetNextMove(gameMap);
                    }
                }
            }
        }



        public Commander NextPlayer
        { get { return Players[(Players.IndexOf(CurrentPlayer) + 1) % Players.Count]; } }

        public void EndPlacement()
        {
            m_currentPlayer = NextPlayer;

            // If the next player is the first player again, then 
            // everyone is done placing - goto BeginTurn.
            if (m_currentPlayer == Players[0])
                m_currentTurnStage = TurnStage.BeginTurn;
            else 
                m_currentTurnStage = TurnStage.PlacementBegin;
        }

        public void EndTurn()
        {
            m_currentPlayer = NextPlayer;
            m_currentTurnStage = TurnStage.BeginTurn;

            // Always clear instructions at the end of the turn.
            m_pendingInstructions = null;
        }

        public void CheckForGameOver()
        {
            int remainingPlayers = Players.Count;
            Commander lastManStanding = null;
            
            foreach (Commander player in Players)
            {
                bool playerStillInTheRunning;
                switch (VictoryCondition)
                {
                    case VictoryType.Assassination:
                        playerStillInTheRunning = player.HasLiveCommander && player.Surrender == false;
                        break;
                    case VictoryType.Elimination:
                        playerStillInTheRunning = player.HasLiveUnit && player.Surrender == false;
                        break;
                    default:
                        throw new Exception("Invalid victory condition.");
                }
                if (playerStillInTheRunning)
                    lastManStanding = player;
                else
                    remainingPlayers--;
            }

            if (remainingPlayers < 1)
                throw new Exception("Everyone is dead and there is no winner?!? Doesn't seem right.");

            if (remainingPlayers == 1) // there can be only one
            {
                m_winningPlayer = lastManStanding;
                m_currentPlayer = null;  //It's nobodies turn anymore.
                m_currentTurnStage = TurnStage.GameOver;
            }
        }



        public void InitArmies(ArmySize armySize)
        {
            if (VictoryCondition == Game.VictoryType.Assassination)
            {
                Unit commander1 = Unit.UnitFactory.MakeCommander(Players[0].Name);
                Players[0].AddUnit(commander1);

                Unit commander2 = Unit.UnitFactory.MakeCommander(Players[1].Name);
                Players[1].AddUnit(commander2);
            }

            for (int ii = 0; ii < ((int)armySize * 2) + 2; ii++) //2,3,4
            {
                Players[ii % 2].AddUnit(Unit.UnitFactory.MakeInfantry()); 
            }

            for (int ii = 0; ii < ((int)armySize * 2)+ 2; ii++) //2,3,4
            {
                Players[ii % 2].AddUnit(Unit.UnitFactory.MakeScout());
            }

            for (int ii = 0; ii < ((int)armySize * 2) + 2; ii++) //2,3,4
            {
                Players[ii % 2].AddUnit(Unit.UnitFactory.MakeRoughRider());
            }

            for (int ii = 0; ii < ((int)armySize * 2) + 2; ii++) //2,3,4
            {
                Players[ii % 2].AddUnit(Unit.UnitFactory.MakeTank());
            }

            for (int ii = 0; ii < ((int)armySize * 2); ii++) //1,2,3
            {
                Players[ii % 2].AddUnit(Unit.UnitFactory.MakeArtillery());
            }

            for (int ii = 0; ii < ((int)armySize * 2) - 2; ii++) //0,1,2
            {
                Players[ii % 2].AddUnit(Unit.UnitFactory.MakeMech());
            }
            return;
        }



    }
}
