using UnityEngine;
using BehaviorTree;

namespace DefenderAI
{
    public class AttackAction : ActionNode
    {
        private bool attackStarted = false;

        public AttackAction(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

        public override NodeState Evaluate()
        {
            DefenderController controller = blackboard.GetValue<DefenderController>("controller");
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
            DefenderController controller = blackboard.GetValue<DefenderController>("controller");
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
            DefenderController controller = blackboard.GetValue<DefenderController>("controller");

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
            DefenderController controller = blackboard.GetValue<DefenderController>("controller");

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
            DefenderController controller = blackboard.GetValue<DefenderController>("controller");
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
            DefenderController controller = blackboard.GetValue<DefenderController>("controller");
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
            DefenderController controller = blackboard.GetValue<DefenderController>("controller");

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
            DefenderController controller = blackboard.GetValue<DefenderController>("controller");
            controller?.Stop();
            base.Reset();
        }
    }

    // 측면 이동 (벽 몰림 방지)
    public class MoveSideways : ActionNode
    {
        private static int lastDirection = 0; // 0 = 좌측, 1 = 우측

        public MoveSideways(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

        public override NodeState Evaluate()
        {
            DefenderController controller = blackboard.GetValue<DefenderController>("controller");
            if (controller == null) return NodeState.Failure;

            // 4방향 제한: 좌 또는 우만 선택
            Vector3 moveDirection;

            // 방향 교대로 선택 (지그재그 패턴)
            if (lastDirection == 0)
            {
                moveDirection = Vector3.left;
                lastDirection = 1;
            }
            else
            {
                moveDirection = Vector3.right;
                lastDirection = 0;
            }

            controller.Move(moveDirection);
            return NodeState.Running;
        }
    }

    public class PatrolOrWait : ActionNode
    {
        public PatrolOrWait(MonoBehaviour owner, Blackboard blackboard)
            : base(owner, blackboard) { }

        public override NodeState Evaluate()
        {
            DefenderController controller = blackboard.GetValue<DefenderController>("controller");
            Transform target = blackboard.GetValue<Transform>("target");

            if (controller == null || target == null) return NodeState.Failure;

            float distance = Vector3.Distance(owner.transform.position, target.position);

            // 데드존 사용: 2.8~4.2미터 사이에서는 움직이지 않음
            if (distance < 2.8f)
            {
                // 너무 가까우면 뒤로 살짝 이동
                Vector3 awayDirection = (owner.transform.position - target.position).normalized;
                controller.Move(awayDirection);
                return NodeState.Running;
            }
            else if (distance > 4.2f)
            {
                // 너무 멀면 살짝 접근 (하지만 조심스럽게)
                Vector3 towardDirection = (target.position - owner.transform.position).normalized;
                controller.Move(towardDirection * 0.5f); // 천천히 이동
                return NodeState.Running;
            }
            else
            {
                // 2.8~4.2미터 = 안전구간, 움직이지 않음
                controller.Stop();
                return NodeState.Success;
            }
        }
    }

    // 넓은 공간으로 이동 (가장자리 회피)
    public class MoveToOpenSpace : ActionNode
    {
        private bool isMoving = false;

        public MoveToOpenSpace(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

        public override NodeState Evaluate()
        {
            DefenderController controller = blackboard.GetValue<DefenderController>("controller");
            Transform target = blackboard.GetValue<Transform>("target");
            Transform groundObject = blackboard.GetValue<Transform>("groundObject");

            if (controller == null || target == null || groundObject == null) return NodeState.Failure;

            Vector3 currentPos = owner.transform.position;

            // 바닥 오브젝트의 중심점을 맵 중심으로 사용
            Vector3 mapCenter = groundObject.position;

            // 바닥 오브젝트의 크기 고려 (스케일 기준)
            Vector3 groundScale = groundObject.localScale;
            float mapRadius = Mathf.Min(groundScale.x, groundScale.z) * 0.3f; // 바닥 크기의 30% 내부를 안전 영역으로

            // 현재 위치가 가장자리에 가까우면 중앙으로
            float distanceFromCenter = Vector3.Distance(currentPos, mapCenter);

            if (distanceFromCenter > mapRadius) // 가장자리에 너무 가까움
            {
                // 맵 중앙 방향으로 이동 시작/계속
                Vector3 toCenterDirection = (mapCenter - currentPos).normalized;
                Vector3 moveDirection = GetClosest4Direction(toCenterDirection);
                controller.Move(moveDirection);
                isMoving = true;
                return NodeState.Running; // 이동 중이므로 Running
            }
            else
            {
                // 안전한 위치에 도달
                if (isMoving)
                {
                    controller.Stop();
                    isMoving = false;
                    return NodeState.Success; // 목표 달성으로 Success
                }
                else
                {
                    return NodeState.Failure; // 이미 안전한 위치에 있으므로 이 노드는 실행할 필요 없음
                }
            }
        }

        public override void Reset()
        {
            isMoving = false;
            base.Reset();
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

    // 공격형을 중앙으로 유도
    public class LureToCenter : ActionNode
    {
        public LureToCenter(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

        public override NodeState Evaluate()
        {
            DefenderController controller = blackboard.GetValue<DefenderController>("controller");
            Transform target = blackboard.GetValue<Transform>("target");
            Transform groundObject = blackboard.GetValue<Transform>("groundObject");

            if (controller == null || target == null || groundObject == null) return NodeState.Failure;

            Vector3 currentPos = owner.transform.position;
            Vector3 targetPos = target.position;
            Vector3 mapCenter = groundObject.position; // 바닥 오브젝트 중심 사용

            // 바닥 오브젝트 크기 기준으로 가장자리 판단
            Vector3 groundScale = groundObject.localScale;
            float edgeDistance = Mathf.Min(groundScale.x, groundScale.z) * 0.4f;

            // 공격형이 가장자리에 있으면 중앙으로 유도
            float targetDistanceFromCenter = Vector3.Distance(targetPos, mapCenter);

            if (targetDistanceFromCenter > edgeDistance) // 공격형이 가장자리에 있음
            {
                // 공격형보다 중앙 쪽에서 대기 (유인)
                Vector3 lurePosition = Vector3.Lerp(targetPos, mapCenter, 0.7f);
                Vector3 toLureDirection = (lurePosition - currentPos).normalized;
                Vector3 moveDirection = GetClosest4Direction(toLureDirection);

                controller.Move(moveDirection);
                return NodeState.Running;
            }
            else
            {
                // 공격형이 이미 중앙에 있음
                return NodeState.Success;
            }
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
}

