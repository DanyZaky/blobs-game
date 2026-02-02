using Blobs.Interfaces;
using Blobs.Services;

namespace Blobs.Presenters
{
    /// <summary>
    /// Service Locator pattern to replace singletons.
    /// Provides centralized access to presenters and services without tight coupling.
    /// SOLID: Dependency Inversion - depend on interfaces, not implementations.
    /// </summary>
    public static class ServiceLocator
    {
        // Presenters
        private static IGamePresenter _game;
        private static IGridPresenter _grid;
        
        // Services (SRP-compliant)
        private static IInputService _input;
        private static ISelectionService _selection;
        private static IMoveService _move;
        private static IFeedbackService _feedback;

        // Presenter accessors
        public static IGamePresenter Game => _game;
        public static IGridPresenter Grid => _grid;
        
        // Service accessors
        public static IInputService Input => _input;
        public static ISelectionService Selection => _selection;
        public static IMoveService Move => _move;
        public static IFeedbackService Feedback => _feedback;

        // Presenter registration
        public static void RegisterGame(IGamePresenter game)
        {
            _game = game;
        }

        public static void RegisterGrid(IGridPresenter grid)
        {
            _grid = grid;
        }
        
        // Service registration
        public static void RegisterInput(IInputService input)
        {
            _input = input;
        }
        
        public static void RegisterSelection(ISelectionService selection)
        {
            _selection = selection;
        }
        
        public static void RegisterMove(IMoveService move)
        {
            _move = move;
        }
        
        public static void RegisterFeedback(IFeedbackService feedback)
        {
            _feedback = feedback;
        }

        public static void Clear()
        {
            _game = null;
            _grid = null;
            _input = null;
            _selection = null;
            _move = null;
            _feedback = null;
        }

        public static bool IsInitialized => _game != null && _grid != null;
        
        public static bool AreServicesInitialized => 
            _input != null && _selection != null && _move != null && _feedback != null;
    }
}
