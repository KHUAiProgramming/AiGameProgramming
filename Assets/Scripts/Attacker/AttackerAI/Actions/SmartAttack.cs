using UnityEngine;
using BehaviorTree;

public class SmartAttack : ActionNode
{
    private bool attackStarted = false;
    private bool useStrongAttack = false;

    private float strongAttackRange = 5.0f;
    private float weakAttackRange = 3.0f;

    public SmartAttack(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard)
    {
    }

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
            if (!controller.CanAttack())
            {
                state = NodeState.Failure;
                return state;
            }

            // 거리에 따라 최적 공격 선택
            float distance = Vector3.Distance(controller.transform.position, target.position);

            if (distance <= weakAttackRange)
            {
                // 가까우면 약공격 (빠르고 안전)
                useStrongAttack = false;
                Debug.Log($"SmartAttack: Using Weak Attack (distance: {distance:F1}m)");
            }
            else if (distance <= strongAttackRange)
            {
                // 중간 거리면 강공격 (더 강력)
                useStrongAttack = true;
                Debug.Log($"SmartAttack: Using Strong Attack (distance: {distance:F1}m)");
            }
            else
            {
                // 너무 멀면 실패
                state = NodeState.Failure;
                return state;
            }

            bool attackSuccess = false;
            if (useStrongAttack)
            {
                attackSuccess = controller.Attack(); // 강공격
            }
            else
            {
                attackSuccess = controller.WeakAttack(); // 약공격
            }

            if (attackSuccess)
            {
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
            // 공격이 진행 중인지 확인
            if (controller.IsAttacking)
            {
                state = NodeState.Running;
            }
            else
            {
                // 공격 완료
                attackStarted = false;
                state = NodeState.Success;
            }
        }

        return state;
    }

    public override void Reset()
    {
        attackStarted = false;
        useStrongAttack = false;
        base.Reset();
    }
}