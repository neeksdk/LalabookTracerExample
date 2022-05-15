using System;
using System.Collections.Generic;
using neeksdk.Scripts.Infrastructure.SceneController;
using neeksdk.Scripts.UI;

namespace neeksdk.Scripts.Infrastructure.StateMachine {
  public class GameStateMachine {
    private readonly Dictionary<Type, IExitableState> _states;
    private IExitableState _activeState;

    public GameStateMachine(MainMenuController mainMenuController, SceneLoader sceneLoader, GameController gameController)
    {
        _states = new Dictionary<Type, IExitableState>()
        {
            [typeof(MainMenuState)] = new MainMenuState(mainMenuController, this),
            [typeof(LoadLevelState)] = new LoadLevelState(sceneLoader, gameController, this),
            [typeof(GameState)] = new GameState(gameController, sceneLoader, this)
        };
    }
    
    public void Enter<TState>() where TState : class, IState {
      IState state = ChangeState<TState>();
      state.Enter();
    }

    public void Enter<TState, TScene, TPath>(TScene scene, TPath path) where TState : class, IPayloadedState<TScene, TPath> {
      TState state = ChangeState<TState>();
      state.Enter(scene, path);
    }
    
    public void Enter<TState, TScene>(TScene scene) where TState : class, ISceneSwitchState<TScene> {
        TState state = ChangeState<TState>();
        state.Enter(scene);
    }

    private TState ChangeState<TState>() where TState : class, IExitableState {
      _activeState?.Exit();
      TState state = GetState<TState>();
      _activeState = state;

      return state;
    }

    private TState GetState<TState>() where TState : class, IExitableState => _states[typeof(TState)] as TState;
  }
}