using neeksdk.Scripts.Infrastructure.SceneController;
using neeksdk.Scripts.Infrastructure.StateMachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace neeksdk.Scripts.Infrastructure
{
    public class GameBootstrapper : MonoBehaviour, ICoroutineRunner
    {
        private GameController _game;
        public static GameBootstrapper Instance = null;

        private void Start()
        {
            if (Instance == this)
            {
                Destroy(gameObject);
                return;
            }

            if (Instance == null)
            {
                InitializeBootstrapper();
            }
        }

        private void InitializeBootstrapper()
        {
            Instance = this;
            DontDestroyOnLoad(this);
            
            _game = new GameController(this);
            _game.StateMachine.Enter<MainMenuState>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            SceneManager.LoadScene(0);
        }
    }
}