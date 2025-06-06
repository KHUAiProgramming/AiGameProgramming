using UnityEngine;
using BehaviorTree;

public class ExecuteQuickAttack : ActionNode
{
    private bool attackStarted = false;
    private float attackProbability;
    private ActionNode fallbackAction;
    
    public ExecuteQuickAttack(MonoBehaviour owner, Blackboard blackboard, float probability = 0.75f, ActionNode fallback = null) 
        : base(owner, blackboard) 
    {
        attackProbability = probability;
        fallbackAction = fallback;
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
            // 확률 계산으로 대체 액션 실행
            if (Random.Range(0f, 1f) > attackProbability && fallbackAction != null)
            {
                Debug.Log($"ExecuteQuickAttack: Using fallback action (probability: {attackProbability:P})");
                return fallbackAction.Evaluate();
            }
            
            if (!controller.CanAttack())
            {
                state = NodeState.Failure;
                return state;
            }
            
            bool success = controller.WeakAttack(); // Quick attack (using weak attack)
            if (success)
            {
                attackStarted = true;
                state = NodeState.Running;
                Debug.Log("ExecuteQuickAttack: Started quick attack");
            }
            else
            {
                state = NodeState.Failure;
            }
        }
        else
        {
            // 공격 진행 중 확인
            if (controller.IsAttacking)
            {
                state = NodeState.Running;
            }
            else
            {
                attackStarted = false;
                state = NodeState.Success;
                Debug.Log("ExecuteQuickAttack: Quick attack completed successfully");
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