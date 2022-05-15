using neeksdk.Scripts.Infrastructure.SceneController;

namespace neeksdk.Scripts.Infrastructure.StateMachine
{
    public class LoadLevelState : IPayloadedState<string, string>
    {
        private readonly SceneLoader _sceneLoader;
        private readonly GameController _gameController;
        private readonly GameStateMachine _stateMachine;

        private string _figurePath;

        public LoadLevelState(SceneLoader sceneLoader, GameController gameController, GameStateMachine stateMachine)
        {
            _sceneLoader = sceneLoader;
            _gameController = gameController;
            _stateMachine = stateMachine;
        }

        public void Enter(string sceneName, string figurePath)
        {
            _figurePath = figurePath;
            _sceneLoader.Load(sceneName, OnLoaded);
        }

        private void OnLoaded()
        {
            _gameController.WarmUp(_figurePath).Then(() =>
            {
                _stateMachine.Enter<GameState>();
            });
        }

        public void Exit()
        {
            
        }
    }
}