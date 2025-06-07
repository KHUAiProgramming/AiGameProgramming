using UnityEngine;
using BehaviorTree;

namespace AttackerAI
{
    public class AttackAction : ActionNode
    {
        private bool attackStarted = false;

        public AttackAction(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

        public override NodeState Evaluate()
        {
            AttackerController controller = blackboard.GetValue<AttackerController>("controller");
            Transform target = blackboard.GetValue<Transform>("target");

            if (controller == null || target == null)
            {
                state = NodeState.Failure;
                return state;
            }

            if (!attackStarted)
            {
                if (controller.CanAttack())
                {
                    // 공격 전에 상대방을 향해 회전!
                    Vector3 directionToTarget = (target.position - owner.transform.position).normalized;
                    directionToTarget.y = 0; // Y축 회전 방지

                    if (directionToTarget.magnitude > 0.1f)
                    {
                        owner.transform.rotation = Quaternion.LookRotation(directionToTarget);
                    }

                    controller.Attack();
                    attackStarted = true;
                    state = NodeState.Running;
                }
                else
                {
                    state = NodeState.Failure;
                }
            }
            else
            {
                if (controller.IsAttacking)
                {
                    state = NodeState.Running;
                }
                else
                {
                    attackStarted = false;
                    state = NodeState.Success;
                }
            }

            return state;
        }

        public override void Reset()
        {
            attackStarted = false;
            base.Reset();
        }
    }

    // 방어 액션
    public class BlockAction : ActionNode
    {
        private bool blockStarted = false;

        public BlockAction(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

        public override NodeState Evaluate()
        {
            AttackerController controller = blackboard.GetValue<AttackerController>("controller");
            if (controller == null)
            {
                state = NodeState.Failure;
                return state;
            }

            if (!blockStarted)
            {
                if (controller.CanBlock())
                {
                    controller.Block();
                    blockStarted = true;
                    state = NodeState.Running;
                }
                else
                {
                    state = NodeState.Failure;
                }
            }
            else
            {
                if (controller.IsBlocking)
                {
                    state = NodeState.Running;
                }
                else
                {
                    blockStarted = false;
                    state = NodeState.Success;
                }
            }

            return state;
        }

        public override void Reset()
        {
            blockStarted = false;
            base.Reset();
        }
    }

    // 타겟에서 멀어지기
    public class MoveAwayFromTarget : ActionNode
    {
        public MoveAwayFromTarget(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

        public override NodeState Evaluate()
        {
            MonoBehaviour self = blackboard.GetValue<MonoBehaviour>("self");
            Transform target = blackboard.GetValue<Transform>("target");
            AttackerController controller = blackboard.GetValue<AttackerController>("controller");

            if (target == null || controller == null || self == null) return NodeState.Failure;

            Vector3 direction = (self.transform.position - target.position).normalized;
            controller.Move(direction);

            return NodeState.Running;
        }
    }

    // 적에서 멀어지는 방향으로 회피
    public class DodgeAway : ActionNode
    {
        public DodgeAway(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

        public override NodeState Evaluate()
        {
            MonoBehaviour self = blackboard.GetValue<MonoBehaviour>("self");
            Transform target = blackboard.GetValue<Transform>("target");
            AttackerController controller = blackboard.GetValue<AttackerController>("controller");

            if (target == null || controller == null || self == null) return NodeState.Failure;

            Vector3 awayDirection = (self.transform.position - target.position).normalized;
            Vector3 dodgeDirection = GetClosest4Direction(awayDirection);

            bool success = controller.Dodge(dodgeDirection);
            return success ? NodeState.Success : NodeState.Failure;
        }

        private Vector3 GetClosest4Direction(Vector3 direction)
        {
            Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
            Vector3 closest = directions[0];
            float maxDot = Vector3.Dot(direction, closest);

            foreach (var dir in directions)
            {
                float dot = Vector3.Dot(direction, dir);
                if (dot > maxDot)
                {
                    maxDot = dot;
                    closest = dir;
                }
            }
            return closest;
        }
    }

