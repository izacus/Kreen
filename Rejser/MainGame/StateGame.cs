using System;
using Kreen.StateManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;
using Kreen.Menu;
using BloomPostprocess;

namespace Kreen.MainGame
{
    class StateGame : GameState
    {
        private StateMenu menuState;
        private ContentManager contentManager;
        // Sky
        private Skybox skyPlane;
        // Track
        private Track track;
        // Car
        private Car playerCar;

        // HUD font
        private SpriteFont font;

        // View
        private Matrix projection;
        private Matrix view;
        private Matrix world;

        // Camera parameters
        private Vector3 cameraPosition;
        private Vector3 cameraTarget = Vector3.Zero;
        private Vector3 cameraUpDirection = Vector3.Up;
        private Vector3 lightDirection = new Vector3(0.033333f, 0.6666667f, 0.6666667f);

        // Shadow mapping objects
        private RenderTarget2D shadowMapRenderTarget;
        private Texture2D shadowMap;

        // HUD
        private SpriteBatch HUD;

        // Racing start time
        private long raceTimeMillis = -1;
        private Boolean updateRaceTime = true;
        private int endCounter;

        #region Initialization

        public StateGame(GameStateManager gameStateManager) : base(gameStateManager)
        {
            playerCar = new Car(this);
            track = new Track();
            skyPlane = new Skybox();
        }

        public override void Initialize()
        {
            // Generate skyplane
            skyPlane.Initialize(gameStateManager.Game.GraphicsDevice);

            // Initialize track
            track.Initialize(gameStateManager.Game.GraphicsDevice);
            CollisionDetection.setupTaggedSegments(track);

            // Initialize HUD
            HUD = new SpriteBatch(gameStateManager.GraphicsDevice);

            // Set camera position
            cameraPosition = new Vector3(50, 30f, 0f);

            // Create relevant matrices
            // PROJECTION
            float aspectRatio = gameStateManager.Game.GraphicsDevice.Viewport.AspectRatio;
            Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 0.1f, 10000.0f, out projection);
            //Matrix.CreateOrthographic(1280, 720, 0.1f, 10000.0f, out projection);

            // VIEW
            Matrix.CreateLookAt(ref cameraPosition, ref cameraTarget, ref cameraUpDirection, out view);

            // WORLD
            world = Matrix.Identity;

            this.Status = StateStatus.Active;
        }

        public void setCar(CarInfo carInfo, StateMenu menuState, Vector3 carStartingPosition)
        {
            playerCar.setCar(carInfo, carStartingPosition);
            this.menuState = menuState;
        }

        public void setTrack(TrackInfo trackInfo)
        {
            track.setTrack(trackInfo);
        }

        public override void LoadContent()
        {
            contentManager = new ContentManager(gameStateManager.Game.Services, "Content");

            // Load model content
            playerCar.LoadContent(gameStateManager);
            track.LoadContent(contentManager);
            skyPlane.LoadContent(contentManager);

            // Load HUD font
            font = contentManager.Load<SpriteFont>(@"HUDFont");

            // Prepare shadow mapping
            // Prepare shadow mapping parameters
            SurfaceFormat shadowMapFormat = SurfaceFormat.Single;
            const int shadowMapSize = 1024;

            // Create the shadow map render target
            shadowMapRenderTarget = new RenderTarget2D(GameStateManager.GraphicsDevice, shadowMapSize, shadowMapSize, true, shadowMapFormat, DepthFormat.Depth24);
        }

        public override void UnloadContent()
        {
            playerCar.UnloadContent();
            contentManager.Unload();
            Debug.WriteLine("Content unloaded.");
        }

        #endregion

        #region Draw and update

        private Matrix createLightViewProjectionMatrix()
        {
            Matrix lightView = Matrix.CreateLookAt(playerCar.Position + new Vector3(2.5f, 30f, 25f), playerCar.Position, Vector3.Up);

            Matrix lightProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1f, 0.1f, 8000.0f);

            //view = lightView;

