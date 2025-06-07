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
}