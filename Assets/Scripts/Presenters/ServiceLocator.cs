using Blobs.Interfaces;

namespace Blobs.Presenters
{
    /// <summary>
    /// Service Locator pattern to replace singletons.
    /// Provides centralized access to presenters without tight coupling.
    /// SOLID: Dependency Inversion - depend on interfaces, not implementations.
    /// </summary>
    public static class ServiceLocator
    {
        private static IGamePresenter _game;
        private static IGridPresenter _grid;

        public static IGamePresenter Game => _game;
        public static IGridPresenter Grid => _grid;

        public static void RegisterGame(IGamePresenter game)
        {
            _game = game;
        }

        public static void RegisterGrid(IGridPresenter grid)
        {
            _grid = grid;
        }

        public static void Clear()
        {
            _game = null;
            _grid = null;
        }

        public static bool IsInitialized => _game != null && _grid != null;
    }
}
