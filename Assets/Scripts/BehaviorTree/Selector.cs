
using UnityEngine;
using System.Collections.Generic;

namespace BehaviorTree
{
    public class SelectorNode : CompositeNode
    {
        public SelectorNode() : base() { }
        public SelectorNode(List<Node> children) : base(children) { }
        public override NodeState Evaluate()
        {

            foreach (Node child in children)
            {
                switch (child.Evaluate())
                {
                    case NodeState.Running:
                        state = NodeState.Running;
                        return state;

                    case NodeState.Success:
                        state = NodeState.Success;
                        return state;

                    case NodeState.Failure:
                        continue;
                }
            }
            Reset();
            state = NodeState.Failure;
            return state;
        }

        public override void Reset()
        {
            base.Reset();
        }
    }

}