using System;
using System.Collections.Generic;
using NUnit.Framework;
using BBUnity.StateMachines;
using BBUnity.StateMachines.Internal;

namespace StateMachines {
    public class StateTests {

        private class StubState : State {
            public int EnterCount  { get; private set; }
            public int ExitCount   { get; private set; }
            public int UpdateCount { get; private set; }

            public override void Enter()  => EnterCount++;
            public override void Exit()   => ExitCount++;
            public override void Update() => UpdateCount++;
        }

        // A state that triggers a transition to another key when Update() is called.
        // Used to test State.SetState(), the protected helper on State itself.
        private class SelfTransitioningState : State {
            private readonly string _nextKey;
            public SelfTransitioningState(string nextKey) { _nextKey = nextKey; }
            public override void Update() => SetState(_nextKey);
        }

        [Test]
        public void AddState_SingleState_CanTransitionToIt() {
            var sm    = new StateMachine();
            var state = new StubState();
            sm.AddState("idle", state);

            sm.SetState("idle");

            Assert.AreEqual(state, sm.CurrentState);
        }

        [Test]
        public void AddState_SetsReferenceKeyOnState() {
            var sm    = new StateMachine();
            var state = new StubState();

            sm.AddState("idle", state);

            Assert.AreEqual("idle", state.ReferenceKey);
        }

        [Test]
        public void AddState_MultipleStates_AllReachableViaSetState() {
            var sm   = new StateMachine();
            var idle = new StubState();
            var run  = new StubState();
            sm.AddState("idle", idle);
            sm.AddState("run",  run);

            sm.SetState("idle");
            Assert.AreEqual(idle, sm.CurrentState);

            sm.SetState("run");
            Assert.AreEqual(run, sm.CurrentState);
        }

        [Test]
        public void AddState_NullKey_ThrowsArgumentNullException() {
            var sm = new StateMachine();

            Assert.Throws<ArgumentNullException>(() => sm.AddState(null, new StubState()));
        }

        /// <summary>
        /// Issue 3 — AddState does not guard against a null state, so calling
        /// state.SetStateMachine(this) on null throws a NullReferenceException
        /// rather than a clear ArgumentNullException.  This test documents the
        /// desired behaviour; it will fail until the fix is applied.
        /// </summary>
        [Test]
        public void AddState_NullState_ThrowsArgumentNullException() {
            var sm = new StateMachine();

            Assert.Throws<ArgumentNullException>(() => sm.AddState("idle", null));
        }

        [Test]
        public void AddState_DuplicateKey_ThrowsArgumentException() {
            var sm = new StateMachine();
            sm.AddState("idle", new StubState());

            // Dictionary.Add throws ArgumentException for a duplicate key.
            Assert.Throws<ArgumentException>(() => sm.AddState("idle", new StubState()));
        }

        [Test]
        public void AddState_StateAlreadyRegisteredInAnotherMachine_ThrowsException() {
            // State.SetStateMachine throws StateAlreadyAssignedException (internal)
            // when a state that already belongs to one machine is registered in another.
            // We assert the base type since the exception class is not public.
            var sm1   = new StateMachine();
            var sm2   = new StateMachine();
            var state = new StubState();
            sm1.AddState("idle", state);

            Assert.Throws<Exception>(() => sm2.AddState("idle", state));
        }

        // ================================================================
        // StateMachine.AddStates (StateParameters)
        // ================================================================

        [Test]
        public void AddStates_RegistersAllProvidedStates() {
            var sm   = new StateMachine();
            var idle = new StubState();
            var run  = new StubState();

            sm.AddStates(new StateParameters {
                { "idle", idle },
                { "run",  run  },
            });

            sm.SetState("idle");
            Assert.AreEqual(idle, sm.CurrentState);
            sm.SetState("run");
            Assert.AreEqual(run, sm.CurrentState);
        }

        [Test]
        public void AddStates_NullParameter_ThrowsArgumentNullException() {
            var sm = new StateMachine();

            Assert.Throws<ArgumentNullException>(() => sm.AddStates(null));
        }

        // ================================================================
        // StateMachine.SetState — normal transitions
        // ================================================================

        [Test]
        public void SetState_UpdatesCurrentState() {
            var sm   = new StateMachine();
            var idle = new StubState();
            var run  = new StubState();
            sm.AddState("idle", idle);
            sm.AddState("run",  run);
            sm.SetState("idle");

            sm.SetState("run");

            Assert.AreEqual(run, sm.CurrentState);
        }

        [Test]
        public void SetState_CallsEnterOnNewState() {
            var sm    = new StateMachine();
            var state = new StubState();
            sm.AddState("idle", state);

            sm.SetState("idle");

            Assert.AreEqual(1, state.EnterCount);
        }

        [Test]
        public void SetState_CallsExitOnPreviousState() {
            var sm   = new StateMachine();
            var idle = new StubState();
            var run  = new StubState();
            sm.AddState("idle", idle);
            sm.AddState("run",  run);
            sm.SetState("idle");

            sm.SetState("run");

            Assert.AreEqual(1, idle.ExitCount);
        }

