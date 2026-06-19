using System;

namespace FPS.Controller
{
    /// <summary>
    /// Generic, event-driven state machine. States are plain objects implementing IState.
    /// Transitions call Exit on the outgoing state and Enter on the incoming state.
    /// </summary>
    public class StateMachine
    {
        public IState CurrentState { get; private set; }

        /// <summary>Fires after every successful state transition with (previous, next) states.</summary>
        public event Action<IState, IState> StateChanged;

        /// <summary>Activates the initial state without triggering a transition event.</summary>
        public void Initialize(IState initialState)
        {
            CurrentState = initialState;
            CurrentState.Enter();
        }

        /// <summary>
        /// Exits the current state and enters the next one.
        /// No-ops when nextState is already the active state.
        /// </summary>
        public void TransitionTo(IState nextState)
        {
            if (nextState == CurrentState) return;

            IState previous = CurrentState;
            CurrentState?.Exit();
            CurrentState = nextState;
            CurrentState.Enter();
            StateChanged?.Invoke(previous, CurrentState);
        }

        public void Tick()      => CurrentState?.Tick();
        public void FixedTick() => CurrentState?.FixedTick();
    }
}
