using neeksdk.Scripts.Infrastructure.StateMachine;

namespace neeksdk.Scripts.Infrastructure.SceneController
{
    public class GameController
    {
        public GameStateMachine StateMachine;

        public GameController() {
            StateMachine = new GameStateMachine();
        }
    }
}