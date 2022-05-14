using neeksdk.Scripts.Infrastructure.SceneController;
using neeksdk.Scripts.Infrastructure.StateMachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace neeksdk.Scripts.Infrastructure
{
    public class GameBootstrapper : MonoBehaviour
    {
        private GameController _game;

        private void Awake()
        {
            _game = new GameController();
            //_game.StateMachine.Enter<LoadLevelState>();

            DontDestroyOnLoad(this);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            SceneManager.LoadScene(0);
        }
    }
}