namespace neeksdk.Scripts.Infrastructure.StateMachine
{
    public class LoadLevelState : IPayloadedState<string>
    {
        public LoadLevelState()
        {
            
        }

        public void Enter(string sceneName)
        {
            
        }

        public void Exit()
        {
            
        }

        private void OnLoaded()
        {
            InitGameWorld();
        }

        private void InitGameWorld()
        {
            
        }
    }
}