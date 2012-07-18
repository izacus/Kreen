using Microsoft.Xna.Framework;

namespace Kreen.StateManager
{
    /// <summary>
    /// Holds possible statuses for a game state
    /// </summary>
    public enum StateStatus
    {
        Active,
        Hidden
    }

    public abstract class GameState
    {
        // Properties
        public GameStateManager gameStateManager;
        private StateStatus status = StateStatus.Hidden;
        private bool otherStateHasFocus;

        public GameState(GameStateManager gameStateManager)
        {
            this.gameStateManager = gameStateManager;
        }

        public virtual void Initialize() { }
        public virtual void LoadContent() { }
        public virtual void UnloadContent() { }

        public virtual void Update(GameTime gameTime, bool otherStateHasFocus)
        {
            this.otherStateHasFocus = otherStateHasFocus;
        }

        // TODO: input handler
        public virtual void HandleInput(InputState inputState) { }
        public virtual void Draw(GameTime gameTime) { }


        public bool IsActive
        {
            get { return !otherStateHasFocus && this.status == StateStatus.Active; }
        }

        public StateStatus Status
        {
            get { return status; }
            set { this.status = value; }
        }

        public GameStateManager GameStateManager
        {
            get { return gameStateManager; }
        }
    }
}
