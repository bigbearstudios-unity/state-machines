using System;

namespace BBUnity.StateMachines.Exceptions {

    internal class StateAlreadyAssignedException : Exception {
        public StateAlreadyAssignedException() : base(message: "BBUnity.StateMachines.State - State is already assigned to a state machine") {}
    }

}