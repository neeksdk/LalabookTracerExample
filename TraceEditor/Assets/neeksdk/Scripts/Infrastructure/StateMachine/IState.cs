namespace neeksdk.Scripts.Infrastructure.StateMachine {
  public interface IState : IExitableState {
    void Enter();
  }

  public interface IPayloadedState<TScene, TPath> : IExitableState {
    void Enter(TScene scene, TPath figurePath);
  }

  public interface ISceneSwitchState<TScene> : IExitableState
  {
      void Enter(TScene scene);
  }
  
  public interface IExitableState {
    void Exit();
  }
}