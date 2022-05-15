using System;
using System.Collections.Generic;
using neeksdk.Scripts.FigureTracer;
using neeksdk.Scripts.Infrastructure.Factory;
using neeksdk.Scripts.Infrastructure.SaveLoad;
using neeksdk.Scripts.Infrastructure.StateMachine;
using neeksdk.Scripts.StaticData.LinesData;
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

        private List<FingerPointer> _allLines = new List<FingerPointer>();
        private BezierFigureData _allFigureData;
        private FingerPointer _currenFingerPointer; 
        
        private int _currentLineIndex;

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
            
            if (!SaveLoadDataClass.TryLoadFigureByFile(figurePath, out BezierFigureData figureData))
            {
                promise.Resolve();
                
                //todo: inform that file can't be loaded 
                
                return promise;
            }

            _allFigureData = figureData;
            for (int index = 0; index < figureData.BezierLinesData.Count; index++)
            {
                FingerPointer fingerPointer = _bezierLineFactory.InstantiateNewFingerPointer(_figureContainer.transform, Vector3.zero);
                _allLines.Add(fingerPointer);
            }
            
            promise.Resolve();
            
            return promise.Then(() => _gameInterfaceView.OnCloseClick += ClearGame);
        }

        public void StartDrawFigure()
        {
            _currentLineIndex = -1;
            List<Func<IPromise>> promises = new List<Func<IPromise>>();
            for (int index = 0; index < _allFigureData.BezierLinesData.Count; index++)
            {
                BezierDotsData dotsData = _allFigureData.BezierLinesData[index];
                FingerPointer fingerPointer = _allLines[index];
                Func<IPromise> lineDrawPromise = new Func<IPromise>(() =>
                {
                    return fingerPointer.PopulateBezierLineData(dotsData);
                });

                promises.Add(lineDrawPromise);
            }

            Promise.Sequence(promises).Then(StartNextLineDraw);
        }

        private void StartNextLineDraw()
        {
            _currentLineIndex += 1;
            _currenFingerPointer = _allLines[_currentLineIndex];
            _currenFingerPointer.SetupFingerPointer(Camera.main);
            _currenFingerPointer.BeginDrag();
            _currenFingerPointer.OnDestinationReached += DestinationReached;
            _currenFingerPointer.OnFingerOutOfPointer += FingerOutOfPointer;
        }

        private void DestinationReached(FingerPointer obj)
        {
            _currenFingerPointer.OnDestinationReached -= DestinationReached;
            _currenFingerPointer.OnFingerOutOfPointer -= FingerOutOfPointer;
            _currenFingerPointer.EndDrag().Then(() =>
            {
                //todo: check if line ends, if not, change sorting orders and continue draw lines
                //todo: if completes - sgow reward and go to main menu
                //todo: add particles to finger pointer
            });
        }

        private void FingerOutOfPointer()
        {
            //todo: inform about finger wrong direction   
        }

        private void ClearGame()
        {
            _gameInterfaceView.OnCloseClick -= OnGoToMainMenu;
            _gameInterfaceView.ClearView();
            _gameInterfaceView = null;
            _figureContainer = null;
            _allLines.Clear();
            _allFigureData = null;
            OnGoToMainMenu?.Invoke();
        }
    }
}