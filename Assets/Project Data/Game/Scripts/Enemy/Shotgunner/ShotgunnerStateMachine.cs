using System.Collections.Generic;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.Enemy.Shotgunner
{
    [RequireComponent(typeof(ShotgunerEnemyBehavior))]
    public class ShotgunnerStateMachine : AbstractStateMachine<State>
    {
        private ShotgunerEnemyBehavior enemy;

        private void Awake()
        {
            enemy = GetComponent<ShotgunerEnemyBehavior>();

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
            attackingStateCase.state = new AimAndAttackState(enemy);
            attackingStateCase.transitions = new List<StateTransition<State>>()
            {
                new StateTransition<State>(AttackingStateTransition)
            };

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

            if (enemy.IsTargetInAttackRange && enemy.IsTargetInSight())
                nextState = State.Attacking;
            else
                nextState = State.Following;
            return true;
        }

        private bool FollowingStateTransition(out State nextState)
        {
            if (enemy.IsTargetInAttackRange && enemy.IsTargetInSight())
            {
                nextState = State.Attacking;
                return true;
            }

            nextState = State.Following;
            return false;
        }

        private bool AttackingStateTransition(out State nextState)
        {
            var attackingState = states[State.Attacking].state;
            if ((attackingState as AimAndAttackState).IsFinished)
            {
                if (enemy.IsTargetInAttackRange && enemy.IsTargetInSight())
                    nextState = State.Attacking;
                else
                    nextState = State.Following;

                return true;
            }

            nextState = State.Attacking;
            return false;
        }
    }
}