using System;
using System.Collections.Generic;
using System.Linq;
using FarmervsZombies.AI;
using FarmervsZombies.GameObjects;
using FarmervsZombies.Managers;
using FarmervsZombies.Menus;
using FarmervsZombies.Pathfinding.GameGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    internal sealed class Game1 : Game
    {
        private readonly GraphicsDeviceManager mGraphics;
        private SpriteBatch mSpriteBatch;

        public static readonly TileMap sTileMap = new TileMap(MapHeight, MapWidth);

        // Random spawn points
        private static readonly Random sRandom = new Random();

        // Menus
        private readonly MainMenu mMain;
        public static readonly OptionsMenu sOptions = new OptionsMenu();
        private readonly StartGameMenu mStartGame;
        private readonly StatisticsMenu mStatistics;
        private readonly AchievementsMenu mAchievements;
        private readonly LevelMenu mLevelMenu;
        public static readonly PauseMenu sPause = new PauseMenu();
        private readonly HudMenu mHudMenu;
        private readonly ResolutionMenu mResolutionMenu = new ResolutionMenu();

        private static readonly LoadGameMenu sLoadGame = new LoadGameMenu();

        // this is the enum for choosing in which state the Game is

        // fog of war
        public static readonly FogOfWar sFog = new FogOfWar(MapWidth, MapHeight);
        private bool mShowFog = true;

        // Camera
        private readonly Camera mCamera;
        private readonly Viewport mViewport;

        // EconomyManager
        public static readonly Statistics sStatistics = new Statistics();
        public static readonly Achievements sAchievements = new Achievements();

        // SelectionManager
        public static readonly SelectionManager sSelection = new SelectionManager(sTileMap, sFog);

        // in game Time
        public static float mTime;

        // Map mode load
        private readonly bool mLoadMap;

        public const int MapWidth = 100;
        public const int MapHeight = 100;
        public static bool FarmerDied { private get; set; }
        public static bool AiDied { private get; set; }
        public static bool GraveyardBuilt { private get; set; }
        private static bool sTechDemoActive;
        private static bool sEndScreen;

        public static int Difficulty { get; set; }

        private static readonly List<float> sFrameLengthsOfLastSecond = new List<float>();
        public static int FramesPerSecond => sFrameLengthsOfLastSecond.Count;

        private static bool sResolutionChanged;
        public static (int, int) Resolution { get; private set; }

        public Game1()
        {
            mGraphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            GameGraph.Build(sTileMap.Width, sTileMap.Height);
            mGraphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            mGraphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            mGraphics.ToggleFullScreen();

            mLoadMap = false;

            // Menus
            mMain = new MainMenu();
            mLevelMenu = new LevelMenu();
            mStartGame = new StartGameMenu();
            mStatistics = new StatisticsMenu();
            mAchievements = new AchievementsMenu();
            mHudMenu = new HudMenu();

            // Camera
            mViewport = GraphicsDevice.Viewport;
            mCamera = new Camera(mViewport);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            IsFixedTimeStep = false;
            // TextureManager
            TextureManager.Initialize(mGraphics.GraphicsDevice);

            // Everyone should execute the project once without loading because the amount of tiles changed
            if (!mLoadMap)
            {
                sTileMap.Build();
            }
            else
            {
                sTileMap.Load("gameState");
            }

            IsMouseVisible = false;

            // Animations
            Animation.Initialize();

            InputManager.Initialize();
            InputManager.AnyActionEvent += ToggleFullScreen;
            InputManager.EscPressed += PauseGame;

            InputManager.AnyActionEvent += GenChicken;
            InputManager.AnyActionEvent += GenCow;
            InputManager.AnyActionEvent += GenPig;
            InputManager.AnyActionEvent += GenAChicken;
            InputManager.AnyActionEvent += GenACow;
            InputManager.AnyActionEvent += GenAPig;
            InputManager.AnyActionEvent += SpawnNecromancer;
            InputManager.AnyActionEvent += ExitEndScreen;
            InputManager.AnyActionEvent += sSelection.MouseClick;
            InputManager.AnyActionEvent += sSelection.ClearBox;

            InputManager.AnyActionEvent += ObjectManager.Instance.ToggleQuadTreeDraw;
            InputManager.AnyActionEvent += ToggleFogOfWar;
            InputManager.AnyActionEvent += DoGeneratePerformanceDemo;
            InputManager.AnyActionEvent += mHudMenu.ToggleFps;

            // The initial state is MainMenu
            GameStateManager.State = Menu.GameState.MainMenu;

            // Menus
            mMain.Initialize(mGraphics.GraphicsDevice);
            sOptions.Initialize(mGraphics.GraphicsDevice);
            mStartGame.Initialize(mGraphics.GraphicsDevice);
            mStatistics.Initialize(mGraphics.GraphicsDevice);
            mAchievements.Initialize(mGraphics.GraphicsDevice);
            mLevelMenu.Initialize(mGraphics.GraphicsDevice);
            sPause.Initialize(mGraphics.GraphicsDevice);
            sLoadGame.Initialize(mGraphics.GraphicsDevice);
            mResolutionMenu.Initialize(mGraphics.GraphicsDevice);

            sSelection.Initialize();

            // FQM
            EconomyManager.Instance.Initialize();
            // Fixing a bug
            FarmerQueueManager.Instance.EmptyFQueue();

            NotificationManager.Initialize(mGraphics.GraphicsDevice);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            mSpriteBatch = new SpriteBatch(GraphicsDevice);

            // TextureManager
            TextureManager.LoadContent(Content);

            // TileMap
            sTileMap.LoadContent();

            // MainMenu
            mMain.LoadContent(Content);
            mStatistics.LoadContent(Content);
            mAchievements.LoadContent(Content);
            sOptions.LoadContent(Content);
            mLevelMenu.LoadContent(Content);
            mStartGame.LoadContent(Content);
            sPause.LoadContent(Content);
            sLoadGame.LoadContent(Content);
            mHudMenu.LoadContent(Content);
            mResolutionMenu.LoadContent(Content);
            // SoundManager
            SoundManager.LoadContent(Content);
            ToggleMusicOn();
            // fog of war
            sFog.LoadContent(Content);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            switch (GameStateManager.State)
            {
                case Menu.GameState.PlayGameMenu:
                    if (!GraveyardBuilt)
                    {
                        var spawncase = sRandom.Next(3);
                        Vector2 spawnpoint;
                        switch (spawncase)
                        {
                            case 1:
                                spawnpoint = new Vector2(38, 90);
                                break;
                            case 2:
                                spawnpoint = new Vector2(12, 76);
                                break;
                            default:
                                spawnpoint = new Vector2(78, 90);
                                break;

                        }
                        Ai.BuildGraveyard(spawnpoint);
                        GraveyardBuilt = true;
                        sTechDemoActive = false;

                        SpawnGraveyardTreasures();
                    }
                    
                    ObjectManager.Instance.Update(gameTime);
                    if (!sTechDemoActive) Ai.Update(gameTime);
                    sTileMap.Update(gameTime);

                    // Update Camera
                    mCamera.UpdateCamera(mViewport, InputManager.GetCurrentInputState());
                    // Update Economy
                    EconomyManager.Instance.UpdateEconomy();
                    mHudMenu.Update();

                    FarmerQueueManager.Instance.Update(gameTime);

                    NotificationManager.Update(gameTime);

                    // Update SelectionManager
                    sSelection.Update(InputManager.GetCurrentInputState(), GameStateManager.State);

                    // fog of war update only if farmer is existing
                    var farmer = ObjectManager.Instance.GetFarmer();
                    if (farmer == null && !FarmerDied)
                    {
                        var spawncase = sRandom.Next(3);
                        Vector2 spawnpoint;
                        switch (spawncase)
                        {
                            case 0:
                            {
                                spawnpoint = new Vector2(8, 12);
                                break;
                            }

                            case 1:
                            {
                                spawnpoint = new Vector2(85, 12);
                                break;
                            }

                            default:
                            {
                                spawnpoint = new Vector2(52, 10);
                                break;
                            }
                        }
                        ObjectManager.Instance.Add(new Farmer((int)spawnpoint.X, (int)spawnpoint.Y));

                        sSelection.mTileMenu.FenceBuildMode = false;
                        ObjectManager.Instance.ClearPreviewFences();

                        // Follow Farmer
                        mCamera.FollowFarmer = true;
                    }
                    else if (farmer != null)
                    {
                        sFog.Update(farmer.Position);
                    }
                    mTime += (float)gameTime.ElapsedGameTime.Milliseconds / 1000;
                    if (sStatistics.OldGameTime + mTime >= 300 && !sAchievements.DasWarErstDerAnfang)
                    {
                        sAchievements.DasWarErstDerAnfang = true;
                        NotificationManager.AddNotification("Achievement freigeschaltet!", "Du hast das Achievement \"Das war erst der Anfang\" erreicht.", 6.0f);
                    } else if (sStatistics.OldGameTime + mTime >= 3000 && !sAchievements.DieLangeNacht)
                    {
                        sAchievements.DieLangeNacht = true;
                        NotificationManager.AddNotification("Achievement freigeschaltet!", "Du hast das Achievement \"Die Apokalypse lange Nacht\" erreicht.", 6.0f);
                    }
                    sStatistics.Update();

                    if (!SoundManager.MusicOn)
                    {
                        ToggleMusicOff();    
                    }
                    if (SoundManager.MusicOn)
                    {
                        ToggleMusicOn(); 
                    }

                    break;

                case Menu.GameState.MainMenu:
                    mMain.Update();
                    break;

                case Menu.GameState.LevelMenu:
                    mLevelMenu.Update();
                    break;

                case Menu.GameState.OptionsMenu:
                    sOptions.Update();
                    break;

                case Menu.GameState.StartGameMenu:
                    mStartGame.Update();
                    break;

                case Menu.GameState.StatisticsMenu:
                    mStatistics.Update();
                    break;

                case Menu.GameState.PauseMenu:
                    sPause.Update();
                    break;

                case Menu.GameState.ResolutionMenu:
                    mResolutionMenu.Update();
                    break;

                case Menu.GameState.LoadGame:
                    sLoadGame.Update(Content);
                    break;

                case Menu.GameState.GameOver:
                    ObjectManager.Instance.Update(gameTime);
                    break;
            }

            // Update managers
            SoundManager.Update(gameTime);
            InputManager.Update(IsActive);   
            AnimationManager.Update(gameTime);
            GameStateManager.Update();

            if (sResolutionChanged)
            {
                mGraphics.PreferredBackBufferWidth = Resolution.Item1;
                mGraphics.PreferredBackBufferHeight = Resolution.Item2;
                mGraphics.ApplyChanges();
                sResolutionChanged = false;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            UpdateFramesCounter(gameTime);
            GraphicsDevice.Clear(Color.TransparentBlack);

            switch (GameStateManager.State)
            {
                case Menu.GameState.PlayGameMenu:
                    DrawGame();
                    break;

                case Menu.GameState.MainMenu:
                    mMain.Draw(mSpriteBatch);
                    break;

                case Menu.GameState.LevelMenu:
                    mLevelMenu.Draw(mSpriteBatch);
                    break;

                case Menu.GameState.OptionsMenu:
                    sOptions.Draw(mSpriteBatch);
                    break;

                case Menu.GameState.StartGameMenu:
                    mStartGame.Draw(mSpriteBatch);
                    break;

                case Menu.GameState.StatisticsMenu:
                    mStatistics.Draw(mSpriteBatch);
                    break;

                case Menu.GameState.AchievementsMenu:
                    mAchievements.Draw(mSpriteBatch);
                    break;

                case Menu.GameState.PauseMenu:
                    DrawGame();
                    sPause.Draw(mSpriteBatch);
                    break;

                case Menu.GameState.LoadGame:
                    sLoadGame.Draw(mSpriteBatch);
                    break;

                case Menu.GameState.ResolutionMenu:
                    mResolutionMenu.Draw(mSpriteBatch);
                    break;

                case Menu.GameState.GameOver:
                    DrawGame();
                    GameEnd(false);
                    break;

                case Menu.GameState.GameWon:
                    GameEnd(true);
                    break;
            }

            // Draw cursor
            var cursor = TextureManager.GetTexture("cursor");
            var cursorPosition = InputManager.GetCurrentInputState().mMouseWindowPosition;
            mSpriteBatch.Begin(SpriteSortMode.FrontToBack, null, SamplerState.PointClamp, null, null, null, null);
            mSpriteBatch.Draw(cursor, new Rectangle((int)cursorPosition.X, (int)cursorPosition.Y, cursor.Width, cursor.Height), Color.White);
            mSpriteBatch.End();
            base.Draw(gameTime);
        }

        private void DrawGame()
        {
            // Render the map
            sTileMap.Draw(mSpriteBatch, Camera.Transform);
            ObjectManager.Instance.Draw(mSpriteBatch, Camera.Transform, mCamera.VisibleArea);

            // only draw the fog when farmer is existing
            if (ObjectManager.Instance.GetFarmer() != null && mShowFog)
            {
                sFog.Draw(mSpriteBatch, Camera.Transform);
            }

            sSelection.Draw(mSpriteBatch, Camera.Transform);

            mHudMenu.Draw(mSpriteBatch);
            NotificationManager.Draw(mSpriteBatch);

            // Game Over Screen
            if (FarmerDied)
            {
                GameStateManager.State = Menu.GameState.GameOver;
            }

            if (AiDied)
            {
                GameStateManager.State = Menu.GameState.GameWon;
            }
        }

        private static void SpawnGraveyardTreasures()
        {
            for (var i = 0; i < 2 * Difficulty; i++)
            {
                while (true)
                {
                    var (item1, item2) = (sRandom.Next(MapWidth), sRandom.Next(25, MapHeight));
                    var free = true;
                    for (var x = item1 - 1; x < item1 + 2; x++)
                    {
                        for (var y = item2; y < item2 + 2; y++)
                        {
                            if (GameGraph.Graph.CheckCollision(x, y))
                            {
                                free = false;
                            }
                        }
                    }

                    if (!free) continue;
                    ObjectManager.Instance.Add(new GraveyardTreasure(item1, item2));
                    break;
                }
            }
        }

        private void ToggleFullScreen(object sender, InputState inputState)
        {
            if (!inputState.IsActionActive(ActionType.ToggleFullscreen)) return;
            mGraphics.ToggleFullScreen();
        }

        public static void SetResolution(int width, int height)
        {
            Resolution = (width, height);
            sResolutionChanged = true;
        }

        private void ToggleFogOfWar(object sender, InputState inputState)
        {
            if (!inputState.IsActionActive(ActionType.ToggleFogOfWar)) return; 
            mShowFog = !mShowFog;
        }

        private void ToggleMusicOn()
        {
            if (GameStateManager.State == Menu.GameState.PlayGameMenu)
            {
                if (ObjectManager.Instance.GetFarmer() != null && ObjectManager.Instance.TargetsInRange(ObjectManager.Instance.GetFarmer().Position, 10).Count() > 6)
                {
                    SoundManager.PlaySong("fight_music", .5f);
                }
                else if (ObjectManager.Instance.GetFarmer() != null && ObjectManager.Instance.TargetsInRange(ObjectManager.Instance.GetFarmer().Position, 10).Count() < 6)
                {
                    SoundManager.PlaySong("volume_alpha", .65f);
                }
            }
            else
            {
                SoundManager.PlaySong("volume_alpha", .65f);
            }
                    
            //else if(ObjectManager.Instance.GetFarmer() != null && ObjectManager.Instance.TargetsInRange(ObjectManager.Instance.GetFarmer().Position, 10).Exists(x => x.Name = "necromancer")
            //            .Contains("Necromancer")
            //{
                
            //}
        }

        private void ToggleMusicOff()
        {
            SoundManager.StopSong();
        }

        private static void GenChicken(object sender, InputState inputState)
        {
            if (!inputState.IsActionActive(ActionType.GenChicken)) return;
            if (GameStateManager.State != Menu.GameState.PlayGameMenu) return;
            var farmer = ObjectManager.Instance.GetFarmer();
            if (sTileMap.GetTileType((int)farmer.Position.X + 2, (int)farmer.Position.Y + 2) == Tile.Grass)
            {
                var chicken = new Chicken(farmer.Position.X + 2, farmer.Position.Y + 2);
                ObjectManager.Instance.Add(chicken);
            }
        }

        private static void GenCow(object sender, InputState inputState)
        {
            if (!inputState.IsActionActive(ActionType.GenCow)) return;
            if (GameStateManager.State != Menu.GameState.PlayGameMenu) return;
            var farmer = ObjectManager.Instance.GetFarmer();
            if (sTileMap.GetTileType((int)farmer.Position.X + 1, (int)farmer.Position.Y + 3) == Tile.Grass)
            {
                var cow = new Cow(farmer.Position.X + 1, farmer.Position.Y + 3);
                ObjectManager.Instance.Add(cow);
            }
        }

        private static void GenPig(object sender, InputState inputState)
        {
            if (!inputState.IsActionActive(ActionType.GenPig)) return;
            if (GameStateManager.State != Menu.GameState.PlayGameMenu) return;
            var farmer = ObjectManager.Instance.GetFarmer();
            if (sTileMap.GetTileType((int)farmer.Position.X + 3, (int)farmer.Position.Y + 1) == Tile.Grass)
            {
                var pig = new Pig(farmer.Position.X + 3, farmer.Position.Y + 1);
                ObjectManager.Instance.Add(pig);
            }
        }

        private static void GenAPig(object sender, InputState inputState)
        {
            if (!inputState.IsActionActive(ActionType.GenAPig)) return;
            if (GameStateManager.State != Menu.GameState.PlayGameMenu) return;
            var farmer = ObjectManager.Instance.GetFarmer();
            if (sTileMap.GetTileType((int)farmer.Position.X + 4, (int)farmer.Position.Y + 1) == Tile.Grass)
            {
                var attackpig = new AttackPig(farmer.Position.X + 4, farmer.Position.Y + 1);
                ObjectManager.Instance.Add(attackpig);
            }
        }

        private static void GenACow(object sender, InputState inputState)
        {
            if (!inputState.IsActionActive(ActionType.GenACow)) return;
            if (GameStateManager.State != Menu.GameState.PlayGameMenu) return;
            var farmer = ObjectManager.Instance.GetFarmer();
            if (sTileMap.GetTileType((int)farmer.Position.X + 1, (int)farmer.Position.Y + 4) == Tile.Grass)
            {
                var attackcow = new AttackCow(farmer.Position.X + 1, farmer.Position.Y + 4);
                ObjectManager.Instance.Add(attackcow);
            }
        }

        private static void GenAChicken(object sender, InputState inputState)
        {
            if (!inputState.IsActionActive(ActionType.GenAChicken)) return;
            if (GameStateManager.State != Menu.GameState.PlayGameMenu) return;
            var farmer = ObjectManager.Instance.GetFarmer();
            if (sTileMap.GetTileType((int)farmer.Position.X + 3, (int)farmer.Position.Y + 3) == Tile.Grass)
            {
                var attackchicken = new AttackChicken(farmer.Position.X + 3, farmer.Position.Y + 3);
                ObjectManager.Instance.Add(attackchicken);
            }
        }

        private static void SpawnNecromancer(object sender, InputState inputState)
        {
            if (!inputState.IsActionActive(ActionType.SpawnNecromancer)) return;
            if (GameStateManager.State != Menu.GameState.PlayGameMenu) return;
            var necromancer = new Necromancer(10, 10);
            ObjectManager.Instance.Add(necromancer);
        }

        private static void DoGeneratePerformanceDemo(object sender, InputState inputState)
        {
            if (!inputState.IsActionActive(ActionType.GeneratePerformanceDemo)) return;
            GeneratePerformanceDemo();
        }

        public static void GeneratePerformanceDemo()
        {
            for (var i = 0; i < MapWidth; i++)
            {
                for (var j = 0; j < MapHeight; j++)
                {
                    sTileMap.PlantGrass(i, j, true);
                }
            }
            GameStateManager.State = Menu.GameState.PlayGameMenu;
            FarmerQueueManager.Instance.EmptyFQueue();
            FarmerDied = false;
            GraveyardBuilt = true;
            AiDied = false;
            sTechDemoActive = true;
            sFog.Reset();
            for (var i = 0; i < 10; i++)
            {
                for (var j = 0; j < 10; j++)
                {
                    for (var k = 0; k < 5; k++)
                    {
                        ObjectManager.Instance.Add(new AttackCow(5 + i + 20 * k, 5 + j));
                        ObjectManager.Instance.Add(new AttackChicken(5 + i + 20 * k, 85 + j, team: false));
                    }
                }
            }
        }

        private static void UpdateFramesCounter(GameTime gameTime)
        {
            var sumOfFrameTime =
                (float) gameTime.ElapsedGameTime.TotalMilliseconds / 1000 + sFrameLengthsOfLastSecond.Sum();
            while (sumOfFrameTime > 1)
            {
                sumOfFrameTime -= sFrameLengthsOfLastSecond.First();
                sFrameLengthsOfLastSecond.RemoveAt(0);
            }
            sFrameLengthsOfLastSecond.Add((float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000);
        }

        private void PauseGame(object sender, InputState inputState)
        {
            ToggleMusicOff();
            switch (GameStateManager.State)
            {
                case Menu.GameState.PlayGameMenu:
                    GameStateManager.State = Menu.GameState.PauseMenu;
                    break;
                case Menu.GameState.PauseMenu:
                    if (sPause.ShowTextBox)
                    {
                        sPause.ShowTextBox = !sPause.ShowTextBox;
                    }
                    else
                    {
                        GameStateManager.State = Menu.GameState.PlayGameMenu;
                        ToggleMusicOn();
                    }
                    break;
                case Menu.GameState.MainMenu:
                    break;
                case Menu.GameState.OptionsMenu:
                    break;
                case Menu.GameState.LevelMenu:
                    break;
                case Menu.GameState.StartGameMenu:
                    break;
                case Menu.GameState.StatisticsMenu:
                    break;
                case Menu.GameState.AchievementsMenu:
                    break;
                case Menu.GameState.LoadGame:
                    break;
                case Menu.GameState.ResolutionMenu:
                    break;
                case Menu.GameState.GameOver:
                    break;
                case Menu.GameState.GameWon:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void GameEnd(bool win)
        {
            if (!sEndScreen) SetupEndScreen();
            var over = win ? Content.Load<Texture2D>("Menus/WinScreen") : TextureManager.GetTexture("game_over");
            mSpriteBatch.Begin();
            if (win)
            {
                mSpriteBatch.Draw(over, new Rectangle(0, 0,
                    GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
            }
            else
            {
                mSpriteBatch.Draw(over, new Vector2(GraphicsDevice.Viewport.Width / 2f - over.Width / 2f, GraphicsDevice.Viewport.Height / 8f), Color.White);

                mCamera.UpdateCamera(mViewport, InputManager.GetCurrentInputState());
            }
            mSpriteBatch.End();
        }

        private void SetupEndScreen()
        {
            sEndScreen = true;
            mHudMenu.HudVisible = false;
            mCamera.SetEndScreenPosition();
            mCamera.Lock();
            SoundManager.StopSounds();
            SoundManager.PlaySong("game_over");

            ObjectManager.Instance.EndScreen = true;

            ObjectManager.Instance.Add(new MoonwalkZombie(20 + 9, 18 + 14));
            ObjectManager.Instance.Add(new MoonwalkZombie(25 + 9, 18 + 14));
            ObjectManager.Instance.Add(new MoonwalkZombie(30 + 9, 18 + 14));
            ObjectManager.Instance.Add(new MoonwalkZombie(35 + 9, 18 + 14));
            ObjectManager.Instance.Add(new MoonwalkZombie(40 + 9, 18 + 14));
            ObjectManager.Instance.Add(new MoonwalkZombie(20 + 9, 20 + 14));
            ObjectManager.Instance.Add(new MoonwalkZombie(25 + 9, 20 + 14));
            ObjectManager.Instance.Add(new MoonwalkZombie(30 + 9, 20 + 14, true));
            ObjectManager.Instance.Add(new MoonwalkZombie(35 + 9, 20 + 14));
            ObjectManager.Instance.Add(new MoonwalkZombie(40 + 9, 20 + 14));
            ObjectManager.Instance.Add(new MoonwalkZombie(20 + 9, 22 + 14));
            ObjectManager.Instance.Add(new MoonwalkZombie(25 + 9, 22 + 14));
            ObjectManager.Instance.Add(new MoonwalkZombie(30 + 9, 22 + 14));
            ObjectManager.Instance.Add(new MoonwalkZombie(35 + 9, 22 + 14));
            ObjectManager.Instance.Add(new MoonwalkZombie(40 + 9, 22 + 14));
        }

        private void ExitEndScreen(object sender, InputState inputState)
        {
            if (!inputState.IsActionActive(ActionType.ExitEndScreen) || !sEndScreen) return;
            sEndScreen = false;
            mCamera.Unlock();
            mHudMenu.HudVisible = true;
            ObjectManager.Instance.ClearMoonwalkers();
            ObjectManager.Instance.EndScreen = false;
            SoundManager.StopSong();
            GameStateManager.State = Menu.GameState.MainMenu;
            sStatistics.Save();
            sAchievements.Save();
            ObjectManager.Instance.UnloadAll();
        }
    }
}
