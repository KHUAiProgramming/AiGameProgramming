using UnityEngine;
using BehaviorTree;

public class IsOpponentCounterWindowOpen : ConditionNode
{
    public IsOpponentCounterWindowOpen(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        Transform target = blackboard.GetValue<Transform>("target");

        if (target == null)
        {
            state = NodeState.Failure;
            return state;
        }

        bool counterWindowOpen = false;

        // DefenderController인 경우 반격 윈도우 확인
        DefenderController opponentDefender = target.GetComponent<DefenderController>();
        if (opponentDefender != null)
        {
            counterWindowOpen = opponentDefender.IsCounterWindowOpen;
        }

        // AttackerController는 반격 윈도우가 없으므로 항상 false

        state = counterWindowOpen ? NodeState.Success : NodeState.Failure;

        if (counterWindowOpen)
        {
            Debug.Log("IsOpponentCounterWindowOpen: Opponent counter window is open - dangerous!");
        }

        return state;
    }
}