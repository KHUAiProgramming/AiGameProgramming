using UnityEngine;
using System.Collections.Generic;

namespace BehaviorTree
{
    public class SequenceNode : CompositeNode
    {
        public SequenceNode() : base() { }
        public SequenceNode(List<Node> children) : base(children) { }
        public override NodeState Evaluate()
        {
            foreach (Node child in children)
            {
                switch (child.Evaluate())
                {
                    case NodeState.Running:
                        state = NodeState.Running;
                        return state;

                    case NodeState.Failure:
                        state = NodeState.Failure;
                        return state;

                    case NodeState.Success:
                        continue;
                }
            }

            Reset();
            state = NodeState.Success;
            return state;
        }

        public override void Reset()
        {
            base.Reset();
        }
    }
}

