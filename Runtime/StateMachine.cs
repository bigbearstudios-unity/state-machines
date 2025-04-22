using System;
using System.Collections;
using System.Collections.Generic;

namespace BBUnity.StateMachines {
    /// <summary>
    /// A basic statemachine which allows the following functionality:
    /// - Storing states to be used
    /// - Setting the state
    /// The StateMachine also acts as a State and thus can be overridden
    /// to perform its own state management
    /// </summary>
    public class StateMachine : IEnumerable<KeyValuePair<int, State>> {

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
        private Dictionary<int, State> _availableStates = new Dictionary<int, State>();

        public StateMachine() { }
        public StateMachine(StateParameters states) {
            AddStates(states);
        }

        public void AddState(string key, State state) {
            if(key == null) throw new ArgumentNullException("key");
            
            state.SetStateMachine(this);
            state.SetReferenceKey(key);

            _availableStates.Add(key.GetHashCode(), state);
        }

        public void AddStates(StateParameters stateParameters) {
            if(stateParameters == null) throw new ArgumentNullException("states");
            
            foreach(StateParameter p in stateParameters) {
                AddState(p.Key, p.State);
            }
        }

        public void SetState(string key, bool forceTransition = false) {
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
            return _availableStates[key.GetHashCode()];
        }

        /// <summary>
        /// Internal method to replace the oldstate with a newState. This can be 
        /// overridden in sub classes if extra functionality is required
        /// </summary>
        /// <param name="oldState"></param>
        /// <param name="newState"></param>
        /// <returns></returns>
        protected void ReplaceState(State oldState, State newState) {
            oldState?.Exit();
            _currentState = newState;

            newState?.Enter();
        }

        public void Update() {
            _currentState?.Update();
        }

        /// <summary>
        /// Calls OnEnter on the state machine, this will:
        /// - Call OnEnter on the current transition
        /// </summary>
        public void Enter() {
            _currentState?.Enter();
        }

        /// <summary>
        /// Calls OnExit on the state machine, this will:
        /// - Call OnExit on the current transition
        /// </summary>
        public void Exit() {
            _currentState?.Exit();
        }

        /*
         * Enumeration
         */

        public IEnumerator<KeyValuePair<int, State>> GetEnumerator() {
            return _availableStates.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}