using neeksdk.Scripts.Infrastructure.Factory;
using neeksdk.Scripts.Infrastructure.StateMachine;
using neeksdk.Scripts.UI;

namespace neeksdk.Scripts.Infrastructure.SceneController
{
    public class GameController
    {
        public GameStateMachine StateMachine { get; }
        private BezierLineFactory _bezierLineFactory;

        public GameController() {
            StateMachine = new GameStateMachine(new MainMenuController());
            _bezierLineFactory = new BezierLineFactory();
        }
    }
}