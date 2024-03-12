using System.Collections.Generic;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.Enemy.BossSniper
{
    [RequireComponent(typeof(BossSniperBehavior))]
    public class BossSniperStateMachine : AbstractStateMachine<State>
    {
        private BossSniperBehavior enemy;

        private BossSniperChangingPositionState changePosState;
        private BossSniperAimState aimState;
        private BossSniperAttackState attackState;

        private void Awake()
        {
            enemy = GetComponent<BossSniperBehavior>();

            var changePosCase = new StateCase();
            changePosState = new BossSniperChangingPositionState(enemy);
            changePosCase.state = changePosState;
            changePosCase.transitions = new List<StateTransition<State>>()
            {
                new StateTransition<State>(ChangePosTransition, StateTransitionType.OnFinish)
            };

            var aimCase = new StateCase();
            aimState = new BossSniperAimState(enemy);
            aimCase.state = aimState;
            aimCase.transitions = new List<StateTransition<State>>()
            {
                new StateTransition<State>(AimTransition, StateTransitionType.OnFinish)
            };

            var shootCase = new StateCase();
            attackState = new BossSniperAttackState(enemy);
            shootCase.state = attackState;
            shootCase.transitions = new List<StateTransition<State>>()
            {
                new StateTransition<State>(ShootTransition, StateTransitionType.OnFinish)
            };

            states.Add(State.ChangingPosition, changePosCase);
            states.Add(State.Aiming, aimCase);
            states.Add(State.Shooting, shootCase);
        }

        private bool ChangePosTransition(out State nextState)
        {
            nextState = State.Aiming;
            return true;
        }

        private bool AimTransition(out State nextState)
        {
            nextState = State.Shooting;
            return true;
        }

        private int shootCount = 0;

        private bool ShootTransition(out State nextState)
        {
            shootCount++;
            if (shootCount == 1)
            {
                shootCount = 0;
                nextState = State.ChangingPosition;
            }
            else
            {
                nextState = State.Aiming;
            }

            return true;
        }
    }
}
