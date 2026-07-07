using System;
using System.Collections;
using System.Collections.Generic;

namespace BBUnity.StateMachines {
    /// <summary>
    /// A basic statemachine which allows the following functionality:
    /// - Storing states to be used
    /// - Setting the state
    /// </summary>
    public class StateMachine : IEnumerable<KeyValuePair<string, State>> {

        /// <summary>
        /// The current state. This is the state which will be processed in
        /// the current frame.
        /// </summary>
        protected State _currentState = null;
        public State CurrentState {
            get { return _currentState; }
        } 

        /// <summary>
        /// The available states. These are added by the Developer either via the constructor
        /// or via the Add method.
        /// </summary>
        private Dictionary<string, State> _availableStates = new Dictionary<string, State>();

        /// <summary>
        /// Fired immediately after every state transition completes.
        /// The first argument is the state that was exited (null on the very first transition),
        /// the second is the state that was entered.
        /// </summary>
        public event Action<State, State> OnStateChanged;

        public StateMachine() { }
        public StateMachine(StateParameters states) {
            AddStates(states);
        }

        public void AddState(string key, State state) {
            if(key == null)   throw new ArgumentNullException("key");
            if(state == null) throw new ArgumentNullException("state");

            state.SetStateMachine(this);
            state.SetReferenceKey(key);

            _availableStates.Add(key, state);
        }

        public void AddStates(StateParameters stateParameters) {
            if(stateParameters == null) throw new ArgumentNullException("states");

            foreach(StateParameter p in stateParameters) {
                AddState(p.Key, p.State);
            }
        }

        /// <summary>
        /// Returns true if a state has been registered under <paramref name="key"/>.
        /// </summary>
        public bool HasState(string key) {
            if(key == null) throw new ArgumentNullException("key");

            return _availableStates.ContainsKey(key);
        }

        /// <summary>
        /// Removes the state registered under <paramref name="key"/>.
        /// Throws <see cref="KeyNotFoundException"/> if no state with that key exists.
        /// </summary>
        public void RemoveState(string key) {
            if(key == null) throw new ArgumentNullException("key");

            if(!_availableStates.Remove(key)) {
                throw new KeyNotFoundException($"BBUnity.StateMachines.StateMachine - No state registered with key '{key}'");
            }
        }

        public void SetState(string key, bool forceTransition = false) {
            if(key == null) throw new ArgumentNullException("key");

            SetState(GetState(key), forceTransition);
        }

        private void SetState(State newState, bool forceTransition = false) {
            if(newState == _currentState) {
                if(forceTransition) {
                    ReplaceState(_currentState, newState);
                }
            } else {
                ReplaceState(_currentState, newState);
            }
        }

        private State GetState(string key) {
            if(!_availableStates.TryGetValue(key, out State state)) {
                throw new KeyNotFoundException($"BBUnity.StateMachines.StateMachine - No state registered with key '{key}'");
            }

            return state;
        }

        /// <summary>
        /// Internal method to replace the oldstate with a newState. This can be 
        /// overridden in sub classes if extra functionality is required
        /// </summary>
        /// <param name="oldState"></param>
        /// <param name="newState"></param>
        /// <returns></returns>
        protected void ReplaceState(State oldState, State newState) {
            if(newState == null) throw new ArgumentNullException("newState", "BBUnity.StateMachines.StateMachine - Cannot transition to a null state");

            oldState?.Exit();
            _currentState = newState;
            newState.Enter();

            OnStateChanged?.Invoke(oldState, newState);
        }

        /// <summary>
        /// Drives the current state's per-frame tick. Call this from MonoBehaviour.Update().
        /// </summary>
        public void Update() {
            _currentState?.Update();
        }

        /// <summary>
        /// Drives the current state's physics tick. Call this from MonoBehaviour.FixedUpdate().
        /// </summary>
        public void FixedUpdate() {
            _currentState?.FixedUpdate();
        }

        /// <summary>
        /// Drives the current state's post-render tick. Call this from MonoBehaviour.LateUpdate().
        /// </summary>
        public void LateUpdate() {
            _currentState?.LateUpdate();
        }

        /// <summary>
        /// Calls Enter on the current state.
        /// </summary>
        public void Enter() {
            _currentState?.Enter();
        }

        /// <summary>
        /// Calls Exit on the current state.
        /// </summary>
        public void Exit() {
            _currentState?.Exit();
        }

        /*
         * Enumeration
         */

        public IEnumerator<KeyValuePair<string, State>> GetEnumerator() {
            return _availableStates.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}