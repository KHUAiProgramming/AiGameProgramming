using System.Collections.Generic;

namespace BehaviorTree
{
    public class RandomSelectorNode : CompositeNode
    {
        private System.Random random = new System.Random();

        public RandomSelectorNode(params Node[] children) : base(children) { }

        public override NodeState Evaluate()
        {
            if (children.Count == 0)
            {
                state = NodeState.Failure;
                return state;
            }

            int index = random.Next(children.Count);
            NodeState result = children[index].Evaluate();
            state = result;
            return state;
        }
    }
}