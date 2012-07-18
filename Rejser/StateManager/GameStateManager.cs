using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace Kreen.StateManager
{
    public class GameStateManager : DrawableGameComponent
    {
        private Game game;

        private bool initialized = false;

        private List<GameState> gameStates = new List<GameState>();
        private List<GameState> waitingGamestates = new List<GameState>();
        private InputState inputState = new InputState();


        #region Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        public GameStateManager(Game game)
               : base(game)
        {
            this.game = game;
        }

        public override void Initialize()
        {
            base.Initialize();

            foreach (GameState state in gameStates)
            {
                Debug.WriteLine("Initializing state...");
                state.Initialize();
            }

            this.initialized = true;
        }

        /// <summary>
        /// Loads content for game and all states
        /// </summary>
        protected override void LoadContent()
        {
            foreach (GameState gameState in gameStates)
            {
                gameState.LoadContent();
            }
        }

        /// <summary>
        /// Unloads content for game and all states
        /// </summary>
        protected override void UnloadContent()
        {
            foreach (GameState gameState in gameStates)
            {
                gameState.UnloadContent();
            }
        }

        #endregion

        #region Update and draw

        public override void Update(GameTime gameTime)
        {
            inputState.Update();

            // Clear waiting gamestates
            waitingGamestates.Clear();

            // Create new list of gamestates to update
            foreach (GameState gameState in gameStates)
                waitingGamestates.Add(gameState);

            bool otherStateHasFocus = !Game.IsActive;
            bool covered = false;

            while (waitingGamestates.Count > 0)
            {
                // Pop last gamestate from waiting queue
                GameState currentState = waitingGamestates[waitingGamestates.Count - 1];
                waitingGamestates.RemoveAt(waitingGamestates.Count - 1);

                // Update the current state
                currentState.Update(gameTime, otherStateHasFocus);

                // Handle input
                currentState.HandleInput(inputState);

                // Set covered
                covered = true;
            }
        }


        /// <summary>
        /// Calls draw method on all gamestates
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            foreach (GameState gameState in gameStates)
            {
                if (gameState.Status == StateStatus.Hidden)
                    continue;

                gameState.Draw(gameTime);
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Adds a new gamestate to gamestate manager
        /// </summary>
        public void AddGameState(GameState state)
        {
            if (initialized)
            {
                state.LoadContent();
                state.Initialize();
            }

            gameStates.Add(state);
        }

        public void RemoveGameState(GameState state)
        {
            if (initialized)
            {
                state.UnloadContent();
            }

            gameStates.Remove(state);
            waitingGamestates.Remove(state);
        }

        public void Quit()
        {
            game.Exit();
        }

        #endregion
    }
}