            return lightView * lightProjection;

        }

        /// <summary>
        /// Draws content to screen
        /// </summary>
        public override void Draw(GameTime gameTime)
        {            
            // Update camera
            cameraPosition = playerCar.Position + (Vector3.Negate(playerCar.Facing) * 50.0f) + new Vector3(0, 20.0f, 0);
            cameraTarget = playerCar.Position;
            view = Matrix.CreateLookAt(cameraPosition, cameraTarget, cameraUpDirection);

            Matrix lightViewProjection = createLightViewProjectionMatrix();

            // PASS 1: CREATE SHADOW MAP
            // ===================================================================

            // Set render target
            GameStateManager.GraphicsDevice.SetRenderTarget(shadowMapRenderTarget);

            // Clear the device
            GameStateManager.GraphicsDevice.Clear(Color.White);

            // Draw objects that drop shadows
            playerCar.Draw(ref world, ref projection, ref view, "CreateShadowMap", ref lightViewProjection, ref shadowMap, ref lightDirection);

            // PASS 2: DRAW SCENE WITH SHADOWS
            // =====================================================================

            // Reset device render target
            GameStateManager.GraphicsDevice.SetRenderTarget(null);
            // Get shadow map
            shadowMap = shadowMapRenderTarget;

            // Clear screen
            GameStateManager.GraphicsDevice.Clear(Color.CornflowerBlue);

            // Draw skyplane
            skyPlane.Draw(ref world, ref projection, ref view, ref cameraPosition);

            // Draw landscape
            track.Draw(ref world, ref projection, ref view, "DrawWithShadowMap", ref lightViewProjection, ref shadowMap, ref lightDirection);

            // Draw player model
            playerCar.Draw(ref world, ref projection, ref view, "DrawWithoutShadowMap", ref lightViewProjection, ref shadowMap, ref lightDirection);

            // Draw HUD
            DrawHUD();

            base.Draw(gameTime);
        }

        private void DrawHUD()
        {
            if (raceTimeMillis < 0)
                return;

            DateTime time = new DateTime(raceTimeMillis * 1000 * 10);
            string timeString = time.ToString("mm:ss:ff");

            HUD.Begin();
            HUD.DrawString(font, timeString, new Vector2(21, 21), Color.Black);
            HUD.DrawString(font, timeString, new Vector2(20, 20), Color.White);
            HUD.End();

        }

        /// <summary>
        /// Updates parameters
        /// </summary>
        public override void Update(GameTime gameTime, bool otherStateHasFocus)
        {
            base.Update(gameTime, otherStateHasFocus);

            if (otherStateHasFocus)
            {
                return;
            }

            if (raceTimeMillis < 0)
                raceTimeMillis = 0;
            else if (updateRaceTime)
            {
                raceTimeMillis += gameTime.ElapsedGameTime.Milliseconds;
            }

            // Update player model
            playerCar.Update(gameTime, ref world);

            if (!updateRaceTime)
            {
                endCounter++;
            }

            // Check end of race
            if (CollisionDetection.LapDone())
            {
                updateRaceTime = false;
            }

            if (endCounter > 200)
            {
                gameStateManager.RemoveGameState(this);
                menuState.LastLapTime = new DateTime(raceTimeMillis * 1000 * 10);
                menuState.LoadContent();
                BloomComponent.Settings = new BloomSettings("Ble", 0.15f, 0, 0f, 1f, 1.0f, 1.0f);
                menuState.Status = StateStatus.Active;
            }
        }


        /// <summary>
        /// Handles input from player
        /// </summary>
        /// <param name="inputState"></param>
        public override void HandleInput(InputState inputState)
        {
            if (inputState.KeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
            {
                playerCar.WheelRight = true;
            }
            else
            {
                playerCar.WheelRight = false;
            }

            if (inputState.KeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
            {
                playerCar.WheelLeft = true;
            }
            else
            {
                playerCar.WheelLeft = false;
            }

            if (inputState.KeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up))
            {
                playerCar.Accelerating = true;
            }
            else
            {
                playerCar.Accelerating = false;
            }

            if (inputState.KeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down))
            {
                playerCar.Deccelerating = true;
            }
            else
            {
                playerCar.Deccelerating = false;
            }

            if (inputState.KeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                menuState.Status = StateStatus.Active;
                this.Status = StateStatus.Hidden;
                menuState.LoadContent();

                BloomComponent.Settings = new BloomSettings("Ble", 0.15f, 0, 0f, 1f, 1.0f, 1.0f);

                gameStateManager.RemoveGameState(this);
            }
        }

        #endregion

        #region Properties

        public Track RaceTrack
        {
            get
            {
                return this.track;
            }
        }

        #endregion
    }
}
