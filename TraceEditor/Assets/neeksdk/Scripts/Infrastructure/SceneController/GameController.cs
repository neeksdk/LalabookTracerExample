using System;
using neeksdk.Scripts.Infrastructure.Factory;
using neeksdk.Scripts.Infrastructure.StateMachine;
using neeksdk.Scripts.UI;
using RSG;
using UnityEngine;

namespace neeksdk.Scripts.Infrastructure.SceneController
{
    public class GameController
    {
        public GameStateMachine StateMachine { get; }
        private BezierLineFactory _bezierLineFactory;
        private GameInterfaceView _gameInterfaceView;
        private FigureContainer _figureContainer;
        
        public Action OnGoToMainMenu;
        
        public GameController(ICoroutineRunner coroutineRunner)
        {
            _bezierLineFactory = new BezierLineFactory();
            StateMachine = new GameStateMachine(new MainMenuController(), new SceneLoader(coroutineRunner), this);
        }

        public IPromise WarmUp(string figurePath)
        {
            Promise promise = new Promise();
            _gameInterfaceView = GameObject.FindObjectOfType<GameInterfaceView>();
            _figureContainer = GameObject.FindObjectOfType<FigureContainer>();
            _gameInterfaceView.SetupView();
            promise.Resolve();
            return promise.Then(() => _gameInterfaceView.OnCloseClick += ClearGame);
        }

        public void StartDrawFigure()
        {
            
        }

        private void ClearGame()
        {
            _gameInterfaceView.OnCloseClick -= OnGoToMainMenu;
            _gameInterfaceView.ClearView();
            _gameInterfaceView = null;
            _figureContainer = null;
            OnGoToMainMenu?.Invoke();
        }
    }
}