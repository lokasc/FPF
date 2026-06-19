namespace FPS.Controller
{
    /// <summary>
    /// Contract every player movement state must fulfill.
    /// </summary>
    public interface IState
    {
        /// <summary>Called once when the state machine enters this state.</summary>
        void Enter();

        /// <summary>Called every Update while this state is active.</summary>
        void Tick();

        /// <summary>Called every FixedUpdate while this state is active.</summary>
        void FixedTick();

        /// <summary>Called once when the state machine leaves this state.</summary>
        void Exit();
    }
}
