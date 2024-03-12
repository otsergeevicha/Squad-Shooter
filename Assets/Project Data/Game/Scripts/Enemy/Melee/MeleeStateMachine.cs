using System.Collections.Generic;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.Enemy.Melee
{
    [RequireComponent(typeof(MeleeEnemyBehaviour))]
    public class MeleeStateMachine : AbstractStateMachine<State>
    {
        private MeleeEnemyBehaviour enemy;

        private void Awake()
        {
            enemy = GetComponent<MeleeEnemyBehaviour>();

            var patrollingStateCase = new StateCase();
            patrollingStateCase.state = new PatrollingState(enemy);
            patrollingStateCase.transitions = new List<StateTransition<State>> {
                new StateTransition<State>(PatrollingStateTransition)
            };

            var followingAttackingStateCase = new StateCase();
            followingAttackingStateCase.state = new MeleeFollowAttackState(enemy);
            followingAttackingStateCase.transitions = new List<StateTransition<State>>
            {
                new StateTransition<State>((out State nextState) =>{
                    nextState = State.Attacking;
                    return false;
                })
            };

            states.Add(State.Patrolling, patrollingStateCase);
            states.Add(State.Attacking, followingAttackingStateCase);
        }

        private bool PatrollingStateTransition(out State nextState)
        {
            var isTargetSpotted = enemy.IsTargetInVisionRange || (!EnemyController.IgnoreAttackAfterDamage && enemy.HasTakenDamage);

            if (!isTargetSpotted)
            {
                nextState = State.Patrolling;
                return false;
            }

            nextState = State.Attacking;
            return true;
        }
    }
}