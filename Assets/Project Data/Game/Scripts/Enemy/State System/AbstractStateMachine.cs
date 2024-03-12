using System.Collections.Generic;
using UnityEngine;
using Watermelon.LevelSystem;

namespace Watermelon.Enemy
{
    /// <summary>
    /// This is an implementation of a finite state machine (FSM) in Unity using generics, where the state machine can be used for any type of Enum.
    /// The AbstractStateMachine class has a generic type T that is constrained to be an Enum type. It inherits from the MonoBehaviour class, 
    /// which allows it to be added to a game object and participate in the Unity event system.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AbstractStateMachine<T> : MonoBehaviour, IStateMachine where T : System.Enum
    {
        /// <summary>
        /// The startState variable is a field of type T and it represents the initial state of the state machine. 
        /// It is marked as SerializeField so that it can be set in the Unity Inspector.
        /// </summary>
        [SerializeField] protected T startState;

        /// <summary>
        /// The states variable is a dictionary that maps states to StateCase objects. 
        /// A StateCase object contains a state variable of type EnemyStateBehavior (an interface for defining enemy states) and a list of transitions, 
        /// each of which is a StateTransition<T> object. The StateTransition class defines the criteria for transitioning between states.
        /// </summary>
        protected Dictionary<T, StateCase> states = new Dictionary<T, StateCase>();

        /// <summary>
        /// The CurrentState property is a getter/setter that tracks the current state of the state machine.
        /// </summary>
        public T CurrentState { get; protected set; }

        /// <summary>
        /// The IsPlaying property is a boolean flag that is used to start and stop the state machine.
        /// </summary>
        public bool IsPlaying { get; protected set; }

        /// <summary>
        /// The StartMachine method initializes the state machine by setting IsPlaying to true, setting the CurrentState to the startState, and calling the StartState method.
        /// </summary>
        public void StartMachine()
        {
            IsPlaying = true;

            CurrentState = startState;
            StartState();
        }

        /// <summary>
        /// The StartState method retrieves the EnemyStateBehavior for the current state, subscribes to the OnFinished event of the state, and calls the OnStart method of the state.
        /// </summary>
        private void StartState()
        {
            var state = states[CurrentState].state;

            state.OnFinished += OnStateFinished;
            state.OnStart();
        }

        /// <summary>
        /// The EndState method retrieves the EnemyStateBehavior for the current state, unsubscribes from the OnFinished event of the state, and calls the OnEnd method of the state.
        /// </summary>
        private void EndState()
        {
            var state = states[CurrentState].state;

            state.OnFinished -= OnStateFinished;
            state.OnEnd();
        }

        /// <summary>
        /// The StopMachine method sets IsPlaying to false and calls the OnEnd method of the current state.
        /// </summary>
        public void StopMachine()
        {
            IsPlaying = false;

            var state = states[CurrentState].state;

            state.OnEnd();
        }

        /// <summary>
        /// The OnStateFinished method is called when the OnFinished event is triggered by the current state. 
        /// It retrieves the StateCase for the current state and iterates through its transitions. 
        /// If a transition's transitionType is set to StateTransitionType.OnFinish and it evaluates to true (i.e. the criteria for the transition are met), 
        /// the EndState method is called, the CurrentState is updated to the next state, and the StartState method is called.
        /// </summary>
        private void OnStateFinished()
        {
            var stateCase = states[CurrentState];

            var state = stateCase.state;
            var transitions = stateCase.transitions;

            for (int i = 0; i < transitions.Count; i++)
            {
                var transition = transitions[i];
                if (transition.transitionType == StateTransitionType.OnFinish && transition.Evaluate(out var nextState))
                {
                    EndState();

                    CurrentState = nextState;

                    StartState();
                    break;
                }
            }
        }

        /// <summary>
        /// The Update method is called once per frame when the state machine is playing. 
        /// It retrieves the StateCase for the current state, calls the OnUpdate method of the state, and iterates through its transitions. 
        /// If a transition's transitionType is set to StateTransitionType.
        /// Independent and it evaluates to true, the EndState method is called, the CurrentState is updated to the next state, and the StartState method is called.
        /// </summary>
        private void Update()
        {
            if (IsPlaying && LevelController.IsGameplayActive)
            {
                var stateCase = states[CurrentState];

                var state = stateCase.state;
                var transitions = stateCase.transitions;

                state.OnUpdate();

                for (int i = 0; i < transitions.Count; i++)
                {
                    var transition = transitions[i];
                    if (transition.transitionType == StateTransitionType.Independent && transition.Evaluate(out var nextState))
                    {
                        EndState();

                        CurrentState = nextState;

                        StartState();
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// The StateCase class is a simple container for a EnemyStateBehavior and its list of StateTransition<T> objects.
        /// </summary>
        public class StateCase
        {
            public EnemyStateBehavior state;
            public List<StateTransition<T>> transitions;
        }
    }

    public interface IStateMachine
    {
        void StartMachine();
        void StopMachine();
    }
}