
using UnityEngine;
using System.Collections.Generic;

namespace BehaviorTree
{
    public class SelectorNode : CompositeNode
    {
        private int currentChildIndex = 0;

        public override NodeState Evaluate()
        {
            for (int i = currentChildIndex; i < children.Count; i++)
            {
                var childState = children[i].Evaluate();

                switch (childState)
                {
                    case NodeState.Running:
                        currentChildIndex = i;
                        state = NodeState.Running;
                        return state;

                    case NodeState.Success:
                        ResetAllChildren();
                        currentChildIndex = 0;
                        state = NodeState.Success;
                        return state;

                    case NodeState.Failure:
                        children[i].Reset();
                        continue; // 다음 자식 노드 시도
                }
            }

            // 모든 자식 노드 실패
            ResetAllChildren();
            currentChildIndex = 0;
            state = NodeState.Failure;
            return state;
        }

        public override void Reset()
        {
            base.Reset();
            currentChildIndex = 0;
        }

        private void ResetAllChildren()
        {
            foreach (Node child in children)
            {
                child.Reset();
            }
        }
    }

}