using UnityEngine;
using BehaviorTree;

namespace DefenderAI
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
            DefenderController controller = blackboard.GetValue<DefenderController>("controller");
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
            DefenderController controller = blackboard.GetValue<DefenderController>("controller");
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
            DefenderController controller = blackboard.GetValue<DefenderController>("controller");
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
            DefenderController controller = blackboard.GetValue<DefenderController>("controller");
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

    // 선제 방어 - 공격형이 접근할 때 미리 방어 준비
    public class PreemptiveBlock : ConditionNode
    {
        private float detectionRange;

        public PreemptiveBlock(MonoBehaviour owner, Blackboard blackboard, float detectionRange = 3.0f)
            : base(owner, blackboard)
        {
            this.detectionRange = detectionRange;
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

            // 공격형이 접근 중이고 일정 거리 내에 있으면 방어 준비
            if (distance <= detectionRange)
            {
                // 공격형이 공격 모션 중인지도 체크 가능
                AttackerController attackerController = target.GetComponent<AttackerController>();
                if (attackerController != null && attackerController.IsAttacking)
                {
                    state = NodeState.Success;
                    return state;
                }

                // 거리만으로도 판단 (공격형이 가까이 오면)
                state = distance <= 2.5f ? NodeState.Success : NodeState.Failure;
                return state;
            }

            state = NodeState.Failure;
            return state;
        }
    }

    // 전술적 후퇴 조건
    public class TacticalRetreat : ConditionNode
    {
        private float dangerRange;
        private float hpThreshold;

        public TacticalRetreat(MonoBehaviour owner, Blackboard blackboard,
                              float dangerRange = 2.0f, float hpThreshold = 0.6f)
            : base(owner, blackboard)
        {
            this.dangerRange = dangerRange;
            this.hpThreshold = hpThreshold;
        }

        public override NodeState Evaluate()
        {
            Transform target = blackboard.GetValue<Transform>("target");
            DefenderController controller = blackboard.GetValue<DefenderController>("controller");

            if (target == null || controller == null)
            {
                state = NodeState.Failure;
                return state;
            }

            float distance = Vector3.Distance(owner.transform.position, target.position);
            float currentHP = controller.HPPercentage;

            // 위험 거리 내에 있고 HP가 낮으면 후퇴
            if (distance <= dangerRange && currentHP <= hpThreshold)
            {
                state = NodeState.Success;
                return state;
            }

            state = NodeState.Failure;
            return state;
        }
    }

    // 반격 기회 포착
    public class CounterOpportunity : ConditionNode
    {
        public CounterOpportunity(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

        public override NodeState Evaluate()
        {
            Transform target = blackboard.GetValue<Transform>("target");
            DefenderController controller = blackboard.GetValue<DefenderController>("controller");

            if (target == null || controller == null)
            {
                state = NodeState.Failure;
                return state;
            }

            // 방어가 성공적으로 끝났거나, 공격형이 공격 후 딜레이 상태일 때
            AttackerController attackerController = target.GetComponent<AttackerController>();
            if (attackerController != null)
            {
                // 공격형이 공격 중이 아니고, 방어형이 공격 가능하면 반격 기회
                if (!attackerController.IsAttacking && controller.CanAttack())
                {
                    state = NodeState.Success;
                    return state;
                }
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
            DefenderController controller = blackboard.GetValue<DefenderController>("controller");
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