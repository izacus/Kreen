using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Kreen.StateManager;
using BloomPostprocess;
using Kreen.Menu;

namespace Kreen
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Kreen : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        GameStateManager gameStateManager;
        BloomComponent bloomComponent;

        public Kreen()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Set resolution
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;

            graphics.IsFullScreen = false;
            graphics.PreferMultiSampling = false;

            graphics.SynchronizeWithVerticalRetrace = true;
            this.IsFixedTimeStep = true;

            graphics.MinimumPixelShaderProfile = ShaderProfile.PS_2_0;
            graphics.MinimumVertexShaderProfile = ShaderProfile.VS_2_0;

            gameStateManager = new GameStateManager(this);
            bloomComponent = new BloomComponent(this);

            Components.Add(gameStateManager);
            Components.Add(bloomComponent);

#if DEBUG
            FPS fps = new FPS(this);
            Components.Add(fps);
#endif

            // Add initial game state
            gameStateManager.AddGameState(new StateMenu(gameStateManager));
        }

        #region Initialization

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {

        }

        #endregion

        #region Update

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        #endregion

        #region Draw

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        #endregion
    }

    #region Entry point

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Kreen game = new Kreen())
            {
                game.Run();
            }
        }
    }

    #endregion
}
