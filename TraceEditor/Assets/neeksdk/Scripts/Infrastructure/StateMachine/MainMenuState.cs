using neeksdk.Scripts.UI;

namespace neeksdk.Scripts.Infrastructure.StateMachine
{
    public class MainMenuState : IState
    {
        private readonly MainMenuController _mainMenuController;
        private readonly GameStateMachine _stateMachine;

        private const string TEST_SCENE_NAME = "TestScene";
        
        public MainMenuState(MainMenuController mainMenuController, GameStateMachine stateMachine)
        {
            _mainMenuController = mainMenuController;
            _stateMachine = stateMachine;
        }
        
        public void Enter()
        {
            _mainMenuController.SetupMainMenu();
            LevelButton.OnLevelClicked += LevelClicked;
        }

        private void LevelClicked(string path)
        {
            LevelButton.OnLevelClicked -= LevelClicked;
            _stateMachine.Enter<LoadLevelState, string, string>(TEST_SCENE_NAME, path);
        }

        public void Exit()
        {
            _mainMenuController.ClearMainMenu();
        }
    }
}