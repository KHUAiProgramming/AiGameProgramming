using UnityEngine;
using BehaviorTree;
using System.Collections.Generic;

public class WeightedSelector : CompositeNode
{
    private float[] weights;
    private int selectedIndex = -1;

    public WeightedSelector(List<Node> children, float[] weights) : base(children)
    {
        this.weights = weights;

        // 가중치 정규화
        float totalWeight = 0f;
        foreach (float weight in weights)
        {
            totalWeight += weight;
        }

        for (int i = 0; i < weights.Length; i++)
        {
            this.weights[i] = weights[i] / totalWeight;
        }
    }

    public override NodeState Evaluate()
    {
        // 아직 선택하지 않았다면 가중치 기반 선택
        if (selectedIndex == -1)
        {
            selectedIndex = SelectWeightedIndex();
            Debug.Log($"WeightedSelector: Selected child {selectedIndex} ({children[selectedIndex].GetType().Name})");
        }

        // 선택된 노드 실행
        NodeState childState = children[selectedIndex].Evaluate();

        if (childState == NodeState.Success || childState == NodeState.Failure)
        {
            selectedIndex = -1; // 리셋
            Debug.Log($"WeightedSelector: Child completed with {childState}");
        }

        return childState;
    }

    private int SelectWeightedIndex()
    {
        float randomValue = Random.value;
        float cumulativeWeight = 0f;

        for (int i = 0; i < weights.Length && i < children.Count; i++)
        {
            cumulativeWeight += weights[i];
            if (randomValue <= cumulativeWeight)
            {
                return i;
            }
        }

        return children.Count - 1; // 안전장치
    }
}