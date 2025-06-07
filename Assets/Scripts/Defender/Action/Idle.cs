using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class Idle : ActionNode
{
    public Idle(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard) { }

    public override NodeState Evaluate()
    {
        //Debug.Log("Standing idle...");
        state = NodeState.Running; // 계속 Idle 유지
        return state;
    }
}