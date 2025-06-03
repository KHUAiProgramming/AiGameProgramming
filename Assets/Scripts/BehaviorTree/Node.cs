
using UnityEngine;
using System.Collections.Generic;


namespace BehaviorTree
{
    public enum NodeState
    {
        Running,    // 실행 중
        Success,    // 성공
        Failure     // 실패
    }

    public abstract class Node
    {
        protected NodeState state;
        public NodeState State => state;

        public abstract NodeState Evaluate();

        public virtual void Reset()
        {
            state = NodeState.Running;
        }
    }

    public abstract class CompositeNode : Node
    {
        protected List<Node> children = new List<Node>();

        public CompositeNode()
        {

        }
        public CompositeNode(List<Node> children)
        {
            this.children = children;
        }


        public void AddChild(Node child)
        {
            children.Add(child);
        }

        public void RemoveChild(Node child)
        {
            children.Remove(child);
        }

        public override void Reset()
        {
            foreach (Node child in children)
            {
                child.Reset();
            }
            base.Reset();
        }
    }

    public abstract class LeafNode : Node
    {
        protected MonoBehaviour owner;
        protected Blackboard blackboard;

        public LeafNode(MonoBehaviour owner, Blackboard blackboard)
        {
            this.owner = owner;
            this.blackboard = blackboard;
        }
    }

    public abstract class ActionNode : LeafNode
    {

        public ActionNode(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard)
        {
        }
    }

    public abstract class ConditionNode : LeafNode
    {
        public ConditionNode(MonoBehaviour owner, Blackboard blackboard) : base(owner, blackboard)
        {
        }
    }

    public abstract class DecoratorNode : Node
    {
        protected Node child;

        public void SetChild(Node child)
        {
            this.child = child;
        }

        public override void Reset()
        {
            child?.Reset();
            base.Reset();
        }
    }
}