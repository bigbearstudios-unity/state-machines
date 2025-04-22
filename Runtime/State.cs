using BBUnity.StateMachines.Exceptions;

namespace BBUnity.StateMachines {

    /// <summary>
    /// A single State 
    /// 
    /// Follows the simple flow of:
    /// - Constuctor (Called when the state is created)
    /// - Enter (Called upon entering the state)
    /// - Exit (Called upon leaving the state)
    /// - Update (Called upon a tick / update)
    /// </summary>
    public abstract class State {

        /// <summary>
        /// Reference to the statemachine which owns this state
        /// </summary>
        protected StateMachine _stateMachine;
        protected StateMachine StateMachine { get { return _stateMachine; } }
        private string _referenceKey = null;
        public string ReferenceKey {
            get { return _referenceKey; }
        }


        /// <summary>
        /// Sets the state machine reference to the passed statemachine, checks
        /// if the reference is already set and throws exception
        /// </summary>
        /// <param name="stateMachine"></param>
        /// <exception cref="StateAlreadyAssignedException"></exception>
        internal void SetStateMachine(StateMachine stateMachine) {
            if(_stateMachine != null) throw new StateAlreadyAssignedException();

            _stateMachine = stateMachine; 
        }

        internal void SetReferenceKey(string key) {
            _referenceKey = key;
        }

        /// <summary>
        /// Utility method which calls 'SetState' on the underlying StateMachine.
        /// This functionality can be recreated manually by calling _stateMachine.SetState()
        /// </summary>
        /// <param name="key"></param>
        /// <param name="forceTransition"></param>
        protected void SetState(string key, bool forceTransition = false) {
            _stateMachine.SetState(key, forceTransition);
        }

        public State() {}

        // TODO
        // Add some helper methods here which can be used to change the state directly
        // via the statemachines update, enter, exit methods

        /// <summary>
        /// Called upon the statemachines update being called if this is the current state
        /// </summary>
        public virtual void Update() {}

        /// <summary>
        /// Called upon entering the state
        /// </summary>
        public virtual void Enter() {}

        /// <summary>
        /// Called upon exiting the state
        /// </summary>
        public virtual void Exit() {}
    }
}