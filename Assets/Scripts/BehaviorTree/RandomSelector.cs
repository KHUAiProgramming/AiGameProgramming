using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    // 자식 노드들을 랜덤 순서로 실행하는 Selector
    public class RandomSelector : SelectorNode
    {
        private List<Node> shuffledChildren;

        public RandomSelector() : base() { }

        public RandomSelector(params Node[] children) : base(children) { }

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
}