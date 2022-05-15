using neeksdk.Scripts.StaticData.LevelData;
using neeksdk.Scripts.UI;
using UnityEngine;

namespace neeksdk.Scripts.Infrastructure.StateMachine
{
    public class MainMenuState : IState
    {
        private readonly MainMenuController _mainMenuController;
        private readonly GameStateMachine _stateMachine;

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
            _stateMachine.Enter<LoadLevelState, string>(path);
        }

        public void Exit()
        {
            _mainMenuController.ClearMainMenu();
        }
    }
}