using UnityEngine;
using BehaviorTree;
using System.Collections.Generic;

public class RandomSelector : CompositeNode
{
    public RandomSelector(List<Node> children) : base(children) { }

    public override NodeState Evaluate()
    {
        // 랜덤하게 자식 노드 순서 섞기
        List<Node> shuffledChildren = new List<Node>(children);
        for (int i = 0; i < shuffledChildren.Count; i++)
        {
            Node temp = shuffledChildren[i];
            int randomIndex = Random.Range(i, shuffledChildren.Count);
            shuffledChildren[i] = shuffledChildren[randomIndex];
            shuffledChildren[randomIndex] = temp;
        }

        // Selector 로직으로 실행
        foreach (Node child in shuffledChildren)
        {
            NodeState childState = child.Evaluate();

            if (childState == NodeState.Success)
            {
                Debug.Log($"RandomSelector: Child succeeded - {child.GetType().Name}");
                return NodeState.Success;
            }
            else if (childState == NodeState.Running)
            {
                Debug.Log($"RandomSelector: Child running - {child.GetType().Name}");
                return NodeState.Running;
            }
            // Failure일 경우 다음 자식으로 계속
        }

        Debug.Log("RandomSelector: All children failed");
        return NodeState.Failure;
    }
}