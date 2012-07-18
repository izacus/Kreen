using System;
using Kreen.StateManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Kreen.MainGame;
using BloomPostprocess;

namespace Kreen.Menu
{
    class StateMenu : GameState
    {
        // TEMP TEMP TEMP TEMP
        private static CarInfo muscleCar = new CarInfo("muscle",
                                                       5.0f,
                                                       60.0f,
                                                       @"Models\muscle",
                                                       @"Textures\muscle body2",
                                                       @"Textures\spojler",
                                                       @"Textures\streek guma",
                                                       @"Textures\streek felna");
        private static CarInfo streekCar = new CarInfo("streek",
                                                       10.0f,
                                                       80.0f,
                                                       @"Models\streek",
                                                       @"Textures\streek body tex",
                                                       @"Textures\streek cockpit",
                                                       @"Textures\streek guma",
                                                       @"Textures\streek felna");

        private static TrackInfo trackInfo0 = new TrackInfo("track01",
                                                        @"Models\track01",
                                                        @"Textures\track01",
                                                        new Vector3(370, 50, -120));

        private static TrackInfo trackInfo1 = new TrackInfo("track02",
                                                        @"Models\track02",
                                                        @"Textures\track02",
                                                        new Vector3(340, 50, -380));

        private enum MenuOptions
        {
            Start,
            Car,
            Track,
            Exit
        }

        private GraphicsDevice graphicsDevice;
        private ContentManager contentManager;

        private MenuOptions selectedOption;

        private SpriteBatch menu;

        // Artwrok
        private Texture2D background;
        private Texture2D start;
        private Texture2D startSelected;
        private Texture2D car;
        private Texture2D carSelected;

        private Texture2D track;
        private Texture2D trackSelected;

        private Texture2D exit;
        private Texture2D exitSelected;
        private Texture2D yes;
        private Texture2D no;

        private Texture2D track0;
        private Texture2D track0Selected;

        private Texture2D track1;
        private Texture2D track1Selected;

        private Texture2D car0;
        private Texture2D car0Selected;
        private Texture2D car1;
        private Texture2D car1Selected;

        private Texture2D loading;

        private SpriteFont HUDFont;

        // Status variables
        private CarInfo currentlySelectedCar = muscleCar;
        private TrackInfo currentlySelectedTrack = trackInfo0;

        private Boolean isDownDown = false;
        private Boolean isUpDown = false;
        private Boolean isLeftDown = false;
        private Boolean isRightDown = false;
        private Boolean isEnterDown = false;

        private Boolean isLoading = false;
        private Boolean loadingDrawn = false;

        private Boolean exitSubmenu = false;
        private Boolean exitSubmenuYes = false;

        private DateTime lastLapTime = new DateTime(0L);

        public StateMenu(GameStateManager gameStateManager)
            : base(gameStateManager)
        {
            
        }

        public override void Initialize()
        {
            this.graphicsDevice = gameStateManager.GraphicsDevice;
            menu = new SpriteBatch(graphicsDevice);

            selectedOption = MenuOptions.Start;

            BloomComponent.Settings = new BloomSettings("Ble", 0.15f, 0, 0f, 1f, 1.0f, 1.0f);

            this.Status = StateStatus.Active;
        }

