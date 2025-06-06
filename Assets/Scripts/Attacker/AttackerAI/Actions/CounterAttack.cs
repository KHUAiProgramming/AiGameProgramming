using UnityEngine;
using BehaviorTree;

public class CounterAttack : ActionNode
{
    private bool attackStarted = false;

    public CounterAttack(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard)
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

        // 상대방의 DefenderController 또는 AttackerController 확인
        var opponentDefender = target.GetComponent<DefenderController>();
        var opponentAttacker = target.GetComponent<AttackerController>();

        bool canBeCountered = false;
        if (opponentDefender != null)
        {
            canBeCountered = opponentDefender.CanBeCountered;
        }
        else if (opponentAttacker != null)
        {
            canBeCountered = opponentAttacker.CanBeCountered;
        }

        if (!attackStarted)
        {
            // 반격 조건 확인: 상대방이 반격 가능 상태이고, 내가 공격 가능하고, 사거리 내에 있어야 함
            if (!canBeCountered || !controller.CanAttack())
            {
                state = NodeState.Failure;
                return state;
            }

            float distance = Vector3.Distance(controller.transform.position, target.position);
            if (distance > 5.0f) // 강공격 사거리
            {
                state = NodeState.Failure;
                return state;
            }

            // 반격은 항상 강공격으로
            bool attackSuccess = controller.Attack();

            if (attackSuccess)
            {
                attackStarted = true;
                state = NodeState.Running;
                Debug.Log("CounterAttack: Successfully initiated counter attack!");
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
                // 반격 완료
                attackStarted = false;
                state = NodeState.Success;
                Debug.Log("CounterAttack: Counter attack completed!");
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