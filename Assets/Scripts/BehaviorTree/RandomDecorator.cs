using UnityEngine;

namespace BehaviorTree
{
    // 자식 노드를 확률에 따라 실행하는 데코레이터
    public class RandomDecorator : Node
    {
        private float probability;
        private Node child;

        public RandomDecorator(float probability, Node child)
        {
            this.probability = Mathf.Clamp01(probability);
            this.child = child;
        }

        public override NodeState Evaluate()
        {
            // 확률 체크
            if (Random.value <= probability)
            {
                state = child.Evaluate();
                return state;
            }
            else
            {
                state = NodeState.Failure;
                return state;
            }
        }

        public override void Reset()
        {
            child?.Reset();
            base.Reset();
        }
    }
}