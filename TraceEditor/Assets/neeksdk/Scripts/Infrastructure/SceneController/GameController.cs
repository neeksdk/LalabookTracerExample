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
            _currenFingerPointer.OnFingerStartDragging += FingerStartDragging;
        }

        private void DestinationReached(FingerPointer fingerPointer)
        {
            _currenFingerPointer.OnDestinationReached -= DestinationReached;
            _currenFingerPointer.OnFingerOutOfPointer -= FingerOutOfPointer;
            _currenFingerPointer.OnFingerStartDragging -= FingerStartDragging;
            _currenFingerPointer.EndDrag().Then(() =>
            {
                if (_allLines.Count < _currentLineIndex + 1)
                {
                    StartNextLineDraw();
                    //todo: change sorting orders
                }
                else
                {
                    _gameInterfaceView.SetInformText("Congratulations! You complete drawing figure! To begin a new draw, please return to main menu.");
                    //todo: show reward
                }
                
                //todo: add particles to finger pointer
            });
        }

        private void FingerStartDragging() => _gameInterfaceView.EmptyInformText();
        private void FingerOutOfPointer() => _gameInterfaceView.SetInformText("You've get out of shape, please try again...");

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