        public override void LoadContent()
        {
            contentManager = new ContentManager(gameStateManager.Game.Services, "Content");

            background = contentManager.Load<Texture2D>(@"Menu\bg_tex");

            // START
            start = contentManager.Load<Texture2D>(@"Menu\start");
            startSelected = contentManager.Load<Texture2D>(@"Menu\start--");

            // CAR SELECT
            car = contentManager.Load<Texture2D>(@"Menu\vehicle");
            carSelected = contentManager.Load<Texture2D>(@"Menu\vehicle--");

            // TRACK SELECT
            track = contentManager.Load<Texture2D>(@"Menu\track");
            trackSelected = contentManager.Load<Texture2D>(@"Menu\track--");

            // EXIT
            exit = contentManager.Load<Texture2D>(@"Menu\exit");
            exitSelected = contentManager.Load<Texture2D>(@"Menu\exit--");

            // MUSCLE CAR
            car0 = contentManager.Load<Texture2D>(@"Menu\car01");
            car0Selected = contentManager.Load<Texture2D>(@"Menu\car01--");

            // OTHER CAR
            car1 = contentManager.Load<Texture2D>(@"Menu\car02");
            car1Selected = contentManager.Load<Texture2D>(@"Menu\car02--");

            // TRACK
            track0Selected = contentManager.Load<Texture2D>(@"Menu\proga01--");
            track0 = contentManager.Load<Texture2D>(@"Menu\proga01");

            track1Selected = contentManager.Load<Texture2D>(@"Menu\proga02--");
            track1 = contentManager.Load<Texture2D>(@"Menu\proga02");

            // LOADING
            loading = contentManager.Load<Texture2D>(@"Menu\loading");

            // EXIT
            no = contentManager.Load<Texture2D>(@"Menu\yesno");
            yes = contentManager.Load<Texture2D>(@"Menu\yesno--");

            HUDFont = contentManager.Load<SpriteFont>("MenuFont");
        }

        public override void HandleInput(InputState inputState)
        {
            if (this.Status != StateStatus.Active)
                return;

            if (inputState.KeyState.IsKeyDown(Keys.Down) && !isDownDown)
            {
                SelectNextOption();
                isDownDown = true;
            }

            if (inputState.KeyState.IsKeyDown(Keys.Up) && !isUpDown)
            {
                SelectPreviousOption();
                isUpDown = true;
            }

            if (inputState.KeyState.IsKeyDown(Keys.Left) && !isLeftDown)
            {
                if (exitSubmenu)
                {
                    exitSubmenuYes = true;
                }
                else if (selectedOption == MenuOptions.Car)
                {
                    changeCar();
                }
                else if (selectedOption == MenuOptions.Track)
                {
                    changeTrack();
                }

                isLeftDown = true;
            }

            if (inputState.KeyState.IsKeyDown(Keys.Right) && !isRightDown)
            {
                if (exitSubmenu)
                {
                    exitSubmenuYes = false;
                }
                else if (selectedOption == MenuOptions.Car)
                {
                    changeCar();
                }
                else if (selectedOption == MenuOptions.Track)
                {
                    changeTrack();
                }

                isRightDown = true;
            }

            if (inputState.KeyState.IsKeyUp(Keys.Up))
                isUpDown = false;
            if (inputState.KeyState.IsKeyUp(Keys.Left))
                isLeftDown = false;
            if (inputState.KeyState.IsKeyUp(Keys.Right))
                isRightDown = false;
            if (inputState.KeyState.IsKeyUp(Keys.Down))
                isDownDown = false;
            if (inputState.KeyState.IsKeyUp(Keys.Enter))
                isEnterDown = false;

            if (inputState.KeyState.IsKeyDown(Keys.Enter) && !isEnterDown)
            {
                isEnterDown = true;
                switch (selectedOption)
                {
                    case MenuOptions.Start:
                        isLoading = true;
                        break;

                    case MenuOptions.Track:
                        changeTrack();
                        break;

                    case MenuOptions.Car:
                        changeCar();
                        break;

                    case MenuOptions.Exit:
                        if (exitSubmenuYes)
                            gameStateManager.Quit();
                        break;
                }
                 
            }
        }

        private void changeCar()
        {
            if (currentlySelectedCar.Name == "muscle")
                currentlySelectedCar = streekCar;
            else
                currentlySelectedCar = muscleCar;
        }

        private void changeTrack()
        {
            if (currentlySelectedTrack.Name == "track01")
                currentlySelectedTrack = trackInfo1;
            else
                currentlySelectedTrack = trackInfo0;
        }

        private void SelectNextOption()
        {
            switch (selectedOption)
            {
                case MenuOptions.Start:
                    selectedOption = MenuOptions.Car;
                    break;
                case MenuOptions.Car:
                    selectedOption = MenuOptions.Track;
                    break;
                case MenuOptions.Track:
                    selectedOption = MenuOptions.Exit;
                    exitSubmenu = true;
                    exitSubmenuYes = false;
                    break;
                case MenuOptions.Exit:
                    selectedOption = MenuOptions.Start;
                    exitSubmenu = false;
                    break;
            }

        }

