using System.Collections.Generic;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.Enemy.Boss
{
    [RequireComponent(typeof(BossBomberBehaviour))]
    public class BossStateMachine : AbstractStateMachine<State>
    {
        private BossBomberBehaviour enemy;

        private EnemyStateBehavior hidingState;
        private EnteringState enteringState;
        private IdleState idleState;
        private ChasingState chasingState;
        private KikkingState hittingState;
        private ShootingState shootingState;

        private void Awake()
        {
            enemy = GetComponent<BossBomberBehaviour>();

            var distanceBasedTransitionOnFinish = new List<StateTransition<State>>()
            {
                new StateTransition<State>(TransitionToIdle, StateTransitionType.OnFinish),
                new StateTransition<State>(TransitionToKicking, StateTransitionType.OnFinish),
                new StateTransition<State>(TransitionToShooting, StateTransitionType.OnFinish),
                new StateTransition<State>(InstantTransitionToChasing, StateTransitionType.OnFinish)
            };

            var distanceBasedTransitionIndependent = new List<StateTransition<State>>()
            {
                new StateTransition<State>(TransitionToIdle, StateTransitionType.Independent),
                new StateTransition<State>(TransitionToKicking, StateTransitionType.Independent),
                new StateTransition<State>(TransitionToShooting, StateTransitionType.Independent),
                new StateTransition<State>(InstantTransitionToChasing, StateTransitionType.Independent)
            };

            var hidingCase = new StateCase();
            hidingState = new EnemyStateBehavior(enemy);
            hidingCase.state = hidingState;
            hidingCase.transitions = new List<StateTransition<State>>()
            {
                new StateTransition<State>(HidingTransition, StateTransitionType.Independent)
            };

            var enteringCase = new StateCase();
            enteringState = new EnteringState(enemy);
            enteringCase.state = enteringState;
            enteringCase.transitions = distanceBasedTransitionOnFinish;

            var idleCase = new StateCase();
            idleState = new IdleState(enemy);
            idleCase.state = idleState;
            idleCase.transitions = distanceBasedTransitionIndependent;

            var chasingCase = new StateCase();
            chasingState = new ChasingState(enemy);
            chasingCase.state = chasingState;
            chasingCase.transitions = distanceBasedTransitionIndependent;

            var hittingCase = new StateCase();
            hittingState = new KikkingState(enemy);
            hittingCase.state = hittingState;
            hittingCase.transitions = distanceBasedTransitionOnFinish;

            var shootingCase = new StateCase();
            shootingState = new ShootingState(enemy);
            shootingCase.state = shootingState;
            shootingCase.transitions = distanceBasedTransitionOnFinish;


            states.Add(State.Hidden, hidingCase);
            states.Add(State.Entering, enteringCase);
            states.Add(State.Idle, idleCase);
            states.Add(State.Chasing, chasingCase);
            states.Add(State.Hitting, hittingCase);
            states.Add(State.Shooting, shootingCase);
        }

        private bool HidingTransition(out State nextState)
        {
            nextState = State.Entering;
            var isInRange = Vector3.Distance(transform.position, CharacterBehaviour.GetBehaviour().transform.position) < 50;
            if (isInRange)
            {
                enemy.Enter();
            }

            return isInRange;
        }

        private bool TransitionToIdle(out State state)
        {
            state = State.Idle;
            var distance = Vector3.Distance(transform.position, CharacterBehaviour.GetBehaviour().transform.position);

            return distance > enemy.VisionRange;
        }

        private bool TransitionToKicking(out State state)
        {
            state = State.Hitting;
            var distance = Vector3.Distance(transform.position, CharacterBehaviour.GetBehaviour().transform.position);

            return distance < enemy.KickDistance;
        }

        private bool TransitionToShooting(out State state)
        {
            state = State.Shooting;
            var distance = Vector3.Distance(transform.position, CharacterBehaviour.GetBehaviour().transform.position);

            return distance > enemy.AttackDistanceMin && distance <= enemy.AttackDistanceMax;
        }

        private bool InstantTransitionToChasing(out State state)
        {
            state = State.Chasing;

            return true;
        }
    }
}