    // 랜덤 방향 회피
    public class DodgeRandom : ActionNode
    {
        public DodgeRandom(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

        public override NodeState Evaluate()
        {
            AttackerController controller = blackboard.GetValue<AttackerController>("controller");
            if (controller == null) return NodeState.Failure;

            Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
            Vector3 randomDirection = directions[Random.Range(0, directions.Length)];

            bool success = controller.Dodge(randomDirection);
            return success ? NodeState.Success : NodeState.Failure;
        }
    }

    // 타겟 주위 원형 이동
    public class CircleTarget : ActionNode
    {
        private Vector3[] directions = { Vector3.right, Vector3.left };
        private int currentDirection = 0;

        public CircleTarget(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

        public override NodeState Evaluate()
        {
            AttackerController controller = blackboard.GetValue<AttackerController>("controller");
            if (controller == null) return NodeState.Failure;

            // 좌우로 번갈아 이동
            Vector3 moveDir = directions[currentDirection];
            currentDirection = (currentDirection + 1) % directions.Length;

            controller.Move(moveDir);
            return NodeState.Running;
        }
    }

    // 타겟으로 이동 (기존 구현 스타일)
    public class MoveToTarget : ActionNode
    {
        public MoveToTarget(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

        public override NodeState Evaluate()
        {
            MonoBehaviour self = blackboard.GetValue<MonoBehaviour>("self");
            Transform enemy = blackboard.GetValue<Transform>("target");
            AttackerController controller = blackboard.GetValue<AttackerController>("controller");

            if (self == null || enemy == null || controller == null) return NodeState.Failure;

            Vector3 toTarget = enemy.position - self.transform.position;
            float distance = Vector3.Distance(self.transform.position, enemy.position);

            if (distance <= 1f)
            {
                controller.Stop();
                return NodeState.Success;
            }

            // 4방향 제한: 앞, 뒤, 좌, 우만 허용
            Vector3 direction = toTarget.normalized;
            controller.Move(direction);

            return NodeState.Running;
        }

        public override void Reset()
        {
            MonoBehaviour self = blackboard.GetValue<MonoBehaviour>("self");
            AttackerController controller = blackboard.GetValue<AttackerController>("controller");
            controller?.Stop();
            base.Reset();
        }
    }

    // 상대방이 방어 중일 때 측면 공격
    public class FlankAttack : ActionNode
    {
        public FlankAttack(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

        public override NodeState Evaluate()
        {
            AttackerController controller = blackboard.GetValue<AttackerController>("controller");
            Transform target = blackboard.GetValue<Transform>("target");

            if (controller == null || target == null) return NodeState.Failure;

            // 측면으로 이동해서 공격
            Vector3 toTarget = target.position - owner.transform.position;
            Vector3 sideDirection = Vector3.Cross(toTarget.normalized, Vector3.up).normalized;

            // 랜덤하게 좌우 선택
            if (Random.value > 0.5f) sideDirection = -sideDirection;

            controller.Move(sideDirection);
            return NodeState.Running;
        }
    }

    // 상대방이 공격 중일 때 기회 공격
    public class CounterAttack : ActionNode
    {
        public CounterAttack(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

        public override NodeState Evaluate()
        {
            AttackerController controller = blackboard.GetValue<AttackerController>("controller");
            Transform target = blackboard.GetValue<Transform>("target");

            if (controller == null || target == null) return NodeState.Failure;

            if (controller.CanAttack())
            {
                // 상대방을 향해 회전 후 즉시 공격
                Vector3 directionToTarget = (target.position - owner.transform.position).normalized;
                directionToTarget.y = 0;

                if (directionToTarget.magnitude > 0.1f)
                {
                    owner.transform.rotation = Quaternion.LookRotation(directionToTarget);
                }

                controller.Attack();
                return NodeState.Success;
            }

            return NodeState.Failure;
        }
    }
}