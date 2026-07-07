# BBUnity State Machines

## Creating a State

Derive from `State` and override the lifecycle methods you need. All overrides are optional — only implement what you use.

```csharp
class RunningState : State {

    public override void Enter() {
        // called when this state becomes active
    }

    public override void Exit() {
        // called when this state is replaced
    }

    public override void Update() {
        // called every frame while this state is active
    }

    public override void FixedUpdate() {
        // called every physics step while this state is active
    }

    public override void LateUpdate() {
        // called every late-update while this state is active
    }
}
```

## Setting up a StateMachine

Register states with string keys, then set an initial state:

```csharp
var stateMachine = new StateMachine();
stateMachine.AddState("Running",   new RunningState());
stateMachine.AddState("Attacking", new AttackingState());
stateMachine.SetState("Running");
```

You can also use `StateParameters` to register all states at construction time:

```csharp
var stateMachine = new StateMachine(new StateParameters {
    { "Running",   new RunningState() },
    { "Attacking", new AttackingState() },
});
stateMachine.SetState("Running");
```

## Driving the StateMachine

`StateMachine` is not a `MonoBehaviour`. Call its tick methods from whichever `MonoBehaviour` owns it:

```csharp
void Update()      => _stateMachine.Update();
void FixedUpdate() => _stateMachine.FixedUpdate();
void LateUpdate()  => _stateMachine.LateUpdate();
```

## Transitioning between states

From outside a state, call `SetState` on the machine directly:

```csharp
stateMachine.SetState("Attacking");
```

From inside a state, use the protected `SetState` helper:

```csharp
class RunningState : State {
    public override void Update() {
        if (SomeCondition()) {
            SetState("Attacking");
        }
    }
}
```

Pass `forceTransition: true` to re-enter the current state (`Exit` then `Enter` are both called):

```csharp
stateMachine.SetState("Running", forceTransition: true);
```

## Querying and removing states

```csharp
if (stateMachine.HasState("Running")) {
    stateMachine.RemoveState("Running");
}
```

## Transition hooks

Subscribe to `OnStateChanged` to react to any transition. The first argument is the previous state (null on the very first transition), the second is the new state:

```csharp
stateMachine.OnStateChanged += (previousState, newState) => {
    Debug.Log($"Transitioned to {newState.ReferenceKey}");
};
```

## Thread Safety

`StateMachine` is **not thread-safe**. The internal state dictionary and `_currentState` field have no synchronisation. In standard Unity single-threaded gameplay this is not a concern, but if you call `SetState()` or `Update()` from a Job, a Task, or any other thread you must add your own locking around all state machine access.
