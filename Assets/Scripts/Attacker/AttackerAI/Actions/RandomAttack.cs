using UnityEngine;
using BehaviorTree;

public class RandomAttack : ActionNode
{
    private bool attackStarted = false;
    private bool useStrongAttack = false;

    [SerializeField] private float strongAttackProbability = 0.75f; // 75% 확률로 강공격

    public RandomAttack(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard)
    {
    }

    public RandomAttack(MonoBehaviour owner, Blackboard blackboard, float strongAttackProbability) : base(owner, blackboard)
    {
        this.strongAttackProbability = strongAttackProbability;
    }

    public override NodeState Evaluate()
    {
        AttackerController controller = blackboard.GetValue<AttackerController>("controller");

        if (controller == null)
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

            // 랜덤하게 공격 타입 선택
            useStrongAttack = Random.value < strongAttackProbability;

            bool attackSuccess = false;
            if (useStrongAttack)
            {
                attackSuccess = controller.Attack(); // 강공격
                Debug.Log("RandomAttack: Using Strong Attack");
            }
            else
            {
                attackSuccess = controller.WeakAttack(); // 약공격
                Debug.Log("RandomAttack: Using Weak Attack");
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
                Debug.Log($"RandomAttack: {(useStrongAttack ? "Strong" : "Weak")} attack completed");
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