using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    public class RandomTwoSelector : SelectorNode
    {
        private List<Node> shuffledChildren;

        public RandomTwoSelector() : base() { }

        public RandomTwoSelector(params Node[] children) : base(children) { }

        public override NodeState Evaluate()
        {
            // 자식이 없으면 실패
            if (children == null || children.Count == 0)
            {
                state = NodeState.Failure;
                return state;
            }

            // 첫 실행이거나 모든 자식이 완료되면 순서를 섞음
            if (shuffledChildren == null || shuffledChildren.Count == 0)
            {
                ShuffleChildren();
            }

            // 섞인 순서대로 자식들을 평가
            for (int i = 0; i < shuffledChildren.Count; i++)
            {
                Node child = shuffledChildren[i];

                switch (child.Evaluate())
                {
                    case NodeState.Running:
                        state = NodeState.Running;
                        return state;
                    case NodeState.Success:
                        // 성공한 자식을 제거하고 Success 반환
                        shuffledChildren.RemoveAt(i);
                        state = NodeState.Success;
                        return state;
                    case NodeState.Failure:
                        // 실패하면 다음 자식으로 계속
                        continue;
                }
            }

            // 모든 자식이 실패하면 순서를 다시 섞고 Failure 반환
            shuffledChildren = null;
            state = NodeState.Failure;
            return state;
        }

        private void ShuffleChildren()
        {
            shuffledChildren = new List<Node>(children);

            // Fisher-Yates 셔플 알고리즘
            for (int i = shuffledChildren.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                Node temp = shuffledChildren[i];
                shuffledChildren[i] = shuffledChildren[j];
                shuffledChildren[j] = temp;
            }
        }

        public override void Reset()
        {
            shuffledChildren = null;
            base.Reset();
        }
    }

    // 확률 기반으로 두 개의 행동 중 하나를 선택하는 Selector
    public class ProbabilitySelector : Node
    {
        private float probability; // 첫 번째 자식이 선택될 확률 (0.0 ~ 1.0)
        private Node firstChild;
        private Node secondChild;

        public ProbabilitySelector(float probability, Node firstChild, Node secondChild)
        {
            this.probability = Mathf.Clamp01(probability); // 0~1 사이로 제한
            this.firstChild = firstChild;
            this.secondChild = secondChild;
        }

        public override NodeState Evaluate()
        {
            // 확률에 따라 자식 선택
            Node selectedChild = (Random.Range(0f, 1f) <= probability) ? firstChild : secondChild;

            // 선택된 자식 평가
            state = selectedChild.Evaluate();
            return state;
        }

        public override void Reset()
        {
            firstChild?.Reset();
            secondChild?.Reset();
            base.Reset();
        }
    }
}