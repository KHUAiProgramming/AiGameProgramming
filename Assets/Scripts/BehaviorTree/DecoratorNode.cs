
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.UI;


namespace BehaviorTree
{
    public class RepeaterNode : DecoratorNode
    {
        private int repeatCount;
        private int currentCount = 0;

        public RepeaterNode(int repeatCount = -1) // -1은 무한 반복
        {
            this.repeatCount = repeatCount;
        }

        public override NodeState Evaluate()
        {
            if (child == null)
            {
                state = NodeState.Failure;
                return state;
            }

            NodeState childState = child.Evaluate();

            if (childState == NodeState.Running)
            {
                state = NodeState.Running;
                return state;
            }

            currentCount++;

            if (repeatCount > 0 && currentCount >= repeatCount) // 반복 완료
            {
                currentCount = 0;
                state = childState; // 마지막 결과
                return state;
            }

            // 자식 리셋하고 계속 반복
            child.Reset();
            state = NodeState.Running;
            return state;
        }

        public override void Reset()
        {
            base.Reset();
            currentCount = 0;
        }
    }

    public class InverterNode : DecoratorNode
    {
        public override NodeState Evaluate()
        {
            if (child == null)
            {
                state = NodeState.Failure;
                return state;
            }

            NodeState childState = child.Evaluate();

            switch (childState)
            {
                case NodeState.Success:
                    state = NodeState.Failure;
                    break;
                case NodeState.Failure:
                    state = NodeState.Success;
                    break;
                case NodeState.Running:
                    state = NodeState.Running;
                    break;
            }

            return state;
        }
    }


    public class RetryNode : DecoratorNode
    {
        private int maxRetries;
        private int currentRetries = 0;

        public RetryNode(int maxRetries)
        {
            this.maxRetries = maxRetries;
        }

        public override NodeState Evaluate()
        {
            if (child == null)
            {
                state = NodeState.Failure;
                return state;
            }

            NodeState childState = child.Evaluate();

            switch (childState)
            {
                case NodeState.Success:
                    currentRetries = 0;
                    state = NodeState.Success;
                    return state;

                case NodeState.Running:
                    state = NodeState.Running;
                    return state;

                case NodeState.Failure:
                    currentRetries++;

                    if (currentRetries < maxRetries)
                    {
                        child.Reset(); // 재시도를 위해 리셋
                        state = NodeState.Running;
                        return state;
                    }
                    else
                    {
                        currentRetries = 0;
                        state = NodeState.Failure;
                        return state;
                    }
            }

            state = NodeState.Failure;
            return state;
        }

        public override void Reset()
        {
            base.Reset();
            currentRetries = 0;
        }
    }

    public class ForceSuccessNode : DecoratorNode
    {
        public override NodeState Evaluate()
        {
            if (child == null)
            {
                state = NodeState.Success;
                return state;
            }

            NodeState childState = child.Evaluate();

            if (childState == NodeState.Running)
            {
                state = NodeState.Running;
            }
            else
            {
                state = NodeState.Success;
            }

            return state;
        }
    }


    public class ForceFailureNode : DecoratorNode
    {
        public override NodeState Evaluate()
        {
            if (child == null)
            {
                state = NodeState.Failure;
                return state;
            }

            NodeState childState = child.Evaluate();

            if (childState == NodeState.Running)
            {
                state = NodeState.Running;
            }
            else
            {
                state = NodeState.Failure;
            }

            return state;
        }
    }


    public class TimeoutNode : DecoratorNode
    {
        private float timeout;
        private float startTime;
        private bool isRunning = false;

        public TimeoutNode(float timeout)
        {
            this.timeout = timeout;
        }

        public override NodeState Evaluate()
        {
            if (child == null)
            {
                state = NodeState.Failure;
                return state;
            }

            if (!isRunning)
            {
                startTime = Time.time;
                isRunning = true;
            }

            // time over
            if (Time.time - startTime >= timeout)
            {
                child.Reset();
                isRunning = false;
                state = NodeState.Failure;
                return state;
            }

            NodeState childState = child.Evaluate();

            if (childState != NodeState.Running)
            {
                isRunning = false;
            }

            state = childState;
            return state;
        }

        public override void Reset()
        {
            base.Reset();
            isRunning = false;
        }
    }

    public class DelayNode : DecoratorNode
    {
        private float delay;
        private float startTime;
        private bool complete = false;

        public DelayNode(float delay)
        {
            this.delay = delay;
        }

        public override NodeState Evaluate()
        {
            if (child == null)
            {
                state = NodeState.Failure;
                return state;
            }

            if (!complete)
            {
                if (startTime == 0)
                {
                    startTime = Time.time;
                }

                if (Time.time - startTime >= delay)
                {
                    complete = true;
                }
                else
                {
                    state = NodeState.Running;
                    return state;
                }
            }

            // 딜레이 끝
            NodeState childState = child.Evaluate();
            state = childState;
            return state;
        }

        public override void Reset()
        {
            base.Reset();
            startTime = 0;
            complete = false;
        }
    }

    public class UntilSuccessNode : DecoratorNode
    {
        public override NodeState Evaluate()
        {
            if (child == null)
            {
                state = NodeState.Failure;
                return state;
            }

            NodeState childState = child.Evaluate();

            switch (childState)
            {
                case NodeState.Success:
                    state = NodeState.Success;
                    return state;

                case NodeState.Running:
                    state = NodeState.Running;
                    return state;

                case NodeState.Failure:
                    child.Reset();
                    state = NodeState.Running;
                    return state;
            }

            state = NodeState.Running;
            return state;
        }
    }

    public class UntilFailureNode : DecoratorNode
    {
        public override NodeState Evaluate()
        {
            if (child == null)
            {
                state = NodeState.Success;
                return state;
            }

            NodeState childState = child.Evaluate();

            switch (childState)
            {
                case NodeState.Failure:
                    state = NodeState.Success;
                    return state;

                case NodeState.Running:
                    state = NodeState.Running;
                    return state;

                case NodeState.Success:
                    child.Reset();
                    state = NodeState.Running;
                    return state;
            }

            state = NodeState.Running;
            return state;
        }
    }
}