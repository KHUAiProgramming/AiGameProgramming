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
            // X축과 Z축 중 절댓값이 큰 쪽을 선택
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
            {
                // X축이 더 크면 좌우 이동
                return direction.x > 0 ? Vector3.right : Vector3.left;
            }
            else
            {
                // Z축이 더 크면 앞뒤 이동
                return direction.z > 0 ? Vector3.forward : Vector3.back;
            }
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

    // 타겟으로 이동 (설정 가능한 버전)
    public class MoveToTarget : ActionNode
    {
        private float stopDistance;

        public MoveToTarget(MonoBehaviour owner, Blackboard blackboard, float stopDistance = 1.0f)
            : base(owner, blackboard)
        {
            this.stopDistance = stopDistance;
        }

        public override NodeState Evaluate()
        {
            MonoBehaviour self = blackboard.GetValue<MonoBehaviour>("self");
            Transform enemy = blackboard.GetValue<Transform>("target");
            DefenderController controller = blackboard.GetValue<DefenderController>("controller");

            if (self == null || enemy == null || controller == null) return NodeState.Failure;

            Vector3 toTarget = enemy.position - self.transform.position;
            float distance = Vector3.Distance(self.transform.position, enemy.position);

            if (distance <= stopDistance)
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

    // 간단한 거리 유지 - 설정 가능한 버전
    public class PatrolOrWait : ActionNode
    {
        private float idealDistance;
        private float deadZone;
        private float moveSpeed;

        public PatrolOrWait(MonoBehaviour owner, Blackboard blackboard,
                           float idealDistance = 3.5f, float deadZone = 0.7f, float moveSpeed = 0.4f)
            : base(owner, blackboard)
        {
            this.idealDistance = idealDistance;
            this.deadZone = deadZone;
            this.moveSpeed = moveSpeed;
        }

        public override NodeState Evaluate()
        {
            DefenderController controller = blackboard.GetValue<DefenderController>("controller");
            Transform target = blackboard.GetValue<Transform>("target");

            if (controller == null || target == null) return NodeState.Failure;

            float distance = Vector3.Distance(owner.transform.position, target.position);
            float distanceFromIdeal = distance - idealDistance;

            // 데드존 내에서는 완전 정지 (바들바들 방지)
            if (Mathf.Abs(distanceFromIdeal) <= deadZone)
            {
                controller.Stop();
                return NodeState.Success;
            }

            // 데드존 밖에서만 이동 (부드럽게)
            Vector3 direction = distanceFromIdeal > 0
                ? (target.position - owner.transform.position).normalized  // 접근
                : (owner.transform.position - target.position).normalized; // 후퇴

            controller.Move(direction * moveSpeed); // 설정 가능한 속도로 이동
            return NodeState.Running;
        }
    }

    // 넓은 공간으로 이동 (가장자리 회피)
    public class MoveToOpenSpace : ActionNode
    {
        private bool isMoving = false;
        private float safeZoneRatio;  // 맵 크기 대비 안전 영역 비율

        public MoveToOpenSpace(MonoBehaviour owner, Blackboard blackboard, float safeZoneRatio = 0.3f)
            : base(owner, blackboard)
        {
            this.safeZoneRatio = safeZoneRatio;
        }

        public override NodeState Evaluate()
        {
            DefenderController controller = blackboard.GetValue<DefenderController>("controller");
            Transform target = blackboard.GetValue<Transform>("target");
            Transform groundObject = blackboard.GetValue<Transform>("groundObject");

            if (controller == null || target == null || groundObject == null) return NodeState.Failure;

            Vector3 currentPos = owner.transform.position;
            Vector3 mapCenter = groundObject.position;
            Vector3 groundScale = groundObject.localScale;
            float mapRadius = Mathf.Min(groundScale.x, groundScale.z) * safeZoneRatio; // 설정 가능한 안전 영역

            float distanceFromCenter = Vector3.Distance(currentPos, mapCenter);

            if (distanceFromCenter > mapRadius)
            {
                Vector3 toCenterDirection = (mapCenter - currentPos).normalized;
                Vector3 moveDirection = GetClosest4Direction(toCenterDirection);
                controller.Move(moveDirection);
                isMoving = true;
                return NodeState.Running;
            }
            else
            {
                if (isMoving)
                {
                    controller.Stop();
                    isMoving = false;
                    return NodeState.Success;
                }
                else
                {
                    return NodeState.Failure;
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
            // X축과 Z축 중 절댓값이 큰 쪽을 선택
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
            {
                // X축이 더 크면 좌우 이동
                return direction.x > 0 ? Vector3.right : Vector3.left;
            }
            else
            {
                // Z축이 더 크면 앞뒤 이동
                return direction.z > 0 ? Vector3.forward : Vector3.back;
            }
        }
    }

    // 공격형을 중앙으로 유도
    public class LureToCenter : ActionNode
    {
        private float edgeRatio;
        private float lureInterpolation;

        public LureToCenter(MonoBehaviour owner, Blackboard blackboard,
                           float edgeRatio = 0.4f, float lureInterpolation = 0.7f)
            : base(owner, blackboard)
        {
            this.edgeRatio = edgeRatio;
            this.lureInterpolation = lureInterpolation;
        }

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
            float edgeDistance = Mathf.Min(groundScale.x, groundScale.z) * edgeRatio;

            // 공격형이 가장자리에 있으면 중앙으로 유도
            float targetDistanceFromCenter = Vector3.Distance(targetPos, mapCenter);

            if (targetDistanceFromCenter > edgeDistance) // 공격형이 가장자리에 있음
            {
                // 공격형보다 중앙 쪽에서 대기 (유인)
                Vector3 lurePosition = Vector3.Lerp(targetPos, mapCenter, lureInterpolation);
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
            // X축과 Z축 중 절댓값이 큰 쪽을 선택
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
            {
                // X축이 더 크면 좌우 이동
                return direction.x > 0 ? Vector3.right : Vector3.left;
            }
            else
            {
                // Z축이 더 크면 앞뒤 이동
                return direction.z > 0 ? Vector3.forward : Vector3.back;
            }
        }
    }

    // 전술적 후퇴 - 안전 거리까지 후퇴
    public class TacticalRetreatAction : ActionNode
    {
        private float retreatDistance;
        private bool isRetreating = false;

        public TacticalRetreatAction(MonoBehaviour owner, Blackboard blackboard, float retreatDistance = 2.0f)
            : base(owner, blackboard)
        {
            this.retreatDistance = retreatDistance;
        }

        public override NodeState Evaluate()
        {
            MonoBehaviour self = blackboard.GetValue<MonoBehaviour>("self");
            Transform target = blackboard.GetValue<Transform>("target");
            DefenderController controller = blackboard.GetValue<DefenderController>("controller");

            if (target == null || controller == null || self == null) return NodeState.Failure;

            float currentDistance = Vector3.Distance(self.transform.position, target.position);

            // 충분히 멀어졌으면 성공
            if (currentDistance >= retreatDistance + 1.0f) // 여유 거리
            {
                controller.Stop();
                isRetreating = false;
                return NodeState.Success;
            }

            // 후퇴 계속
            Vector3 retreatDirection = (self.transform.position - target.position).normalized;
            Vector3 moveDirection = GetClosest4Direction(retreatDirection);
            controller.Move(moveDirection);
            isRetreating = true;

            return NodeState.Running;
        }

        public override void Reset()
        {
            isRetreating = false;
            base.Reset();
        }

        private Vector3 GetClosest4Direction(Vector3 direction)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
            {
                return direction.x > 0 ? Vector3.right : Vector3.left;
            }
            else
            {
                return direction.z > 0 ? Vector3.forward : Vector3.back;
            }
        }
    }

    // 방어 후 즉시 반격
    public class CounterAfterBlock : ActionNode
    {
        private bool blockCompleted = false;
        private bool counterAttackStarted = false;

        public CounterAfterBlock(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

        public override NodeState Evaluate()
        {
            DefenderController controller = blackboard.GetValue<DefenderController>("controller");
            Transform target = blackboard.GetValue<Transform>("target");

            if (controller == null || target == null) return NodeState.Failure;

            // 방어가 막 끝났는지 체크 (JustFinishedBlocking 프로퍼티 사용)
            if (controller.JustFinishedBlocking && !blockCompleted)
            {
                blockCompleted = true;
            }

            // 방어 완료 후 반격
            if (blockCompleted && !counterAttackStarted)
            {
                if (controller.CanAttack())
                {
                    // 공격 전 회전
                    Vector3 directionToTarget = (target.position - owner.transform.position).normalized;
                    directionToTarget.y = 0;
                    if (directionToTarget.magnitude > 0.1f)
                    {
                        owner.transform.rotation = Quaternion.LookRotation(directionToTarget);
                    }

                    controller.Attack();
                    counterAttackStarted = true;
                    return NodeState.Running;
                }
                else
                {
                    return NodeState.Failure;
                }
            }

            // 반격 진행 중
            if (counterAttackStarted)
            {
                if (controller.IsAttacking)
                {
                    return NodeState.Running;
                }
                else
                {
                    // 반격 완료
                    blockCompleted = false;
                    counterAttackStarted = false;
                    return NodeState.Success;
                }
            }

            return NodeState.Failure;
        }

        public override void Reset()
        {
            blockCompleted = false;
            counterAttackStarted = false;
            base.Reset();
        }
    }

    // 능동적 포지셔닝 - 공격형의 움직임에 대응
    public class AdaptivePositioning : ActionNode
    {
        private Vector3 lastTargetPosition;
        private float positionUpdateTime = 0f;
        private const float UPDATE_INTERVAL = 0.5f;

        public AdaptivePositioning(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

        public override NodeState Evaluate()
        {
            Transform target = blackboard.GetValue<Transform>("target");
            DefenderController controller = blackboard.GetValue<DefenderController>("controller");

            if (target == null || controller == null) return NodeState.Failure;

            positionUpdateTime += Time.deltaTime;

            // 일정 시간마다 타겟 위치 업데이트
            if (positionUpdateTime >= UPDATE_INTERVAL)
            {
                Vector3 currentTargetPos = target.position;
                Vector3 targetMovement = currentTargetPos - lastTargetPosition;

                // 공격형의 이동 방향에 대응하여 측면으로 이동
                Vector3 perpendicular = Vector3.Cross(targetMovement.normalized, Vector3.up);
                Vector3 moveDirection = GetClosest4Direction(perpendicular);

                controller.Move(moveDirection * 0.6f); // 적당한 속도로

                lastTargetPosition = currentTargetPos;
                positionUpdateTime = 0f;
            }

            return NodeState.Running;
        }

        private Vector3 GetClosest4Direction(Vector3 direction)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
            {
                return direction.x > 0 ? Vector3.right : Vector3.left;
            }
            else
            {
                return direction.z > 0 ? Vector3.forward : Vector3.back;
            }
        }
    }
}

