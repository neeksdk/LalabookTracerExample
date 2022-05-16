using System;
using System.Collections.Generic;
using neeksdk.Scripts.FigureTracer;
using neeksdk.Scripts.Game;
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
        private readonly BezierLineFactory _bezierLineFactory;
        private GameInterfaceView _gameInterfaceView;
        private FigureContainer _figureContainer;

        private readonly List<FingerPointer> _allLines = new List<FingerPointer>();
        private BezierFigureData _allFigureData;
        private FingerPointer _currentFingerPointer; 
        
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

            if (!SaveLoadDataClass.TryLoadFigureByFile(figurePath, out BezierFigureData figureData))
            {
                promise.Resolve();
                _gameInterfaceView.SetInformText("File can't be loaded. Return to main menu and try another file.");
                
                return promise;
            }
            
            _allFigureData = figureData;
            _allLines.Clear();
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

            Promise.Sequence(promises).Then(StartNextLineDraw).Then(() => _gameInterfaceView.SetupView());
        }

        private void StartNextLineDraw()
        {
            _currentLineIndex += 1;
            _currentFingerPointer = _allLines[_currentLineIndex];
            _currentFingerPointer.SetSortingOrder(1);
            _currentFingerPointer.SetupFingerPointer(Camera.main);
            _currentFingerPointer.BeginDrag();
            _currentFingerPointer.OnDestinationReached += DestinationReached;
            _currentFingerPointer.OnFingerOutOfPointer += FingerOutOfPointer;
            _currentFingerPointer.OnFingerStartDragging += FingerStartDragging;
        }

        private void DestinationReached(FingerPointer fingerPointer)
        {
            _currentFingerPointer.OnDestinationReached -= DestinationReached;
            _currentFingerPointer.OnFingerOutOfPointer -= FingerOutOfPointer;
            _currentFingerPointer.OnFingerStartDragging -= FingerStartDragging;
            _currentFingerPointer.EndDrag().Then(() =>
            {
                if (_allLines.Count > _currentLineIndex + 1)
                {

                    _allLines[_currentLineIndex].SetSortingOrder(0);
                    StartNextLineDraw();
                }
                else
                {
                    _gameInterfaceView.SetInformText("Congratulations! You complete drawing figure! To begin a new draw, please return to main menu.");
                }
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