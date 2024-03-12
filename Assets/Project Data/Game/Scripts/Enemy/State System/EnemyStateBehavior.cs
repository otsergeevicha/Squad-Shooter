using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.Enemy
{
    public class EnemyStateBehavior
    {
        public BaseEnemyBehavior Enemy { get; private set; }

        protected Vector3 Position => Enemy.transform.position;
        protected Vector3 TargetPosition => Enemy.Target.position;

        protected bool IsTargetInsideAttackRange => Vector3.Distance(Position, TargetPosition) <= Enemy.Stats.AttackDistance;
        protected bool IsTargetInsideFleeRange => Vector3.Distance(Position, TargetPosition) <= Enemy.Stats.FleeDistance;

        public event SimpleCallback OnFinished;
        protected void InvokeOnFinished()
        {
            OnFinished?.Invoke();
        }

        public EnemyStateBehavior(BaseEnemyBehavior enemy)
        {
            Enemy = enemy;
        }

        public virtual void OnStart()
        {

        }

        public virtual void OnEnd()
        {

        }

        public virtual void OnUpdate()
        {

        }
    }

    public class StateTransition<T> where T : System.Enum
    {
        public StateTransitionType transitionType;
        public delegate bool EvaluateDelegate(out T nextState);
        public EvaluateDelegate Evaluate { get; set; }

        public StateTransition(EvaluateDelegate evaluate, StateTransitionType transitionType = StateTransitionType.Independent)
        {
            this.transitionType = transitionType;
            Evaluate = evaluate;
        }
    }

    public enum StateTransitionType
    {
        Independent,
        OnFinish,
    }
}