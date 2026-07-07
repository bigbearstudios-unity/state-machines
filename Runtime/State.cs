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

        private StateMachine _stateMachine;
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
        /// This functionality can be recreated manually by calling StateMachine.SetState()
        /// </summary>
        /// <param name="key"></param>
        /// <param name="forceTransition"></param>
        protected void SetState(string key, bool forceTransition = false) {
            _stateMachine.SetState(key, forceTransition);
        }

        public State() {}

        /// <summary>
        /// Called every frame while this state is active. Override to add per-frame behaviour.
        /// </summary>
        public virtual void Update() {}

        /// <summary>
        /// Called every physics step while this state is active. Override to add physics behaviour.
        /// </summary>
        public virtual void FixedUpdate() {}

        /// <summary>
        /// Called each late-update while this state is active. Override to add post-render behaviour.
        /// </summary>
        public virtual void LateUpdate() {}

        /// <summary>
        /// Called when this state becomes the active state.
        /// </summary>
        public virtual void Enter() {}

        /// <summary>
        /// Called when this state is replaced by another state.
        /// </summary>
        public virtual void Exit() {}
    }
}