        [Test]
        public void SetState_DoesNotCallExitOnFirstTransition() {
            // No previous state exists on first SetState so Exit must not fire.
            var sm    = new StateMachine();
            var state = new StubState();
            sm.AddState("idle", state);

            sm.SetState("idle");

            Assert.AreEqual(0, state.ExitCount);
        }

        [Test]
        public void SetState_DoesNotCallEnterOnPreviousState() {
            var sm   = new StateMachine();
            var idle = new StubState();
            var run  = new StubState();
            sm.AddState("idle", idle);
            sm.AddState("run",  run);
            sm.SetState("idle");

            sm.SetState("run");

            Assert.AreEqual(1, idle.EnterCount, "Idle Enter should only have been called once (on the first SetState)");
        }

        // ================================================================
        // StateMachine.SetState — self-transitions
        // ================================================================

        [Test]
        public void SetState_SameState_WithoutForce_DoesNotCallEnterOrExit() {
            var sm    = new StateMachine();
            var state = new StubState();
            sm.AddState("idle", state);
            sm.SetState("idle");             // initial enter — EnterCount = 1

            sm.SetState("idle");             // same state, no force

            Assert.AreEqual(1, state.EnterCount, "Enter should not fire on a non-forced self-transition");
            Assert.AreEqual(0, state.ExitCount,  "Exit should not fire on a non-forced self-transition");
        }

        [Test]
        public void SetState_SameState_WithForce_CallsExitThenEnter() {
            var sm    = new StateMachine();
            var state = new StubState();
            sm.AddState("idle", state);
            sm.SetState("idle");             // initial enter — EnterCount = 1

            sm.SetState("idle", forceTransition: true);

            Assert.AreEqual(2, state.EnterCount, "Enter should fire again on a forced self-transition");
            Assert.AreEqual(1, state.ExitCount,  "Exit should fire on a forced self-transition");
        }

        // ================================================================
        // StateMachine.SetState — error cases
        // ================================================================

        /// <summary>
        /// Issue 2 — GetState has no guard for an unregistered key, so the
        /// dictionary throws a raw KeyNotFoundException with no helpful context.
        /// </summary>
        [Test]
        public void SetState_UnregisteredKey_ThrowsKeyNotFoundException() {
            var sm = new StateMachine();

            Assert.Throws<KeyNotFoundException>(() => sm.SetState("nonexistent"));
        }

        /// <summary>
        /// Issue 4 — SetState does not validate its key parameter before passing
        /// it to GetState, so a null key reaches the dictionary which raises
        /// ArgumentNullException rather than a meaningful error from SetState itself.
        /// </summary>
        [Test]
        public void SetState_NullKey_ThrowsArgumentNullException() {
            var sm = new StateMachine();

            Assert.Throws<ArgumentNullException>(() => sm.SetState(null));
        }

        // ================================================================
        // StateMachine.Update / Enter / Exit — safety with no current state
        // ================================================================

        [Test]
        public void Update_WithNoCurrentState_DoesNotThrow() {
            var sm = new StateMachine();

            Assert.DoesNotThrow(() => sm.Update());
        }

        [Test]
        public void Enter_WithNoCurrentState_DoesNotThrow() {
            var sm = new StateMachine();

            Assert.DoesNotThrow(() => sm.Enter());
        }

        [Test]
        public void Exit_WithNoCurrentState_DoesNotThrow() {
            var sm = new StateMachine();

            Assert.DoesNotThrow(() => sm.Exit());
        }

        // ================================================================
        // StateMachine.Update / Enter / Exit — delegation to current state
        // ================================================================

        [Test]
        public void Update_DelegatesToCurrentState() {
            var sm    = new StateMachine();
            var state = new StubState();
            sm.AddState("idle", state);
            sm.SetState("idle");

            sm.Update();
            sm.Update();

            Assert.AreEqual(2, state.UpdateCount);
        }

        [Test]
        public void Update_DoesNotTickInactiveStates() {
            var sm   = new StateMachine();
            var idle = new StubState();
            var run  = new StubState();
            sm.AddState("idle", idle);
            sm.AddState("run",  run);
            sm.SetState("idle");

            sm.Update();
            sm.SetState("run");
            sm.Update();

            Assert.AreEqual(1, idle.UpdateCount, "Idle should only have been ticked while it was current");
            Assert.AreEqual(1, run.UpdateCount);
        }

        [Test]
        public void Enter_DelegatesToCurrentState() {
            var sm    = new StateMachine();
            var state = new StubState();
            sm.AddState("idle", state);
            sm.SetState("idle");             // EnterCount = 1

            sm.Enter();

            Assert.AreEqual(2, state.EnterCount); // 1 from SetState + 1 from Enter
        }

        [Test]
        public void Exit_DelegatesToCurrentState() {
            var sm    = new StateMachine();
            var state = new StubState();
            sm.AddState("idle", state);
            sm.SetState("idle");

            sm.Exit();

            Assert.AreEqual(1, state.ExitCount);
        }

        // ================================================================
        // StateParameter — construction guards
        // ================================================================

