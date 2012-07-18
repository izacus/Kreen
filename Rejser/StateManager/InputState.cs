using Microsoft.Xna.Framework.Input;

namespace Kreen.StateManager
{
    public class InputState
    {
        private KeyboardState keyboardState;

        public InputState()
        {
            
        }

        /// <summary>
        /// Update current input status
        /// </summary>
        public void Update()
        {
            keyboardState = Keyboard.GetState();
        }

        public KeyboardState KeyState
        {
            get { return keyboardState; }
        }
    }
}
