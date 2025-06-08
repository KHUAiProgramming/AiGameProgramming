using UnityEngine;
using BehaviorTree;

namespace AttackerAI
{
    // 기본 거리 체크
    public class IsInRange : ConditionNode
    {
        private float range;

        public IsInRange(MonoBehaviour owner, Blackboard blackboard, float range) : base(owner, blackboard)
        {
            this.range = range;
        }

        public override NodeState Evaluate()
        {
            Transform target = blackboard.GetValue<Transform>("target");
            if (target == null)
            {
                state = NodeState.Failure;
                return state;
            }

            float distance = Vector3.Distance(owner.transform.position, target.position);
            state = distance <= range ? NodeState.Success : NodeState.Failure;
            return state;
        }
    }

    // HP 체크
    public class IsLowHP : ConditionNode
    {
        private float threshold;

        public IsLowHP(MonoBehaviour owner, Blackboard blackboard, float threshold) : base(owner, blackboard)
        {
            this.threshold = threshold;
        }

        public override NodeState Evaluate()
        {
            AttackerController controller = blackboard.GetValue<AttackerController>("controller");
            if (controller == null)
            {
                state = NodeState.Failure;
                return state;
            }

            state = controller.HPPercentage <= threshold ? NodeState.Success : NodeState.Failure;
            return state;
        }
    }

    // 공격 가능 체크
    public class CanAttack : ConditionNode
    {
        public CanAttack(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

        public override NodeState Evaluate()
        {
            AttackerController controller = blackboard.GetValue<AttackerController>("controller");
            if (controller == null)
            {
                state = NodeState.Failure;
                return state;
            }

            state = controller.CanAttack() ? NodeState.Success : NodeState.Failure;
            return state;
        }
    }

    // 방어 가능 체크
    public class CanBlock : ConditionNode
    {
        public CanBlock(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

        public override NodeState Evaluate()
        {
            AttackerController controller = blackboard.GetValue<AttackerController>("controller");
            if (controller == null)
            {
                state = NodeState.Failure;
                return state;
            }

            state = controller.CanBlock() ? NodeState.Success : NodeState.Failure;
            return state;
        }
    }

    // 회피 가능 체크
    public class CanDodge : ConditionNode
    {
        public CanDodge(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

        public override NodeState Evaluate()
        {
            AttackerController controller = blackboard.GetValue<AttackerController>("controller");
            if (controller == null)
            {
                state = NodeState.Failure;
                return state;
            }

            state = controller.CanDodge() ? NodeState.Success : NodeState.Failure;
            return state;
        }
    }

    // 랜덤 확률 체크
    public class RandomChance : ConditionNode
    {
        private float probability;

        public RandomChance(MonoBehaviour owner, Blackboard blackboard, float probability) : base(owner, blackboard)
        {
            this.probability = probability;
        }

        public override NodeState Evaluate()
        {
            state = Random.value <= probability ? NodeState.Success : NodeState.Failure;
            return state;
        }
    }

    // 상대방이 방어 중일 때 체크
    public class IsTargetBlocking : ConditionNode
    {
        public IsTargetBlocking(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

        public override NodeState Evaluate()
        {
            Transform target = blackboard.GetValue<Transform>("target");

            if (target == null)
            {
                state = NodeState.Failure;
                return state;
            }

            DefenderController defenderController = target.GetComponent<DefenderController>();
            if (defenderController != null && defenderController.IsBlocking)
            {
                state = NodeState.Success;
                return state;
            }

            state = NodeState.Failure;
            return state;
        }
    }

    // 상대방이 공격 중일 때 체크
    public class IsTargetAttacking : ConditionNode
    {
        public IsTargetAttacking(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

        public override NodeState Evaluate()
        {
            Transform target = blackboard.GetValue<Transform>("target");

            if (target == null)
            {
                state = NodeState.Failure;
                return state;
            }

            DefenderController defenderController = target.GetComponent<DefenderController>();
            if (defenderController != null && defenderController.IsAttacking)
            {
                state = NodeState.Success;
                return state;
            }

            state = NodeState.Failure;
            return state;
        }
    }

    // 타겟과 멀리 떨어져 있는지 체크
    public class IsFarFromTarget : ConditionNode
    {
        private float distance;

        public IsFarFromTarget(MonoBehaviour owner, Blackboard blackboard, float distance) : base(owner, blackboard)
        {
            this.distance = distance;
        }

        public override NodeState Evaluate()
        {
            MonoBehaviour self = blackboard.GetValue<MonoBehaviour>("self");
            Transform target = blackboard.GetValue<Transform>("target");

            if (self == null || target == null)
            {
                state = NodeState.Failure;
                return state;
            }

            float currentDistance = Vector3.Distance(self.transform.position, target.position);
            state = currentDistance > distance ? NodeState.Success : NodeState.Failure;
            return state;
        }
    }

    // 스턴 상태 체크
    public class IsStunned : ConditionNode
    {
        public IsStunned(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

        public override NodeState Evaluate()
        {
            AttackerController controller = blackboard.GetValue<AttackerController>("controller");
            if (controller == null)
            {
                state = NodeState.Failure;
                return state;
            }

            if (controller.IsStunned)
            {
                state = NodeState.Success;
                return state;
            }

            state = NodeState.Failure;
            return state;
        }
    }

    // 죽음 상태 체크 - Behavior Tree 최상위에서 사용
    public class IsDead : ConditionNode
    {
        public IsDead(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

        public override NodeState Evaluate()
        {
            AttackerController controller = blackboard.GetValue<AttackerController>("controller");
            if (controller == null)
            {
                state = NodeState.Failure;
                return state;
            }

            // 죽으면 Success를 반환하여 다른 모든 행동을 차단
            state = controller.IsDead ? NodeState.Success : NodeState.Failure;
            return state;
        }
    }
}