using System;
using System.Collections.Generic;
using neeksdk.Scripts.UI;

namespace neeksdk.Scripts.Infrastructure.StateMachine {
  public class GameStateMachine {
    private readonly Dictionary<Type, IExitableState> _states;
    private IExitableState _activeState;

    public GameStateMachine(MainMenuController mainMenuController)
    {
        _states = new Dictionary<Type, IExitableState>()
        {
            [typeof(MainMenuState)] = new MainMenuState(mainMenuController, this),
            [typeof(LoadLevelState)] = new LoadLevelState(),
            [typeof(GameState)] = new GameState()
        };
    }
    
    public void Enter<TState>() where TState : class, IState {
      IState state = ChangeState<TState>();
      state.Enter();
    }

    public void Enter<TState, TPayload>(TPayload payload) where TState : class, IPayloadedState<TPayload> {
      TState state = ChangeState<TState>();
      state.Enter(payload);
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