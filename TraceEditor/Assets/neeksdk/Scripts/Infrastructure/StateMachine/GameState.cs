using neeksdk.Scripts.Infrastructure.SceneController;

namespace neeksdk.Scripts.Infrastructure.StateMachine
{
    public class GameState : IState
    {
        private readonly GameController _gameController;
        private readonly SceneLoader _sceneLoader;
        private readonly GameStateMachine _stateMachine;

        private const string MAIN_MENU_SCENE_NAME = "MainMenuScene";
        
        public GameState(GameController gameController, SceneLoader sceneLoader, GameStateMachine stateMachine)
        {
            _gameController = gameController;
            _sceneLoader = sceneLoader;
            _stateMachine = stateMachine;
        }
        
        public void Enter()
        {
            _gameController.StartDrawFigure();
            _gameController.OnGoToMainMenu += GoToMainMenu;
        }

        private void GoToMainMenu() =>
            _sceneLoader.Load(MAIN_MENU_SCENE_NAME, () => _stateMachine.Enter<MainMenuState>());

        public void Exit()
        {
            
        }
    }
}