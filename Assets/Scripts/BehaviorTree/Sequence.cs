using UnityEngine;
using System.Collections.Generic;

namespace BehaviorTree
{
    public class SequenceNode : CompositeNode
    {
        private int currentChildIndex = 0;

        public override NodeState Evaluate()
        {
            for (int i = currentChildIndex; i < children.Count; i++)
            {
                NodeState childState = children[i].Evaluate();

                switch (childState)
                {
                    case NodeState.Running:
                        currentChildIndex = i;
                        state = NodeState.Running;
                        return state;

                    case NodeState.Failure:
                        ResetAllChildren();
                        currentChildIndex = 0;
                        state = NodeState.Failure;
                        return state;

                    case NodeState.Success:
                        continue;
                }
            }

            // 모든 자식 노드 성공한 경우
            ResetAllChildren();
            currentChildIndex = 0;
            state = NodeState.Success;
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

