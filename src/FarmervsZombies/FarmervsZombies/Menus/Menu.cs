namespace FarmervsZombies.Menus
{
    internal class Menu
    {
        protected int mScreenWidth;
        protected int mScreenHeight;

        protected const int ButtonDistance = 125;

        public enum GameState
        {
            MainMenu,
            OptionsMenu,
            LevelMenu,
            PlayGameMenu,
            StartGameMenu,
            StatisticsMenu,
            PauseMenu,
            LoadGame,
            GameOver,
            GameWon,
            AchievementsMenu,
            ResolutionMenu
        }
    }
}