        private void SelectPreviousOption()
        {

            switch (selectedOption)
            {
                case MenuOptions.Start:
                    selectedOption = MenuOptions.Exit;
                    exitSubmenu = true;
                    exitSubmenuYes = false;
                    break;
                case MenuOptions.Car:
                    selectedOption = MenuOptions.Start;
                    break;
                case MenuOptions.Track:
                    selectedOption = MenuOptions.Car;
                    break;
                case MenuOptions.Exit:
                    selectedOption = MenuOptions.Track;
                    exitSubmenu = false;
                    break;
            }
        }

        private void StartGame()
        {
            this.isLoading = true;
            this.Status = StateStatus.Hidden;
            StateGame stateGame = new StateGame(gameStateManager);

            stateGame.setCar(currentlySelectedCar, this, currentlySelectedTrack.StartingPosition);
            stateGame.setTrack(currentlySelectedTrack);

            gameStateManager.AddGameState(stateGame);

            contentManager.Unload();
        }

        public override void Update(GameTime gameTime, bool otherStateHasFocus)
        {
            if (otherStateHasFocus)
                return;

            if (isLoading && loadingDrawn)
            {
                BloomComponent.Settings =  new BloomSettings("Ble", 0.15f, 3f, 1.05f, 1.1f, 1.6f, 1.25f);

                StartGame();

                isLoading = false;
                loadingDrawn = false;
                selectedOption = MenuOptions.Start;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            graphicsDevice.Clear(Color.Black);

            menu.Begin();
            menu.Draw(background, new Rectangle(0, 0, 1280, 720), Color.White);

            // START
            menu.Draw(selectedOption == MenuOptions.Start ? startSelected : start, 
                      new Rectangle(100, 250, 581, 92), 
                      Color.White);

            menu.Draw(selectedOption == MenuOptions.Car ? carSelected : car, 
                      new Rectangle(100, 330, 581, 92), 
                      Color.White);

            menu.Draw(selectedOption == MenuOptions.Track ? trackSelected : track,
                      new Rectangle(100, 420, 581, 92),
                      Color.White);

            menu.Draw(selectedOption == MenuOptions.Exit ? exitSelected : exit, 
                      new Rectangle(100, 520, 581, 92), 
                      Color.White);

            menu.DrawString(HUDFont,
                            "Last lap: " + lastLapTime.ToString("mm:ss:ff"),
                            new Vector2(452, 62),
                            Color.DarkGray);

            menu.DrawString(HUDFont,
                            "Last lap: " + lastLapTime.ToString("mm:ss:ff"),
                            new Vector2(450, 60),
                            Color.Black);

            if (selectedOption == MenuOptions.Track)
            {
                menu.Draw(currentlySelectedTrack.Name == "track01" ? track0Selected : track0,
                          new Rectangle(660, 315, 334, 219),
                          Color.White);

                menu.Draw(currentlySelectedTrack.Name != "track01" ? track1Selected : track1,
                          new Rectangle(905, 325, 334, 219),
                          Color.White);
            }

            if (selectedOption == MenuOptions.Car)
            {
                menu.Draw(currentlySelectedCar.Name == "muscle" ? car0Selected : car0,
                          new Rectangle(705, 285, 219, 183),
                          Color.White);

                menu.Draw(currentlySelectedCar.Name != "muscle" ? car1Selected : car1,
                          new Rectangle(915, 290, 219, 183),
                          Color.White);
            }

            if (isLoading)
            {
                menu.Draw(loading,
                          new Rectangle(500, 550, 297, 110),
                          Color.White);

                loadingDrawn = true;
            }

            if (exitSubmenu)
            {
                menu.Draw(exitSubmenuYes ? yes : no,
                          new Rectangle(770, 515, 357, 115),
                          Color.White);
            }

            menu.End();

            base.Draw(gameTime);
        }

        public int getCenter(string text, SpriteFont font, int x)
        {
            Vector2 result = font.MeasureString(text);

            return x - (int)Math.Round(result.X / 2f);
        }

        public DateTime LastLapTime
        {
            get { return lastLapTime; }
            set { lastLapTime = value; }
        }
    }
}
