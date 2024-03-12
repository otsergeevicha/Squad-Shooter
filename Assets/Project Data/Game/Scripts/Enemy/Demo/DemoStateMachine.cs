using System.Collections.Generic;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.Enemy.Demo
{
    [RequireComponent(typeof(DemoEnemyBehavior))]
    public class DemoStateMachine : AbstractStateMachine<State>
    {
        private DemoEnemyBehavior enemy;

        private void Awake()
        {
            enemy = GetComponent<DemoEnemyBehavior>();

            var patrollingStateCase = new StateCase();
            patrollingStateCase.state = new PatrollingState(enemy);
            patrollingStateCase.transitions = new List<StateTransition<State>> {
                new StateTransition<State>(PatrollingStateTransition)
            };

            var followingStateCase = new StateCase();
            followingStateCase.state = new FollowingState(enemy);
            followingStateCase.transitions = new List<StateTransition<State>>
            {
                new StateTransition<State>(FollowingStateTransition)
            };

            var attackingStateCase = new StateCase();
            attackingStateCase.state = new AttackingState(enemy);
            attackingStateCase.transitions = new List<StateTransition<State>>();

            states.Add(State.Patrolling, patrollingStateCase);
            states.Add(State.Following, followingStateCase);
            states.Add(State.Attacking, attackingStateCase);
        }

        private bool PatrollingStateTransition(out State nextState)
        {
            var isTargetSpotted = enemy.IsTargetInVisionRange || (!EnemyController.IgnoreAttackAfterDamage && enemy.HasTakenDamage);

            if (!isTargetSpotted)
            {
                nextState = State.Patrolling;
                return false;
            }

            if (enemy.IsTargetInAttackRange)
                nextState = State.Attacking;
            else
                nextState = State.Following;
            return true;
        }

        private bool FollowingStateTransition(out State nextState)
        {
            var shouldAttack = enemy.IsTargetInAttackRange && enemy.IsTargetInSight();
            nextState = shouldAttack ? State.Attacking : State.Following;
            return shouldAttack;
        }
    }
}