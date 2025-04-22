
using System.Collections.Generic;

namespace BBUnity.StateMachines {

    /// <summary>
    /// 
    /// </summary>
    sealed public class StateParameter {
        private string _key;
        private State _state;

        public string Key { get { return _key; } }
        public State State { get { return _state; } }

        public StateParameter(string key, State state) {
            if(key == null) throw new System.ArgumentNullException("key");
            if(state == null) throw new System.ArgumentNullException("state");

            _key = key;
            _state = state;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class StateParameters : List<StateParameter> {
        public void Add(string key, State state) {
            Add(new StateParameter(key, state));
        }
    }
}