        [Test]
        public void StateParameter_NullKey_ThrowsArgumentNullException() {
            Assert.Throws<ArgumentNullException>(() => new StateParameter(null, new StubState()));
        }

        [Test]
        public void StateParameter_NullState_ThrowsArgumentNullException() {
            Assert.Throws<ArgumentNullException>(() => new StateParameter("idle", null));
        }

        [Test]
        public void StateParameter_ValidArguments_ExposesKeyAndState() {
            var state = new StubState();
            var param = new StateParameter("idle", state);

            Assert.AreEqual("idle", param.Key);
            Assert.AreEqual(state,  param.State);
        }

        // ================================================================
        // StateMachine — constructor with StateParameters
        // ================================================================

        [Test]
        public void Constructor_WithStateParameters_RegistersAllStates() {
            var idle = new StubState();
            var run  = new StubState();

            var sm = new StateMachine(new StateParameters {
                { "idle", idle },
                { "run",  run  },
            });

            sm.SetState("idle");
            Assert.AreEqual(idle, sm.CurrentState);
            sm.SetState("run");
            Assert.AreEqual(run, sm.CurrentState);
        }

        [Test]
        public void Constructor_WithNullStateParameters_ThrowsArgumentNullException() {
            Assert.Throws<ArgumentNullException>(() => new StateMachine(null));
        }

        // ================================================================
        // StateMachine — multi-step transitions
        // ================================================================

        [Test]
        public void SetState_MultipleSequentialTransitions_CorrectEnterExitCounts() {
            var sm   = new StateMachine();
            var idle = new StubState();
            var run  = new StubState();
            var jump = new StubState();
            sm.AddState("idle", idle);
            sm.AddState("run",  run);
            sm.AddState("jump", jump);

            sm.SetState("idle");
            sm.SetState("run");
            sm.SetState("jump");
            sm.SetState("idle");   // back to idle

            Assert.AreEqual(2, idle.EnterCount, "Idle was entered twice (start and return)");
            Assert.AreEqual(1, idle.ExitCount,  "Idle was exited once (to run)");
            Assert.AreEqual(1, run.EnterCount);
            Assert.AreEqual(1, run.ExitCount);
            Assert.AreEqual(1, jump.EnterCount);
            Assert.AreEqual(1, jump.ExitCount);
        }

        [Test]
        public void SetState_UnregisteredKey_ErrorMessageContainsKey() {
            var sm = new StateMachine();

            var ex = Assert.Throws<KeyNotFoundException>(() => sm.SetState("missingState"));

            StringAssert.Contains("missingState", ex.Message);
        }

        // ================================================================
        // State.SetState — state-driven transitions
        // ================================================================

        [Test]
        public void State_SetState_TransitionsToAnotherStateFromWithinUpdate() {
            var sm   = new StateMachine();
            var idle = new SelfTransitioningState("run");
            var run  = new StubState();
            sm.AddState("idle", idle);
            sm.AddState("run",  run);
            sm.SetState("idle");

            sm.Update();   // idle.Update() calls SetState("run") internally

            Assert.AreEqual(run, sm.CurrentState);
        }

        [Test]
        public void State_SetState_CallsEnterOnNewStateAndExitOnPrevious() {
            var sm   = new StateMachine();
            var idle = new SelfTransitioningState("run");
            var run  = new StubState();
            sm.AddState("idle", idle);
            sm.AddState("run",  run);
            sm.SetState("idle");

            sm.Update();   // idle exits, run enters

            Assert.AreEqual(1, run.EnterCount, "Run should have been entered");
            Assert.AreEqual(0, run.ExitCount,  "Run should not have been exited");
        }

        // ================================================================
        // CallbackState
        // ================================================================

        [Test]
        public void CallbackState_OnTick_IsInvokedOnUpdate() {
            int calls = 0;
            var sm    = new StateMachine();
            sm.AddState("cb", new CallbackState(onTick: () => calls++));
            sm.SetState("cb");

            sm.Update();
            sm.Update();

            Assert.AreEqual(2, calls);
        }

        [Test]
        public void CallbackState_OnEnter_IsInvokedOnEnter() {
            int calls = 0;
            var sm    = new StateMachine();
            sm.AddState("cb", new CallbackState(onEnter: () => calls++));

            sm.SetState("cb");

            Assert.AreEqual(1, calls);
        }

        [Test]
        public void CallbackState_OnExit_IsInvokedOnExit() {
            int calls = 0;
            var sm    = new StateMachine();
            sm.AddState("cb",  new CallbackState(onExit: () => calls++));
            sm.AddState("cb2", new StubState());
            sm.SetState("cb");

            sm.SetState("cb2");

            Assert.AreEqual(1, calls);
        }

        [Test]
        public void CallbackState_NullCallbacks_DoNotThrowOnUpdateEnterExit() {
            var sm = new StateMachine();
            sm.AddState("cb", new CallbackState());
            sm.SetState("cb");

            Assert.DoesNotThrow(() => sm.Update());
            Assert.DoesNotThrow(() => sm.Enter());
            Assert.DoesNotThrow(() => sm.Exit());
        }
    }
}
